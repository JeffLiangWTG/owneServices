using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobComInvoiceHeaderFunctionalBaseOnlyTest : TestCaseWithFactory
	{
		public void TestLoadSupplierAndAddress()
		{
			var factory2 = NewFactory();

			var org1 = factory2.New<OrgHeader>();
			var org2 = factory2.New<OrgHeader>();

			org1.OH_Code = "TSTORG01";
			org2.OH_Code = "TSTORG02";

			var addr1 = org1.MainAddress;
			var addr2 = org2.MainAddress;

			var invoice1 = factory2.New<BaseJobComInvoiceHeader>();
			var invoice2 = factory2.New<BaseJobComInvoiceHeader>();
			var invoice3 = factory2.New<BaseJobComInvoiceHeader>();
			var invoice4 = factory2.New<BaseJobComInvoiceHeader>();

			factory2.Save();

			Assert("(pre-condition)", addr1.PK.IsValid);
			Assert("(pre-condition)", addr2.PK.IsValid);
			AssertNotEquals("(pre-condition)", addr1.OA_OH, addr2.OA_OH);

			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_JobComInvoiceHeader_SupplierAddress ON JobComInvoiceHeader");
			TestConnection.ExecuteNonQuery($"update dbo.JobComInvoiceHeader set JZ_OH_Supplier = '{org1.PK}', JZ_OA_SupplierAddress = '{addr1.PK}', JZ_SystemLastEditTimeUtc = GetUtcDate(), JZ_SystemLastEditUser = '~BP' where JZ_PK='{invoice1.PK}'");
			TestConnection.ExecuteNonQuery($"update dbo.JobComInvoiceHeader set JZ_OH_Supplier = NULL, JZ_OA_SupplierAddress = '{addr1.PK}', JZ_SystemLastEditTimeUtc = GetUtcDate(), JZ_SystemLastEditUser = '~BP' where JZ_PK='{invoice2.PK}'");
			TestConnection.ExecuteNonQuery($"update dbo.JobComInvoiceHeader set JZ_OH_Supplier = '{org1.PK}', JZ_OA_SupplierAddress = NULL, JZ_SystemLastEditTimeUtc = GetUtcDate(), JZ_SystemLastEditUser = '~BP' where JZ_PK='{invoice3.PK}'");
			TestConnection.ExecuteNonQuery($"update dbo.JobComInvoiceHeader set JZ_OH_Supplier = '{org1.PK}', JZ_OA_SupplierAddress = '{addr2.PK}', JZ_SystemLastEditTimeUtc = GetUtcDate(), JZ_SystemLastEditUser = '~BP' where JZ_PK='{invoice4.PK}'");
			TestConnection.ExecuteNonQuery("ENABLE TRIGGER TG_JobComInvoiceHeader_SupplierAddress ON JobComInvoiceHeader");

			var invoice1r = (IBusinessObjectInternals)Factory.Load<BaseJobComInvoiceHeader>(invoice1.PK);
			var invoice2r = (IBusinessObjectInternals)Factory.Load<BaseJobComInvoiceHeader>(invoice2.PK);
			var invoice3r = (IBusinessObjectInternals)Factory.Load<BaseJobComInvoiceHeader>(invoice3.PK);
			var invoice4r = (IBusinessObjectInternals)Factory.Load<BaseJobComInvoiceHeader>(invoice4.PK);

			CombineAssertions("loading invoice from database, does not change supplier parameters", () =>
			{
				// note: GetValueFromRowSafely is used because some countries (US) override getters for properties, and reported values that does not match actual row values

				AssertEquals($"invoice1 - OH", org1.PK, new ZGuid(invoice1r.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OH_Supplier, DataRowVersion.Default)));
				AssertEquals($"invoice1 - OA", addr1.PK, new ZGuid(invoice1r.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, DataRowVersion.Default)));

				AssertEquals($"invoice2 - OH", ZGuid.Empty, new ZGuid(invoice2r.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OH_Supplier, DataRowVersion.Default)));
				AssertEquals($"invoice2 - OA", addr1.PK, new ZGuid(invoice2r.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, DataRowVersion.Default)));

				AssertEquals($"invoice3 - OH", org1.PK, new ZGuid(invoice3r.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OH_Supplier, DataRowVersion.Default)));
				AssertEquals($"invoice3 - OA", ZGuid.Empty, new ZGuid(invoice3r.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, DataRowVersion.Default)));

				AssertEquals($"invoice4 - OH", org1.PK, new ZGuid(invoice4r.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OH_Supplier, DataRowVersion.Default)));
				AssertEquals($"invoice4 - OA", addr2.PK, new ZGuid(invoice4r.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, DataRowVersion.Default)));
			});
		}

		public void TestSettingSupplierFromAddress_StandaloneInvoice()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.OH_Code = "TSTORG01";
			org2.OH_Code = "TSTORG02";

			var addr1 = org1.Addresses.AddNew();
			var addr2 = org2.Addresses.AddNew();

			var invoice = Factory.New<BaseJobComInvoiceHeader>();

			// note: GetValueFromRowSafely is used because some countries (US) override getters for properties, and reported values that does not match actual row values
			var invoiceInternals = (IBusinessObjectInternals)invoice;

			invoice.JZ_OA_SupplierAddress = addr1.PK;

			CombineAssertions(() =>
			{
				AssertEquals(addr1.PK, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, DataRowVersion.Default)));
				AssertEquals(org1.PK, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OH_Supplier, DataRowVersion.Default)));

				invoice.JZ_OA_SupplierAddress = addr2.PK;
				AssertEquals(addr2.PK, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, DataRowVersion.Default)));
				AssertEquals(org2.PK, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OH_Supplier, DataRowVersion.Default)));

				invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
				AssertEquals(ZGuid.Empty, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, DataRowVersion.Default)));
				AssertEquals(ZGuid.Empty, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OH_Supplier, DataRowVersion.Default)));
			});
		}

		public void TestSettingSupplierFromAddress_DeclarationInvoice()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.OH_Code = "TSTORG01";
			org2.OH_Code = "TSTORG02";

			var addr1 = org1.Addresses.AddNew();
			var addr2 = org2.Addresses.AddNew();

			AssertNotEquals(org1.MainAddress.PK, addr1.PK);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = org1.PK;

			var invoice = declaration.Invoices.AddNew();

			// note: GetValueFromRowSafely is used because some countries (US) override getters for properties, and reported values that does not match actual row values
			var invoiceInternals = (IBusinessObjectInternals)invoice;

			invoice.JZ_OA_SupplierAddress = addr2.PK;
			CombineAssertions(() =>
			{
				AssertEquals(addr2.PK, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, DataRowVersion.Default)));
				AssertEquals(org2.PK, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OH_Supplier, DataRowVersion.Default)));

				invoice.JZ_OA_SupplierAddress = addr1.PK;
				AssertEquals(addr1.PK, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, DataRowVersion.Default)));
				AssertEquals(org1.PK, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OH_Supplier, DataRowVersion.Default)));

				invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
				AssertEquals(ZGuid.Empty, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, DataRowVersion.Default)));
				AssertEquals(ZGuid.Empty, new ZGuid(invoiceInternals.GetValueFromRowSafely(JobComInvoiceHeaderSchema.JZ_OH_Supplier, DataRowVersion.Default)));
			});
		}

		public void TestGetDefaultDataGroupingCode()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals("No binded Declaration and no binded Branch use Invoice current countrycode", currentCountry, invoice.GetDefaultDataGroupingCode());
			AssertEquals("No binded Declaration and no binded Branch use Invoice current countrycode - Tariff", currentCountry, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
			AssertEquals("No binded Declaration and no binded Branch use Invoice current countrycode - DutyRateCodes", currentCountry, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
			AssertEquals("No binded Declaration and no binded Branch use Invoice current countrycode - CusProcedure", currentCountry, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
			AssertEquals("No binded Declaration and no binded Branch use Invoice current countrycode - AdditionalDocumentCodes", currentCountry, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;

			invoice.JZ_GB = branch.PK;
			AssertEquals("No binded Declaration and has binded Branch use Invoice Branch's countrycode", Core.Constants.CountryCodes.SouthAfrica, invoice.GetDefaultDataGroupingCode());
			AssertEquals("No binded Declaration and has binded Branch use Invoice Branch's countrycode - Tariff", Core.Constants.CountryCodes.SouthAfrica, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
			AssertEquals("No binded Declaration and has binded Branch use Invoice Branch's countrycode- DutyRateCodes", Core.Constants.CountryCodes.SouthAfrica, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
			AssertEquals("No binded Declaration and has binded Branch use Invoice Branch's countrycode- CusProcedure", Core.Constants.CountryCodes.SouthAfrica, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
			AssertEquals("No binded Declaration and has binded Branch use Invoice Branch's countrycode- AdditionalDocumentCodes", Core.Constants.CountryCodes.SouthAfrica, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));

			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(currentCountry, declaration.GetDefaultDataGroupingCode());
			invoice.JZ_JE = declaration.PK;
			AssertEquals("Has binded Declaration and use Declaration default dataGrouping", currentCountry, invoice.GetDefaultDataGroupingCode());
			AssertEquals("Has binded Declaration and use Declaration default dataGrouping - Tariff", currentCountry, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
			AssertEquals("Has binded Declaration and use Declaration default dataGrouping - DutyRateCodes", currentCountry, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
			AssertEquals("Has binded Declaration and use Declaration default dataGrouping - CusProcedure", currentCountry, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
			AssertEquals("Has binded Declaration and use Declaration default dataGrouping - AdditionalDocumentCodes", currentCountry, invoice.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));
		}

		public void TestGetEffectiveConsigeeAddress()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			org1.MainAddress.OA_Address1 = "ORG1 MAIN ADDRESS";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			org2.MainAddress.OA_Address1 = "ORG2 MAIN ADDRESS";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "ORG3";
			org3.MainAddress.OA_Address1 = "ORG3 MAIN ADDRESS";
			var org3SelectedAddress = org3.Addresses.AddNew();
			org3SelectedAddress.OA_Address1 = "ORG3 SELECTED ADDRESS";

			declaration.JE_OH_Importer = org1.PK;
			var inv1 = declaration.Invoices.AddNew();
			inv1.JZ_OH_Buyer = ZGuid.Empty;
			AssertEquals("EffectiveConsigeeAddress", org1.MainAddress, inv1.EffectiveConsigeeAddress);

			inv1.JZ_OH_Buyer = org2.PK;
			AssertEquals("EffectiveConsigeeAddress", org2.MainAddress, inv1.EffectiveConsigeeAddress);

			inv1.JZ_OA_ConsigneeAddress = org3SelectedAddress.PK;
			AssertEquals("EffectiveConsigeeAddress", org3SelectedAddress, inv1.EffectiveConsigeeAddress);
		}
		public void TestGetEffectiveSupplierAddress()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1";
			org1.MainAddress.OA_Address1 = "ORG1 MAIN ADDRESS";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2";
			org2.MainAddress.OA_Address1 = "ORG2 MAIN ADDRESS";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "ORG3";
			org3.MainAddress.OA_Address1 = "ORG3 MAIN ADDRESS";
			var org3SelectedAddress = org3.Addresses.AddNew();
			org3SelectedAddress.OA_Address1 = "ORG3 SELECTED ADDRESS";

			declaration.JE_OH_Supplier = org1.PK;
			var inv1 = declaration.Invoices.AddNew();
			inv1.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("EffectiveeSupplierAddress", org1.MainAddress, inv1.EffectiveSupplierAddress);

			inv1.JZ_OH_Supplier = org2.PK;
			AssertEquals("EffectiveeSupplierAddress", org2.MainAddress, inv1.EffectiveSupplierAddress);

			inv1.JZ_OA_SupplierAddress = org3SelectedAddress.PK;
			AssertEquals("EffectiveeSupplierAddress", org3SelectedAddress, inv1.EffectiveSupplierAddress);
		}

		public void TestChangeIsLinkedOnLinkPackageAndSyncTheValueToContainerPivot()
		{
			var declaration = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();
			declaration.JE_MasterBill = "123";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "ANYTHING";

			var package1 = declaration.Packages.AddNew();
			package1.CW_HouseBill = declaration.JE_MasterBill;
			package1.CW_PackQty = 10;
			package1.CW_PackType = "PK";
			package1.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;

			var package2 = declaration.Packages.AddNew();
			package2.CW_HouseBill = declaration.JE_MasterBill;
			package2.CW_PackQty = 10;
			package2.CW_PackType = "PK";
			package2.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;

			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();

			invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
			invoiceLine3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;

			var invoicePack = new BaseCusLinkPackage(invoice1) { Package = package1, IsLinked = true };

			Assert("Should be true as the package1 is linked on the invoiceLine1.", invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be true as the package1 is linked on the invoiceLine2.", invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be false as the package1 is not linked on the invoiceLine3.", !invoiceLine3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			invoicePack.IsLinked = false;

			Assert("Should be false as the package1 is not linked on the invoiceLine1.", !invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be false as the package1 is not linked on the invoiceLine2.", !invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be false as the package1 is not linked on the invoiceLine3.", !invoiceLine3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			var invoiceLinePack = new BaseCusLinkPackage(invoiceLine2) { Package = package2, IsLinked = true };

			Assert("Should be false as package1 and package2 are not linked on the invoiceLine1.", !invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be true as the package2 is linked on the invoiceLine2.", invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be false as package1 and package2 are not linked on the invoiceLine3.", !invoiceLine3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			invoicePack.IsLinked = true;

			Assert("Should be true as the package1 is linked on the invoiceLine1.", invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be true as package1 and package2 are both linked on the invoiceLine2.", invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be false as package1 and package2 are not linked on the invoiceLine3.", !invoiceLine3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			invoicePack.IsLinked = false;

			Assert("Should be false as package1 and package2 are not linked on the invoiceLine1.", !invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be true as the package2 is linked on the invoiceLine2.", invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be false as package1 and package2 are not linked on the invoiceLine3.", !invoiceLine3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);

			invoiceLinePack = new BaseCusLinkPackage(invoiceLine3) { Package = package2, IsLinked = true };

			Assert("Should be false as package1 and package2 are not linked on the invoiceLine1.", !invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be true as the package2 is linked on the invoiceLine2.", invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
			Assert("Should be true as the package2 is linked on the invoiceLine3.", invoiceLine3.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine);
		}

		public void TestSettingExportDateSetsCurrencyConverterDate()
		{
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_ExportDate = new ZDateTime(2005, 8, 14);
			AssertEquals("DateForRate", new ZDateTime(2005, 8, 14), invoiceHeader.CurrencyConverter.DateForRate);
			declaration.JE_ExportDate = new ZDateTime(2005, 8, 15);
			AssertEquals("DateForRate", new ZDateTime(2005, 8, 15), invoiceHeader.CurrencyConverter.DateForRate);
		}

		public void TestSettingValuationDateOverrideSetsCurrencyConverterDate()
		{
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_ExportDate = new ZDateTime(2005, 8, 14);
			AssertEquals("DateForRate", new ZDateTime(2005, 8, 14), invoiceHeader.CurrencyConverter.DateForRate);
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2005, 8, 15);
			AssertEquals("DateForRate", new ZDateTime(2005, 8, 15), invoiceHeader.CurrencyConverter.DateForRate);
		}

		[TestDate(2020, 10, 7, 2, 15, 0)]
		public void TestInvoiceHeaderAuditColumns()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";

			Factory.Save();

			var createdTimeUtc = Env.Time.GetUtcFromLocalTime(new DateTime(2020, 10, 7, 2, 15, 0));
			var headerPK = ZGuid.Empty;
			using (Env.SetTemporaryUserContext(new UserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartment.PK)))
			{
				var header = Factory.New<BaseJobDeclaration>().JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				headerPK = header.PK;
				Factory.Save();
				AssertEquals("Newly created header JZ_SystemCreateTimeUtc", createdTimeUtc, header.JZ_SystemCreateTimeUtc.ToDateTime());
				AssertEquals("Newly created header JZ_SystemCreateUser", staff1.GS_Code, header.JZ_SystemCreateUser);
				AssertEquals("Newly created header JZ_SystemLastEditTimeUtc", createdTimeUtc, header.JZ_SystemLastEditTimeUtc.ToDateTime());
				AssertEquals("Newly created header JZ_SystemLastEditUser", staff1.GS_Code, header.JZ_SystemLastEditUser);
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			var updatedTimeUtc = createdTimeUtc.AddHours(1);
			using (Env.SetTemporaryUserContext(new UserContext(staff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartment.PK)))
			{
				var header = Factory.Load<BaseJobComInvoiceHeader>(headerPK);
				header.JZ_Volume = 10;
				Factory.Save();
				AssertEquals("Updated header JZ_SystemCreateTimeUtc", createdTimeUtc, header.JZ_SystemCreateTimeUtc.ToDateTime());
				AssertEquals("Updated header JZ_SystemCreateUser", staff1.GS_Code, header.JZ_SystemCreateUser);
				AssertEquals("Updated header JZ_SystemLastEditTimeUtc", updatedTimeUtc, header.JZ_SystemLastEditTimeUtc.ToDateTime());
				AssertEquals("Updated header JZ_SystemLastEditUser", staff2.GS_Code, header.JZ_SystemLastEditUser);
			}
		}

		BaseJobDeclaration declaration;
		BaseJobComInvoiceGroupHeader groupHeader;

		BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<BaseJobDeclaration>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetNewDeclaration();
			groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			_ = groupHeader.JobComInvoiceHeaders.AddNew();
		}
	}
}
