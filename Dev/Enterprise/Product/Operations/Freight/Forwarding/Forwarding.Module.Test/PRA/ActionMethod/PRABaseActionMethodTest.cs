using System.Collections.Generic;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	internal abstract class PRABaseActionMethodTest<T> : OperationalActionMethodTest<T>
		where T : PRABaseActionMethod
	{
		public void TestRequiredSecurityCheckpoints()
		{
			AssertContainsExactElementsInAnyOrder(
				s => s.DisplayTextPathToSecurityRight,
				new SecurityCheckpoint[] { Env.Security.ForwardingContainerPRAMessagingAU },
				Method.GetRequiredSecurityCheckpoints());
		}

		public void TestHasControl()
		{
			AssertEquals("Should not have control enabled", false, Method.HasControl);
		}

		public void TestHasSettings()
		{
			AssertEquals("Should have settings enabled", true, Method.HasSettings);
			AssertEquals("Expected Settings Type", typeof(PRASettings), Method.NewSetting(Factory).GetType());
		}

		public void TestRequirements()
		{
			FilterRequirementList requirements = Method.GetFilterRequirements();

			AssertContainsExactElementsInAnyOrder("Constraint Names",
				new string[] { FilterConstants.Country },
				new List<FilterRequirement>(requirements).ConvertAll((r) => r.ConstraintName));

			AssertContainsExactElementsInAnyOrder("Countries",
				new string[] { Constants.CountryCodes.Australia },
				requirements[FilterConstants.Country]);
		}
	}
}
