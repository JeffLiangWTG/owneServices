using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPatternMatchOverrideValidation : AutoOrgPatternMatchOverrideValidation
	{
		public OrgPatternMatchOverrideValidation(AutoOrgPatternMatchOverride parent) : base(parent)
		{
		}

		#region CheckOO_Relationship

		protected override void CheckOO_Relationship()
		{
			base.CheckOO_Relationship();
			ValidateOO_ForeignCode();
			ValidateOO_LocalGuid();

			ListValidation.ErrorIfInvalidCode(Parent.OO_RelationshipInfo, Lookups.OO_Relationship_List);
		}

		#endregion

		protected override void CheckOO_ForeignCode()
		{
			base.CheckOO_ForeignCode();
			MandatoryValidation.CheckEntered(Parent.OO_ForeignCodeInfo);

			if (Parent.OO_Context == Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage
				&& Parent.OO_Relationship == Constants.OrgPatternMatchOverrideRelationships.Port)
			{
				if (!Parent.OO_ForeignCode.IsEmpty && !IsCodeUniqueForOceanCarrierMessage(Parent, false))
				{
					var duplicateForeignCodeForRelationship = Res.GetString("f0eb769b-26b7-4848-833e-1b40674256b0", "A mapping already exists for foreign port code {0} with the context OCM.", Parent.OO_ForeignCode);
					Parent.OO_ForeignCodeInfo.AddError(duplicateForeignCodeForRelationship);
				}
			}
			else if (!CodeIsUniqueForRelationshipOnThisOrg)
			{
				var duplicateForeignCodeForRelationship = Res.GetString("099688dd-51cf-4322-a56d-095bb28eca4d", "A code mapping already exists between the code {0} and the relationship {1}.", Parent.OO_ForeignCode, Parent.OO_Relationship);
				Parent.OO_ForeignCodeInfo.AddError(duplicateForeignCodeForRelationship);
			}

			if (!Parent.OO_ForeignCodeInfo.HasErrors()
				&& Parent.IsComPayMapping)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OO_ForeignCodeInfo, Lookups.OO_ForeignCode_List);
			}
		}

		#region Check OO_LocalCode

		protected override void CheckOO_LocalCode()
		{
			base.CheckOO_LocalCode();

			if (!Parent.IsNotCode)
			{
				ExecuteCheck(MandatoryValidation.CheckEntered);
				ExecuteCheck(CheckValidInList);
			}
		}

		void ExecuteCheck(Action<ZPropertyInfo> checkAction)
		{
			if (!Parent.OO_LocalCodeInfo.HasErrors())
			{
				checkAction(Parent.OO_LocalCodeInfo);
			}
		}

		void CheckValidInList(ZPropertyInfo propertyInfo)
		{
			bool isCodeValid = true;

			switch (Parent.OO_Relationship)
			{
				case Constants.OrgPatternMatchOverrideRelationships.IncoTerm:
					isCodeValid = Lookups.OO_IncoTerm_List.ContainsCode(Parent.OO_LocalCode);
					break;
				case Constants.OrgPatternMatchOverrideRelationships.PackageType:
					isCodeValid = Lookups.PackageTypes.ContainsCode(Parent.OO_LocalCode);
					break;
				case Constants.OrgPatternMatchOverrideRelationships.EventCode:
					isCodeValid = Lookups.EventTypes.ContainsCode(Parent.OO_LocalCode);
					break;
				case Constants.OrgPatternMatchOverrideRelationships.ChargeCodes:
					isCodeValid = ((IFindBoxListProvider)Lookups.ChargeCodes).GetBusinessObjectFromCodeWithoutFilter(Parent.OO_LocalCode) != null;
					if (isCodeValid && !Lookups.ChargeCodes.ContainsCode(Parent.OO_LocalCode))
					{
						string properyDescriptionString = propertyInfo.HasHumanReadableName ? propertyInfo.HumanReadableName.ToString() : Res.GetString("e5091464-d5b3-4031-9f0b-ad2d0b4839b0", "value");
						Parent.OO_LocalCodeInfo.AddWarning(Res.GetString("11acc5ab-a106-4da8-982c-d858207ac6d5", "This {0} does not belong to current company.", properyDescriptionString));
					}
					break;
				case Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel:
					isCodeValid = ((IFindBoxListProvider)Lookups.CarrierServiceLevels).GetBusinessObjectFromCodeWithoutFilter(Parent.OO_LocalCode) != null;
					break;
			}

			if (!isCodeValid)
			{
				string properyDescriptionString = propertyInfo.HasHumanReadableName ? propertyInfo.HumanReadableName.ToString() : Res.GetString("e5091464-d5b3-4031-9f0b-ad2d0b4839b0", "value");
				string notification = Res.GetString("33c422b5-7252-4d2b-8fe1-10e0d33b3cdc", "Enter a valid {0}.", properyDescriptionString);
				Parent.OO_LocalCodeInfo.AddError(notification);
			}
		}

		#endregion

		protected override void CheckOO_LocalGuid()
		{
			base.CheckOO_LocalGuid();
			ValidateOO_ForeignCode();

			if (!Parent.OO_LocalGuidInfo.HasErrors() && !Parent.IsNotGuid)
			{
				MandatoryValidation.CheckEntered(Parent.OO_LocalGuidInfo);
			}

			if (!Parent.OO_LocalCodeInfo.HasErrors()
				&& Parent.OO_Context == Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage
				&& Parent.OO_Relationship == Constants.OrgPatternMatchOverrideRelationships.Port)
			{
				var orgCoBusinessObject = Parent.OrgCoBusinessObject;
				if (orgCoBusinessObject != null && !IsCodeUniqueForOceanCarrierMessage(Parent, true))
				{
					var duplicateForeignCodeForRelationship = Res.GetString("e9da3498-8165-4612-b2f3-89500bb604f5", "A mapping already exists for local port code {0} with the context OCM.", ((IRefUNLOCO)orgCoBusinessObject).RL_Code);
					Parent.OO_LocalGuidInfo.AddError(duplicateForeignCodeForRelationship);
				}
			}
		}

		bool CodeIsUniqueForRelationshipOnThisOrg
		{
			get
			{
				if (Parent.OO_OH.IsValid)
				{
					return IsCodeUniqueForRelationship(Parent);
				}
				else
				{
					return true;
				}
			}
		}

		static bool IsCodeUniqueForRelationship(OrgPatternMatchOverride patternMatch)
		{
			var query1 = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, patternMatch.OO_OH)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, patternMatch.OO_ForeignCode)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, patternMatch.OO_Relationship);
			var items = patternMatch.Factory.Load<OrgPatternMatchOverride>(query1);
			var isDuplicate = items.Where(y => y.PK != patternMatch.PK)
					.Any(x => x.OO_Context == patternMatch.OO_Context || x.OrgCoNameorGuid != patternMatch.OrgCoNameorGuid);
			return !isDuplicate;
		}

		static bool IsCodeUniqueForOceanCarrierMessage(OrgPatternMatchOverride patternMatch, bool isCheckingLocalCode)
		{
			if (!patternMatch.OO_OH.IsValid)
			{
				return true;
			}

			var query = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, patternMatch.OO_OH)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, patternMatch.OO_Relationship)
			.AddToFilter(OrgPatternMatchOverrideSchema.PK, SQLComparisonOperator.NotEqual, patternMatch.PK)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_Context, Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage);

			var codeQuery = new ZQuery();
			if (isCheckingLocalCode)
			{
				codeQuery.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, patternMatch.OO_LocalGuid);
			}
			else
			{
				codeQuery.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, patternMatch.OO_ForeignCode);
			}

			query.AddToFilter(codeQuery);

			return !patternMatch.Factory.Load<OrgPatternMatchOverride>(query).Any();
		}

		protected override void CheckOO_Context()
		{
			base.CheckOO_Context();
			ValidateOO_ForeignCode();

			if (!Parent.OO_ContextInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.OO_ContextInfo, Parent.Lookups.OO_Context_List);
			}
		}

		OrgPatternMatchOverrideLookups Lookups
		{
			get
			{
				return lookups ?? (lookups = new OrgPatternMatchOverrideLookups(Parent));
			}
		}

		OrgPatternMatchOverrideLookups lookups;

		protected new OrgPatternMatchOverride Parent
		{
			get { return base.Parent as OrgPatternMatchOverride; }
		}
	}
}
