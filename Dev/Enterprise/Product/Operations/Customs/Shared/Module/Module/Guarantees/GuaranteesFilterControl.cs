using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public partial class GuaranteesFilterControl : CusPermitFilterControl
	{
		public GuaranteesFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			grid.SetColumnCaption(CusPermitHeaderSchema.Constants.CPH_RN_NKCountryCode, Res.GetString("C4CCC77B-BD73-4BD1-B14C-6DD68AB8B79E", "Creation Country"));
		}
	}
}
