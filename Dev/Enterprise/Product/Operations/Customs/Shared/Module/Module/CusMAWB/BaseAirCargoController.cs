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
	public class BaseAirCargoController : ZController
	{
		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			return CheckAirCargoBelongsToThisCountry(sourceEntity)
				? base.ShowViewForm(sourceEntity)
				: null;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			return CheckAirCargoBelongsToThisCountry(sourceEntity)
				? base.ShowEditForm(sourceEntity)
				: null;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			return CheckAirCargoBelongsToThisCountry(sourceEntity)
				? base.ShowDeleteForm(sourceEntity)
				: null;
		}

		bool CheckAirCargoBelongsToThisCountry(BusinessObject sourceEntity)
		{
			bool result = true;

			var mawb = sourceEntity as CusMAWB;
			RefCountry country = null;

			if (mawb is null)
			{
				var hawb = sourceEntity as CusHAWB;
				country = hawb?.MAWB?.Branch?.Company?.Country;
			}
			else
			{
				country = mawb?.Branch?.Company?.Country;
			}

			if (country != null && country.PK != GlbCompany.CurrentCompany.Country.PK)
			{
				var message = Res.GetString("21665109-748e-4a52-98d1-2db34562a66b", "You are trying to view an {0} that belongs to a different country. Please log into a company which country is '{1}' and try again.", mawb is null
					? Res.GetString("710c9964-516f-4397-8671-5d9fa8df0a33", "Air Cargo House Bill")
					: Res.GetString("7e454e64-f274-4119-8379-a2a5b6fe7950", "Air Cargo"), country.RN_Desc);
				Globals.Message.ShowError(message);
				result = false;
			}

			return result;
		}

		#region Implement

		public override ControllerID ID => ControllerIDs.Customs.BaseAirCargo;

		public override ModuleIdentifier ModuleID => null;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusMAWB);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsMain;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsMain;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsMain;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsMain;

		#endregion
	}
}
