using CommunityToolkit.Mvvm.ComponentModel;

namespace NationalClock.ViewModels;

/// <summary>
/// Base ViewModel class that provides INotifyPropertyChanged implementation
/// using CommunityToolkit.Mvvm ObservableObject
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    // ObservableObject from CommunityToolkit.Mvvm provides:
    // - INotifyPropertyChanged implementation
    // - SetProperty method for property change notification
    // - OnPropertyChanged method for manual property change notification
    // - Source generators for [ObservableProperty] attributes
}