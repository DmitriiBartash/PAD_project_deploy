using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Manager_App.Models;
using MongoDB.Bson;

namespace Manager_App.Services
{
    public class ConditionerService
    {
        private readonly IMongoCollection<ConditionerModel> _conditionersCollection;

        public ConditionerService(IConfiguration configuration)
        {
            var client = new MongoClient("mongodb://root:example@mongo:27017/");
            var database = client.GetDatabase("ConditionerDatabase");
            _conditionersCollection = database.GetCollection<ConditionerModel>("Conditioners");
        }

        // Метод для добавления кондиционера
        public async Task AddConditionerAsync(ConditionerModel conditioner)
        {
            await _conditionersCollection.InsertOneAsync(conditioner);
        }
        // Метод для получения хэш-суммы списка кондиционеров
        public async Task<string> GetConditionerHashAsync()
        {
            var conditioner = await _conditionersCollection.Find(_ => true).FirstOrDefaultAsync();
            return conditioner?.Hash;
        }

        // Метод для получения всех кондиционеров
        public async Task<List<ConditionerModel>> GetAllConditionersAsync()
        {
            return await _conditionersCollection.Find(_ => true).ToListAsync();
        }

        // Метод для получения кондиционеров по диапазону BTU
        public async Task<List<ConditionerModel>> GetConditionersInRangeAsync(string lowerBTU, string upperBTU)
        {
            // Применяем фильтр для поиска кондиционеров в заданном диапазоне BTU
            var filter = Builders<ConditionerModel>.Filter.Gte(c => c.BTU, lowerBTU) &
                         Builders<ConditionerModel>.Filter.Lte(c => c.BTU, upperBTU);
            return await _conditionersCollection.Find(filter).ToListAsync();
        }

        // Метод для получения конкретного кондиционера по ID
        public async Task<ConditionerModel> GetConditionerByIdAsync(string id)
        {
            var filter = Builders<ConditionerModel>.Filter.Eq(c => c.Id, ObjectId.Parse(id));
            return await _conditionersCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task ClearAllConditionersAsync()
        {
            await _conditionersCollection.DeleteManyAsync(FilterDefinition<ConditionerModel>.Empty);
        }

    }
}
