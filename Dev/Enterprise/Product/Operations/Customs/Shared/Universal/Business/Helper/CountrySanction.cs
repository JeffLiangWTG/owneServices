using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Interface stub to be used to validate new customs reference data from other WTG modules")]
	public class CountrySanction : Integration.Customs.ISanctionDataProvider
	{
		public Integration.Customs.ISanctionData ConditionState(ZString issuingCountry, ZString destinationCountry, ZDateTime effectiveDate)
		{
			return new SanctionValidation(issuingCountry, destinationCountry, effectiveDate);
		}

		class SanctionValidation : Integration.Customs.ISanctionData
		{
			public SanctionValidation(ZString issuingCountry, ZString destinationCountry, ZDateTime effectiveDate)
			{
				this.issuingCountry = issuingCountry;
				this.destinationCountry = destinationCountry;
				this.effectiveDate = effectiveDate.IsValid ? effectiveDate : ZDateTime.UtcToday;
				conditionDesc = ZString.Empty;
				conditionType = ZString.Empty;
				isActive = false;
				GetDestinationCountrySanction();
			}
			ZString issuingCountry;
			readonly ZString destinationCountry;
			ZDateTime effectiveDate;
			ZString conditionDesc;
			ZString conditionType;
			bool isActive;

			#region Integration.Customs.ISanctionData Implementation

			bool Integration.Customs.ISanctionData.IsConditionActive
			{
				get
				{
					return isActive;
				}
			}

			ZString Integration.Customs.ISanctionData.ConditionType
			{
				get
				{
					return conditionType;
				}
			}

			ZString Integration.Customs.ISanctionData.ConditionDescription
			{
				get
				{
					return conditionDesc;
				}
			}

			#endregion

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Sanction information website address")]
			void GetDestinationCountrySanction()
			{
				issuingCountry = Core.Constants.CountryCodes.UnitedStates;   // see comment below
				switch (destinationCountry)
				{
					case Core.Constants.CountryCodes.LibyanArabJamahiriya:
						if (issuingCountry == Core.Constants.CountryCodes.UnitedStates && effectiveDate.IsValid) // issuingCountry & effectiveDate input values processing are not required until data is created in reference table - they need to be used however to avoid CodeAnalysis errors
						/*
							It will be the date to use in the testing of the records currency.  
							I.e. it must be between the Start and End Dates in the ZZ Condition Records.  
							If not supplied the Effective date of today is assumed.
						*/
						{
							conditionDesc = (NoResString)"OFAC Sanctions Programs exists for Country refer to: https://www.treasury.gov/resource-center/sanctions/Programs/pages/libya.aspx";
							conditionType = "ALL";
							isActive = true;
						}
						break;
					case Core.Constants.CountryCodes.KoreaNorth:
						if (issuingCountry == Core.Constants.CountryCodes.UnitedStates && effectiveDate.IsValid) // not required until data is created in reference table
						{
							conditionDesc = (NoResString)"OFAC Sanctions Programs exists for Country refer to: https://www.treasury.gov/resource-center/sanctions/Programs/pages/nkorea.aspx";
							conditionType = "ALL";
							effectiveDate = ZDateTime.UtcToday;
							isActive = true;
						}
						break;
					case Core.Constants.CountryCodes.Iran:
						if (issuingCountry == Core.Constants.CountryCodes.UnitedStates && effectiveDate.IsValid) // not required until data is created in reference table
						{
							conditionDesc = "OFAC Sanctions Programs exists for Country refer to: https://www.treasury.gov/resource-center/sanctions/Programs/Pages/iran.aspx";
							conditionType = "ALL";
							effectiveDate = ZDateTime.UtcToday;
							isActive = true;
						}
						break;
					case Core.Constants.CountryCodes.Belarus:
						if (issuingCountry == Core.Constants.CountryCodes.UnitedStates && effectiveDate.IsValid) // not required until data is created in reference table
						{
							conditionDesc = "OFAC Sanctions Programs exists for Country refer to: https://www.treasury.gov/resource-center/sanctions/Programs/pages/belarus.aspx";
							conditionType = "ALL";
							effectiveDate = ZDateTime.UtcToday;
							isActive = true;
						}
						break;
					default:
						conditionDesc = "";
						conditionType = "";
						effectiveDate = ZDateTime.Empty;
						isActive = false;
						break;
				}
			}
		}
	}
}
