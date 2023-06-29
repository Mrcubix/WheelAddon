using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace WheelAddon.UX.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ObservableCollection<WheelBindingDisplayViewModel> _displays;

    private BindingDisplayViewModel _clockWiseBindingDisplay = null!;
    private BindingDisplayViewModel _counterClockWiseBindingDisplay = null!;

    private float _actionValue = 20.0f;
    private int _lastIndex = 0;

    public float ActionValue 
    {
        get => _actionValue;
        set => this.RaiseAndSetIfChanged(ref _actionValue, value);
    }

    public int LastIndex 
    {
        get => _lastIndex;
        set => this.RaiseAndSetIfChanged(ref _lastIndex, value);
    }

    public BindingDisplayViewModel ClockWiseBindingDisplay
    {
        get => _clockWiseBindingDisplay;
        set => this.RaiseAndSetIfChanged(ref _clockWiseBindingDisplay, value);
    }

    public BindingDisplayViewModel CounterClockWiseBindingDisplay
    {
        get => _counterClockWiseBindingDisplay;
        set => this.RaiseAndSetIfChanged(ref _counterClockWiseBindingDisplay, value);
    }

    public ObservableCollection<WheelBindingDisplayViewModel> Displays
    {
        get => _displays;
        set => this.RaiseAndSetIfChanged(ref _displays, value);
    }

    public MainViewModel()
    {
        // simple mode binding displays
        _clockWiseBindingDisplay = new BindingDisplayViewModel();
        _clockWiseBindingDisplay.Description = "Clockwise Rotation";
        _clockWiseBindingDisplay.Store = null!;

        _counterClockWiseBindingDisplay = new BindingDisplayViewModel();
        _counterClockWiseBindingDisplay.Description = "Counter Clockwise Rotation";
        _counterClockWiseBindingDisplay.Store = null!;

        // wheel binding displays

        _displays = new ObservableCollection<WheelBindingDisplayViewModel>();
        OnSliceAdded = null!;
        OnSliceRemoved = null!;

        var defaultDisplay = new WheelBindingDisplayViewModel();
        defaultDisplay.Description = "Slice 1";
        defaultDisplay.Store = null!;
        defaultDisplay.Start = 0;
        defaultDisplay.End = 20;

        _displays.Add(defaultDisplay);
    }


    public event EventHandler<WheelBindingDisplayViewModel> OnSliceAdded;
    public event EventHandler<int> OnSliceRemoved;

    public void OnSliceAddedEvent()
    {
        var display = new WheelBindingDisplayViewModel();
        display.Description = "Slice " + (_displays.Count + 1);
        display.Store = null!;

        _displays.Add(display);

        OnSliceAdded?.Invoke(this, display);

        LastIndex = _displays.Count - 1;
    }

    public void OnSliceRemovedEvent(int index)
    {
        if (index < 1 || index >= _displays.Count)
            return;

        _displays.RemoveAt(index);

        OnSliceRemoved?.Invoke(this, index);

        LastIndex = _displays.Count - 1;
    }
}
