using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module
{
	public class NZCConcessionController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public NZCConcessionController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.NZ.Concession; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(NZCConcession); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new NZCConcessionForm(businessEntity as NZCConcession);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.NZCustomsConcession; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.NZCustomsConcession; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NZCustomsConcession; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.NZCustomsConcession; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.NZ.Concession; }
		}
	}
}
