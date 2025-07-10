using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.Module
{
	public partial class CustomerServiceTicketFilterControl : ZFilterStripControl
	{
		public CustomerServiceTicketFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				SetCustomLabels();
			}
		}

		void SetCustomLabels()
		{
			foreach (var columnStyle in grid.ColumnStyles)
			{
				var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
				if (textBoxColumnStyle != null)
				{
					switch (textBoxColumnStyle.ColumnName)
					{
						case AutoWorkRequest.Schema.WKR_SelectionCriteria1:
							textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.SelectionCriterion1Caption.Value;
							break;
						case AutoWorkRequest.Schema.WKR_SelectionCriteria2:
							textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.SelectionCriterion2Caption.Value;
							break;
						case AutoWorkRequest.Schema.WKR_SelectionCriteria3:
							textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.SelectionCriterion3Caption.Value;
							break;
						case AutoWorkRequest.Schema.WKR_SelectionCriteria4:
							textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.SelectionCriterion4Caption.Value;
							break;
						case AutoWorkRequest.Schema.WKR_SelectionCriteria5:
							textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.SelectionCriterion5Caption.Value;
							break;
					}
				}
			}
		}
	}
}
