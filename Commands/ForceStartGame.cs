using CommandSystem;
using PluginAPI.Core;
using SwiftAPI.Commands;

namespace SwiftZombies.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class ForceStartGame : CommandBase
    {
        public override string[] GetAliases() => ["fs", "startgame"];

        public override string GetCommandName() => "start";

        public override string GetDescription() => "Starts the game with 1 player. ";

        public override PlayerPermissions[] GetPerms() => null;

        public override bool Function(string[] args, ICommandSender sender, out string result)
        {
            if (Round.IsRoundStarted)
            {
                result = "Round is already started! ";

                return false;
            }

            if (Server.PlayerCount > 1)
            {
                result = "Player count is more than 1! Wait for lobby to start.";

                return false;
            }

            Round.Start();

            result = "Started game with 1 player! ";

            return true;
        }
    }
}
