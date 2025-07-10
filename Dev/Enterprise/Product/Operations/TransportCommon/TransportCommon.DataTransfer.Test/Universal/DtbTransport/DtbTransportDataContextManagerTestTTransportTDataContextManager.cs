using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Testing
{
	public abstract class DtbTransportDataContextManagerTest<TTransport, TDataContextManager> : ShipmentDataContextManagerTestCase<TDataContextManager, TTransport>
			where TTransport : DtbTransport
			where TDataContextManager : DtbTransportDataContextManager<TTransport>, new()
	{
		#region Context

		#region TestDataContextType

		public void TestDataContextType()
		{
			AssertEquals(ExpectedDataContextType, new TDataContextManager().DataContextType);
		}

		protected abstract DataContextType ExpectedDataContextType { get; }

		#endregion

		#region TestDataContextKey

		public void TestDataContextKey()
		{
			var transport = Factory.New<TTransport>();
			transport.KM_JobID = "CN00001";
			AssertEquals("CN00001", transport.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion

		#region TestGetDataContextKeyMatchingQuery

		public void TestGetDataContextKeyMatchingQuery()
		{
			var consolidation = GetNewConsolidation();
			var transport = (TTransport)consolidation.Bookings.AddNew();
			transport.KM_JobID = "CN00001";
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(ExpectedDataContextType, transport.KM_JobID);
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.ContextCollection = new List<Context> { new Context { Type = nameof(UniversalEvent.ContextTypes.QuoteNumber), Value = "RandomValue" } };
			eventDataObject.EventType = Events.AuthorisedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;

			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			AssertEquals(string.Format("Linked Event to {0}.", transport.HumanReadableName), importResults.Single().ToString());
			AssertEquals(1, transport.Logs.Find(l => l.SL_SE_NKEvent == Events.AuthorisedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		#endregion

		#region TestDefaultOutputDirectory

		public void TestDefaultOutputDirectory()
		{
			AssertNull(new TDataContextManager().DefaultOutputDirectory);
		}

		#endregion

		#endregion

		#region Shipments

		#region TestManagesShipments

		public void TestManagesShipments()
		{
			AssertEquals(true, new TDataContextManager().ManagesShipments);
		}

		#endregion

		#region TestShipmentDataObjectWriter

		public void TestShipmentDataObjectWriter()
		{
			IShipmentDataContextManager manager = new TDataContextManager();
			AssertEquals(ExpectedShipmentDataObjectWriterType, manager.GetShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetType());
		}

		protected abstract Type ExpectedShipmentDataObjectWriterType { get; }

		#endregion

		#region TestUniversalShipmentUpdateTransport

		#region TestUniversalShipmentUpdateTransport_NonOverridenTranportWithCharges

		public void TestUniversalShipmentUpdateTransport_NonOverridenTranportWithCharges()
		{
			AssertUniversalShipmentUpdateTransport(false);
		}

		#endregion

		#region TestUniversalShipmentUpdateTransport_OverridenTranportWithCharges

		public void TestUniversalShipmentUpdateTransport_OverridenTranportWithCharges()
		{
			AssertUniversalShipmentUpdateTransport(true);
		}

		#endregion

		void AssertUniversalShipmentUpdateTransport(bool isOverridden)
		{
			var debtor = Helper.CreateOrganisation("Org1");
			debtor.OH_IsDebtor = true;
			var creditor = Helper.CreateOrganisation("Org2");
			creditor.OH_IsCreditor = true;

			var rate = Factory.New<RefAccTaxRate>();
			rate.ZAT_StartDate = ZDate.Today.AddDays(-100);
			rate.ZAT_EndDate = ZDate.Today.AddDays(100);
			rate.ZAT_ReferenceRateType = "STD";
			rate.ZAT_RN_NKCountry = "AU";

			var transport = GetTransport();
			transport.KM_JobID = "TB0001";
			transport.ConsolidationSingleJob.KB_IsOverridden = isOverridden;
			new JobHeader.Loader(transport).TryLoadOrCreate();

			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "OCART"));
			code.AC_DepartmentFilterList = "ALL";
			Factory.SaveForTesting();

			AssertNotNull("Precondition", transport.Job);

			var shipmentForTransport = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentForTransport.DataContext = DataContextFactory.New();
			shipmentForTransport.DataContext.AddDataTarget(ExpectedDataContextType, "TB0001");
			shipmentForTransport.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			AddJobCosting(shipmentForTransport);

			var message = GetQueuedUniversalShipmentMessage(shipmentForTransport);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var results = manager.Process(message).ImportResults;
			var importResult = results.Single(o => o.DataContextType == ExpectedDataContextType);
			var importedTransport = (DtbTransport)importResult.GetBizOForTesting(shipmentForTransport, new BusinessObjectFactory());
			AssertEquals(true, importResult.WasSuccessful);
			AssertEquals(transport.PK, importedTransport.PK);

			var charge = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, importedTransport.Job.PK)).Single();
			AssertEquals(debtor.PK, charge.JR_OH_SellAccount);
			AssertEquals("OCART", charge.ChargeCode.AC_Code);
			AssertEquals(1500m, charge.JR_LocalSellAmt);
			AssertEquals("AUD", charge.JR_RX_NKCostCurrency);
		}

		void AddJobCosting(UniversalShipment shipment)
		{
			shipment.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.JobCosting.SetChargeLineCollection(() =>
			{
				var chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ChargeCode = new ChargeCode { Code = "OCART" },
					CostLocalAmount = 1400m,
					CostOSAmount = 1500m,
					CostOSCurrency = new Currency { Code = "AUD" },
					Debtor = new OrganizationReference() { Type = "Organization", Key = "Org1" },
					Creditor = new OrganizationReference() { Type = "Organization", Key = "Org2" },
					Description = "My feelings, your skateboard",

					ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Instruction = InstructionType.UpdateAndInsertIfNotFound,
					}
				};

				chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
				{
					new MatchingCriteria() { FieldName = "ChargeCode", Value = "OCART" }
				});

				return new List<ChargeLine> { chargeLine };
			});
		}

		protected abstract DtbTransport GetTransport();

		#endregion

		#endregion

		#region Implementation

		protected sealed override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return Array.Empty<RecipientRoleType>(); }
		}

		protected abstract DtbTransportConsolidation GetNewConsolidation();

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get { return "<UniversalShipment></UniversalShipment>"; }
		}

		protected TransportCommonTestHelper Helper
		{
			get { return helper ?? (helper = new TransportCommonTestHelper(Factory.BOFactory)); }
		}
		TransportCommonTestHelper helper;

		#endregion
	}
}
