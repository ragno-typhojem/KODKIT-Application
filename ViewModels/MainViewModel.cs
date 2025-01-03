using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KodKit.Models;
using KodKit.Services.Interfaces;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Windows.Storage;
using Windows.Storage.Pickers;
using KodKit.Services;

namespace KodKit.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private BlockCategory _selectedCategory;
        public BlockCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    OnSelectedCategoryChanged(value);
                }
            }
        }

        private Block? _selectedBlock;
        public Block? SelectedBlock
        {
            get => _selectedBlock;
            set => SetProperty(ref _selectedBlock, value);
        }

        private Block? _draggedBlock;
        public Block? DraggedBlock
        {
            get => _draggedBlock;
            set => SetProperty(ref _draggedBlock, value);
        }

        private bool _isExecuting;
        public bool IsExecuting
        {
            get => _isExecuting;
            set => SetProperty(ref _isExecuting, value);
        }

        private string _projectName = "Untitled Project";
        public string ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value);
        }

        private bool _isProjectModified;
        public bool IsProjectModified
        {
            get => _isProjectModified;
            set => SetProperty(ref _isProjectModified, value);
        }

        private double _zoomLevel = 1.0;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        private readonly ObservableCollection<Block> _blocks = new();
        private readonly ObservableCollection<string> _executionLog = new();
        private readonly ObservableCollection<BlockTemplate> _blockTemplates = new();
        private readonly ObservableCollection<string> _availableInstruments = new();

        public ObservableCollection<Block> Blocks => _blocks;
        public ObservableCollection<string> ExecutionLog => _executionLog;
        public ObservableCollection<BlockTemplate> BlockTemplates => _blockTemplates;
        public ObservableCollection<string> AvailableInstruments => _availableInstruments;

        private readonly IBlockService _blockService;
        private readonly ISoundService _soundService;
        private readonly IExecutionService _executionService;

        public MainViewModel(
            IBlockService blockService,
            ISoundService soundService,
            IExecutionService executionService)
        {
            _blockService = blockService ?? throw new ArgumentNullException(nameof(blockService));
            _soundService = soundService ?? throw new ArgumentNullException(nameof(soundService));
            _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));

            InitializeViewModel();
        }

        public MainViewModel(BlockService blockService, SoundService soundService, ExecutionService executionService)
        {
        }

        private void InitializeViewModel()
        {
            try
            {
                LoadInitialData();
                InitializeInstruments();
            }
            catch (Exception ex)
            {
                ExecutionLog.Add($"Initialization Error: {ex.Message}");
            }
        }

        private void LoadInitialData()
        {
            var savedBlocks = _blockService.GetBlocks();
            _blocks.Clear();
            foreach (var block in savedBlocks)
            {
                _blocks.Add(block);
            }

            UpdateBlockTemplates();
        }

        private void UpdateBlockTemplates()
        {
            _blockTemplates.Clear();
            var templates = _blockService.GetBlockTemplates(SelectedCategory);
            foreach (var template in templates)
            {
                _blockTemplates.Add(template);
            }
        }

        private void InitializeInstruments()
        {
            _availableInstruments.Clear();
            var instruments = _soundService.GetAvailableInstruments();
            foreach (var instrument in instruments)
            {
                _availableInstruments.Add(instrument);
            }
        }

        [RelayCommand]
        private void AddBlock(BlockTemplate template)
        {
            try
            {
                var newBlock = _blockService.CreateBlockFromTemplate(template, new Windows.Foundation.Point());
                _blocks.Add(newBlock);
                IsProjectModified = true;
            }
            catch (Exception ex)
            {
                ExecutionLog.Add($"Block Addition Error: {ex.Message}");
            }
        }

        [RelayCommand]
        private void DeleteBlock(Block block)
        {
            try
            {
                _blocks.Remove(block);
                _blockService.RemoveBlock(block);
                IsProjectModified = true;
            }
            catch (Exception ex)
            {
                ExecutionLog.Add($"Block Deletion Error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ExecuteProjectAsync()
        {
            if (IsExecuting) return;

            try
            {
                IsExecuting = true;
                ExecutionLog.Clear();

                foreach (var block in Blocks)
                {
                    await _executionService.ExecuteBlockAsync(block);
                }
            }
            catch (Exception ex)
            {
                ExecutionLog.Add($"Execution Error: {ex.Message}");
            }
            finally
            {
                IsExecuting = false;
            }
        }

        private void OnSelectedCategoryChanged(BlockCategory value)
        {
            UpdateBlockTemplates();
        }
    }
}
