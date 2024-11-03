namespace Manager_App.Models
{
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

    public class BTUResponseModel
    {
        public double CalculatedPowerKW { get; set; } // Расчетная мощность в кВт
        public double CalculatedPowerBTU { get; set; } // Расчетная мощность в БТЕ
        public RangeModel RecommendedRangeKW { get; set; } // Рекомендуемый диапазон в кВт
        public RangeModel RecommendedRangeBTU { get; set; } // Рекомендуемый диапазон в БТЕ

        public class RangeModel
        {
            public double Lower { get; set; }
            public double Upper { get; set; }
        }
    }
}
