using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Events = Enterprise.ZArchitecture.Business.Events;

namespace Enterprise.Freight.DataTransfer.Test.Freight.Universal.Reading
{
	public class AviationSecurityAddtionalInspectionTypeDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region Shipment

		public void TestShipmentAviationSecurityInspectionTypeDataObjectReader()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var inspectionType = new CodeDescriptionPair { Code = "UNK", Description = "Unknown" };
			var reader = new AviationSecurityAdditionalInspectionTypeDataObjectReader(inspectionType, logger, Factory, shipmentBO, shipmentBO.Logs, shipmentBO.IsInDatabase);
			var cusEntryNumber = reader.ReadIntoBusinessObject();

			AssertEquals("UNK", cusEntryNumber.CE_EntryNum);

			inspectionType.Code = "XRY";
			inspectionType.Description = "XRay Equipment";
			cusEntryNumber = reader.ReadIntoBusinessObject();

			AssertEquals(CusEntryNumber.EntryType.AdditionalInspectionStatus, cusEntryNumber.CE_EntryType);
			AssertEquals("XRY", cusEntryNumber.CE_EntryNum);
		}

		public void TestShipmentLogInspectionTypeCodeChanged()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentBO.JS_RL_NKOrigin = "AUSYD";
			shipmentBO.JS_RL_NKDestination = "CNSHA";
			shipmentBO.JS_IsHighRisk = true;
			shipmentBO.JS_AdditionalInspectionTypeCode = "MAI";
			Factory.SaveForTesting();

			((ISupportDataImporting)shipmentBO).IsImportingData = true;
			var inspectionType = new CodeDescriptionPair { Code = "XRY", Description = "XRay Equipment" };
			var reader = new AviationSecurityAdditionalInspectionTypeDataObjectReader(inspectionType, logger, Factory, shipmentBO, shipmentBO.Logs, shipmentBO.IsInDatabase, "Shipment Additional Inspection Type Changed by Data Import");
			reader.ReadIntoBusinessObject();

			var secEvent = shipmentBO.Logs.MostRecentLogByEventTime(Events.SecurityModified);
			AssertNotNull(secEvent);
			AssertEquals("|NEW=XRY|OLD=MAI|RES=Shipment Additional Inspection Type Changed by Data Import|TYP=Additional Inspection", secEvent.SL_Reference);
		}

		#endregion

		#region PackLine
		public void TestPackLineAviationSecurityInspectionTypeDataObjectReader()
		{
			var packLineBO = Factory.NewWithValidTestData<PackLine>();
			var inspectionType = new CodeDescriptionPair { Code = "UNK", Description = "Unknown" };
			var reader = new AviationSecurityAdditionalInspectionTypeDataObjectReader(inspectionType, logger, Factory, packLineBO, packLineBO.Logs, packLineBO.IsInDatabase);
			var cusEntryNumber = reader.ReadIntoBusinessObject();

			AssertEquals("UNK", cusEntryNumber.CE_EntryNum);

			inspectionType.Code = "XRY";
			inspectionType.Description = "XRay Equipment";
			cusEntryNumber = reader.ReadIntoBusinessObject();

			AssertEquals(CusEntryNumber.EntryType.AdditionalInspectionStatus, cusEntryNumber.CE_EntryType);
			AssertEquals("XRY", cusEntryNumber.CE_EntryNum);
		}

		public void TestPackLineLogInspectionTypeCodeChanged()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentBO.JS_RL_NKOrigin = "AUSYD";
			shipmentBO.JS_RL_NKDestination = "CNSHA";
			shipmentBO.JS_IsHighRisk = true;
			shipmentBO.JS_AdditionalInspectionTypeCode = "MAI";

			var packLineBO = shipmentBO.OuterPackLines.AddNew();
			packLineBO.JL_IsHighRisk = true;
			packLineBO.JL_AdditionalInspectionTypeCode = "MAI";
			Factory.SaveForTesting();

			((ISupportDataImporting)shipmentBO).IsImportingData = true;
			var inspectionType = new CodeDescriptionPair { Code = "XRY", Description = "XRay Equipment" };
			var reader = new AviationSecurityAdditionalInspectionTypeDataObjectReader(inspectionType, logger, Factory, packLineBO, packLineBO.Shipment.Logs, packLineBO.IsInDatabase, "Packline Additional Inspection Type Changed by Data Import");
			reader.ReadIntoBusinessObject();

			var secEvent = packLineBO.Shipment.Logs.MostRecentLogByEventTime(Events.SecurityModified);
			AssertNotNull(secEvent);
			AssertEquals("|NEW=XRY|OLD=MAI|RES=Packline Additional Inspection Type Changed by Data Import|TYP=Additional Inspection", secEvent.SL_Reference);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		#endregion
	}
}
