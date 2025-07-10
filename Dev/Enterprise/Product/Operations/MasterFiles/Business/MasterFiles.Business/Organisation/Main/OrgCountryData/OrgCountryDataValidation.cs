using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataValidation : AutoOrgCountryDataValidation
	{
		public OrgCountryDataValidation(AutoOrgCountryData parent)
			: base(parent)
		{
		}

		new internal OrgCountryData Parent
		{
			get { return (OrgCountryData)base.Parent; }
		}

		public bool IsValidationRequired => Parent.SupplyChainSecurityConfiguration.IsOnlyForDevelopers ||
											Parent.SupplyChainSecurityConfiguration.IsEnabled;

		#region OV_EXApprovedOrMajorExporter

		protected override void CheckOV_EXApprovedOrMajorExporter()
		{
			if (IsValidationRequired && (!Parent.SupplyChainSecurityConfiguration.IsAddressLevelScheme || Parent.AddedThroughCollection))
			{
				if (Parent.SupplyChainSecurityConfiguration.IsEnabled)
				{
					ListValidation.ErrorIfInvalidCode(Parent.OV_EXApprovedOrMajorExporterInfo);
				}

				AddEditApprovalErrorForUncertifiedUser(Parent.OV_EXApprovedOrMajorExporterInfo);

				if (Parent.ApprovedLocation != null)
				{
					var errorForApprovalCodeIsInvalidForCountry = Parent.SupplyChainSecurityConfiguration.GetErrorForApprovalCodeInvalidForCountry(Parent.OV_EXApprovedOrMajorExporter, Parent.ApprovedLocation.OA_RN_NKCountryCode);
					if (!errorForApprovalCodeIsInvalidForCountry.IsEmpty)
					{
						Parent.OV_EXApprovedOrMajorExporterInfo.AddError(errorForApprovalCodeIsInvalidForCountry);
					}
				}

				CheckOrgLevelApproval();

				if (!Parent.OV_EXApprovedOrMajorExporterInfo.HasErrors() && RequiredDocumentValidationApplies)
				{
					string requiredDocType = Parent.ApprovedOrganisationRequiredDocType;
					if (!string.IsNullOrEmpty(requiredDocType) && !TrackingDocumentExists(false))
					{
						var requiredDocError = Parent.SupplyChainSecurityConfiguration.UseApprovedOrganisationRequiredDocTypeSpecifiedInRegistry
							? Res.GetString("7581ac15-63f2-46d2-884d-4c30904a3a5c", "A document of type {0} with Document Tracking must be attached to this organization before flagging it as approved.\r\nThis document type can be configured in the Registry at Freight > Supply Chain Security > Default Required Document type for Approved Organization.", requiredDocType)
							: Res.GetString("4ff8894e-68f1-41ef-a4a6-0537cb250c51", "Before flagging this organization as approved, attach a document to eDocs using type \"{0}\" and record the details for the document within the Document Tracking grid.", requiredDocType);

						Parent.OV_EXApprovedOrMajorExporterInfo.AddError(requiredDocError);
					}
				}
			}
		}

		void CheckOrgLevelApproval()
		{
			if (!Parent.SupplyChainSecurityConfiguration.IsAddressLevelScheme
					|| Parent.OrgHeader == null
					|| !Parent.OrgLevelApprovalApplies)
			{
				return;
			}

			var orgLevelApproval = Parent.OrgHeader.Addresses
				.Cast<OrgAddress>()
				.FirstOrDefault(x => x.KnownShipperDetails.Count == 1
					&& Parent.PK != x.KnownShipper.PK
					&& Parent.SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval(x.KnownShipper.OV_EXApprovedOrMajorExporter))
				?.KnownShipper;

			if (orgLevelApproval != null)
			{
				var codes = new AviationSecuritySchemeMembership();
				var orgLevelApprovalDescription = codes.GetDescriptionFromCode(orgLevelApproval.OV_EXApprovedOrMajorExporter);
				if (Parent.OV_EXApprovedOrMajorExporter == orgLevelApproval.OV_EXApprovedOrMajorExporter)
				{
					Parent.OV_EXApprovedOrMajorExporterInfo.AddError(Res.GetString("c7623c98-6e9c-4671-a211-86312259d4a3", "{0} approval status is given to the whole company so the code per address does not need to be defined. Suggest to use single {1} type with Main Organization address.",
						orgLevelApprovalDescription,
						orgLevelApproval.OV_EXApprovedOrMajorExporter));
				}
				else
				{
					Parent.OV_EXApprovedOrMajorExporterInfo.AddError(Res.GetString("d8332a22-7874-4a45-a303-463b7d8e0242", "{0} approval status is given to the whole company so other approval types are not possible.",
						orgLevelApprovalDescription));
				}
			}
		}

		bool RequiredDocumentValidationApplies
		{
			get { return Parent.SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation(Parent.OV_EXApprovedOrMajorExporter); }
		}

		#endregion

		#region OV_EXApprovalExpiryDate

		protected override void CheckOV_EXApprovalExpiryDate()
		{
			if (!IsValidationRequired || !Parent.AddedThroughCollection)
			{
				return;
			}

			AddEditApprovalErrorForUncertifiedUser(Parent.OV_EXApprovalExpiryDateInfo);

			if (Parent.SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(Parent.OV_EXApprovedOrMajorExporter) || Parent.SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate(Parent.OV_EXApprovedOrMajorExporter))
			{
				if (Parent.SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(Parent.OV_EXApprovedOrMajorExporter))
				{
					MandatoryValidation.CheckEntered(Parent.OV_EXApprovalExpiryDateInfo);
				}

				if (Parent.OV_EXApprovalExpiryDate.IsValid)
				{
					CheckOV_EXApprovalExpiryDateExpiration();
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.OV_EXApprovalExpiryDateInfo, Res.GetString("9efce1bd-6f5a-486e-ac10-59488ccdfa6d", "Supply Chain Security Expiry Date"));
			}

			base.CheckOV_EXApprovalExpiryDate();
		}

		public virtual void CheckOV_EXApprovalExpiryDateExpiration(int maximumValidityValue = 1)
		{
			if (Parent.OV_EXApprovalExpiryDate < ZDate.Today)
			{
				if (!Parent.IsInDatabase || Parent.OV_EXApprovalExpiryDateInfo.HasChanges || Parent.OV_EXApprovedOrMajorExporterInfo.HasChanges)
				{
					Parent.OV_EXApprovalExpiryDateInfo.AddError(Res.GetString("7c6a479c-8534-4261-ad8a-c96ffd587431", "The Expiry Date must be in the future."));
				}
				else
				{
					var overrideWarning = Parent.SupplyChainSecurityConfiguration.ApprovalCodeWarningIfApprovalHasExpired(Parent.OV_EXApprovedOrMajorExporter);
					Parent.OV_EXApprovalExpiryDateInfo.AddWarning(overrideWarning.IsEmpty ? Res.GetString("001940db-9c3b-454c-9951-dc00da95891b", "The Approval has expired.") : overrideWarning.ToString());
				}
			}
			else
			{
				var maximumValidity = Parent.SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(Parent.OV_EXApprovedOrMajorExporter);
				if (maximumValidity > 0 && Parent.OV_EXApprovalExpiryDate > ZDate.Today.AddYears(maximumValidity))
				{
					var overrideError = Parent.SupplyChainSecurityConfiguration.ApprovalCodeErrorIfMaximumApprovalValidityExceeded(Parent.OV_EXApprovedOrMajorExporter);
					if (!overrideError.IsEmpty)
					{
						Parent.OV_EXApprovalExpiryDateInfo.AddError(overrideError);
					}
					else
					{
						Parent.OV_EXApprovalExpiryDateInfo.AddError(maximumValidity == maximumValidityValue
							? Res.GetString("8913da08-ebce-46a5-99dc-293bca75f5f1", "The Expiry Date cannot be more than {0} year in the future.", maximumValidityValue)
							: Res.GetString("0c3795a5-4bd0-470f-a9f6-6ffae38d62f3", "The Expiry Date cannot be more than {0} years in the future.", maximumValidity));
					}
				}
				else if (Parent.SupplyChainSecurityConfiguration.ApprovalCodeExpiryDateMustMatchRequiredDocumentExpiry(Parent.OV_EXApprovedOrMajorExporter) && TrackingDocumentExists())
				{
					ZString requiredDocType = Parent.ApprovedOrganisationRequiredDocType;
					if (!Parent.OrgHeader.RequiredDocuments.Cast<JobRequiredDocument>().Any(x => x.EQ_DocType == requiredDocType && x.EQ_DocNumber == Parent.OV_EXApprovalNumber
								&& x.IsPeriodic && x.EQ_ValidToDate.Date == Parent.OV_EXApprovalExpiryDate))
					{
						Parent.OV_EXApprovalExpiryDateInfo.AddError(Res.GetString("84fe6697-007c-40de-acce-246a4a6f9492", "The Expiry Date must match the Valid To Date for the required document type KCA before marking the organization as approved.", requiredDocType));
					}
				}

				var warnIfApprovalWillLapseInMonths = Parent.SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths(Parent.OV_EXApprovedOrMajorExporter);
				if (warnIfApprovalWillLapseInMonths > 0
					&& Parent.OV_EXApprovalExpiryDate < ZDate.Today.AddMonths(warnIfApprovalWillLapseInMonths)
					&& Parent.OrgHeader != null
					&& Parent.OrgHeader.IsProxyOrgOfAnyCompany())
				{
					var overrideWarning = Parent.SupplyChainSecurityConfiguration.ApprovalCodeWarningIfApprovalWillLapseInMonths(Parent.OV_EXApprovedOrMajorExporter);
					Parent.OV_EXApprovalExpiryDateInfo.AddWarning(overrideWarning.IsEmpty ? Res.GetString("575fa65f-10f7-499a-a282-4c6ba39b48d9", "Expiry date is within {0} months.", warnIfApprovalWillLapseInMonths) : overrideWarning.ToString());
				}
			}
		}

		protected override void CheckOV_EXApprovalExpiryDateIsValidZDateRange()
		{
			if (Parent.SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(Parent.OV_EXApprovedOrMajorExporter) > 0)
			{
				return;
			}

			TypeValidation.CheckValidZDateTimeRange(Parent.OV_EXApprovalExpiryDateInfo,
				new TypeValidationLimits { FutureYearsBeforeWarning = 5, FutureYearsBeforeError = 10 });
		}

		#endregion

		#region OV_OA_ApprovedLocation

		protected override void CheckOV_OA_ApprovedLocation()
		{
			if (!IsValidationRequired)
			{
				return;
			}

			if (Parent.AddedThroughCollection && IsDuplicate())
			{
				Parent.OV_OA_ApprovedLocationInfo.AddError(Res.GetString("e6b6961c-1a74-4e6c-8248-171a62328b3a", "{0} cannot be duplicated.", Parent.OV_OA_ApprovedLocationInfo.HumanReadableName));
			}

			base.CheckOV_OA_ApprovedLocation();

			if (Parent.SupplyChainSecurityConfiguration.IsAddressLevelScheme && Parent.AddedThroughCollection)
			{
				MandatoryValidation.CheckEntered(Parent.OV_OA_ApprovedLocationInfo);
				AddEditApprovalErrorForUncertifiedUser(Parent.OV_OA_ApprovedLocationInfo);
			}
		}

		#endregion

		#region OV_EXApprovalNumber

		protected override void CheckOV_EXApprovalNumber()
		{
			if (!IsValidationRequired || !Parent.AddedThroughCollection)
			{
				return;
			}

			base.CheckOV_EXApprovalNumber();

			AddEditApprovalErrorForUncertifiedUser(Parent.OV_EXApprovalNumberInfo);

			if (!Parent.OV_EXApprovalNumberInfo.HasErrors())
			{
				CheckApprovalNumberMandatoryValidation();
			}

			if (!Parent.OV_EXApprovalNumberInfo.HasErrors())
			{
				CheckApprovalNumberFormat();
			}

			if (!Parent.OV_EXApprovalNumberInfo.HasErrors())
			{
				CheckDuplicateApprovalNumber();
			}

			if (!Parent.OV_EXApprovalNumberInfo.HasErrors())
			{
				CheckApprovalNumberMatchesRequiredDocumentNumber();
			}

			if (!Parent.OV_EXApprovalNumberInfo.HasErrors())
			{
				AddWarningForAdditionalApprovalCodeValidation();
			}
		}

		public virtual void CheckApprovalNumberMandatoryValidation()
		{
			if (Parent.SupplyChainSecurityConfiguration.ApprovalCodeRequiresApprovalNumber(Parent.OV_EXApprovedOrMajorExporter) && Parent.OV_EXApprovalNumber.IsEmpty)
			{
				var overriddenErrorMessage = Parent.SupplyChainSecurityConfiguration.ApprovalCodeErrorForOwnAgentApprovalNumberNotEntered(Parent.OV_EXApprovedOrMajorExporter);
				if (!overriddenErrorMessage.IsEmpty
					&& Parent.OrgHeader != null
					&& Parent.OrgHeader.IsProxyOrgOfAnyCompany())
				{
					Parent.OV_EXApprovalNumberInfo.AddError(overriddenErrorMessage);
				}
				else
				{
					MandatoryValidation.CheckEntered(Parent.OV_EXApprovalNumberInfo);
				}
			}
			else if (!Parent.SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber(Parent.OV_EXApprovedOrMajorExporter))
			{
				MandatoryValidation.CheckNotEntered(Parent.OV_EXApprovalNumberInfo);
			}
		}

		void CheckApprovalNumberFormat()
		{
			if (!Parent.OV_EXApprovalNumber.IsEmpty
				&& !Parent.SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberFormat(Parent.OV_EXApprovedOrMajorExporter).IsEmpty)
			{
				var pattern = Parent.SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberFormat(Parent.OV_EXApprovedOrMajorExporter);
				var regex = new Regex(pattern);

				if (!regex.IsMatch(Parent.OV_EXApprovalNumber))
				{
					Parent.OV_EXApprovalNumberInfo.AddError(Parent.SupplyChainSecurityConfiguration.ApprovalCodeErrorForApprovalNumberFormatNotMet(Parent.OV_EXApprovedOrMajorExporter));
				}
			}
		}

		void CheckDuplicateApprovalNumber()
		{
			if (Parent.OrgHeader != null
				&& Parent.OV_EXApprovalNumber != string.Empty
				&& Parent.SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber(Parent.OV_EXApprovedOrMajorExporter))
			{
				var filter = new ZQuery(OrgCountryDataSchema.OV_OH_OrgHeader, Parent.OrgHeader.PK);
				filter.AddToFilter(OrgCountryDataSchema.OV_EXApprovalNumber, Parent.OV_EXApprovalNumber);
				filter.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, Parent.OV_RN_NKClientCountryRelation);
				filter.AddToFilter(OrgCountryDataSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.SupplyChainSecurityConfiguration.ApprovalCodeAllowsDuplicateApprovalNumber(Parent.OV_EXApprovedOrMajorExporter))
				{
					filter.AddToFilter(OrgCountryDataSchema.OV_EXApprovedOrMajorExporter, SQLComparisonOperator.NotEqual, Parent.OV_EXApprovedOrMajorExporter);
				}

				if (Parent.Factory.Exists(typeof(OrgCountryData), filter))
				{
					Parent.OV_EXApprovalNumberInfo.AddError(Res.GetString("b9465183-d75d-4290-9a46-9eda2ca2b471", "Approval Number already exists."));
				}
			}
		}

		void CheckApprovalNumberMatchesRequiredDocumentNumber()
		{
			if (!Parent.OV_EXApprovalNumber.IsEmpty
				&& Parent.SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberMustMatchRequiredDocumentNumber(Parent.OV_EXApprovedOrMajorExporter))
			{
				var requiredDocType = Parent.ApprovedOrganisationRequiredDocType;
				if (!requiredDocType.IsEmpty && !TrackingDocumentExists())
				{
					Parent.OV_EXApprovalNumberInfo.AddError(Res.GetString("c1c41102-7658-4b96-8b7d-ef906c0391d4", "The Approval Number must match the Document Number for the required document type {0} before marking the organization as approved.", requiredDocType));
				}
			}
		}

		void AddWarningForAdditionalApprovalCodeValidation()
		{
			var warning = Parent.SupplyChainSecurityConfiguration.GetWarningForAdditionalApprovalCodeValidation(Parent);
			if (!warning.IsEmpty)
			{
				Parent.OV_EXApprovalNumberInfo.AddWarning(warning);
			}
		}

		#endregion

		#region OV_RN_NKIssuingAuthorityCountry

		protected override void CheckOV_RN_NKIssuingAuthorityCountry()
		{
			AddEditApprovalErrorForUncertifiedUser(Parent.OV_RN_NKIssuingAuthorityCountryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OV_RN_NKIssuingAuthorityCountryInfo);

			if (!Parent.OV_RN_NKClientCountryRelation.IsEmpty && Parent.IssuingAuthorityCountry != null)
			{
				if (Parent.OV_RN_NKClientCountryRelation.EqualsIgnoringCase(Core.Constants.CountryCodes.EuropeanUnion) &&
					!Core.Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(Parent.OV_RN_NKIssuingAuthorityCountry))
				{
					Parent.OV_RN_NKIssuingAuthorityCountryInfo.AddError(Res.GetString("ba2e5099-6073-4880-914c-4481389e635a", "Please enter an EU country code."));
				}
			}

			base.CheckOV_RN_NKIssuingAuthorityCountry();
		}

		#endregion

		#region OV_EXExportPermissionDetails

		protected override void CheckOV_EXExportPermissionDetails()
		{
			AddEditApprovalErrorForUncertifiedUser(Parent.OV_EXExportPermissionDetailsInfo);

			base.CheckOV_EXExportPermissionDetails();
		}

		#endregion

		#region Implementation

		protected bool IsDuplicate()
		{
			if (Parent.SupplyChainSecurityConfiguration.IsAddressLevelScheme)
			{
				ZQuery filter = new ZQuery(OrgCountryDataSchema.OV_OA_ApprovedLocation, Parent.OV_OA_ApprovedLocation);
				filter.AddToFilter(JoinCondition.And, OrgCountryDataSchema.OV_RN_NKClientCountryRelation, Parent.OV_RN_NKClientCountryRelation);
				filter.AddToFilter(JoinCondition.And, OrgCountryDataSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				return Parent.Factory.LoadTop1<OrgCountryData>(filter) != null;
			}

			return false;
		}

		protected bool TrackingDocumentExists(bool checkApprovalNumber = true)
		{
			ZString requiredDocType = Parent.ApprovedOrganisationRequiredDocType;
			return !requiredDocType.IsEmpty
				&& Parent.OrgHeader != null
				&& Parent.OrgHeader.RequiredDocuments.Cast<JobRequiredDocument>().Any(x => x.EQ_DocType == requiredDocType && (!checkApprovalNumber || x.EQ_DocNumber == Parent.OV_EXApprovalNumber));
		}

		public void AddEditApprovalErrorForUncertifiedUser(ZPropertyInfo propertyInfo)
		{
			if (Parent.IsInDatabase && !propertyInfo.HasChanges)
			{
				return;
			}

			var error = Parent.SupplyChainSecurityConfiguration.GetEditApprovalErrorForUncertifiedUser(Parent);
			if (!error.IsEmpty)
			{
				propertyInfo.AddError(error);
			}
		}

		#endregion
	}
}
