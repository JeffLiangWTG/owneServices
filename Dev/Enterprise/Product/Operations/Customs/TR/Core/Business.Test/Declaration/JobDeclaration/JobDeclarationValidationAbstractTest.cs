using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationValidation))]
	abstract class JobDeclarationValidationAbstractTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDischargeOffice()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.DischargeOfficeInfo);
		}

		public void TestCheckDischargePlace()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.DischargePlaceInfo);
		}

		public void TestCheckJE_PaymentMethod()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_PaymentMethodInfo, "A", "B");
		}

		public void TestCheckJE_TransportModeInland()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportModeInlandInfo);
		}

		public void TestCheckEntryOffice()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.EntryOfficeInfo);
		}

		public void TestCheckBondedWarehouseCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.BondedWarehouseCodeInfo, "INV", "ABC");
		}

		public void TestCheckJE_ExportGoodsTypeMessageError()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_ExportGoodsTypeInfo);
		}

		public void TestCheckJE_ExportGoodsTypeInvalidCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_ExportGoodsTypeInfo, "A", "9");
		}

		public void TestCheckJE_EntrySubStyle()
		{
			jobDeclaration.JE_EntrySubStyle = "#";
			AssertHasMessageError(jobDeclaration.JE_EntrySubStyleInfo, "The code you have selected is not in the list.");

			jobDeclaration.JE_EntrySubStyle = EntrySubStyleList.Codes._22;
			AssertNoNotifications(jobDeclaration.JE_EntrySubStyleInfo);
		}
		
		public void TestCheckJE_TransportMeans()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(jobDeclaration.JE_TransportMeansInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			validation = GetValidation();

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "Bonded Warehouse Codes List", "TR");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "ABC", "ABC Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
		}
		protected JobDeclaration jobDeclaration;
		protected JobDeclarationValidation validation;

		protected abstract string MessageType { get; }

		protected abstract JobDeclarationValidation GetValidation();
	}
}
