using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class GlbCompanyWrapper : MasterFiles.Business.GlbCompanyWrapper, IUSGlbCompanyWrapper
	{
		protected GlbCompanyWrapper(GlbCompany company)
			: base(company)
		{
		}

		#region PasswordCollection

		[ChildEditable]
		public GlbCompanyCredentialCollection PasswordCollection
		{
			get
			{
				if (passwordCollection == null)
				{
					passwordCollection = new GlbCompanyCredentialCollection(Company);
					passwordCollection.Load();
					RegisterEditableChildObject(passwordCollection);
				}

				return passwordCollection;
			}
		}
		GlbCompanyCredentialCollection passwordCollection;

		#endregion

		IGlbExternalPasswordCollection_US IUSGlbCompanyWrapper.PasswordCollection => PasswordCollection;

		public override bool IsValidWrapper
		{
			get
			{
				var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(currentCountryCode);
				return countryCode == Core.Constants.CountryCodes.UnitedStates && Company.GC_RN_NKCountryCode == currentCountryCode;
			}
		}
	}
}
