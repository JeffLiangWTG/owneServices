using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Protest
{
	public class ProtestValidation : ZValidation
	{
		public ProtestValidation(Protest protest)
			: base(protest)
		{
			this.Protest = protest;
		}

		public override void ValidateAll()
		{
			ValidateTariffActCitation();
		}

		internal void ValidateEntries()
		{
			Protest.ClearRowNotifications();
			if (Protest.LinkedEntries.Count == 0)
			{
				Protest.AddRowMessageError(EntryRequired);
			}
		}
		internal const string EntryRequired = "At least one Linked Entry is required for protest messaging.";

		#region Validate RefundPartyAddress
		internal void ValidateRefundPartyOrganisationPK(JobDocAddressValidation validation)
		{
			JobDocAddress refundParty = validation.Parent;

			if (refundParty.OrganisationPK.IsEmpty && !Protest.US_P_RefundCOPartyType.IsEmpty)
			{
				refundParty.OrganisationPKInfo.AddMessageError("Refund C/O Party is required when a Refund C/O Party Type has a value.");
			}
			else if (!refundParty.OrganisationPK.IsEmpty && Protest.RefundPartyId.IsEmpty)
			{
				refundParty.OrganisationPKInfo.AddMessageError("Refund Party Organization does not have a valid EIN, SSN or CBN Identification Number.");
			}
		}
		#endregion

		#region Validate Protestant Address Details

		internal void ValidateProtestantOrganisationPK(JobDocAddressValidation validation)
		{
			JobDocAddress protestant = validation.Parent;

			if (!protestant.E2_AddressOverride)
			{
				if (protestant.OrganisationPK.IsEmpty)
				{
					protestant.OrganisationPKInfo.AddMessageError(ProtestantIsRequired);
				}
				else
				{
					RegistrationNumberResult regNumberResult = GetRegistrationNumberResult(protestant);
					if (regNumberResult.RegistrationNumber.IsEmpty)
					{
						protestant.OrganisationPKInfo.AddMessageError(ProtestantIDIsRequired);
					}
					else
					{
						string message = "";
						if (!ProtestantTypeList.RequiresProtestantAddressDetails(Protest.US_P_ProtestantType))
						{
							message = CheckProtestantIdentificationFormat(regNumberResult.RegistrationNumber);
							if (!string.IsNullOrEmpty(message))
							{
								message += "\nNOTE: Only (EIN), (SSN) and (CBP) numbers are enterable on the organisation. For all other identification numbers, please override the address.";
								protestant.OrganisationPKInfo.AddMessageError(message);
							}
						}

						message = CheckCountryCode(protestant.E2_RN_NKCountryCode);
						if (!string.IsNullOrEmpty(message))
						{
							protestant.OrganisationPKInfo.AddMessageError(message);
						}
					}

					Protest.Declaration.AddInfoValidation.ValidateUS_P_ProtestantType();
				}
			}
		}

		internal const string ProtestantIsRequired = "Protestant is required.";
		internal const string ProtestantIDIsRequired = "Protestant Identification Number is required.";

		internal RegistrationNumberResult GetRegistrationNumberResult(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true,
				delegate
				{
					RegistrationNumber result = new RegistrationNumber();
					if (docAddress != null && !docAddress.IsDeleted)
					{
						OrgHeader org = Protest.Factory.Load<OrgHeader>(docAddress.OrganisationPK);

						if (org != null)
						{
							OrgCusCode cusCode = OrgHeaderWrapper.GetCustomsRelatedOrgCusCode(org, OrgMatchedCustomsRegNoType.EIN);
							if (cusCode != null)
							{
								result.Number = cusCode.OK_CustomsRegNo;
								result.NumberType = cusCode.OK_CodeType;
							}
						}
					}
					return result;
				});
		}

		internal void ValidateCountry(JobDocAddressValidation validation)
		{
			JobDocAddress protestant = validation.Parent;
			string message = CheckCountryCode(protestant.E2_RN_NKCountryCode);
			if (!string.IsNullOrEmpty(message))
			{
				protestant.E2_RN_NKCountryCodeInfo.AddMessageError(message);
			}
		}

		string CheckCountryCode(ZString countryCode)
		{
			ZString result = "";

			if (!countryCode.IsEmpty)
			{
				bool validCountry =
					countryCode == Core.Constants.CountryCodes.UnitedStates
					|| countryCode == Core.Constants.CountryCodes.Canada
					|| countryCode == Core.Constants.CountryCodes.Mexico
					|| countryCode == Core.Constants.CountryCodes.Chile;

				if (!validCountry && ProtestantTypeList.RequiresProtestantAddressDetails(Protest.US_P_ProtestantType))
				{
					result = ValidProtestantCountry;
				}
			}

			if (countryCode == Core.Constants.CountryCodes.Chile)
			{
				if (!Is520Petition || Protest.US_P_ProtestantType != ProtestantTypeList.Codes.ForeignExporterProducer)
				{
					result = "Chile ('CL') is only valid in the case that Tariff Act Citation is '520(d) Petition' and Protestant Type is 'Foreign Exporter/Producer'.";
				}
			}

			return result;
		}
		internal const string ValidProtestantCountry = "Valid Protestant Country (Cnty) Codes are 'US', 'CA', 'MX', 'CL'.";

		internal void ValidateGovRegNo(JobDocAddressValidation validation)
		{
			JobDocAddress protestant = validation.Parent;
			if (protestant.E2_GovRegNum.IsEmpty)
			{
				protestant.E2_GovRegNumInfo.AddMessageError(ProtestantIDIsRequired);
			}
			else
			{
				string message = CheckProtestantIdentificationFormat(protestant.E2_GovRegNum);
				if (!string.IsNullOrEmpty(message))
				{
					protestant.E2_GovRegNumInfo.AddMessageError(message);
				}
			}
		}

		string CheckProtestantIdentificationFormat(string protestantIdentificationNumber)
		{
			string result = "";

			bool correctFormat =
				Regex.IsMatch(protestantIdentificationNumber, @"^[0-9]{3}$")
				|| Regex.IsMatch(protestantIdentificationNumber, @"^[0-9]{2}-[0-9]{7}[A-Z0-9]{2}$")
				|| Regex.IsMatch(protestantIdentificationNumber, @"^[0-9]{3}-[0-9]{2}-[0-9]{4}$")
				|| Regex.IsMatch(protestantIdentificationNumber, @"^[0-9]{2}[A-Z0-9]{4}-[0-9]{5}$")
				|| Regex.IsMatch(protestantIdentificationNumber, @"^[A-Z]{3}[0-9]{6}$")
				|| Regex.IsMatch(protestantIdentificationNumber, @"^[A-Z]{4}[0-9]{6}[A-Z0-9]{3}$")
				|| Regex.IsMatch(protestantIdentificationNumber, @"^[A-Z]{3}[0-9]{6}[A-Z0-9]{3}$");

			if (!correctFormat)
			{
				result =
@"Please enter a valid Protestant Identification Number.

Valid formats are;
'NNN' CBP-Assigned Surety Code
'NN-NNNNNNNXX' Employer Identification Number (EIN) or Internal Revenue Service Number
'NNN-NN-NNNN' Social Security Number (SSN)
'YYDDPPP-NNNNN' CBP-Assigned Number (CBP) (New format only)
'AAANNNNNN' Canadian Employer Number
'AAANNNNNN' Canadian Importer/Exporter Number
'AAAAYYMMDDXXX' Mexican Federal Tax Registry Number (individual)
'AAAYYMMDDXXX' Mexican Federal Tax Registry Number (corporate)";
			}

			return result;
		}

		bool Is520Petition
		{
			get { return Protest.TariffActCitation == TariffActCitationList.Codes.C_Section520d; }
		}

		#endregion

		#region TariffActCitation

		public void ValidateTariffActCitation()
		{
			ValidateCalculatedProperty(Protest.TariffActCitationInfo);
		}

		protected void CheckTariffActCitation()
		{
			if (Protest.TariffActCitation.IsEmpty)
			{
				Protest.TariffActCitationInfo.AddMessageError(TariffActCitationRequired);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Protest.TariffActCitationInfo, Protest.Lookups.TariffActCitations, (NoResString)TariffActCitation);
				Protest.Declaration.AddInfoValidation.ValidateUS_P_ProtestantType();
				Protest.Declaration.AddInfoValidation.ValidateUS_P_AcceleratedDispositionInd();
				Protest.Declaration.AddInfoValidation.ValidateUS_P_TestSummonsNo();
				Protest.Declaration.AddInfoValidation.ValidateUS_P_InternalAdviceNo();
				Protest.Declaration.AddInfoValidation.ValidateUS_P_ApplicationQuestion1();
				Protest.Declaration.AddInfoValidation.ValidateUS_P_ApplicationQuestion2();
				Protest.Declaration.AddInfoValidation.ValidateUS_P_ApplicationQuestion3();
				Protest.Declaration.AddInfoValidation.ValidateUS_P_PeriodBaseDate();
			}
		}
		internal const string TariffActCitation = "Tariff Act Citation you have entered is not in the list.";
		internal const string TariffActCitationRequired = "Tariff Act Citation is required for Protest.";

		#endregion

		#region JustificationNote

		public void ValidateJustificationNote()
		{
			ValidateCalculatedProperty(Protest.JustificationNoteInfo);
		}

		protected void CheckJustificationNote()
		{
			if (Protest.JustificationNote.IsEmpty)
			{
				Protest.JustificationNoteInfo.AddMessageError(JustificationNoteRequired);
			}
		}
		internal const string JustificationNoteRequired = "Justification Note is mandatory.";

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(Protest); }
		}

		protected readonly Protest Protest;

		#endregion
	}
}
