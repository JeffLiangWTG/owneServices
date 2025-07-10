using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business.Testing
{
	partial class VoyageAccountTest
	{
		public void TestExRateSourceProvider()
		{
			AssertContainsExactElementsInAnyOrder(
				"Should support Voyage",
				new ExRateSourceType[] { ExRateSourceType.Voyage },
				SupportExRateSourceAttribute.SupportedExRateSources(typeof(VoyageAccount)));

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			IJobInvoicingExRateSourceProvider provider = Account1;
			Account1.NA_JV = voyage.PK;

			AssertEquals("(ExRateSourceType)(-1)", null, provider.GetExRateSource((ExRateSourceType)(-1)));
			AssertEquals("ExRateSourceType.Voyage", voyage, provider.GetExRateSource(ExRateSourceType.Voyage));
		}

		public void TestExRatesSourceReturnsExchangeRatesOfTheCurrentCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = company.PK;

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();
			Factory.Save();

			var va1 = CreateExRateAndVoyageAccounting(GlbBranch.CurrentBranch, 0.75M);
			var va2 = CreateExRateAndVoyageAccounting(newBranch, 0.58M);

			var vygFactory = new BusinessObjectFactory();
			var voyageInNewFacotry = vygFactory.Load<JobVoyage>(voyage.PK);
			AssertVoyageExRate(vygFactory, GlbBranch.CurrentBranch, va1, voyageInNewFacotry, 0.75M);
			AssertVoyageExRate(vygFactory, newBranch, va2, voyageInNewFacotry, 0.58M);

			ZGuid CreateExRateAndVoyageAccounting(GlbBranch branch, ZDecimal rate)
			{
				using (new TemporaryUserContext() { BranchPK = branch.PK.ToGuid(), DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set())
				{
					var vaFactory = new BusinessObjectFactory();
					var voyageReloaded = vaFactory.Load<JobVoyage>(voyage.PK);

					var exRate1 = voyageReloaded.ExRates.AddNew();
					exRate1.E8_RX_NKExCurrency = "USD";
					exRate1.E8_VoyageExchangeRate = rate;

					var voyageAccounting1 = vaFactory.NewWithValidTestData<VoyageAccount>();
					voyageAccounting1.NA_JV = voyage.PK;
					voyageAccounting1.Header.OH_Code = branch.GB_Code;
					voyageAccounting1.NA_GC = branch.GB_GC;

					vaFactory.Save();
					return voyageAccounting1.PK;
				}
			}

			void AssertVoyageExRate(BusinessObjectFactory factory, GlbBranch branch, ZGuid vaPK, JobVoyage expectedVoyage, ZDecimal expectedRate)
			{
				using (new TemporaryUserContext() { BranchPK = branch.PK.ToGuid(), DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set())
				{
					var vaReloaded = factory.Load<VoyageAccount>(vaPK);
					AssertEquals("(ExRateSourceType)(-1)", null, vaReloaded.GetExRateSource((ExRateSourceType)(-1)));
					AssertEquals("ExRateSourceType.Voyage", expectedVoyage, vaReloaded.GetExRateSource(ExRateSourceType.Voyage));
					AssertEquals("ExRateSourceType.Voyage.ExRate", expectedRate, vaReloaded.GetExRateSource(ExRateSourceType.Voyage).GetExchangeRate("USD", ZGuid.Empty, ExchangeRateValidLedgerEnum.None));
				}
			}
		}

		#region IJobInvoicingPlugIn Members

		public void TestInvoicingEmptyMembers()
		{
			IJobInvoicingPlugIn invoicing = Account1;

			AssertEquals("ActualChargeable", 0m, invoicing.InvoicingSupporter.ActualChargeable);
			AssertEquals("ActualChargeableUnit", "", invoicing.InvoicingSupporter.ActualChargeableUnit);
			AssertEquals("ActualVolume", 0m, invoicing.InvoicingSupporter.ActualVolume);
			AssertEquals("ActualVolumeUnit", "", invoicing.InvoicingSupporter.ActualVolumeUnit);
			AssertEquals("ActualWeight", 0m, invoicing.InvoicingSupporter.ActualWeight);
			AssertEquals("ActualWeightUnit", "", invoicing.InvoicingSupporter.ActualWeightUnit);
			AssertEquals("Consignee", null, invoicing.InvoicingSupporter.Consignee);
			AssertEquals("Consignor", null, invoicing.InvoicingSupporter.Consignor);
			AssertEquals("ConsolExchangeRate", 0m, invoicing.InvoicingSupporter.ConsolExchangeRate);
			AssertEquals("ConsolRateCurrency", null, invoicing.InvoicingSupporter.ConsolRateCurrency);
			AssertEquals("ContainerMode", "", invoicing.InvoicingSupporter.ContainerMode);
			AssertEquals("Destination", null, invoicing.InvoicingSupporter.Destination);
			AssertEquals("ETA", ZDateTime.Empty, invoicing.InvoicingSupporter.ETA);
			AssertEquals("ETD", ZDateTime.Empty, invoicing.InvoicingSupporter.ETD);
			AssertEquals("EditSecurityLock", false, invoicing.InvoicingSupporter.EditSecurityLock);
			AssertEquals("EditSecurityMessage", "", invoicing.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("HouseBillNumber", "", invoicing.InvoicingSupporter.HouseBillNumber);
			AssertEquals("PaymentTerm", null, invoicing.InvoicingSupporter.PaymentTerm);
			AssertEquals("IsDirectShipment", false, invoicing.InvoicingSupporter.IsDirectShipment);
			AssertEquals("IsDomestic", false, invoicing.InvoicingSupporter.IsDomestic);
			AssertEquals("IsExport", false, invoicing.InvoicingSupporter.IsExport);
			AssertEquals("IsImport", false, invoicing.InvoicingSupporter.IsImport);
			AssertEquals("IsPlugInReadOnly", false, invoicing.InvoicingSupporter.IsPlugInReadOnly);
			AssertEquals("MasterBillNumber", "", invoicing.InvoicingSupporter.MasterBillNumber);
			AssertEquals("GetOperationsSignificantDate", ZDateTime.Empty, invoicing.InvoicingSupporter.GetOperationsSignificantDate(""));
			AssertEquals("Origin", null, invoicing.InvoicingSupporter.Origin);
			AssertEquals("OverriddenDepartmentPK", ZGuid.Empty, invoicing.InvoicingSupporter.OverriddenDepartmentPK);
			AssertEquals("ReceivingAgent", null, invoicing.InvoicingSupporter.ReceivingAgent);
			AssertEquals("SendingAgent", null, invoicing.InvoicingSupporter.SendingAgent);
			AssertEquals("ShipmentNumberOfColoadMaster", "", invoicing.InvoicingSupporter.ShipmentNumberOfColoadMaster);
			AssertEquals("TranshipmentPort", null, invoicing.InvoicingSupporter.GetTranshipmentPort(CostSell.Cost));
			AssertEquals("TranshipmentPort", null, invoicing.InvoicingSupporter.GetTranshipmentPort(CostSell.Revenue));
			AssertEquals("DefaultCreditor", null, invoicing.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(null, null)));
		}

		public void TestInvoicingMeaningfulMembers()
		{
			Account1.NA_JobNumber = "VA00000999";
			IJobInvoicingPlugIn invoicing = Account1;

			AssertEquals("OperationsBranch", GlbBranch.CurrentBranch, invoicing.InvoicingSupporter.OperationsBranch);
			AssertEquals("TransportMode", Constants.TransportModes.Sea, invoicing.InvoicingSupporter.TransportMode);
			AssertEquals("ConsumerType", JobInvoicingConsumerTypes.AgencyVoyageAccounting, invoicing.InvoicingSupporter.ConsumerType);
			AssertEquals("JobInvoicingSecurity", Env.Security.AgencyVoyageAccountJobInvoicing, invoicing.InvoicingSupporter.JobInvoicingSecurity);
			AssertEquals("AuditSecurity", Env.Security.AgencyVoyageAccountAuditBilling, invoicing.InvoicingSupporter.AuditSecurity);
			AssertEquals("EditSecurityCheckpoint", Env.Security.None, invoicing.InvoicingSupporter.EditSecurityCheckpoint);

			AssertEquals("JobNumber", "VA00000999", invoicing.JobNumber);
		}

		public void TestIJobInvoicingPlugIn_DefaultCreditor()
		{
			IJobInvoicingPlugIn invoicing = Factory.NewWithValidTestData<VoyageAccount>();
			AssertEquals("DefaultCreditor", null, invoicing.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(null, null)));
		}

		public void TestIJobInvoicingPlugIn_DefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<VoyageAccount>();
			AssertEquals("DefaultChargeGroup should be 'SDS'", "SDS", testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent containerDetention = Factory.New<SundryCharges>();
			Assert(containerDetention.AllowInvoiceDeletion);
		}

		#endregion
	}
}
