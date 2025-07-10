using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			RemoveInvalidColumn();
			AddColumns();
		}

		void RemoveInvalidColumn()
		{
			FilteredGrid.SetAvailability(false, [JobDeclaration.Schema.JE_DateOfFirstArrival, JobDeclaration.Schema.JE_RL_NKPortOfFirstArrival]);
		}

		void AddColumns()
		{
			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("3BD30783-71C4-4E09-A98B-2DC4FE2316AD", "Case Numbers");
			zTextBoxColumnStyleInfo1.ColumnName = "CaseNumbers";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			var zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("51994340-DD58-4648-A152-19038BF26A8E", "Supporting Document Status");
			zTextBoxColumnStyleInfo2.ColumnName = "SupportingDocumentStatus";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

			var zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("FD809A91-2BA5-4C3C-B302-445BD0EE8F34", "Unique Consignment Reference (UCR)");
			zTextBoxColumnStyleInfo3.ColumnName = "CombinedUCREntryNumbers";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);

			var zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("98FE57AC-521D-4967-8D42-7101ECB929B3", "Customs Printed Release Required");
			zTextBoxColumnStyleInfo4.ColumnName = "CombinedReleasePrintIndicator";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
		}

		protected override ZFilterStrip NewZFilterStrip() => new JobDeclarationModuleStrip();
	}
}

