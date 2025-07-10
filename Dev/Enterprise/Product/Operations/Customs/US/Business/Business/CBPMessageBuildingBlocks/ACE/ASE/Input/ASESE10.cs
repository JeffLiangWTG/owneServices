using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	public class ASESE10 : Abstract.ASESE10, IBIRDHeaderRecord, IBIRDHeaderIDRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			declaration.ImportEntryNumber = EntryNumber;
			declaration.US_EntryType = EntryType;

			if (declaration.IOROrgPK.IsEmpty)
			{
				declaration.IOROrgPK = BIRDOrganisationMatching.GetOrganisation(declaration.Factory, OrgMatchedCustomsRegNoType.EIN, GetCompleteEINNumber(ImporterOfRecord), "Importer of Record", notifications)?.PK ?? ZGuid.Empty;
			}
			if (declaration.JE_OH_Importer.IsEmpty)
			{
				declaration.JE_OH_Importer = declaration.IOROrgPK;
			}
			if (!EntryFilerCode.IsEmpty)
			{
				declaration.US_EntryFilerCode = EntryFilerCode;
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

			BIRDTransportMode.SetTransportMode(declaration, ModeOfTransportationMOTCode, notifications);
			declaration.US_BondType = BondTypeCode;
			declaration.US_SchDEntry = PlannedPortOfEntry;
			declaration.US_SchDArrival = PortOfUnlading;
			declaration.US_SESplitRel = SplitShipmentReleaseCode;

			if (declaration.IsEstimatedEnteredValueRequired)
			{
				declaration.US_EstEnteredValue = EstimatedEntryValue;
			}
		}

		ZString GetCompleteEINNumber(ZString number)
		{
			return number.Length == 10 ? number.PadRight(12, '0') : number;
		}

		#endregion

		ZString IBIRDHeaderIDRecord.EntryFilerCode
		{
			get { return EntryFilerCode; }
		}

		ZString IBIRDHeaderIDRecord.EntryNumber
		{
			get { return EntryNumber; }
		}
	}
}
