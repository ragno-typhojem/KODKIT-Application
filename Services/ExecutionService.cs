using KodKit.Services.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using KodKit.Models;

namespace KodKit.Services
{
    public class ExecutionService : IExecutionService
    {
        private double _currentX;
        private double _currentY;
        private double _currentDirection;
        private bool _isVisible;
        private string _currentCostume;
        private readonly Dictionary<string, object> _variables;
        private double _animationSpeed;
        private bool _isAnimating;

        public ExecutionService()
        {
            _currentX = 0;
            _currentY = 0;
            _currentDirection = 90;
            _isVisible = true;
            _currentCostume = "default";
            _variables = new Dictionary<string, object>();
            _animationSpeed = 1.0;
            _isAnimating = false;
        }
        public async Task ExecuteBlockAsync(Block block)
        {
            ArgumentNullException.ThrowIfNull(block);

            try
            {
                switch (block.Category)
                {
                    case "Motion":
                        await ExecuteMotionBlockAsync(block);
                        break;
                    case "Looks":
                        await ExecuteLooksBlockAsync(block);
                        break;
                    case "Control":
                        await ExecuteControlBlockAsync(block);
                        break;
                    case "Variables":
                        await ExecuteVariableBlockAsync(block);
                        break;
                    default:
                        throw new NotSupportedException($"Block category {block.Category} is not supported.");
                }

                // Execute next block if exists
                if (block.NextBlock != null)
                {
                    await ExecuteBlockAsync(block.NextBlock);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error executing block {block.Type}: {ex.Message}");
                throw;
            }
        }

        private async Task ExecuteMotionBlockAsync(Block block)
        {
            switch (block.Type)
            {
                case "MoveSteps":
                    await ExecuteMoveStepsAsync(double.Parse(block.Parameter));
                    break;
                case "TurnRight":
                    await ExecuteTurnRightAsync(double.Parse(block.Parameter));
                    break;
                case "TurnLeft":
                    await ExecuteTurnLeftAsync(double.Parse(block.Parameter));
                    break;
                case "GoToPosition":
                    var coords = block.Parameter.Split(',');
                    await ExecuteGoToPositionAsync(
                        double.Parse(coords[0]),
                        double.Parse(coords[1]));
                    break;
                case "SetXPosition":
                    await ExecuteSetXPositionAsync(double.Parse(block.Parameter));
                    break;
                case "SetYPosition":
                    await ExecuteSetYPositionAsync(double.Parse(block.Parameter));
                    break;
                default:
                    throw new NotSupportedException($"Motion block type {block.Type} is not supported.");
            }
        }

        private async Task ExecuteLooksBlockAsync(Block block)
        {
            switch (block.Type)
            {
                case "ShowSprite":
                    await ExecuteShowSpriteAsync();
                    break;
                case "HideSprite":
                    await ExecuteHideSpriteAsync();
                    break;
                case "SayMessage":
                    await ExecuteSayMessageAsync(block.Parameter);
                    break;
                case "ChangeCostume":
                    await ExecuteChangeCostumeAsync(block.Parameter);
                    break;
                default:
                    throw new NotSupportedException($"Looks block type {block.Type} is not supported.");
            }
        }

        private async Task ExecuteControlBlockAsync(Block block)
        {
            switch (block.Type)
            {
                case "Wait":
                    await Task.Delay((int)(double.Parse(block.Parameter) * 1000));
                    break;
                case "Repeat":
                    var times = int.Parse(block.Parameter);
                    for (int i = 0; i < times; i++)
                    {
                        foreach (var innerBlock in block.InnerBlocks)
                        {
                            await ExecuteBlockAsync(innerBlock);
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"Control block type {block.Type} is not supported.");
            }
        }

        private async Task ExecuteVariableBlockAsync(Block block)
        {
            switch (block.Type)
            {
                case "SetVariable":
                    var parts = block.Parameter.Split('=');
                    if (parts.Length == 2)
                    {
                        await SetVariableAsync(parts[0].Trim(), parts[1].Trim());
                    }
                    break;
                default:
                    throw new NotSupportedException($"Variable block type {block.Type} is not supported.");
            }
        }
        public async Task ExecuteMoveStepsAsync(double steps)
        {
            double angleRadians = _currentDirection * Math.PI / 180;
            _currentX += steps * Math.Cos(angleRadians);
            _currentY += steps * Math.Sin(angleRadians);
            await AnimateMovementAsync();
        }

        public async Task ExecuteTurnRightAsync(double angle)
        {
            _currentDirection = (_currentDirection + angle) % 360;
            await AnimateRotationAsync();
        }

        public async Task ExecuteTurnLeftAsync(double angle)
        {
            _currentDirection = (_currentDirection - angle) % 360;
            if (_currentDirection < 0) _currentDirection += 360;
            await AnimateRotationAsync();
        }

        public async Task ExecuteGoToPositionAsync(double x, double y)
        {
            _currentX = x;
            _currentY = y;
            await AnimateMovementAsync();
        }

        public async Task ExecuteSetXPositionAsync(double x)
        {
            _currentX = x;
            await AnimateMovementAsync();
        }

        public async Task ExecuteSetYPositionAsync(double y)
        {
            _currentY = y;
            await AnimateMovementAsync();
        }

        public async Task ExecuteShowSpriteAsync()
        {
            _isVisible = true;
            await Task.Delay(50);
        }

        public async Task ExecuteHideSpriteAsync()
        {
            _isVisible = false;
            await Task.Delay(50);
        }

        public async Task ExecuteSayMessageAsync(string message)
        {
            Debug.WriteLine($"Sprite says: {message}");
            await Task.Delay((int)(1000 * _animationSpeed));
        }

        public async Task ExecuteChangeCostumeAsync(string costumeName)
        {
            _currentCostume = costumeName;
            await Task.Delay((int)(100 * _animationSpeed));
        }

        public (double X, double Y) GetCurrentPosition()
        {
            return (_currentX, _currentY);
        }

        public double GetCurrentDirection()
        {
            return _currentDirection;
        }

        public bool GetVisibility()
        {
            return _isVisible;
        }

        public string GetCurrentCostume()
        {
            return _currentCostume;
        }

        public async Task SetVariableAsync(string name, object value)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            await Task.Run(() => _variables[name] = value);
        }

        public async Task<object> GetVariableAsync(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            return await Task.Run(() =>
                _variables.TryGetValue(name, out var value)
                    ? value
                    : throw new KeyNotFoundException($"Variable not found: {name}"));
        }

        public async Task<bool> ExecuteConditionAsync(string condition)
        {
            ArgumentNullException.ThrowIfNull(condition);

            return await Task.Run(() =>
            {
                try
                {
                    if (condition.Contains("="))
                    {
                        var parts = condition.Split('=');
                        if (_variables.TryGetValue(parts[0].Trim(), out var value))
                        {
                            return value.ToString() == parts[1].Trim();
                        }
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task SetAnimationSpeedAsync(double speed)
        {
            if (speed < 0) throw new ArgumentException("Speed cannot be negative");
            _animationSpeed = speed;
            await Task.CompletedTask;
        }

        public async Task WaitForAnimationAsync()
        {
            while (_isAnimating)
            {
                await Task.Delay(16); // ~60fps
            }
        }

        public void StopAllAnimations()
        {
            _isAnimating = false;
        }

        private async Task AnimateMovementAsync()
        {
            _isAnimating = true;
            await Task.Delay((int)(100 * _animationSpeed));
            _isAnimating = false;
        }

        private async Task AnimateRotationAsync()
        {
            _isAnimating = true;
            await Task.Delay((int)(50 * _animationSpeed));
            _isAnimating = false;
        }
    }
}
