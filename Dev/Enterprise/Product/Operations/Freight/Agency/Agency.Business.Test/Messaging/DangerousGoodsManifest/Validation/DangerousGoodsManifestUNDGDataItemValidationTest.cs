using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Test
{
	public class DangerousGoodsManifestUNDGDataItemValidationTest : BaseAgencyTest
	{
		public void TestDI_DGFlashPoint()
		{
			const string message = "Dangerous Goods Shipment flash point is required for all class 3 and Sub Label 3 cargo with a UNDG Number.";

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "0001";
			substance.DG_UniqueRecordId = "51";
			substance.DG_Standard = "IAT";
			substance.DG_Class = "3";

			UNDGData.DI_DG = substance.PK;
			UNDGData.DI_DGFlashPoint = 2;
			UNDGData.Validation.ValidateDI_DGFlashPoint();
			AssertNoMessageError(UNDGData.DI_DGFlashPointInfo, message);

			UNDGData.DI_DGFlashPoint = 0;
			UNDGData.Validation.ValidateDI_DGFlashPoint();
			AssertHasMessageError(UNDGData.DI_DGFlashPointInfo, message);

			substance.DG_Class = "2";
			UNDGData.Validation.ValidateDI_DGFlashPoint();
			AssertNoMessageError(UNDGData.DI_DGFlashPointInfo, message);

			substance.DG_SubLabel1 = "3";
			UNDGData.Validation.ValidateDI_DGFlashPoint();
			AssertHasMessageError(UNDGData.DI_DGFlashPointInfo, message);

			UNDGData.DI_DGFlashPoint = 2;
			UNDGData.Validation.ValidateDI_DGFlashPoint();
			AssertNoMessageError(UNDGData.DI_DGFlashPointInfo, message);
		}

		public void TestSubstancePK()
		{
			const string message = "Dangerous Goods UNDG Code is required.";

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "0001";
			substance.DG_UniqueRecordId = "51";
			substance.DG_Standard = "IAT";

			var substance2 = Factory.New<UNDGSubstance>();
			substance2.DG_UNNO = ZString.Empty;
			substance2.DG_UniqueRecordId = "52";
			substance2.DG_Standard = "IAT";

			UNDGData.SubstancePK = substance.PK;
			UNDGData.Validation.ValidateSubstancePK();
			AssertNoMessageError(UNDGData.SubstancePKInfo, message);

			UNDGData.SubstancePK = substance2.PK;
			UNDGData.Validation.ValidateSubstancePK();
			AssertHasMessageError(UNDGData.SubstancePKInfo, message);
		}

		public void TestDI_DG_ClassForBinding()
		{
			const string message = "Dangerous Goods IMO Class is required.";

			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_Class = "Test";

			UNDGData.DI_DG = substance.PK;
			UNDGData.Validation.ValidateDI_DG_ClassForBinding();
			AssertNoMessageError(UNDGData.DI_DG_ClassForBindingInfo, message);

			UNDGData.DI_DG = ZGuid.Empty;
			UNDGData.Validation.ValidateDI_DG_ClassForBinding();
			AssertHasMessageError(UNDGData.DI_DG_ClassForBindingInfo, message);
		}

		public void TestDI_PackageCount()
		{
			const string message = "Dangerous Goods package count is required.";

			UNDGData.DI_PackageCount = 2;
			UNDGData.Validation.ValidateDI_PackageCount();
			AssertNoMessageError(UNDGData.DI_PackageCountInfo, message);

			UNDGData.DI_PackageCount = 0;
			UNDGData.Validation.ValidateDI_PackageCount();
			AssertHasMessageError(UNDGData.DI_PackageCountInfo, message);
		}

		public void TestDI_DGWeight()
		{
			const string message = "Dangerous Goods weight is required.";

			var data = PackLine.UNDGs.First();

			data.DI_DGWeight = 0;
			data.Validation.ValidateDI_DGWeight();
			AssertNoMessageError(UNDGData.DI_DGWeightInfo, message);

			UNDGData.DI_DGWeight = 2;
			UNDGData.Validation.ValidateDI_DGWeight();
			AssertNoMessageError(UNDGData.DI_DGWeightInfo, message);

			UNDGData.DI_DGWeight = 0;
			UNDGData.Validation.ValidateDI_DGWeight();
			AssertHasMessageError(UNDGData.DI_DGWeightInfo, message);
		}

		public void TestDI_TechnicalName()
		{
			const string message = "Technical Name is required for Dangerous Goods code 0001";
			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_UNNO = "0001";
			UNDGData.DI_DG = substance.PK;

			UNDGData.DI_TechnicalName = "Test";
			UNDGData.Validation.ValidateDI_TechnicalName();
			AssertNoMessageError(UNDGData.DI_TechnicalNameInfo, message);

			UNDGData.DI_TechnicalName = ZString.Empty;
			UNDGData.Validation.ValidateDI_TechnicalName();
			AssertHasMessageError(UNDGData.DI_TechnicalNameInfo, message);
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

		UNDGDataItem UNDGData
		{
			get { return undgData ?? (undgData = PackLine.UNDGs.AddNew()); }
		}
		UNDGDataItem undgData;

		protected override void SetUp()
		{
			base.SetUp();
			DangerousGoodsManifestMessageValidationStrategy.RegisterForFactory(Factory);

			PackLine.UNDGs.AddNew();
		}

		#endregion
	}
}
