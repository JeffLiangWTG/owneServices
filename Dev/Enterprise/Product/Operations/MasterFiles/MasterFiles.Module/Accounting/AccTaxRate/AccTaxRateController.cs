using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccTaxRateController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccTaxRate; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccTaxRate; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccTaxRate); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccTaxRateForm((AccTaxRate)businessEntity);
		}

		public override IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("3f3ecad1-d6a6-43c3-bc7e-e09f658e227d", "You are not allowed to add a new tax code"));
			return null;
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GSTTaxRatesModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GSTTaxRatesModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GSTTaxRatesModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GSTTaxRates; }
		}
	}
}
