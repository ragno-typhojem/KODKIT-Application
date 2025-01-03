using System.Collections.Generic;
using System;
using KodKit.Models;

public class Sprite
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Sprite";
    public double X { get; set; }
    public double Y { get; set; }
    public double Direction { get; set; } = 90;
    public bool Visible { get; set; } = true;
    public string CurrentCostume { get; set; } = "default";
    public List<string> Costumes { get; set; } = new();
    public List<Block> Scripts { get; set; } = new();
}