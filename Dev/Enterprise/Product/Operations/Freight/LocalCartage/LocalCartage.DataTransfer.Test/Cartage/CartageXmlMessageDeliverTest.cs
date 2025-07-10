using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	public class CartageXmlMessageDeliverTest : TestCaseWithFactory
	{
		public void TestProcess_DoesNotSaveFactoryOnDeliver()
		{
			using (Factory.AddDisposableService())
			{
				var initialSaveCount = Factory.SaveCount;
				var buffer = new NotificationBuffer();
				var dummyParent = new DummyCartageParent(Factory);
				var cartage = Factory.NewWithValidTestData<CommonCartage>();
				var dataAdapter = new CommonCartageStatusValueObjectDataAdapter();
				var interchange = dataAdapter.GetXMLIntechangeWithTargetType(cartage, buffer);
				var cartageType = ((ICartageParent)dummyParent).CartageTypes.First();
				var logs = ((IStmALogParent)cartageType.CartageParent).Logs;
				var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				mode.EK_Destination = "EDIDATDAT";
				var delivery = new CartageXmlMessageDeliver(logs.Parent, new MessageProcessorCommunicationModesResult(new[] { mode }, null), cartage, cartage, dataAdapter, interchange);
				delivery.Process(buffer);
				AssertEquals("Save count after Process", initialSaveCount, Factory.SaveCount);
				Factory.Save();
				AssertEquals("Save count after Save", initialSaveCount + 1, Factory.SaveCount);
			}
		}
	}
}
