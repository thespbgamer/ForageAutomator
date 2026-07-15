namespace ForageAutomator.Automation
{
    internal static class ForageTypeExtensions
    {
        public static bool IsOtherInteraction(this ForageType type)
        {
            return type >= ForageType.CrabPot;
        }
    }

}
