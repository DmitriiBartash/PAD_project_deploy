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
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("ConditionerDatabase");
            _conditionersCollection = database.GetCollection<ConditionerModel>("Conditioners");
        }

        public async Task AddConditionerAsync(ConditionerModel conditioner)
        {
            await _conditionersCollection.InsertOneAsync(conditioner);
        }

        public async Task<List<ConditionerModel>> GetAllConditionersAsync()
        {
            return await _conditionersCollection.Find(_ => true).ToListAsync();
        }

        public async Task<string> GetConditionerHashAsync()
        {
            var conditioner = await _conditionersCollection.Find(_ => true).FirstOrDefaultAsync();
            return conditioner?.Hash; 
        }

        public async Task<List<ConditionerModel>> GetConditionersInRangeAsync(int lowerBTU, int upperBTU)
        {
            var filter = Builders<ConditionerModel>.Filter.Gte(c => c.BTU, lowerBTU) &
                         Builders<ConditionerModel>.Filter.Lte(c => c.BTU, upperBTU);
            return await _conditionersCollection.Find(filter).ToListAsync();
        }

        public async Task<bool> ConditionerExistsAsync(string id)
        {
            var filter = Builders<ConditionerModel>.Filter.Eq(c => c.Id, ObjectId.Parse(id));
            return await _conditionersCollection.Find(filter).FirstOrDefaultAsync() != null;
        }

        public async Task<ConditionerModel> GetConditionerAsync(string id)
        {
            var filter = Builders<ConditionerModel>.Filter.Eq(c => c.Id, ObjectId.Parse(id));
            return await _conditionersCollection.Find(filter).FirstOrDefaultAsync();
        }

    }
}