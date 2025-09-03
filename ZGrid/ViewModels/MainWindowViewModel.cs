using CommunityToolkit.Mvvm.ComponentModel;
using ZGrid.Models;

namespace ZGrid.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object? selectedObject;

        public MainWindowViewModel()
        {
            SelectedObject = new MySettings();
        }
    }
}
