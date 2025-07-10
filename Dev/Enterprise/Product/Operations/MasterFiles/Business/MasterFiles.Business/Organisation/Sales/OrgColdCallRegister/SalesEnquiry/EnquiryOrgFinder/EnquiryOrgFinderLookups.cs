using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgFinderLookups : ZLookups
	{
		public OrgFinderLookups(EnquiryOrgFinder parent)
			: base(parent) { }

		public OrgHeaderCollection Organisations
		{
			get { return organisations ?? (organisations = new OrgHeaderCollection(Parent.Factory)); }
		}
		OrgHeaderCollection organisations;

		#region Implementation

		protected new EnquiryOrgFinder Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (EnquiryOrgFinder)base.Parent; }
		}

		#endregion
	}
}
