using System;
using System.Collections.ObjectModel;
using System.Drawing;
using ReactiveUI;
using WheelAddon.UX.Extensions;

namespace WheelAddon.UX.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ObservableCollection<WheelBindingDisplayViewModel> _displays;
    private ObservableCollection<SliceDisplayViewModel> _slices = null!;

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

    public ObservableCollection<SliceDisplayViewModel> Slices
    {
        get => _slices;
        set => this.RaiseAndSetIfChanged(ref _slices, value);
    }

    public MainViewModel()
    {
        // simple mode binding displays
        _clockWiseBindingDisplay = new BindingDisplayViewModel
        {
            Description = "Clockwise Rotation",
            Store = null!
        };

        _counterClockWiseBindingDisplay = new BindingDisplayViewModel
        {
            Description = "Counter Clockwise Rotation",
            Store = null!
        };

        // wheel binding displays

        _displays = new ObservableCollection<WheelBindingDisplayViewModel>();
        OnSliceAdded = null!;
        OnSliceRemoved = null!;

        var defaultDisplay = new WheelBindingDisplayViewModel
        {
            Description = "Slice 1",
            Store = null!,
            Start = 0,
            End = 20,
            Max = 183
        };

        _displays.Add(defaultDisplay);

        // wheel slices

        _slices = new ObservableCollection<SliceDisplayViewModel>();

        var defaultSlice = new SliceDisplayViewModel(defaultDisplay)
        {
            Color = ColorExtensions.RandomColor().ToHex()
        };

        _slices.Add(defaultSlice);
    }

    public event EventHandler<SliceDisplayViewModel> OnSliceAdded;
    public event EventHandler<int> OnSliceRemoved;

    public void OnSliceAddedEvent()
    {
        // get the last slice
        var lastSlice = _displays[LastIndex];

        // generate a new slice
        var display = new WheelBindingDisplayViewModel
        {
            Description = "Slice " + (_displays.Count + 1),
            Store = null!,
            Start = lastSlice.End,
            End = lastSlice.Max,
            Max = lastSlice.Max
        };

        _displays.Add(display);

        var slice = new SliceDisplayViewModel(display)
        {
            Color = ColorExtensions.RandomColor().ToHex()
        };

        _slices.Add(slice);

        OnSliceAdded?.Invoke(this, slice);

        LastIndex = _displays.Count - 1;
    }

    public void OnSliceRemovedEvent(int index)
    {
        if (index < 1 || index >= _displays.Count)
            return;

        // remove the slice
        _displays.RemoveAt(index);

        _slices.RemoveAt(index);

        OnSliceRemoved?.Invoke(this, index);

        LastIndex = _displays.Count - 1;
    }
}
