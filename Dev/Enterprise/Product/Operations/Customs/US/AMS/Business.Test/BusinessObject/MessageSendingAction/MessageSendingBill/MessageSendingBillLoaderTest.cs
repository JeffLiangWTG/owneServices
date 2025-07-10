using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(MessageSendingObject.Loader))]
	sealed class MessageSendingBillLoaderTest : LoaderTestCase
	{
		public void TestLoadOrNew()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = "OTT1";
			var moveHeader = header.MovementHeader;
			moveHeader.BM_ManifestSequenceNumber = "000123";
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			var sendingObj = new MessageSendingObject(moveDetail, ActionCode.VesselArrival);

			var loader = new MessageSendingObject.Loader(Factory);

			AssertNotEquals(sendingObj, loader.LoadOrNew(moveDetail2, ActionCode.VesselArrival));
			AssertEquals(sendingObj, loader.LoadOrNew(moveDetail, ActionCode.VesselArrival));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new MessageSendingObject.Loader(Factory);
	}
}
