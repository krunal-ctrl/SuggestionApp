using Microsoft.AspNetCore.Components;

namespace SuggestionAppUI.Pages;

public partial class Details
{
    [Parameter]
    public string Id { get; set; }
    private SuggestionModel suggestion;
    protected async override Task OnInitializedAsync()
    {
        suggestion = await suggestionData.GetSuggestionAsync(Id);
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }

    private string GetUpvoteTopText()
    {
        if (suggestion.UserVotes?.Count > 0)
        {
            return suggestion.UserVotes.Count.ToString("00");
        }

        return "Click To";
    }

    private string GetUpvoteBottomText()
    {
        if (suggestion.UserVotes?.Count > 0)
        {
            return "Up votes";
        }

        return "Up vote";
    }
}
