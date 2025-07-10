using System;
using CargoWise.EntityFramework.Testing;
using static Enterprise.ZArchitecture.Business.HasChangesHunter;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class HasChangesRecurserTestCase : TestCaseWithFactory
	{
		public void TestIsIncludedNamespaceWhenEmpty()
		{
			HasChangesRecurser recurser = new HasChangesRecurser(System.Array.Empty<HasChangesHunterExclusionDetails>(), Array.Empty<string>());
			Assert(recurser.IsIncludedNamespace(typeof(System.String)));
		}

		public void TestIsIncludedNamespaceWhenPartialMatch()
		{
			HasChangesRecurser recurser = new HasChangesRecurser(System.Array.Empty<HasChangesHunterExclusionDetails>(), new string[] { "Enterprise.ZArchitecture" });
			Assert(recurser.IsIncludedNamespace(typeof(Environment.AccountingPeriodManager)));
		}

		public void TestIsIncludedNamespaceWhenFullMatch()
		{
			HasChangesRecurser recurser = new HasChangesRecurser(System.Array.Empty<HasChangesHunterExclusionDetails>(), new string[] { "Enterprise.ZArchitecture.Environment" });
			Assert(recurser.IsIncludedNamespace(typeof(Environment.AccountingPeriodManager)));
		}

		public void TestIsIncludedNamespaceWhenNoMatch()
		{
			HasChangesRecurser recurser = new HasChangesRecurser(System.Array.Empty<HasChangesHunterExclusionDetails>(), new string[] { "Enterprise.ZArchitecture.Environment" });
			AssertEquals(false, recurser.IsIncludedNamespace(typeof(Core.TempFile)));
		}

		public void TestIsIncludedNamespaceWhenDirectBaseType()
		{
			HasChangesRecurser recurser = new HasChangesRecurser(System.Array.Empty<HasChangesHunterExclusionDetails>(), new string[] { "MyNamespace" });
			Assert(recurser.IsIncludedNamespace(typeof(MyNamespace.Interface1)));
		}

		public void TestIsIncludedNamespaceWhenMultiLevelBaseType()
		{
			HasChangesRecurser recurser = new HasChangesRecurser(System.Array.Empty<HasChangesHunterExclusionDetails>(), new string[] { "MyNamespace" });
			Assert(recurser.IsIncludedNamespace(typeof(MyNamespace2.Interface2)));
		}
	}
}

namespace MyNamespace
{
	internal interface Interface1
	{ }
}

namespace MyNamespace2
{
	internal interface Interface2 : MyNamespace.Interface1
	{ }
}
