using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using static Enterprise.Customs.PL.Business.Constants;
using CusSupportingInfoType = Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes;
using UniversalReferenceCusSupportingInfoTypes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.CusSupportingInfoTypes;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportAdditionalInfoValidation(AdditionalInfo parent) : AdditionalInfoValidation(parent)
{
	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
		CheckRuleR0038E();
		Utils.RuleR0091EForSCOPurpose((JobDeclaration)Parent.Declaration, Parent);
		CheckRuleG0825();
	}

	protected override void CheckCSI_SubType()
	{
		base.CheckCSI_SubType();

		var parent = Parent;
		var subTypeInfo = parent.CSI_SubTypeInfo;
		if (parent.CSI_SubType.IsEmpty)
		{
			subTypeInfo.AddMessageError(Res.GetString("PLExportAdditionalInfoValidation|CSI_SubTypeEmptyMessageError",
				"Please enter a Kind of Document."));
		}

		ListValidation.MessageErrorIfInvalidCode(subTypeInfo);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		var parent = Parent;
		var refNumberInfo = parent.CSI_ReferenceNumberInfo;
		if (parent.CSI_SubType == AdditionalInfoKindList.Codes.TRA && parent.CSI_ReferenceNumber.IsEmpty)
		{
			refNumberInfo.AddMessageError(Res.GetString("PLExportAdditionalInfoValidation|CSI_ReferenceNumberEmptyMessageError",
				"A reference number is required."));
		}

		CheckRuleR0101E();
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		CheckRuleR0029E();
		CheckRuleR0041E();
		CheckRuleR0085E();
		CheckRuleR0100E();
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();

		CheckRuleR0083E();
		CheckRuleR0097E();
		CheckRuleR0039E();
	}

	void CheckRuleR0097E()
	{
		var parent = Parent;

		if (parent.CSI_Type == CusSupportingInfoType.AdditionalInfo
			&& parent.CSI_SubType == AdditionalInfoKindList.Codes.INF
			&& parent.CSI_Code == AdditionalInfoCodes._EXP04
			&& (parent.CSI_Description.Length != 2 || !parent.CSI_Description.IsNumbersOnlyOrEmpty))
		{
			parent.CSI_DescriptionInfo.AddMessageError(Res.GetString("3DBD2C38-07D2-41D2-A121-9EEC62A66DEC",
				"[R0097E] Description must contain only two digits if Additional Documents contain Kind = ‘INF’ and Full Type = ‘EXP04’ code"));
		}
	}

	void CheckRuleR0029E()
	{
		var parent = Parent;
		if (parent.CSI_Code == AdditionalInfoCodes._00100
			&& parent.HasAuthorizationUsageCode(CusAuthorizationUsageType.C019))
		{
			parent.CSI_CodeInfo.AddMessageError(Res.GetString("PLExportAdditionalInfoValidation|CheckRuleR0029E",
				"(R0029E) The Additional Information code '00100' is not allowed with Authorization code 'C019'"));
		}
	}

	void CheckRuleR0038E()
	{
		var parent = Parent;
		if (parent.CSI_SubType != AdditionalInfoKindList.Codes.INF || parent.CSI_Code != AdditionalInfoCodes._EXP15)
		{
			return;
		}

		if (parent.Parent is CusEntryInstruction entryInstruction && entryInstruction.InvoiceLines.Any(line => HasDuplicateAdditionalInfo(line.EffectiveAdditionalInfos()))
			|| parent.Parent is JobComInvoiceHeader invoice && invoice.InvoiceLines.Cast<JobComInvoiceLine>().Any(line => HasDuplicateAdditionalInfo(line.EffectiveAdditionalInfos()))
			|| parent.Parent is JobComInvoiceLine invoiceLine && HasDuplicateAdditionalInfo(invoiceLine.EffectiveAdditionalInfos()))
		{
			parent.AddRowMessageError(Res.GetString("51D95540-E597-447B-A4D7-9383C228FA57",
				"[R0038E] Only one Additional Document with Kind = ’INF’ and Type = ‘EXP15’ is allowed either on Entry Instruction, Invoice Headers or Invoice Lines."));
		}

		return;

		bool HasDuplicateAdditionalInfo(IEnumerable<Integration.Customs.ICusSupportingInfo> additionalInfos)
		{
			var equalityComparer = new LambdaComparer<Integration.Customs.ICusSupportingInfo>(
				equals: (lhsInfo, rhsInfo) => lhsInfo.CSI_SubType.Equals(rhsInfo.CSI_SubType) && lhsInfo.CSI_Code.Equals(rhsInfo.CSI_Code),
				getHashcode: info => HashCodeHelper.GetCompositeHashCode(33, [info.CSI_SubType, info.CSI_Code]));
			return additionalInfos.Except([parent]).Contains(parent, equalityComparer);
		}
	}

	void CheckRuleR0083E()
	{
		var parent = Parent;
		if (parent.CSI_SubType == AdditionalInfoKindList.Codes.INF
			&& (parent.CSI_Code == AdditionalInfoCodes._POW01 || parent.CSI_Code == AdditionalInfoCodes._PCS01)
			&& parent.CSI_Description.IsEmpty)
		{
			parent.CSI_DescriptionInfo.AddMessageError(Res.GetString("F9AF8455-C17B-4FA8-8660-85D5CCD49ED0",
				"[R0083E] Description is required for Additional Information Type ‘POW01’ or ‘PCS01’ and must contain IDSISC or email address."));
		}
	}

	void CheckRuleR0041E()
	{
		var parent = Parent;
		if (parent.CSI_Code != AdditionalInfoCodes._EXP15
			|| parent.Parent is not CusEntryInstruction { JobDeclaration: { DeclarantAddress: not null, ExporterDocAddress.Address: not null } declaration })
		{
			return;
		}

		var declarantEori = AddressHelper.GetEORIForOrganisationAddress(declaration.DeclarantAddress);
		var exporterEori = AddressHelper.GetEORIForOrganisationAddress(declaration.ExporterDocAddress.Address);
		if (!declarantEori.IsEmpty
			&& declarantEori == exporterEori)
		{
			parent.CSI_CodeInfo.AddMessageError(Res.GetString("2613A424-1AED-4311-BA3C-D0E57D018611",
				"[R0041E] When Exporter and Declarant both have same EORI then Additional Information with Type = 'EXP15' can't be declared."));
		}
	}

	void CheckRuleR0085E()
	{
		var parent = Parent;
		if (parent.Parent is JobComInvoiceHeader invoiceHeader
			&& parent.CSI_Code == AdditionalInfoCodes._1PL18
			&& parent.CSI_SubType == AdditionalInfoKindList.Codes.INF
			&& invoiceHeader.CusEntryInstructions.Any(HasSubStyleVOrZ)
			&& (!invoiceHeader.PreviousDocuments.Any() || !invoiceHeader.HasPreviousDocumentCode(PreviousDocumentCodes.NCLE)))
		{
			parent.CSI_CodeInfo.AddMessageError(Res.GetString("1e6d4263-fa09-4569-8987-0a93130b4811",
				"You have not entered Previous Document Type [NCLE]."));
		}

		bool HasSubStyleVOrZ(Customs.Business.CusEntryInstruction entryInstruction) =>
			entryInstruction.CEI_SubStyle == SubStyleCodes.V
			|| entryInstruction.CEI_SubStyle == SubStyleCodes.Z;
	}

	void CheckRuleR0100E()
	{
		var parent = Parent;
		if (parent.Parent is JobComInvoiceLine invoiceLine &&
			parent.CSI_Type == CusSupportingInfoType.AdditionalInfo &&
			parent.CSI_SubType == AdditionalInfoKindList.Codes.REF &&
			IsY121orY798(parent) &&
			invoiceLine.AdditionalInfos.Cast<AdditionalInfo>().Except(parent)
				.FirstOrDefault(IsY121orY798) is { } another)
		{
			parent.CSI_CodeInfo.AddMessageError(
				(parent.CSI_Code.ToString(), another.CSI_Code.ToString()) switch
				{
					(UniversalReferenceCusSupportingInfoTypes.Y121, UniversalReferenceCusSupportingInfoTypes.Y121)
						=> Res.GetString("9311C5BA-0088-4A05-A458-F9468F9004BD",
							"[R0100E]: You have entered more than one Additional Reference (Kind = 'REF') with Full Type = 'Y121'."),

					(UniversalReferenceCusSupportingInfoTypes.Y798, UniversalReferenceCusSupportingInfoTypes.Y798)
						=> Res.GetString("C20932D-C399-4F16-9EB2-1D423B7DAF4B",
							"[R0100E]: You have entered more than one Additional Reference (Kind = 'REF') with Full Type = 'Y798'."),

					_ => Res.GetString("8A69A64A-A97E-4052-AC6D-5293672BC45A",
							"[R0100E]: You have entered Additional Reference (Kind = 'REF') with Full Type = ('Y121' as well as 'Y798')."),
				});
		}
	}

	void CheckRuleR0101E()
	{
		var parent = Parent;
		if (parent.Parent is JobComInvoiceLine &&
			parent.CSI_Type == CusSupportingInfoType.AdditionalInfo &&
			parent.CSI_SubType == AdditionalInfoKindList.Codes.REF &&
			parent.CSI_ReferenceNumber.IsEmpty &&
			IsY121orY798(parent))
		{
			parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("59A55D30-3A00-47CA-B233-79E8E360B049",
				"[R0101E]: For codes Y121 and Y798, the quantity (always expressed in TCE for Y121 (FGAS) and in KGM for Y798 (ODS)) must be entered in the \"Reference number\" element and is mandatory in the format n 16.6. Value must be greater than zero."));
		}
	}

	static bool IsY121orY798(AdditionalInfo additionalInfo)
		=> additionalInfo.CSI_Code.ToString()
			is UniversalReferenceCusSupportingInfoTypes.Y121
			or UniversalReferenceCusSupportingInfoTypes.Y798;

	void CheckRuleG0825()
	{
		var parent = Parent;
		if (parent.Parent is not CusEntryInstruction)
		{
			if (parent.CSI_SubType == AdditionalInfoKindList.Codes.INF
				&& parent.CSI_Code == AdditionalInfoCodes._4PL03)
			{
				parent.AddRowMessageError(Res.GetString("C280D3D1-5881-439D-951F-6E51A2D42B88",
					"(G0825) Documents/Information common for the Entry must be entered in Entry Instruction."));
			}
		}
	}

	void CheckRuleR0039E()
	{
		var parent = Parent;
		if (parent.CSI_SubType == AdditionalInfoKindList.Codes.INF
			&& parent.CSI_Code == AdditionalInfoCodes._EXP15
			&& parent.CSI_Description.IsEmpty)
		{
			parent.CSI_DescriptionInfo.AddMessageError(Res.GetString("9E897844-2E00-4A80-BABB-0A332B4682DD",
				"[R0039E] Description is required for Additional Information Type 'EXP15'."));
		}
	}
}
