using MongoDB.Driver;
using System.Threading.Tasks;
using Manager_App.Models;

namespace Manager_App.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<ManagerAccount> _usersCollection;

        public MongoDbService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("UserDatabase");
            _usersCollection = database.GetCollection<ManagerAccount>("Users");
        }

        public async Task AddUserAsync(ManagerAccount managerAccount)
        {
            await _usersCollection.InsertOneAsync(managerAccount);
        }

        public async Task<ManagerAccount> GetUserAsync(string username)
        {
            var filter = Builders<ManagerAccount>.Filter.Eq(u => u.Username, username);
            return await _usersCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            var filter = Builders<ManagerAccount>.Filter.Eq(u => u.Username, username);
            return await _usersCollection.Find(filter).FirstOrDefaultAsync() != null;
        }
    }
}