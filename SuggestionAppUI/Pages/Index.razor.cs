namespace SuggestionAppUI.Pages;

public partial class Index
{
    private List<SuggestionModel> suggestions;
    private List<CategoryModel> categories;
    private List<StatusModel> status;
    private string selectedCategory = "All";
    private string selectedStatus = "All";
    private string searchText = "";
    bool isSortedByNew = true;
    protected async override Task OnInitializedAsync()
    {
        categories = await categoryData.GetAllCategoriesAsync();
        status = await statusData.GetAllStatusAsync();
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            await FilterSuggestions();
            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var stringResult = await sessionStorage.GetAsync<string>(nameof(selectedCategory));
        selectedCategory = stringResult.Success ? stringResult.Value : "All";
        stringResult = await sessionStorage.GetAsync<string>(nameof(selectedStatus));
        selectedStatus = stringResult.Success ? stringResult.Value : "All";
        stringResult = await sessionStorage.GetAsync<string>(nameof(searchText));
        searchText = stringResult.Success ? stringResult.Value : "";
        var boolResult = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
        isSortedByNew = boolResult.Success ? boolResult.Value : true;
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(selectedCategory), selectedCategory);
        await sessionStorage.SetAsync(nameof(selectedStatus), selectedStatus);
        await sessionStorage.SetAsync(nameof(searchText), searchText);
        await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
    }

    private async Task FilterSuggestions()
    {
        var output = await suggestionData.GetAllApprovedSuggestionsAsync();
        if (selectedCategory != "All")
        {
            output = output.Where(s => s.Category?.Name == selectedCategory).ToList();
        }

        if (selectedStatus != "All")
        {
            output = output.Where(s => s.SuggestionStatus?.Name == selectedStatus).ToList();
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            output = output.Where(s => s.Suggestion.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) || s.Description.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(s => s.DateCreated).ToList();
        }
        else
        {
            output = output.OrderByDescending(s => s.UserVotes.Count).ThenByDescending(s => s.DateCreated).ToList();
        }

        suggestions = output;
        await SaveFilterState();
    }

    private async Task OrderByNew(bool isNew)
    {
        isSortedByNew = true;
        await FilterSuggestions();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterSuggestions();
    }

    private async Task OnCategoryClick(string category = "All")
    {
        selectedCategory = category;
        await FilterSuggestions();
    }

    private async Task OnStatusClick(string status = "All")
    {
        selectedStatus = status;
        await FilterSuggestions();
    }

    private string GetUpvoteTopText(SuggestionModel suggestion)
    {
        if (suggestion.UserVotes?.Count > 0)
        {
            return suggestion.UserVotes.Count.ToString("00");
        }

        return "Click To";
    }

    private string GetUpvoteBottomText(SuggestionModel suggestion)
    {
        if (suggestion.UserVotes?.Count > 0)
        {
            return "Up votes";
        }

        return "Up vote";
    }

    private void OpenDetails(SuggestionModel suggestion)
    {
        navManager.NavigateTo($"/Details/{suggestion.Id}");
    }
}
