// See https://aka.ms/new-console-template for more information

using StringLib;
using System.Text.RegularExpressions;

var console = new ConsoleHelper(RegexOptions.IgnoreCase);

console.Rules.Add(new Rule("hello", "red"));
console.WriteLine("Hello, world!"); 