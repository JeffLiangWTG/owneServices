using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntrySummary7501Print))]
	sealed class EntrySummary7501PrintTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldPrintBrokerSignature()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ehp.ShouldPrintBrokerSignature);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ehp.ShouldPrintBrokerSignature);
		}

		public void TestBrokerSignatureImage()
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertNull(ehp.BrokerSignatureImage);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertNull(ehp.BrokerSignatureImage);

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "OOO";
			Declaration.JE_GS_NKCusAgent = broker.GS_Code;

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertNull(ehp.BrokerSignatureImage);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertNull(ehp.BrokerSignatureImage);

			broker.SignatureImage = new Bitmap(1, 2);
			AssertNotNull(ehp.BrokerSignatureImage);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertNull(ehp.BrokerSignatureImage);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertNull(ehp.BrokerSignatureImage);

			GlbStaff.CurrentUser.SignatureImage = new Bitmap(1, 2);
			AssertNotNull(ehp.BrokerSignatureImage);
		}

		public void TestBrokerFileNo()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			ZString expectedResult = Declaration.JE_DeclarationReference;
			AssertEquals("Broker File No (Declaration reference)", expectedResult, ehp.BrokerFileNo);

			Declaration.JE_OwnerRef = "MWB-00395/15";
			expectedResult = declaration.JE_DeclarationReference + " / Ref: MWB-00395/15";
			AssertEquals("Broker File No (with Owner reference)", expectedResult, ehp.BrokerFileNo);

			DataRegistry.Business.USCustomsDataRegistry.Instance.DefaultOwnerRefOn7501.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			expectedResult = declaration.JE_DeclarationReference;
			AssertEquals("Broker File No should exclude Owner reference", expectedResult, ehp.BrokerFileNo);
		}

		public void TestTIBStatement()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB statement from Registry should not print for this entry type", "", ehp.TIBStatement);

			string expectedResult = USCustomsDataRegistry.Instance.TIBStatement.GetFallBackValueAtAllLevels(entry.Declaration.Branch.GB_GC.ToGuid(), entry.Declaration.Branch.PK.ToGuid(), Guid.Empty);
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Declaration.US_TIBMotorVehicles = YesNoList.Codes.No;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB default statement from Registry should print for Temporary Import Bond entry type", expectedResult, ehp.TIBStatement);

			USCustomsDataRegistry.Instance.TIBStatement.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "Changed Statement Value");
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Updated TIB statement Registry value should print for Temporary Import Bond entry type", "Changed Statement Value", ehp.TIBStatement);

			var expectedResultMV = USCustomsDataRegistry.Instance.TIBStatementForMV.GetFallBackValueAtAllLevels(entry.Declaration.Branch.GB_GC.ToGuid(), entry.Declaration.Branch.PK.ToGuid(), Guid.Empty);
			USCustomsDataRegistry.Instance.TIBStatementForMV.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "Statement Value For MV");
			Declaration.US_TIBMotorVehicles = YesNoList.Codes.Yes;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Updated TIB statement Registry For MV value should print", "Statement Value For MV", ehp.TIBStatement);
		}

		public void TestTIBPurpose()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_TIBPurpose = "for something";
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("for something", ehp.TIBPurpose);
		}

		public void TestMessageToPrintOn7501()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("MessageToPrintOn7501 should be blank", "", ehp.MessageToPrintOn7501);

			StmNote messageToPrintNote = declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOn7501.Description, "Dept. of Defense - Duty free claimed/r/nPursuant to Schedule 8 of Tariff ACT");
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("MessageToPrintOn7501 should now return value from note", "Dept. of Defense - Duty free claimed/r/nPursuant to Schedule 8 of Tariff ACT", ehp.MessageToPrintOn7501);
		}

		public void TestBondedAmountWithdrawalStatement()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("BondedAmountWithdrawalStatement should be blank", "", ehp.BondedAmountWithdrawalStatement);

			Declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			Declaration.US_QtyInWHBeforeWithdrawal = 500m;
			Declaration.US_QtyBeingWithdrawn = 75.95m;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("BondedAmountWithdrawalStatement", "BONDED AMOUNT 500    WITHDRAWAL 75.95    BALANCE 424.05", ehp.BondedAmountWithdrawalStatement);
		}

		public void TestTIBTotalOther()
		{
			CreateTIBWithAddCVDFees();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals(197.48m, ehp.TotalAntidumpingDutyAmountPayable);
			AssertEquals(40.98m, ehp.TotalCountervailingDutyPayable);
			AssertEquals("TIB TotalOther should not contain ADD and CVD fees", 0m, ehp.TotalOther);
		}

		[TestDate(2008, 08, 21)]
		public void TestTIBTotalDuty()
		{
			CreateTIBDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB calculated duty for entry summary printing", 697.60m, ehp.TIBTotalDuty);
		}

		[TestDate(2008, 08, 21)]
		public void TestTIBTotalCharges()
		{
			CreateTIBDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Should be charges calculated even when duty is present for an entry", 50.52m, ehp.TIBTotalCharges);
			AssertEquals("TIB Charges Desc", "(MPF: 50.52)", ehp.TIBTotalChargesDesc);

			Declaration.InvoiceLines[0].JI_LinePrice = 1000m;
			Declaration.InvoiceLines[0].US_SPI = "AU";
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("No duty - TIB minimum bond charge for entry summary printing", 100m, ehp.TIBBondChg);
		}

		[TestDate(2008, 08, 21)]
		public void TestTIBTotalChargesWithUS_BondType()
		{
			CreateTIBDeclaration();
			Declaration.US_TIBMotorVehicles = YesNoList.Codes.Yes;
			Declaration.US_TIBMVNonConforming = true;
			Declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Bond Charge BondType8 = CustomsValue 24055 * 3", 72165m, ehp.TIBBondChg);

			Declaration.US_TIBMVNonConforming = false;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Bond Charge BondType8", 1496.24m, ehp.TIBBondChg);

			Declaration.US_TIBMVNonConforming = false;
			Declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Bond Charge BondType9", 1497.00m, ehp.TIBBondChg); //Declaration.US_BondAmount has DecimalPoint(0) attribute.

			Declaration.US_TIBMVNonConforming = true;
			Declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Bond Charge BondType9 = CustomsValue 24055 * 3", 72165m, ehp.TIBBondChg);
		}

		public void TestExpirationDate()
		{
			var expirationDateType = Factory.New<RefSysConfigType>();
			expirationDateType.ZRT_ConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.CBP7501ED;
			expirationDateType.ZRT_Description = "Expiration Statement to print on CBP Form 7501";
			expirationDateType.ZRT_LongDescription = "Expiration Statement to print on CBP Form 7501";
			var expirationDate = Factory.New<RefSysConfig>();
			expirationDate.ZRC_ZRT_NKConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.CBP7501ED;
			expirationDate.ZRC_DecimalValue = 0;
			expirationDate.ZRC_StringValue = "EXPIRATION DATE 01/31/2021";
			expirationDate.ZRC_StartDate = new ZDateTime(2021, 01, 31);
			expirationDate.ZRC_EndDate = new ZDateTime(2079, 06, 06);
			Factory.Save();

			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("ExpirationDate", "EXPIRATION DATE 01/31/2021", ehp.ExpirationDate);
		}

		public void TestRevisionDate()
		{
			var revisionDateType = Factory.New<RefSysConfigType>();
			revisionDateType.ZRT_ConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.CBP7501RD;
			revisionDateType.ZRT_Description = "Revision Statement to print on CBP Form 7501";
			revisionDateType.ZRT_LongDescription = "Revision Statement to print on CBP Form 7501";
			var revisionDate = Factory.New<RefSysConfig>();
			revisionDate.ZRC_ZRT_NKConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.CBP7501RD;
			revisionDate.ZRC_DecimalValue = 0;
			revisionDate.ZRC_StringValue = "(12/19)";
			revisionDate.ZRC_StartDate = new ZDateTime(2019, 12, 01);
			revisionDate.ZRC_EndDate = new ZDateTime(2079, 06, 06);
			Factory.Save();

			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("RevisionDate", "(12/19)", ehp.RevisionDate);
		}

		public void TestApprovalNumber()
		{
			var revisionDateType = Factory.New<RefSysConfigType>();
			revisionDateType.ZRT_ConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.CBP7501AN;
			revisionDateType.ZRT_Description = "Approval Statement to print on CBP Form 7501";
			revisionDateType.ZRT_LongDescription = "Approval Statement to print on CBP Form 7501";
			var revisionDate = Factory.New<RefSysConfig>();
			revisionDate.ZRC_ZRT_NKConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.CBP7501AN;
			revisionDate.ZRC_DecimalValue = 0;
			revisionDate.ZRC_StringValue = "1651-0022";
			revisionDate.ZRC_StartDate = new ZDateTime(2019, 12, 01);
			revisionDate.ZRC_EndDate = new ZDateTime(2079, 06, 06);
			Factory.Save();

			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("ApprovalNumber", "1651-0022", ehp.ApprovalNumber);
		}

		[TestDate(2008, 08, 21)]
		public void TestTIBBondChg()
		{
			CreateTIBDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Duty", 697.60m, ehp.TIBTotalDuty);
			AssertEquals("TIB Charges", 50.52m, ehp.TIBTotalCharges);
			AssertEquals("TIB Charges Desc", "(MPF: 50.52)", ehp.TIBTotalChargesDesc);
			AssertEquals("TIB Total Due Customs", 748.12m, ehp.TIBTotal);
			AssertEquals("TIB Bond CHG for entry summary printing", 1496.24m, ehp.TIBBondChg);

			Declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			Declaration.US_BondAmount = 1750m;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Bond CHG for entry summary printing should print Manual entered value", 1750m, ehp.TIBBondChg);
		}

		[TestDate(2009, 04, 07)]
		public void TestTIBBondChgForExeptionTariffs()
		{
			CreateTIBWithExemptTariffsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Duty", 312m, ehp.TIBTotalDuty);
			AssertEquals("TIB Charges", 25m, ehp.TIBTotalCharges);
			AssertEquals("TIB Charges Desc", "(MPF - Minimum: 25.00)", ehp.TIBTotalChargesDesc);
			AssertEquals("TIB Total Due Customs", 337m, ehp.TIBTotal);
			AssertEquals("TIB Bond CHG for exception tariffs should only be 110% of est duties and charges", 370.70m, ehp.TIBBondChg);
		}

		[TestDate(2008, 08, 21)]
		public void TestTIBBondChgForCombinedTariffs()
		{
			CreateTIBWithBothExemptAndStandardTariffsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Duty", 1009.6m, ehp.TIBTotalDuty);
			AssertEquals("TIB Charges", 68.29m, ehp.TIBTotalCharges);
			AssertEquals("TIB Charges Desc", "(MPF: 68.29)", ehp.TIBTotalChargesDesc);
			AssertEquals("TIB Total Due Customs", 1077.89m, ehp.TIBTotal);
			AssertEquals("TIB Bond CHG should be 110% for exception tariffs and twice standard tariffs (of est duties) and charges", 2155.78m, ehp.TIBBondChg);
		}

		public void TestTIBStatementForMV()
		{
			CreateTIBMotorVehicleDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB statement from Registry should not print for this entry type", "", ehp.TIBStatement);

			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_TIBMotorVehicles = YesNoList.Codes.No;

			var expectedStatement = USCustomsDataRegistry.Instance.TIBStatement.GetFallBackValueAtAllLevels(entry.Declaration.Branch.GB_GC.ToGuid(), entry.Declaration.Branch.PK.ToGuid(), Guid.Empty);
			var expectedResultForMV = USCustomsDataRegistry.Instance.TIBStatementForMV.GetFallBackValueAtAllLevels(entry.Declaration.Branch.GB_GC.ToGuid(), entry.Declaration.Branch.PK.ToGuid(), Guid.Empty);
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Motor Vehicle default statement from RegistryForMV should print for Temporary Import Bond entry type", expectedStatement, ehp.TIBStatement);

			Declaration.US_TIBMotorVehicles = YesNoList.Codes.Yes;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Motor Vehicle default statement from Registry should print for Temporary Import Bond entry type", expectedResultForMV, ehp.TIBStatement);

			Declaration.US_TIBMotorVehicles = YesNoList.Codes.Yes;
			USCustomsDataRegistry.Instance.TIBStatementForMV.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "MV Changed Statement Value - SIX Months");
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Updated TIB statement Registry value should print for Temporary Import Bond entry type", "MV Changed Statement Value - SIX Months", ehp.TIBStatement);
		}

		public void TestDeclarationCheckBoxes()
		{
			CreateTIBDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			Declaration.US_7501Agent = true;
			Declaration.US_7501Purchased = "N";
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("CheckBox1", ehp.DeclarationCheckBox1, "");
			AssertEquals("CheckBox2", ehp.DeclarationCheckBox2, "X");
			AssertEquals("CheckBox3", ehp.DeclarationCheckBox3, "");
			AssertEquals("CheckBox4", ehp.DeclarationCheckBox4, "X");

			Declaration.US_7501Agent = false;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("CheckBox1 - with company proxy", ehp.DeclarationCheckBox1, "X");
			AssertEquals("CheckBox2 - with company proxy", ehp.DeclarationCheckBox2, "");
			AssertEquals("CheckBox3", ehp.DeclarationCheckBox3, "");
			AssertEquals("CheckBox4", ehp.DeclarationCheckBox4, "X");

			Declaration.US_7501Purchased = "Y";
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("CheckBox1 - with branch proxy", ehp.DeclarationCheckBox1, "X");
			AssertEquals("CheckBox2 - with branch proxy", ehp.DeclarationCheckBox2, "");
			AssertEquals("CheckBox3 - importer purchased", ehp.DeclarationCheckBox3, "X");
			AssertEquals("CheckBox4 - importer purchased", ehp.DeclarationCheckBox4, "");

			Declaration.US_7501Purchased = "";
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("CheckBox3 & 4 should not be checked if not entered on declaration", ehp.DeclarationCheckBox3, "");
			AssertEquals("CheckBox3 & 4 should not be checked if not entered on declaration", ehp.DeclarationCheckBox4, "");
		}

		public void TestBoxNumber()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("", ehp.BoxNumber);

			var boxNoCollection = USCustomsDataRegistry.Instance.BoxNumbers.GetValueWithoutFallback(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty);
			BoxNumber boxNo = boxNoCollection.AddNew();
			BoxNumber boxNoDetails1 = boxNoCollection[0];
			boxNoDetails1.TransportMode = BoxNoTransportModeList.Codes.ALL;
			boxNoDetails1.BoxNo = "756";
			USCustomsDataRegistry.Instance.BoxNumbers.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, boxNoCollection);
			AssertEquals("BOX 756", ehp.BoxNumber);

			Declaration.US_EntryMode = EntryModeList.Codes.RLF;
			AssertEquals("", ehp.BoxNumber);
		}

		[TestDate(2009, 04, 08)]
		public void TestWatchEntryAssembledWithUSComponents()
		{
			//Admin EXAMPLE NUMBER (1)
			CreateWatchDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Entry should have pro-rated summary", true, ehp.EntryPrintLines[0].ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", ehp.EntryPrintLines[0].ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", ehp.EntryPrintLines[0].ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", ehp.EntryPrintLines[0].ProRatedLine3);
			AssertEquals("Box 37 Total Duty should remain as actual Duty", 537.25m, ehp.TotalDutyAmt);

			AssertEquals("Secondary Line 1 Actual Duty", 296.88m, ehp.EntryPrintLines[0].secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, ehp.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", ehp.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 3 Actual Duty", 106.03m, ehp.EntryPrintLines[0].secondaryTariffLine3.DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, ehp.EntryPrintLines[0].SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", ehp.EntryPrintLines[0].SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 5 Actual Duty", 127.05m, ehp.EntryPrintLines[0].secondaryTariffLine5.DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.3m, ehp.EntryPrintLines[0].SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", ehp.EntryPrintLines[0].SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 7 Actual Duty", 7.29m, ehp.EntryPrintLines[0].secondaryTariffLine7.DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, ehp.EntryPrintLines[0].SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", ehp.EntryPrintLines[0].SecondaryLine7DutyPercentAsString);

			AssertEquals("TotalDutyAmt", 537.25m, ehp.TotalDutyAmt);
			AssertEquals("TotalEstTax", 0m, ehp.TotalEstTax);
			AssertEquals("TotalOther", 25m, ehp.TotalOther);
			AssertEquals("Block40Total", 562.25m, ehp.Block40Total);
		}

		[TestDate(2009, 04, 08)]
		public void TestWatchEntryPrintedFromMessageAndHeader()
		{
			CreateWatchDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               217                  10A2818            91-013199000                 0         GAZ 0000078801        20                         402818033109B00001092            650  033109         22            08109384852                         00000000                      30                                  0                                   QF      40001TH00000018520000000000                                                     50 9802008068                                                       TH033109N   51                                                                              60                                                                              62          49900001335                                                         709102111010 0000029688000000100000NO                               0000003406  809802008068                                                        0000001010  819102111020 0000010603000000100000NO                               0000001609  819802008068                                                                    819102111030 0000012705000000100000NO                               0000001345  819802008068                                                        0000000204  819102111040 0000000729000000100000NO                                           8949900000002500                                                                9000000053725           0                       0000000250000000009426          Y      GAZEI00018000000053725";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);

			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			AssertEquals("Parent 1 Tariff number", "9802.00.8068", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", printBO.EntryPrintLines[0].SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", printBO.EntryPrintLines[0].SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", printBO.EntryPrintLines[0].SecondaryLine7FormattedTariff);

			AssertEquals("Entry should have pro-rated summary", true, printBO.EntryPrintLines[0].ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", printBO.EntryPrintLines[0].ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", printBO.EntryPrintLines[0].ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", printBO.EntryPrintLines[0].ProRatedLine3);
			AssertEquals("Box 37 Total Duty should remain as actual Duty", 537.25m, printBO.TotalDutyAmt);

			AssertEquals("Secondary Line 1 Actual Duty", 296.88m, entry.MergedLines[0].ChildLines[0].DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 3 Actual Duty", 106.03m, entry.MergedLines[0].ChildLines[2].DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, printBO.EntryPrintLines[0].SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 5 Actual Duty", 127.05m, entry.MergedLines[0].ChildLines[4].DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.3m, printBO.EntryPrintLines[0].SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 7 Actual Duty", 7.29m, entry.MergedLines[0].ChildLines[6].DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, printBO.EntryPrintLines[0].SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine7DutyPercentAsString);
		}

		[TestDate(2009, 04, 08)]
		public void TestWatchEntryPrintedFromMessageAndSnapshot()
		{
			CreateWatchDeclaration();
			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               217                  10A2818            91-013199000                 0         GAZ 0000078801        20                         402818033109B00001092            650  033109         22            08109384852                         00000000                      30                                  0                                   QF      40001TH00000018520000000000                                                     50 9802008068                                                       TH033109N   51                                                                              60                                                                              62          49900001335                                                         709102111010 0000029688000000100000NO                               0000003406  809802008068                                                        0000001010  819102111020 0000010603000000100000NO                               0000001609  819802008068                                                                    819102111030 0000012705000000100000NO                               0000001345  819802008068                                                        0000000204  819102111040 0000000729000000100000NO                                           8949900000002500                                                                9000000053725           0                       0000000250000000009426          Y      GAZEI00018000000053725";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);

			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			AssertEquals("Parent 1 Tariff number", "9802.00.8068", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", printBO.EntryPrintLines[0].SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", printBO.EntryPrintLines[0].SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", printBO.EntryPrintLines[0].SecondaryLine7FormattedTariff);

			AssertEquals("Entry should have pro-rated summary", true, printBO.EntryPrintLines[0].ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", printBO.EntryPrintLines[0].ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", printBO.EntryPrintLines[0].ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", printBO.EntryPrintLines[0].ProRatedLine3);
			AssertEquals("Box 37 Total Duty should remain as actual Duty", 537.25m, printBO.TotalDutyAmt);

			CusEntryLine secondaryTariffLine1 = null;
			CusEntryLine secondaryTariffLine3 = null;
			CusEntryLine secondaryTariffLine5 = null;
			CusEntryLine secondaryTariffLine7 = null;
			foreach (CusEntryLine cusLine in entry.EntryLines)
			{
				secondaryTariffLine1 = cusLine.ChildLines[0];
				secondaryTariffLine3 = cusLine.ChildLines[2];
				secondaryTariffLine5 = cusLine.ChildLines[4];
				secondaryTariffLine7 = cusLine.ChildLines[6];
			}

			AssertEquals("Secondary Line 1 Actual Duty", 296.88m, secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 3 Actual Duty", 106.03m, secondaryTariffLine3.DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, printBO.EntryPrintLines[0].SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 5 Actual Duty", 127.05m, secondaryTariffLine5.DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.3m, printBO.EntryPrintLines[0].SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 7 Actual Duty", 7.29m, secondaryTariffLine7.DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, printBO.EntryPrintLines[0].SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine7DutyPercentAsString);
		}

		[TestDate(2009, 04, 08)]
		public void TestWatchEntryReturnedToUSAfterRepairs()
		{
			//Admin EXAMPLE NUMBER (2)
			CreateWatchWithRepairsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Box 37 Total Duty", 537.22m, ehp.TotalDutyAmt);

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("Secondary Line 1 Actual Duty", 287.70m, entryLine.secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 3 Actual Duty", 135.91m, entryLine.secondaryTariffLine3.DutyAmount);
			AssertEquals("Secondary Line 5 Actual Duty", 113.61m, entryLine.secondaryTariffLine5.DutyAmount);
			AssertEquals("Secondary Line 7 Actual Duty", 0m, entryLine.secondaryTariffLine7.DutyAmount);

			AssertEquals("Entry should have Ad Valorem Calculation summary", true, entryLine.AdValoremConversionCalculation);
			AssertEquals("1) A: Total Watches", "1000 x $0.44 NO", entryLine.AVWatches);
			AssertEquals("1) A: Total Watches Duty", 440m, entryLine.AVWatchesDuty);
			AssertEquals("1) B: Cases", "$2619 x 6%", entryLine.AVCases);
			AssertEquals("1) B: Cases Duty", 157.14m, entryLine.AVCasesDuty);
			AssertEquals("1) C: Bracelets", "$1345 x 14%", entryLine.AVBracelets);
			AssertEquals("1) C: Bracelets Duty", 188.30m, entryLine.AVBraceletsDuty);
			AssertEquals("1) D: Batteries", "$204 x 5.3%", entryLine.AVBatteries);
			AssertEquals("1) C: Batteries Duty", 10.81m, entryLine.AVBatteriesDuty);
			AssertEquals("1) Total Duty", 796.25m, entryLine.AVTotalDuty);
			AssertEquals("Ad Valorem Calculation summary line 2", "$796.25/$9426.00 (Total Entered Value) = 8.447%", entryLine.AVLine2);

			AssertEquals("TotalDutyAmt", 537.22m, ehp.TotalDutyAmt);
			AssertEquals("TotalEstTax", 0m, ehp.TotalEstTax);
			AssertEquals("TotalOther", 0m, ehp.TotalOther);
			AssertEquals("Block40Total", 537.22m, ehp.Block40Total);
		}

		public void TestReturnedAfterRepairsWatchEntryPrintedFromMessage()
		{
			CreateWatchWithRepairsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               217                  10A2818            91-013199000                 0         GAZ 0000078801        20                         402818033109B00001092            650  033109         22            08109384852                         00000000                      30                                  0                                   QF      40001TH00000018520000000000                                                     50 9802004040                                                       TH033109N   51                                                                              60                                                                              62          49900001335                                                         709102111010 0000029688000000100000NO                               0000003406  809802004040                                                        0000001010  819102111020 0000010603000000100000NO                               0000001609  819802004040                                                                    819102111030 0000012705000000100000NO                               0000001345  819802004040                                                        0000000204  819102111040 0000000729000000100000NO                                           8949900000002500                                                                9000000053725           0                       0000000250000000009426          Y      GAZEI00018000000053725";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);

			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			EntrySummary7501Line entryLine = printBO.EntryPrintLines[0];
			AssertEquals("Entry should have Ad Valorem Calculation summary", true, entryLine.AdValoremConversionCalculation);
			AssertEquals("1) A: Total Watches", "1000 x $0.44 NO", entryLine.AVWatches);
			AssertEquals("1) A: Total Watches Duty", 440m, entryLine.AVWatchesDuty);
			AssertEquals("1) B: Cases", "$2619 x 6%", entryLine.AVCases);
			AssertEquals("1) B: Cases Duty", 157.14m, entryLine.AVCasesDuty);
			AssertEquals("1) C: Bracelets", "$1345 x 14%", entryLine.AVBracelets);
			AssertEquals("1) C: Bracelets Duty", 188.30m, entryLine.AVBraceletsDuty);
			AssertEquals("1) D: Batteries", "$204 x 5.3%", entryLine.AVBatteries);
			AssertEquals("1) C: Batteries Duty", 10.81m, entryLine.AVBatteriesDuty);
			AssertEquals("Ad Valorem Calculation summary line 2", "$796.25/$9426.00 (Total Entered Value) = 8.447%", entryLine.AVLine2);
		}

		[TestDate(2009, 04, 08)]
		public void TestWatchEntryAdminExample3()
		{
			//Admin EXAMPLE NUMBER (3)
			CreateAdminExample3();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Box 37 Total Duty", 287.70m, ehp.TotalDutyAmt);

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("Secondary Line 1 Actual Duty", 287.70m, entryLine.secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 3 Actual Duty", 0m, entryLine.secondaryTariffLine3.DutyAmount);
			AssertEquals("Secondary Line 5 Actual Duty", 0m, entryLine.secondaryTariffLine5.DutyAmount);
			AssertEquals("Secondary Line 7 Actual Duty", 0m, entryLine.secondaryTariffLine7.DutyAmount);

			AssertEquals("Entry should have Ad Valorem Calculation summary", true, entryLine.AdValoremConversionCalculation);
			AssertEquals("1) A: Total Watches", "1000 x $0.44 NO", entryLine.AVWatches);
			AssertEquals("1) A: Total Watches Duty", 440m, entryLine.AVWatchesDuty);
			AssertEquals("1) B: Cases", "$2619 x 6%", entryLine.AVCases);
			AssertEquals("1) B: Cases Duty", 157.14m, entryLine.AVCasesDuty);
			AssertEquals("1) C: Bracelets", "$1345 x 14%", entryLine.AVBracelets);
			AssertEquals("1) C: Bracelets Duty", 188.30m, entryLine.AVBraceletsDuty);
			AssertEquals("1) D: Batteries", "$204 x 5.3%", entryLine.AVBatteries);
			AssertEquals("1) C: Batteries Duty", 10.81m, entryLine.AVBatteriesDuty);
			AssertEquals("1) Total Duty", 796.25m, entryLine.AVTotalDuty);
			AssertEquals("Ad Valorem Calculation summary line 2", "$796.25/$9426.00 (Total Entered Value) = 8.447%", entryLine.AVLine2);

			AssertEquals("TotalDutyAmt", 287.70m, ehp.TotalDutyAmt);
			AssertEquals("TotalEstTax", 0m, ehp.TotalEstTax);
			AssertEquals("TotalOther", 0m, ehp.TotalOther);
			AssertEquals("Block40Total", 287.70m, ehp.Block40Total);
		}

		[TestDate(2009, 04, 08)]
		public void TestWatchEntryAdminExample4()
		{
			//Admin EXAMPLE NUMBER (4)
			CreateAdminExample4();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Box 37 Total Duty", 0m, ehp.TotalDutyAmt);

			EntrySummary7501Line entryLine = ehp.EntryPrintLines[0];
			AssertEquals("Secondary Line 1 Actual Duty", 0m, entryLine.secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 3 Actual Duty", 0m, entryLine.secondaryTariffLine3.DutyAmount);
			AssertEquals("Secondary Line 5 Actual Duty", 0m, entryLine.secondaryTariffLine5.DutyAmount);
			AssertEquals("Secondary Line 7 Actual Duty", 0m, entryLine.secondaryTariffLine7.DutyAmount);

			AssertEquals("Entry should not have Ad Valorem Calculation summary", false, entryLine.AdValoremConversionCalculation);
		}

		public void TestEntryDate()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("EntryDate", ZDateTime.Empty, ehp.EstimatedEntryDate);

			var expectedDate = new ZDateTime(2013, 06, 01);
			Declaration.US_EstimatedEntryDate = expectedDate;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("EntryDate should return Estimated Date in this case", expectedDate, ehp.EstimatedEntryDate);
		}

		public void TestEntryDateFromPresentationDate()
		{
			CreateWatchDeclaration();
			var expectedDate = new ZDateTime(2013, 06, 15);
			Declaration.US_PresentationDate = expectedDate;

			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               217                  10A2818            91-013199000                 0         GAZ 0000078801        20                         402818033109B00001092            650  033109         22            08109384852                         00000000                      30                                  0                                   QF      40001TH00000018520000000000                                                     50 9802008068                                                       TH033109N   51                                                                              60                                                                              62          49900001335                                                         709102111010 0000029688000000100000NO                               0000003406  809802008068                                                        0000001010  819102111020 0000010603000000100000NO                               0000001609  819802008068                                                                    819102111030 0000012705000000100000NO                               0000001345  819802008068                                                        0000000204  819102111040 0000000729000000100000NO                                           8949900000002500                                                                9000000053725           0                       0000000250000000009426          Y      GAZEI00018000000053725";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);
			AssertEquals("EntryDate should return Presentation Date now", expectedDate, printBO.EstimatedEntryDate);
		}

		public void TestEntryDateFromAuthorisationDate()
		{
			CreateWatchDeclaration();
			var expectedDate = new ZDateTime(2013, 06, 30);
			Declaration.JE_EntryAuthorisationDate = expectedDate;

			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               217                  10A2818            91-013199000                 0         GAZ 0000078801        20                         402818033109B00001092            650  033109         22            08109384852                         00000000                      30                                  0                                   QF      40001TH00000018520000000000                                                     50 9802008068                                                       TH033109N   51                                                                              60                                                                              62          49900001335                                                         709102111010 0000029688000000100000NO                               0000003406  809802008068                                                        0000001010  819102111020 0000010603000000100000NO                               0000001609  819802008068                                                                    819102111030 0000012705000000100000NO                               0000001345  819802008068                                                        0000000204  819102111040 0000000729000000100000NO                                           8949900000002500                                                                9000000053725           0                       0000000250000000009426          Y      GAZEI00018000000053725";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);
			AssertEquals("EntryDate should return Authorisation Date now", expectedDate, printBO.EstimatedEntryDate);
		}

		public void TestLocationOfGoodsAndName()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("LocationOfGoodsAndName", "", ehp.LocationOfGoodsAndName);

			Declaration.US_GeneralOrderNo = "483720958101";
			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders[0];
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Location should return 12 digit formatted General Order Number", "G.O. 483720958101", ehp.LocationOfGoodsAndName);

			Declaration.US_GeneralOrderNo = "2012270408101";
			ensEntry = Declaration.ActiveEntryHeaders[0];
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Location should return 13 digit formatted General Order Number", "GO-2012-2704-08101", ehp.LocationOfGoodsAndName);
		}

		public void TestLocationOfGoodsAndNameForBondedWarehouseEntry()
		{
			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "LOCA", "Location", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Declaration.US_EnableENS = true;
			Declaration.US_US_NKLocationOfGoods = "LOCA";

			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Organisation name to be used when US_US_NKLocationOfGoods has been entered, but no warehouse", "LOCA/Location", ehp.LocationOfGoodsAndName);

			var warehouse = Factory.New<OrgHeader>();
			warehouse.FillWithValidTestData();
			warehouse.OH_FullName = "JPDuminy Bond Stores";
			warehouse.OH_IsWarehouseClient = true;

			Declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Organisation name to be used when warehouse has no FIRMS code", "LOCA/Location", ehp.LocationOfGoodsAndName);

			var address = warehouse.Addresses.AddNew();
			address.OA_Address1 = "123";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "WH01");
			Declaration.WarehouseDocAddress.E2_OA_Address = address.PK;

			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			address.OA_CompanyNameOverride = "Warehouse Store 2";
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("FIRMS Code Name to be used where a valid firms code is found and company name is overridden", "WH01/Warehouse Store 2", ehp.LocationOfGoodsAndName);

			address.OA_CompanyNameOverride = ZString.Empty;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Organisation name to be used when US_US_NKLocationOfGoods and warehouse has been entered", "WH01/JPDuminy Bond Stores", ehp.LocationOfGoodsAndName);

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Organisation name to be used when FIRMS Code does not exist in table", "WH01/JPDuminy Bond Stores", ehp.LocationOfGoodsAndName);
		}

		public void TestLocationOfGoodsAndNameForBondedWarehousePrintNameWhenNoCode()
		{
			var warehouse = Factory.New<OrgHeader>();
			warehouse.FillWithValidTestData();
			warehouse.OH_FullName = "Slim Dusty's Warehouse With No FIRMS Code";

			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Declaration.US_EnableENS = true;
			Declaration.US_US_NKLocationOfGoods = "NWHS";
			Declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Organisation name to be used as fallback", "NWHS", ehp.LocationOfGoodsAndName);

			var address = warehouse.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "WH01");
			address.OA_Address1 = "123";
			Declaration.WarehouseDocAddress.E2_OA_Address = address.PK;

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Organisation name to be used with code when FIRMS Code does not exist in table", "WH01/Slim Dusty's Warehouse With No FIRMS Code", ehp.LocationOfGoodsAndName);
		}

		[TestDate(2010, 04, 30)]
		public void TestBlock40TotalWithDistilledSpirits()
		{
			CreateDistilledSpiritsEntry();
			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("TotalDutyAmt", 0m, ehp.TotalDutyAmt);
			AssertEquals("TotalEstTax", 713.26m, ehp.TotalEstTax);
			AssertEquals("TotalOther", 25m, ehp.TotalOther);
			AssertEquals("Block40Total includes spirits tax", 738.26m, ehp.Block40Total);
		}

		[TestDate(2010, 04, 30)]
		public void TestBlock40TotalWithDistilledSpiritsWhenTaxIsDeferred()
		{
			CreateDistilledSpiritsEntry();
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("TotalDutyAmt", 0m, ehp.TotalDutyAmt);
			AssertEquals("TotalEstTax", 713.26m, ehp.TotalEstTax);
			AssertEquals("TotalOther", 25m, ehp.TotalOther);
			AssertEquals("Block40Total - should not include Spirits Tax when deferred", 25m, ehp.Block40Total);
		}

		[TestDate(2010, 09, 29)]
		public void TestBlock40TotalWithDistilledSpiritsWarehouseEntry()
		{
			CreateDistilledSpiritsEntry();
			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax;
			Declaration.US_IsHMFApplicable = "Y";
			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("TotalDutyAmt", 0m, ehp.TotalDutyAmt);
			AssertEquals("TotalEstTax", 713.26m, ehp.TotalEstTax);
			AssertEquals("MPF applicable", 25m, ehp.SummaryFee1);
			AssertEquals("HMF calculated", 12.5m, ehp.SummaryFee2);
			AssertEquals("TotalOther should only show HMF fee applicable", 12.5m, ehp.TotalOther);
			AssertEquals("Block40Total - only HMF fee is payable", 12.5m, ehp.Block40Total);
		}

		public void TestHasInvoiceAdjustmentLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG1";
			invoice.JZ_IncoTerm = "CIF";
			invoice.JZ_InvoiceAmount = 24887m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.RemoveAndDeleteAll();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 24887m;

			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("EntryHasInvoiceAdjustmentLines", false, ehp.EntryHasInvoiceAdjustmentLines);

			var charge = invoice.Charges.AddNew("OFT", 2033.05m, JobDeclaration.LocalCurrencyConstantCode);
			charge.J7_IsNotIncludedInInvoice = false;
			charge.J7_IsIncludedInITOT = true;

			charge = invoice.Charges.AddNew("ONS", 17.58m, JobDeclaration.LocalCurrencyConstantCode);
			charge.J7_IsNotIncludedInInvoice = false;
			charge.J7_IsIncludedInITOT = true;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			EntrySummary7501Invoice invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("invoiceLine should have apportioned charges", 2, invoiceLine.ApportionedCharges.Count);
			AssertEquals("InvoiceNo", "001/INV-CHG1", invoice7501Summary.InvoiceNo);

			AssertEquals("AdjustmentItem1USD", "(-) International Freight", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1USD", 2033.05m, invoice7501Summary.Adjustment1USD);

			AssertEquals("AdjustmentItem2USD", "(-) International Insurance", invoice7501Summary.AdjustmentItem2USD);
			AssertEquals("Adjustment2USD", 17.58m, invoice7501Summary.Adjustment2USD);

			AssertEquals("InvoiceValueInUSD", 24887m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("TEV", 22836m, invoice7501Summary.TEV);

			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("EntryHasInvoiceAdjustmentLines", true, ehp.EntryHasInvoiceAdjustmentLines);
		}

		public void TestHasInvoiceAdjustmentLinesWhenAMMVEnteredAtLineLevel()
		{
			var declaration = Factory.New<JobDeclaration>();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-CHG1";
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 24887m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 24887m;

			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("EntryHasInvoiceAdjustmentLines", false, ehp.EntryHasInvoiceAdjustmentLines);

			var invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);

			AssertEquals("invoiceLine should have no apportioned charges", 0, invoiceLine.ApportionedCharges.Count);
			AssertEquals("InvoiceNo", "001/INV-CHG1", invoice7501Summary.InvoiceNo);
			AssertEquals("Should be no adjustment line", "", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("InvoiceValueInUSD", 24887m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("TEV", 24887m, invoice7501Summary.TEV);

			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "KGS";
			invoiceLine.US_AMMVPerUnit = 12.5m;
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("EntryHasInvoiceAdjustmentLines", true, ehp.EntryHasInvoiceAdjustmentLines);
			AssertEquals("invoice line should have 1 ADD charge", 1, invoiceLine.ApportionedCharges.Count);
			AssertEquals("charge should be AMMV value", 1250m, invoiceLine.ApportionedCharges[0].J7_Amount);

			invoice7501Summary = new EntrySummary7501Invoice(invoice, 1, false);
			AssertEquals("InvoiceValueInUSD", 24887m, invoice7501Summary.InvoiceValueInUSD);
			AssertEquals("Should be adjustment line as AMMV is entered on invoice line", "(+) Additional Charge", invoice7501Summary.AdjustmentItem1USD);
			AssertEquals("Adjustment1USD", 1250m, invoice7501Summary.Adjustment1USD);
			AssertEquals("TEV", 26137m, invoice7501Summary.TEV);

			invoiceLine.US_AMMVPerUnit = 0m;
			invoiceLine.US_AMMVPercentage = 10m;
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("EntryHasInvoiceAdjustmentLines", true, ehp.EntryHasInvoiceAdjustmentLines);
		}

		[TestDate(2009, 6, 1)]
		public void TestPrintingOfExcessFees()
		{
			var declaration = Factory.New<JobDeclaration>();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_RN_NKDefaultOrigin = "IT";
			invoice.US_UC_NKCountryOfExport = "IT";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 22474m;
			invoiceLine1.JI_Tariff = "1902192030";
			invoiceLine1.JI_CustomsQuantity = 12351m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.US_ADD_NA = true;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2000m;
			invoiceLine2.JI_CustomsQuantity = 150m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_Tariff = "0804.40.0010";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 6000m;
			invoiceLine3.JI_CustomsQuantity = 150m;
			invoiceLine3.JI_CustomsUnitQty = "KG";
			invoiceLine3.JI_Tariff = "0804.40.0010";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entry = declaration.ActiveEntryHeaders[0];
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Avocado, 100m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Beef, 200m);
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("SummaryBlockOverflow", "", ehp.SummaryBlockOverflow);
			AssertEquals("Summary Fee Desc 1", "053 053 Desc from DB", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total Beef Fee)", 200m, ehp.SummaryFee1);
			AssertEquals("Summary Fee Desc 2", "107 107 Desc from DB", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total Avocado Fee)", 100m, ehp.SummaryFee2);
			AssertEquals("Summary Fee 3", "499 499 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total MPF)", 64m, ehp.SummaryFee3);
			AssertEquals("Summary Fee 4", "501 501 Desc from DB", ehp.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total HMF)", 38.09m, ehp.SummaryFee4);
			AssertEquals("Total Other Fees should inlcude new avocado & beef fees", 402.09m, ehp.TotalOtherFees);

			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry, 300m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 400m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes, 500m);
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("SummaryBlockOverflow", "+", ehp.SummaryBlockOverflow);
			AssertEquals("Summary Fee Desc 1", "053 053 Desc from DB", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total Beef Fee)", 200m, ehp.SummaryFee1);
			AssertEquals("Summary Fee Desc 2", "056 056 Desc from DB", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total Cotton Fee)", 400m, ehp.SummaryFee2);
			AssertEquals("Summary Fee Desc 3", "102 102 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total Fresh Limes Fee)", 500m, ehp.SummaryFee3);
			AssertEquals("Summary Fee Desc 4", "106 106 Desc from DB", ehp.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total Blueberry Fee)", 300m, ehp.SummaryFee4);
			AssertEquals("Summary Fee Desc 5", "107 107 Desc from DB", ehp.SummaryFeeDesc5);
			AssertEquals("SummaryFee5 (Total Avocado Fee)", 100m, ehp.SummaryFee5);
			AssertEquals("Summary Fee 6", "499 499 Desc from DB", ehp.SummaryFeeDesc6);
			AssertEquals("SummaryFee6 (Total MPF)", 64m, ehp.SummaryFee6);
			AssertEquals("Summary Fee 7", "501 501 Desc from DB", ehp.SummaryFeeDesc7);
			AssertEquals("SummaryFee7 (Total HMF)", 38.09m, ehp.SummaryFee7);
			AssertEquals("Total Other Fees should inlcude new blueberry, cotton & lime fees", 1602.09m, ehp.TotalOtherFees);

			AssertEquals("Excess Fee 1", "107 107 Desc from DB", ehp.EntryPrintExcessFees[0].SummaryFeeDesc);
			AssertEquals("ExcessFee1", 100m, ehp.EntryPrintExcessFees[0].SummaryFee);
			AssertEquals("Excess Fee 2", "499 499 Desc from DB", ehp.EntryPrintExcessFees[1].SummaryFeeDesc);
			AssertEquals("ExcessFee3", 64m, ehp.EntryPrintExcessFees[1].SummaryFee);
			AssertEquals("Excess Fee 3", "501 501 Desc from DB", ehp.EntryPrintExcessFees[2].SummaryFeeDesc);
			AssertEquals("ExcessFee3", 38.09m, ehp.EntryPrintExcessFees[2].SummaryFee);
		}

		[TestDate(2009, 6, 1)]
		public void TestPrintingOfMaximumExcessFees()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A475818001";
			addCase.U5_ISOCountryCode = "IT";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "19021920";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 0.19m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C475819005";
			cvdCase.U5_ISOCountryCode = "IT";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "19021920";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 0.01m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_RN_NKDefaultOrigin = "IT";
			invoice.US_UC_NKCountryOfExport = "IT";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 22474m;
			invoiceLine1.JI_Tariff = "1902192030";
			invoiceLine1.JI_CustomsQuantity = 12351m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.US_ADD_NA = true;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2000m;
			invoiceLine2.JI_CustomsQuantity = 0m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_Tariff = "0804.40.0010";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 6000m;
			invoiceLine3.JI_CustomsQuantity = 0m;
			invoiceLine3.JI_CustomsUnitQty = "KG";
			invoiceLine3.JI_Tariff = "0804.40.0010";

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 22474m;
			invoiceLine4.JI_Tariff = "1902192030";
			invoiceLine4.JI_CustomsQuantity = 12351m;
			invoiceLine4.JI_CustomsUnitQty = "KG";
			invoiceLine4.US_ADD_NA = true;
			invoiceLine4.US_CVDCaseNo = "C475819005";
			invoiceLine4.US_CVDDepositRateIndicator = "1";

			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 8550m;
			invoiceLine5.JI_Tariff = "1902192030";
			invoiceLine5.JI_CustomsQuantity = 4077m;
			invoiceLine5.JI_CustomsUnitQty = "KG";
			invoiceLine5.US_CVD_NA = true;
			invoiceLine5.US_ADDCaseNo = "A475818001";
			invoiceLine5.US_ADDDepositRateIndicator = "1";

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entry = declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("SummaryBlockOverflow", "", ehp.SummaryBlockOverflow);
			AssertEquals("Summary Fee Desc 1", "012 AD", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total AD)", 1624.50m, ehp.SummaryFee1);
			AssertEquals("Summary Fee Desc 2", "013 CVD", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total CVD)", 224.74m, ehp.SummaryFee2);
			AssertEquals("Summary Fee 3", "499 499 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total MPF)", 129.16m, ehp.SummaryFee3);
			AssertEquals("Summary Fee 4", "501 501 Desc from DB", ehp.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total HMF)", 76.87m, ehp.SummaryFee4);
			AssertEquals("Total Other Fees", 2055.27m, ehp.TotalOtherFees);
			AssertEquals("Total Other Fees should inlcude new avocado & beef fees", 2055.27m, ehp.TotalOtherFees);

			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Avocado, 100m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Beef, 200m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry, 300m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 400m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes, 450.55m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Honey, 500m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Mango, 600m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom, 700m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry, 800m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Pork, 900m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Potato, 1000m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, 1100m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Sorghum, 1200m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Sugar, 1300m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Watermelon, 1400m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail, 1500m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, 1600m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, 1700m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies, 1800.88m);
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("SummaryBlockOverflow", "+", ehp.SummaryBlockOverflow);
			AssertEquals("Summary Fee Desc 1", "012 AD", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total AD)", 1624.50m, ehp.SummaryFee1);
			AssertEquals("Summary Fee Desc 2", "013 CVD", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total CVD)", 224.74m, ehp.SummaryFee2);
			AssertEquals("Summary Fee 3", "053 053 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total Beef Fee)", 200m, ehp.SummaryFee3);
			AssertEquals("Summary Fee 4", "054 054 Desc from DB", ehp.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total Pork Fee)", 900m, ehp.SummaryFee4);
			AssertEquals("Summary Fee 5", "055 055 Desc from DB", ehp.SummaryFeeDesc5);
			AssertEquals("SummaryFee5 (Total Honey Fee)", 500m, ehp.SummaryFee5);
			AssertEquals("SummaryFee6", "056 056 Desc from DB", ehp.SummaryFeeDesc6);
			AssertEquals("SummaryFee6 (Total Cotton Fee)", 400m, ehp.SummaryFee6);
			AssertEquals("SummaryFee7", "057 057 Desc from DB", ehp.SummaryFeeDesc7);
			AssertEquals("SummaryFee7 (Total Raspberry Fee)", 800m, ehp.SummaryFee7);
			AssertEquals("SummaryFee8", "058 058 Desc from DB", ehp.SummaryFeeDesc8);
			AssertEquals("SummaryFee8 (Total Other Agencies)", 1800.88m, ehp.SummaryFee8);
			AssertEquals("SummaryFee9", "079 079 Desc from DB", ehp.SummaryFeeDesc9);
			AssertEquals("SummaryFee9 (Total Sugar Fee)", 1300m, ehp.SummaryFee9);
			AssertEquals("SummaryFee10", "090 090 Desc from DB", ehp.SummaryFeeDesc10);
			AssertEquals("SummaryFee10 (Total Potato Fee)", 1000m, ehp.SummaryFee10);
			AssertEquals("SummaryFee11", "102 102 Desc from DB", ehp.SummaryFeeDesc11);
			AssertEquals("SummaryFee11 (Total Fresh Limes Fee)", 450.55m, ehp.SummaryFee11);
			AssertEquals("SummaryFee12", "103 103 Desc from DB", ehp.SummaryFeeDesc12);
			AssertEquals("SummaryFee12 (Total Mushroom Fee)", 700m, ehp.SummaryFee12);
			AssertEquals("SummaryFee13", "104 104 Desc from DB", ehp.SummaryFeeDesc13);
			AssertEquals("SummaryFee13 (Total Watermelon Fee)", 1400m, ehp.SummaryFee13);
			AssertEquals("SummaryFee14", "105 105 Desc from DB", ehp.SummaryFeeDesc14);
			AssertEquals("SummaryFee14 (Total Softwood Lumber Fee)", 1100m, ehp.SummaryFee14);
			AssertEquals("SummaryFee15", "106 106 Desc from DB", ehp.SummaryFeeDesc15);
			AssertEquals("SummaryFee15 (Total Blueberry Fee)", 300m, ehp.SummaryFee15);
			AssertEquals("SummaryFee16", "107 107 Desc from DB", ehp.SummaryFeeDesc16);
			AssertEquals("SummaryFee16 (Total Avocado Fee)", 100m, ehp.SummaryFee16);
			AssertEquals("SummaryFee17", "108 108 Desc from DB", ehp.SummaryFeeDesc17);
			AssertEquals("SummaryFee17 (Total Mango Fee)", 600m, ehp.SummaryFee17);
			AssertEquals("SummaryFee18", "109 109 Desc from DB", ehp.SummaryFeeDesc18);
			AssertEquals("SummaryFee18 (Total Sorghum Fee)", 1200m, ehp.SummaryFee18);
			AssertEquals("SummaryFee19", "311 311 Desc from DB", ehp.SummaryFeeDesc19);
			AssertEquals("SummaryFee19 (Total Merchandise Informal Fee)", 1600m, ehp.SummaryFee19);
			AssertEquals("SummaryFee20", "496 496 Desc from DB", ehp.SummaryFeeDesc20);
			AssertEquals("SummaryFee20 (Total Dutiable Mail Fee)", 1500m, ehp.SummaryFee20);
			AssertEquals("SummaryFee21", "499 499 Desc from DB", ehp.SummaryFeeDesc21);
			AssertEquals("SummaryFee21 (Total Merchandise Processing Fee)", 129.16m, ehp.SummaryFee21);
			AssertEquals("SummaryFee22", "500 500 Desc from DB", ehp.SummaryFeeDesc22);
			AssertEquals("SummaryFee22 (Total Merchandise Surcharge Fee)", 1700m, ehp.SummaryFee22);
			AssertEquals("SummaryFee23", "501 501 Desc from DB", ehp.SummaryFeeDesc23);
			AssertEquals("SummaryFee23 (Total Harbor Maintenance Fee)", 76.87m, ehp.SummaryFee23);
			AssertEquals("Total Other Fees", 19606.70m, ehp.TotalOtherFees);
			AssertEquals("Total Other Fees should inlcude all fees", 19606.70m, ehp.TotalOtherFees);

			AssertEquals("EntryPrintExcessFees count", 19, ehp.EntryPrintExcessFees.Count);
			AssertEquals("Excess Fee Collection: Excess Fee 1", "055 055 Desc from DB", ehp.EntryPrintExcessFees[0].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee1", 500m, ehp.EntryPrintExcessFees[0].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 2", "056 056 Desc from DB", ehp.EntryPrintExcessFees[1].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee2", 400m, ehp.EntryPrintExcessFees[1].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 3", "057 057 Desc from DB", ehp.EntryPrintExcessFees[2].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee3", 800m, ehp.EntryPrintExcessFees[2].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 4", "058 058 Desc from DB", ehp.EntryPrintExcessFees[3].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee4", 1800.88m, ehp.EntryPrintExcessFees[3].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 5", "079 079 Desc from DB", ehp.EntryPrintExcessFees[4].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee5", 1300m, ehp.EntryPrintExcessFees[4].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 6", "090 090 Desc from DB", ehp.EntryPrintExcessFees[5].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee6", 1000m, ehp.EntryPrintExcessFees[5].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 7", "102 102 Desc from DB", ehp.EntryPrintExcessFees[6].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee7", 450.55m, ehp.EntryPrintExcessFees[6].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 8", "103 103 Desc from DB", ehp.EntryPrintExcessFees[7].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee8", 700m, ehp.EntryPrintExcessFees[7].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 9", "104 104 Desc from DB", ehp.EntryPrintExcessFees[8].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee9", 1400m, ehp.EntryPrintExcessFees[8].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 10", "105 105 Desc from DB", ehp.EntryPrintExcessFees[9].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee10", 1100m, ehp.EntryPrintExcessFees[9].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 11", "106 106 Desc from DB", ehp.EntryPrintExcessFees[10].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee11", 300m, ehp.EntryPrintExcessFees[10].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 12", "107 107 Desc from DB", ehp.EntryPrintExcessFees[11].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee12", 100m, ehp.EntryPrintExcessFees[11].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 13", "108 108 Desc from DB", ehp.EntryPrintExcessFees[12].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee13", 600m, ehp.EntryPrintExcessFees[12].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 14", "109 109 Desc from DB", ehp.EntryPrintExcessFees[13].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee14", 1200m, ehp.EntryPrintExcessFees[13].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 15", "311 311 Desc from DB", ehp.EntryPrintExcessFees[14].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee15", 1600m, ehp.EntryPrintExcessFees[14].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 16", "496 496 Desc from DB", ehp.EntryPrintExcessFees[15].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee16", 1500m, ehp.EntryPrintExcessFees[15].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 17", "499 499 Desc from DB", ehp.EntryPrintExcessFees[16].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee17", 129.16m, ehp.EntryPrintExcessFees[16].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 18", "500 500 Desc from DB", ehp.EntryPrintExcessFees[17].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee18", 1700m, ehp.EntryPrintExcessFees[17].SummaryFee);
			AssertEquals("Excess Fee Collection: Excess Fee 19", "501 501 Desc from DB", ehp.EntryPrintExcessFees[18].SummaryFeeDesc);
			AssertEquals("Excess Fee Collection: ExcessFee19", 76.87m, ehp.EntryPrintExcessFees[18].SummaryFee);
		}

		public void TestSummaryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_RN_NKDefaultOrigin = "IT";
			invoice.US_UC_NKCountryOfExport = "IT";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 22474m;
			invoiceLine1.JI_Tariff = "1902192030";
			invoiceLine1.JI_CustomsQuantity = 12351m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entry = declaration.ActiveEntryHeaders[0];
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoingMessage1 = mock.Object;
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageNum = "17611";
			outgoingMessage1.EM_MessageText = "B012507175EI                                               17611                10A250713-4063812CX13-4063812CX                 8         175 1916203001098  CA 20                         302507011812OOI207090C1               011912W096 001 22            17519162030                         00000017BE         TRAV       30                                  0               2020112             TRAV    40001US00000002400000001440                    0000000004                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40002US00000000000000000012                                                     50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40003US00000001760000000035                    0000000003                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40004US00000000420000000008                    0000000001                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40005CN00000000170000000003                                                     50 40169350500000000043000000000100KG                               MX011812Y   60                                        MXWABTEC2700MEX                       62          49900000006                                                         40006US00000000480000000054                    0000000001                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40007DE00000026630000000532                    0000000042                       50 39269099800000014114            X                                MX011812Y   60                                        MXWABTEC2700MEX                       62          49900000922                                                         40008US00000000070000000001                                                     50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40009US00000002240000000045                    0000000004                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40010US00000008640000000172                    0000000014                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40011US00000001520000000030                    0000000002                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40012US00000002150000000043                    0000000003                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       8949900000002500                                                                9000000014157           0                       0000000250000000007891          Y  2507175EI00059000000014157";
			outgoingMessage1.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingMessage1.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			entry.Messages.Add(outgoingMessage1);

			var responseMessage1Accepted = Factory.New<MQEDIMessage>();
			responseMessage1Accepted.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage1Accepted.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1Accepted.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			responseMessage1Accepted.EM_MessageNum = "17611";
			responseMessage1Accepted.EM_MessageText = "B012507175ER                                               17611                10A250713-4063812CX13-4063812CX                 8         175 1916203001098  CA 40002US00000000000000000012000000000000000000000000000000                       50 9801001095                      X                                MX011812Y   E502507175 1916203000228E01   *CENSUS* OR-LO VAL/QTY (2)TARIFF1        OOI2070909000000014157000000000000 00000000000000000000000000000250000000007891          E902507175 19162030   58401761ENT-SUM ACCEPTED WITH WARNINGS           OOI207090Y  2507175ER00006000000014157";
			entry.Messages.Add(responseMessage1Accepted);
			declaration.US_PaperlessEntry = YesNoDefaultList.Codes.No;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var mock1 = Factory.NewMoq<MQEDIMessage>();
			mock1.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoingMessage2 = mock1.Object;
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageNum = "20397";
			outgoingMessage2.EM_MessageText = "B012507175EI                                               20397                10R250713-4063812CX13-4063812CX                 8         175 1916203001098  CA 20                         302507011812OOI207090C1               011912W096 001 22            17519162030                         00000017BE         TRAV       30                                  0               2020112             TRAV    40001US00000002400000001440                    0000000004                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40002US00000000000000000012                                                     50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40003US00000001760000000035                    0000000003                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       40004US00000000420000000008                    0000000001                       50 9801001095                      X                                MX011812Y   60                                        MXWABTEC2700MEX                       8949900000002500                                                                9000000014157           0                       0000000250000000007891          Y  2507175EI00059000000014157";
			outgoingMessage2.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			outgoingMessage2.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			entry.Messages.Add(outgoingMessage2);

			var responseMessage2Rejected = Factory.New<MQEDIMessage>();
			responseMessage2Rejected.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage2Rejected.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2Rejected.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			responseMessage2Rejected.EM_MessageNum = "20397";
			responseMessage2Rejected.EM_MessageText = "B012507175ER                                               20397                10R250713-4063812CX13-4063812CX                 8         175 1916203001098  CA E102507175 19162030   54601   EXISTING ENTRY IN CUSTOMS STATUS         OOI207090E102507175 19162030   52401   TRANSACTION DATA REJECTED                OOI207090Y  2507175ER00003000000014157";
			entry.Messages.Add(responseMessage2Rejected);
			entry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryReplace;
			Factory.Save();

			var entryPrintFromMessage = new EntryMessageENS7501Print(entry, outgoingMessage1, responseMessage1Accepted, null);
			AssertEquals("Should be Census Warning Summary Status", USConstants.EntrySummaryDisposition.CensusWarning, entryPrintFromMessage.SummaryStatus);

			declaration.US_ManEntry = true;
			var entryPrintFromHeader = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Should be Empty, because Summary Status not relevant for header printing", ZString.Empty, entryPrintFromHeader.SummaryStatus);
		}

		public void TestImportingCarrier()
		{
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.JE_VesselName = "Test Carrier";
			var entryPrint = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Importing Carrier", "Test Carrier", entryPrint.ImportingCarrier);

			Declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			Declaration.US_PipelineName = "Test PipelineName";
			AssertEquals("Importing Carrier", "Test PipelineName", entryPrint.ImportingCarrier);
		}

		public void TestEFTPaymentDate()
		{
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals(ZDate.Empty, ehp.EFTPaymentDate);

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_PaymentDueDate = new ZDateTime(2013, 06, 21);
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Deferred Tax Due Date should be calculated based on Duty Due Date", new ZDateTime(2013, 07, 12), ehp.EFTPaymentDate);

			Declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2013, 06, 29);
			Declaration.US_PaymentDueDate = new ZDateTime(2013, 06, 16);
			Entry.US_CollectionDate = new ZDateTime(2013, 06, 15);
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals(@"Based on Collection date, note if duty due had been used, 
it would have pushed this to the next payment period and the taxes would have been late", new ZDateTime(2013, 06, 28), ehp.EFTPaymentDate);

			Declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			Declaration.US_EstimatedEntryDate = new ZDateTime(2013, 06, 05);
			Declaration.US_PaymentDueDate = new ZDateTime(2013, 06, 19);
			Entry.US_CollectionDate = new ZDateTime(2013, 06, 19);
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals(@"Based on Estimated Entry Date, note if duty due or collection had been used, 
it would have pushed this to the next payment period and the taxes would have been late", new ZDateTime(2013, 06, 28), ehp.EFTPaymentDate);
		}

		public void TestIVisualizerNoteSupporterMembers()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			var supporter = ehp as IVisualizerNoteSupporter;
			AssertNotNull("EntryHeaderENS7501Print should implement IVisualizerNoteSupporter", supporter);
			AssertEquals("supporter.PK", Entry.PK, supporter.PK);
			AssertEquals("supporter.TableCode", CusEntryHeaderSchema.Constants.Prefix, supporter.TableCode);
			AssertEquals("supporter.ChildBusinessObjectPK", ZGuid.Empty, supporter.ChildBusinessObjectPK);
		}

		protected override BusinessObject GetNewBusinessObject() => new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}

		void CreateTIBDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;

			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		[TestDate(2018, 12, 27)]
		void CreateTIBWithAddCVDFees()
		{
			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A570956000";
			case1.U5_ISOCountryCode = "CN";
			case1.U5_CaseStatus = "AC";
			case1.U5_CaseStatusDate = ZDateTime.Today;

			var case2 = Factory.New<USCACCase>();
			case2.U5_CaseNumber = "C570957001";
			case2.U5_ISOCountryCode = "CN";
			case2.U5_CaseStatus = "AC";
			case2.U5_CaseStatusDate = ZDateTime.Today;

			var caseRate1 = Factory.New<USCACCaseRate>();
			caseRate1.U6_CaseNumber = "A570956000";
			caseRate1.U6_AdValoremRate = 0.9874m;
			caseRate1.U6_EffectiveDate = ZDateTime.Today;

			var caseRate2 = Factory.New<USCACCaseRate>();
			caseRate2.U6_CaseNumber = "C570957001";
			caseRate2.U6_AdValoremRate = 0.1366m;
			caseRate2.U6_EffectiveDate = ZDateTime.Today;

			var caseTariff1 = Factory.New<USCACCaseTariff>();
			caseTariff1.U9_CaseNumber = "A570956000";
			caseTariff1.U9_TariffNumber = "7304316050";
			caseTariff1.U9_AddedDate = ZDateTime.Today;
			caseTariff1.U9_InactivatedDate = new ZDateTime(2059, 6, 6);

			var caseTariff2 = Factory.New<USCACCaseTariff>();
			caseTariff2.U9_CaseNumber = "C570957001";
			caseTariff2.U9_TariffNumber = "7304316050";
			caseTariff2.U9_AddedDate = ZDateTime.Today;
			caseTariff2.U9_InactivatedDate = new ZDateTime(2059, 6, 6);

			Factory.Save();

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 500m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.JI_CustomsUnitQty = "PFL";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "7304316050";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.US_ADDCaseNo = "A570956000";
			invoiceLine.US_ADDDepositValue = 200m;
			invoiceLine.US_CVDCaseNo = "C570957001";
			invoiceLine.US_CVDDepositValue = 300m;

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateTIBWithExemptTariffsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 8464m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130050";
			invoiceLine1.JI_InvoiceQuantity = 95m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 8000m;

			invoiceLine1.JI_Tariff = "8528723600";
			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_CustomsUnitQty = "NO";

			JobComInvoiceLine invoiceLine2 = Declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "98130050";
			invoiceLine2.JI_InvoiceQuantity = 5m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_LinePrice = 464m;

			invoiceLine2.JI_Tariff = "7318154000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateTIBWithBothExemptAndStandardTariffsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "COMBINED";
			invoiceHeader.JZ_InvoiceAmount = 32519.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;
			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine2 = Declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "98130050";
			invoiceLine2.JI_InvoiceQuantity = 95m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_LinePrice = 8000m;
			invoiceLine2.JI_CountryOfOrigin = "GB";

			invoiceLine2.JI_Tariff = "8528723600";
			invoiceLine2.JI_CustomsQuantity = 10m;
			invoiceLine2.JI_CustomsUnitQty = "NO";

			JobComInvoiceLine invoiceLine3 = Declaration.InvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "98130050";
			invoiceLine3.JI_InvoiceQuantity = 5m;
			invoiceLine3.JI_InvoiceUQ = "KG";
			invoiceLine3.JI_LinePrice = 464m;
			invoiceLine3.JI_CountryOfOrigin = "GB";
			invoiceLine3.JI_Tariff = "7318154000";
			invoiceLine3.JI_CustomsQuantity = 50m;
			invoiceLine3.JI_CustomsUnitQty = "KG";

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateTIBMotorVehicleDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130075";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "NO";
			invoiceLine1.JI_LinePrice = 243055m;

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateWatchDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH";
			invoiceHeader.JZ_InvoiceAmount = 9426m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802008068";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802008068";
				childLine2.US_98GoodsValue = 1010m;
				childLine2.JI_Tariff = "9102111020";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802008068";
				childLine4.JI_Tariff = "9102111030";
				childLine4.JI_InvoiceQuantity = 1000m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802008068";
				childLine6.US_98GoodsValue = 204m;
				childLine6.JI_Tariff = "9102111040";
				childLine6.JI_InvoiceQuantity = 1000m;
				childLine6.JI_InvoiceUQ = "NO";
				childLine6.JI_CustomsQuantity = 1000m;
				childLine6.JI_CustomsUnitQty = "NO";
				childLine6.JI_LinePrice = 0m;
			}

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateWatchWithRepairsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "REPAIRED WATCH";
				invoiceHeader.JZ_InvoiceAmount = 9426m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
				invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
				invoiceHeader.JZ_IncoTerm = "FOB";

				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802004040";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802004040";
				childLine2.US_98GoodsValue = 1010m;
				childLine2.JI_Tariff = "9102111020";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802004040";
				childLine4.JI_Tariff = "9102111030";
				childLine4.JI_InvoiceQuantity = 1000m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802004040";
				childLine6.US_98GoodsValue = 204m;
				childLine6.JI_Tariff = "9102111040";
				childLine6.JI_InvoiceQuantity = 1000m;
				childLine6.JI_InvoiceUQ = "NO";
				childLine6.JI_CustomsQuantity = 1000m;
				childLine6.JI_CustomsUnitQty = "NO";
				childLine6.JI_LinePrice = 0m;
			}

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateAdminExample3()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "REPAIRED WATCH";
			invoiceHeader.JZ_InvoiceAmount = 9426m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802004040";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802004040";
				childLine2.US_98GoodsValue = 2619m;
				childLine2.JI_Tariff = "9102111020";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 0m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802004040";
				childLine4.US_98GoodsValue = 1345m;
				childLine4.JI_Tariff = "9102111030";
				childLine4.JI_InvoiceQuantity = 1000m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802004040";
				childLine6.US_98GoodsValue = 204m;
				childLine6.JI_Tariff = "9102111040";
				childLine6.JI_InvoiceQuantity = 1000m;
				childLine6.JI_InvoiceUQ = "NO";
				childLine6.JI_CustomsQuantity = 1000m;
				childLine6.JI_CustomsUnitQty = "NO";
				childLine6.JI_LinePrice = 0m;
			}

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateAdminExample4()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "REPAIRED WATCH";
			invoiceHeader.JZ_InvoiceAmount = 9426m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802004040";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;
				invoiceLine1.US_SPI = SpecialProgramList.Codes.CA;
				invoiceLine1.US_UC_NKCountryOfOrigin = "XO";

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802004040";
				childLine2.US_98GoodsValue = 1010m;
				childLine2.JI_Tariff = "9102111020";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802004040";
				childLine4.JI_Tariff = "9102111030";
				childLine4.JI_InvoiceQuantity = 1000m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802004040";
				childLine6.US_98GoodsValue = 204m;
				childLine6.JI_Tariff = "9102111040";
				childLine6.JI_InvoiceQuantity = 1000m;
				childLine6.JI_InvoiceUQ = "NO";
				childLine6.JI_CustomsQuantity = 1000m;
				childLine6.JI_CustomsUnitQty = "NO";
				childLine6.JI_LinePrice = 0m;
			}

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateDistilledSpiritsEntry()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2208303030";
			invoiceLine1.JI_InvoiceQuantity = 500m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_CustomsQuantity = 200m;
			invoiceLine1.JI_CustomsUnitQty = "PFL";
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		CusEntryHeader Entry
		{
			get { return entry ?? (entry = Declaration.CustomsEntryHeaders.AddNew()); }
		}
		CusEntryHeader entry;
	}
}
