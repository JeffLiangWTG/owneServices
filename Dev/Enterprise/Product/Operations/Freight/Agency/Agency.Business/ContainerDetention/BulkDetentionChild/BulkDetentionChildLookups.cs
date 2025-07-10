using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkDetentionChildLookups : ZLookups
	{
		public BulkDetentionChildLookups(BulkDetentionChild parent)
			: base(parent) { }

		public ShipsAgencyPrincipalCollection Principals
		{
			get { return new ShipsAgencyPrincipalCollection(Factory); }
		}

		public OrganisationsFindBoxCollection Clients
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public DetentionInvoiceType DetentionTypes
		{
			get { return directionTypes ?? (directionTypes = new DetentionInvoiceType()); }
		}
		DetentionInvoiceType directionTypes;

		#region Implementation

		protected new BulkDetentionChild Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkDetentionChild)base.Parent; }
		}

		#endregion
	}
}
