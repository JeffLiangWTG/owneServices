using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SimilarOrgMatchesModuleButtonGrid : ZModuleButtonGridWithoutColumnStylesSerialisation
	{
		public SimilarOrgMatchesModuleButtonGrid()
		{
			InitializeComponent();
			InnerGrid.IsWholeRowSelectedOnClick = true;
			ModuleID = ModuleIDs.Organisation;
			NameOfAGridElement = Res.GetData("32042E23-5653-4270-97B8-7FBD04AF78C3", "Similar Organization");
		}

		#region Implementation

		protected override void Edit(BusinessObject selected, object sender)
		{
			SimilarOrgMatchForApproval match = (SimilarOrgMatchForApproval)selected;
			base.Edit(match == null ? null : match.OrgPatternMatch.Header, sender);
		}

		#endregion
	}
}
