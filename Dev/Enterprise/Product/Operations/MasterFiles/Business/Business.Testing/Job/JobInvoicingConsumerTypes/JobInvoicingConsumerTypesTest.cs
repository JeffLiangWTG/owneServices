using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class JobInvoicingConsumerTypesTest : TestCase
	{
		public void TestList()
		{
			var list = JobInvoicingConsumerTypes.New();
			var consumerTypePairs = list.GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).Select(field => field.GetValue(this)).OfType<JobInvoicingConsumerType>();

			foreach (var pair in consumerTypePairs)
			{
				if (NonVisibleJobTypes.Contains(pair.Code))
				{
					AssertEquals("This item should not be in the list - " + pair.Code, false, list.Contains(pair));
				}
				else
				{
					AssertEquals("This item should be in the list - " + pair.Code, true, list.Contains(pair));
				}
			}

			Assert("At least 1 item must exist", list.Count > 0);
		}

		protected virtual List<string> NonVisibleJobTypes
		{
			get
			{
				var result = new List<string>
				{
					JobInvoicingConsumerTypes.Consol.Code,
					JobInvoicingConsumerTypes.OneOffQuotation.Code
				};

				return result;
			}
		}

		public void TestOverseasAgentApplicability()
		{
			var list = JobInvoicingConsumerTypes.New();
			var consumerTypePairs = list.GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).Select(field => field.GetValue(this)).OfType<JobInvoicingConsumerType>();

			foreach (var pair in consumerTypePairs)
			{
				if (OverseasAgentApplicableJobTypes.Contains(pair.Code))
				{
					AssertEquals("This item should have the Overseas Agent marked as applicable - " + pair.Code, true, pair.OverseasAgentApplicable);
				}
				else
				{
					AssertEquals("This item should NOT have the Overseas Agent marked as applicable - " + pair.Code, false, pair.OverseasAgentApplicable);
				}
			}

			Assert("At least 1 item must exist", list.Count > 0);
		}

		public void TestNewOnlyJobInvoicingTypes()
		{
			var fullList = JobInvoicingConsumerTypes.New();
			var jobOnlyList = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();

			AssertEquals(fullList.Count - 1, jobOnlyList.Count);
			Assert(fullList.ContainsCode("ORG"));
			Assert(!jobOnlyList.ContainsCode("ORG"));
		}

		static List<string> OverseasAgentApplicableJobTypes
		{
			get
			{
				var result = new List<string>
				{
					JobInvoicingConsumerTypes.Consol.Code,
					JobInvoicingConsumerTypes.ForwardingConsol.Code,
					JobInvoicingConsumerTypes.GatewayConsol.Code,
					JobInvoicingConsumerTypes.Shipment.Code,
					JobInvoicingConsumerTypes.Brokerage.Code,
					JobInvoicingConsumerTypes.QuotedBooking.Code
				};

				return result;
			}
		}

		public void TestConstantsMatchGetters()
		{
			var typesList = JobInvoicingConsumerTypes.New();
			var constantsList = ListOfConstantsInOrder;

			AssertEquals("The number of constants should equal the number of types", constantsList.Count, typesList.Count);
			foreach (JobInvoicingConsumerType type in typesList)
			{
				AssertCollectionContains("Every code type in JobConsumerTypes should be declared as a constant. No match for : " + type.Code, type.Code, ListOfConstantsInOrder);
			}
		}

		static List<string> ListOfConstantsInOrder
		{
			get
			{
				var result = new List<string>
				{
					JobInvoicingConsumerTypes.ShipmentCode,
					JobInvoicingConsumerTypes.BrokerageCode,
					JobInvoicingConsumerTypes.PostClearanceBrokerageCode,
					JobInvoicingConsumerTypes.AgencyBillOfLadingCode,
					JobInvoicingConsumerTypes.QuotedBookingCode,
					JobInvoicingConsumerTypes.ForwardingConsolCode,
					JobInvoicingConsumerTypes.GatewayConsolCode,
					JobInvoicingConsumerTypes.MasterAWBCode,
					JobInvoicingConsumerTypes.CFSShipmentCode,
					JobInvoicingConsumerTypes.CFSLoadListCode,
					JobInvoicingConsumerTypes.FCLStorageCode,
					JobInvoicingConsumerTypes.LocalCartageCode,
					JobInvoicingConsumerTypes.WarehouseInwardsCode,
					JobInvoicingConsumerTypes.WarehouseOutwardsCode,
					JobInvoicingConsumerTypes.WarehouseStorageCode,
					JobInvoicingConsumerTypes.WarehouseStocktakeCode,
					JobInvoicingConsumerTypes.WarehouseAdHocServiceJobCode,
					JobInvoicingConsumerTypes.WarehouseVASOrderCode,
					JobInvoicingConsumerTypes.CusMAWBCode,
					JobInvoicingConsumerTypes.CusUnderbondCode,
					JobInvoicingConsumerTypes.CTOCusMAWBCode,
					JobInvoicingConsumerTypes.CTOCusImportHAWBCode,
					JobInvoicingConsumerTypes.CTOCusExportHAWBCode,
					JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode,
					JobInvoicingConsumerTypes.AgencyVoyageAccountingCode,
					JobInvoicingConsumerTypes.AgencySundryChargesCode,
					JobInvoicingConsumerTypes.AgencyBookingCode,
					JobInvoicingConsumerTypes.AgentBookingCode,
					JobInvoicingConsumerTypes.TransportBookingCode,
					JobInvoicingConsumerTypes.TransportBookingConsignmentCode,
					JobInvoicingConsumerTypes.TransportBookingWithAgentCode,
					JobInvoicingConsumerTypes.TransportConsignmentCode,
					JobInvoicingConsumerTypes.ImporterSecurityFilingCode,
					JobInvoicingConsumerTypes.OrganisationCode,
					JobInvoicingConsumerTypes.eManifestCode,
					JobInvoicingConsumerTypes.CAeManifestCode,
					JobInvoicingConsumerTypes.WorkItemCode,
					JobInvoicingConsumerTypes.ProjectCode,
					JobInvoicingConsumerTypes.WorkRequestCode,
					JobInvoicingConsumerTypes.CYDReceiveAdviceJobCode,
					JobInvoicingConsumerTypes.CYDReleaseAdviceJobCode,
					JobInvoicingConsumerTypes.CYDTransportationUnitJobCode,
					JobInvoicingConsumerTypes.CYDAdHocServiceOrderJobCode,
					JobInvoicingConsumerTypes.MNRWorkOrderHeaderJobCode,
					JobInvoicingConsumerTypes.CYDPeriodicInvoicingJobCode,
					JobInvoicingConsumerTypes.TransitReceiveCode,
					JobInvoicingConsumerTypes.TransitReceiveTransportationUnitCode,
					JobInvoicingConsumerTypes.TransitDispatchCode,
					JobInvoicingConsumerTypes.TransitDispatchLoadListCode,
					JobInvoicingConsumerTypes.TransitDispatchTransportationUnitCode,
					JobInvoicingConsumerTypes.CustomsTransitNCTSCode,
					JobInvoicingConsumerTypes.BRLPCOCode,
					JobInvoicingConsumerTypes.CustomsTemporaryStorageCode,
				};

				return result;
			}
		}
	}
}
