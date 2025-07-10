using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	public class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPA_ArrivedQuantity()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_ArrivedQuantity = 0;
			AssertNoNotifications(pack.APA_ArrivedQuantityInfo);
			pack.APA_ArrivedQuantity = 1;
			AssertNoNotifications(pack.APA_ArrivedQuantityInfo);
			pack.APA_ArrivedQuantity = -1;
			AssertHasErrorContaining(pack.APA_ArrivedQuantityInfo, "Arrived Qty cannot be negative.");
			pack.APA_ArrivedWeight = 0;
			pack.APA_ArrivedQuantity = 0;
			AssertNoErrorContaining(pack.APA_ArrivedQuantityInfo, "Arrived Qty cannot be zero.");
			pack.APA_ArrivedWeight = 1;
			AssertHasErrorContaining(pack.APA_ArrivedQuantityInfo, "Arrived Qty cannot be zero.");
		}

		public void TestCheckAPA_ArrivedWeight()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_ArrivedWeight = 0;
			AssertNoNotifications(pack.APA_ArrivedWeightInfo);
			pack.APA_ArrivedWeight = 12345678.111;
			AssertNoNotifications(pack.APA_ArrivedWeightInfo);
			pack.APA_ArrivedWeight = -1;
			AssertHasErrorContaining(pack.APA_ArrivedWeightInfo, "Arrived Weight cannot be negative.");
			pack.APA_ArrivedQuantity = 0;
			pack.APA_ArrivedWeight = 0;
			AssertNoErrorContaining(pack.APA_ArrivedWeightInfo, "Arrived Weight cannot be zero.");
			pack.APA_ArrivedQuantity = 1;
			AssertHasErrorContaining(pack.APA_ArrivedWeightInfo, "Arrived Weight cannot be zero.");
			pack.APA_ArrivedWeight = 123456789.111;
			AssertHasErrors(pack.APA_ArrivedWeightInfo);
		}

		public void TestCheckAPA_Weight()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_Weight = 0;
			AssertHasMessageErrorContaining(pack.APA_WeightInfo, "Weight (on Pack) cannot be zero.");
		}

		public void TestCheckAPA_Volume()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_Volume = 0;
			AssertHasMessageErrorContaining(pack.APA_VolumeInfo, "Volume (on Pack) cannot be zero.");
		}

		public void TestCheckAPA_MarksAndNumbers()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_MarksAndNumbers = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_MarksAndNumbersInfo, "You have not entered a Marks and Numbers (on Pack).");
		}

		public void TestCheckAPA_GoodsDescription()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();

			pack.APA_GoodsDescription = ZString.Empty;
			AssertHasMessageErrorContaining(pack.APA_GoodsDescriptionInfo, "You have not entered a Goods' Description (on Pack).");
		}

		public void TestVolumeUQ()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();
			pack.APA_VolumeUQ = ZString.Empty;

			AssertHasMessageError(pack.APA_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered + " a Volume Unit.");
		}

		public void TestPackUQ()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();
			pack.APA_PackUQ = ZString.Empty;

			AssertHasMessageError(pack.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered + " a Pack Unit.");
		}
	}
}
