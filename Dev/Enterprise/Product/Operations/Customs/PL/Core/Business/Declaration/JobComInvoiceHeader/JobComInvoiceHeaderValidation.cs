using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class JobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
{
	public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
		: base(invoiceHeader)
	{
	}

	protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateAdditionalTranCircumstanceCodesAsString();
		ValidateTranCircumstance1();
	}

	public void ValidateAdditionalTranCircumstanceCodesAsString()
	{
		ValidateCalculatedProperty(Parent.AdditionalTranCircumstanceCodesAsStringInfo);
	}

	protected void CheckAdditionalTranCircumstanceCodesAsString()
	{
		var additionalProcedureCodesList = Parent.AdditionalTranCircumstanceCodes.Cast<TranCircumstance>().ToList();
		additionalProcedureCodesList.ForEach(x => Parent.AdditionalTranCircumstanceCodesAsStringInfo.AddAllNotificationsFrom(x.CY_CodeInfo));
	}

	protected override void CheckJZ_RX_NKInvoice_Currency()
	{
		base.CheckJZ_RX_NKInvoice_Currency();
		Parent.InvoiceLines.ToList().ForEach(x => ((JobComInvoiceLine)x).Validation.ValidateJI_CEI());
	}

	public void ValidateTranCircumstance1()
	{
		ValidateCalculatedProperty(Parent.TranCircumstanceCode1Info);
	}

	protected virtual void CheckTranCircumstanceCode1()
	{
	}

	protected override void CheckJZ_OA_SupplierAddress()
	{
		base.CheckJZ_OA_SupplierAddress();
		ValidateEmptyOrgAddress(Parent.SupplierOrgPK, Parent.JZ_OA_SupplierAddressInfo);
	}

	protected override void CheckJZ_OA_BuyerAddress()
	{
		base.CheckJZ_OA_BuyerAddress();
		ValidateEmptyOrgAddress(Parent.BuyerOrgPK, Parent.JZ_OA_BuyerAddressInfo);
	}

	protected override void CheckJZ_OA_ExporterAddress()
	{
		base.CheckJZ_OA_ExporterAddress();
		ValidateEmptyOrgAddress(Parent.ExporterOrgPK, Parent.JZ_OA_ExporterAddressInfo);
	}

	protected override void CheckJZ_OA_SellerAddress()
	{
		base.CheckJZ_OA_SellerAddress();
		ValidateEmptyOrgAddress(Parent.SellerOrgPK, Parent.JZ_OA_SellerAddressInfo);
	}

	protected override void CheckJZ_OA_ConsigneeAddress()
	{
		base.CheckJZ_OA_ConsigneeAddress();
		ValidateEmptyOrgAddress(Parent.ConsigneeOrgPK, Parent.JZ_OA_ConsigneeAddressInfo);
	}

	protected override void CheckJZ_OA_InvoicerAddress()
	{
		base.CheckJZ_OA_ConsigneeAddress();
		ValidateEmptyOrgAddress(Parent.InvoicerOrgPK, Parent.JZ_OA_InvoicerAddressInfo);
	}

	protected override void CheckJZ_OA_ManufacturerAddress()
	{
		base.CheckJZ_OA_ConsigneeAddress();
		ValidateEmptyOrgAddress(Parent.ManufacturerOrgPK, Parent.JZ_OA_ManufacturerAddressInfo);
	}

	protected override void CheckJZ_InvoiceCurrExRate()
	{
		base.CheckJZ_InvoiceCurrExRate();

		var parent = Parent;
		if (parent.JZ_InvoiceCurrExRate.IsEmpty
			&& parent.CusEntryInstructions.Any(e => e.CEI_SubStyle == SubStyleCodes.R))
		{
			parent.JZ_InvoiceCurrExRateInfo.AddMessageError(Res.GetString("PLExportJobComInvoiceLineValidation|CheckRuleR2010",
				"[R2010] The Exchange Rate for retrospective declaration (Sub Style = 'R') is required"));
		}
	}

	protected void ValidateEmptyOrgAddress(ZGuid org, ZPropertyInfo orgAddressInfo)
	{
		var orgAddresss = orgAddressInfo.Value;
		if (!org.IsEmpty && orgAddresss.IsEmpty)
		{
			orgAddressInfo.AddMessageError(Res.GetString("E43D417D-93F2-470F-B240-3899A3FD8996", "Enter a valid {0}", orgAddressInfo.HumanReadableName));
		}
	}

	protected override void CheckJZ_IncoTerm()
	{
		if (!Parent.JZ_IncoTermInfo.ReadOnly)
		{
			base.CheckJZ_IncoTerm();
		}
	}

	protected override void CheckJZ_IncoTermPlace()
	{
		if (!Parent.JZ_IncoTermPlaceInfo.ReadOnly)
		{
			base.CheckJZ_IncoTermPlace();
		}
	}

	protected override void CheckJZ_ValuationCode()
	{
		if (!Parent.JZ_ValuationCodeInfo.ReadOnly)
		{
			base.CheckJZ_ValuationCode();
		}
	}

	protected override void CheckJZ_NetWeight()
	{
		if (!Parent.JZ_NetWeightInfo.ReadOnly)
		{
			base.CheckJZ_NetWeight();
		}
	}

	protected override void CheckJZ_NetWeightUQ()
	{
		if (!Parent.JZ_NetWeightUQInfo.ReadOnly)
		{
			base.CheckJZ_NetWeightUQ();
		}
	}
}
