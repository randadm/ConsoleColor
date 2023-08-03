using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace StringLib
{
    public class Rule
    {
        internal static int RuleCounter = 1;

        internal static Rule Empty = new Rule("Empty", "", "");
        public readonly string Name;
        public readonly string Pattern;  // regex  
        public readonly string Replace;

        public Rule(string find, string replace) : this("Rule" + RuleCounter++, find, replace)
        { }
        public Rule(string name, string find, string replace)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            this.Name = name;
            this.Pattern = find;
            this.Replace = replace;            
        }

        public override string ToString()
        {
            return Name;
        }
    }
    public class ConsoleHelper
    {
        public static Dictionary<string, ConsoleColor> ColorMap = new Dictionary<string, ConsoleColor>(StringComparer.OrdinalIgnoreCase)
        {
            { "black", ConsoleColor.Black },
            { "0", ConsoleColor.Black },
            { "darkblue", ConsoleColor.DarkBlue },
            { "1", ConsoleColor.DarkBlue },
            { "darkgreen", ConsoleColor.DarkGreen },
            { "2", ConsoleColor.DarkGreen },
            { "darkcyan", ConsoleColor.DarkCyan },
            { "3", ConsoleColor.DarkCyan },
            { "darkred", ConsoleColor.DarkRed },
            { "4", ConsoleColor.DarkRed },
            { "darkmagenta", ConsoleColor.DarkMagenta },
            { "5", ConsoleColor.DarkMagenta },
            { "darkyellow", ConsoleColor.DarkYellow },
            { "6", ConsoleColor.DarkYellow },
            { "gray", ConsoleColor.Gray },
            { "7", ConsoleColor.Gray },
            { "darkgray", ConsoleColor.DarkGray },
            { "8", ConsoleColor.DarkGray },
            { "blue", ConsoleColor.Blue },
            { "9", ConsoleColor.Blue },
            { "green", ConsoleColor.Green },
            { "10", ConsoleColor.Green },
            { "cyan", ConsoleColor.Cyan },
            { "11", ConsoleColor.Cyan },
            { "red", ConsoleColor.Red },
            { "12", ConsoleColor.Red },
            { "magenta", ConsoleColor.Magenta },
            { "13", ConsoleColor.Magenta },
            { "yellow", ConsoleColor.Yellow },
            { "14", ConsoleColor.Yellow },
            { "white", ConsoleColor.White },
            { "15", ConsoleColor.White },
        };
        private static ConsoleColor StartupForegroundConsoleColor;
        private static ConsoleColor StartupBackgroundConsoleColor;
        private const string DELIMETERS = ";:,";
        private string TagStart { get; }
        private string TagEnd { get; }
        private char Delim { get; }
        private static ConsoleColor TryGetColor(string v)
            => ColorMap.ContainsKey(v) ? ColorMap[v] : throw new IndexOutOfRangeException($"Unknown color '{v}'");
        public List<Rule> Rules = new List<Rule>();
        public Stopwatch TTE { get; private set; }
        private RegexOptions Options { get; }

        public void WriteLine(string text, ConsoleColor foreground, ConsoleColor? background = null) => Write(text + Environment.NewLine, foreground, background);
        public void Write(string text, ConsoleColor foreground, ConsoleColor? background = null)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background ?? StartupBackgroundConsoleColor;
            Write(text, false, true);
        }

        public void WriteLine(string text, bool disableRules = false) => Write(text, true, disableRules);

        /// <seealso cref="ColorMap"/>
        public void Write(string text, bool addNewLine = false, bool skipRules = false)
        {
            int orig_len = text.Length;
            TTE = Stopwatch.StartNew();
            if (!skipRules)
                text = PreprocessRules(text, Options);

            string expression = $"({Regex.Escape(TagStart)}[\\w\\d]+([{Regex.Escape(Delim.ToString())}][\\w\\d]+)?{Regex.Escape(TagEnd)})";
            var matches = Regex.Matches(text, expression, Options);

            int len = 0;
            if (!matches.Any())
            {
                len += text.Length;
                Console.WriteLine(text);
            }
            else
            {
                int index = 0;
                foreach (Match match in matches)
                {
                    Console.Write(text.Substring(index, match.Index - index));
                    len += match.Index - index;

                    string input = match.Value.Substring(TagStart.Length, match.Value.Length - (TagStart.Length + TagEnd.Length));
                    string[] tokens = input.Split(DELIMETERS.ToCharArray());

                    if (input.StartsWith("reset", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.ResetColor();
                    }
                    else if (input.StartsWith("default", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.ResetColor();
                    }
                    else
                    {
                        if (tokens.Length >= 1)
                            Console.ForegroundColor = TryGetColor(tokens[0]);

                        if (tokens.Length >= 2)
                            Console.BackgroundColor = TryGetColor(tokens[1]);
                    }
                    index = match.Index + match.Length;
                }
                Console.Write(text.Substring(index));
                len += text.Length - index;
            }

            Console.ResetColor();
            if (addNewLine)
                Console.WriteLine();
            TTE.Stop();
#if DEBUG
            Debug.WriteLine($"{nameof(Write)} processed {len}/{orig_len} characters{(skipRules | Rules.Count == 0 ? " not applying any rules" : $" processing {Rules.Count} rule(s)")} in {TTE.ElapsedMilliseconds}ms");
#endif
        }

        private List<string> Rebuild(string text, Dictionary<Match, string> items)
        {
            var list = new List<string>();

            if (!items.Any())
                return list;


            Dictionary<int, Tuple<int, string, string>> segments = new Dictionary<int, Tuple<int, string, string>>();

            int prev_left = 0;

            foreach (Match m in items.Keys)
            {
                int left = m.Index;
                int right = m.Index + m.Length;

                if (prev_left < left)
                    segments.Add(prev_left, new Tuple<int, string, string>(left, text.Substring(prev_left, left - prev_left), text.Substring(prev_left, left - prev_left)));

                segments.Add(left, new Tuple<int, string, string>(right, text.Substring(left, right - left), items[m]));

                prev_left = right;
            }
            if (prev_left < text.Length)
                segments.Add(prev_left, new Tuple<int, string, string>(text.Length, text.Substring(prev_left, text.Length - prev_left), text.Substring(prev_left, text.Length - prev_left)));

            foreach (var item in segments.Values)
            {
                list.Add(item.Item3);
            }

            return list;
        }

        private List<string> MaskAndRebuild(string text, Rule rule, RegexOptions options)
        {
            var matches = Regex.Matches(text, rule.Pattern, options);
            Dictionary<Match, string> replacements = new Dictionary<Match, string>();
            
            foreach (Match match in matches)
            {
                //Debug.WriteLine($">> Creating {nameof(text.IsImmutable)}: pos= {match.Index}, len= {match.Length} ");
                //Debug.WriteLine($"   {part}");
                //Debug.WriteLine($"   {part.Mask.Convert()}");
                //Debug.WriteLine($"");
                //
                string replacementText = $"{TagStart}{rule.Replace}{TagEnd}{match.Value}{TagStart}reset{TagEnd}";

                if (!replacements.ContainsKey(match))
                    replacements.Add(match, replacementText);
            }

            //Debug.WriteLine($">> Applying: '{rule.Name}' to {replacements.Count} item(s)");
            //Debug.WriteLine($"   {text.Value}");
            //Debug.WriteLine($"   {text.Mask.Convert()}");
            //Debug.WriteLine($"");

            var results = Rebuild(text,replacements);

            return results;
        }
        private void Traverse(List<string> list, int ruleId, ref List<string> ol, RegexOptions options)
        {
            if (ruleId > Rules.Count - 1)
            {
                ol.AddRange(list);
                return;
            }

            var rule = Rules[ruleId];

            foreach (var item in list)
            {
                var items = MaskAndRebuild(item, rule, options);

                if (items.Any())
                {
                    Traverse(items, ruleId + 1, ref ol, options);
                }
                else
                {
                    ol.Add(item);
                }
            }
        }
        private string PreprocessRules(string text, RegexOptions options)
        {
            var list = new List<string>();
            
            Traverse(new List<string>() { text, }, 0, ref list, options);

            return string.Concat(list);
        }

        static ConsoleHelper()
        {
            StartupForegroundConsoleColor = Console.ForegroundColor;
            StartupBackgroundConsoleColor = Console.BackgroundColor;
        }
        public ConsoleHelper(RegexOptions options = RegexOptions.None, string header = "_[", string terminator = "]_", char delim = ';')
        {
            TagStart = header;
            TagEnd = terminator;
            Delim = delim;
            TTE = new Stopwatch();
            Options = options;
        }
    }
}