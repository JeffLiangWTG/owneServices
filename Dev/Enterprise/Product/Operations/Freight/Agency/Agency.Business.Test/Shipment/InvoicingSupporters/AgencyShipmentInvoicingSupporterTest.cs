using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Testing
{
	[TestedType(typeof(AgencyShipmentInvoicingSupporter))]
	internal sealed class AgencyShipmentInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestHouseBillNumber()
		{
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "Principal";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = principal.PK;

			IJobInvoicingPlugIn invoice = shipment;
			shipment.JS_HouseBill = "TEST12345";

			AssertEquals("HouseBillNumber should be empty", "", invoice.InvoicingSupporter.HouseBillNumber);
		}

		public void TestMasterBillNumber()
		{
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "Principal";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = principal.PK;

			IJobInvoicingPlugIn invoice = shipment;
			shipment.JS_HouseBill = "TEST12345";

			AssertEquals("HouseBillNumber should come from JS_HouseBill", "TEST12345", invoice.InvoicingSupporter.MasterBillNumber);
		}

		public void TestGetDefaultCreditor()
		{
			AccChargeCode pChargeCode = Factory.New<AccChargeCode>();
			pChargeCode.AC_Code = "_PC";
			pChargeCode.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal;

			AccChargeCode aChargeCode = Factory.New<AccChargeCode>();
			aChargeCode.AC_Code = "_AC";
			aChargeCode.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Agent;

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "Principal";

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = principal.PK;

			IJobInvoicingPlugIn invoice = shipment;

			AgencyRegistry.Instance.DefaultCreditorFromPrincipal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			AssertEquals(principal, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, AgencyInvoiceTypesList.Codes.ForeignCollect)));
			AssertEquals(principal, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, AgencyInvoiceTypesList.Codes.ForeignPrePaid)));
			AssertEquals(principal, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, AgencyInvoiceTypesList.Codes.LocalCollect)));
			AssertEquals(principal, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, AgencyInvoiceTypesList.Codes.LocalPrePaid)));
			AssertEquals(null, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(aChargeCode, AgencyInvoiceTypesList.Codes.ForeignCollect)));
			AssertEquals(null, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(aChargeCode, AgencyInvoiceTypesList.Codes.ForeignPrePaid)));
			AssertEquals(null, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(aChargeCode, AgencyInvoiceTypesList.Codes.LocalCollect)));
			AssertEquals(null, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(aChargeCode, AgencyInvoiceTypesList.Codes.LocalPrePaid)));

			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUBNE";

			AssertEquals(principal, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, AgencyInvoiceTypesList.Codes.ForeignCollect)));
			AssertEquals(principal, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, AgencyInvoiceTypesList.Codes.ForeignPrePaid)));
			AssertEquals(principal, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, AgencyInvoiceTypesList.Codes.LocalCollect)));
			AssertEquals(principal, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, AgencyInvoiceTypesList.Codes.LocalPrePaid)));
			AssertEquals(null, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(aChargeCode, AgencyInvoiceTypesList.Codes.ForeignCollect)));
			AssertEquals(null, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(aChargeCode, AgencyInvoiceTypesList.Codes.ForeignPrePaid)));
			AssertEquals(null, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(aChargeCode, AgencyInvoiceTypesList.Codes.LocalCollect)));
			AssertEquals(null, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(aChargeCode, AgencyInvoiceTypesList.Codes.LocalPrePaid)));

			AgencyRegistry.Instance.DefaultCreditorFromPrincipal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertEquals(null, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, AgencyInvoiceTypesList.Codes.ForeignCollect)));
			AssertEquals(null, invoice.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(pChargeCode, AgencyInvoiceTypesList.Codes.LocalCollect)));
		}

		public void TestCreateAccountingJobOnSavingOfOperationsJob()
		{
			IJobInvoicingPlugIn plugin = Factory.New<AgencyShipment>();
			AssertEquals(true, plugin.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(It.IsAny<BusinessObject>())).Returns(false);

			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals(false, plugin.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
			}

			mock.VerifyAll();
		}

		public void TestIncludeOnApportionmentsAlwaysReturnsTrue()
		{
			IJobInvoicingPlugIn invoiceable = Factory.New<AgencyShipment>();
			Assert(invoiceable.InvoicingSupporter.IncludeInConsolCosting(false));
			Assert(invoiceable.InvoicingSupporter.IncludeInConsolCosting(true));
		}

		public void TestInvoiceAndRatingConsumerTypes()
		{
			IJobInvoicingPlugIn invoiceableBooking = Factory.New<AgencyBooking>();
			AssertNotNull(invoiceableBooking.InvoicingSupporter.ConsumerType);
			AssertEquals(JobInvoicingConsumerTypes.AgencyBooking.Code, invoiceableBooking.InvoicingSupporter.ConsumerType.Code);

			IJobInvoicingPlugIn invoiceableBillOfLading = Factory.New<BillOfLading>();
			AssertNotNull(invoiceableBillOfLading.InvoicingSupporter.ConsumerType);
			AssertEquals(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, invoiceableBillOfLading.InvoicingSupporter.ConsumerType.Code);
		}

		public void TestArrivalAtLoadPort()
		{
			ZDateTime today = ZDateTime.Today;
			ZDateTime sailing_E_ATL = today.AddDays(20);
			ZDateTime sailing_A_ATL = today.AddDays(21);

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "12";
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("APL IVORY", Factory).First().RV_FK;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

			var sailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			AssertNotNull("Sailing should exist", sailing);

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var supporter = shipment.InvoicingSupporter;
			AssertEquals("Shipment has NO Sailing", null, shipment.Sailing);
			AssertEquals("ATL: Shipment with NO Sailing - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);

			shipment.JS_JX = sailing.PK;
			AssertEquals("Shipment has Sailing", sailing, shipment.Sailing);
			AssertEquals("Sailing A_ATL: Empty", ZDateTime.Empty, shipment.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: Empty", ZDateTime.Empty, shipment.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL and E_ATL are Empty - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);

			sailing.Origin.JA_E_ARV = sailing_E_ATL;
			AssertEquals("Sailing A_ATL: Empty", ZDateTime.Empty, shipment.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing_E_ATL, shipment.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL is Empty - Sailing E_ATL", sailing_E_ATL, supporter.ArrivalAtLoadPort);

			sailing.Origin.JA_A_ARV = sailing_A_ATL;
			AssertEquals("Sailing A_ATL: sailing_A_ATL", sailing_A_ATL, shipment.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing_E_ATL, shipment.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL", sailing_A_ATL, supporter.ArrivalAtLoadPort);
		}

		#region Implementation

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AgencyShipment>();
		}

		#endregion
	}
}
