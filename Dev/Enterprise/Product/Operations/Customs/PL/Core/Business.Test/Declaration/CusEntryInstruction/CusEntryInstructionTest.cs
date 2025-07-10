using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryInstruction))]
sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
{
	public void TestAllInstructionSupportingDocuments()
	{
		var instruction = GetInstruction();
		var invoice = instruction.JobDeclaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		var otherInstruction = instruction.JobDeclaration.CustomsEntryInstructions.AddNew();
		var otherInvoice = instruction.JobDeclaration.Invoices.AddNew();
		var otherInvoiceLine = otherInvoice.InvoiceLines.AddNew();
		otherInvoiceLine.JI_CEI = otherInstruction.PK;
		instruction.JobDeclaration.SupportingDocuments.AddNew();
		invoice.SupportingDocuments.AddNew();
		invoiceLine.SupportingDocuments.AddNew();
		otherInvoice.SupportingDocuments.AddNew();
		otherInvoiceLine.SupportingDocuments.AddNew();
		AssertEquals(3, instruction.AllInstructionSupportingDocuments.Count());
	}

	public void TestCusAuthorizationUsages()
	{
		AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>>(GetInstruction().CusAuthorizationUsages);
	}

	public void TestFiscalReferences()
	{
		AssertType<CusFiscalReferenceCollection<CusFiscalReference>>(GetInstruction().FiscalReferences);
	}

	public void TestJobDeclaration()
	{
		AssertType<JobDeclaration>(GetInstruction().JobDeclaration);
	}

	public void TestValidation()
	{
		var cusEntryInstruction = GetInstruction();
		var declaration = cusEntryInstruction.JobDeclaration;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportCusEntryInstructionValidation>("Import", cusEntryInstruction.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportCusEntryInstructionValidation>("Export", cusEntryInstruction.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<CusEntryInstructionValidation>("Miscellaneous", cusEntryInstruction.Validation);
		});
	}

	public void TestLookups() => AssertType<CusEntryInstructionLookups>(Factory.New<CusEntryInstruction>().Lookups);

	public void TestHasInvoiceLineStartWithProcedureCodeValue()
	{
		var cusEntryInstruction = GetInstruction();
		var declaration = cusEntryInstruction.JobDeclaration;
		AssertEquals(false, cusEntryInstruction.HasInvoiceLineStartWithProcedureCodeValue("71"));

		var invoice = declaration.Invoices.AddNew();
		var invoiceLines = invoice.InvoiceLines.AddNew();
		AssertEquals(false, cusEntryInstruction.HasInvoiceLineStartWithProcedureCodeValue("71"));

		invoiceLines.JI_CEI = cusEntryInstruction.PK;
		AssertEquals(false, cusEntryInstruction.HasInvoiceLineStartWithProcedureCodeValue("71"));

		invoiceLines.JI_Procedure = "71";
		AssertEquals(true, cusEntryInstruction.HasInvoiceLineStartWithProcedureCodeValue("71"));
	}

	public void TestHasOnlyOneGuarantee()
	{
		var cusEntryInstruction = GetInstruction();
		cusEntryInstruction.Guarantees.AddNew();
		AssertEquals(true, cusEntryInstruction.HasOnlyOneGuarantee());

		cusEntryInstruction.Guarantees.AddNew();
		AssertEquals(false, cusEntryInstruction.HasOnlyOneGuarantee());
	}

	public void TestSetGuaranteeAmount1()
	{
		var cusEntryInstruction = GetInstruction();
		var bondDetail = cusEntryInstruction.Guarantees.AddNew();
		bondDetail.PW_BondAmount = 9m;
		cusEntryInstruction.SetGuaranteeAmountIfNeeded();

		AssertEquals(0m, bondDetail.PW_BondAmount);
	}

	public void TestDefaultValues()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();

		CombineAssertions(() =>
		{
			AssertEquals("ZG_PostExportTransit", false, entryInstruction.ZG_PostExportTransit);
			AssertEquals("ZG_ExportManifest", false, entryInstruction.ZG_ExportManifest);
			AssertEquals("ZG_EADPrintOut", EadPrintOutList.Codes._0, entryInstruction.ZG_EADPrintOut);
			AssertEquals("CEI_DateForDuty", ZDateTime.Today, entryInstruction.CEI_DateForDuty);
		});
	}

	public void TestZG_TemporaryLocationCodeType_MaxLength()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals(4, entryInstruction.ZG_TemporaryLocationCodeTypeInfo.MaxLength);
	}

	public void TestZG_EADPrintOut_MaxLength()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals(1, entryInstruction.ZG_EADPrintOutInfo.MaxLength);
	}

	public void TestZG_TemporaryLocation_MaxLength()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entryInstruction.ZG_TemporaryLocationCodeType = string.Empty;
		AssertEquals(35, entryInstruction.ZG_TemporaryLocationInfo.MaxLength);

		entryInstruction.ZG_TemporaryLocationCodeType = TemporaryLocationCodeTypeList.Codes.CODE;
		AssertEquals(17, entryInstruction.ZG_TemporaryLocationInfo.MaxLength);

		entryInstruction.ZG_TemporaryLocationCodeType = TemporaryLocationCodeTypeList.Codes.DESC;
		AssertEquals(35, entryInstruction.ZG_TemporaryLocationInfo.MaxLength);
	}

	public void TestZG_SealsCountCaption()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals("Caption", "Total Seals Amount", DataBoundResourceStrings.GetDataForProperty(entryInstruction.ZG_SealsCountInfo).Caption);
	}

	public void TestIsExitSummary() => CombineAssertions(() =>
	{
		var cusEntryInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals("JobDeclaration is not defined", false, cusEntryInstruction.IsExitSummary);
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.Export;
		cusEntryInstruction.CEI_JE = declaration.PK;
		AssertEquals("JobDeclaration is not exit summary", false, cusEntryInstruction.IsExitSummary);
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("JobDeclaration is exit summary", true, cusEntryInstruction.IsExitSummary);
	});

	public void TestIsExportManifest()
	{
		var cusEntryInstruction = GetInstruction();
		cusEntryInstruction.ZG_ExportManifest = true;

		var declaration = cusEntryInstruction.JobDeclaration;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Empty Export", false, cusEntryInstruction.IsExportManifest);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.W;
			declaration.JE_LocationOfGoods = "ABC";
			declaration.JE_OfficeOfEntryExit = "CBA";
			AssertEquals("LocationOfGoods is different than OfficeOfEntryExit", false, cusEntryInstruction.IsExportManifest);

			declaration.JE_LocationOfGoods = "ABC";
			declaration.JE_OfficeOfEntryExit = "ABC";
			AssertEquals("Location of goods is same as OfficeOfEntryExit without customs office identifier", false, cusEntryInstruction.IsExportManifest);

			declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.V;
			declaration.JE_LocationOfGoods = "ABC";
			declaration.JE_OfficeOfEntryExit = "ABC";
			AssertEquals("IsExportManifest is true", true, cusEntryInstruction.IsExportManifest);

			cusEntryInstruction.ZG_ExportManifest = false;
			AssertEquals("Export ZG_ExportManifest is false", false, cusEntryInstruction.IsExportManifest);

			cusEntryInstruction.ZG_ExportManifest = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_LocationOfGoods = "ABC";
			declaration.JE_OfficeOfEntryExit = "ABC";
			AssertEquals("Import", false, cusEntryInstruction.IsExportManifest);
		});
	}

	public void TestDateForDutyIsObsolete()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		instruction.CEI_DateForDuty = ZDateTime.Empty;
		AssertEquals(true, instruction.DateForDutyIsObsolete);

		instruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-2);
		AssertEquals(true, instruction.DateForDutyIsObsolete);

		instruction.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
		AssertEquals(false, instruction.DateForDutyIsObsolete);

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		AssertNotNull(instruction.EntryHeader);

		entryHeader.EntryNumber = "Test";
		AssertEquals(true, entryHeader.HasBeenLodgedAtCustoms);
		instruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-2);
		AssertEquals(false, instruction.DateForDutyIsObsolete);
		instruction.CEI_DateForDuty = ZDateTime.Empty;
		AssertEquals(false, instruction.DateForDutyIsObsolete);
		instruction.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
		AssertEquals(false, instruction.DateForDutyIsObsolete);

		entryHeader.EntryNumber = ZString.Empty;
		AssertEquals(false, entryHeader.HasBeenLodgedAtCustoms);
		instruction.CEI_DateForDuty = ZDateTime.Empty;
		AssertEquals(true, instruction.DateForDutyIsObsolete);
		instruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-2);
		AssertEquals(true, instruction.DateForDutyIsObsolete);
		instruction.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
		AssertEquals(false, instruction.DateForDutyIsObsolete);
	}

	public void TestCEI_Procedure()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals(2, entryInstruction.CEI_ProcedureInfo.MaxLength);
	}

	public void TestDescriptionForDisplay()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		AssertEquals("DescriptionForDisplay", CusEntryInstruction.Schema.DescriptionForDisplay);
		AssertEquals("CPC  - ", instruction.DescriptionForDisplay);

		instruction.CEI_Procedure = "13";
		AssertEquals("CPC 13 - ", instruction.DescriptionForDisplay);

		instruction.CEI_Description = "someDescription";
		AssertEquals("CPC 13 - someDescription", instruction.DescriptionForDisplay);

		instruction.CEI_Procedure = ZString.Empty;
		AssertEquals("CPC  - someDescription", instruction.DescriptionForDisplay);
	}

	public void TestIsIE515BMessage()
	{
		var cusEntryInstruction = GetInstruction();
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = ZString.Empty;
			AssertEquals("When CEI_SubStyle is empty, the result is false", false, cusEntryInstruction.IsIE515BMessage);
			cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
			AssertEquals("when CEI_SubStyle matches the judgment, the result is true", true, cusEntryInstruction.IsIE515BMessage);
			cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
			AssertEquals("when CEI_SubStyle does not match the judgment, the result is false", false, cusEntryInstruction.IsIE515BMessage);
		});
	}

	public void TestSupportingDocuments()
	{
		var instruction = GetInstruction();
		CombineAssertions(() =>
		{
			AssertType<SupportingDocumentCollection>("SupportingDocumentCollection", instruction.SupportingDocuments);
			AssertEquals("CusSupportingInfoType", typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)instruction).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.SupportingDocument]);
		});
	}

	public void TestAdditionalInfos()
	{
		var instruction = GetInstruction();
		CombineAssertions(() =>
		{
			AssertType<AdditionalInfoCollection>("AdditionalInfoCollection", instruction.AdditionalInfos);
			AssertEquals("CusSupportingInfoType", typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)instruction).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		});
	}

	public void TestPreviousDocuments()
	{
		var instruction = GetInstruction();
		CombineAssertions(() =>
		{
			AssertType<PreviousDocumentCollection>("PreviousDocumentCollection", instruction.PreviousDocuments);
			AssertEquals("CusSupportingInfoType", typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)instruction).GetCusSupportingInfoTypes()[CusSupportingInfoTypeList.Codes.PreviousDocument]);
		});
	}

	public void TestIsSimplifiedEntryInstruction() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		var possibleTypes = new SharedJobMessageTypeList().GetAllCodes();
		var possibleSubTypes = typeof(SubStyleCodes)
			.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
			.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
			.Select(fi => (string)fi.GetRawConstantValue())
			.ToArray();
		foreach (var (type, subType) in possibleTypes.Zip(possibleSubTypes, (x, y) => (x, y)))
		{
			declaration.JE_MessageType = type;
			instruction.CEI_SubStyle = subType;
			if (declaration.IsExport && subType
				is SubStyleCodes.B
				or SubStyleCodes.E
				or SubStyleCodes.C
				or SubStyleCodes.F)
			{
				Assert($"Declaration type {type}, SubType {subType}", instruction.IsSimplifiedEntryInstruction);
			}
			else
			{
				AssertEquals($"Declaration type {type}, SubType {subType}", false, instruction.IsSimplifiedEntryInstruction);
			}
		}
	});

	public void TestIsSupplementary() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		var possibleSubTypes = typeof(SubStyleCodes)
							.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
							.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
							.Select(fi => (string)fi.GetRawConstantValue())
							.ToArray();
		foreach (var subType in possibleSubTypes)
		{
			instruction.CEI_SubStyle = subType;
			if (subType
				is SubStyleCodes.X
				or SubStyleCodes.Y)
			{
				Assert($"Declaration SubType {subType}", instruction.IsSupplementary);
			}
			else
			{
				AssertEquals($"Declaration SubType {subType}", false, instruction.IsSupplementary);
			}
		}
	});

	CusEntryInstruction GetInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		return declaration.CustomsEntryInstructions.AddNew();
	}
}
