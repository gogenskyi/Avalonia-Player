using CommunityToolkit.Mvvm.Input;

namespace PlayerAvalonia.Models
{
    public class MenuListItem
    {
        public string Name { get; set; }
        public IRelayCommand Command { get; }

        public MenuListItem(string title, IRelayCommand command)
        {
            Name = title;
            Command = command;
        }
        //public string Icon { get; set; }

    }
}
