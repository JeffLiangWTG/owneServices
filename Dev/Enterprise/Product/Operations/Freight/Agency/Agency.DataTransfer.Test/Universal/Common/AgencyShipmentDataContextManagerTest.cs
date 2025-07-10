using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal abstract class AgencyShipmentDataContextManagerTest<T, U> : ShipmentDataContextManagerTestCase<T, U> where T : AgencyShipmentDataContextManager<U>, new()
		where U : AgencyShipment
	{
		public void TestDataContextKey()
		{
			var shipment = Factory.New<U>();
			shipment.JS_UniqueConsignRef = "S001";
			var manager = GetNewDataContextManager();
			((IDataContextManager)manager).Init(shipment);
			AssertEquals("S001", manager.DataContextKey);
		}

		public void TestDefaultOutputDirectory()
		{
			const string testDirectory = @"c:\Test"; // constant for test
			string originalShipmentExportDirectory = SystemDataRegistry.Instance.ShipmentExportDirectory.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			SystemDataRegistry.Instance.ShipmentExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);
			try
			{
				var manager = GetNewDataContextManager();
				AssertEquals(testDirectory, manager.DefaultOutputDirectory);
			}
			finally
			{
				SystemDataRegistry.Instance.ShipmentExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalShipmentExportDirectory);
			}
		}

		public void TestManagesShipmentsManagesEvents()
		{
			var manager = GetNewDataContextManager();
			AssertEquals(true, manager.ManagesShipments);
			AssertEquals(true, manager.ManagesEvents);
		}

		public void TestGetDataContextKeyMatchingQuery()
		{
			var shipment = Factory.New<U>();
			shipment.JS_UniqueConsignRef = "S1";
			var manager = GetNewDataContextManager();
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.CodesMappedToTarget = true;
			dataObject.DataContext.AddDataTarget(DataContextType.DummyBusinessObject, shipment.JS_UniqueConsignRef);
			var dataTarget = dataObject.DataContext.DataTargetCollection.FirstOrDefault();
			AssertEquals(shipment, ((IDataContextManager)manager).LoadBusinessObjectFromDataTarget(dataObject, dataTarget, Factory.BOFactory, new DummyLogger()));
		}

		public void TestPopulateUniversalEventContextReferences()
		{
			var billOfLading = Factory.New<U>();
			billOfLading.JS_HouseBill = "HBL001";
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "12345";
			var manager = GetNewDataContextManager();
			((IDataContextManager)manager).Init(billOfLading);
			AssertContainsExactElementsInAnyOrder(new[] { "MBOLNumber|HBL001" }, Format(manager.EventContextValues));
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "USSFO", "USS ESSES", "001");
			sailing.Vessel.RV_LloydsNumber = vessel.RV_LloydsNumber;
			billOfLading.JS_JX = sailing.PK;
			manager = GetNewDataContextManager();
			((IDataContextManager)manager).Init(billOfLading);
			AssertContainsExactElementsInAnyOrder(new[] { "MBOLNumber|HBL001", "LloydsNumber|12345", "VesselName|USS ESSES", "VoyageNumber|001", "LegOriginUNLOCO|AUSYD", "LegDestinationUNLOCO|USSFO" }, Format(manager.EventContextValues));
		}

		public void TestTransformWithEventTypeSBR()
		{
			var universalEvent = new UniversalEvent();
			var referenceContext = new Context() { Type = new ContextType { Type = "Reference" }, Value = "reference" };
			var c1cContext = new Context() { Type = new ContextType { Type = "CarrierC1CCode" }, Value = "C1CO" };
			universalEvent.ContextCollection = new List<Context> { referenceContext, c1cContext };

			var eventValue = new EventValue(Events.SubscriptionRequested);
			var result = GetNewDataContextManager().Transform(eventValue, universalEvent, null);

			AssertEquals("Shipment Visibility", result.Parameters[EventReferenceParameters.Codes.Type]);
			AssertEquals(null, result.Parameters[EventReferenceParameters.Codes.MessageType]);
			AssertEquals("reference", result.Parameters[EventReferenceParameters.Codes.InterchangeNumber]);
			AssertEquals("C1CO", result.Parameters[EventReferenceParameters.Codes.Organization]);
		}

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, UniversalShipment shipmentWithRecipientRole)
		{
			if (recipientRoleType == RecipientRoleType.CAR)
			{
				shipmentWithRecipientRole.DataContext.ClearDataSourceCollection();
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.AgencyBooking, null);
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get
			{
				return new RecipientRoleType[] { RecipientRoleType.CAR };
			}
		}

		#region Implementation
		protected T GetNewDataContextManager()
		{
			return new T();
		}

		IEnumerable<string> Format(IEnumerable<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			return contextValues.Select(Format);
		}

		string Format(KeyValuePair<TypeWithDescription, IZType> contextValue)
		{
			return string.Format("{0}|{1}", contextValue.Key.Type, contextValue.Value);
		}
		#endregion
	}
}
