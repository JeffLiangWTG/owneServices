using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceGroupHeaderValidation : JobComInvoiceHeaderValidation
	{
		public JobComInvoiceGroupHeaderValidation(BaseJobComInvoiceGroupHeader groupHeader) : base(groupHeader)
		{
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();
			ListValidation.ErrorIfInvalidCode(Parent.JZ_RX_NKInvoice_CurrencyInfo);
		}

		public BaseJobComInvoiceGroupHeader InvoiceGroupHeader
		{
			get { return (BaseJobComInvoiceGroupHeader)Parent; }
		}

		public ExternalMessageValidation MessageValidation
		{
			get { return messageValidation ?? (messageValidation = GetNewExternalMessageValidation()); }
		}
		ExternalMessageValidation messageValidation;

		protected virtual ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(InvoiceGroupHeader);
		}

		public void ValidateCharges()
		{
			foreach (BaseJobComInvHeaderCharge charge in InvoiceGroupHeader.Charges)
			{
				charge.RunPreSaveValidation();
			}
		}

		public void CheckApportioned(ZPropertyInfo amountInfo, ZPropertyInfo currencyInfo)
		{
			if (!amountInfo.Value.IsEmpty && currencyInfo.Value.IsValid)
			{
				if (InvoiceGroupHeader.AllJobComInvoiceHeaders.Count == 0)
				{
					if (!amountInfo.HasErrors()) //HACK: - will blow with duplicate message otherwise
					{
						amountInfo.AddError(Res.GetString("aa672072-793d-414f-bc49-547d0d7c6f45", "Unable to apportion {0} charges because the current group contains no invoices", InvoiceGroupHeader.TableName + "." + amountInfo.Name));
					}
				}
				else
				{
					bool headerAcceptedCharge = false;
					foreach (BaseJobComInvoiceHeader aHeader in InvoiceGroupHeader.AllJobComInvoiceHeaders)
					{
						headerAcceptedCharge = (ZDecimal)aHeader[amountInfo.Name] == 0.0m;
						if (headerAcceptedCharge)
						{
							break;
						}
					}
					if (!headerAcceptedCharge)
					{
						if (!amountInfo.HasErrors()) //HACK: Suppress duplicate message warning
						{
							amountInfo.AddError(Res.GetString("53a42a79-895d-490f-bf4a-3e8ad63f56ab", "Unable to apportion {0} charges because all invoices in the group have the charge assigned", InvoiceGroupHeader.TableName + "." + amountInfo.Name));
						}
					}
				}
			}
		}
	}
}
