using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using ISelectionItem = Enterprise.Customs.ASYCUDA.Business.ISelectionItem;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class MessageChooserValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCycleDate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			chooser.CycleDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(chooser.CycleDateInfo, MandatoryValidation.YouHaveNotEntered);
			chooser.CycleDate = new ZDateTime(2017, 9, 1);
			AssertNoMessageErrorContaining(chooser.CycleDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCycleNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var chooser = new MessageChooser(header, new ISelectionItem[] { bill1, bill2 }, true);
			chooser.CycleNumber = ZString.Empty;
			AssertHasMessageErrorContaining(chooser.CycleNumberInfo, MandatoryValidation.YouHaveNotEntered);
			chooser.CycleNumber = "XXX";
			AssertHasMessageError(chooser.CycleNumberInfo, ListValidation.InvalidCodeMessageError);
			chooser.CycleNumber = "9999";
			AssertHasMessageError(chooser.CycleNumberInfo, ListValidation.InvalidCodeMessageError);
			chooser.CycleNumber = "1";
			AssertNoNotifications(chooser.CycleNumberInfo);
		}
	}
}
