//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefTariffVersionValidation
//
//    This class should be used for overriding validation in AutoCusRefTariffVersionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	using CargoWise.EntityFramework;

	public class CusRefTariffVersionValidation : AutoCusRefTariffVersionValidation
	{
		public CusRefTariffVersionValidation(AutoCusRefTariffVersion parent) : base(parent)
		{
		}

		protected override void CheckCRT_Version()
		{
			var code = Parent.CRT_Version;
			if (code.IsEmpty)
			{
				Parent.CRT_VersionInfo.AddError(MandatoryValidation.MustBeEnteredMessage(Parent.CRT_VersionInfo.HumanReadableName));
			}
			else if (!Parent.IsInDatabase && Parent.Factory.ExistsInDatabase(CusRefTariffVersionSchema.Constants.TableName, new ZQuery(CusRefTariffVersionSchema.CRT_Version, code)))
			{
				Parent.CRT_VersionInfo.AddError(Res.GetString("DEE80339-AC19-409E-92F4-15216A1CB50A", "There's a version with the code existed in DB. The Code should be unique."));
			}
		}

		protected override void CheckCRT_Description()
		{
			var description = Parent.CRT_Description;
			if (description.IsEmpty)
			{
				Parent.CRT_DescriptionInfo.AddError(MandatoryValidation.MustBeEnteredMessage(Parent.CRT_DescriptionInfo.HumanReadableName));
			}
			else if (!Parent.IsInDatabase || Parent.CRT_DescriptionInfo.HasChanges)
			{
				var query = new ZQuery(CusRefTariffVersionSchema.CRT_Description, description);
				query.AddToFilter(CusRefTariffVersionSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				if (Parent.Factory.ExistsInDatabase(CusRefTariffVersionSchema.Constants.TableName, query))
				{
					Parent.CRT_DescriptionInfo.AddError(Res.GetString("C6C18CA6-77D5-41C6-B7A6-6A293D9823F5", "There's a version with the description existed in DB. The Description should be unique."));
				}
			}
		}

		protected override void CheckCRT_RN_NKCountryCode()
		{
			CheckDuplicateVersionWithTheSameCountryCodeAndEffectiveDate(Parent.CRT_RN_NKCountryCodeInfo);
		}

		protected override void CheckCRT_EffectiveDate()
		{
			CheckDuplicateVersionWithTheSameCountryCodeAndEffectiveDate(Parent.CRT_EffectiveDateInfo);
		}

		void CheckDuplicateVersionWithTheSameCountryCodeAndEffectiveDate(ZPropertyInfo info)
		{
			if (!Parent.IsInDatabase || Parent.CRT_EffectiveDateInfo.HasChanges)
			{
				var countryCode = Parent.CRT_RN_NKCountryCode;
				var effectiveDate = Parent.CRT_EffectiveDate;

				if (!countryCode.IsEmpty && effectiveDate.IsValid)
				{
					var query = new ZQuery(CusRefTariffVersionSchema.CRT_RN_NKCountryCode, countryCode);
					query.AddToFilter(CusRefTariffVersionSchema.CRT_EffectiveDate, effectiveDate);
					query.AddToFilter(CusRefTariffVersionSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

					if (Parent.Factory.ExistsInDatabase(CusRefTariffVersionSchema.Constants.TableName, query))
					{
						info.AddError(DuplicateVersionError);
					}
				}
			}
		}

		static internal string DuplicateVersionError => Res.GetString("812B1A9D-3B78-4C23-BCD3-7D2D408DA303",
			"There's a tariff version with the same country code and effective date existed in DB.");
	}
}
