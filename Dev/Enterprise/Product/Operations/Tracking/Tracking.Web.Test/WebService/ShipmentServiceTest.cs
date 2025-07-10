using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Module;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.ShipmentDeclaration;
using Enterprise.Tracking.Web.WebService;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ShipmentServiceTest : BaseWebServiceTest<ShipmentService>
	{
		public void TestGetShipmentsListInternalEmpty()
		{
			Factory.Save();
			WebShipments result = WebService.GetShipmentsListInternal(CurrentOrg, null);
			AssertNotNull("even though there are no shipments, it should return something", result);
			AssertEquals("empty content", 0, result.WebShipment.Count);
		}

		public void TestGetShipmentsListInternalFull()
		{
			var shipment = Factory.NewWithValidTestData<TrackingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			var shipment2 = Factory.NewWithValidTestData<TrackingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_OH_Forwarder = CurrentOrg.PK;

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration2.JE_OH_Supplier = CurrentOrg.PK;
			declaration2.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration3.JE_OH_Supplier = CurrentOrg.PK;
			declaration3.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var declaration4 = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration4.JE_OH_Importer = CurrentOrg.PK;
			declaration4.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var declaration5 = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration5.JE_OH_Importer = CurrentOrg.PK;
			declaration5.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var declaration6 = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration6.JE_OH_Importer = CurrentOrg.PK;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "hello";
			job.JH_ParentID = declaration6.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			declaration6.Job.JH_OA_LocalChargesAddr = CurrentOrg.MainAddress.PK;

			CurrentOrg.OH_IsConsignee = true;
			var testContact = CurrentOrg.Contacts.AddNew();
			testContact.OC_Email = "test@edi.com.au";
			var password = "abc123";
			testContact.SetHashedPassword(password);
			testContact.OC_WebAccessEnabled = true;

			shipment.ConsigneePK = CurrentOrg.PK;
			declaration.JE_OH_Importer = CurrentOrg.PK;

			Factory.Save();

			ZArchitecture.Web.Business.WebEnv.AppInstance.SiteUser.Login(CurrentOrg.OH_Code, testContact.OC_Email, password);

			WebShipments result = WebService.GetShipmentsListInternal(CurrentOrg, null);
			AssertNotNull("there are shipments, it should return something", result);
			AssertEquals("Should contain one shipment and three declarations", 1, result.WebShipment.Count);
			var numberCollection = result.WebShipment.Cast<WebShipment>().Select(ws => ws.Number);
			AssertCollectionContains("Should contain shipment", shipment.JS_UniqueConsignRef, numberCollection);
			AssertEquals("Total row count", "1", result.TotalRows);
			AssertEquals("returned row count", "1", result.ReturnedRows);
		}

		public void TestGetFilterBusinessObjectEmpty()
		{
			TrackingShipmentFilterBusinessObject filterBizO = WebService.GetFilterBusinessObject(null);
			AssertNotNull("Filter Business Object must be created", filterBizO);
		}

		public void TestGetFilterBusinessObjectFull()
		{
			var mockCO2eFeatureControl = new Mock<ICO2eFeatureControlHelper>();
			mockCO2eFeatureControl.Setup(m => m.Enabled).Returns(true);

			using (ObjectFactory.Substitute(mockCO2eFeatureControl.Object))
			{
				WebShipmentFilter xmlFilter = new WebShipmentFilter();
				xmlFilter.Number = new WebShipmentFilterNumber();
				xmlFilter.Number.NumberValue = "S00001278";
				xmlFilter.Number.NumberSearchField = ShipmentNumberFieldsList.ALL;
				TrackingShipmentFilterBusinessObject filterBizO = WebService.GetFilterBusinessObject(xmlFilter);
				foreach (string description in TrackingShipmentFilterBizOToXmlMappings.Instance.AllNumberFilters)
				{
					AssertEquals("number", "S00001278", ((ModuleNumberFilter)filterBizO[description]).Property);
					AssertEquals(FilterOrCategory.Blue, filterBizO[description].OrCategory);
					Assert(filterBizO[description].IsActive);
				}

				xmlFilter = new WebShipmentFilter();
				xmlFilter.Date = new WebShipmentFilterDate();
				xmlFilter.Date.FromDate = new ZDate(2010, 10, 10);
				xmlFilter.Date.ToDate = new ZDate(2011, 11, 11);
				xmlFilter.Date.DateSearchField = ShipmentDateFieldsList.ALL;
				filterBizO = WebService.GetFilterBusinessObject(xmlFilter);
				foreach (string description in TrackingShipmentFilterBizOToXmlMappings.Instance.AllDateFilters)
				{
					AssertEquals("date", new ZDateTime(2010, 10, 10), ((ModuleDateFilter)filterBizO[description]).Property1);
					AssertEquals("date", new ZDateTime(2011, 11, 11), ((ModuleDateFilter)filterBizO[description]).Property2);
					AssertEquals("date", ModuleDateFilter.SpecifiedDateRange, ((ModuleDateFilter)filterBizO[description]).PropertySearch);
					AssertEquals(FilterOrCategory.Brown, filterBizO[description].OrCategory);
					Assert(filterBizO[description].IsActive);
				}

				xmlFilter = new WebShipmentFilter();
				xmlFilter.Organisation = new WebShipmentFilterOrganisation();
				xmlFilter.Organisation.Organisation1 = "MERRIL";
				xmlFilter.Organisation.Organisation2 = "BRATEX";
				xmlFilter.Organisation.OrganisationSearchField = ShipmentOrganisationFieldsList.ALL;
				filterBizO = WebService.GetFilterBusinessObject(xmlFilter);
				foreach (string description in TrackingShipmentFilterBizOToXmlMappings.Instance.AllOrganizationFilters)
				{
					if (filterBizO[description] is ModuleGuidFilter)
					{
						AssertEquals("org", new ZGuid("49ad5edf-f405-412f-bbab-64c2340d3b43"), ((ModuleGuidFilter)filterBizO[description]).Property);
					}
					if (filterBizO[description] is ModuleGuidsFilter)
					{
						AssertEquals("org", new ZGuid("49ad5edf-f405-412f-bbab-64c2340d3b43"), ((ModuleGuidsFilter)filterBizO[description]).Property1);
						AssertEquals("org", new ZGuid("5aff7e55-519d-46f5-958b-81f5201690b2"), ((ModuleGuidsFilter)filterBizO[description]).Property2);
					}
					AssertEquals(FilterOrCategory.Green, filterBizO[description].OrCategory);
					Assert(filterBizO[description].IsActive);
				}

				xmlFilter = new WebShipmentFilter();
				xmlFilter.Location = new WebShipmentFilterLocation();
				xmlFilter.Location.Location1.Value = "AUSYD";
				xmlFilter.Location.Location2.Value = "AUMEL";
				filterBizO = WebService.GetFilterBusinessObject(xmlFilter);
				foreach (string description in TrackingShipmentFilterBizOToXmlMappings.Instance.AllLocationFilters)
				{
					AssertEquals("location", "AUSYD", ((ModuleLocationFilter)filterBizO[description]).Property1);
					AssertEquals("location", "AUMEL", ((ModuleLocationFilter)filterBizO[description]).Property2);
					AssertEquals(FilterOrCategory.Grey, filterBizO[description].OrCategory);
					Assert(filterBizO[description].IsActive);
				}

				xmlFilter = new WebShipmentFilter();
				filterBizO = WebService.GetFilterBusinessObject(xmlFilter);
				AssertEquals("status is not specified", string.Empty, ((ModuleTextFilter)filterBizO[TrackingShipmentFilterBusinessObject.Descriptions.Status]).Property);
				Assert(!filterBizO[TrackingShipmentFilterBusinessObject.Descriptions.Status].IsActive);

				xmlFilter.Status = ShipmentStatusList.Delivered;
				filterBizO = WebService.GetFilterBusinessObject(xmlFilter);
				AssertEquals("status is specified", Business.Shipments.ShipmentStatus.Codes.Delivered, ((ModuleTextFilter)filterBizO[TrackingShipmentFilterBusinessObject.Descriptions.Status]).Property);
				Assert(filterBizO[TrackingShipmentFilterBusinessObject.Descriptions.Status].IsActive);

				xmlFilter = new WebShipmentFilter();
				xmlFilter.Vessel = "123";
				xmlFilter.VoyageFlight = "456";
				filterBizO = WebService.GetFilterBusinessObject(xmlFilter);
				AssertEquals("vessel and voyage", "456", ((VoyageVesselModuleFilter)filterBizO[JobShipmentFilterBusinessObject.Descriptions.FlightVoyageAndVessel]).VoyageFlightNo);
				AssertEquals("vessel and voyage", "123", ((VoyageVesselModuleFilter)filterBizO[JobShipmentFilterBusinessObject.Descriptions.FlightVoyageAndVessel]).Vessel);
			}
		}

		public void TestSoapBodyElementName()
		{
			// Callers expect to provide XML bodies beginning with capital letters.
			var getShipmentListParameters = typeof(ShipmentService).GetMethod(nameof(ShipmentService.GetShipmentsList)).GetParameters();
			var xmlAttribute = getShipmentListParameters[0].GetCustomAttributes(true).OfType<XmlElementAttribute>().FirstOrDefault();

			AssertEquals("Filter", xmlAttribute.ElementName);
		}
	}
}
