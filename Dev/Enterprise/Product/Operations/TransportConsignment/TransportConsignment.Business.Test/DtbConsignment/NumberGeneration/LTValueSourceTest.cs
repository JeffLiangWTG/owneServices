using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class LTValueSourceTest : TestCaseWithFactory
	{
		public void TestServiceLevel()
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment("LT001");
			var valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new LTValueSource(consignment));
			consignment.LTC_RS_NKServiceLevel = "D2D";

			AssertEquals("ServiceLevel", "D2D", valueProviders[Keys.ServiceLevel].GetValue(Generator, ""));
		}

		#region Implementation

		NumberGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new NumberGenerator();
					generator.Factory = Factory;
					generator.Context = new NumberGeneratorContext();
				}
				return generator;
			}
		}

		NumberGenerator generator;

		#endregion
	}
}
