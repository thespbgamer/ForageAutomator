using Microsoft.Xna.Framework;

namespace ForageAutomator.Rendering
{
    internal static class ConfigColor
    {
        public const string DefaultReady = "80,255,100,200";
        public const string DefaultOutOfRange = "80,180,255,200";
        public const string DefaultMissingTool = "255,210,50,220";
        public const string DefaultInventoryFull = "255,80,80,220";
        public const string DefaultUnreachable = "160,160,160,200";
        public const string DefaultEmptyBush = "120,120,120,160";

        public const string DefaultCrabPot = "255,140,60,200";
        public const string DefaultFruitTree = "200,120,255,200";
        public const string DefaultMachine = "160,200,255,200";
        public const string DefaultTapper = "180,140,80,200";
        public const string DefaultBeeHouse = "255,220,80,200";
        public const string DefaultMushroomBox = "140,200,120,200";
        public const string DefaultGarbageCan = "200,200,200,200";
        public const string DefaultHay = "220,180,100,200";
        public const string DefaultNpcWitness = "255,120,120,220";

        public static Color Ready => Parse(DefaultReady, Color.Lime);
        public static Color OutOfRange => Parse(DefaultOutOfRange, Color.CornflowerBlue);
        public static Color MissingTool => Parse(DefaultMissingTool, Color.Gold);
        public static Color InventoryFull => Parse(DefaultInventoryFull, Color.Red);
        public static Color Unreachable => Parse(DefaultUnreachable, Color.Gray);
        public static Color EmptyBush => Parse(DefaultEmptyBush, Color.DimGray);
        public static Color NpcWitness => Parse(DefaultNpcWitness, Color.IndianRed);

        public static void Sanitize(ModConfig config)
        {
            config.ColorLineReady = SanitizeValue(config.ColorLineReady, DefaultReady);
            config.ColorLineOutOfRange = SanitizeValue(config.ColorLineOutOfRange, DefaultOutOfRange);
            config.ColorLineMissingTool = SanitizeValue(config.ColorLineMissingTool, DefaultMissingTool);
            config.ColorLineInventoryFull = SanitizeValue(config.ColorLineInventoryFull, DefaultInventoryFull);
            config.ColorLineUnreachable = SanitizeValue(config.ColorLineUnreachable, DefaultUnreachable);
            config.ColorLineEmptyBush = SanitizeValue(config.ColorLineEmptyBush, DefaultEmptyBush);

            SanitizeInteractionRule(config.OtherInteractions.CrabPots, DefaultCrabPot);
            SanitizeInteractionRule(config.OtherInteractions.FruitTrees, DefaultFruitTree);
            SanitizeInteractionRule(config.OtherInteractions.Machines, DefaultMachine);
            SanitizeInteractionRule(config.OtherInteractions.Tappers, DefaultTapper);
            SanitizeInteractionRule(config.OtherInteractions.BeeHouses, DefaultBeeHouse);
            SanitizeInteractionRule(config.OtherInteractions.MushroomBoxes, DefaultMushroomBox);
            SanitizeInteractionRule(config.OtherInteractions.GarbageCans, DefaultGarbageCan);
            SanitizeInteractionRule(config.OtherInteractions.HayGrass, DefaultHay);
        }

        private static void SanitizeInteractionRule(InteractionRule rule, string defaultColor)
        {
            rule.LineColor = SanitizeValue(rule.LineColor, defaultColor);
        }

        public static string SanitizeValue(string value, string defaultValue)
        {
            return IsValid(value) ? value : defaultValue;
        }

        public static Color Parse(string value, Color fallback)
        {
            if (!IsValid(value))
                return fallback;

            string[] parts = value.Split(',');
            TryByte(parts[0], out byte r);
            TryByte(parts[1], out byte g);
            TryByte(parts[2], out byte b);

            byte a = 255;
            if (parts.Length >= 4)
                TryByte(parts[3], out a);

            return new Color(r, g, b, a);
        }

        private static bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string[] parts = value.Split(',');
            if (parts.Length is < 3 or > 4)
                return false;

            if (!TryByte(parts[0], out _)
                || !TryByte(parts[1], out _)
                || !TryByte(parts[2], out _))
                return false;

            if (parts.Length == 4 && !TryByte(parts[3], out _))
                return false;

            return true;
        }

        private static bool TryByte(string text, out byte value)
        {
            if (int.TryParse(text.Trim(), out int parsed) && parsed is >= 0 and <= 255)
            {
                value = (byte)parsed;
                return true;
            }

            value = 0;
            return false;
        }
    }

}
