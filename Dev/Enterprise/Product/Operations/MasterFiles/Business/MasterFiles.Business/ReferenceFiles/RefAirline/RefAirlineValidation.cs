using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineValidation : AutoRefAirlineValidation
	{
		public RefAirlineValidation(AutoRefAirline parent) : base(parent)
		{
		}

		protected override void CheckRM_AirlineName1()
		{
			base.CheckRM_AirlineName1();

			MandatoryValidation.CheckEntered(Parent.RM_AirlineName1Info);

			ZQuery query = new ZQuery(RefAirlineSchema.RM_AirlineName1, Parent.RM_AirlineName1);
			query.AddToFilter(JoinCondition.And, RefAirlineSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(JoinCondition.And, RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.Equal, Parent.RM_EagleAddedAirlinePrefixOrAccountingCode);
			query.AddToFilter(JoinCondition.And, RefAirlineSchema.RM_ThreeLetterCode, SQLComparisonOperator.Equal, Parent.RM_ThreeLetterCode);
			if (Parent.Factory.ExistsInDatabase(RefAirlineSchema.Constants.TableName, query))
			{
				Parent.RM_AirlineName1Info.AddError(Res.GetString("b3a68681-2930-4fe4-831b-bb3cd44e5a2e", "This Airline Name 1 already exists. Please enter another name."));
			}
		}

		protected override void CheckRM_EagleAddedAirlinePrefixOrAccountingCode()
		{
			base.CheckRM_EagleAddedAirlinePrefixOrAccountingCode();

			if (!Parent.RM_EagleAddedAirlinePrefixOrAccountingCode.IsEmpty)
			{
				if (!Parent.RM_EagleAddedAirlinePrefixOrAccountingCode.IsNumbersOnlyOrEmpty && !Parent.RM_MembershipFlagIATA)
				{
					Parent.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo.AddError(Res.GetString("4461eac5-d678-408b-9922-e2e5065de219", "Only numeric code is allowed."));
				}
				else if (Parent.RM_IsActive)
				{
					ZQuery query = new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, Parent.RM_EagleAddedAirlinePrefixOrAccountingCode);
					query.AddToFilter(JoinCondition.And, RefAirlineSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					query.AddToFilter(JoinCondition.And, RefAirlineSchema.RM_IsActive, SQLComparisonOperator.Equal, true);
					if (Parent.Factory.ExistsInDatabase(RefAirlineSchema.Constants.TableName, query))
					{
						Parent.RM_EagleAddedAirlinePrefixOrAccountingCodeInfo.AddError(Res.GetString("bf29cc11-4944-4b7e-b50c-d6f360becd09", "This Airline Numeric Code already exists. Please ensure you have entered the correct Airline Numeric Code."));
					}
				}
			}
		}

		protected override void CheckRM_ThreeLetterCode()
		{
			base.CheckRM_ThreeLetterCode();

			if (!Parent.RM_ThreeLetterCode.IsEmpty && Parent.RM_IsActive)
			{
				ZQuery query = new ZQuery(RefAirlineSchema.RM_ThreeLetterCode, Parent.RM_ThreeLetterCode);
				query.AddToFilter(JoinCondition.And, RefAirlineSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(JoinCondition.And, RefAirlineSchema.RM_IsActive, SQLComparisonOperator.Equal, true);
				if (Parent.Factory.ExistsInDatabase(RefAirlineSchema.Constants.TableName, query))
				{
					Parent.RM_ThreeLetterCodeInfo.AddError(Res.GetString("f1b7a5fd-9269-4dd0-b3b4-549f2787e5c1", "This Three Letter Code already exists. Please ensure you have entered the correct code."));
				}
			}
		}

		protected override void CheckRM_TwoCharacterCode()
		{
			base.CheckRM_TwoCharacterCode();

			MandatoryValidation.CheckEntered(Parent.RM_TwoCharacterCodeInfo);
		}

		protected override void CheckRM_AddressLine1()
		{
			base.CheckRM_AddressLine1();

			MandatoryValidation.CheckEntered(Parent.RM_AddressLine1Info);
		}

		protected override void CheckRM_AirlineCity()
		{
			base.CheckRM_AirlineCity();

			MandatoryValidation.CheckEntered(Parent.RM_AirlineCityInfo);
		}

		protected override void CheckRM_AirlineCountry()
		{
			base.CheckRM_AirlineCountry();

			MandatoryValidation.CheckEntered(Parent.RM_AirlineCountryInfo);
		}

		protected override void CheckRM_ContactNameOCIIdentifier()
		{
			base.CheckRM_ContactNameOCIIdentifier();
			CheckContainsOnlyAlphaCharacters(Parent.RM_ContactNameOCIIdentifierInfo);
		}

		protected override void CheckRM_ContactPhoneOCIIdentifier()
		{
			base.CheckRM_ContactPhoneOCIIdentifier();
			CheckContainsOnlyAlphaCharacters(Parent.RM_ContactPhoneOCIIdentifierInfo);
		}

		#region RM_IsUpdatable

		protected override void CheckRM_IsUpdatable()
		{
			base.CheckRM_IsUpdatable();
			if (!Parent.RM_IsUpdatable)
			{
				Parent.RM_IsUpdatableInfo.AddWarning(Res.GetString("150b5e2e-5c0c-4a16-8d51-f1985e91acff", "When unselected, no updates will be provided for this airline. User settings will be kept."));
			}
		}

		#endregion

		static void CheckContainsOnlyAlphaCharacters(ZPropertyInfo info)
		{
			if (!UpToTwoAlphaCharactersRegex.IsMatch(info.Value.ToString()))
			{
				info.AddError(Res.GetString("ce6b39f1-c5eb-44f7-b53a-e3202937cd23", "Can contain only alpha characters"));
			}
		}

		static Regex UpToTwoAlphaCharactersRegex => upToTwoAlphaCharactersRegex ??
													(upToTwoAlphaCharactersRegex =
														new Regex("^[a-zA-Z]*$", RegexOptions.Compiled));

		[ThreadStatic]
		static Regex upToTwoAlphaCharactersRegex;
	}
}
