using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLHeaderCompanyFilterCollection : DependentBusinessObjectCollection<AccGLHeaderCompanyFilter, AccGLHeader>
	{
		public AccGLHeaderCompanyFilterCollection(AccGLHeader accGLHeader)
			: base(accGLHeader)
		{
		}
		protected override string FkColumnName
		{
			get
			{
				return AccGLHeaderCompanyFilterSchema.Constants.ACF_AG_Header;
			}
		}
	}
}
