using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefCityTownValidation : AutoRefCityTownValidation
	{
		public RefCityTownValidation(AutoRefCityTown parent) : base(parent)
		{
		}

		#region International Name

		protected override void CheckR9_InternationalName()
		{
			base.CheckR9_InternationalName();
			MandatoryValidation.CheckEntered(Parent.R9_InternationalNameInfo, Res.GetString("4d44c2a8-8ff6-4a7b-a0c1-703c2b589569", "International Name"));
		}

		#endregion

		#region Country

		protected override void CheckR9_RN_NKCountry()
		{
			base.CheckR9_RN_NKCountry();
			MandatoryValidation.CheckEntered(Parent.R9_RN_NKCountryInfo, Res.GetString("8c7af925-2f70-4b89-85d2-9cacfa51d026", "Country/Region"));
			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("13978c1a-1a80-4b97-88cf-81906c06336e", "Please enter a valid Country/Region"), Parent.R9_RN_NKCountryInfo);
		}

		#endregion

		#region State

		protected override void CheckR9_RW_NKState()
		{
			base.CheckR9_RW_NKState();

			if (Parent.R9_RW_NKState.IsEmpty && !Parent.R9_RN_NKCountry.IsEmpty)
			{
				Parent.R9_RW_NKStateInfo.AddWarning(Res.GetString("68316c56-ab40-4833-beb4-6f284e708ab0", "State is required in most countries/regions"));
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("1be55467-fad7-4b74-ad95-bb74aaa3d38f", "Please enter a valid State"), Parent.R9_RW_NKStateInfo);

				if (Parent.Country != null && Parent.State != null && Parent.State.RW_RN_NKCountryCode != Parent.Country.RN_Code)
				{
					Parent.R9_RW_NKStateInfo.AddError(ResString.GetMultilingualString("5b992176-20c1-41df-af1d-c6c20b6d445a", "Please choose a state that is in '{0}'", Parent.Country.RN_DescMultilingual));
				}
			}
		}

		#endregion
	}
}
