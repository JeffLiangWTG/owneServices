using CargoWise.Types;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	public class ElectronicInvoiceStatusCalculator
	{
		public ElectronicInvoiceStatusCalculator(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		public ZString ElectronicInvoiceStatus
		{
			get { return GetElectronicInvoiceStatus(); }
		}

		ZString GetElectronicInvoiceStatus()
		{
			ZString status = ZString.Empty;

			foreach (JobComInvoiceHeader invoiceHeader in declaration.Invoices)
			{
				if (status == ZString.Empty)
				{
					status = invoiceHeader.JZ_MessageStatus;
				}
				else if (status != invoiceHeader.JZ_MessageStatus)
				{
					status = MessageStatusListEI.Codes.Multiple;
					break;
				}
			}

			return status;
		}

		#region Implementation

		readonly JobDeclaration declaration;

		#endregion
	}
}
