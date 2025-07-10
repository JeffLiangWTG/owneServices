using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(USExportAsycudaBillCollectionSynchroniser))]
	sealed class USExportAsycudaBillCollectionSynchroniserTest : ManifestBillCollectionSynchroniserTest
	{
		protected override BusinessObjectCollectionSynchroniser GetManifestBillCollectionSynchroniser(IManifestHeaderForSynchroniser header)
			=> new USExportAsycudaBillCollectionSynchroniser((USExportAsycudaManifestHeader)header);

		protected override IManifestHeaderForSynchroniser GetManifestBillHeader(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = "JK";
			return header;
		}

		public void TestSynchronisersMasterBOL()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			consol.JK_MasterBillNum = "666888";
			Factory.Save();
			header.SetParent(consol);
			header.Synchroniser.SetEnabled(true, false);
			header.Synchroniser.Synchronise();
			AssertEquals("666888", header.MasterBOL);
		}
	}
}
