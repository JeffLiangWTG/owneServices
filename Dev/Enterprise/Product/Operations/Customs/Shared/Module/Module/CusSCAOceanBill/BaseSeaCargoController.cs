using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class BaseSeaCargoController : ZController
	{
		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			return CheckSeaCargoBelongsToThisCountry(sourceEntity)
				? base.ShowViewForm(sourceEntity)
				: null;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			return CheckSeaCargoBelongsToThisCountry(sourceEntity)
				? base.ShowEditForm(sourceEntity)
				: null;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			return CheckSeaCargoBelongsToThisCountry(sourceEntity)
				? base.ShowDeleteForm(sourceEntity)
				: null;
		}

		bool CheckSeaCargoBelongsToThisCountry(BusinessObject sourceEntity)
		{
			bool result = true;
			var oceanBill = sourceEntity as BaseCusSCAOceanBill;
			var country = oceanBill?.Branch?.Company?.Country;

			if (country != null && country.PK != GlbCompany.CurrentCompany.Country.PK)
			{
				Globals.Message.ShowError(Res.GetString("F84A9C25-B6F9-4A07-9E62-79CA36DB05FD", "You are trying to view a Sea Cargo entry that belongs to a different country.\r\nPlease log into a company for the relevant country, which is '{0}', if you wish to view this job.", country.RN_Desc));
				result = false;
			}

			return result;
		}

		#region Implement

		public override ControllerID ID => ControllerIDs.Customs.BaseSeaCargo;

		public override ModuleIdentifier ModuleID => null;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(BaseCusSCAOceanBill);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsMain;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsMain;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsMain;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsMain;

		#endregion
	}
}
