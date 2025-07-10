using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassShipmentDocumentSupporter))]
	public class GatePassShipmentDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage_NoPacklines()
		{
			var shipment = Factory.New<GatePassShipment>();
			shipment.DestinationCFSDepartures.DeleteAll();

			var contextArray = new[]
			{
				Constants.DataContext.GatePassContainerLeg,
				Constants.DataContext.GenericPickupDeliveryConfirm,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "Please register at least one delivery before attempting to print a Gate Pass.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

			shipment.FillWithValidTestData();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 5;
			packLine.JL_Outturn = 20;

			var confirm = shipment.DestinationCFSDepartures.AddNew();
			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture;

			var divot = confirm.GetDivot(packLine);
			divot.J8_PackagesDelivered = 10;

			Factory.Save();

			AssertNotFoundMessage(shipment, menu, contextArray, false, ZString.Empty);
		}

		protected override bool ShouldSkipWithContextAndMenu(Constants.DataContext context, IStmMenuItem menu)
		{
			var shipment = Factory.New<GatePassShipment>();
			var supporter = shipment.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, menu);

			return wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null);
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForMessageNotPrintingTest
		{
			get { return new[] { Factory.New<GatePassShipment>() }; }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var shipment = Factory.New<GatePassShipment>();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = packLine1.JL_Outturn = 20;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = packLine2.JL_Outturn = 20;

			shipment.DestinationCFSDepartures.AddNew();

			return shipment;
		}

		public void TestSupportedDataContext()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			AssertEquals("Core.Constants.DataContext.GenericPickupDeliveryConfirm is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericPickupDeliveryConfirm)));
		}

		public void TestGetConfirmWrappersToPrint()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			CFSPackLine packLine = shipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm leg = shipment.DestinationCFSDepartures.AddNew();
			leg.EU_PickupDeliveryType = "CDD";
			CommonConfirmDivot divot = leg.GetDivot(packLine);
			divot.J8_PackagesDelivered = 10;

			DocumentPickupDeliveryConfirm documentConfirm = new DocumentPickupDeliveryConfirm(leg);
			AssertEquals(false, documentConfirm.IsInDatabase);
			AssertEquals(ZString.Empty, documentConfirm.Confirm.UniqueID);

			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericPickupDeliveryConfirm, null);

			AssertEquals("One wrapper", 1, wrappers.Length);
		}
	}
}
