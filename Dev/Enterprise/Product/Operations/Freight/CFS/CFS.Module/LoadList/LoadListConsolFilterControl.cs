using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.Module
{
	public partial class LoadListConsolFilterControl : ZFilterStripControl<LoadListConsolFilterStrip>
	{
		readonly System.ComponentModel.Container components;

		public LoadListConsolFilterControl()
		{
			InitializeComponent();
		}

		public LoadListConsolFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
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

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZArchitecture.ZTextBoxColumnStyleInfo();

				zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|7a39e59d-c2e5-4e90-8935-22b88c4b4f93", "CCN");
				zTextBoxColumnStyleInfo12.ColumnName = "CanadaCCNNumber";
				zTextBoxColumnStyleInfo12.IsVisible = false;
				zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|50d3bc48-e4b4-4183-aecf-1d7df7af2b5b", "PCN");
				zTextBoxColumnStyleInfo13.ColumnName = "CanadaPCNNumber";
				zTextBoxColumnStyleInfo13.IsVisible = false;

				this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
				this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			}
		}
	}
}

