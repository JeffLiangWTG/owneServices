using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class EmptyReportContingency
	{
		public EmptyReportContingency(ZString type, ZString emailAddress) : this(type, emailAddress, ZString.Empty, ZString.Empty)
		{
		}
		public EmptyReportContingency(ZString type, ZString emailAddress, ZString ccEmailAddress, ZString bccEmailAddress)
		{
			Type = type;
			EmailAddress = emailAddress;
			CcEmailAddress = ccEmailAddress;
			BccEmailAddress = bccEmailAddress;
		}

		public ZString Type { get; }
		public ZString EmailAddress { get; }
		public ZString CcEmailAddress { get; }
		public ZString BccEmailAddress { get; }
	}
}