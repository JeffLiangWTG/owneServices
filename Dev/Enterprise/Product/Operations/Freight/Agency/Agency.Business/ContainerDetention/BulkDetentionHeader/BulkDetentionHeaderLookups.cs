using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkDetentionHeaderLookups : ZLookups
	{
		public BulkDetentionHeaderLookups(BulkDetentionHeader parent)
			: base(parent) { }

		public DetentionInvoiceType DetentionTypes
		{
			get { return detentionTypes ?? (detentionTypes = new DetentionInvoiceType()); }
		}
		DetentionInvoiceType detentionTypes;

		public OrgHeaderCollection Principals
		{
			get { return new ShipsAgencyPrincipalCollection(Factory); }
		}

		public OrgHeaderCollection Clients
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		#region Implementation

		protected new BulkDetentionHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkDetentionHeader)base.Parent; }
		}

		#endregion
	}
}
