using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Manager_App.Models;
using Manager_App.Services;
using Newtonsoft.Json;

namespace Manager_App.Controllers
{
    public class FindConditionersBTUController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ConditionerService _conditionerService;

       /* public FindConditionersBTUController(IHttpClientFactory httpClientFactory,
            ConditionerService conditionerService)
        {
            _httpClientFactory = httpClientFactory;
            _conditionerService = conditionerService;
        }
*/
        [HttpGet]
        public IActionResult ConditionersHome()
        {
            return View();
        }

        /* [HttpPost]
         public async Task<IActionResult> FindConditioners(int targetBTU)
         {
             int lowerBTU = targetBTU - 1000; // Нижний предел
             int upperBTU = targetBTU + 1000; // Верхний предел
 
             // Получаем текущую хэш-сумму из базы данных
             var currentHash = await _conditionerService.GetConditionerHashAsync();
 
             // Создаем HTTP-клиент
             var client = _httpClientFactory.CreateClient();
             var url = "http://localhost:5000/api/scrape"; // URL Python микросервиса
 
             try
             {
                 // Отправляем запрос на парсинг данных
                 var response = await client.PostAsync(url, null);
 
                 if (response.IsSuccessStatusCode)
                 {
                     // Получаем список кондиционеров из ответа
                     var responseString = await response.Content.ReadAsStringAsync();
                     var conditioners = JsonConvert.DeserializeObject<List<ConditionerModel>>(responseString);
 
                     // Проверка хэш-суммы
                     if (currentHash == conditioners.First().Hash) // Предполагается, что первый кондиционер содержит актуальную хэш-сумму
                     {
                         var conditionersInRange = await _conditionerService.GetConditionersInRangeAsync(lowerBTU, upperBTU);
                         return View("ConditionersHome", conditionersInRange);
                     }
 
                     // Сохраняем кондиционеров в БД
                     foreach (var conditioner in conditioners)
                     {
                         await _conditionerService.AddConditionerAsync(conditioner);
                     }
 
                     var updatedConditionersInRange = await _conditionerService.GetConditionersInRangeAsync(lowerBTU, upperBTU);
                     return View("ConditionersHome", updatedConditionersInRange);
                 }
                 else
                 {
                     Console.WriteLine($"Ошибка при обновлении кондиционеров: {response.StatusCode}");
                     return StatusCode((int)response.StatusCode, "Failed to update conditioners.");
                 }
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Ошибка при обращении к микросервису: {ex.Message}");
                 return StatusCode(500, "Internal server error.");
             }
         }*/
    }
}
