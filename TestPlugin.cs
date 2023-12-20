using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;

namespace CSTest;
public class TestPlugin : BasePlugin
{
    public override string ModuleName => "Test Plugin";
    public override string ModuleVersion => "0.0.1";
    private List<CCSPlayerController> connectedPlayers = new List<CCSPlayerController>();
    private List<CCSPlayerController> noSpreadPlayers = new List<CCSPlayerController>();
    public override void Load(bool hotReload)
    {
        if (hotReload)
        {
            Utilities.GetPlayers().ForEach(controller =>
            {
                connectedPlayers.Add(controller);
            });
        }

        RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
        {
            var player = @event.Userid;

            if (player.IsBot || !player.IsValid)
            {
                return HookResult.Continue;

            }
            else
            {
                connectedPlayers.Add(player);
                return HookResult.Continue;
            }
        });

        RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            var player = @event.Userid;

            if (player.IsBot || !player.IsValid)
            {
                return HookResult.Continue;

            }
            else
            {
                connectedPlayers.Remove(player);
                
                return HookResult.Continue;
            }
        });

        RegisterListener<Listeners.OnTick>(() =>
        {
            foreach (var player in connectedPlayers)
            {
                if (player.IsValid && !player.IsBot && player.PawnIsAlive)
                {
                    var velocity = player.AbsVelocity.Length2D();
                    if (velocity > 50.0)
                    {
                        noSpreadPlayers.Add(player);
                    }
                    else
                    {
                        
                        noSpreadPlayers.Remove(player);
                        
                    }
                }
            }
        });

        RegisterEventHandler<EventWeaponFire>((@event,info) => {
            var player = @event.Userid;
            if (noSpreadPlayers.Contains(player))
            {
                ConVar.Find("weapon_accuracy_nospread")?.SetValue("1");
            }
            else
            {
                ConVar.Find("weapon_accuracy_nospread")?.SetValue("0");
            }
            return HookResult.Continue;
        });
    }
}
