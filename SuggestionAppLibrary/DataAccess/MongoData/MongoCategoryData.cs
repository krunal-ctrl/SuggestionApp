using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace SuggestionAppLibrary.DataAccess.MongoData;

public class MongoCategoryData : ICategoryData
{
    private readonly IMongoCollection<CategoryModel> _categories;
    private readonly IMemoryCache _cache;
    private const string cacheName = "CategoryData";
    public MongoCategoryData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _categories = db.CategoriesCollection;
    }

    public async Task<List<CategoryModel>> GetAllCategoriesAsync()
    {
        var output = _cache.Get<List<CategoryModel>>(cacheName);
        if (output is null)
        {
            var result = await _categories.FindAsync(_ => true);
            output = result.ToList();

            _cache.Set(cacheName, output, TimeSpan.FromDays(1));
        }
        return output.ToList();
    }

    public Task CreateCategory(CategoryModel category)
    {
        return _categories.InsertOneAsync(category);
    }
}
