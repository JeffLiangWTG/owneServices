using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.US.Business.Testing
{
	class MessageTypeBasedValueDefaulterTest : TestCaseWithFactory
	{
		public void TestMergeByDefaultingFromMessageTypeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			var importer = OrgHeader.New(Factory);
			importer.OH_FullName = "Importer";
			importer.OH_RL_NKClosestPort = "Z!XXX"; // non local organisation
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;

			var supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "Supplier";
			supplier.OH_RL_NKClosestPort = "V!XXX"; // non local organisation
			supplier.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.PartNumber;

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals("declaration.JE_MergeBy", OrgConstants.MergeInvoiceLines.PartNumber, declaration.JE_MergeBy);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("declaration.JE_MergeBy", OrgConstants.MergeInvoiceLines.TariffAndDescription, declaration.JE_MergeBy);
		}

		public void TestManufacturerDefaultingFromMessageTypeChanges()
		{
			USCustomsDataRegistry.Instance.DefaultManufacturerFromSupplier.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			var importer = OrgHeader.New(Factory);
			importer.OH_FullName = "Importer";
			importer.OH_RL_NKClosestPort = "Z!XXX"; // non local organisation
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "BBB";

			var supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "Supplier";
			supplier.OH_RL_NKClosestPort = "V!XXX"; // non local organisation
			supplier.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "DDD";

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("declaration.JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals("Manufacturer should not default", ZGuid.Empty, declaration.JE_OA_ManufacturerAddress);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Manufacturer should default to Supplier", supplier.MainAddress.PK, declaration.JE_OA_ManufacturerAddress);
		}

		public void TestCargoReleaseDefaultingFromMessageTypeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_EnableCRL = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("Cargo Release Type", ZString.Empty, declaration.US_CargoReleaseType);
			AssertEquals("Cargo Release Certify", false, declaration.US_CertifyCargoRelease);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals("Cargo Release Type", CargoReleaseTypeList.Codes.BCR, declaration.US_CargoReleaseType);
			AssertEquals("Cargo Release Certify", true, declaration.US_CertifyCargoRelease);

			declaration.US_CargoReleaseType = "";
			declaration.US_CertifyCargoRelease = false;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("Cargo Release Type", ZString.Empty, declaration.US_CargoReleaseType);
			AssertEquals("Cargo Release Certify", false, declaration.US_CertifyCargoRelease);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			AssertEquals("Cargo Release Type", CargoReleaseTypeList.Codes.CR, declaration.US_CargoReleaseType);
			AssertEquals("Cargo Release Certify", false, declaration.US_CertifyCargoRelease);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			AssertEquals("Cargo Release Type", CargoReleaseTypeList.Codes.SE, declaration.US_CargoReleaseType);

			declaration.US_EnableCRL = false;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			AssertEquals("Cargo Release Type", CargoReleaseTypeList.Codes.SE, declaration.US_CargoReleaseType);

			declaration.US_EnableCRL = false;
			AssertEquals("Cargo Release Type", ZString.Empty, declaration.US_CargoReleaseType);

			declaration.US_EnableENS = true;
			AssertEquals("Cargo Release Type", CargoReleaseTypeList.Codes.ACE, declaration.US_CargoReleaseType);

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			AssertEquals("Cargo Release Type", CargoReleaseTypeList.Codes.ACE, declaration.US_CargoReleaseType);
		}

		public void TestDefaultCargoReleaseTypeShouldNotRemoveSplitDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_SESplitRel = SplitShipmentReleaseCodeList.Codes.HoldAll;

			declaration.US_EnableCRL = true;
			AssertEquals(CargoReleaseTypeList.Codes.SE, declaration.US_CargoReleaseType);
			AssertEquals("should not have removed this during the course of defaulting", SplitShipmentReleaseCodeList.Codes.HoldAll, declaration.US_SESplitRel);

			declaration.US_EnableCRL = false;
			AssertEquals(CargoReleaseTypeList.Codes.ACE, declaration.US_CargoReleaseType);
			AssertEquals("should not have removed this during the course of defaulting", SplitShipmentReleaseCodeList.Codes.HoldAll, declaration.US_SESplitRel);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			AssertEquals(CargoReleaseTypeList.Codes.ACE, declaration.US_CargoReleaseType);
			AssertEquals("should not have removed this during the course of defaulting", SplitShipmentReleaseCodeList.Codes.HoldAll, declaration.US_SESplitRel);
		}

		public void TestSupplierClearInvoiceAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "orgCode";
			orgHeader.OH_FullName = "orgName";
			orgHeader.MainAddress.OA_Address1 = "orgAddress";

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "orgCode1";
			orgHeader1.OH_FullName = "orgName1";
			orgHeader1.MainAddress.OA_Address1 = "orgAddress1";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_SupplierAddress = orgHeader.MainAddress.PK;
			Factory.Save();

			declaration.JE_OH_Supplier = orgHeader.PK;
			invoice.JZ_OA_SupplierAddress = Guid.Empty;
			AssertEquals("If the declaration is not created from low value, the address on the invoice is from the address on the declaration.", orgHeader.MainAddress.PK, invoice.JZ_OA_SupplierAddress);

			MarkDeclarationAsFromLowValue(declaration);
			invoice.JZ_OA_SupplierAddress = orgHeader1.MainAddress.PK;
			Factory.Save();

			declaration.JE_OH_Supplier = orgHeader1.PK;
			invoice.JZ_OA_SupplierAddress = Guid.Empty;
			AssertEquals("If the declaration is created from low value, the address on the invoice has no relationship with the address on the declaration.", Guid.Empty, invoice.JZ_OA_SupplierAddress);

			void MarkDeclarationAsFromLowValue(JobDeclaration declaration)
			{
				declaration.Logs.AddNew(AutoEvents.Transferred,
							[
								new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "LV001"),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "USLV")
							]);
			}
		}
	}
}
