namespace SuggestionAppUI.Pages
{
    public partial class Profile
    {
        private UserModel loggedInUser;
        private List<SuggestionModel> submissions;
        private List<SuggestionModel> approved;
        private List<SuggestionModel> archived;
        private List<SuggestionModel> pending;
        private List<SuggestionModel> rejected;
        protected async override Task OnInitializedAsync()
        {
            //TODO:
            loggedInUser = await userData.GetUserFromAuthenticationAsync("abc-123");
            var results = await suggestionData.GetUserSuggestions(loggedInUser.Id);
            if (loggedInUser is not null && results is not null)
            {
                submissions = results.OrderByDescending(s => s.DateCreated).ToList();
                approved = submissions.Where(s => s.ApprovedForRelease && !s.Archived && !s.Rejected).ToList();
                archived = submissions.Where(s => s.Archived && !s.Rejected).ToList();
                pending = submissions.Where(s => !s.ApprovedForRelease && !s.Rejected).ToList();
                rejected = submissions.Where(s => s.Rejected).ToList();
            }
        }

        private void ClosePage()
        {
            navManager.NavigateTo("/");
        }
    }
}