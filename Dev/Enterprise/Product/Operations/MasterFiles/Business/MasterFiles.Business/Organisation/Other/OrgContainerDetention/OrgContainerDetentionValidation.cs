//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgContainerDetentionValidation
//
//    This class should be used for overriding validation in AutoOrgContainerDetentionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContainerDetentionValidation : AutoOrgContainerDetentionValidation
	{
		public OrgContainerDetentionValidation(AutoOrgContainerDetention parent) : base(parent)
		{
		}

		protected override void CheckPD_OH_Client()
		{
			base.CheckPD_OH_Client();

			CheckUniqueness(Parent.PD_OH_ClientInfo);
			if (!Parent.PD_OH_Client.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.PD_OH_ClientInfo);
			}
		}

		protected override void CheckPD_OH_Carrier()
		{
			base.CheckPD_OH_Carrier();

			CheckUniqueness(Parent.PD_OH_CarrierInfo);
			if (!Parent.PD_OH_Carrier.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.PD_OH_CarrierInfo);
			}
		}

		protected override void CheckPD_OriginPortOrCountry()
		{
			base.CheckPD_OriginPortOrCountry();
			CheckUniqueness(Parent.PD_OriginPortOrCountryInfo);
			if (!Parent.PD_OriginPortOrCountry.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.PD_OriginPortOrCountryInfo);
				if (!Parent.PD_OriginPortOrCountryInfo.HasErrors() && Parent.PD_OriginPortOrCountry.Length == 3)
				{
					Parent.PD_OriginPortOrCountryInfo.AddError(Res.GetString("f1f090c9-58fd-4665-af72-73145ce8dfef", "Enter a valid port or country/region code"));
				}
			}
		}

		protected override void CheckPD_DetentionPortOrCountry()
		{
			base.CheckPD_DetentionPortOrCountry();
			CheckUniqueness(Parent.PD_DetentionPortOrCountryInfo);
			if (!Parent.PD_DetentionPortOrCountry.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.PD_DetentionPortOrCountryInfo);
				if (!Parent.PD_DetentionPortOrCountryInfo.HasErrors() && Parent.PD_DetentionPortOrCountry.Length == 3)
				{
					Parent.PD_DetentionPortOrCountryInfo.AddError(Res.GetString("f1f090c9-58fd-4665-af72-73145ce8dfef", "Enter a valid port or country/region code"));
				}
			}
		}

		protected override void CheckPD_ContainerType()
		{
			base.CheckPD_ContainerType();
			CheckUniqueness(Parent.PD_ContainerTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PD_ContainerTypeInfo);
		}

		protected override void CheckPD_FreeDayType()
		{
			base.CheckPD_FreeDayType();
			var info = Parent.PD_FreeDayTypeInfo;

			if (!info.HasErrors())
			{
				MandatoryValidation.CheckEntered(info);
				ListValidation.ErrorIfInvalidCode(info);
			}
		}

		protected override void CheckPD_CreditorType()
		{
			base.CheckPD_CreditorType();

			CheckUniqueness(Parent.PD_CreditorTypeInfo);
			if (Parent.PD_PenaltyType == Enterprise.Core.Constants.ContainerDetentionPenaltyType.STO)
			{
				MandatoryValidation.CheckEntered(Parent.PD_CreditorTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.PD_CreditorTypeInfo);
			}
		}

		protected override void CheckPD_OH_CTO()
		{
			base.CheckPD_OH_CTO();

			CheckUniqueness(Parent.PD_OH_CTOInfo);
			if (!Parent.PD_OH_CTO.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.PD_OH_CTOInfo);
			}
		}

		protected override void CheckPD_PenaltyType()
		{
			base.CheckPD_PenaltyType();
			var info = Parent.PD_PenaltyTypeInfo;

			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info);

			if (!info.HasErrors())
			{
				CheckUniqueness(Parent.PD_PenaltyTypeInfo);
			}
		}

		protected override void CheckPD_Direction()
		{
			base.CheckPD_Direction();
			var info = Parent.PD_DirectionInfo;

			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info);

			if (!info.HasErrors())
			{
				CheckUniqueness(Parent.PD_DirectionInfo);
			}
		}

		void CheckUniqueness(ZPropertyInfo property)
		{
			ZQuery query = new ZQuery();
			if (Parent.PD_OH_Client.IsValid)
			{
				query.AddToFilter(OrgContainerDetentionSchema.PD_OH_Client, Parent.PD_OH_Client);
			}
			else
			{
				query.AddToFilter(OrgContainerDetentionSchema.PD_OH_Client, null);
			}

			if (Parent.PD_OH_Carrier.IsValid)
			{
				query.AddToFilter(OrgContainerDetentionSchema.PD_OH_Carrier, Parent.PD_OH_Carrier);
			}
			else
			{
				query.AddToFilter(OrgContainerDetentionSchema.PD_OH_Carrier, null);
			}

			if (Parent.PD_OH_CTO.IsValid)
			{
				query.AddToFilter(OrgContainerDetentionSchema.PD_OH_CTO, Parent.PD_OH_CTO);
			}
			else
			{
				query.AddToFilter(OrgContainerDetentionSchema.PD_OH_CTO, null);
			}

			if (Parent.PD_PenaltyType != Core.Constants.ContainerDetentionPenaltyType.MDD)
			{
				query.AddToFilter(OrgContainerDetentionSchema.PD_PenaltyType, SQLComparisonOperator.Equal, new string[] { Parent.PD_PenaltyType, Core.Constants.ContainerDetentionPenaltyType.MDD });
			}

			query.AddToFilter(OrgContainerDetentionSchema.PD_CreditorType, Parent.PD_CreditorType);
			query.AddToFilter(OrgContainerDetentionSchema.PD_OriginPortOrCountry, Parent.PD_OriginPortOrCountry);
			query.AddToFilter(OrgContainerDetentionSchema.PD_DetentionPortOrCountry, Parent.PD_DetentionPortOrCountry);
			query.AddToFilter(OrgContainerDetentionSchema.PD_ContainerType, Parent.PD_ContainerType);
			query.AddToFilter(OrgContainerDetentionSchema.PD_Direction, Parent.PD_Direction);
			query.AddToFilter(OrgContainerDetentionSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			var matchedPenalty = Parent.Factory.LoadTop1<OrgContainerDetention>(query);
			if (matchedPenalty != null)
			{
				if (Parent.PD_PenaltyType == Enterprise.Core.Constants.ContainerDetentionPenaltyType.MDD)
				{
					AddUniquenessError(matchedPenalty.PD_PenaltyType, property);
				}
				else
				{
					AddUniquenessError(Parent.PD_PenaltyType, property);
				}
			}
		}

		void AddUniquenessError(ZString penaltyType, ZPropertyInfo property)
		{
			if (!IsPropertyVisibleForUniqueness(property))
			{
				return;
			}

			if (penaltyType == Enterprise.Core.Constants.ContainerDetentionPenaltyType.STO)
			{
				property.AddError(Res.GetString("9e7ef17b-8f55-49a5-b3b2-a480a0c295c7", "You can't enter more than one setting for storage free days."));
			}
			else
			{
				property.AddError(Res.GetString("b2d5b4ed-172a-43f2-abff-19d69d583c7b", "You can't enter more than one setting for detention free days."));
			}
		}

		bool IsPropertyVisibleForUniqueness(ZPropertyInfo property)
		{
			if (property.Name == OrgContainerDetentionSchema.Constants.PD_DetentionPortOrCountry || property.Name == OrgContainerDetentionSchema.Constants.PD_ContainerType)
			{
				return true;
			}

			if (Parent is OrgContainerDetention detention)
			{
				if (Parent.PD_PenaltyType == Enterprise.Core.Constants.ContainerDetentionPenaltyType.STO)
				{
					switch (detention.ParentType)
					{
						case OrgContainerDetentionCollection.ParentType.ServiceEXP:
						case OrgContainerDetentionCollection.ParentType.ServiceIMP:
							return property.Name == OrgContainerDetentionSchema.Constants.PD_OH_Carrier ||
								property.Name == OrgContainerDetentionSchema.Constants.PD_OH_Client;
						case OrgContainerDetentionCollection.ParentType.Consignee:
						case OrgContainerDetentionCollection.ParentType.Consignor:
							return detention.PD_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier
								? property.Name == OrgContainerDetentionSchema.Constants.PD_OH_Carrier
									|| property.Name == OrgContainerDetentionSchema.Constants.PD_Direction
									|| property.Name == OrgContainerDetentionSchema.Constants.PD_OriginPortOrCountry
									|| property.Name == OrgContainerDetentionSchema.Constants.PD_OH_Client
								: property.Name == OrgContainerDetentionSchema.Constants.PD_OH_Carrier
									|| property.Name == OrgContainerDetentionSchema.Constants.PD_OH_CTO
									|| property.Name == OrgContainerDetentionSchema.Constants.PD_CreditorType;
						case OrgContainerDetentionCollection.ParentType.Carrier:
							return property.Name == OrgContainerDetentionSchema.Constants.PD_OH_Client ||
								property.Name == OrgContainerDetentionSchema.Constants.PD_OH_CTO ||
								property.Name == OrgContainerDetentionSchema.Constants.PD_Direction ||
								property.Name == OrgContainerDetentionSchema.Constants.PD_PenaltyType;
					}
				}
				else
				{
					switch (detention.ParentType)
					{
						case OrgContainerDetentionCollection.ParentType.Consignor:
						case OrgContainerDetentionCollection.ParentType.Consignee:
							return property.Name == OrgContainerDetentionSchema.Constants.PD_OH_Carrier
								|| property.Name == OrgContainerDetentionSchema.Constants.PD_OriginPortOrCountry
								|| property.Name == OrgContainerDetentionSchema.Constants.PD_Direction
								|| property.Name == OrgContainerDetentionSchema.Constants.PD_OH_Client;
						case OrgContainerDetentionCollection.ParentType.Carrier:
							return property.Name == OrgContainerDetentionSchema.Constants.PD_OH_Client ||
								property.Name == OrgContainerDetentionSchema.Constants.PD_OriginPortOrCountry ||
								property.Name == OrgContainerDetentionSchema.Constants.PD_Direction ||
								property.Name == OrgContainerDetentionSchema.Constants.PD_PenaltyType;
					}
				}
			}

			return false;
		}
	}
}
