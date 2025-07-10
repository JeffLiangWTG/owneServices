using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class AllocationMethodDefaultRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAllocationMethod()
		{
			Rule.AllocationMethod = "XXX";
			AssertHasError(Rule.AllocationMethodInfo, "Enter a valid Allocation Method.");

			Rule.AllocationMethod = AllocationMethodList.Codes.Country;
			AssertNoNotifications(Rule.AllocationMethodInfo);

			Rule.AllocationMethod = "";
			AssertHasError(Rule.AllocationMethodInfo, "Please enter an Allocation Method.");
		}

		public void TestCountryCode()
		{
			Rule.CountryCode = "XX";
			AssertHasError(Rule.CountryCodeInfo, "Enter a valid Country.");

			Rule.CountryCode = "AU";
			AssertNoNotifications(Rule.CountryCodeInfo);

			Rule.CountryCode = "";
			AssertHasError(Rule.CountryCodeInfo, "Please enter a Country.");
		}

		#region Implementation

		AllocationMethodDefaultHeader Header
		{
			get
			{
				if (header == null)
				{
					header = new AllocationMethodDefaultHeader(Factory);
				}
				return header;
			}
		}
		AllocationMethodDefaultHeader header;

		AllocationMethodDefaultRule Rule
		{
			get
			{
				if (rule == null)
				{
					rule = Header.Rules.AddNew();
				}
				return rule;
			}
		}
		AllocationMethodDefaultRule rule;

		#endregion
	}
}
