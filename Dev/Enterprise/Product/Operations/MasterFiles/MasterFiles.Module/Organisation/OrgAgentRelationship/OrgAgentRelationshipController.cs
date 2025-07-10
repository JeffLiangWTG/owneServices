using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrgAgentRelationshipController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.ProfitShare; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgAgentRelationship); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrgProfitShareForm((OrgAgentRelationship)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.ProfitShare;
			}
		}

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ProfitShareView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ProfitShareDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ProfitShareEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ProfitShareNew; }
		}

		#endregion
	}
}
