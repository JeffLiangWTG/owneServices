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
	[TestedType(typeof(CFSShipmentDocumentSupporter))]
	class CFSShipmentDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage_ArrivalConfirmations()
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.OriginCFSArrivals.DeleteAll();

			var contextArray = new[]
			{
				Constants.DataContext.CFSContainerLeg,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This shipment does not have any Arrival information entered within the Arrival tab.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoServices()
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.DocsAndCartage.Services.RemoveAndDeleteAll();

			var contextArray = new[]
			{
				Constants.DataContext.RequestForService,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This shipment does not have any Services information entered within the Shipment > Services grid.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoPacklines()
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var contextArray = new[]
			{
				Constants.DataContext.GenericFreightJobByPackages,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This shipment does not have any packlines.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 5;

			AssertNotFoundMessage(shipment, menu, contextArray, false, ZString.Empty);
		}

		protected override bool ShouldSkipWithContextAndMenu(Constants.DataContext context, IStmMenuItem menu)
		{
			var shipment = Factory.New<CFSShipment>();
			var supporter = shipment.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, menu);

			return wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null);
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForMessageNotPrintingTest
		{
			get { return new[] { Factory.New<CFSShipment>() }; }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			CFSLoadListConsol consol = shipment.Consols.AddNew();
			CFSContainer container = consol.Containers.AddNew();

			CFSPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 7;

			CommonPickupDeliveryConfirm confirm = shipment.OriginCFSArrivals.AddNew();

			CFSService service = shipment.DocsAndCartage.Services.AddNew();
			service.ES_ServiceCode = "FUM";

			Factory.Save();

			return shipment;
		}

		public void TestGetGenericWrapperForOuterPacks()
		{
			var shipment = Factory.New<CFSShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = "PLT";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 12;
			packLine2.JL_F3_NKPackType = "PLT";

			DocumentWrapper[] wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobByPackages, null);
			AssertEquals("Shipment wrappers should be created", 22, wrapper.Length);
		}
	}
}
