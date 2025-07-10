using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefPostCodeValidation : AutoRefPostCodeValidation
	{
		public RefPostCodeValidation(AutoRefPostCode parent) : base(parent)
		{
		}

		#region Post Code

		protected override void CheckRK_CityTownPostCode()
		{
			base.CheckRK_CityTownPostCode();
			MandatoryValidation.CheckEntered(Parent.RK_CityTownPostCodeInfo);
			CheckDuplicates(Parent.RK_CityTownPostCodeInfo);
		}

		#endregion

		#region Country

		protected override void CheckRK_RN_NKCountry()
		{
			base.CheckRK_RN_NKCountry();
			MandatoryValidation.CheckEntered(Parent.RK_RN_NKCountryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RK_RN_NKCountryInfo);
			CheckDuplicates(Parent.RK_RN_NKCountryInfo);
		}

		#endregion

		#region IsActive

		protected override void CheckRK_IsActive()
		{
			base.CheckRK_IsActive();
			CheckDuplicates(Parent.RK_IsActiveInfo);
		}

		#endregion

		void CheckDuplicates(ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(Parent.RK_CityTownPostCode) && !string.IsNullOrEmpty(Parent.RK_RN_NKCountry))
			{
				var query = new ZDBOnlyQuery(typeof(RefPostCode));
				query.AddToFilter(RefPostCodeSchema.RK_CityTownPostCode, Parent.RK_CityTownPostCode);
				query.AddToFilter(RefPostCodeSchema.RK_RN_NKCountry, Parent.RK_RN_NKCountry);

				if (Parent.IsInDatabase)
				{
					query.AddToFilter(RefPostCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				}

				if (!Parent.IsInDatabase ||
					Parent.RK_CityTownPostCodeInfo.OriginalValue.ToString() != Parent.RK_CityTownPostCode.ToString() ||
					Parent.RK_RN_NKCountryInfo.OriginalValue.ToString() != Parent.RK_RN_NKCountry.ToString() ||
					Parent.RK_IsActiveInfo.OriginalValue.ToString() != Parent.RK_IsActive.ToString())
				{
					var postcodes = Parent.Factory.Load<RefPostCode>(query);
					var activePostcodes = postcodes.Where(p => p.RK_IsActive).ToList();
					var inActivePostcodes = postcodes.Where(p => !p.RK_IsActive).ToList();

					if (Parent.RK_IsActive)
					{
						if (activePostcodes.Any())
						{
							propertyInfo.AddError(duplicatePostcodeError);
						}
						else if (inActivePostcodes.Any())
						{
							propertyInfo.AddError(inActivePostcodeError);
						}
					}
					else
					{
						if (inActivePostcodes.Any())
						{
							propertyInfo.AddError(duplicatePostcodeError);
						}
					}
				}
			}
		}

		string duplicatePostcodeError => Res.GetString("11292a87-8f27-4522-b1c1-70d883b6c51e", "(Postcode: {0} + Country/Region: {1} + Active Status: {2}) already exists, please retrieve and use it.", Parent.RK_CityTownPostCode, Parent.RK_RN_NKCountry, Parent.RK_IsActive ? (NoResString)"Active" : (NoResString)"Inactive");
		string inActivePostcodeError => Res.GetString("8129ea83-8f27-4522-b1c5-701883b6c51a", "(Postcode: {0} + Country/Region: {1} + Active Status: Inactive) already exists, please retrieve and activate it.", Parent.RK_CityTownPostCode, Parent.RK_RN_NKCountry);
	}
}
