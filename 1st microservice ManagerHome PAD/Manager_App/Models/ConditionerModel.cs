using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Manager_App.Models
{
    public class ConditionerModel
    {
        [BsonId]
        public ObjectId Id { get; set; } 

        public string Url { get; set; }
        public string Name { get; set; }
        public string Price { get; set; } 
        public string BTU { get; set; } 
        public string ServiceArea { get; set; }
        public string Hash { get; set; }
    }
}