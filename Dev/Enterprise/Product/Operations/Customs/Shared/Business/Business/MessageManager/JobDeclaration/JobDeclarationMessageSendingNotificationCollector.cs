using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationMessageSendingNotificationCollector : CustomsNotificationCollector
	{
		public JobDeclarationMessageSendingNotificationCollector(BaseJobDeclaration declaration, IEnumerable<CusEntryHeader> selectedEntries) : base(declaration, true, false, PropertyDescriptionType.HumanReadableName)
		{
			this.selectedEntries = selectedEntries;
		}
		readonly IEnumerable<CusEntryHeader> selectedEntries;

		protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
		{
			var result = base.ShouldIncludeNotificationsFromObject(businessObject);

			if (result)
			{
				switch (businessObject)
				{
					case CusEntryHeader _:
						result = selectedEntries.Any(x => x.PK == businessObject.PK);
						break;
					case CusEntryInstruction _:
						result = selectedEntries.Any(x => x.CH_CEI_Instruction == businessObject.PK);
						break;
					case BaseJobComInvoiceLine _:
						result = selectedEntries.Any(x => x.InvoiceLines.Any(l => l.PK == businessObject.PK));
						break;
					case BaseJobComInvoiceHeader _:
						result = selectedEntries.Any(x => x.InvoiceHeaders.Any(h => h.PK == businessObject.PK));
						break;
				}
			}

			return result;
		}
	}
}
