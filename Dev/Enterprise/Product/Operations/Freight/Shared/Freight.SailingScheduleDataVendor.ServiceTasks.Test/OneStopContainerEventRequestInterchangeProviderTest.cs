using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingScheduleDataVendor.ServiceTasks.Test
{
	sealed class OneStopContainerEventRequestInterchangeProviderTest : InterchangeProviderTestCase
	{
		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new OneStopContainerEventRequestInterchangeProvider(collection);
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;

			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "ABN";

			Helper.Consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2005, 1, 1);
			Helper.Consol.JK_RL_NKLoadPort = "AUSYD";
			Helper.Consol.JK_RL_NKDischargePort = "NZAKL";
			Helper.Container.JC_ContainerNum = "Container1";
			Helper.Container2.JC_ContainerNum = "Container2";

			var requestList = OneStopContainerEventRequestList.New();
			requestList.FindOrCreateRequest(Helper.Container);
			requestList.FindOrCreateRequest(Helper.Container2);
			requestList.CreateMessages();

			NonDependentEDIMessageCollection collection = new NonDependentEDIMessageCollection(Factory);
			collection.Load(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ComTrac));
			var provider = GetInterchangeProvider(collection);

			AssertEquals(2, provider.Interchanges.Length);
			AssertEquals(string.Empty, provider.Interchanges[0].EI_FooterNText);
			AssertEquals(string.Empty, provider.Interchanges[1].EI_FooterNText);
		}
	}
}
