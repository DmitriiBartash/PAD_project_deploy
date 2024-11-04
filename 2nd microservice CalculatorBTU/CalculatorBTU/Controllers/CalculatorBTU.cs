using Microsoft.AspNetCore.Mvc;

namespace BTUService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BTUCalculatorController : ControllerBase
    {
        [HttpPost]
        public IActionResult CalculateBTU([FromBody] BTURequestModel request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            // 1. Преобразование площади комнаты в квадратные метры, если она указана в квадратных футах.
            double roomSizeInMeters = request.SizeUnit.ToLower() switch
            {
                "square feet" => request.RoomSize * 0.092903,
                "square meters" => request.RoomSize,
                _ => request.RoomSize
            };

            // 2. Преобразование высоты потолка в метры, если она указана в футах.
            double ceilingHeightInMeters = request.HeightUnit.ToLower() switch
            {
                "feet" => request.CeilingHeight * 0.3048,
                "meters" => request.CeilingHeight,
                _ => request.CeilingHeight
            };

            // 3. Расчет Q1: теплопритоки от окна, стен, пола и потолка
            double qCoefficient = request.SunExposure.ToLower() switch
            {
                "low" => 30, // слабая освещенность
                "medium" => 35, // средняя освещенность
                "high" => 40, // сильная освещенность
                _ => 35
            };

            double Q1 = roomSizeInMeters * ceilingHeightInMeters * qCoefficient / 1000;

            // 4. Расчет Q2: теплопритоки от людей
            double Q2 = request.PeopleCount > 0 ? request.PeopleCount * 0.1 : 0.0;

            // 5. Расчет Q3: теплопритоки от компьютеров и телевизоров
            double Q3 = 0;
            if (request.NumberOfComputers > 0)
            {
                Q3 += request.NumberOfComputers * 0.3; // Учет 0.3 кВт на каждый компьютер
            }
            if (request.NumberOfTVs > 0)
            {
                Q3 += request.NumberOfTVs * 0.2; // Учет 0.2 кВт на каждый телевизор
            }

            // 6. Учет бытовых приборов
            if (request.OtherAppliancesKWattage > 0)
            {
                // Учет бытовых приборов: холодильник выделяет 30% от максимальной потребляемой мощности
                double otherAppliancesHeatContribution = (request.OtherAppliancesKWattage * 0.30); // 30%
                Q3 += otherAppliancesHeatContribution;
            }

            // 7. Общий расчет мощности до учета дополнительных факторов
            double Q = Q1 + Q2 + Q3;

            // 8. Учет вентиляции с кратностью воздухообмена
            if (request.HasVentilation && request.AirExchangeRate >= 0.5 && request.AirExchangeRate <= 3.0)
            {
                double increasePercentage = request.AirExchangeRate switch
                {
                    0.5 => 0.11,
                    1.0 => 0.22,
                    1.5 => 0.33,
                    2.0 => 0.44,
                    2.5 => 0.55,
                    3.0 => 0.66,
                    _ => 0.0
                };

                Q1 *= 1 + increasePercentage;
                Q = Q1 + Q2 + Q3;
            }

            // 9. Учет гарантированных 20 градусов
            if (request.Guaranteed20Degrees)
            {
                Q *= 1.15;
            }

            // 10. Корректировка на верхний этаж
            if (request.IsTopFloor)
            {
                Q *= 1.15;
            }

            // 11. Учет большой площади остекления
            if (request.HasLargeWindow && request.WindowArea > 2.0)
            {
                double additionalWindowLoad = request.SunExposure.ToLower() switch
                {
                    "low" => (request.WindowArea - 2.0) * 0.05,
                    "medium" => (request.WindowArea - 2.0) * 0.1,
                    "high" => (request.WindowArea - 2.0) * 0.2,
                    _ => 0
                };
                Q += additionalWindowLoad;
            }

            // 12. Рекомендуемый диапазон мощности
            double lowerLimit = Q * 0.95; // -5%
            double upperLimit = Q * 1.15; // +15%

            // 13. Перевод в BTU (1 кВт ≈ 3412 BTU)
            double Q_BTU = Q * 3412;
            double lowerLimit_BTU = lowerLimit * 3412;
            double upperLimit_BTU = upperLimit * 3412;
            //Console.WriteLine($"Calculated BTU:{Q_BTU}");

            // 14. Округление значений
            //Console.WriteLine($"Calculated Q (before rounding): {Q}");
            double calculatedPowerKW = Math.Round(Q, 2); // Две цифры после запятой
            //Console.WriteLine($"Calculated Power (kW) after rounding: {calculatedPowerKW}");
            double calculatedPowerBTU = Math.Round(Q_BTU / 1000) * 1000; // Округляем до ближайшего целого числа, кратного 1000
            //Console.WriteLine($"Calculated Power (BTU) after rounding to nearest 1000: {calculatedPowerBTU}");
            double recommendedLowerKW = Math.Round(lowerLimit, 2); // Две цифры после запятой
            //Console.WriteLine($"Recommended Lower Power (kW) after rounding: {recommendedLowerKW}");
            double recommendedUpperKW = Math.Round(upperLimit, 2); // Две цифры после запятой
            //Console.WriteLine($"Recommended Upper Power (kW) after rounding: {recommendedUpperKW}");

            // Округление до ближайшего числа кратного 1000
            double recommendedLowerBTU = Math.Round(lowerLimit_BTU / 1000) * 1000; // Округляем до ближайшего числа кратного 1000
            //Console.WriteLine($"Recommended Lower Power (BTU) after rounding to nearest 1000: {recommendedLowerBTU}");
            double recommendedUpperBTU = Math.Round(upperLimit_BTU / 1000) * 1000; // Округляем до ближайшего числа кратного 1000
            //Console.WriteLine($"Recommended Upper Power (BTU) after rounding to nearest 1000: {recommendedUpperBTU}");

            // 15. Формируем результат
            return Ok(new
            {
                CalculatedPowerKW = calculatedPowerKW,
                CalculatedPowerBTU = calculatedPowerBTU,
                RecommendedRangeKW = new { Lower = recommendedLowerKW, Upper = recommendedUpperKW },
                RecommendedRangeBTU = new { Lower = recommendedLowerBTU, Upper = recommendedUpperBTU }
            });
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Ok();
        }

    }

    public class BTURequestModel
    {
        public double RoomSize { get; set; } // Площадь помещения
        public string SizeUnit { get; set; } // Единица измерения площади ("square meters" или "square feet")
        public double CeilingHeight { get; set; } // Высота потолка
        public string HeightUnit { get; set; } // Единица измерения высоты ("meters" или "feet")
        public string SunExposure { get; set; } // Освещенность: "low", "medium", "high"
        public int PeopleCount { get; set; } // Количество людей
        public int NumberOfComputers { get; set; } // Количество компьютеров
        public int NumberOfTVs { get; set; } // Количество телевизоров
        public double OtherAppliancesKWattage { get; set; } // Мощность других приборов в кВт
        public bool HasVentilation { get; set; } // Наличие вентиляции (приоткрытое окно)
        public double AirExchangeRate { get; set; } // Кратность воздухообмена (от 0.5 до 3.0)
        public bool Guaranteed20Degrees { get; set; } // Гарантированные 20 градусов (true/false)
        public bool IsTopFloor { get; set; } // Верхний этаж
        public bool HasLargeWindow { get; set; } // Наличие большого окна
        public double WindowArea { get; set; } // Площадь окна в м²
    }
}
