using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PackageValidationTest : Customs.Business.Testing.CusDecHouseContainerPackValidationTest
	{
		public void TestCW_ContainerNo()
		{
			bool autoAllocate = CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			try
			{
				CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.DisableDefaultPackingInformation = true;
				declaration.US_EnableINB = true;

				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = "C";

				var bill = declaration.Bills.AddNew();
				bill.CU_BillNum = "B";

				var package = declaration.Packages.AddNew();
				AssertNoExceptionThrown(() => package.RunPreSaveValidation());

				//want to set container no first
				AssertNoExceptionThrown(() => package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber);
				package.CW_HouseBill = bill.CU_BillUniqueCode;

				package.CW_ContainerNoOrEquipmentNo = ZString.Empty;
				AssertEquals(false, package.CW_ContainerNoOrEquipmentNoInfo.HasMessageErrors());

				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

				package.CW_ContainerNoOrEquipmentNo = ZString.Empty;
				AssertEquals(false, package.CW_ContainerNoOrEquipmentNoInfo.HasMessageErrors());
			}
			finally
			{
				CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, autoAllocate);
			}
		}

		public void TestCheckCW_PackType()
		{
			package.CW_PackType = "~";
			AssertHasWarning(package.CW_PackTypeInfo, ValidationConstants.InvalidPackTypeMessage);
			package.CW_PackType = package.Lookups.PackTypeList[0].Code;
			AssertNoWarning(package.CW_PackTypeInfo, ValidationConstants.InvalidPackTypeMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();

			var mockBill = Factory.NewMoq<Bill>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;

			bill = mockBill.Object;
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			declaration.Bills.Add(bill);
			bill.US_SequenceNo = 1;
			bill.CU_MasterBill = "1";

			package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			AssertNotNull(package.Bill);
		}

		JobDeclaration declaration;
		Bill bill;
		Package package;
	}
}
