using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ShipmentVisualizerDocumentDataIncomingEventProcessorTest : TestCaseWithFactory
	{
		#region TestProcess

		public void TestProcess_CRESA()
		{
			var shipment = CreateShipment();
			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_LastKnownTransitWarehouseStatus = "DSP";
			var packline2 = shipment.OuterPackLines.AddNew();

			Factory.Save();

			AssertNull("Pre-condition: ECV number is not set.", shipment.Numbers.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ExportConventional, Core.Constants.CountryCodes.France));
			AssertNullOrEmpty("Pre-condition: Export Reference Number is not set.", packline1.JL_ExportRefNumber);
			AssertNullOrEmpty("Pre-condition: Export Reference Number is not set.", packline2.JL_ExportRefNumber);

			var visualizerDocumentData = Factory.New<VisualizerDocumentData>();
			visualizerDocumentData.JDD_ParentID = shipment.PK;
			visualizerDocumentData.JDD_ParentTableCode = shipment.TablePrefix;
			visualizerDocumentData.JDD_Name = "XXX";

			var parameters = new Dictionary<string, string>
			{
				["MST"] = FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA,
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber] = "refnumber"
			};

			visualizerDocumentData.Logs.CreateOrRecreateEventLog(Events.MessageAccepted, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, parameters.ToArray());

			var ecvValue = shipment.Numbers.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ExportConventional, Core.Constants.CountryCodes.France);

			AssertEquals("ECV number is set", "refnumber", ecvValue.CE_EntryNum);
			AssertEquals("Export Reference Number is set.", "refnumber", packline1.JL_ExportRefNumber);
			AssertNullOrEmpty("Export Reference Number is not set.", packline2.JL_ExportRefNumber);
		}

		public void TestProcess_ISN()
		{
			var shipment = CreateShipment();

			Factory.Save();

			var visualizerDocumentData = Factory.New<VisualizerDocumentData>();
			visualizerDocumentData.JDD_ParentID = shipment.PK;
			visualizerDocumentData.JDD_ParentTableCode = shipment.TablePrefix;
			visualizerDocumentData.JDD_Name = "XXX";

			var parameters = new Dictionary<string, string>
			{
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType] = ShipmentDocumentNames.BookingRequest,
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber] = "refNumber"
			};

			visualizerDocumentData.Logs.CreateOrRecreateEventLog(Events.InterchangeSent, EstimateActual.Actual, ZDateTimeOffset.Now, string.Empty, parameters.ToArray());

			var cmrValue = shipment.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference);

			AssertNotNull(cmrValue);
			Assert(cmrValue.CE_EntryIsSystemGenerated);
			AssertEquals("CRM Number is set", "refNumber", cmrValue.CE_EntryNum);
		}

		#endregion

		#region implementation
		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDischargePort = "FRMRS";
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_RL_NKLoadPort = "FRPAR";
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";

			Factory.Save();

			return shipment;
		}
		#endregion
	}
}
