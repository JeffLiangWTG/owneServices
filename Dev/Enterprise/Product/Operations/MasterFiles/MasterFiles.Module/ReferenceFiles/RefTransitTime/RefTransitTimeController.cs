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
	public class RefTransitTimeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.RefTransitTime; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefTransitTime); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefTransitTime; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefTransitTimeForm((RefTransitTime)businessEntity);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TransitTimeDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.TransitTimeEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TransitTimeNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TransitTimeView; }
		}

		#endregion
	}
}
