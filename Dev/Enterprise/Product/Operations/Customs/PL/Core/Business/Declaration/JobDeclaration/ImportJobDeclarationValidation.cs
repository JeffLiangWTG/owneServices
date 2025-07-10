using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class ImportJobDeclarationValidation(JobDeclaration parent) : JobDeclarationValidation(parent)
{
	#region GoodsLocationCustomsOffice

	protected override void CheckGoodsLocationCustomsOffice()
	{
		base.CheckGoodsLocationCustomsOffice();
		CheckRuleR1513();
	}

	void CheckRuleR1513()
	{
		var declaration = Parent;
		var goodsLocationCustomsOffice = declaration.GoodsLocationCustomsOffice;

		if (!goodsLocationCustomsOffice.IsEmpty
			&& goodsLocationCustomsOffice.SubstringSafe(0, 4) != declaration.JE_CustomsOffice.SubstringSafe(0, 4)
			&& declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.EffectiveAdditionalInfos().All(y => y.CSI_Code != AdditionalInfoCodes._0PL12)))
		{
			declaration.GoodsLocationCustomsOfficeInfo.AddMessageError(Res.GetString(
				"PLImportJobDeclarationValidation|CheckRuleR1513",
				"(R1513) – An additional info code 0PL12 is required for each invoice line (it is recommended to add the additional information code 0PL12 in the invoice header or in Declaration/Misc)."));
		}
	}

	#endregion

	protected override void CheckJE_VoyageFlightNo()
	{
		base.CheckJE_VoyageFlightNo();

		var declaration = Parent;
		if (declaration.IsAir)
		{
			ImportJobDeclarationValidationHelper.CheckRuleR489((ZPropertyInfoString)declaration.JE_VoyageFlightNoInfo, declaration);
		}
	}

	protected override void CheckJE_VesselName()
	{
		base.CheckJE_VesselName();

		var declaration = Parent;
		if (!declaration.IsMail
			&& !declaration.IsFixedInstallation
			&& (!declaration.IsSea
				|| declaration.ZG_BorderTransportMeans != ImportEUBorderTransportMeansList.Codes.Ship))
		{
			ImportJobDeclarationValidationHelper.CheckRuleR489((ZPropertyInfoString)declaration.JE_VesselNameInfo, declaration);
		}
		ImportJobDeclarationValidationHelper.CheckRuleR233((ZPropertyInfoString)declaration.JE_VesselNameInfo);
	}

	protected override void CheckJE_LloydsIMO()
	{
		base.CheckJE_LloydsIMO();

		var declaration = Parent;
		if (declaration.IsSea
			&& declaration.ZG_BorderTransportMeans == ImportEUBorderTransportMeansList.Codes.Ship)
		{
			ImportJobDeclarationValidationHelper.CheckRuleR489((ZPropertyInfoString)declaration.JE_LloydsIMOInfo, declaration);
		}
	}

	protected override void CheckJE_GoodsOrigin()
	{
		var parent = Parent;
		base.CheckJE_GoodsOrigin();
		UCC6CheckForRuleR261();
		UCC6CheckForRuleR433();
		if (!parent.JE_EntryStyle.IsEmpty && parent.JE_EntryStyle != EntryStyleListImport.Codes.ImportFromSpecialTerritory)
		{
			UCC6CheckForRuleR254();
			UCC6CheckForRuleR431();
		}
	}

	void UCC6CheckForRuleR254()
	{
		var parent = Parent;
		if (parent.JE_GoodsOrigin == Core.Constants.CountryCodes.Poland)
		{
			parent.JE_GoodsOriginInfo.AddMessageError(Res.GetString("8e66837b-992d-4100-9112-4515792ff472", "(R254) – For the selected Entry Style code, the country of Export/Goods Origin must be other than PL."));
		}
	}

	void UCC6CheckForRuleR431()
	{
		var parent = Parent;
		if (IsEU(parent.JE_GoodsOrigin) && parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => CheckPerviousProcedureCode(x.PreviousProcedureCode)))
		{
			parent.JE_GoodsOriginInfo.AddMessageError(Res.GetString("22d87eb6-c4f3-49c5-a919-d61c9f18cd24", "(R431) – For the selected Entry Style code and previous procedure code, the country of Export/Goods Origin must be other than EU."));
		}

		bool CheckPerviousProcedureCode(ZString previousProcedure) => previousProcedure != Constants.ProcedureCodes._51 && previousProcedure != Constants.ProcedureCodes._53 && previousProcedure != Constants.ProcedureCodes._71;

		bool IsEU(ZString countryCode) => ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(countryCode);
	}

	void UCC6CheckForRuleR261()
	{
		var parent = Parent;
		if (parent is JobDeclaration jobDeclaration && DeclarationBreaksR261(jobDeclaration))
		{
			parent.JE_GoodsOriginInfo.AddMessageError(Res.GetString("75cd735e-7a78-4f26-9521-8a8f6dd45682", "(R261) – For the selected Entry Style code, if the country of Export/Goods Origin must be other than CH, NO, IS, LI, TR, MK, XS, RS."));
		}

		bool DeclarationBreaksR261(JobDeclaration declaration)
		{
			return declaration != null
					&& EntryStyleIsNotEu(declaration)
					&& (IsCountryCodeInvalid(declaration.JE_GoodsOrigin) || declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(IsExportInvalid));
		}

		bool EntryStyleIsNotEu(JobDeclaration declaration)
		{
			var entryStyle = declaration?.JE_EntryStyle ?? ZString.Empty;
			return !entryStyle.IsEmpty && entryStyle != EntryStyleListImport.Codes.ImportFromEFTAMember;
		}

		bool IsExportInvalid(JobComInvoiceLine invoiceLine) => IsCountryCodeInvalid(invoiceLine?.ZG_CountryOfSupply ?? ZString.Empty);

		bool IsCountryCodeInvalid(string countryCode)
		{
			return countryCode == Core.Constants.CountryCodes.Switzerland
					|| countryCode == Core.Constants.CountryCodes.Norway
					|| countryCode == Core.Constants.CountryCodes.Iceland
					|| countryCode == Core.Constants.CountryCodes.Liechtenstein
					|| countryCode == Core.Constants.CountryCodes.Turkey
					|| countryCode == Core.Constants.CountryCodes.Macedonia
					|| countryCode == Core.Constants.CountryCodes.Serbia_ForEUTrading
					|| countryCode == Core.Constants.CountryCodes.Serbia;
		}
	}

	void UCC6CheckForRuleR433()
	{
		var parent = Parent;
		if (parent is JobDeclaration declaration && declaration.JE_GoodsOrigin.IsEmpty && declaration.InvoiceLines.Any())
		{
			parent.JE_GoodsOriginInfo.AddMessageError(Res.GetString("92451c22-85e5-447d-872a-280e830d2388", "(R433) – Country of export code is required."));
		}
	}

	protected override void CheckJE_DeclarantType()
	{
		base.CheckJE_DeclarantType();

		CheckRuleR1509();
	}

	void CheckRuleR1509()
	{
		var declaration = Parent;
		if (declaration.JE_DeclarantType == PLRepresentationTypeList.Codes._5Indirect
			&& declaration.Representative?.Header is OrgHeader representative
			&& representative.OH_Category == Constants.OrgHeaderCategory.BUS)
		{
			bool? declarationHaveN990Document = null;
			if (declaration.CustomsEntryInstructions.Any(instruction =>
				{
					var ceiProcedure = instruction.CEI_Procedure;
					return IsCeiProcedureR1509_51Or53(ceiProcedure)
							|| (IsCeiProcedureR1509_40Or42Or45(ceiProcedure)
								&& (DeclarationHaveN990Document() || InstructionN990SupportingDocumentExists(instruction)));
				})
				)
			{
				declaration.JE_DeclarantTypeInfo.AddMessageError(Res.GetString("ImportJobDeclarationValidation|CheckRuleR1509", "(R1509) - Selected Representation Type is not allowed."));
			}

			bool DeclarationHaveN990Document() => (declarationHaveN990Document ?? (declarationHaveN990Document = HaveSupportingDocumentN990(declaration.SupportingDocuments))).Value;
		}

		bool InstructionN990SupportingDocumentExists(CusEntryInstruction instruction)
		{
			return instruction.Invoices.Any(invoice => HaveSupportingDocumentN990(invoice.SupportingDocuments)
														|| instruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(line => HaveSupportingDocumentN990(line.SupportingDocuments)));
		}

		bool IsCeiProcedureR1509_51Or53(ZString ceiProcedure) => ceiProcedure == Constants.ProcedureCodes._51 || ceiProcedure == Constants.ProcedureCodes._53;

		bool IsCeiProcedureR1509_40Or42Or45(ZString ceiProcedure) => ceiProcedure == Constants.ProcedureCodes._40 || ceiProcedure == Constants.ProcedureCodes._42 || ceiProcedure == Constants.ProcedureCodes._45;

		bool HaveSupportingDocumentN990(EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection supportingDocumentCollection) =>
			supportingDocumentCollection.Cast<SupportingDocument>().Any(document => document.CSI_Code == Constants.SupportingDocumentCodes.N990);
	}

	protected override void CheckJE_LocationOfGoods()
	{
		base.CheckJE_LocationOfGoods();
		var declaration = Parent;
		if (!declaration.JE_LocationQualifier.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(declaration.JE_LocationOfGoodsInfo);
		}
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();

		var parent = Parent;

		if (parent.DeclarantAddress?.Header is OrgHeader orgHeader)
		{
			PLOrgHeaderValidationHelper.ValidateOrganizationName(orgHeader, parent.JE_OA_DeclarantAddressInfo);
		}
	}

	protected override void CheckJE_OA_Representative()
	{
		base.CheckJE_OA_Representative();

		var parent = Parent;

		if (parent.Representative?.Header is OrgHeader orgHeader)
		{
			PLOrgHeaderValidationHelper.ValidateOrganizationName(orgHeader, parent.JE_OA_RepresentativeInfo);
		}
	}

	protected override void CheckJE_OA_SellerAddress()
	{
		base.CheckJE_OA_SellerAddress();

		var parent = Parent;

		if (parent.SellerAddress?.Header is OrgHeader orgHeader)
		{
			PLOrgHeaderValidationHelper.ValidateOrganizationName(orgHeader, parent.JE_OA_SellerAddressInfo);
		}
	}

	protected override void CheckJE_PaymentMethod()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_PaymentMethodInfo);
	}

	protected override void CheckJE_TransportModeInland()
	{
		base.CheckJE_TransportModeInland();
		var declaration = Parent;
		if (!declaration.ZG_Box18TransportID.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(declaration.JE_TransportModeInlandInfo);
		}
	}

	protected override void CheckJE_TransportIDInland()
	{
		base.CheckJE_TransportIDInland();
		ImportJobDeclarationValidationHelper.CheckRuleR208((ZPropertyInfoString)Parent.JE_TransportIDInlandInfo);
	}

	protected override void CheckJE_Trailer1RegNo()
	{
		base.CheckJE_Trailer1RegNo();
		ImportJobDeclarationValidationHelper.CheckRuleR208((ZPropertyInfoString)Parent.JE_Trailer1RegNoInfo);
	}

	protected override void CheckJE_Trailer2RegNo()
	{
		base.CheckJE_Trailer2RegNo();
		ImportJobDeclarationValidationHelper.CheckRuleR208((ZPropertyInfoString)Parent.JE_Trailer2RegNoInfo);
	}
}
