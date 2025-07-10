using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	[TestedType(typeof(DtbConsignmentDataContextManager))]
	class DtbConsignmentDataContextManagerTest : ShipmentDataContextManagerTestCase<DtbConsignmentDataContextManager, DtbConsignment>
	{
		#region Context
		#region TestDataContextType
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.LandTransportConsignment, new DtbConsignmentDataContextManager().DataContextType);
		}

		#endregion
		#region TestDataContextKey
		public void TestDataContextKey()
		{
			var consignment = Helper.CreateConsignment("LT0001");
			AssertEquals("LT0001", consignment.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion
		#endregion
		#region Shipments
		#region TestShipmentDataObjectWriter
		public void TestShipmentDataObjectWriter()
		{
			IShipmentDataContextManager manager = new DtbConsignmentDataContextManager();
			var dataWritingManager = new DataWritingManager(new DummyActionInfo());
			AssertEquals(typeof(DtbConsignmentDataObjectWriter), manager.GetShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetType());
		}

		#endregion
		#region TestManagesShipments
		public void TestManagesShipments()
		{
			AssertEquals(true, new DtbConsignmentDataContextManager().ManagesShipments);
		}

		#endregion
		#endregion
		#region TestEventContextValues
		public void TestEventContextValues()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "TB001";
			var consignment = Helper.CreateConsignment("LT001");
			consignment.LTC_KM_Booking = booking.PK;
			var manager = consignment.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValuesStringContents = manager.EventContextValues.ToStringContents(o => o.Key + " - " + o.Value);
			AssertEquals("Should have no event context values, as GetEventContextValues returns an empty enumerable", string.Empty, eventContextValuesStringContents);
		}

		public void TestEventContextValues_EmptyLTC_KM_Booking()
		{
			var consignment = Helper.CreateConsignment("LT001");
			AssertEquals("Precondition: LTC_KM_Booking is Empty", ZGuid.Empty, consignment.LTC_KM_Booking);
			var manager = consignment.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValuesStringContents = (string)null;
			AssertNoExceptionThrown("Should not have exception when LTC_KM_Booking is empty", () =>
			{
				eventContextValuesStringContents = manager.EventContextValues.ToStringContents(o => o.Key + " - " + o.Value);
			});
			AssertEquals("Should indicate lack of event context values", string.Empty, eventContextValuesStringContents);
		}

		#endregion
		#region TestDefaultOutputDirectory
		public void TestDefaultOutputDirectory()
		{
			AssertNull(new DtbConsignmentDataContextManager().DefaultOutputDirectory);
		}

		#endregion
		#region TestGetDataContextKeyMatchingQuery
		public void TestGetDataContextKeyMatchingQuery()
		{
			var consignment = Helper.CreateConsignment("LT0001");
			Factory.SaveForTesting();
			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(DataContextType.LandTransportConsignment, consignment.LTC_JobID);
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.ContextCollection = new List<Context> { new Context { Type = nameof(UniversalEventDataObject.ContextTypes.QuoteNumber), Value = "RandomValue" } };
			eventDataObject.EventType = Events.AuthorisedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			AssertEquals(string.Format("Linked Event to {0}.", consignment.HumanReadableName), importResults.Single().ToString());
			AssertEquals(1, consignment.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		#endregion
		#region TestShipmentDataObjectReader
		public void TestShipmentDataObjectReader()
		{
			var consignment = Helper.CreateConsignment("LT0001");
			Factory.SaveForTesting();
			var shipmentForTransport = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentForTransport.DataContext = DataContextFactory.New();
			shipmentForTransport.DataContext.AddDataTarget(DataContextType.LandTransportConsignment, "LT0001");
			shipmentForTransport.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var message = GetQueuedUniversalShipmentMessage(shipmentForTransport);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var results = manager.Process(message).ImportResults;
			var importResult = results.Single(o => o.DataContextType == DataContextType.LandTransportConsignment);
			var importedTransport = (DtbConsignment)importResult.GetBizOForTesting(shipmentForTransport, new BusinessObjectFactory());
			AssertEquals(true, importResult.WasSuccessful);
			AssertEquals(consignment.PK, importedTransport.PK);
		}

		#endregion
		#region TestUniversalShipmentUpdateTransport
		// This test is Billing related, which is not implemented yet in consignment
		/*
		public void TestUniversalShipmentUpdateTransport()
		{
			var debtor = Helper.CreateOrganisation("Org1");
			debtor.OH_IsDebtor = true;

			var consignment = Helper.CreateConsignment("LT0001");
			new JobHeader.Loader(consignment).LoadOrCreate();
			Factory.SaveForTesting();

			AssertNotNull("Precondition", consignment.Job);

			var shipmentForTransport = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentForTransport.DataContext = DataContextFactory.New();
			shipmentForTransport.DataContext.AddDataTarget(DataContextType.LandTransportConsignment, "LT0001");
			shipmentForTransport.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			AddJobCosting(shipmentForTransport);

			var message = GetQueuedUniversalShipmentMessage(shipmentForTransport);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var results = manager.Process(message).ImportResults;
			var importResult = results.Single(o => o.DataContextType == DataContextType.LandTransportConsignment);
			var importedTransport = (DtbConsignment)importResult.GetBizOForTesting(shipmentForTransport, new BusinessObjectFactory());
			AssertEquals(true, importResult.WasSuccessful);
			AssertEquals(consignment.PK, importedTransport.PK);

			var charge = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, importedTransport.Job.PK)).Single();
			AssertEquals(debtor.PK, charge.JR_OH_SellAccount);
			AssertEquals("OCART", charge.ChargeCode.AC_Code);
			AssertEquals(1500m, charge.JR_LocalSellAmt);
			AssertEquals("AUD", charge.JR_RX_NKCostCurrency);
		}

		void AddJobCosting(UniversalShipment shipment)
		{
			shipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.JobCosting.ChargeLineCollection = new List<ChargeLine>();
			var chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ChargeCode = new ChargeCode { Code = "OCART" },
				CostLocalAmount = 1400m,
				CostOSAmount = 1500m,
				CostOSCurrency = new Currency { Code = "AUD" },
				Debtor = new OrganizationReference() { Type = "Organization", Key = "Org1" },

				ImportMetaData = new ImportMetaData()
				{
					Instruction = InstructionType.UpdateAndInsertIfNotFound,
					MatchingCriteriaCollection = new List<MatchingCriteria>()
					{
						new MatchingCriteria(){ FieldName = "ChargeCode", Value = "OCART" }
					}
				}
			};

			shipment.JobCosting.ChargeLineCollection.Add(chargeLine);
		}
		*/
		#endregion
		#region Helper
		TransportConsignmentTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new TransportConsignmentTestHelper(Factory.BOFactory));
			}
		}

		TransportConsignmentTestHelper helper;
		#endregion
		#region Implementation
		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get
			{
				return System.Array.Empty<RecipientRoleType>();
			}
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return "<UniversalShipment></UniversalShipment>";
			}
		}
		#endregion
	}
}
