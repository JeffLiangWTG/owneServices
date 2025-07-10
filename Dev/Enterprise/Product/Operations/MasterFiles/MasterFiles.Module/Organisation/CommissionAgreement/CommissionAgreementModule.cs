using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
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
	public class CommissionAgreementModule : ZFilterGridModule
	{
		public CommissionAgreementModule()
		{
		}

		#region ID

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.OrgCommissionAgreement; }
		}

		#endregion

		#region Allowed Actions

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region Controller

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
		}

		#endregion

		#region Grid Collection

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgCommissionAgreementCollection(Factory);
		}

		#endregion

		#region Filter Business Object

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CommissionAgreementFilterBusinessObject();
		}

		#endregion

		#region Filter Control

		protected override IFilterControl GetNewFilterControl()
		{
			return new CommissionAgreementFilterControl(GridCollection, (CommissionAgreementFilterBusinessObject)FilterBusinessObject);
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.OpportunityManagement; }
		}
		public override SecurityCheckpoint[] GetSecurityCheckpointForPopups()
		{
			if (securityCheckpointForPopups == null)
			{
				securityCheckpointForPopups = new[] { Env.Security.CommissionAgreementView };
			}
			return securityCheckpointForPopups;
		}
		SecurityCheckpoint[] securityCheckpointForPopups;
		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region Action Menus

		public bool ShouldShowCalculationQueueActionMenuItems { get; set; }

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			if (ShouldShowCalculationQueueActionMenuItems)
			{
				result.Add(new ZMenuItem(ResString.GetMultilingualString("8e274838-cc86-411c-a8bf-e88534ce6436", "&Remove from Calculation Queue"), delegate
				{ RemoveFromCalculationQueue(); }));
			}

			return result.ToArray();
		}

		void RemoveFromCalculationQueue()
		{
			if (!Env.Security.CommissionCalculationQueueEdit.IsAllowed)
			{
				Env.Security.CommissionCalculationQueueEdit.ShowError();
			}
			else
			{
				var selectedBusinessObjects = SelectedBusinessObjects?.ToArray() ?? Enumerable.Empty<BusinessObject>();

				if (selectedBusinessObjects.Any())
				{
					var warningMessageResult = Globals.Message.Show(
						Res.GetString("91744376-eee1-4b47-820d-0f63b6cd75ac", @"Are you sure you wish to remove the Agreement(s) from the Calculation Queue?"),
						Res.GetString("c5bd5061-4dd9-4b46-a922-c2766b8eea52", "Editing Calculation Queue"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);

					if (warningMessageResult == DialogResult.Yes)
					{
						foreach (var queue in Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, selectedBusinessObjects.Select(x => x.PK))))
						{
							queue.Delete();
						}

						Factory.Save();
						PerformSearch();
					}
				}
				else
				{
					ShowNoSelectedMessage();
				}
			}
		}

		#endregion
	}
}
