using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		[ExpectNoExceptions]
		public void TestHouseBill()
		{
			var parent = Factory.New<Bill>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Validation.Bill));
		}

		public void TestCheckDuplicatedHouseBillType()
		{
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "XX1112";
			AssertHasMessageError(houseBill.CU_BillTypeInfo, ValidationConstants.Bill.ErrorOneHouseBillOnly);
		}

		public void TestCheckBillTypeAvailable()
		{
			declaration.Bills.RemoveAndDeleteAll();
			var cnBill = declaration.Bills.AddNew();
			cnBill.CU_BillType = BillTypeList.Codes.ContainerNote;
			AssertNoMessageError(cnBill.CU_BillTypeInfo, ValidationConstants.Bill.ContainerNoteShouldNotExistAtTheSameTime);
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			AssertHasMessageError(houseBill.CU_BillTypeInfo, ValidationConstants.Bill.ContainerNoteShouldNotExistAtTheSameTime);
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			AssertHasMessageError(masterBill.CU_BillTypeInfo, ValidationConstants.Bill.ContainerNoteShouldNotExistAtTheSameTime);
		}

		public void TestCheckContainerNoteMaximumRows()
		{
			declaration.Bills.RemoveAndDeleteAll();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.ContainerNote;
			AssertNoRowMessageError(bill, ValidationConstants.Bill.ContainerNoteMaximumRows);
			for (int i = 0; i < 99; i++)
			{
				bill = declaration.Bills.AddNew();
				bill.CU_BillType = BillTypeList.Codes.ContainerNote;
			}

			AssertHasRowMessageError(bill, ValidationConstants.Bill.ContainerNoteMaximumRows);
		}

		[ExpectNoExceptions]
		public void TestNeedToValidateHouseBillAndPackages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "12345";
			NUnit.Framework.Assert.That(houseBill.CU_BillNumInfo.HasMessageError(Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill), NUnit.Framework.Is.EqualTo(false), "HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill)");
			var packingGroup = houseBill.PackingGroups.AddNew();
			houseBill.Validation.ValidateCU_BillNum();
			NUnit.Framework.Assert.That(houseBill.CU_BillNumInfo.HasMessageError(Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill), NUnit.Framework.Is.EqualTo(false), "HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill)");
			var package = packingGroup.Packages.AddNew();
			NUnit.Framework.Assert.That(houseBill.CU_BillNumInfo.HasMessageError(Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill), NUnit.Framework.Is.EqualTo(false), "HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill)");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "MB2111";
			declaration.JE_HouseBill = "XX1111";
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
		}

		[ExpectNoExceptions]
		public void TestMasterBillValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_MasterBill = "081-1111111";
			NUnit.Framework.Assert.That(declaration.JE_MasterBillInfo.HasMessageErrors(), NUnit.Framework.Is.True, "Bad Masterbill CheckDigit should show error");
			declaration.JE_MasterBill = "081-11111111";
			NUnit.Framework.Assert.That(!declaration.JE_MasterBillInfo.HasMessageErrors(), NUnit.Framework.Is.True, "Good Masterbill CheckDigit should not show error");
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
	}
}
