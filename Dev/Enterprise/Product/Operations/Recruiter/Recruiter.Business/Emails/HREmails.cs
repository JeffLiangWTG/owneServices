using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MailManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class HREmails : MailItem
	{
		public const string Code = "HRE";

		public HREmails(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
