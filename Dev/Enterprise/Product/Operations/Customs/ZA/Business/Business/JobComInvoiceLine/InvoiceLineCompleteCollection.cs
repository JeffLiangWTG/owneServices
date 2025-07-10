using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class InvoiceLineCompleteCollection : Customs.Business.InvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration declaration)
			: base(declaration)
		{
			UpdateMaxCountValidation();
		}

		JobDeclaration Declaration => (JobDeclaration)JobDeclaration;

		public new JobComInvoiceLine this[int index]
		{
			get { return (JobComInvoiceLine)base[index]; }
		}

		public new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			using (child.SuspendSettingHasChanges())
			{
				if (child is JobComInvoiceLine invoiceLine)
				{
					invoiceLine.JI_AdvancePaymentNo = invoiceLine.InvoiceHeader?.JZ_PaymentNo ?? ZString.Empty;
					invoiceLine.DefaultPreference();
				}
			}
		}

		public void UpdateMaxCountValidation()
		{
			if (Declaration.IsExWarehouse)
			{
				var maxAllowedInvoiceLines = ZACustomsRegistry.Instance.ExbondMaxNumberJobInvoiceLines.Value;
				if (maxAllowedInvoiceLines > 0)
				{
					MaxCountValidationEnable(maxAllowedInvoiceLines, MaxCountErrorMessage(maxAllowedInvoiceLines));
				}
				else
				{
					MaxCountValidationDisable();
				}
			}
			else
			{
				MaxCountValidationDisable();
			}
		}

		string MaxCountErrorMessage(int maxAllowedCount)
			=> Res.GetString("2891B2FB-6216-48E4-A65B-9F04744D65E4", "Ex-bond Max Number of Job Invoice Lines has been Exceeded, Maximum allowed is {0}", maxAllowedCount);
	}
}
