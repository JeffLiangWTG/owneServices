using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS11 : Abstract.AENS11, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			if (declaration.IOROrgPK.IsEmpty)
			{
				declaration.IOROrgPK = BIRDOrganisationMatching.GetOrganisation(declaration.Factory, OrgMatchedCustomsRegNoType.EIN, GetCompleteEINNumber(ImporterOfRecordNumber), "Importer of Record", notifications)?.PK ?? ZGuid.Empty;
			}

			if (!DesignatedNotifyParty4811Number.IsEmpty)
			{
				if (DesignatedNotifyParty4811Number != declaration.CBPF4811ReferenceNumber)
				{
					foreach (var codeType in new[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber })
					{
						var notifyPartyPK = BIRDOrganisationMatching.GetOrganisation(declaration.Factory, codeType, DesignatedNotifyParty4811Number, "CBPF 4811 Notify Party", null)?.PK ?? ZGuid.Empty;
						if (!notifyPartyPK.IsEmpty)
						{
							declaration.JE_OH_NotifyParty = notifyPartyPK;
							break;
						}
					}
				}

				if (notifications != null && DesignatedNotifyParty4811Number != declaration.CBPF4811ReferenceNumber)
				{
					notifications.AddWarning(BIRDOrganisationMatching.GetNoOrganizationMatchMsg("CBPF 4811 Notify Party", DesignatedNotifyParty4811Number)
						+ "CBPF 4811 Notify Party can either be set up on Declaration or on Importer Of Record (Organization -> Config -> US Defaults)");
				}
			}

			if (declaration.JE_OH_Importer.IsEmpty)
			{
				declaration.JE_OH_Importer = declaration.IOROrgPK;
			}

			if (declaration.JE_OA_ConsigneeAddress.IsEmpty)
			{
				declaration.JE_OA_ConsigneeAddress = BIRDOrganisationMatching.GetCustomsRecordOrMainAddressOfOrganisation(declaration.Factory, OrgMatchedCustomsRegNoType.EIN, GetCompleteEINNumber(ConsigneeNumber), "Ultimate Consignee", notifications);
			}

			if (!declaration.IsInDatabase)
			{
				var importerWrapper = OrgHeaderWrapper.New(declaration.Importer);

				if (importerWrapper != null && importerWrapper.ZO_GB.IsValid)
				{
					declaration.JE_GB = importerWrapper.ZO_GB;
					if (declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.PGARequirementIndicator.HasACEFDARequirement))
					{
						declaration.DefaultFDAContact();
					}
				}
			}

			declaration.US_EstimatedEntryDate = EstimatedEntryDate;
			declaration.JE_DateOfArrival = DateOfImportation;
			declaration.US_DestinationState = USStateOfDestinationCode;

			if (!ForeignTradeZoneIdentifier.IsEmpty || !NewForeignTradeZoneIdentifier.IsEmpty)
			{
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
				declaration.US_FTZNo = ForeignTradeZoneIdentifier.IsEmpty ? NewForeignTradeZoneIdentifier : ForeignTradeZoneIdentifier;
			}
		}

		ZString GetCompleteEINNumber(ZString number)
		{
			return number.Length == 10 ? number.PadRight(12, '0') : number;
		}

		#endregion
	}
}
