using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ConsolExportAWBAccountingInformation))]
	sealed class ConsolExportAWBAccountingInformationTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<ConsolExportAWBAccountingInformation>();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var header = factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			header.ForceSavingByFactory = true;

			var result = factory.NewWithValidTestData<ConsolExportAWBAccountingInformation>();
			result.EA_EH = header.PK;

			return result;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var accountingInformation = Factory.NewWithValidTestData<ConsolExportAWBAccountingInformation>();

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = Factory.NewWithValidTestData<ForwardingConsol>().PK;
			accountingInformation.EA_EH = header.PK;

			return accountingInformation;
		}

		#endregion

	}
}
