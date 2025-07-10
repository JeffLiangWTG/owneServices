using System;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AutoratingDateFilteringUserControl : ZUserControl
	{
		public AutoratingDateFilteringUserControl()
			: base()
		{
			InitializeComponent();

			filterTypeDropEdit.SelectedIndexChanged += (s, e) =>
			{
				autoratingDateFilteringChargeGroupAndCustomizedUserControl.Visible = filterTypeDropEdit.Text == Constants.RatingDateFilterTypes.Codes.Custom;
			};
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var orgHeader = CurrentDataItem as OrgHeader;
			autoratingDateFilteringChargeGroupAndCustomizedUserControl.Visible = orgHeader.MiscServ.OM_AutoratingDateFiltering == Constants.RatingDateFilterTypes.Codes.Custom;
		}
	}
}
