using System;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class CRLH2Test : BIRDHeaderUpdateTest
	{
		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			CRLH2 h2 = new CRLH2();
			h2.LocationOfGoods = "A123";
			h2.UltimateConsigneeNumber = "12-1234567AB";
			h2.EntryDateElectionCode = EntryDateElectionCodeList.Codes.ArrivalDate;
			h2.VoyageFlightTripManifestNumber = "V12";
			h2.TotalEntryValue = 10000m;
			h2.VesselNameForeignTradeZoneNumber = "SOUTHERN STAR";

			CRLH2 h2FTZ = new CRLH2();
			h2FTZ.LocationOfGoods = "A123";
			h2FTZ.UltimateConsigneeNumber = "12-1234567AB";
			h2FTZ.EntryDateElectionCode = EntryDateElectionCodeList.Codes.PresentationDate;
			h2FTZ.VoyageFlightTripManifestNumber = "";
			h2FTZ.TotalEntryValue = 10000m;
			h2FTZ.VesselNameForeignTradeZoneNumber = "FTZ087";

			return new IBIRDHeaderRecord[] { h2 };
		}

		protected override void PrepareDeclaration(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			OrgHeader ultimateConsignee = Factory.NewWithValidTestData<OrgHeader>();
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567AB", GlbCompany.CurrentCompany.Country);

			Factory.Save();

			base.PrepareDeclaration(declaration, headerRecord);

			declaration.JE_DeclarationReference = ZString.Empty;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Invoices[0].JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			declaration.InvoiceLines[0].JI_LinePrice = 10000m;
		}

		protected override EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new CargoReleaseMessageBuilder(declaration.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add, false);
		}

		protected override Type GetTypeOfMessageBlock()
		{
			return typeof(CRLH2);
		}

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"BrokerReferenceNumber",//This is not used for BIRD update.
			};
		}
	}
}
