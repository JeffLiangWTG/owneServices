using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.Module
{
	/// <summary>
	/// Filter control for ManifestTally.
	/// </summary>
	public partial class ManifestTallyFilterControl : ZFilterStripControl<ManifestTallyFilterStrip>
	{
		readonly System.ComponentModel.Container components;

		public ManifestTallyFilterControl()
		{
			InitializeComponent();
		}

		public ManifestTallyFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();

				zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|7a39e59d-c2e5-4e90-8935-22b88c4b4f93", "Consol CCN");
				zTextBoxColumnStyleInfo1.ColumnName = "CanadaCCNNumber";
				zTextBoxColumnStyleInfo1.IsVisible = false;
				zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|50d3bc48-e4b4-4183-aecf-1d7df7af2b5b", "Consol PCN");
				zTextBoxColumnStyleInfo2.ColumnName = "CanadaPCNNumber";
				zTextBoxColumnStyleInfo2.IsVisible = false;

				this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
