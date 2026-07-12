using StardewModdingAPI;

namespace ForageAutomator
{
    internal static class ModTranslations
    {
        private static ITranslationHelper translation = null!;

        public static void Initialize(ITranslationHelper helper) => translation = helper;

        public static string Get(string key) => translation.Get(key);

        public static string Get(string key, object tokens) => translation.Get(key, tokens);
    }

}
