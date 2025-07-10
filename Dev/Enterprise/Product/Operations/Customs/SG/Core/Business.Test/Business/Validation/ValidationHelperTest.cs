using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class ValidationHelperTest : TestCaseWithFactory
	{
		public void TestHasUEN()
		{
			AssertEquals(true, ValidationHelper.HasUEN(null));
			OrgHeader organisation = Factory.New<OrgHeader>();
			AssertEquals(false, ValidationHelper.HasUEN(organisation));
			organisation.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "TEST");
			AssertEquals(false, ValidationHelper.HasUEN(organisation));
			organisation.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			AssertEquals(true, ValidationHelper.HasUEN(organisation));
		}

		public void TestNonAdValoremDutyRatedGoods()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BW1", SGCPlaces.Constants.PremiseType.BondedWarehouse, "BW1");
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BWCY1", SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard, "BWCY1");
			Factory.Save();
			Declaration.SG_US_NKPlaceOfCargoRelease = "BW1";
			AssertEquals(true, ValidationHelper.IsNonAdValoremDutyRatedGoods(Declaration));
			Declaration.SG_US_NKPlaceOfCargoRelease = "BWCY1";
			AssertEquals(false, ValidationHelper.IsNonAdValoremDutyRatedGoods(Declaration));
			Declaration.SG_GoodsPreviouslyExemptedFromDuties = true;
			AssertEquals(true, ValidationHelper.IsNonAdValoremDutyRatedGoods(Declaration));
			Declaration.SG_GoodsPreviouslyExemptedFromDuties = false;
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ShortPayment.ShortPaymentInvolvingUpdates;
			AssertEquals(true, ValidationHelper.IsNonAdValoremDutyRatedGoods(Declaration));
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.RecoveryPayment.RecoveryPaymentNotInvolvingUpdates;
			AssertEquals(true, ValidationHelper.IsNonAdValoremDutyRatedGoods(Declaration));
			Declaration.SG_US_NKPlaceOfReceipt = "";
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			AssertEquals(true, ValidationHelper.IsNonAdValoremDutyRatedGoods(Declaration));
		}

		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		#endregion
		#region Validation Helper
		ValidationHelper ValidationHelper
		{
			get
			{
				return validationHelper ?? (validationHelper = new ValidationHelper());
			}
		}

		ValidationHelper validationHelper;
		#endregion
	}
}
