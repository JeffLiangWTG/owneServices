using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoJobComInvoiceHeaderValidation : USAddInfoValidation
	{
		public AddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceHeader Parent
		{
			get { return (JobComInvoiceHeader)base.Parent.Parent; }
		}

		protected AddInfoJobComInvoiceHeaderLookups Lookups
		{
			get { return (AddInfoJobComInvoiceHeaderLookups)base.Parent.Lookups; }
		}

		protected override void CheckUS_ReleaseEntryNumber()
		{
			Parent.ClearRowNotifications();
			var declaration = Parent.JobDeclaration;

			if (declaration != null && declaration.US_ConsolACE && !declaration.IsCreatedFromUSLowValue)
			{
				if (Parent.US_ReleaseEntryNumber.IsEmpty)
				{
					Parent.US_ReleaseEntryNumberInfo.AddMessageError(EntryFilerEntryNumberMandatory);
				}
				else
				{
					EntryNumberValidator.ValidateFormatAndCheckDigit(Parent.US_ReleaseEntryNumberInfo, Parent.Branch);

					var releaseEntryJob = Parent.ReleaseDeclaration;
					if (releaseEntryJob == null)
					{
						Parent.US_ReleaseEntryNumberInfo.AddMessageError(ZString.Format(NoMatchingReleaseEntryExists, Parent.US_ReleaseEntryNumber));
					}
					else
					{
						if (declaration.PK == releaseEntryJob.PK)
						{
							Parent.US_ReleaseEntryNumberInfo.AddMessageError(EnterEntryNumberFromReleaseOnlyDeclaration);
						}
						else
						{
							CheckUS_ReleaseEntryNumber_TwoDeclarationIsDifferent(declaration, releaseEntryJob);
						}
					}
				}
			}
		}

		void CheckUS_ReleaseEntryNumber_TwoDeclarationIsDifferent(JobDeclaration declaration, JobDeclaration releaseEntryJob)
		{
			var consolidatedJobNumber = releaseEntryJob.US_ConsolidatedJobNumber;
			if (!consolidatedJobNumber.IsEmpty && consolidatedJobNumber != declaration.JE_DeclarationReference)
			{
				Parent.US_ReleaseEntryNumberInfo.AddError(ZString.Format(ReleaseEntryAlreadyOnAnotherConsolidatedEntry, consolidatedJobNumber));
			}

			if (declaration.US_SchDEntry != releaseEntryJob.US_SchDEntry)
			{
				Parent.US_ReleaseEntryNumberInfo.AddMessageError(PortOfEntryShouldMatch);
			}

			if (declaration.IOROrgPK != releaseEntryJob.IOROrgPK)
			{
				Parent.US_ReleaseEntryNumberInfo.AddMessageError(ImporterOfRecordShouldMatch);
			}

			if (declaration.JE_TransportMode != releaseEntryJob.JE_TransportMode)
			{
				Parent.US_ReleaseEntryNumberInfo.AddMessageError(TransportModeShouldMatch);
			}
			else
			{
				if (declaration.JE_TransportMode == TransportTypeList.Codes.Sea)
				{
					if (declaration.JE_VesselName != releaseEntryJob.JE_VesselName || declaration.JE_VoyageFlightNo != releaseEntryJob.JE_VoyageFlightNo)
					{
						Parent.US_ReleaseEntryNumberInfo.AddMessageError(VesselVoyageShouldMatch);
					}
				}
				else if (declaration.JE_TransportMode == TransportTypeList.Codes.Air)
				{
					if (declaration.US_UI_NKCarrierSCAC != releaseEntryJob.US_UI_NKCarrierSCAC)
					{
						Parent.US_ReleaseEntryNumberInfo.AddMessageError(AirCarrierCodeShouldMatch);
					}
				}
			}

			if (releaseEntryJob.US_EntryType != EntryTypeList.Codes.ConsumptionFreeDutiable && releaseEntryJob.US_EntryType != EntryTypeList.Codes.InformalFreeDutiable)
			{
				Parent.US_ReleaseEntryNumberInfo.AddMessageError(EntryTypeNotAccepted);
			}

			if (Parent.ConsigneeAddressOrgPK != releaseEntryJob.ConsigneeAddressOrgPK)
			{
				Parent.US_ReleaseEntryNumberInfo.AddMessageError(ConsigneeShouldMatch);
			}

			if (declaration.RelatedStatement == null || declaration.RelatedStatement.B2_Status != StatementHeaderStatusList.Codes.Final)
			{
				var validReleaseDate = declaration.GetValidReleaseDate();
				if (releaseEntryJob.JE_EntryAuthorisationDate == ZDateTime.Empty)
				{
					Parent.US_ReleaseEntryNumberInfo.AddMessageError(ReleaseDateShouldExist);
				}
				else if (releaseEntryJob.JE_EntryAuthorisationDate < validReleaseDate)
				{
					Parent.US_ReleaseEntryNumberInfo.AddMessageError(ReleaseDatePastLimit);
				}
			}

			if (releaseEntryJob.ReleaseStatus != CRLReleaseStatusList.Codes.REL)
			{
				Parent.US_ReleaseEntryNumberInfo.AddMessageError(ReleaseStatusNotAccepted);
			}

			var releaseEntry = Parent.ENSReleaseEntry;
			if (releaseEntry != null && releaseEntry.HasBeenLodgedAtCustoms)
			{
				Parent.US_ReleaseEntryNumberInfo.AddMessageError(MessageTypeNotAccepted);
			}
		}

		protected override void CheckUS_UltimateDestinationCountry()
		{
			base.CheckUS_UltimateDestinationCountry();
			if (Parent.US_UltimateDestinationCountry != USCCountry.Unknown)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_UltimateDestinationCountryInfo, Parent.AddInfoLookups.CountryOfDestinations);
			}
			if (Parent != null && Parent.JZ_OH_Buyer != ZGuid.Empty && Parent.JobDeclaration != null
				&& Parent.JZ_OH_Buyer != Parent.JobDeclaration.JE_OH_Importer && !Parent.UltimateConsigneeDocAddress.E2_AddressOverride)
			{
				Parent.US_UltimateDestinationCountryInfo.AddWarning(ZString.Format(UltimateDestinationCountryUpdated, Parent.US_UltimateDestinationCountry));
			}
		}

		internal const string EntryFilerEntryNumberMandatory = "Both entry filer code and entry number should be entered.";
		internal const string NoMatchingReleaseEntryExists = "Release Entry '{0}' does not exist.";
		internal const string ReleaseEntryAlreadyOnAnotherConsolidatedEntry = "This Release Entry is already on another Consolidated Entry '{0}'.";
		internal const string PortOfEntryShouldMatch = "'Entry Port' of Release Entry should be same as Consolidated Entry.";
		internal const string ImporterOfRecordShouldMatch = "'Importer Of Record' of Release Entry should be same as Consolidated Entry.";
		internal const string ConsigneeShouldMatch = "'Consignee' of Release Entry should be same as Consolidated Entry.";
		internal const string TransportModeShouldMatch = "'Transport' of Release Entry should be same as Consolidated Entry.";
		internal const string VesselVoyageShouldMatch = "'Vessel/Voyage' of Release Entry should be same as Consolidated Entry.";
		internal const string AirCarrierCodeShouldMatch = "'Carrier SCAC' of Release Entry should be same as Consolidated Entry.";
		internal const string ReleaseDateShouldExist = "Release Date  should not be empty.";
		internal const string ReleaseDatePastLimit = "Release Date is older than 10 days.";
		internal const string EntryTypeNotAccepted = "'Entry Type' of Release Entry should be '01' or '11'";
		internal const string MessageTypeNotAccepted = "Release Entry should not have an accepted ENS message";
		internal const string ReleaseStatusNotAccepted = "Release Entry should have release status 'REL'";
		internal const string UltimateDestinationCountryUpdated = "Ult. Country of Destination: '{0}' has been automatically updated to match the current Consignee - you may manually change this if required.";
		internal const string EnterEntryNumberFromReleaseOnlyDeclaration = "You cannot enter the entry number of a consolidated entry in to the release entry number. Please enter an entry number from a release only declaration. ";
	}
}
