using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class CusAuthorisationsControl : ZFilterStripControl
	{
		public CusAuthorisationsControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			ConfigureAdHocCheckBoxColumn();
		}

		void ConfigureAdHocCheckBoxColumn()
		{
			if (CusAuthorisationHeaderProvider.EnableAdHoc)
			{
				var adHocCheckBoxColumnStyle = new ZArchitecture.ZCheckBoxColumnStyleInfo();
				adHocCheckBoxColumnStyle.ColumnName = "CPH_IsAdHoc";
				adHocCheckBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
				adHocCheckBoxColumnStyle.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("E3DB3965-0C39-40F4-BCE6-1BCAF1366D56", CusAuthorisationHeaderCollection.FilterConstants.AdHoc);
				grid.ColumnStyles.Add(adHocCheckBoxColumnStyle);
			}
		}

		protected override ZFilterStrip NewZFilterStrip() => new CusAuthorisationsFilterStrip();

		CusAuthorisationHeaderProvider CusAuthorisationHeaderProvider => cusAuthorisationHeaderProvider ?? (cusAuthorisationHeaderProvider = CusAuthorisationHeaderProvider.GetByCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		CusAuthorisationHeaderProvider cusAuthorisationHeaderProvider;
	}
}
