using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using KodKit.Models;
using KodKit.Services.Interfaces;

namespace KodKit.Services
{
    public class BlockService : IBlockService
    {
        private readonly Dictionary<string, object> _variables;
        private readonly List<Block> _blocks;
        private readonly Dictionary<string, List<Block>> _blockGroups;

        public BlockService()
        {
            _variables = new Dictionary<string, object>();
            _blocks = new List<Block>();
            _blockGroups = new Dictionary<string, List<Block>>();
        }

        public void AddBlockToGroup(Block parentBlock, Block childBlock)
        {
            if (parentBlock == null)
                throw new ArgumentNullException(nameof(parentBlock), "Parent block cannot be null");
            if (childBlock == null)
                throw new ArgumentNullException(nameof(childBlock), "Child block cannot be null");

            // Döngüsel bağlantı kontrolü
            if (IsCircularReference(parentBlock, childBlock))
                throw new InvalidOperationException("Adding this block would create a circular reference");

            // Eğer child block başka bir grubun parçasıysa, önce onu kaldır
            foreach (var group in _blockGroups.Values)
            {
                group.Remove(childBlock);
            }

            // Parent block'un group ID'sini kontrol et
            string groupId = parentBlock.Id.ToString();
            if (!_blockGroups.ContainsKey(groupId))
            {
                _blockGroups[groupId] = new List<Block>();
            }

            if (!parentBlock.InnerBlocks.Contains(childBlock))
            {
                parentBlock.InnerBlocks.Add(childBlock);
                _blockGroups[groupId].Add(childBlock);
            }
        }

        public void RemoveBlockFromGroup(Block parentBlock, Block childBlock)
        {
            if (parentBlock == null)
                throw new ArgumentNullException(nameof(parentBlock), "Parent block cannot be null");
            if (childBlock == null)
                throw new ArgumentNullException(nameof(childBlock), "Child block cannot be null");

            string groupId = parentBlock.Id.ToString();
            if (_blockGroups.ContainsKey(groupId))
            {
                _blockGroups[groupId].Remove(childBlock);
                parentBlock.InnerBlocks.Remove(childBlock);
            }
        }

        public void SetVariable(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

            _variables[name] = value;
        }

        public object GetVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

            if (!_variables.TryGetValue(name, out var value))
                throw new KeyNotFoundException($"Variable '{name}' not found");

            return value;
        }

        public bool HasVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

            return _variables.ContainsKey(name);
        }

        public bool CanConnectBlocks(Block sourceBlock, Block targetBlock)
        {
            if (sourceBlock == null || targetBlock == null)
                return false;

            // Aynı blokları bağlama kontrolü
            if (sourceBlock.Id == targetBlock.Id)
                return false;

            // Döngüsel bağlantı kontrolü
            return !IsCircularReference(sourceBlock, targetBlock);
        }

        public bool IsValidBlockPosition(Point position)
        {
            // Temel pozisyon doğrulama
            if (position.X < 0 || position.Y < 0)
                return false;

            // Çakışma kontrolü
            return !_blocks.Any(block =>
                Math.Abs(block.Position.X - position.X) < 10 &&
                Math.Abs(block.Position.Y - position.Y) < 10);
        }

        public void ImportBlocks(IEnumerable<Block> blocks)
        {
            if (blocks == null)
                throw new ArgumentNullException(nameof(blocks));

            _blocks.Clear();
            _blockGroups.Clear();
            _blocks.AddRange(blocks);

            // Grup ilişkilerini yeniden oluştur
            foreach (var block in _blocks.Where(b => b.InnerBlocks.Any()))
            {
                string groupId = block.Id.ToString();
                _blockGroups[groupId] = new List<Block>(block.InnerBlocks);
            }
        }

        public IEnumerable<Block> ExportBlocks()
        {
            return new List<Block>(_blocks);
        }

        public Block CreateCustomBlock(string name, BlockType type, string? parameter = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Block name cannot be null or empty", nameof(name));

            var block = new Block
            {
                Id = Guid.NewGuid(),
                Type = type.ToString(),
                Parameter = parameter ?? GetDefaultParameter(type),
                Category = GetBlockCategory(type),
                Position = new Point(0, 0),
                IsTopLevel = IsTopLevelBlock(type)
            };

            _blocks.Add(block);
            return block;
        }

        // Yardımcı metodlar
        private bool IsCircularReference(Block sourceBlock, Block targetBlock)
        {
            var visited = new HashSet<Guid>();
            var current = targetBlock;

            while (current != null)
            {
                if (!visited.Add(current.Id))
                    return true;
                if (current.Id == sourceBlock.Id)
                    return true;
                current = current.NextBlock;
            }

            return false;
        }

        private string GetBlockCategory(BlockType type)
        {
            return type switch
            {
                BlockType.MoveSteps or BlockType.TurnRight or BlockType.TurnLeft or
                BlockType.GoToPosition or BlockType.SetXPosition or BlockType.SetYPosition
                    => "Motion",

                BlockType.SayMessage or BlockType.ShowSprite or BlockType.HideSprite or
                BlockType.ChangeCostume
                    => "Looks",

                BlockType.PlaySound or BlockType.StopAllSounds or BlockType.PlayNote or
                BlockType.PlayDrum
                    => "Sound",

                BlockType.WhenFlagClicked or BlockType.WhenKeyPressed or
                BlockType.WhenSpriteClicked
                    => "Events",

                BlockType.Wait or BlockType.Repeat or BlockType.Forever or BlockType.IfThen or
                BlockType.WaitUntil
                    => "Control",

                BlockType.SetVariable or BlockType.ChangeVariable or BlockType.ShowVariable
                    => "Variables",

                _ => "Unknown"
            };
        }

        private string GetDefaultParameter(BlockType type)
        {
            return type switch
            {
                BlockType.MoveSteps => "10",
                BlockType.TurnRight or BlockType.TurnLeft => "90",
                BlockType.GoToPosition => "0, 0",
                BlockType.SetXPosition or BlockType.SetYPosition => "0",
                BlockType.SayMessage => "Merhaba!",
                BlockType.PlaySound => "pop",
                BlockType.PlayNote => "60",
                BlockType.PlayDrum => "1",
                BlockType.Wait => "1",
                BlockType.Repeat => "10",
                BlockType.WaitUntil => "true",
                BlockType.SetVariable or BlockType.ChangeVariable => "0",
                _ => string.Empty
            };
        }

        private bool IsTopLevelBlock(BlockType type)
        {
            return type switch
            {
                BlockType.WhenFlagClicked or
                BlockType.WhenKeyPressed or
                BlockType.WhenSpriteClicked => true,
                _ => false
            };
        }
    }
}
