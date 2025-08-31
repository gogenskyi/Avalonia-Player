using System.Collections.Generic;
using System.Dynamic;
using PlayerAvalonia.Models;

namespace PlayerAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public List<MenuListItem> Menu { get; } = new()
        {
            new MenuListItem
            {
                Name = "Home", Page = "MainPage"
            },
            new MenuListItem
            {
                Name = "Search", Page = "MainPage"
            },
            new MenuListItem
            {
                Name = "Library", Page = "MainPage"
            },
            new MenuListItem
            {
                Name = "Albums", Page = "Albums"
            },
            new MenuListItem
            {
                Name = "Artists", Page = "Artists" 
            }
        };

        
    }
}
