using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class OrganisationFilterContentControl : ZUserControl
	{
		public OrganisationFilterContentControl()
		{
			InitializeComponent();

			var defaultFilter = AddFilterAndResizeControl();
			defaultFilter.RemoveFilterButton.Enabled = false;
		}

		internal FilterItemUserControl AddFilterAndResizeControl()
		{
			var newFilter = new FilterItemUserControl();
			newFilter.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
			newFilter.AutoSize = true;
			DynamicFilterPanel.Controls.Add(newFilter);
			DynamicFilterPanel.AutoScroll = false;
			DynamicFilterPanel.AutoScroll = DynamicFilterPanel.Height >= ControlDpiScalingHelper.ScaleToCurrentDpiY(135);
			return newFilter;
		}

		internal void SetupDataContext<TMaster, TCandidate>(DeduplicationResultDetail<TMaster, TCandidate> duplicationResultDetail, PotentialDuplicatesUserControl potentialDuplicatesUserControl)
			where TMaster : IDeduplicationMaster, IDeduplicationGlowObject
			where TCandidate : DuplicationCandidate
		{
			if (duplicationResultDetail is DeduplicationOrganisationResultDetail organisationResultDetail)
			{
				var advancedFilterCriteriaControl = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl;
				FilterOperationUserControl.SetupDataContext(organisationResultDetail, potentialDuplicatesUserControl, advancedFilterCriteriaControl);
			}
		}
	}
}
