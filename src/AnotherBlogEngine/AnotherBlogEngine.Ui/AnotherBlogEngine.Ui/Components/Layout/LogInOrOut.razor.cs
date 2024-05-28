using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace AnotherBlogEngine.Ui.Components.Layout
{
    public partial class LogInOrOut : IDisposable
    {
        [Inject]
        protected NavigationManager NavigationManager { get; set; }
        
        private string? currentUrl;

        protected override void OnInitialized()
        {
            currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            NavigationManager.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            currentUrl = NavigationManager.ToBaseRelativePath(e.Location);
            StateHasChanged();
        }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
    }
}
