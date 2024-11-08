﻿using Microsoft.AspNetCore.Mvc;
using Manager_App.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Manager_App.Services;
using Newtonsoft.Json.Linq;

namespace Manager_App.Controllers
{
    public class CalculateBTUController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ConditionerService _conditionerService; 

        public CalculateBTUController(IHttpClientFactory httpClientFactory, ConditionerService conditionerService)
        {
            _httpClientFactory = httpClientFactory;
            _conditionerService = conditionerService; 
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Создаем начальный ViewModel для отображения формы
            var viewModel = new BTUCalculatorViewModel
            {
                RequestModel = new BTURequestModel(),
                // ConditionerModels = new List<ConditionerModel>()
            };
            return View("BTUCalculator", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CalculateBtuValue([FromBody] BTURequestModel requestModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Возвращаем ошибки валидации
            }

            var client = _httpClientFactory.CreateClient();
            var url = "http://calculator:80/api/BTUCalculator"; 

            // Сериализуем модель в JSON
            var jsonContent = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                // Отправляем POST-запрос на микросервис
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    // Десериализуем ответ в модель BTUResponseModel
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseModel = JsonConvert.DeserializeObject<BTUResponseModel>(responseString);

                    var result = new
                    {
                        CalculatedPowerBTU = responseModel.CalculatedPowerBTU,
                        RecommendedRangeBTU = new
                        {
                            Lower = responseModel.RecommendedRangeBTU.Lower,
                            Upper = responseModel.RecommendedRangeBTU.Upper
                        },
                        CalculatedPowerKW = responseModel.CalculatedPowerKW,
                        RecommendedRangeKW = new
                        {
                            Lower = responseModel.RecommendedRangeKW.Lower,
                            Upper = responseModel.RecommendedRangeKW.Upper
                        }
                    };

                    return Json(result); // Возвращаем результат в формате JSON
                }
                else
                {
                    return StatusCode((int)response.StatusCode,
                        "Ошибка при расчете BTU. Пожалуйста, попробуйте снова.");
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500,
                    "Ошибка при отправке запроса. Пожалуйста, проверьте соединение и попробуйте снова.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateConditioners()
        {
            var client = _httpClientFactory.CreateClient();
            var url = "http://webscrapper:5000/api/cond";

            try
            {
                var currentHash = await _conditionerService.GetConditionerHashAsync();

                // Отправляем GET-запрос на парсинг данных
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    // Console.WriteLine("Response from web scraper: " + responseString);

                    // Десериализуем ответ
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);
                    var newHash = data["hash"].ToString();
                    var items = data["items"] as JArray; // Получаем массив
                    var conditioners = items.ToObject<List<ConditionerModel>>(); // Преобразуем в список моделей

                    if (currentHash == newHash)
                    {
                        return Ok(await _conditionerService.GetAllConditionersAsync());
                    }
                    else
                    {
                        // Если хэш-суммы не совпадают, очищаем коллекцию и добавляем новые кондиционеры
                        await _conditionerService.ClearAllConditionersAsync(); 
                        foreach (var conditioner in conditioners)
                        {
                            conditioner.Hash = newHash; 
                            await _conditionerService.AddConditionerAsync(conditioner);
                        }
                    }

                    return Ok(conditioners);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to update conditioners.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating conditioners: " + ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}