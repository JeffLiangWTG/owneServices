using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class ChassisJobComInvLineRefsValidation : Customs.Business.JobComInvLineRefsValidation
	{
		public ChassisJobComInvLineRefsValidation(ChassisJobComInvLineRefs parent)
			: base(parent)
		{
		}

		public new ChassisJobComInvLineRefs Parent => (ChassisJobComInvLineRefs)base.Parent;

		protected override void CheckJG_ReferenceNumber()
		{
			base.CheckJG_ReferenceNumber();

			var targetInfo = Parent.JG_ReferenceNumberInfo;
			var parentInvoiceLine = Parent.InvoiceLine;
			var referenceNumber = Parent.JG_ReferenceNumber;

			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			if (!referenceNumber.IsEmpty)
			{
				var chassisCollection = parentInvoiceLine.ChassisJobComInvLineRefsCollection.Cast<ChassisJobComInvLineRefs>();

				var invoiceQuantity = parentInvoiceLine.JI_InvoiceQuantity;
				var chassisNumbers = chassisCollection.Count(chassis => !chassis.JG_ReferenceNumber.IsEmpty);

				if (invoiceQuantity != chassisNumbers)
				{
					targetInfo.AddMessageError(Res.GetString("BFD23D68-1B34-4A72-81BF-E5889C18BE56", "The number of chassis number must be equals to invoice quantity."));
				}

				bool hasDuplicates = chassisCollection.Any(x => x.JG_ReferenceNumber == referenceNumber && x.PK != Parent.PK);
				if (hasDuplicates)
				{
					targetInfo.AddWarning(Res.GetString("CF0CEABB-F5BB-4D4B-A55A-E5EFC63D9BD6", "This chassis number already exists in this invoice line"));
				}
			}
		}
	}
}
