using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AddInfoJobComInvoiceLineValidation : EU.Business.Declaration.AddInfoJobComInvoiceLineValidation
	{
		public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent) : base(parent)
		{
		}

		protected override void CheckZG_UsedGoodsCode()
		{
			base.CheckZG_UsedGoodsCode();
			if (Parent.InvoiceLine.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_UsedGoodsCodeInfo);
			}
		}

		protected override void CheckZG_ReturningGoodsReasonCode()
		{
			base.CheckZG_ReturningGoodsReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_ReturningGoodsReasonCodeInfo);
		}

		protected override void CheckZG_InwardProcessingLicenseLineNumber()
		{
			base.CheckZG_InwardProcessingLicenseLineNumber();
			var lineNumber = Parent.ZG_InwardProcessingLicenseLineNumber;
			if (!lineNumber.IsEmpty && !IsValidLineNumber(lineNumber))
			{
				Parent.ZG_InwardProcessingLicenseLineNumberInfo.AddMessageError(Res.GetString("19D83A01-AF81-4B81-AD5E-74179D6AB9C0", "Line number should be 11 alphanumeric characters in format of \"{0}\" or {1}", "XX.X.XXXXX.XXX", "99999999"));
			}
		}

		bool IsValidLineNumber(string lineNumber)
		{
			var result = false;

			var length = lineNumber.Length;

			if (length == 8)
			{
				result = Regex.IsMatch(lineNumber, @"^9{8}$");
			}
			else if (length == 14)
			{
				result = Regex.IsMatch(lineNumber, @"^\w{2}\.\w{1}\.\w{5}\.\w{3}$");
			}

			return result;
		}

		protected override void CheckZG_EntryExitPurposeCode()
		{
			base.CheckZG_EntryExitPurposeCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_EntryExitPurposeCodeInfo);
		}

		protected override void CheckZG_ExportUnionProductionYear()
		{
			base.CheckZG_ExportUnionProductionYear();
			MandatoryValidation.CheckNotNegative(Parent.ZG_ExportUnionProductionYearInfo);
		}

		protected override void CheckZG_ExportUnionPackCode()
		{
			base.CheckZG_ExportUnionPackCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_ExportUnionPackCodeInfo);
		}

		protected override void CheckZG_ExportUnionThreadCode()
		{
			base.CheckZG_ExportUnionThreadCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_ExportUnionThreadCodeInfo);
		}

		protected override void CheckZG_ExportUnionAdditionalTariffCode()
		{
			base.CheckZG_ExportUnionAdditionalTariffCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_ExportUnionAdditionalTariffCodeInfo);
		}

		protected override void CheckZG_RW_NKBorderTradeStateCode()
		{
			base.CheckZG_RW_NKBorderTradeStateCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_RW_NKBorderTradeStateCodeInfo);
			var supplements = Parent.InvoiceLine.JI_AdditionalSupplements.Split(',');
			if (Parent.ZG_RW_NKBorderTradeStateCode.IsEmpty
				&& supplements.Any(cus => cus.Contains("STEX", StringComparison.OrdinalIgnoreCase) || cus.Contains("STIM", StringComparison.OrdinalIgnoreCase)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_RW_NKBorderTradeStateCodeInfo);
			}
		}

		protected override void CheckZG_CommercialPaymentCode()
		{
			base.CheckZG_CommercialPaymentCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_CommercialPaymentCodeInfo);
		}

		protected override void CheckZG_CommercialPaymentNumber()
		{
			base.CheckZG_CommercialPaymentNumber();
			var procedureCode = Parent.InvoiceLine.ProcedureCode;
			var mandatoryProcedureList = new HashSet<string> { "6121", "6123", "6323", "6771", "5100", "5121", "5171", "5191", "5300", "5321", "5353", "5358", "5371", "5391", "5800" };
			if (Parent.ZG_CommercialPaymentNumber.IsEmpty &&
				(procedureCode.StartsWith("4", StringComparison.Ordinal) || procedureCode.StartsWith("71", StringComparison.Ordinal) || mandatoryProcedureList.Contains(procedureCode)))
			{
				Parent.ZG_CommercialPaymentNumberInfo.AddMessageError(Res.GetString("012AB53C-C8C9-45DA-94FC-AC4BB34BDD46", "Payment Code field is mandatory for procedure codes starting with 4 and 71, and for the procedure codes 6121, 6123, 6323, 6771, 5100, 5121, 5171, 5191, 5300, 5321, 5353, 5358, 5371, 5391, 5800."));
			}
		}

		protected override void CheckZG_CommercialPaymentAmount()
		{
			base.CheckZG_CommercialPaymentAmount();
			if (!Parent.ZG_CommercialPaymentCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_CommercialPaymentAmountInfo);
			}
		}

		protected override void CheckZG_ReturnToOrigin()
		{
			base.CheckZG_ReturnToOrigin();

			var invoiceHeader = Parent.InvoiceLine.InvoiceHeader;

			if (!Parent.ZG_ReturnToOrigin && invoiceHeader != null)
			{
				if (invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.ZG_ReturnToOrigin))
				{
					Parent.ZG_ReturnToOriginInfo.AddWarning(Res.GetString("9718C97B-D631-4F17-AA3D-249F2870C777", "Are you sure that these goods are not Returned to Origin? They will be merged as another entry line."));
				}
			}
		}

		protected override void CheckZG_PriceType()
		{
			base.CheckZG_PriceType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_PriceTypeInfo);
		}

		new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;
	}
}
