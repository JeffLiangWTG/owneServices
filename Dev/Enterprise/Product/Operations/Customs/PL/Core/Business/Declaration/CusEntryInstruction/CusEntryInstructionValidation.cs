using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryInstructionValidation(CusEntryInstruction parent) : EU.Business.Declaration.CusEntryInstructionValidation(parent)
{
	public override void ValidateAll()
	{
		base.ValidateAll();
		CheckRuleE1301();
		CheckRuleR842AndR286();
		CheckRuleR408();
		CheckMaximumNumberOfPackages();
	}

	protected override ZBool CheckDuplicatedInstruction => true;

	protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

	protected void CheckMaximumNumberOfPackages()
	{
		if (Parent.InvoiceLines
				.SelectMany(x => x.PackagesPivot.Cast<Customs.Business.InvoiceLinePackagePivot>())
				.Sum(p => p.CHC_NumberOfPacks) > Constants.MaximumBusinessObjectsAmounts.MaximumPackages)
		{
			Parent.AddRowMessageError(Res.GetString("FB416A28-1B2D-4ADA-80B9-724F046042B0", "The number of packages is too large. {0} is the maximum allowed for an entry.", Constants.MaximumBusinessObjectsAmounts.MaximumPackages));
		}
	}

	protected override void CheckCEI_SubStyle()
	{
		var parent = Parent;
		if (parent.IsExitSummary)
		{
			return;
		}

		base.CheckCEI_SubStyle();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CEI_SubStyleInfo);
	}

	protected override void CheckCEI_DateForDuty()
	{
		base.CheckCEI_DateForDuty();

		if (Parent.CEI_DateForDuty.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_DateForDutyInfo);
		}
		else if (Parent.DateForDutyIsObsolete)
		{
			Parent.CEI_DateForDutyInfo.AddWarning(Res.GetString("PLCusEntryInstructionValidation|CheckCEI_DateForDuty", "Date is older than current date."));
		}
	}

	protected override void CheckCEI_Procedure()
	{
		base.CheckCEI_Procedure();

		var parent = Parent;
		if (parent.IsExitSummary)
		{
			return;
		}

		var procedure = parent.CEI_Procedure;
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CEI_ProcedureInfo);
		if ((procedure == Constants.ProcedureCodes._42 || procedure == Constants.ProcedureCodes._63)
			&& parent.InvoiceLines.Any(x => CheckSupportingDocuments(x.SupportingDocuments) && CheckSupportingDocuments(x.InvoiceHeader.SupportingDocuments) && CheckSupportingDocuments(x.Declaration.SupportingDocuments)))
		{
			parent.CEI_ProcedureInfo.AddMessageError(Res.GetString("PLCusEntryInstructionValidation|CheckRuleR266",
				"(R266) For the requested procedure code 42 and 63, supporting document \'Y044\' is required."));
		}

		bool CheckSupportingDocuments(SupportingDocumentCollection supportingDocuments)
		{
			return supportingDocuments.Count == 0 || supportingDocuments.Cast<SupportingDocument>().All(s => s.CSI_Code != Constants.SupportingDocumentCodes.Y044);
		}
	}

	protected override void CheckCEI_OA_Warehouse()
	{
		base.CheckCEI_OA_Warehouse();

		var parent = Parent;

		var warehouse = parent.Warehouse;
		CheckWarehouseHasAuthorizationNumber(warehouse, parent.CEI_OA_WarehouseInfo);

		if (warehouse != null && warehouse.OA_Email.Length > Constants.MaximumBusinessObjectsAmounts.WarehouseAddressMaxLength)
		{
			parent.CEI_OA_WarehouseInfo.AddMessageError(Res.GetString("B13A1132-8C23-4E05-A1E6-D0610A605851", "The organization email address is longer than {0} characters.", Constants.MaximumBusinessObjectsAmounts.WarehouseAddressMaxLength));
		}
	}

	protected override void CheckCEI_OA_Warehouse2()
	{
		base.CheckCEI_OA_Warehouse2();
		CheckWarehouseHasAuthorizationNumber(Parent.Warehouse2, Parent.CEI_OA_Warehouse2Info);
	}

	protected void CheckRuleR408()
	{
		if (Parent.CEI_Procedure == Constants.ProcedureCodes._71 && Parent.Guarantees.Any())
		{
			Parent.AddRowMessageError(Res.GetString("PLCusEntryInstructionValidation|CheckRuleR408Message",
				"(R408) - For custom procedure '71' guarantee is not used"));
		}
	}

	protected void CheckRuleE1301()
	{
		var parent = Parent;

		if (!parent.IsAESTransitionPeriod())
		{
			return;
		}

		if (parent.Invoices.Select(x => x.JZ_ValuationCode).Distinct().Count() != 1)
		{
			parent.AddRowMessageError(Res.GetString("DDDD2990-C97F-4E78-957E-1F8D7DE9DF3C",
				"You have entered different valuation method codes for the Entry (E1301-not allowed in transition period)"));
		}
	}

	protected void CheckRuleR842AndR286()
	{
		if (Parent.CEI_Procedure != Constants.ProcedureCodes._71 && Parent.Guarantees.Count == 0)
		{
			var fees = Parent.EntryHeader?.AllEntryLines.Cast<CusEntryLine>().SelectMany(x => x.Fees.Cast<CusEntryLineFee>()).ToArray() ??
						Array.Empty<CusEntryLineFee>();
			if (CheckRuleR842(fees) || CheckRuleR286(fees))
			{
				Parent.AddRowMessageError(Res.GetString("PLCusEntryInstructionValidation|CheckRuleR842AndR286Message",
					"(R286/R842) - Guarantee is required"));
			}
		}
	}

	static bool CheckRuleR842(IEnumerable<CusEntryLineFee> fees) =>
		fees.Any(x =>
		{
			var methodOfPayment = x.CF_MethodOfPayment;
			return x.CF_ChargeAmount > 0
					&& (methodOfPayment == PLMethodOfPaymentList.Codes.R
						|| methodOfPayment == PLMethodOfPaymentList.Codes.D
						|| methodOfPayment == PLMethodOfPaymentList.Codes.E);
		});

	static bool CheckRuleR286(IEnumerable<CusEntryLineFee> fees)
	{
		return fees.Any(x => x.CF_MethodOfPayment == PLMethodOfPaymentList.Codes.E &&
							x.CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
	}

	static void CheckWarehouseHasAuthorizationNumber(OrgAddress warehouseAddress, ZPropertyInfo info)
	{
		if (warehouseAddress != null)
		{
			var warehouseCustomsRegNo = warehouseAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, Core.Constants.CountryCodes.Poland);

			if (warehouseCustomsRegNo.IsEmpty)
			{
				info.AddMessageError(Res.GetString("12A47604-E7BD-4CB2-9020-96AC0D43B1B8", "A warehouse authorization number(country PL type CPW) is missing for selected address"));
			}
		}
	}

	protected override ZBool CanFiscalRepresentationEnteredWhenNoInvoiceLinesStartwith42Or63 => ZBool.True;

	protected override ZBool AllRelatedInvoicesMustHaveSameCurrency => ZBool.True;

	protected bool HasAdditionalInformationCode(ZString csiType, ZString csiCode) =>
		Parent.JobDeclaration.HasAdditionalInformationCode(csiType, csiCode)
		|| Parent.Invoices.Cast<JobComInvoiceHeader>()
			.Any(invoice => invoice.HasAdditionalInformationCode(csiType, csiCode))
		|| Parent.InvoiceLines.Cast<JobComInvoiceLine>()
			.Any(invoiceLine => invoiceLine.HasAdditionalInformationCode(csiType, csiCode));

	protected bool DoesNotHaveAdditionalInformationCode(ZString csiType, ZString csiCode) =>
		!Parent.JobDeclaration.HasAdditionalInformationCode(csiType, csiCode)
		&& Parent.Invoices.Cast<JobComInvoiceHeader>()
			.Any(invoice => !invoice.HasAdditionalInformationCode(csiType, csiCode))
		&& Parent.InvoiceLines.Cast<JobComInvoiceLine>()
			.Any(invoiceLine => !invoiceLine.HasAdditionalInformationCode(csiType, csiCode));

	protected bool HasSupportingDocumentCodes(IReadOnlyCollection<ZString> supportingDocumentCsiCodes) =>
		Parent.JobDeclaration.HasSupportingDocumentCode(supportingDocumentCsiCodes)
		|| Parent.InvoiceLines.Cast<JobComInvoiceLine>()
			.Any(invoiceLine => invoiceLine.HasSupportingDocumentCode(supportingDocumentCsiCodes))
		|| Parent.Invoices.Cast<JobComInvoiceHeader>()
			.Any(invoice => invoice.HasSupportingDocumentCode(supportingDocumentCsiCodes));

	protected bool HasSupportingDocumentCode(ZString supportingDocumentCsiCode) =>
		Parent.JobDeclaration.HasSupportingDocumentCode(supportingDocumentCsiCode)
		|| Parent.Invoices.Cast<JobComInvoiceHeader>()
			.Any(invoice => invoice.HasSupportingDocumentCode(supportingDocumentCsiCode))
		|| Parent.InvoiceLines.Cast<JobComInvoiceLine>()
			.Any(invoiceLine => invoiceLine.HasSupportingDocumentCode(supportingDocumentCsiCode));

	protected bool DoesNotHaveSupportingDocumentCode(ZString supportingDocumentCsiCode) =>
		!Parent.JobDeclaration.HasSupportingDocumentCode(supportingDocumentCsiCode)
		&& Parent.Invoices.Cast<JobComInvoiceHeader>()
			.Any(invoice => !invoice.HasSupportingDocumentCode(supportingDocumentCsiCode))
		&& Parent.InvoiceLines.Cast<JobComInvoiceLine>()
			.Any(invoiceLine => !invoiceLine.HasSupportingDocumentCode(supportingDocumentCsiCode));
}
