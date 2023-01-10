namespace SuggestionAppLibrary.DataAccess;

public interface ISuggestionData
{
    Task CrateSuggestion(SuggestionModel suggestion);
    Task<List<SuggestionModel>> GetAllApprovedSuggestionsAsync();
    Task<List<SuggestionModel>> GetAllSuggestionsAsync();
    Task<List<SuggestionModel>> GetAllSuggestionWaitingForApprovalAsync();
    Task<SuggestionModel> GetSuggestionAsync(string id);
    Task<List<SuggestionModel>> GetUserSuggestions(string userId);
    Task UpdateSuggestionAsync(SuggestionModel suggestion);
    Task UpvoteSuggestion(string suggestionId, string userId);
}