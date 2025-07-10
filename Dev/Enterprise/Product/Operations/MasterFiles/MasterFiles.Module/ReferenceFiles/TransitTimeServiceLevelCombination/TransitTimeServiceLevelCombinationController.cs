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
	public class TransitTimeServiceLevelCombinationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.TransitTimeServiceLevelCombination; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(TransitTimeServiceLevelCombinationView); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.TransitTimeServiceLevelCombination; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var serviceLevel = Factory.Load<RefServiceLevel>(((TransitTimeServiceLevelCombinationView)businessEntity).TSC_RS);
			return new RefServiceLevelForm(serviceLevel);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.TransitTimeServiceLevelCombinationView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion
	}
}
