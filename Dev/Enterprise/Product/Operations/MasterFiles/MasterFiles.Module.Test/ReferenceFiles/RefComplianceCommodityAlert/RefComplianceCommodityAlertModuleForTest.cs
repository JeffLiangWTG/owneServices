using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefComplianceCommodityAlertModuleForTest : RefComplianceCommodityAlertModule
	{
		public IFilterControl GetNewFilterControlForTest() => GetNewFilterControl();

		public IBusinessObjectCollection GetNewGridCollectionForTest() => GetNewGridCollection();

		public FilterBusinessObject GetNewFilterBusinessObjectForTest() => GetNewFilterBusinessObject();

		public ZDisplayGrid Grid_Exposed
		{
			get => Grid;
		}

		public void OnSetRiskStatus_Exposed(string riskStatus)
		{
			SetRiskStatus(riskStatus);
		}

		public MenuItem[] GetNewActionMenuItemsForTest() => GetNewActionMenuItems();

		public MenuItem SetRiskStatusToHighRiskMenuItem_Exposed => SetRiskStatusToHighRiskMenuItem;

		public MenuItem SetRiskStatusToPossibleRiskMenuItem_Exposed => SetRiskStatusToPossibleRiskMenuItem;
	}
}
