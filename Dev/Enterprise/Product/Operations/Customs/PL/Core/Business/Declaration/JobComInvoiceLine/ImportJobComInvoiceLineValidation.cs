using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportJobComInvoiceLineValidation(JobComInvoiceLine parent) : JobComInvoiceLineValidation(parent)
{
	ZString JI_ProcedureProcedureCode => Parent.ProcedureCodeBase;

	IEnumerable<ZString> ConcessionCodes => Parent.ConcessionCodes;

	ZString CountryOfOrigin => Parent.JI_CountryOfOrigin;

	List<AdditionalProcedureCode> AdditionalProcedureCodesList => Parent.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().ToList();

	public override void ValidateAll()
	{
		base.ValidateAll();
		CheckRuleR1043();
		CheckRuleR1007();
	}

	protected override void CheckProcedureCodeBase()
	{
		base.CheckProcedureCodeBase();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.ProcedureCodeBaseInfo);

		var invLine = Parent;
		var propertyInfo = invLine.ProcedureCodeBaseInfo;
		var value = invLine.ProcedureCodeBase;

		if (value == ProcedureCodes._42 || value == ProcedureCodes._63)
		{
			bool fr1_found = false,
				fr2_found = false,
				fr3_found = false;
			foreach (CusFiscalReference fiscalReference in invLine.FiscalReferences)
			{
				fr1_found |= fiscalReference.CFR_Code == FiscalReferenceCodeList.Codes.FR1_Importer;
				fr2_found |= fiscalReference.CFR_Code == FiscalReferenceCodeList.Codes.FR2_Customer;
				fr3_found |= fiscalReference.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			}

			if (!fr2_found)
			{
				propertyInfo.AddMessageError(Res.GetString("17624A61-F5BB-4FB3-8BB2-09ED43B16373",
					"(R211/R412) Fiscal role code FR2 is missing."));
			}
			if (!fr1_found && !fr3_found)
			{
				propertyInfo.AddMessageError(Res.GetString("119573A3-2503-477B-BF73-1CB1E6A949D9",
					"(R411) Fiscal role code FR1 or FR3 is required for the requested procedure specified."));
			}
			else if (fr3_found && !invLine.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == SupportingDocumentCodes._3DK5))
			{
				propertyInfo.AddMessageError(Res.GetString("51E44CD8-B2F5-49D0-AA27-3D68FEAB8E8B",
					"(R417) The Supporting document code 3DK5 is required for the requested procedure specified with the Fiscal Role code FR3."));
			}
		}

		CheckForRuleR416(propertyInfo);
	}

	protected override void CheckPreviousProcedureCode()
	{
		base.CheckPreviousProcedureCode();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.PreviousProcedureCodeInfo);
	}

	#region AdditionalProcedureCodesAsString

	protected override void CheckAdditionalProcedureCodesAsString()
	{
		base.CheckAdditionalProcedureCodesAsString();

		CheckForRuleR422();
	}

	void CheckForRuleR422()
	{
		if (JI_ProcedureProcedureCode == ProcedureCodes._53)
		{
			var itemList = new List<ZString>() { Constants.ConcessionCodes.D51 };
			for (int i = 1; i <= 30; i++)
			{
				itemList.Add($"D{i.ToString().PadLeft(2, '0')}");
			}

			if (!DoesAdditionalProcedureCodeContainsAnyConcessionCodeFromList(itemList))
			{
				Parent.AdditionalProcedureCodesAsStringInfo.AddMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|R422", "R422 – for CPC=53 the Additional Procedure code from the list (D01..D30, D51) is required."));
			}
		}
	}
	#endregion

	#region JI_CountryOfOrigin

	protected override void CheckJI_CountryOfOrigin()
	{
		base.CheckJI_CountryOfOrigin();
		if (!CountryOfOrigin.IsEmpty)
		{
			CheckRuleR1585();
		}
	}

	void CheckRuleR1585()
	{
		var euMembersProvider = ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>();
		if (euMembersProvider.IsInEuropeanCustomsUnion(Parent.JI_CountryOfOrigin))
		{
			Parent.JI_CountryOfOriginInfo.AddWarning(Res.GetString("PLImportJobComInvoiceLineValidation|R1585", "(R1585) The EU code will be used in the customs declaration"));
		}
	}

	#endregion

	#region JI_Tariff

	protected override void CheckJI_Tariff()
	{
		if (Parent.JI_Tariff.IsEmpty)
		{
			CheckForRuleR436();
		}
		else
		{
			base.CheckJI_Tariff();
		}
	}

	void CheckForRuleR436()
	{
		if (!DoesAdditionalProcedureCodeContainsConcessionCode(Constants.ConcessionCodes._2PL) &&
			(JI_ProcedureProcedureCode != ProcedureCodes._76))
		{
			Parent.JI_TariffInfo.AddMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|R436", "R436 - Tariff Code is mandatory."));
		}
	}

	#endregion

	#region JI_Procedure

	protected override void CheckJI_Procedure()
	{
		base.CheckJI_Procedure();
		if (!Parent.JI_Procedure.IsEmpty)
		{
			CheckJI_ProcedureLengthIsCorrect();
			CheckForRuleR240();
			CheckForRuleR860();
		}

		CheckForRuleR416(Parent.JI_ProcedureInfo);
	}

	void CheckJI_ProcedureLengthIsCorrect()
	{
		if ((Parent.JI_Procedure.Length != 4) &&
			(AdditionalProcedureCodesList.Any(x => x.CY_Code.Length == 4)))
		{
			Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|InvalidCPCLength", "CPC length must be 4 characters long."));
		}
	}

	void CheckForRuleR240()
	{
		if (JI_ProcedureProcedureCode == ProcedureCodes._49 &&
			!GetAllAdditionalInfosCSI_Codes().Contains(AdditionalInfoCodes._4PL04))
		{
			Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|R240", "R240 - Missing Additional Information Code for ‘4PL04’ for customs procedure requested ‘49’."));
		}
	}

	void CheckForRuleR416(ZPropertyInfo procedureInfo)
	{
		if (IsGoodsDestinationEmpty() && !HasValidProcedure())
		{
			procedureInfo.AddMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|R416", "(R416) – The Customs Procedure Detail code is not valid because the declaration doesn't have a goods destination country code."));
		}

		bool HasValidProcedure()
		{
			return ConcessionCodes.Any(IsValidR416ConcessionCode);
		}

		bool IsGoodsDestinationEmpty() => Parent.Declaration?.JE_GoodsDestination.IsEmpty ?? false;

		bool IsValidR416ConcessionCode(ZString concessionCode) => concessionCode == Constants.ConcessionCodes._2PL;
	}

	void CheckForRuleR860()
	{
		var csiCodes = GetAllAdditionalInfosCSI_Codes();
		if (csiCodes.Contains(AdditionalInfoCodes._00100) &&
			JI_ProcedureProcedureCode == ProcedureCodes._71)
		{
			Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("PLAdditionalInfo|R860", "R860 - Invalid Additional Information Code 00100 for customs procedure requested ‘71’."));
		}
	}

	#endregion

	#region JI_CEI

	protected override void CheckJI_CEI()
	{
		CheckRuleR229();
	}

	void CheckRuleR229()
	{
		var procedureCode = JI_ProcedureProcedureCode;
		if ((procedureCode == ProcedureCodes._45 || procedureCode == ProcedureCodes._68 || procedureCode == ProcedureCodes._96 || DoesAdditionalProcedureCodeContainsConcessionCode(Constants.ConcessionCodes.F06))
			&& Parent.EntryInstruction is CusEntryInstruction instruction
			&& (instruction.CEI_SubStyle != SubStyleCodes.C || !AdditionalInfoCodeExists(AdditionalInfoCodes._4PL12))
			&& !SupportingDocumentExists(SupportingDocumentCodes.C651))
		{
			Parent.JI_CEIInfo.AddMessageError(Res.GetString("ImportJobComInvoiceLineValidation|CheckRuleR229|MissingSupportingDocument", "(R229) C651 Supporting document is missing or e-AD position number for C651 is invalid."));
		}
	}

	bool AdditionalInfoCodeExists(ZString additionalInfoCode) => Parent.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == additionalInfoCode) ||
																(Parent.Declaration?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == additionalInfoCode) ?? false) ||
																(Parent.InvoiceHeader?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == additionalInfoCode) ?? false);

	bool SupportingDocumentExists(ZString supportingDocumentCode) => Parent.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == supportingDocumentCode && SupportingDocumentC651DoesNotHaveValidDescription(x.CSI_Description)) ||
																	(Parent.InvoiceHeader?.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == supportingDocumentCode && SupportingDocumentC651DoesNotHaveValidDescription(x.CSI_Description)) ?? false) ||
																	(Parent.Declaration?.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == supportingDocumentCode && SupportingDocumentC651DoesNotHaveValidDescription(x.CSI_Description)) ?? false);

	static bool SupportingDocumentC651DoesNotHaveValidDescription(ZString supportingDocumentDescription) => !supportingDocumentDescription.IsEmpty && supportingDocumentDescription.IsNumbersOnlyOrEmpty;

	#endregion

	#region JI_ValuationCode

	protected override void CheckJI_ValuationCode()
	{
		base.CheckJI_ValuationCode();
		CheckRuleR916();
	}

	void CheckRuleR916()
	{
		if (Parent.JI_ValuationCode == Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1 &&
			Parent.InvoiceHeader is JobComInvoiceHeader invoiceHeader &&
			invoiceHeader.JZ_ValuationCode == InvoiceHeaderValuationCodes._11 &&
			Parent.EntryInstruction is CusEntryInstruction entryInstruction
			)
		{
			var procedure = entryInstruction.CEI_Procedure;
			if ((procedure.StartsWith("4") || procedure.StartsWith("6")) &&
				!(FindN935InSupportingDocuments(Parent.SupportingDocuments) ||
				FindN935InSupportingDocuments(invoiceHeader.SupportingDocuments) ||
				(Parent.Declaration is JobDeclaration declaration && FindN935InSupportingDocuments(declaration.SupportingDocuments))))
			{
				Parent.JI_ValuationCodeInfo.AddMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|CheckRuleR916",
					"Supporting document code N935 is required (Invoice number)."));
			}
		}
	}

	static bool FindN935InSupportingDocuments(SupportingDocumentCollection supportingDocuments) => supportingDocuments.Cast<SupportingDocument>()
		.Any(x => x.CSI_Code == EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935);

	#endregion

	protected override void CheckJI_OA_ExporterAddress()
	{
		base.CheckJI_OA_ExporterAddress();

		var parent = Parent;

		if (parent.ExporterAddress?.Header is OrgHeader orgHeader)
		{
			PLOrgHeaderValidationHelper.ValidateOrganizationName(orgHeader, parent.JI_OA_ExporterAddressInfo);
		}
	}

	protected override void CheckJI_OA_ConsigneeAddress()
	{
		base.CheckJI_OA_ConsigneeAddress();

		var parent = Parent;

		if (parent.ConsigneeAddress?.Header is OrgHeader orgHeader)
		{
			PLOrgHeaderValidationHelper.ValidateOrganizationName(orgHeader, parent.JI_OA_ConsigneeAddressInfo);
		}
	}

	bool DoesAdditionalProcedureCodeContainsConcessionCode(ZString concession) => ConcessionCodes.Any(x => x == concession);

	bool DoesAdditionalProcedureCodeContainsAnyConcessionCodeFromList(List<ZString> itemList) => ConcessionCodes.Any(itemList.Contains);

	List<ZString> GetAllAdditionalInfosCSI_Codes()
	{
		var result = new List<ZString>();

		AddAdditionalInfosCodesToList(result, Parent.AdditionalInfos);
		AddAdditionalInfosCodesToList(result, Parent.InvoiceHeader?.AdditionalInfos);
		AddAdditionalInfosCodesToList(result, Parent.Declaration?.AdditionalInfos);

		return result;
	}

	static void AddAdditionalInfosCodesToList(List<ZString> list, AdditionalInfoCollection additionalInfo)
	{
		var listToAdd = additionalInfo?.Select(x => ((AdditionalInfo)x).CSI_Code).ToList();
		if (listToAdd != null)
		{
			list.AddRange(listToAdd);
		}
	}

	protected override void CheckJI_ValuationDateOverrideIsValidZDateTime()
	{
		base.CheckJI_ValuationDateOverrideIsValidZDateTime();

		var valuationDateOverride = Parent.JI_ValuationDateOverride;
		if (valuationDateOverride.IsEmpty
			&& AnyEntryInstructionValuationDateIsNotEmpty())
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ValuationDateOverrideInfo);
		}
	}

	bool AnyEntryInstructionValuationDateIsNotEmpty() => Parent.EntryInstruction?.AnyInvoiceLineValuationDateIsNotEmpty ?? false;

	protected override void CheckJI_PrimaryPreference()
	{
		base.CheckJI_PrimaryPreference();
		CheckRuleR425();
		CheckRuleR1502();
	}

	void CheckRuleR425()
	{
		if (!Parent.JI_ConcessionOrder.IsEmpty
			&& !PrimaryCodeIsOnR425AndR481PrimaryPreferenceCodeList(Parent.JI_PrimaryPreference))
		{
			Parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|CheckRuleR425", "(R425) for Quota only following preference codes are allowed 120, 123, 125, 128, 220, 223, 225, 320, 323, 325, 420"));
		}
	}

	void CheckRuleR1502()
	{
		if (IsPrimaryPreferenceStartsWith2Or3Or4()
			&& IsCountryOfOriginNotCountryOfSuppy()
			&& IsProcedureCodeStartsWith4Or6()
			&& !GetAllAdditionalInfosCSI_Codes().Contains(AdditionalInfoCodes._1PL17))
		{
			Parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|CheckRuleR1502", "(R1502)The condition of direct import is not met"));
		}

		bool IsProcedureCodeStartsWith4Or6()
		{
			var entryInstructionProcedure = Parent.EntryInstruction?.CEI_Procedure ?? ZString.Empty;
			return entryInstructionProcedure.StartsWith("4") || entryInstructionProcedure.StartsWith("6");
		}

		bool IsPrimaryPreferenceStartsWith2Or3Or4()
		{
			var primaryPreference = Parent.JI_PrimaryPreference;
			return primaryPreference.StartsWith("2") || primaryPreference.StartsWith("3") || primaryPreference.StartsWith("4");
		}

		bool IsCountryOfOriginNotCountryOfSuppy() => CountryOfOrigin != (Parent.ZG_CountryOfSupply.IsEmpty ? (Parent.Declaration?.JE_GoodsOrigin ?? ZString.Empty) : Parent.ZG_CountryOfSupply);
	}

	protected override void CheckJI_ConcessionOrder()
	{
		base.CheckJI_ConcessionOrder();
		CheckRuleR481();
	}

	void CheckRuleR481()
	{
		if (Parent.JI_ConcessionOrder.IsEmpty
			&& PrimaryCodeIsOnR425AndR481PrimaryPreferenceCodeList(Parent.JI_PrimaryPreference))
		{
			Parent.JI_ConcessionOrderInfo.AddMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|CheckRuleR481", "(R481) for the given preference code [36], [39] Quota number is required"));
		}
	}

	bool PrimaryCodeIsOnR425AndR481PrimaryPreferenceCodeList(ZString primaryCode) => ValidationLists.R425AndR481PrimaryPreferenceCodeList(Parent.Factory).Contains(primaryCode);

	protected override void CheckJI_MarkModel()
	{
		if (Parent.JI_MarkModel.IsEmpty)
		{
			if (Parent.FirstVehicle.IsCarDetailsDataRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_MarkModelInfo);
			}
		}
		else
		{
			if (Parent.MarkModel == null)
			{
				Parent.JI_MarkModelInfo.AddMessageError(Res.GetString("PLJobComInvoiceLineValidation|InvalidMakeModel",
					"Invalid {0} name.", Parent.JI_MarkModelInfo.HumanReadableName));
			}
		}
	}

	protected void CheckRuleR1007()
	{
		var parent = Parent;
		if (parent.EntryInstruction is CusEntryInstruction instruction)
		{
			if (EntryInstructionHasRuleSupportingDocument(instruction)
				&& !parent.EffectiveAdditionalInfos().Cast<AdditionalInfo>().Any(IsRuleAdditionalInfo)
				&& instruction.AllInstructionAdditionalInfos.Any(IsRuleAdditionalInfo))
			{
				parent.AddRowMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|CheckRuleR1007",
					"(R1007) – An additional info code 0PL12 is required for each invoice line (it is recommended to add the additional information code 0PL12 in the invoice header or in Declaration/Misc)."));
			}
		}

		bool EntryInstructionHasRuleSupportingDocument(CusEntryInstruction entryInstruction) =>
			entryInstruction.AllInstructionSupportingDocuments.Any(x =>
			{
				var code = x.CSI_Code;
				return code == SupportingDocumentCodes.C512 || code == SupportingDocumentCodes.C513 || code == SupportingDocumentCodes.C514;
			});

		bool IsRuleAdditionalInfo(AdditionalInfo x) => x.CSI_Code == AdditionalInfoCodes._0PL12;
	}

	protected void CheckRuleR1043()
	{
		var declarationHas00500AddInfo = Parent.Declaration.AllAdditionalInfos.Any(x => x.CSI_Code == AdditionalInfoCodes._00500);
		var invoiceHas00500AddInfo = Parent.EffectiveAdditionalInfos().Cast<AdditionalInfo>().Any(x => x.CSI_Code == AdditionalInfoCodes._00500);
		if (declarationHas00500AddInfo && !invoiceHas00500AddInfo)
		{
			Parent.AddRowMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|CheckRuleR1043",
				"(R1043) – An additional info code 00500 is required for each invoice line (it is recommended to add the additional information code 00500 in the invoice header or in Declaration/Misc)."));
		}
	}

	protected override void CheckJI_DateForDutyOverride()
	{
		base.CheckJI_DateForDutyOverride();

		var dateForDuty = Parent.JI_DateForDutyOverride;
		if (!DutyCalculationHelper.HasCUDExchangeRateOrSpecifiedDateIsInvalid(Parent.Factory, Parent.JI_DateForDutyOverride))
		{
			Parent.JI_DateForDutyOverrideInfo.AddMessageError(Res.GetString("PLImportJobComInvoiceLineValidation|CheckJI_DateForDutyOverride"
				, "The EUR (CUD type) exchange rate for {0} is missing. No duty / taxes calculation can be done.", dateForDuty.ToShortDateString()));
		}
	}
}
