using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class WarehouseEntryDataEventContextCreatorTest : TestCaseWithFactory
	{
		public void TestEventContextValues()
		{
			// Setup Declaration with 2 entries
			// Generate the event context for one entry
			// Assert that only data related to that entry is included

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB100031";
			declaration.JE_HouseBill = "HB1000460";
			declaration.JE_RL_NKFinalDestination = "CATOR";
			declaration.JE_OH_ShippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "CCCB", "US").PK;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = "TP1";
			entry1.EntryNumber = "ENTNUM123";
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = "TP2";
			entry2.EntryNumber = "ENTNUM456";

			Factory.Save();
			var manager = entry1.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValues = string.Join(System.Environment.NewLine, manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());

			AssertMultilineASCIIEquals("manager.EventContextValues", @"
CarrierCode - CCCB
MBOLNumber - MB100031
MBOLDestinationUNLOCO - CATOR
HBOLNumber - HB1000460
HBOLDestinationUNLOCO - CATOR
EntryNumber - ENTNUM123
EntryNumberCountryOfIssue - ER
EntryNumberType - TP1
MessageType - TP1".Trim(), eventContextValues);
		}
	}
}
