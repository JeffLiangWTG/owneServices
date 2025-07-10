using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AntlrMacroContextTest : TestCase
	{
		public void TestScopeConstructed()
		{
			var env = new Macros.Environment();
			Dictionary<string, (object, Type)> variables = new Dictionary<string, (object, Type)>
			{
				{ "var1", (1, typeof(int)) },
				{ "var2", (null, typeof(int)) }
			};
			var context = new AntlrMacroContext(null, typeof(int), null, variables, null);
			AssertEquals("Should add variable with actual object", 1, context.Scope.GetVariable("var1"));
			AssertEquals("Should add variable with null object", null, context.Scope.GetVariable("var2"));

			var context2 = new AntlrMacroContext(null, typeof(int), null, null, null);
			AssertNoExceptionThrown(
				"Null Varibales should be accepted", () =>
				{
					var scope = context2.Scope;
				});
		}

		public void TestInvalidArguments() 
		{
			AssertNoExceptionThrown(
				"No Exception when Parent is not null", () =>
				{
					var context = new AntlrMacroContext(1, null, null, null, null);
				});
			AssertNoExceptionThrown(
				"No Exception when ParentType is not null", () =>
				{
					var context = new AntlrMacroContext(null, typeof(int), null, null, null);
				});
			AssertExceptionThrown(
				typeof(ArgumentNullException), () =>
				{
					var context = new AntlrMacroContext(null, null, null, null, null);
				});
		}
	}
}
