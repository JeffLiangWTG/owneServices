using System;

using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GenCustomAddOnRuleController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GenCustomAddOnRule; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GenCustomAddOnRule; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GenCustomAddOnRule); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GenCustomAddOnRuleForm((GenCustomAddOnRule)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AddOnRulesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AddOnRulesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AddOnRulesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AddOnRulesView; }
		}

		#endregion
	}
}
