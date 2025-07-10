using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Module Controller for RefVessel.
	/// </summary>
	public class RefVesselZZController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public RefVesselZZController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.RefVesselZZ;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefVesselZZ; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefVesselZZ); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.VesselsModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.VesselsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.VesselsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Vessels; }
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("Does not support New functionality");
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowEditFormNotSupportedException("Does not support Edit functionality");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowViewFormNotSupportedException("Does not support View functionality");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowDeleteFormNotSupportedException("Does not support Delete functionality");
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleTemplateCopyNotSupportedException("Does not support Template Copy functionality");
		}
	}
}
