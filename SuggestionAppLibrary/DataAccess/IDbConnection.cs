using MongoDB.Driver;

namespace SuggestionAppLibrary.DataAccess;

public interface IDbConnection
{
    IMongoCollection<CategoryModel> CategoriesCollection { get; }
    string CategoryCollectionName { get; }
    MongoClient Client { get; }
    string DbName { get; }
    IMongoCollection<StatusModel> StatusCollection { get; }
    string StatusColllectionName { get; }
    IMongoCollection<SuggestionModel> SuggestionCollection { get; }
    string SuggestionCollectionName { get; }
    IMongoCollection<UserModel> UserCollection { get; }
    string UserCollectionName { get; }
}