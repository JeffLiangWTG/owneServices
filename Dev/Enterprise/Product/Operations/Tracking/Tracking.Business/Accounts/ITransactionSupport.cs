using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Interface to support for transactions
	/// </summary>
	public interface ITransactionSupport
	{
		ZGuid PK { get; }
		BusinessObjectFactory Factory { get; }
		OrgHeader LoggedInOrganisation { get; }
		TrackingSiteUser SiteUser { get; }
		ZString Reference { get; }
		TrackingInvoiceLoader InvoiceLoader { get; }
	}
}
