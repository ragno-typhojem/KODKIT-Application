// Tüm dosyaların başına eklenecek ortak using'ler
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KodKit.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KodKit.Services.Interfaces
{
    public interface IExecutionService
    {
        // Hareket Blokları
        Task ExecuteMoveStepsAsync(double steps);
        Task ExecuteTurnRightAsync(double angle);
        Task ExecuteTurnLeftAsync(double angle);
        Task ExecuteGoToPositionAsync(double x, double y);
        Task ExecuteSetXPositionAsync(double x);
        Task ExecuteSetYPositionAsync(double y);

        Task ExecuteBlockAsync(Block block);
        // Görünüm Blokları
        Task ExecuteShowSpriteAsync();
        Task ExecuteHideSpriteAsync();
        Task ExecuteSayMessageAsync(string message);
        Task ExecuteChangeCostumeAsync(string costumeName);

        // Durum Getirme
        (double X, double Y) GetCurrentPosition();
        double GetCurrentDirection();
        bool GetVisibility();
        string GetCurrentCostume();

        // Değişken İşlemleri
        Task SetVariableAsync(string name, object value);
        Task<object> GetVariableAsync(string name);
        Task<bool> ExecuteConditionAsync(string condition);

        // Animasyon Kontrolü
        Task SetAnimationSpeedAsync(double speed);
        Task WaitForAnimationAsync();
        void StopAllAnimations();
    }
}