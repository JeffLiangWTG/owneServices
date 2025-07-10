using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefComplianceListModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefComplianceList; }
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.RefComplianceList;

		public RefComplianceListSecurityProvider SecurityProvider
		{
			get
			{
				if (securityProvider == null)
				{
					securityProvider = new RefComplianceListSecurityProvider();
				}
				return securityProvider;
			}
		}

		RefComplianceListSecurityProvider securityProvider;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefComplianceList);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefComplianceListFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefComplianceListFilterControl(GridCollection, (RefComplianceListFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefComplianceListCollection(Factory);
		}

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		public override bool AllowEdit => true;

		public override bool SupportsWorkflow => true;

		#region Menu / Toolbar

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menu = base.GetNewActionMenuItems();

			if (!SecurityProvider.HasEditConfigurationSecurity)
			{
				return menu;
			}

			var result = new List<MenuItem>(menu);

			result.Add(new ZMenuItem("-"));
			result.Add(IncludeComplianceListMenuItem);
			result.Add(ExcludeComplianceListMenuItem);

			return result.ToArray();
		}

		protected MenuItem IncludeComplianceListMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.RefComplianceList.IncludeComplianceList", "&Include Selected Lists"), new EventHandler((object sender, EventArgs e) => { IncludeExclude(false); })); }
		}

		protected MenuItem ExcludeComplianceListMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.RefComplianceList.ExcludeComplianceList", "&Exclude Selected Lists"), new EventHandler((object sender, EventArgs e) => { IncludeExclude(true); })); }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.RefComplianceListWorkflowDescriptorCode; }
		}

		protected void IncludeExclude(bool exclude)
		{
			if (Grid.SelectedRowCount == 0)
			{
				ShowNoSelectedMessage();
			}
			else
			{
				var needToSave = false;

				var complianceLists = Factory.Load<RefComplianceList>(new ZQuery(RefComplianceListSchema.PK, GridSelectedElements));
				complianceLists.ForEach(x =>
				{
					if (x.RCL_IsExcluded != exclude)
					{
						x.RCL_IsExcluded = exclude;
						needToSave = true;
					}
				});

				if (needToSave)
				{
					Factory.Save();
				}
			}
		}

		ZGuid[] GridSelectedElements
		{
			get { return Array.ConvertAll(Grid.SelectedElements, x => x.PK); }
		}

		#endregion
	}
}
