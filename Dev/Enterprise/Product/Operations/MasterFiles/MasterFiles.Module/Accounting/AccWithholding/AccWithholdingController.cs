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
	public class AccWithholdingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccWithholding; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccWithholding; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccWithholding); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccWithholdingForm((AccWithholding)businessEntity);
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			Globals.Message.Show(sourceEntity.ReasonForNotAbleToDelete);
			return null;
		}

		public override IZForm ShowNewForm()
		{
			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				return base.ShowNewForm();
			}
			else
			{
				Globals.Message.Show(Res.GetString("48c9c1d1-9040-47ab-8ee5-a98aec89006a", "You are not allowed to add a new withholding tax code"));
				return null;
			}
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get
			{
				return Env.Security.WHTTaxRatesModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get
			{
				return Env.Security.WHTTaxRatesModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get
			{
				return Env.Security.WHTTaxRatesModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get
			{
				return Env.Security.WHTTaxRates;
			}
		}
	}
}
