using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace day_04.Model
{
    public class Dish
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }
}