using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using KodKit.Views;
using KodKit.Services;
using KodKit.ViewModels;
using System;
using KodKit.Services.Interfaces;

namespace KodKit
{
    public partial class App : Application
    {
        private static App? _current;
        public static new App Current => _current ??= (App)Application.Current;

        public IServiceProvider Services { get; }
        private Window? _window;

        public App()
        {
            _current = this;
            this.InitializeComponent();

            try
            {
                Services = ConfigureServices();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Service configuration failed: {ex}");
                throw new InvalidOperationException("Failed to configure services", ex);
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Servisleri kaydet
            services.AddSingleton<IExecutionService, ExecutionService>();
            services.AddSingleton<IBlockService, BlockService>();
            services.AddSingleton<ISoundService, SoundService>();
            services.AddSingleton<MainViewModel>();

            // MainWindow'u kaydet
            services.AddSingleton(provider =>
            {
                var viewModel = provider.GetRequiredService<MainViewModel>();
                return new MainWindow(viewModel);
            });

            var serviceProvider = services.BuildServiceProvider();

            // Servislerin doğru yapılandırıldığını kontrol et
            using (var scope = serviceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<MainViewModel>();
                scope.ServiceProvider.GetRequiredService<IExecutionService>();
                scope.ServiceProvider.GetRequiredService<IBlockService>();
                scope.ServiceProvider.GetRequiredService<ISoundService>();
            }

            return serviceProvider;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                _window = Services.GetRequiredService<MainWindow>();
                if (_window == null)
                {
                    throw new InvalidOperationException("Failed to create main window");
                }
                _window.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Application launch failed: {ex}");
                throw new InvalidOperationException("Application failed to launch", ex);
            }
        }
    }
}
