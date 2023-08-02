using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace StringLib
{
    public static class BitMaskExtension
    {
        public static string Convert(this BitArray obj)
        {
            StringBuilder sb = new StringBuilder(obj.Length);

            for (int i = 0; i < obj.Length; i++)
            {
                sb.Append(obj[i] ? '^' : ' ');
            }

            return sb.ToString();
        }
        public static BitArray Submask(this BitArray obj, int index, int length = -1)
        {
            if (length < 0) length = obj.Length - index;
            BitArray result = new BitArray(length);
            for (int i = 0; i < length; i++)
            {
                result[i] = obj[index + i];
            }
            return result;
        }
        public static BitArray Concat(this BitArray obj, BitArray other)
        {
            int totalLength = obj.Length + other.Length;
            BitArray result = new BitArray(totalLength);

            for (int i = 0; i < obj.Length; i++)
                result[i] = obj[i];
            for (int i = 0; i < other.Length; i++)
                result[i] = other[i];

            return result;
        }
        public static void Fill(this BitArray obj, int index, int length = -1, bool value = true)
        {
            if (length < 0) length = obj.Length - index;
            for (int i = index; i < index + length; i++)
            {
                obj[i] = value;
            }
        }
        public static bool IsAllSet(this BitArray obj, bool optimistic = false)
        {
            if (obj.Length == 0) return false;
            if (optimistic) return obj[0];

            foreach (bool b in obj) if (b == false) return false;
            return true;
        }
        public static int GetLength(this BitArray obj, int index)
        {
            int count = 0;
            while (index + count < obj.Length && obj[index] == obj[index + count]) count++;
            return count;
        }

    }
    public class Rule
    {
        internal static Rule Empty = new Rule("", "", "Empty");
        internal static int RuleCounter = 1;
        public readonly string Name;
        public readonly string Pattern;  // regex  
        public readonly string Replace;


        public Rule(string find, string replace) : this(find, replace, "") { }


        public Rule(string name, string find, string replace)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "Rule" + RuleCounter++;
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
        internal class Text
        {
            public string Value;
            public string Substitute;
            public Rule Rule;
            public BitArray Mask;
            public bool IsImmutable => Mask.IsAllSet();
            public Text(string value, bool immutable = false)
            {
                this.Value = value;
                this.Substitute = "";
                this.Mask = new BitArray(value.Length, immutable);
                this.Rule = Rule.Empty;
            }
            private Text(string value, BitArray mask)
            {
                this.Value = value;
                this.Substitute = "";
                this.Mask = mask;
                this.Rule = Rule.Empty;
            }
            public int Length => Value.Length;
            public Text Insert(Text text, Match pos, Text insert, out int lengthChange)
            {
                Text left = text.Left(pos);
                Text right = text.Right(pos);
                int prev_len = pos.Length;
                int new_len = insert.Length;

                if (prev_len == new_len)
                    lengthChange = 0;
                else
                    lengthChange = new_len - prev_len;

                return left.Concat(insert).Concat(right);
            }
            public List<Text> Rebuild(Dictionary<Match, string> items)
            {
                if (!items.Any())
                    return new List<Text>();

                List<Text> list = new List<Text>();

                Dictionary<int, Tuple<int, Text, string>> segments = new Dictionary<int, Tuple<int, Text, string>>();

                int prev_left = 0;

                foreach (Match m in items.Keys)
                {
                    int left = m.Index;
                    int right = m.Index + m.Length;

                    if (prev_left < left)
                        segments.Add(prev_left, new Tuple<int, Text, string>(left, Mid(prev_left, left - prev_left), Mid(prev_left, left - prev_left).Value));

                    segments.Add(left, new Tuple<int, Text, string>(right, Mid(left, right - left), items[m]));

                    prev_left = right;
                }
                if (prev_left < Length)
                    segments.Add(prev_left, new Tuple<int, Text, string>(Length, Mid(prev_left, Length - prev_left), Mid(prev_left, Length - prev_left).Value));

                foreach (var item in segments.Values)
                {
                    list.Add(new Text(item.Item3));
                }

                //foreach (int j in segments.Keys)
                //{
                //    Text segment = Mid(j, segments[j] - j);
                //    if (items.j))
                //    {
                //        segment.Substitute = replacements[segment.Value];
                //    }
                //    list.Add(segment);
                //}


                return list;
            }

            public Text Left(Match m) => Left(m.Index);
            public Text Right(Match m) => Right(m.Index + m.Length);
            public Text Mid(Match m) => Mid(m.Index, m.Length);
            public Text Left(int pos)
            {
                return new Text(Value.Substring(0, pos), Mask.Submask(0, pos));
            }
            public Text Right(int pos)
            {
                return new Text(Value.Substring(pos), Mask.Submask(pos));
            }
            public Text Mid(int index, int length)
            {
                return new Text(Value.Substring(index, length), Mask.Submask(index, length));
            }
            public Text Concat(Text other)
            {
                return new Text(this.Value + other.Value, this.Mask.Concat(other.Mask));
            }
            internal Text Clone()
            {
                return new Text(Value, Mask);
            }
            public override string ToString()
            {
                return this.Value + (IsImmutable ? $" [{nameof(IsImmutable)}]" : "");
            }
        }

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

        public void WriteLine(string text, bool disableRules = false) => Write(text, true, disableRules);
        public void Write(string text, ConsoleColor foreground, ConsoleColor? background = null)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background ?? StartupBackgroundConsoleColor;
            Write(text, false, true);
        }

        /// <summary>
        /// Output string can be escaped with the following command '*e[foreground|(;background)m'
        /// </summary>
        /// <param name="text">output string</param>
        /// <seealso cref="ColorMap"/>
        public void Write(string text, bool addNewLine = false, bool skipRules = false)
        {
            int orig_len = text.Length;
            TTE = Stopwatch.StartNew();
            if (!skipRules)
                text = ApplyRules(text, Options);

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

            Debug.WriteLine($"{nameof(Write)} processed {len}/{orig_len} characters{(skipRules | Rules.Count == 0 ? " not applying any rules" : $" processing {Rules.Count} rule(s)")} in {TTE.ElapsedMilliseconds}ms");
        }
        private List<Text> CreateLeafs(Text text, Rule rule, RegexOptions options)
        {
            var matches = Regex.Matches(text.Value, rule.Pattern, options);

            List<Text> list = new List<Text>();
            Dictionary<Match, string> replacements = new Dictionary<Match, string>();

            foreach (Match match in matches)
            {
                text.Mask.Fill(match.Index, match.Length);

                Text part = text.Mid(match);

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

            var results = text.Rebuild(replacements);


            return results;
        }
        private void CreateBranch(List<Text> list, int ruleId, ref List<Text> ol, RegexOptions options)
        {
            if (ruleId > Rules.Count - 1)
            {
                ol.AddRange(list);
                return;
            }

            var rule = Rules[ruleId];

            foreach (var item in list)
            {
                if (!item.IsImmutable)
                {
                    var items = CreateLeafs(item, rule, options);

                    if (items.Any())
                    {
                        CreateBranch(items, ruleId + 1, ref ol, options);
                    }
                    else
                    {
                        ol.Add(item);
                    }
                }
                else
                {
                    ol.Add(item);
                }
            }
        }
        private string ApplyRules(string text, RegexOptions options)
        {
            var list = new List<Text>();
            CreateBranch(new List<Text>() { new Text(text), }, 0, ref list, options);

            // concat
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                if (item.IsImmutable)
                    sb.Append(item.Substitute);
                else
                    sb.Append(item.Value);
            }
            text = sb.ToString();
            return text;
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