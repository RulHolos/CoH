using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game.Views;

public partial class ConfigMenu : View
{
    public override ILogger Logger { get; set; }

    public ConfigMenu()
        : base()
    {
        Logger = Log.ForContext("Tag", "ConfigMenu");
    }

    public override void Frame(float deltaTime)
    {
        
    }

    public override void Render(float deltaTime)
    {
        
    }
}
