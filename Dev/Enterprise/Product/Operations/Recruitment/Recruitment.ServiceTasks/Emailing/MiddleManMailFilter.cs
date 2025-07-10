using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruitment.ServiceTasks.Emailing
{
	public class MiddleManMailFilter : IMailFilter
	{
		readonly string address;

		public MiddleManMailFilter(string address)
		{
			this.address = address;
		}

		public bool CanProcess(IMailItem item)
			=> ((MailItem)item)
				.MailRecipients
				.Cast<MailRecipient>()
				.Any(r => address == EmailForwarderUtilities.ParseEmailAddress(r.EmailAddress));

		public ZQuery LoadQuery(int limit = -1)
		{
			return new ZQuery(MailDBItemsSchema.MI_Application, SQLComparisonOperator.Equal, Code)
			{
				MaximumRows = limit > 0 ? limit : null
			}.AddToFilter(MailDBItemsSchema.MI_Status, SQLComparisonOperator.Equal, MailStatus.Queued)
			.AddToFilter(MailDBItemsSchema.MI_Direction, SQLComparisonOperator.Equal, MailDirection.Receive);
		}

		public IMailItem[] Load(BusinessObjectFactory factory, int n)
		{
			return factory.Load<MailItem>(LoadQuery(n));
		}

		public string Code => MiddleManServiceTask.Code;

		public bool IsEnabled => true;
	}
}
