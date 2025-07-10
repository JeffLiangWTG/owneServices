using System.IO;
using System.Reflection;

namespace WinzorTestAdapter.Test
{
	static class TestClassesInfo
	{
		public static string TestClassesSource => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "WinzorTestAdapter.TestClasses.dll");

		public const string PassingTest = "WinzorTestAdapter.TestClasses.TestClass.TestPass";
		public const string FailingTest = "WinzorTestAdapter.TestClasses.TestClass.TestFail";
		public const string ExplicitTest = "WinzorTestAdapter.TestClasses.TestClass.TestExplicit";
		public const string ExplicitWithWhiteSpaceTest = "WinzorTestAdapter.TestClasses.TestClass.TestExplicitWithWhiteSpace";
		public const string SlowTest = "WinzorTestAdapter.TestClasses.TestClass.TestSlow";
		public const string RequiresSourceCodeTest = "WinzorTestAdapter.TestClasses.TestClass.TestRequiresSourceCode"; 
		public const string GuiTest = "WinzorTestAdapter.TestClasses.TestClass.TestGui";
	}
}
