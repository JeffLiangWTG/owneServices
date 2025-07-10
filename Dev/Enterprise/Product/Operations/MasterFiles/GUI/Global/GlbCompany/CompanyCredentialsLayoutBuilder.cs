using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class CompanyCredentialsLayoutBuilder<T> : ColumnLayoutBuilder<T, CompanyCredentialsControlBag> where T : GlbCompanyWrapper
	{
		public override CompanyCredentialsControlBag CommonBag => CompanyCredentialsControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
