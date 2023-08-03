// See https://aka.ms/new-console-template for more information

using StringLib;
using System.Text.RegularExpressions;

var console = new ConsoleHelper(RegexOptions.IgnoreCase);

console.Rules.Add(new Rule("Comments", "\\/\\/.*(\r|$)", "darkgreen"));
console.Rules.Add(new Rule("Numbers", "\\d+", "cyan"));
console.Rules.Add(new Rule("url+emails", "((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\\+\\$,\\w]+@)[A-Za-z0-9.-]+)((?:\\/[\\+~%\\/.\\w\\-_]*)?\\??(?:[-\\+=&;%@.\\w_]*)#?(?:[.\\!\\/\\\\w]*))?)", "red"));

/* 
 * 
 * Hello, world!
 * 
 * 
 */


string text = "//Hello, this is Randall Adamson, my contact number is 123-456-7890, you can reach me at randall_adamson@email.com, and check my profile at www.randalladamson.com for more information.";  File.ReadAllText(@"..\..\..\ConsoleHelper.cs");
console.WriteLine(text);