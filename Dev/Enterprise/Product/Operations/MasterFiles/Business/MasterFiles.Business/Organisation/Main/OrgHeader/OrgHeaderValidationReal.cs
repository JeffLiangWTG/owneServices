using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CountryCompliance.Portugal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderValidationReal : OrgHeaderValidation
	{
		public OrgHeaderValidationReal(OrgHeader parent)
			: base(parent)
		{
		}

		public new OrgHeader Parent
		{
			get { return (OrgHeader)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateAPSettlementGroupPK();
			ValidateARSettlementGroupPK();
			ValidateSubscriptions();
		}

		#region Validate index on Subscriptions table

		void ValidateSubscriptions()
		{
			Parent.Subscriptions.ValidateAllMembers();
		}

		#endregion

		#region Org Type Validation

		protected override void CheckOH_IsWarehouseClient()
		{
			base.CheckOH_IsWarehouseClient();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsWarehouseClientInfo);
		}

		protected override void CheckOH_IsAirLine()
		{
			base.CheckOH_IsAirLine();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsAirLineInfo);
			CheckIsAirLineAndIsShippingLineAreMutuallyExclusive(Parent.OH_IsAirLineInfo);
		}

		protected override void CheckOH_IsBroker()
		{
			base.CheckOH_IsBroker();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsBrokerInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent, Parent.OH_IsBrokerInfo);
		}

		protected override void CheckOH_IsCompetitor()
		{
			base.CheckOH_IsCompetitor();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsCompetitorInfo);
		}

		protected override void CheckOH_IsConsignee()
		{
			base.CheckOH_IsConsignee();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsConsigneeInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent, Parent.OH_IsConsigneeInfo);
		}

		protected override void CheckOH_IsConsignor()
		{
			base.CheckOH_IsConsignor();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsConsignorInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent, Parent.OH_IsConsignorInfo);
		}

		protected override void CheckOH_IsMiscFreightServices()
		{
			base.CheckOH_IsMiscFreightServices();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsMiscFreightServicesInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent, Parent.OH_IsMiscFreightServicesInfo);
		}

		protected override void CheckOH_IsAirCTO()
		{
			base.CheckOH_IsAirCTO();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsAirCTOInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent, Parent.OH_IsAirCTOInfo);
		}

		protected override void CheckOH_IsSeaCTO()
		{
			base.CheckOH_IsSeaCTO();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsSeaCTOInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent, Parent.OH_IsSeaCTOInfo);
		}

		protected override void CheckOH_IsShippingConsortium()
		{
			base.CheckOH_IsShippingConsortium();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsShippingConsortiumInfo);
		}

		protected override void CheckOH_IsContainerLeasingCompany()
		{
			base.CheckOH_IsContainerLeasingCompany();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsContainerLeasingCompanyInfo);
		}

		protected override void CheckOH_IsContainerYard()
		{
			base.CheckOH_IsContainerYard();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsContainerYardInfo);
		}

		protected override void CheckOH_IsForwarder()
		{
			base.CheckOH_IsForwarder();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsForwarderInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent, Parent.OH_IsForwarderInfo);
		}

		protected override void CheckOH_IsFumigationContractor()
		{
			base.CheckOH_IsFumigationContractor();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsFumigationContractorInfo);
		}

		protected override void CheckOH_IsVGMContractor()
		{
			base.CheckOH_IsVGMContractor();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsVGMContractorInfo);
		}

		protected override void CheckOH_IsInlandWaterwayProvider()
		{
			base.CheckOH_IsInlandWaterwayProvider();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsInlandWaterwayProviderInfo);
		}

		protected override void CheckOH_IsLineHaulProvider()
		{
			base.CheckOH_IsLineHaulProvider();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsLineHaulProviderInfo);
		}

		protected override void CheckOH_IsLocalTransport()
		{
			base.CheckOH_IsLocalTransport();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsLocalTransportInfo);
		}

		protected override void CheckOH_IsPackDepot()
		{
			base.CheckOH_IsPackDepot();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsPackDepotInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent, Parent.OH_IsPackDepotInfo);
		}

		protected override void CheckOH_IsUnpackDepot()
		{
			base.CheckOH_IsUnpackDepot();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsUnpackDepotInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent, Parent.OH_IsUnpackDepotInfo);
		}

		protected override void CheckOH_IsRailHead()
		{
			base.CheckOH_IsRailHead();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsRailHeadInfo);
		}

		protected override void CheckOH_IsRailProvider()
		{
			base.CheckOH_IsRailProvider();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsRailProviderInfo);
		}

		protected override void CheckOH_IsRoadFreightDepot()
		{
			base.CheckOH_IsRoadFreightDepot();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsRoadFreightDepotInfo);
		}

		protected override void CheckOH_IsSalesLead()
		{
			base.CheckOH_IsSalesLead();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsSalesLeadInfo);
		}

		protected override void CheckOH_IsShippingLine()
		{
			base.CheckOH_IsShippingLine();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsShippingLineInfo);
			CheckIsAirLineAndIsShippingLineAreMutuallyExclusive(Parent.OH_IsShippingLineInfo);
		}

		protected override void CheckOH_IsSeaWholesaler()
		{
			base.CheckOH_IsSeaWholesaler();
		}

		protected override void CheckOH_IsTransportClient()
		{
			base.CheckOH_IsTransportClient();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsTransportClientInfo);
		}

		protected override void CheckOH_IsShippingProvider()
		{
			base.CheckOH_IsShippingProvider();
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsShippingProviderInfo);
			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfAtleastOneExpected(Parent, Parent.OH_IsShippingProviderInfo);
		}

		protected override void CheckOH_IsTempAccount()
		{
			base.CheckOH_IsTempAccount();

			if (Parent.OH_IsTempAccount)
			{
				ValidateOH_IsConsignee();
				ValidateOH_IsConsignor();
				Parent.CompanyData.Validation.ValidateOB_IsDebtor();
				Parent.CompanyData.Validation.ValidateOB_IsCreditor();

				if (Parent.OH_IsTempAccount && !Parent.OH_IsConsignee && !Parent.OH_IsConsignor && !Parent.OH_IsDebtor && !Parent.OH_IsCreditor && !Parent.OH_IsSalesLead)
				{
					Parent.OH_IsTempAccountInfo.AddError(Res.GetString("ffb96b27-8509-46e7-a1da-2931ce28ed11", "{0} organizations must be of at least one these types: {1}, {2}, {3} {4} or {5}.",
						Parent.OH_IsTempAccountInfo.HumanReadableName,
						Parent.OH_IsConsigneeInfo.HumanReadableName,
						Parent.OH_IsConsignorInfo.HumanReadableName,
						Parent.OH_IsDebtorInfo.HumanReadableName,
						Parent.OH_IsCreditorInfo.HumanReadableName,
						Parent.OH_IsSalesLeadInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckOH_IsControllingAgent()
		{
			base.CheckOH_IsControllingAgent();

			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsControllingAgentInfo);
			CheckIsControllingAgentAndCustomerAreExclusive(Parent.OH_IsControllingAgentInfo);
		}

		protected override void CheckOH_IsControllingCustomer()
		{
			base.CheckOH_IsControllingCustomer();

			OrgHeaderTypeValidation.ValidateOrgTypeMandatoryIfExpected(Parent, Parent.OH_IsControllingCustomerInfo);
			CheckIsControllingAgentAndCustomerAreExclusive(Parent.OH_IsControllingCustomerInfo);

			if (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value
				&& Parent.OH_IsControllingCustomer && !Parent.OH_IsControllingCustomerInfo.ReadOnly
				&& Parent.AllRelatedParties.Find(new ZQuery(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ControllingAgent)).Length == 0
				&& (
							Parent.IsInDatabase && !Parent.SecurityProvider.HasModifyDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent
						|| !Parent.IsInDatabase && !Parent.SecurityProvider.HasNewDetailsOrgTypeFlagCtrlCustomerWithoutCtrlAgent
				)
			)
			{
				Parent.OH_IsControllingCustomerInfo.AddError(Res.GetString("340fffaa-de1c-4942-bf12-56cacb600898", @"You have insufficient security rights to flag an organization as a Controlling Customer without specifying a Controlling Agent first.
Add related party CAG – Controlling Agent first, or remove ""Controlling Customer"" organization type flag."));
			}
		}

		void CheckIsControllingAgentAndCustomerAreExclusive(ZPropertyInfo info)
		{
			if (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value
				&& OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value
				&& Parent.OH_IsControllingAgent
				&& Parent.OH_IsControllingCustomer)
			{
				info.AddError(Res.GetString("89ab8cd9-228d-4326-95a7-83a084472239", @"You cannot mark an Organization as both {0} and {1}. Please only choose one of these options.", Parent.OH_IsControllingAgentInfo.HumanReadableName, Parent.OH_IsControllingCustomerInfo.HumanReadableName));
			}
		}

		void CheckIsAirLineAndIsShippingLineAreMutuallyExclusive(ZPropertyInfo info)
		{
			var errorMessage = Res.GetString("A089A6BB-2DCB-4B37-B799-53E0DFB64377", "An Organization record cannot be both a Shipping Line and an Airline. These should be setup as separate Organizations.");
			if (Parent.OH_IsAirLine && Parent.OH_IsShippingLine)
			{
				info.AddError(errorMessage);
			}

			if (info == Parent.OH_IsAirLineInfo)
			{
				ValidateOH_IsShippingLine();
			}
			else
			{
				ValidateOH_IsAirLine();
			}
		}

		#endregion

		#region Sales Call Filter

		public void ValidateDateOfCallFrom()
		{
			ValidateCalculatedProperty(Parent.DateOfCallFromInfo);
		}

		protected void CheckDateOfCallFrom()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.DateOfCallFromInfo);
		}

		public void ValidateDateOfCallTo()
		{
			ValidateCalculatedProperty(Parent.DateOfCallToInfo);
		}

		protected void CheckDateOfCallTo()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.DateOfCallToInfo);
		}

		public void ValidateDateNextCallTo()
		{
			ValidateCalculatedProperty(Parent.DateNextCallToInfo);
		}

		protected void CheckDateNextCallTo()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.DateNextCallToInfo);
		}

		public void ValidateDateNextCallFrom()
		{
			ValidateCalculatedProperty(Parent.DateNextCallFromInfo);
		}

		protected void CheckDateNextCallFrom()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.DateNextCallFromInfo);
		}

		public void ValidateCallContact()
		{
			ValidateCalculatedProperty(Parent.CallContactInfo);
		}

		protected void CheckCallContact()
		{
			ListValidation.ErrorIfInvalidPK(Parent.CallContactInfo);
		}

		public void ValidateCallSalesRep()
		{
			ValidateCalculatedProperty(Parent.CallSalesRepInfo);
		}

		protected void CheckCallSalesRep()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CallSalesRepInfo);
		}

		public void ValidateCallDirection()
		{
			ValidateCalculatedProperty(Parent.CallDirectionInfo);
		}

		protected void CheckCallDirection()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CallDirectionInfo);
		}

		public void ValidateCallLocation()
		{
			ValidateCalculatedProperty(Parent.CallLocationInfo);
		}

		protected void CheckCallLocation()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CallLocationInfo);
		}

		public void ValidateDateOfCallNoteFrom()
		{
			ValidateCalculatedProperty(Parent.DateOfCallNoteFromInfo);
		}

		protected void CheckDateOfCallNoteFrom()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.DateOfCallNoteFromInfo);
		}

		public void ValidateDateOfCallNoteTo()
		{
			ValidateCalculatedProperty(Parent.DateOfCallNoteToInfo);
		}

		protected void CheckDateOfCallNoteTo()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.DateOfCallNoteToInfo);
		}

		public void ValidateDateFollowUpFrom()
		{
			ValidateCalculatedProperty(Parent.DateFollowUpFromInfo);
		}

		protected void CheckDateFollowUpFrom()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.DateFollowUpFromInfo);
		}

		public void ValidateDateFollowUpTo()
		{
			ValidateCalculatedProperty(Parent.DateFollowUpToInfo);
		}

		protected void CheckDateFollowUpTo()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.DateFollowUpToInfo);
		}

		public void ValidateCallNoteContact()
		{
			ValidateCalculatedProperty(Parent.CallNoteContactInfo);
		}

		protected void CheckCallNoteContact()
		{
			ListValidation.ErrorIfInvalidPK(Parent.CallNoteContactInfo);
		}

		public void ValidateCallingStaff()
		{
			ValidateCalculatedProperty(Parent.CallingStaffInfo);
		}

		protected void CheckCallingStaff()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CallingStaffInfo);
		}

		#endregion

		#region Collection Call Filter

		public void ValidateCollectionCallStatus()
		{
			ValidateCalculatedProperty(Parent.CollectionCallStatusInfo);
		}

		protected void CheckCollectionCallStatus()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CollectionCallStatusInfo);
		}

		public void ValidateCollectionCallDisposition()
		{
			ValidateCalculatedProperty(Parent.CollectionCallDispositionInfo);
		}

		protected void CheckCollectionCallDisposition()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CollectionCallDispositionInfo);
		}

		#endregion

		#region Opportunity Filter

		public void ValidateOpportunityDateFrom()
		{
			ValidateCalculatedProperty(Parent.OpportunityDateFromInfo);
		}

		protected void CheckOpportunityDateFrom()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.OpportunityDateFromInfo);
			if (!Parent.OpportunityDateTo.IsEmpty && Parent.OpportunityDateTo < Parent.OpportunityDateFrom)
			{
				Parent.OpportunityDateFromInfo.AddError(Res.GetString("0704913c-2578-4a74-ae61-3f76cd9f4e06", "The From date must be smaller than the To date"));
			}
			ValidateOpportunityDateTo();
		}

		public void ValidateOpportunityDateTo()
		{
			ValidateCalculatedProperty(Parent.OpportunityDateToInfo);
		}

		protected void CheckOpportunityDateTo()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.OpportunityDateToInfo);
			if (!Parent.OpportunityDateTo.IsEmpty && Parent.OpportunityDateFrom > Parent.OpportunityDateTo)
			{
				Parent.OpportunityDateToInfo.AddError(Res.GetString("64e03458-2e32-4d9e-bb58-c8dceaec15ef", "The From date must be smaller than the To date"));
			}
			ValidateOpportunityDateFrom();
		}

		public void ValidateOpportunitiesDateTypeToFilter()
		{
			ValidateCalculatedProperty(Parent.OpportunitiesDateTypeToFilterInfo);
		}

		protected void CheckOpportunitiesDateTypeToFilter()
		{
			ListValidation.ErrorIfInvalidCode(Parent.OpportunitiesDateTypeToFilterInfo);
		}

		#endregion

		#region OH_Language

		protected override void CheckOH_Language()
		{
			base.CheckOH_Language();
			MandatoryValidation.CheckEntered(Parent.OH_LanguageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OH_LanguageInfo);
		}

		#endregion

		#region OH_Category

		protected override void CheckOH_Category()
		{
			base.CheckOH_Category();
			MandatoryValidation.CheckEntered(Parent.OH_CategoryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OH_CategoryInfo);
		}

		#endregion

		#region OH_Isactive

		protected override void CheckOH_IsActive()
		{
			base.CheckOH_IsActive();

			if (Parent.OH_IsActive)
			{
				StmNote[] notesFound = Parent.Notes.FindByDescription(PredefinedNoteTypes.Instance.InactiveRecordDetails.Description);
				if (notesFound.Length > 0)
				{
					ZString error = Res.GetString("475ffbd2-09b7-4516-aca5-87a177c30901", "There is a note of type '{0}' attached to this organization. Please remove this note to make this record active.", PredefinedNoteTypes.Instance.InactiveRecordDetails.Description);
					Parent.OH_IsActiveInfo.AddError(error);
				}
			}
		}

		#endregion

		#region OH_Code

		protected override void CheckOH_Code()
		{
			base.CheckOH_Code();

			if (Env.Registry.CanUserEditOrganisationCode && Env.Security.OrgDetailsModifyCode.IsAllowed)
			{
				MandatoryValidation.CheckEntered(Parent.OH_CodeInfo);
				EnsureCodeIsNotStartedWithWhiteSpace();
				EnsureCodeIsUnique();
				if (GetTotalLengthOfValidCharsInCode() != Parent.OH_Code.Trim().Length)
				{
					Parent.OH_CodeInfo.AddError(EnglishCharactersValidation.GetNotificationMessage(Parent.OH_CodeInfo));
				}
			}
			if (Parent.RegeneratingCodeAfterPossibleUserEdit)
			{
				Parent.OH_CodeInfo.AddWarning(Res.GetString("706794c8-4f23-432b-9bd1-eb91e9a29071", "Code has been regenerated even though it may have been edited by a user."));
			}
			if (Parent.InvalidCodeGenAlgorithm)
			{
				Parent.OH_CodeInfo.AddWarning(Res.GetString("0f87c398-554e-4985-981a-c7ea836b0058", "Code generation algorithm is not valid. Check the setting in the Registry under Organizations > Codes."));
			}
			if (Parent.OrgCodeGen.SkippedIllegalCharacter)
			{
				string part1 = Res.GetString("63274530-8013-4487-b707-731be09dcc48", "Some characters from Organization Name could not be transliterated and were stripped out for the purpose of Code Generation. Non English characters are not allowed in Code.");
				string part2 = Res.GetString("fc0814d2-dac5-42c7-8383-9a4168691629", "To edit Organization Codes, please enable") + " " + ((IRegistryItemInternals)Env.Registry.RawRegistry.CanUserEditOrganisationCode).Location;
				Parent.OH_CodeInfo.AddWarning(part1 + (!Env.Registry.CanUserEditOrganisationCode ? " " + part2 : ""));
			}
		}

		int GetTotalLengthOfValidCharsInCode()
		{
			int length = Parent.OH_Code.KeepAlphanumericCharacters().Length;
			foreach (char c in Parent.OrgCodeGen.ValidNonLetterCharsInCode)
			{
				length += Parent.OH_Code.KeepChars(c.ToString()).Length;
			}
			return length;
		}

		void EnsureCodeIsUnique()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, Parent.OH_Code);
			filter.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			bool isInDatabase = Parent.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(OrgHeader)), filter);
			if (isInDatabase)
			{
				Parent.OH_CodeInfo.AddError(Res.GetString("2b738c5e-f271-48e7-8140-701a149e8b60", "This organization code has already been used on another organization. Please specify a different code."));
			}
		}

		void EnsureCodeIsNotStartedWithWhiteSpace()
		{
			if (Parent.OH_Code.Length > 0 && Char.IsWhiteSpace(Parent.OH_Code[0]))
			{
				Parent.OH_CodeInfo.AddError(Res.GetString("a529c396-1f5f-4caa-ba43-5309cbefe17b", "Organization codes cannot start with a space character. Please change your organization code."));
			}
		}

		#endregion

		#region OH_FullName

		protected override void CheckOH_FullName()
		{
			base.CheckOH_FullName();
			MandatoryValidation.CheckEntered(Parent.OH_FullNameInfo);

			try
			{
				Parent.OH_FullName.ToString().Normalize(System.Text.NormalizationForm.FormD);
			}
			catch (ArgumentException)
			{
				Parent.OH_FullNameInfo.AddError(Res.GetString("a906ff3b-e442-4aad-b1d0-199c03f2fc6f", "Full Name contains invalid character(s)"));
			}

			if (Parent.OH_FullName.Length > 50 && !Parent.OH_FullNameInfo.HasErrors())
			{
				Parent.OH_FullNameInfo.AddWarning(Res.GetString("b8a16f40-b5e5-4182-bb9d-fab6f7d3e85b", "Some legacy documents cannot display company names over 50 characters. The name will be stored but truncated when displayed on these documents."));
			}
			else if (Parent.IsDuplicateFound)
			{
				Parent.OH_FullNameInfo.AddWarningWithoutValidationCheck(Res.GetString("e752b635-f582-49ff-8d33-6742e5072ecd", "The name you have entered resulted in potential duplicates. Please confirm that they are actual duplicates."));
			}

			if (!Parent.OH_FullNameInfo.HasErrors())
			{
				PortugalValidatorHelper.AddErrorIfOrgFullNameChangeIsNotAllowed(Parent.OH_FullNameInfo, Res.GetString("cd5a8314-ab2e-45d8-b59a-07529b226ad4", "You cannot edit the full name.At least one transaction has been posted in a Portugal Login Company in this database using this Organization."));
			}
		}

		#endregion

		#region OH_RL_NKClosestPort

		public static string OH_RL_NKClosestPortMismatchMessage
		{
			get { return Res.GetString("8078fb33-5c42-4899-b1a4-4106560c46ae", "UNLOCO of Organization should match Related City/Port of Main Address. Please correct one or the other."); }
		}

		protected override void CheckOH_RL_NKClosestPort()
		{
			base.CheckOH_RL_NKClosestPort();
			MandatoryValidation.CheckEntered(Parent.OH_RL_NKClosestPortInfo);

			ListValidation.ErrorIfInvalidCode(Parent.OH_RL_NKClosestPortInfo);
			if (Parent.MainAddress != null)
			{
				Parent.MainAddress.Validation.ValidateOA_RL_NKRelatedPortCode();
				if (!Parent.OH_RL_NKClosestPort.IsEmpty && !Parent.MainAddress.OA_RL_NKRelatedPortCode.IsEmpty && Parent.OH_RL_NKClosestPort != Parent.MainAddress.OA_RL_NKRelatedPortCode)
				{
					Parent.OH_RL_NKClosestPortInfo.AddError(OH_RL_NKClosestPortMismatchMessage);
				}
			}
			Parent.RegenerateCodeIfRequired();
		}

		#endregion

		#region OH_RSL_ShippingLine

		protected override void CheckOH_RSL_ShippingLine()
		{
			base.CheckOH_RSL_ShippingLine();

			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_RSL_ShippingLine, Parent.OH_RSL_ShippingLine);

			var orgHeaders = Parent.Factory.Load<OrgHeader>(filter);
			if (orgHeaders.Length > 1)
			{
				var existingOrgOH_Code = (orgHeaders[0].OH_Code != Parent.OH_Code ? orgHeaders[0].OH_Code : orgHeaders[1].OH_Code);

				Parent.OH_RSL_ShippingLineInfo.AddError(Res.GetString("88706698-D5ED-4938-81AF-C69A2165D5E1",
				"This C1C Code is already entered against Organization {0}. \r\nPlease remove it from that Organization in order to save it against this one", existingOrgOH_Code));
			}
		}

		#endregion

		#region OH_IsNationalAccount

		protected override void CheckOH_IsNationalAccount()
		{
			base.CheckOH_IsNationalAccount();

			if (Parent.OH_IsNationalAccount && Parent.OH_IsGlobalAccount)
			{
				Parent.OH_IsNationalAccountInfo.AddError(Res.GetString("a9a0510f-e3bc-47fb-b7ed-8af8d781ca94", @"You cannot mark an Organization as both a National and a Global account. Please only choose one of these options.

A National account is one that has offices in many locations within the same country, whilst a Global account is one that has offices around the world."));
			}
		}

		#endregion

		#region OH_IsGlobalAccount

		protected override void CheckOH_IsGlobalAccount()
		{
			base.CheckOH_IsGlobalAccount();

			if (Parent.OH_IsGlobalAccount)
			{
				if (Parent.OH_IsNationalAccount)
				{
					Parent.OH_IsGlobalAccountInfo.AddError(Res.GetString("7a304b57-29c5-42bf-aa00-d191d0c1dc42", @"You cannot mark an Organization as both a National and a Global account. Please only choose one of these options.

A National account is one that has offices in many locations within the same country, whilst a Global account is one that has offices around the world."));
				}
				else if (!Parent.OH_IsShippingProvider)
				{
					Parent.OH_IsGlobalAccountInfo.AddError(
Res.GetString("66c9e5ac-1a3f-4aa7-9cf2-8b83e386573c", @"Global organizations are used to define global carrier relationships.
They are not suitable for clients, shippers or other relationships as these must have specific local entities (the corporations in each location) that you deal with.

This validation prevents the use of a global organization on anything but a carrier. If you do bypass this and use a non-carrier as a global organization, the reporting system and other systems will not function correctly and errors will result.

Please revert this organization to being a non-global organization, or if it is a carrier please set the carrier flag on."));
				}
			}
		}

		#endregion

		#region OH_ScreeningStatus

		protected override void CheckOH_ScreeningStatus()
		{
			base.CheckOH_ScreeningStatus();
			MandatoryValidation.CheckEntered(Parent.OH_ScreeningStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OH_ScreeningStatusInfo, Parent.Lookups.ScreeningStatusesList);
		}

		#endregion

		#region Validate APSettlementGroupPK

		public void ValidateAPSettlementGroupPK()
		{
			ValidateCalculatedProperty(Parent.APSettlementGroupPKInfo);
		}

		protected void CheckAPSettlementGroupPK()
		{
			if (Parent.APSettlementGroup != null && !Parent.APSettlementGroup.OH_IsActive)
			{
				Parent.APSettlementGroupPKInfo.AddWarning(Res.GetString("78f59eeb-6252-4c82-99b5-ca6dd5d8aa7a", "The Settlement Group is Inactive."));
			}
		}

		#endregion

		#region Validate ARSettlementGroupPK

		public void ValidateARSettlementGroupPK()
		{
			ValidateCalculatedProperty(Parent.ARSettlementGroupPKInfo);
		}

		protected void CheckARSettlementGroupPK()
		{
			if (Parent.ARSettlementGroup != null && !Parent.ARSettlementGroup.OH_IsActive)
			{
				Parent.ARSettlementGroupPKInfo.AddWarning(Res.GetString("c2164b47-d331-403c-9955-c7742e9606e4", "The Settlement Group is Inactive."));
			}
		}

		#endregion
	}
}
