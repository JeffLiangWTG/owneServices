using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS10 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS10, IBIRDHeaderRecord, IBIRDHeaderIDRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.US_SchDEntry = DistrictPortOfEntry;

			declaration.IOROrgPK = BIRDOrganisationMatching.GetOrganisation(declaration.Factory, OrgMatchedCustomsRegNoType.EIN, GetCompleteEINNumber(ImporterOfRecordNumber), "Importer of Record", notifications)?.PK ?? ZGuid.Empty;

			if (!CBPF4811ReferenceNumber.IsEmpty)
			{
				if (CBPF4811ReferenceNumber != declaration.CBPF4811ReferenceNumber)
				{
					foreach (var codeType in new[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber })
					{
						var notifyPartyPK = BIRDOrganisationMatching.GetOrganisation(declaration.Factory, codeType, CBPF4811ReferenceNumber, "CBPF 4811 Notify Party", null)?.PK ?? ZGuid.Empty;
						if (!notifyPartyPK.IsEmpty)
						{
							declaration.JE_OH_NotifyParty = notifyPartyPK;
							break;
						}
					}
				}

				if (notifications != null && CBPF4811ReferenceNumber != declaration.CBPF4811ReferenceNumber)
				{
					notifications.AddWarning(BIRDOrganisationMatching.GetNoOrganizationMatchMsg("CBPF 4811 Notify Party", CBPF4811ReferenceNumber)
						+ "CBPF 4811 Notify Party can either be set up on Declaration or on Importer Of Record (Organization -> Config -> US Defaults)");
				}
			}

			if (declaration.JE_OH_Importer.IsEmpty)
			{
				declaration.JE_OH_Importer = declaration.IOROrgPK;
			}

			declaration.JE_OA_ConsigneeAddress = BIRDOrganisationMatching.GetCustomsRecordOrMainAddressOfOrganisation(declaration.Factory, OrgMatchedCustomsRegNoType.EIN, GetCompleteEINNumber(UltimateConsigneeNumber), "Ultimate Consignee", notifications);

			if (!declaration.IsInDatabase)
			{
				OrgHeaderWrapper importerWrapper = OrgHeaderWrapper.New(declaration.Importer);

				if (importerWrapper != null && importerWrapper.ZO_GB.IsValid)
				{
					declaration.JE_GB = importerWrapper.ZO_GB;
					if (declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.PGARequirementIndicator.HasFDARequirement))
					{
						declaration.DefaultFDAContact();
					}
				}
			}

			declaration.US_LiveEntryIndicator = LiveEntryIndicator == 1 ? YesNoDefaultList.Codes.Yes : string.Empty;

			declaration.US_MissingDocument1 = MissingDocumentCodes.SubstringSafe(0, 2);

			declaration.US_MissingDocument2 = MissingDocumentCodes.SubstringSafe(2, 2);

			declaration.US_BondType = BondType != BondTypeList.Codes.NoBondRequired ? BondType : ZString.Empty;

			declaration.US_EstimatedEntryDate = EstimatedEntryDate;

			if (!EntryFilerCode.IsEmpty)
			{
				declaration.US_EntryFilerCode = EntryFilerCode;
			}

			declaration.ImportEntryNumber = EntryNumber;

			declaration.US_EntryType = EntryType;

			declaration.US_SuretyCode = SuretyCode;

			declaration.US_DestinationState = StateOfDestination;

			declaration.US_OGALineReleaseIndicator = OGALineReleaseIndicator == 1 ? YesNoDefaultList.Codes.Yes : string.Empty;
		}

		ZString GetCompleteEINNumber(ZString number)
		{
			return number.Length == 10 ? number.PadRight(12, '0') : number;
		}

		#endregion

		#region IBIRDHeaderIDRecord Members

		ZString IBIRDHeaderIDRecord.EntryNumber
		{
			get { return EntryNumber; }
		}

		ZString IBIRDHeaderIDRecord.EntryFilerCode
		{
			get { return EntryFilerCode; }
		}

		#endregion
	}
}
