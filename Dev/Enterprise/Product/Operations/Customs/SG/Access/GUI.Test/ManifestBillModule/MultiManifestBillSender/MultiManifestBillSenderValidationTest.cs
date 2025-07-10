using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.Access.Business;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	class MultiManifestBillSenderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCycleDate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var sender = new MultiManifestBillSender(new[] { bill1, bill2 });
			sender.CycleDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(sender.CycleDateInfo, MandatoryValidation.YouHaveNotEntered);
			sender.CycleDate = ZDateTime.Today.AddDays(-1);
			AssertNoMessageErrorContaining(sender.CycleDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(sender.CycleDateInfo, "Cycle Date can not be in the past.");
			sender.CycleDate = ZDateTime.Today.AddDays(1);
			AssertNoMessageErrors(sender.CycleDateInfo);
		}

		public void TestCheckCycleNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var sender = new MultiManifestBillSender(new[] { bill1, bill2 });
			sender.CycleNumber = ZString.Empty;
			AssertHasMessageErrorContaining(sender.CycleNumberInfo, MandatoryValidation.YouHaveNotEntered);
			sender.CycleNumber = "XXX";
			AssertHasMessageError(sender.CycleNumberInfo, ListValidation.InvalidCodeMessageError);
			sender.CycleNumber = "9999";
			AssertHasMessageError(sender.CycleNumberInfo, ListValidation.InvalidCodeMessageError);
			sender.CycleNumber = "1";
			AssertNoNotifications(sender.CycleNumberInfo);
		}
	}
}
