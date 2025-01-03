// KodKit.Services.Interfaces/IBlockService.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using KodKit.Models;
using Windows.Foundation;

namespace KodKit.Services.Interfaces
{
    public interface IBlockService
    {
        ObservableCollection<Block> GetBlocks();
        void AddBlock(Block block);
        void RemoveBlock(Block block);
        void MoveBlock(Block block, int newIndex);
        void UpdateBlockPosition(Block block, Point position);
        void ClearBlocks();
        void ConnectBlocks(Block parent, Block child);
        void DisconnectBlocks(Block block);
        void AddBlockToGroup(Block parentBlock, Block childBlock);
        void RemoveBlockFromGroup(Block parentBlock, Block childBlock);
        Task ExecuteBlocksAsync();
        Task ExecuteBlockTreeAsync(Block block);
        Task ExecuteSingleBlockAsync(Block block);
        void SetVariable(string name, object value);
        object GetVariable(string name);
        bool HasVariable(string name);
        bool CanConnectBlocks(Block parent, Block child);
        bool IsValidBlockPosition(Point position);
        void ImportBlocks(IEnumerable<Block> blocks);
        IEnumerable<Block> ExportBlocks();
        Block CreateCustomBlock(string name, BlockType type, string? parameter = null);
        void RegisterCustomBlockType(string typeName, Func<Block, Task> handler);
        IEnumerable<BlockTemplate> GetBlockTemplates(BlockCategory category);
        Block CreateBlockFromTemplate(BlockTemplate template, Point position);
        Sprite? ActiveSprite { get; }
        void AddSprite(Sprite sprite);
        void RemoveSprite(string spriteId);
        void SetActiveSprite(Sprite? sprite);
        IEnumerable<Sprite> GetAllSprites();
        Task SaveProjectAsync(string filename);
        Task LoadProjectAsync(string filename);
    }
}
