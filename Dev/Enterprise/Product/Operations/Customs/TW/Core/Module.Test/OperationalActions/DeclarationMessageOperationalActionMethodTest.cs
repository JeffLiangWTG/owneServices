using System.Linq;
using Enterprise.Customs.TW.Module.OperationalActions;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(DeclarationMessageOperationalActionMethod))]
	public sealed class DeclarationMessageOperationalActionMethodTest : OperationalActionMethodTest<DeclarationMessageOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new DeclarationMessageOperationalActionMethod();
			CombineAssertions(() =>
			{
				AssertEquals("TestMethodDescription", "Submit Original Entry to Taiwan Customs", method.Description);
				AssertEquals("TestMethodName", "Configuration", method.Name);
				AssertEquals("TestHasControl", true, method.HasControl);
				AssertEquals("TestHasSettings", false, method.HasSettings);
				AssertEquals("TestHasSettings", false, method.IsRunAgainDisabled);
				using (var control = method.NewGuiControl())
				{
					AssertType<SendOriginalEntryConfigurationControl>(control);
				}
			}

			);
		}

		public void TestGetFilterRequirements()
		{
			var filterRequirements = new DeclarationMessageOperationalActionMethod().GetFilterRequirements();
			Assert(filterRequirements.Any(x => x.ConstraintName == FilterConstants.Country && x.Contains("TW")));
		}

		public void TestNewApplicatorType()
		{
			AssertType<DeclarationMessageOperationalActionMethodApplicator>(new DeclarationMessageOperationalActionMethod().NewApplicator(null, null));
		}

		protected override DeclarationMessageOperationalActionMethod NewMethod()
		{
			return new DeclarationMessageOperationalActionMethod();
		}
	}
}
