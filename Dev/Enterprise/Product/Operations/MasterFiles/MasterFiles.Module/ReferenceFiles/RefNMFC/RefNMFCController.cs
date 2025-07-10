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
	/// <summary>
	/// Module Controller for RefNMFC.
	/// </summary>
	public class RefNMFCController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public RefNMFCController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.RefNMFC;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefNMFC; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefNMFC); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefNMFCForm((RefNMFC)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.RefNMFCModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.RefNMFCModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.RefNMFCModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.RefNMFC; }
		}
	}
}
