// See https://aka.ms/new-console-template for more information

using StringLib;
using System.Text.RegularExpressions;

var console = new ConsoleHelper(RegexOptions.IgnoreCase);

console.Rules.Add(new Rule("Comments", "\\/\\/.*\r", "green"));

/* 
 * 
 * Hello, world!
 * 
 * 
 */


string text = File.ReadAllText(@"..\..\..\ConsoleHelper.cs");
console.WriteLine(text);