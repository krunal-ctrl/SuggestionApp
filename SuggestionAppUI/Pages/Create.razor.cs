using SuggestionAppUI.Models;

namespace SuggestionAppUI.Pages;

public partial class Create
{
    private CreateSuggestionModel suggestion = new();
    private List<CategoryModel> categories;
    private UserModel loggedUser;

    protected async override Task OnInitializedAsync()
    {
        categories = await categoryData.GetAllCategoriesAsync();
        //TODO - Replace with user lookup
        loggedUser = await userData.GetUserFromAuthenticationAsync("abc-123");
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }

    private async Task CreateSuggestion()
    {
        SuggestionModel s = new();
        s.Suggestion = suggestion.Suggestion;
        s.Description = suggestion.Description;
        s.Author = new BasicUserModel(loggedUser);
        s.Category = categories.Where(c => c.Id == suggestion.CategoryId).FirstOrDefault();
        if(s.Category is null)
        {
            suggestion.CategoryId = "";
            return;
        }
        await suggestionData.CrateSuggestion(s);
        ClosePage();
    }
}
