using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefComplianceCommodityAlertModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefComplianceCommodityAlert; }
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.RefComplianceCommodityAlert;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefComplianceCommodityAlert);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefComplianceCommodityAlertFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefComplianceCommodityAlertFilterControl(GridCollection, (RefComplianceCommodityAlertFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefComplianceCommodityAlertCollection(Factory);
		}

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		public override bool AllowEdit => true;

		public override bool SupportsWorkflow => false;

		#region Menu / Toolbar

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menu = base.GetNewActionMenuItems();

			if(!Env.Security.RefComplianceCommodityAlertEdit.IsAllowed)
			{
				return menu;
			}

			var result = new List<MenuItem>(menu);

			result.Add(new ZMenuItem("-"));
			result.Add(SetRiskStatusToHighRiskMenuItem);
			result.Add(SetRiskStatusToPossibleRiskMenuItem);

			return result.ToArray();
		}

		protected MenuItem SetRiskStatusToHighRiskMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.RefComplianceCommodityAlert.SetRiskStatusToHighRiskMenuItem", "Set Risk Status to High Risk"), new EventHandler((object sender, EventArgs e) => { SetRiskStatus(ComplianceRiskStatusCodeList.Codes.HighRisk); })); }
		}

		protected MenuItem SetRiskStatusToPossibleRiskMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.RefComplianceCommodityAlert.SetRiskStatusToPossibleRiskMenuItem", "Set Risk Status to Possible Risk"), new EventHandler((object sender, EventArgs e) => { SetRiskStatus(ComplianceRiskStatusCodeList.Codes.PossibleRisk); })); }
		}

		protected void SetRiskStatus(string riskStatus)
		{
			if (Grid.SelectedRowCount == 0)
			{
				ShowNoSelectedMessage();
			}
			else
			{
				var needToSave = false;

				var complianceCommodityAlerts = Factory.Load<RefComplianceCommodityAlert>(new ZQuery(RefComplianceCommodityAlertSchema.PK, Array.ConvertAll(Grid.SelectedElements, x => x.PK)));
				complianceCommodityAlerts.ForEach(x =>
				{
					if (x.RCR_CommodityRiskStatus != riskStatus)
					{
						x.RCR_CommodityRiskStatus = riskStatus;
						needToSave = true;
					}
				});

				if (needToSave)
				{
					Factory.Save();
				}

				var message = Res.GetString("68689A35-2A16-4197-8FFE-300035D91050", "Selected compliance lists were updated to {0}",
					(riskStatus == ComplianceRiskStatusCodeList.Codes.HighRisk ? ComplianceRiskStatusCodeList.Descriptions.HighRisk : ComplianceRiskStatusCodeList.Descriptions.PossibleRisk));

				Globals.Message.ShowInformation(message);
			}
		}

		#endregion
	}
}
