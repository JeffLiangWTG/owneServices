using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class ViewSailingRelatedJobLookups : AutoViewSailingRelatedJobLookups
	{
		public ViewSailingRelatedJobLookups(AutoViewSailingRelatedJob parent) : base(parent)
		{
		}

		#region Companies

		public virtual GlbCompanyCollection Companies
		{
			get { return new GlbCompanyCollection(Factory); }
		}

		#endregion
	}
}
