using StardewValley;

namespace ForageAutomator.Automation
{
    /// <summary>
    /// Matches vanilla animation cancel (Right-Shift + Del + R) so tool swings
    /// apply their effect without locking the player in the sprite animation.
    /// </summary>
    internal static class ToolAnimationHelper
    {
        public static void Cancel(Farmer player)
        {
            Game1.freezeControls = false;
            player.forceCanMove();
            player.completelyStopAnimatingOrDoingAction();
            player.UsingTool = false;
            player.canReleaseTool = true;
            player.FarmerSprite.PauseForSingleAnimation = false;
            Farmer.canMoveNow(player);
        }
    }

}
