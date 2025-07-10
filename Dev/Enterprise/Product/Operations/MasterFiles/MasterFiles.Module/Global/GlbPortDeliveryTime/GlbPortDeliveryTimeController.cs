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
	public class GlbPortDeliveryTimeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GlbPortDeliveryTimeController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbPortDeliveryTime; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlbPortDeliveryTime; }
		}

		#region SecurityCheckpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GlbPortDeliveryTimeDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GlbPortDeliveryTimeEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GlbPortDeliveryTimeNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GlbPortDeliveryTimeView; }
		}

		#endregion

		public override Type TypeOfTopLevelBusinessObject
		{
			//TODO: Return your BusinessObject type here.
			//		If you are using a collection as your top level BusinessEntity,
			//		override GetLoadedBusinessEntityInLocalFactory() and GetNewBusinessEntityInLocalFactory()
			get { return typeof(GlbPortDeliveryTime); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbPortDeliveryTimeForm((GlbPortDeliveryTime)businessEntity);
		}
	}
}
