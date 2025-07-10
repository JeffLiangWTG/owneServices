using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrgAgentRelationshipModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ProfitShare; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ProfitShare);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgAgentRelationshipFilterControl(GridCollection, (OrgAgentRelationshipFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgAgentRelationshipCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgAgentRelationshipFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ProfitShare; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region New Menu Items

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			MenuItem[] result = base.GetNewStandardMenuItems();
			CodeDescriptionPairList profitShareTypes = new OrgAgentRelationshipLookups(null).ProfitShareTypeList;
			NewMenuItem.MenuItems.Add(new ZMenuItem((NoResString)profitShareTypes.GetDescriptionFromCode(OrgAgentRelationship.ProfitShareTypes.Standard), HandleNewClick));
			NewMenuItem.MenuItems.Add(new ZMenuItem((NoResString)profitShareTypes.GetDescriptionFromCode(OrgAgentRelationship.ProfitShareTypes.AgencyProfile), HandleNewAgentProfileClick));
			return result;
		}

		void HandleNewAgentProfileClick(object sender, EventArgs e)
		{
			OrgProfitShareForm form = (OrgProfitShareForm)ShowNewForm();
			((OrgAgentRelationship)form.BusinessEntity).O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
		}

		#endregion
	}
}
