using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LogMacroExecutorTests : TestCase
	{
		public void TestExecuteWithNoEmbeddedMacro()
		{
			AssertMacroEquals("Hello there|FOO=Bar|BAZ=BOO", "Hello there|FOO=Bar|BAZ=BOO");
		}

		public void TestExecuteWithEmbeddedMacro()
		{
			AssertMacroEquals("Hello <@data.Name>|FOO=<@data.Foo>|BAR=<@data.Bar>", "Hello Bob|FOO=fff|BAR=bbb", new { Name = "Bob", Foo = "fff", Bar = "bbb" });
		}

		void AssertMacroEquals(string macro, string expected, object data = null)
		{
			var executor = new LogMacroExecutor(macro);
			var result = executor.Execute(data ?? new object());

			AssertEquals("Errors: \n" + string.Join("\r\n", result.Item2.Select(msg => msg.Message)), expected, result.Item1);
		}
	}
}
