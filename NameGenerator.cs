namespace SLGameLogger
{
    public static class NameGenerator
    {
        private static readonly Random _random = new();

        private static readonly List<string> _phoneticAlphabet = new()
        {
            "Alpha",
            "Bravo",
            "Charlie",
            "Delta",
            "Echo",
            "Foxtrot",
            "Golf",
            "Hotel",
            "India",
            "Juliett",
            "Kilo",
            "Lima",
            "Mike",
            "November",
            "Oscar",
            "Papa",
            "Quebec",
            "Romeo",
            "Sierra",
            "Tango",
            "Uniform",
            "Victor",
            "Whiskey",
            "X-ray",
            "Yankee",
            "Zulu"
        };

        public static string GetName()
        {
            string result = "";

            result += _phoneticAlphabet.RandomItem();
            result += "-";
            result += _phoneticAlphabet.RandomItem();
            result += "-d";
            result += DateTime.UtcNow.Day;
            result += "m";
            result += DateTime.UtcNow.Month;
            result += "-";
            result += DateTime.UtcNow.Hour.ToString() + DateTime.UtcNow.Minute.ToString() + DateTime.UtcNow.Second.ToString();

            return result;
        }
    }
}