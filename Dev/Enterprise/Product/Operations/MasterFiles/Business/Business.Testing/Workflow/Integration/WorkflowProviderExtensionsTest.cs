using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowProviderExtensionsTest : TestCaseWithFactory
	{
		public void TestGetTemplateSelectionCriteria()
		{
			BusinessObject shipment = (BusinessObject)Factory.New<Freight.Integration.CFS.ICFSShipment>();
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.Parent = (IJobHeaderParent)shipment;
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "INBOM";
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			job.JH_OA_LocalChargesAddr = org.MainAddress.PK;

			ColumnValueRanker ranker = ((IJobInvoicingPlugIn)shipment).GetJobRelatedTemplateSelectionCriteria();
			AssertEquals(GlbBranch.CurrentBranch.PK, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB)[0]);
			AssertEquals(null, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB)[1]);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, ranker.GetValues(ProcessTaskTemplateSchema.P0_GE)[0]);
			AssertEquals(null, ranker.GetValues(ProcessTaskTemplateSchema.P0_GE)[1]);
			AssertEquals(org.PK, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
			AssertEquals(ZGuid.Empty, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[1]);

			AssertEquals("AUSYD", ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry)[0]);
			AssertEquals("AU", ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry)[1]);
			AssertEquals("", ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry)[2]);
			AssertEquals("INBOM", ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry)[0]);
			AssertEquals("IN", ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry)[1]);
			AssertEquals("", ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry)[2]);
		}

		public void TestGetTemplateSelectionCriteria_WithMissingData()
		{
			BusinessObject shipment = (BusinessObject)Factory.New<Freight.Integration.CFS.ICFSShipment>();
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.Parent = (IJobHeaderParent)shipment;
			job.JH_GE = ZGuid.Empty;
			job.JH_GB = ZGuid.Empty;
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "";
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "";

			ColumnValueRanker ranker = ((IJobInvoicingPlugIn)shipment).GetJobRelatedTemplateSelectionCriteria();
			AssertEquals(ZGuid.Empty, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB)[0]);
			AssertEquals(null, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB)[1]);
			AssertEquals(ZGuid.Empty, ranker.GetValues(ProcessTaskTemplateSchema.P0_GE)[0]);
			AssertEquals(null, ranker.GetValues(ProcessTaskTemplateSchema.P0_GE)[1]);
			AssertEquals(ZGuid.Empty, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);

			AssertEquals("", ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry)[0]);
			AssertEquals("", ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry)[0]);
		}

		public void TestGetTemplateSelectionCriteria_ForImportClient()
		{
			var taskTemplateSearchProviderMock = new Mock<IJobInvoicingPlugIn>();
			taskTemplateSearchProviderMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			taskTemplateSearchProviderMock.Setup(m => m.Factory).Returns(Factory);

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.IsImport).Returns(true);
			mockSupporter.Setup(m => m.Consignee).Returns(consignee);
			mockSupporter.Setup(m => m.Consignor).Returns(consignor);
			taskTemplateSearchProviderMock.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			taskTemplateSearchProviderMock.Setup(m => m.IsInDatabase).Returns(false);

			ColumnValueRanker ranker = taskTemplateSearchProviderMock.Object.GetJobRelatedTemplateSelectionCriteria();
			AssertEquals("Consignee first for import", consignee.PK, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
			AssertEquals("Consignor second for import", consignor.PK, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[1]);

			taskTemplateSearchProviderMock.VerifyAll();
			mockSupporter.VerifyAll();
		}

		public void TestGetTemplateSelectionCriteria_ForExportClient()
		{
			var taskTemplateSearchProviderMock = new Mock<IJobInvoicingPlugIn>();
			taskTemplateSearchProviderMock.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			taskTemplateSearchProviderMock.Setup(m => m.Factory).Returns(Factory);

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.IsImport).Returns(false);
			mockSupporter.Setup(m => m.Consignee).Returns(consignee);
			mockSupporter.Setup(m => m.Consignor).Returns(consignor);
			taskTemplateSearchProviderMock.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			taskTemplateSearchProviderMock.Setup(m => m.IsInDatabase).Returns(false);

			ColumnValueRanker ranker = taskTemplateSearchProviderMock.Object.GetJobRelatedTemplateSelectionCriteria();
			AssertEquals("Consignor first for export", consignor.PK, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
			AssertEquals("Consignee second for export", consignee.PK, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[1]);

			taskTemplateSearchProviderMock.VerifyAll();
			mockSupporter.VerifyAll();
		}

		public void TestGetTemplateSelectionCriteria_LoadedJobHasParent()
		{
			BusinessObject shipment = (BusinessObject)Factory.New<Freight.Integration.CFS.ICFSShipment>();
			shipment[JobShipmentSchema.JS_TransportMode] = Core.Constants.TransportModes.Air;
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "INBOM";
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.Parent = (IJobHeaderParent)shipment;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<Freight.Integration.CFS.ICFSShipment>(shipment.PK);
			var jobInNewFactory = newFactory.Load<JobHeader>(job.PK);
			((IJobInvoicingPlugIn)shipmentInNewFactory).GetJobRelatedTemplateSelectionCriteria();
			AssertNotNull("We should load job with parent in GetTemplateSelectionCriteria", jobInNewFactory.Parent);
		}

		public void TestGetTemplateSelectionCriteria_NullCountryCodeExceptionNotThrown_Origin()
		{
			RefUNLOCO port = Factory.New<RefUNLOCO>();
			port.RL_Code = "XXXXX";
			BusinessObject shipment = (BusinessObject)Factory.New<Freight.Integration.CFS.ICFSShipment>();
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = port.RL_Code;

			AssertNoExceptionThrown("When saving with a non-existant country code, no exceptions should not be thrown.", () =>
			{
				((IJobInvoicingPlugIn)shipment).GetJobRelatedTemplateSelectionCriteria();
			});
		}

		public void TestGetTemplateSelectionCriteria_NullCountryCodeExceptionNotThrown_Destination()
		{
			RefUNLOCO port = Factory.New<RefUNLOCO>();
			port.RL_Code = "XXXXX";
			BusinessObject shipment = (BusinessObject)Factory.New<Freight.Integration.CFS.ICFSShipment>();
			shipment[JobShipmentSchema.JS_RL_NKDestination] = port.RL_Code;

			AssertNoExceptionThrown("When saving with a non-existent country code, no exceptions should not be thrown.", () =>
			{
				((IJobInvoicingPlugIn)shipment).GetJobRelatedTemplateSelectionCriteria();
			});
		}

		public void TestGetTemplateSelectionCriteria_OriginCodeIncludedWhenCountryCodeNull_Origin()
		{
			RefUNLOCO port = Factory.New<RefUNLOCO>();
			port.RL_Code = "XXXXX";
			BusinessObject shipment = (BusinessObject)Factory.New<Freight.Integration.CFS.ICFSShipment>();
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = port.RL_Code;

			ColumnValueRanker ranker = null;
			ranker = ((IJobInvoicingPlugIn)shipment).GetJobRelatedTemplateSelectionCriteria();
			AssertEquals("The port code should still be included in ranker when country code is null.", "XXXXX", ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry)[0]);
			AssertEquals("The country code is null and thus an empty string must be added to the ranker.", ZString.Empty, ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry)[1]);
		}

		public void TestGetTemplateSelectionCriteria_OriginCodeIncludedWhenCountryCodeNull_Destination()
		{
			RefUNLOCO port = Factory.New<RefUNLOCO>();
			port.RL_Code = "XXXXX";
			BusinessObject shipment = (BusinessObject)Factory.New<Freight.Integration.CFS.ICFSShipment>();
			shipment[JobShipmentSchema.JS_RL_NKDestination] = port.RL_Code;

			ColumnValueRanker ranker = null;
			ranker = ((IJobInvoicingPlugIn)shipment).GetJobRelatedTemplateSelectionCriteria();
			AssertEquals("The port code should still be included in ranker when country code is null.", "XXXXX", ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry)[0]);
			AssertEquals("The country code is null and thus an empty string must be added to the ranker.", ZString.Empty, ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry)[1]);
		}
	}
}
