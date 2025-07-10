//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobRequiredDocAttribValidation
//
//    This class should be used for overriding validation in AutoJobRequiredDocAttribValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocAttribValidation : AutoJobRequiredDocAttribValidation
	{
		public JobRequiredDocAttribValidation(AutoJobRequiredDocAttrib parent)
			: base(parent)
		{
		}

		protected new JobRequiredDocAttrib Parent => (JobRequiredDocAttrib)base.Parent;

		public void ValidateD0_AttribDisplayValue()
		{
			ValidateCalculatedProperty(Parent.D0_AttribDisplayValueInfo);
		}

		void CheckProtocolloFormat(ZString protocolText, ZPropertyInfo propertyInfo)
		{
			if (Parent.IsGovernmentAuthorisationReference)
			{
				Regex rgx = new Regex(@"^\d{17}-\d{6}$");
				if (!rgx.IsMatch(protocolText))
				{
					propertyInfo.AddError(InvalidProtocolloFormat);
				}
			}
		}

		void CheckDocumentReceivedDate(ZString dateAsText, ZPropertyInfo propertyInfo)
		{
			if (Parent.IsDocumentReceivedDate)
			{
				ZDateTime result;
				if (!ZDateTime.TryParseISO8601Date(dateAsText, out result))
				{
					propertyInfo.AddError(InvalidDocumentReceivedDate);
				}
			}
		}

		void CheckBuyerIssueDate(ZString dateAsText, ZPropertyInfo propertyInfo)
		{
			if (Parent.IsBuyerIssueDate)
			{
				ZDateTime result;
				if (!ZDateTime.TryParseISO8601Date(dateAsText, out result))
				{
					propertyInfo.AddError(InvalidBuyerIssueDate);
				}
			}
		}

		void CheckCeilingLimit(ZString ceilingLimitAsText, ZPropertyInfo propertyInfo)
		{
			if (Parent.IsCeilingLimit)
			{
				var fromDisplay = propertyInfo == Parent.D0_AttribDisplayValueInfo;
				ZDecimal result = 0;
				if (!ceilingLimitAsText.IsEmpty && !JobRequiredDocAttrib.TryParseCeilingLimit(ceilingLimitAsText, out result, fromDisplay))
				{
					propertyInfo.AddError(InvalidCeilingLimit);
				}
				else if (result <= 0)
				{
					propertyInfo.AddError(CeilingLimitMustBePositive);
				}
			}
		}

		void CheckTaiwanAttorneyDocumentForBroker(ZString attribValue, ZPropertyInfo propertyInfo)
		{
			var requiredDocument = Parent.RequiredDocument;
			if (requiredDocument != null && requiredDocument.IsTaiwanAttorneyDocumentForBroker)
			{
				if (Parent.IsCustomsDistrict)
				{
					if (attribValue.IsEmpty)
					{
						propertyInfo.AddError(GetErrorForTaiwanAttorneyAttributeValueMissing(JobRequiredDocAttribTypeList.Codes.CustomsDistrict));
					}
					else
					{
						ListValidation.ErrorIfInvalidCode(propertyInfo);
					}
				}
				else if (Parent.IsBoxNumber)
				{
					if (attribValue.IsEmpty)
					{
						propertyInfo.AddError(GetErrorForTaiwanAttorneyAttributeValueMissing(JobRequiredDocAttribTypeList.Codes.BoxNumber));
					}
					else
					{
						if (!Regex.IsMatch(attribValue, @"^[A-Z0-9]{3}$"))
						{
							propertyInfo.AddError(InvalidBoxNumber);
						}
						ListValidation.MessageErrorIfInvalidCode(propertyInfo, Parent.Lookups.AttributeValueList);
					}
				}
				CheckDuplicateAttribValueForTaiwanPOADocument(requiredDocument, propertyInfo);
			}
		}

		void CheckDuplicateAttribValueForTaiwanPOADocument(JobRequiredDocument currentRequiredDocument, ZPropertyInfo propertyInfo)
		{
			var currentJobRequiredDocAttrib = Parent;
			var currentAttribValue = currentJobRequiredDocAttrib.D0_AttribValue;
			var currentCustomsDistrict = ZString.Empty;
			var currentBoxNumber = ZString.Empty;
			var currentBondedID = ZString.Empty;
			if ((currentJobRequiredDocAttrib.IsBondedID || currentJobRequiredDocAttrib.IsBoxNumber || currentJobRequiredDocAttrib.IsCustomsDistrict)
				&& !currentAttribValue.IsEmpty)
			{
				currentCustomsDistrict = currentJobRequiredDocAttrib.IsCustomsDistrict ? currentAttribValue : currentRequiredDocument.CustomsDistrictDocAttrib?.D0_AttribValue ?? ZString.Empty;
				currentBoxNumber = currentJobRequiredDocAttrib.IsBoxNumber ? currentAttribValue : currentRequiredDocument.BoxNumberDocAttrib?.D0_AttribValue ?? ZString.Empty;
				currentBondedID = currentJobRequiredDocAttrib.IsBondedID ? currentAttribValue : currentRequiredDocument.Attributes?.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID)?.D0_AttribValue ?? ZString.Empty;
			}

			if (!currentCustomsDistrict.IsEmpty && !currentBoxNumber.IsEmpty && !currentBondedID.IsEmpty)
			{
				if (currentRequiredDocument.Parent?.RequiredDocuments.Cast<JobRequiredDocument>().Where(x => x.EQ_DocType == currentRequiredDocument.EQ_DocType).Any(
					doc => doc.PK != currentRequiredDocument.PK
					&& (doc.CustomsDistrictDocAttrib?.D0_AttribValue ?? ZString.Empty).Equals(currentCustomsDistrict)
					&& (doc.BoxNumberDocAttrib?.D0_AttribValue ?? ZString.Empty).Equals(currentBoxNumber)
					&& (doc.Attributes?.FirstOrDefault(x => x.D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID)?.D0_AttribValue ?? ZString.Empty).Equals(currentBondedID)) ?? false)
				{
					propertyInfo.AddError(InvalidPowerOfAttorneyNumber);
				}
			}
		}

		void CheckCountryRegionMatches(ZString attribValue, ZPropertyInfo attribValueInfo)
		{
			if (Parent.IsCompanyCode && Parent.RequiredDocument is JobRequiredDocument reqDoc)
			{
				if (!reqDoc.EQ_RN_NKRelatedCountry.IsEmpty && !attribValue.IsEmpty)
				{
					GlbCompany company;

					if (ZGuid.IsGuid(attribValue))
					{
						company = Parent.Factory.Load<GlbCompany>(new ZGuid(attribValue));
					}
					else
					{
						company = Parent.Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, attribValue);
					}

					if (company != null && company.GC_RN_NKCountryCode != reqDoc.EQ_RN_NKRelatedCountry)
					{
						attribValueInfo.AddError(CountryCodeOfCompanyDifferFromCountryRegionField);
					}
				}
			}
		}

		void CheckTradePreferenceCodeRegionMatch(ZString attribValue, ZPropertyInfo attribValueInfo)
		{
			if (Parent.IsTradePreferenceCode && Parent.RequiredDocument is JobRequiredDocument reqDoc)
			{
				if (!reqDoc.EQ_RN_NKRelatedCountry.IsEmpty && !attribValue.IsEmpty)
				{
					if (attribValue != "S" && reqDoc.EQ_RN_NKRelatedCountry == Core.Constants.CountryCodes.UnitedStates)
					{
						attribValueInfo.AddError(InvalidTradePreferenceCodeInUnitedStates);
					}
					if (reqDoc.EQ_RN_NKRelatedCountry == Core.Constants.CountryCodes.Canada)
					{
						ListValidation.ErrorIfInvalidCode(ListValidation.InvalidCodeMessageError, attribValueInfo);
					}
				}
			}
		}

		void CheckAttribValueCore(ZString attribValue, ZPropertyInfo attribValueInfo)
		{
			CheckProtocolloFormat(attribValue, attribValueInfo);
			CheckDocumentReceivedDate(attribValue, attribValueInfo);
			CheckBuyerIssueDate(attribValue, attribValueInfo);
			CheckCeilingLimit(attribValue, attribValueInfo);
			CheckTaiwanAttorneyDocumentForBroker(attribValue, attribValueInfo);
			CheckCountryRegionMatches(attribValue, attribValueInfo);
			CheckTradePreferenceCodeRegionMatch(attribValue, attribValueInfo);
		}

		protected override void CheckD0_AttribValue()
		{
			CheckAttribValueCore(Parent.D0_AttribValue, Parent.D0_AttribValueInfo);

			if (Parent.IsCostaRicaEXVDocumentType || Parent.IsIssuingAuthorityName)
			{
				var requiredDocument = Parent.RequiredDocument;
				if (requiredDocument != null && requiredDocument.IsCostaRicaExporterExemptionDocumentForDebtor)
				{
					if (Parent.D0_AttribValue.IsEmpty)
					{
						Parent.D0_AttribValueInfo.AddError(GetMessageForCostaRicaEXVAttributeValueMissing(Parent.IsCostaRicaEXVDocumentType
							? JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType : JobRequiredDocAttribTypeList.Codes.IssuingAuthorityName));
					}
				}
			}
		}

		protected virtual void CheckD0_AttribDisplayValue()
		{
			CheckAttribValueCore(Parent.D0_AttribDisplayValue, Parent.D0_AttribDisplayValueInfo);
		}

		protected override void CheckD0_AttribName()
		{
			base.CheckD0_AttribName();
			var parent = Parent;
			MandatoryValidation.CheckEntered(parent.D0_AttribNameInfo);

			if (!parent.IsCustomAttribute)
			{
				ListValidation.ErrorIfInvalidCode(parent.D0_AttribNameInfo);
			}

			var reqDoc = parent.RequiredDocument;
			if (parent.IsDirection || parent.IsPortOfEntry)
			{
				if (reqDoc != null)
				{
					if (!IsPowerOfAttorney(reqDoc.EQ_DocType))
					{
						parent.D0_AttribNameInfo.AddWarning(AttributeIsOnlyApplicableToPowerOfAttorney(parent.D0_AttribName));
					}
					else if (parent.IsPortOfEntry && reqDoc.Attributes[JobRequiredDocAttribTypeList.Codes.Direction, ImportExportCodeList.Codes.Export] != null)
					{
						parent.D0_AttribNameInfo.AddWarning(PortOfEntryNotApplicableForExport);
					}
				}
			}

			if (parent.IsSellerControlNumber || parent.IsDocumentReceivedDate || parent.IsBuyerIssueDate || parent.IsGovernmentAuthorisationReference || parent.IsCeilingLimit)
			{
				if (!parent.IsInDatabase && !GlbCompany.CurrentCompany.Country.SupportDeclarationOfIntent)
				{
					parent.D0_AttribNameInfo.AddError(CurrentLoginCompanyDoesNotSupportDeclarationOfIntent(parent.D0_AttribName));
				}

				if (reqDoc != null)
				{
					if (!reqDoc.RelatedCountrySupportDeclarationOfIntent)
					{
						parent.D0_AttribNameInfo.AddError(DocumentRelatedCountryDoesNotSupportDeclarationOfIntent(parent.D0_AttribName));
					}
					if (reqDoc.EQ_DocType != Core.Constants.RefDocTypes.VATExporterExemption)
					{
						parent.D0_AttribNameInfo.AddError(AttributeIsOnlyApplicableToVATExporterExemption(parent.D0_AttribName));
					}
					if (parent.IsSellerControlNumber && reqDoc.EQ_DocUsage != JobRequiredDocument.DocUsage.Debtor)
					{
						parent.D0_AttribNameInfo.AddError(SellerControlNumberIsOnlyApplicableToEXVDebtor);
					}
				}
			}

			if (parent.IsCompanyCode && reqDoc != null)
			{
				var documentTypesSupportCompanyCode = new ZString[]
				{
					Core.Constants.RefDocTypes.VATExporterExemption,
					Core.Constants.RefDocTypes.PowerOfAttorney,
					Core.Constants.RefDocTypes.PowerOfAttorneyCustoms,
					Core.Constants.RefDocTypes.PowerOfAttorneyForwarding
				};

				if (!documentTypesSupportCompanyCode.Contains(reqDoc.EQ_DocType))
				{
					parent.D0_AttribNameInfo.AddError(CompanyCodeAttributeDoesNotSupportDocumentType(documentTypesSupportCompanyCode));
				}
				if (reqDoc.EQ_DocType == Core.Constants.RefDocTypes.VATExporterExemption)
				{
					if (!parent.IsInDatabase && !GlbCompany.CurrentCompany.Country.SupportDeclarationOfIntent)
					{
						parent.D0_AttribNameInfo.AddError(CurrentLoginCompanyDoesNotSupportDeclarationOfIntent(parent.D0_AttribName));
					}
					if (!reqDoc.RelatedCountrySupportDeclarationOfIntent)
					{
						parent.D0_AttribNameInfo.AddError(DocumentRelatedCountryDoesNotSupportDeclarationOfIntent(parent.D0_AttribName));
					}
				}
			}

			if (parent.IsTradePreferenceCode && reqDoc != null)
			{
				var documentTypesSupportTradePreferenceCode = new ZString[]
				{
					Core.Constants.RefDocTypes.CertificateOfOrigin
				};
				var relatedCountrySupportTradePreferenceCode = new ZString[]
				{
					Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.CountryCodes.Canada
				};

				if (!documentTypesSupportTradePreferenceCode.Contains(reqDoc.EQ_DocType) || !relatedCountrySupportTradePreferenceCode.Contains(reqDoc.EQ_RN_NKRelatedCountry))
				{
					parent.D0_AttribNameInfo.AddError(TradePreferenceCodeAttributeDoesNotSupportDocumentType(documentTypesSupportTradePreferenceCode, relatedCountrySupportTradePreferenceCode));
				}
			}

			if (reqDoc != null)
			{
				reqDoc.Validation.AddRowErrorForCostaRicaIfApplicable();

				if (!reqDoc.IsCostaRicaExporterExemptionDocumentForDebtor && parent.IsCostaRicaEXVDocumentType)
				{
					parent.D0_AttribNameInfo.AddWarning(GetWarningForCostaRicaEXVAttributeUsedByNonApplicableSettings(JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType));
				}
				else if (!reqDoc.IsTaiwanAttorneyDocumentForBroker)
				{
					var isCustomsDistrict = parent.IsCustomsDistrict;
					if (isCustomsDistrict || parent.IsBondedID)
					{
						var attributeName = isCustomsDistrict ? JobRequiredDocAttribTypeList.Codes.CustomsDistrict : JobRequiredDocAttribTypeList.Codes.BondedID;
						parent.D0_AttribNameInfo.AddWarning(GetWarningForTaiwanAttorneyAttributeUsedByNonApplicableSettings(attributeName));
					}
				}
			}

			CheckDuplicateAttrib();
		}

		void CheckDuplicateAttrib()
		{
			if (!Parent.D0_AttribName.IsEmpty)
			{
				var reqDoc = Parent.RequiredDocument;
				if (reqDoc != null)
				{
					foreach (var attrib in reqDoc.Attributes)
					{
						if (attrib != Parent && attrib.D0_AttribName == Parent.D0_AttribName)
						{
							Parent.D0_AttribNameInfo.AddError(OnlyOneAttributeTypeIsAllowedPerRecord(Parent.D0_AttribName));
							break;
						}
					}
				}
			}
		}

		public static string CompanyCodeAttributeDoesNotSupportDocumentType(ZString[] documentTypesSupportCompanyCode)
		{
			return ResString.GetMultilingualString("EBCE9BEF-4D30-4E03-B80D-01B1F4B88FA8", "Attribute 'COMPANY CODE' is only applicable for Document Type '{0}'.", string.Join("', '", documentTypesSupportCompanyCode));
		}

		public static string TradePreferenceCodeAttributeDoesNotSupportDocumentType(ZString[] documentTypesSupportTradePreferenceCode, ZString[] relatedCountrySupportTradePreferenceCode)
		{
			return ResString.GetMultilingualString("F9FA738E-E4E9-4106-94DF-5F0DC0A0088D", "Attribute 'TRADE PERFERENCE CODE' is only applicable for document type '{0}' and country '{1}'.", string.Join(", ", documentTypesSupportTradePreferenceCode), string.Join("/", relatedCountrySupportTradePreferenceCode));
		}

		public static string AttributeIsOnlyApplicableToVATExporterExemption(string name)
		{
			return ResString.GetMultilingualString("B437EAC2-A71E-43DC-A4AC-BDC461B9C56E", "Attribute '{0}' is only applicable for Document Type '{1}'.", name, Core.Constants.RefDocTypes.VATExporterExemption);
		}

		public static string SellerControlNumberIsOnlyApplicableToEXVDebtor
		{
			get { return ResString.GetMultilingualString("3B426248-7543-4BAB-87FA-167D7B282F19", "Seller Control Number is only applicable to EXV document type with Usage type DBT."); }
		}

		public static string CurrentLoginCompanyDoesNotSupportDeclarationOfIntent(string name)
		{
			return ResString.GetMultilingualString("9B4B1E31-AD15-4265-892E-F80743B1FAE7", "Attribute '{0}' is only applicable when the current login company's country/region support declaration of intent.", name);
		}

		public static string DocumentRelatedCountryDoesNotSupportDeclarationOfIntent(string name)
		{
			return ResString.GetMultilingualString("4AE4E4C2-A372-4EEF-9E62-C2929EED4701", "Attribute '{0}' is only applicable when the document's related country/region support declaration of intent.", name);
		}
		public static string AttributeIsOnlyApplicableToPowerOfAttorney(string name)
		{
			return ResString.GetMultilingualString("B579E28D-8847-4e6d-B5C9-2421CFFE30AA", "Attribute '{0}' is only applicable for Document Type '{1} - {2}', '{3} - {4} and '{5} - {6}'.", name, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypeDescriptions.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypeDescriptions.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding, Core.Constants.RefDocTypeDescriptions.PowerOfAttorneyForwarding);
		}

		public static string PortOfEntryNotApplicableForExport
		{
			get { return ResString.GetMultilingualString("5DDB31B0-84A1-4295-B3C3-B12485B69B69", "Port Of Entry attribute will be ignored when a Direction attribute with 'EXP' value is specified."); }
		}

		public static string OnlyOneAttributeTypeIsAllowedPerRecord(string name)
		{
			return ResString.GetMultilingualString("FDBBB3EF-BC37-491C-BC08-015F3AAB9101", "Only one '{0}' attribute type is allowed per document tracking record.", name);
		}

		public static string InvalidProtocolloFormat
		{
			get { return ResString.GetMultilingualString("0052C097-2646-4702-9B7B-5907E3487811", "Invalid format - it must be 17 numeric characters – six numeric characters: 'nnnnnnnnnnnnnnnnn-nnnnnn'."); }
		}

		public static string InvalidDocumentReceivedDate
		{
			get { return ResString.GetMultilingualString("973AEC57-A3EC-4ECB-9C2A-408C97992219", "Invalid document received date."); }
		}

		public static string InvalidBuyerIssueDate
		{
			get { return ResString.GetMultilingualString("6ade8221-3084-4181-bfb3-b64eff468de6", "Invalid buyer issue date."); }
		}

		public static string InvalidCeilingLimit
		{
			get { return ResString.GetMultilingualString("ce847a78-c416-4a9b-8d13-98d1a1cd9a72", "Invalid ceiling limit."); }
		}

		public static string CeilingLimitMustBePositive
		{
			get { return ResString.GetMultilingualString("d2ba0c78-b72d-48c3-b1d2-81f543931d15", "Ceiling limit must be a positive number."); }
		}

		public static string GetMessageForCostaRicaEXVAttributeValueMissing(string attributeName)
			=> ResString.GetMultilingualString("71870B21-DDCA-42F1-A3FF-C6FE58A6F559", "Value for Attribute: '{0}' is required for Country/Region: 'Costa Rica', Document Type: 'EXV' and Usage: 'DBT'", attributeName);

		public static string GetWarningForCostaRicaEXVAttributeUsedByNonApplicableSettings(string attributeName)
			=> ResString.GetMultilingualString("7E8CCC2E-EF5C-42DD-B375-B0660858E150", "Attribute: '{0}' is only used for Country/Region: 'Costa Rica', Document Type: 'EXV' and Usage: 'DBT'", attributeName);

		public static string GetWarningForTaiwanAttorneyAttributeUsedByNonApplicableSettings(string attributeName)
			=> ResString.GetMultilingualString("41CC3D6B-25E1-4E22-9820-FFDB750CEC4D", "Attribute: '{0}' is only used for Country/Region: 'Taiwan', Document Type in (POA,POC) and Usage: 'BRK'", attributeName);

		public static string InvalidBoxNumber => ResString.GetMultilingualString("00D0F6B5-2BAA-4EB6-8EB2-58BEAD960B3D", "Box Number must consist of exactly three alphanumeric characters.");

		public static string GetErrorForTaiwanAttorneyAttributeValueMissing(string attributeName)
			=> ResString.GetMultilingualString("BD67FD62-552E-4631-A755-685E6EC70208", "Value for Attribute: '{0}' is required.", attributeName);

		public static string InvalidPowerOfAttorneyNumber
		{
			get { return ResString.GetMultilingualString("69fa30c4-40a9-4df8-bf01-65cd6dcc5784", "The entered Power of Attorney number is invalid. There is already a Power of Attorney number with the same Customs District and Box Number and Bonded ID"); }
		}

		public static string CountryCodeOfCompanyDifferFromCountryRegionField
		{
			get { return ResString.GetMultilingualString("8553c08e-da9f-4971-9937-9447252a0ef7", "The country/region of this company does not match the country/region entered in Country/Region field."); }
		}

		public static string InvalidTradePreferenceCodeInUnitedStates
		{
			get { return ResString.GetMultilingualString("23d68afa-91b1-427c-bb49-71ed113f0537", "\"S\" is the only valid value acceptable for TRADE PREFERENCE CODE attribute when the document type = COO, Country = US."); }
		}

		bool IsPowerOfAttorney(ZString docType)
		{
			switch (docType)
			{
				case Core.Constants.RefDocTypes.PowerOfAttorney:
				case Core.Constants.RefDocTypes.PowerOfAttorneyCustoms:
				case Core.Constants.RefDocTypes.PowerOfAttorneyForwarding:
					return true;
				default:
					return false;
			}
		}
	}
}
