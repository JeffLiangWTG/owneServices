using CargoWise.Types;
using Enterprise.Freight.Agency.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Test
{
	public class DangerousGoodsManifestPackLineValidationTest : BaseAgencyTest
	{
		public void TestJL_DetailedDescription()
		{
			const string message = "Detailed Description is required.";

			PackLine.JL_DetailedDescription = "Fread";
			AssertNoMessageError(PackLine.JL_DetailedDescriptionInfo, message);

			PackLine.JL_DetailedDescription = "";
			AssertHasMessageError(PackLine.JL_DetailedDescriptionInfo, message);
		}

		public void TestJL_PackageCount()
		{
			const string message = "Number of packages are required.";

			PackLine.JL_PackageCount = 1;
			AssertNoMessageError(PackLine.JL_PackageCountInfo, message);

			PackLine.JL_PackageCount = 0;
			AssertHasMessageError(PackLine.JL_PackageCountInfo, message);

			PackLine.UNDGs.AddNew();
			PackLine.Validation.ValidateJL_PackageCount();
			AssertNoMessageError(PackLine.JL_ActualWeightInfo, message);
		}

		public void TestJL_F3_NKPackType()
		{
			const string message = "Package Type is required.";

			PackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			AssertNoMessageError(PackLine.JL_F3_NKPackTypeInfo, message);

			PackLine.JL_F3_NKPackType = ZString.Empty;
			AssertHasMessageError(PackLine.JL_F3_NKPackTypeInfo, message);
		}

		public void TestJL_ActualWeight()
		{
			const string message = "Weight is required.";

			PackLine.JL_ActualWeight = 1;
			AssertNoMessageError(PackLine.JL_ActualWeightInfo, message);

			PackLine.JL_ActualWeight = 0;
			AssertHasMessageError(PackLine.JL_ActualWeightInfo, message);
		}

		#region Implementation

		BillOfLading Shipment
		{
			get { return shipment ?? (shipment = Factory.New<BillOfLading>()); }
		}
		BillOfLading shipment;

		BillOfLadingPackLine PackLine
		{
			get { return packline ?? (packline = Shipment.OuterPackLines.AddNew()); }
		}
		BillOfLadingPackLine packline;

		protected override void SetUp()
		{
			base.SetUp();
			DangerousGoodsManifestMessageValidationStrategy.RegisterForFactory(Factory);

			PackLine.UNDGs.AddNew();
		}

		#endregion
	}
}
