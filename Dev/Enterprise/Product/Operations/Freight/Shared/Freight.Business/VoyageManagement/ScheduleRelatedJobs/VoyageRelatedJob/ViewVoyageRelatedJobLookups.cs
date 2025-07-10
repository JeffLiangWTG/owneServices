using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class ViewVoyageRelatedJobLookups : AutoViewVoyageRelatedJobLookups
	{
		public ViewVoyageRelatedJobLookups(AutoViewVoyageRelatedJob parent) : base(parent)
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
