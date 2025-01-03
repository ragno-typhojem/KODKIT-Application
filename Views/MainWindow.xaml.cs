using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Storage.Pickers;
using KodKit.ViewModels;
using KodKit.Models;
using System;
using WinRT;
using KodKit.Services;

namespace KodKit.Views
{
    public sealed partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private Point _dragStartPoint;
        private readonly IntPtr _windowHandle;

        public MainViewModel ViewModel => _viewModel;

        public MainWindow()
        {
            this.InitializeComponent();

            // Window handle'ýný al
            _windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);

            _viewModel = new MainViewModel(
                new BlockService(),
                new SoundService(),
                new ExecutionService()
            );

            this.Content = new Grid(); // Root grid
            ((Grid)this.Content).DataContext = _viewModel;

            InitializeCommandBarButtons();
        }

        private void InitializeCommandBarButtons()
        {
            var commandBar = RootGrid.FindName("TopCommandBar") as CommandBar;
            if (commandBar != null)
            {
                foreach (var item in commandBar.PrimaryCommands)
                {
                    if (item is AppBarButton button)
                    {
                        switch (button.Icon.ToString())
                        {
                            case "Play":
                                button.Click += async (s, e) => await _viewModel.ExecuteProjectAsync();
                                break;
                            case "Stop":
                                button.Click += (s, e) => _viewModel.StopExecution();
                                break;
                            case "Save":
                                button.Click += SaveButton_Click;
                                break;
                            case "OpenFile":
                                button.Click += OpenButton_Click;
                                break;
                        }
                    }
                }
            }
        }

        private void Block_DragStarting(UIElement sender, DragStartingEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                string blockType = element.Name switch
                {
                    "MoveStepsBlock" => "MoveSteps",
                    "TurnRightBlock" => "TurnRight",
                    "TurnLeftBlock" => "TurnLeft",
                    "SayMessageBlock" => "SayMessage",
                    "PlaySoundBlock" => "PlaySound",
                    "WaitBlock" => "Wait",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(blockType))
                {
                    e.Data.SetText(blockType);
                    e.AllowedOperations = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                }
            }
        }

        private void WorkArea_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Bloðu Buraya Býrak";
        }

        private async void WorkArea_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
            {
                string blockType = await e.DataView.GetTextAsync();
                Point dropPosition = e.GetPosition(WorkArea);

                var template = new BlockTemplate
                {
                    Type = blockType,
                    DefaultParameter = "0"
                };

                _viewModel.AddBlock(template);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // WinUI 3 için gerekli
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, _windowHandle);

            savePicker.FileTypeChoices.Add("KodKit Projesi", new[] { ".kodkit" });
            savePicker.SuggestedFileName = "YeniProje";

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                await _viewModel.SaveProjectAsync(file);
            }
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // WinUI 3 için gerekli
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, _windowHandle);

            openPicker.FileTypeFilter.Add(".kodkit");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                await _viewModel.LoadProjectAsync(file);
            }
        }
    }
}
