using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class CodeFormatterForWebTest : TestCase
	{
		public void TestHighlightCodeBlock()
		{
			string csharpInput = @"What is the output of the following C# code?[code:csharp]string s1 = ""hello"";string s2 = 'h';Console.DoSomethingElse((object)s1);[/code]";
			string expectCsharpOutput = @"What is the output of the following C# code?<pre class=""codeblock""><font color=""#0000FF"">string</font> s1 = <font color=""#FF00FF"">""hello""</font>;<font color=""#0000FF"">string</font> s2 = <font color=""#FF00FF"">'h'</font>;<font color=""green"">Console</font>.DoSomethingElse((<font color=""#0000FF"">object</font>)s1);</pre>";
			AssertEquals(expectCsharpOutput, codeFormatter.HighlightCodeBlock(csharpInput));

			string tsqlInput = @"[code:tsql]ALTER INDEX id_index ON Employees (id, lastName);[/code] does not work because a column cannot be added to an existing index using the [code:tsql]ALTER INDEX[/code] statement";
			string expectTsqlOutput = @"<pre class=""codeblock""><font color=""#0000FF"">ALTER</font> <font color=""#0000FF"">INDEX</font> id_index <font color=""#0000FF"">ON</font> Employees (id, lastName);</pre> does not work because a column cannot be added to an existing index using the <pre class=""codeblock""><font color=""#0000FF"">ALTER</font> <font color=""#0000FF"">INDEX</font></pre> statement";
			AssertEquals(expectTsqlOutput, codeFormatter.HighlightCodeBlock(tsqlInput));

			string notSupportedInput = @"[code:html]some html code[/code]";
			AssertEquals(string.Empty, codeFormatter.HighlightCodeBlock(notSupportedInput));
		}

		public void TestHighlightOptions()
		{
			string input = "[code:csharp;ln=on;alt=on;title]sample code\r\n\"String one\"[/code]";
			string expectOutput = "<pre class=\"codeblock\"><pre class=\"alt\"><font color=\"#000000\">   1 </font>sample code</pre>\n<pre><font color=\"#000000\">   2 </font><font color=\"#FF00FF\">\"String one\"</font></pre>\n</pre>";
			AssertEquals(expectOutput, codeFormatter.HighlightCodeBlock(input));
		}

		readonly CodeFormatterForWeb codeFormatter = new CodeFormatterForWeb();
	}
}
