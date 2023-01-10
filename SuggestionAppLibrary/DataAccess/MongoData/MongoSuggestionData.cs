using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace SuggestionAppLibrary.DataAccess.MongoData;

public class MongoSuggestionData : ISuggestionData
{
    private readonly IDbConnection _db;
    private readonly IUserData _userData;
    private readonly IMemoryCache _cache;
    private readonly IMongoCollection<SuggestionModel> _suggestions;
    private const string CacheName = "SuggestionData";
    public MongoSuggestionData(IDbConnection db, IUserData userData, IMemoryCache cache)
    {
        _db = db;
        _userData = userData;
        _cache = cache;
        _suggestions = _db.SuggestionCollection;

    }

    public async Task<List<SuggestionModel>> GetAllSuggestionsAsync()
    {
        var output = _cache.Get<List<SuggestionModel>>(CacheName);
        if (output is null)
        {
            var results = await _suggestions.FindAsync(s => s.Archived == false);
            output = results.ToList();

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<List<SuggestionModel>> GetUserSuggestions(string userId)
    {
        var output = _cache.Get<List<SuggestionModel>>(userId);
        if(output is null)
        {
            var results = await _suggestions.FindAsync(s => s.Author.Id == userId);
            output = results.ToList();

            _cache.Set(userId, output,TimeSpan.FromMinutes(1));
        }
        return output;
    }

    public async Task<List<SuggestionModel>> GetAllApprovedSuggestionsAsync()
    {
        var output = await GetAllSuggestionsAsync();
        return output.Where(x => x.ApprovedForRelease).ToList();
    }

    public async Task<SuggestionModel> GetSuggestionAsync(string id)
    {
        var result = await _suggestions.FindAsync(s => s.Id == id);
        return result.FirstOrDefault();
    }

    public async Task<List<SuggestionModel>> GetAllSuggestionWaitingForApprovalAsync()
    {
        var output = await GetAllSuggestionsAsync();
        return output.Where(x => !x.ApprovedForRelease && !x.Rejected).ToList();
    }

    public async Task UpdateSuggestionAsync(SuggestionModel suggestion)
    {
        await _suggestions.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
        _cache.Remove(CacheName);
    }

    public async Task UpvoteSuggestion(string suggestionId, string userId)
    {
        var client = _db.Client;
        using var session = await client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var suggestionsTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
            var suggestion = (await suggestionsTransaction.FindAsync(s => s.Id == suggestionId)).First();

            bool isUpvote = suggestion.UserVotes.Add(userId);
            if (!isUpvote)
            {
                suggestion.UserVotes.Remove(userId);
            }

            await suggestionsTransaction.ReplaceOneAsync(s => s.Id == suggestionId, suggestion);

            var userInTranscation = db.GetCollection<UserModel>(_db.SuggestionCollectionName);
            var user = await _userData.GetUserByIdAsync(userId);

            if (isUpvote)
            {
                user.VotedOnSuggestion.Add(new BasicSuggestionModel(suggestion));
            }
            else
            {
                var suggestionToRemove = user.VotedOnSuggestion.Where(s => s.Id == suggestionId).First();
                user.VotedOnSuggestion.Remove(suggestionToRemove);
            }

            await userInTranscation.ReplaceOneAsync(u => u.Id == userId, user);
            await session.CommitTransactionAsync();

            _cache.Remove(CacheName);
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task CrateSuggestion(SuggestionModel suggestion)
    {
        var client = _db.Client;
        using var session = await client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var suggestionsTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
            await suggestionsTransaction.InsertOneAsync(suggestion);

            var userInTranscation = db.GetCollection<UserModel>(_db.UserCollectionName);
            var user = await _userData.GetUserByIdAsync(suggestion.Author.Id);

            user.AuthorSuggestions.Add(new BasicSuggestionModel(suggestion));
            await userInTranscation.ReplaceOneAsync(u => u.Id == user.Id, user);

            await session.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
}
