// See https://aka.ms/new-console-template for more information

using StringLib;
using System.Text.RegularExpressions;

var console = new ConsoleHelper(RegexOptions.IgnoreCase);

console.Rules.Add(new Rule("Comments", "\\/\\/.*\r", "green"));
console.Rules.Add(new Rule("Comments", "/\\*.*\\*/", "red"));

/* 
 * 
 * Hello, world!
 * 
 * 
 */


string text = File.ReadAllText(@"..\..\..\Program.cs");
console.WriteLine(text);