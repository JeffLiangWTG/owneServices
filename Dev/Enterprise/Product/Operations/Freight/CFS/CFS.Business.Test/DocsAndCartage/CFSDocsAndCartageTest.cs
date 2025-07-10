using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSDocsAndCartageTest : BaseFreightTest
	{
		public void TestCFSShouldNotValidateOrders_OrdersIsAForwardingThingNotCFS()
		{
			OverseasConsignee.MiscServ.OM_IMJobRequireOrderTrackLink = true;

			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_IsForwardRegistered = true;
			shipment.ConsigneePK = OverseasConsignee.PK;

			AssertNotNull("Hit to Lazy Load", shipment.DocsAndCartage);

			shipment.RunPreSaveValidation();
			AssertNoNotifications("Should have no notifications", shipment.DocsAndCartage.JP_OrderItemsAsStringInfo);
		}

		public void TestJobServices()
		{
			CFSDocsAndCartage docsAndCartage = Factory.New<CFSDocsAndCartage>();
			AssertNotNull("DocsAndCartage should contain a job services collection", docsAndCartage.Services);
		}

		public void TestJobServicesGetsLoadedProperly()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			JobService fumigation = shipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CFSShipment loadedShipment = newFactory.Load<CFSShipment>(shipment.PK);
			Assert("Shipment's job services did not load correctly", loadedShipment.DocsAndCartage.Services.Count > 0);
		}

		public void TestJobServicesIsRegisteredEditableChild()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.DocsAndCartage.SetReadOnlyIncludingChildren(true);
			AssertEquals("Job services should be read only (registered editable child of JobDocsAndCartage)", true, shipment.DocsAndCartage.Services.ReadOnly);
		}

		#region PopulateDates

		public void TestUpdateLCLAvailableDate()
		{
			ZDateTime now = ZDateTime.Now;
			DocsAndCartage.UpdateLCLAvailableDate(now);
			AssertEquals("Available date should be following date", now.AddDays(1), DocsAndCartage.JP_LCLAvailable);
		}

		public void TestUpdateLCLAvailableDateWithHazardous()
		{
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "A", "IMO").First().PK;
			ZDateTime now = ZDateTime.Now;
			DocsAndCartage.UpdateLCLAvailableDate(now);
			AssertEquals("Available date with packline that is hazardous should be the same date", now, DocsAndCartage.JP_LCLAvailable);
		}

		public void TestStorageDateDefaults()
		{
			Assert("Pre-condition: Expected registry to default to using Client Free Days", CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.Value);
			AssertEquals("Pre-condition: Expected registry to default to 3 for sea shipments", 3, EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightLCLStorageFreeDays.Value);

			ZDateTime now = ZDateTime.Now;
			DocsAndCartage.JP_LCLAvailable = now;

			AssertEquals("Storage date should fall back to registry if client days are empty", now.AddDays(3), DocsAndCartage.JP_LCLStorageCommences);

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_IMSeaDepotFreeDays = 5;
			DocsAndCartage.ClientForStorage = client.PK;
			DocsAndCartage.JP_LCLAvailable = now;

			AssertEquals("Storage date should use client free days if available", now.AddDays(5), DocsAndCartage.JP_LCLStorageCommences);

			CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			DocsAndCartage.JP_LCLAvailable = now;

			AssertEquals("Storage date should be following date +3", now.AddDays(3), DocsAndCartage.JP_LCLStorageCommences);
		}

		public void TestStorageDateDefaultsWithHazardous()
		{
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "A", "IMO").First().PK;
			ZDateTime now = ZDateTime.Now;
			DocsAndCartage.JP_LCLAvailable = now;
			AssertEquals("Storage date with packline that is hazardous should be the same date", now, DocsAndCartage.JP_LCLStorageCommences);
		}

		#region Storage Date Is Updated By Available Date

		public void TestStorageDateIsUpdatedByAvailableDate_ForAir()
		{
			TestStorageDateIsUpdatedByAvailableDate(Constants.TransportModes.Air);
		}

		public void TestStorageDateIsUpdatedByAvailableDate_ForSea()
		{
			TestStorageDateIsUpdatedByAvailableDate(Constants.TransportModes.Sea);
		}

		void TestStorageDateIsUpdatedByAvailableDate(string mode)
		{
			AssertEquals("Pre-condition: CFS Air registry should default to using client free days", true, CFSDataRegistry.Instance.CFSAirFreightUseClientFreeDays.Value);
			AssertEquals("Pre-condition: CFS Air registry should default 1 free days", 1, EnvProxy.Instance.Registry.RawRegistry.CFSAirFreightLCLStorageFreeDays.Value);
			AssertEquals("Pre-condition: CFS Sea registry should default to using client free days", true, CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.Value);
			AssertEquals("Pre-condition: CFS Sea registry should default 3 free days", 3, EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightLCLStorageFreeDays.Value);

			Shipment.JS_TransportMode = mode;

			var testDate = new ZDateTime(2012, 03, 18);
			DocsAndCartage.JP_LCLAvailable = testDate;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ[GetFreeDaysByTransportMode(client, mode)] = (ZByte)4;
			DocsAndCartage.ClientForStorage = client.PK;

			DocsAndCartage.JP_LCLAvailable = testDate;
			AssertEquals("Should use ClientForStorage's free days", DocsAndCartage.JP_LCLAvailable.AddDays(4), DocsAndCartage.JP_LCLStorageCommences);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ[GetFreeDaysByTransportMode(consignee, mode)] = (ZByte)8;
			Shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			DocsAndCartage.JP_LCLAvailable = testDate;
			AssertEquals("Should use Consignee free days.", DocsAndCartage.JP_LCLAvailable.AddDays(8), DocsAndCartage.JP_LCLStorageCommences);

			consignee.MiscServ[GetFreeDaysByTransportMode(consignee, mode)] = (ZByte)0;

			DocsAndCartage.JP_LCLAvailable = testDate;
			AssertEquals("Should fall back to ClientForStorage if consignee free days are 0", DocsAndCartage.JP_LCLAvailable.AddDays(4), DocsAndCartage.JP_LCLStorageCommences);

			EnvProxy.Instance.Registry.RawRegistry.CFSAirFreightLCLStorageFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 20);
			EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightLCLStorageFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 20);
			DocsAndCartage.JP_LCLAvailable = testDate;

			AssertEquals("Should still take free days from ClientForStorage since registry is set to using client free days", DocsAndCartage.JP_LCLAvailable.AddDays(4), DocsAndCartage.JP_LCLStorageCommences);

			CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			CFSDataRegistry.Instance.CFSAirFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			DocsAndCartage.JP_LCLAvailable = testDate;

			AssertEquals("Should calculate free days from the registry", DocsAndCartage.JP_LCLAvailable.AddDays(20), DocsAndCartage.JP_LCLStorageCommences);

			EnvProxy.Instance.Registry.RawRegistry.CFSAirFreightLCLStorageFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 0);
			EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightLCLStorageFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 0);
			DocsAndCartage.JP_LCLAvailable = testDate;

			AssertEquals("Should still use free days from the registry, even if they are 0", DocsAndCartage.JP_LCLAvailable, DocsAndCartage.JP_LCLStorageCommences);

			CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			CFSDataRegistry.Instance.CFSAirFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			DocsAndCartage.JP_LCLAvailable = testDate;

			AssertEquals("Should not take free days from the registry", DocsAndCartage.JP_LCLAvailable.AddDays(4), DocsAndCartage.JP_LCLStorageCommences);
		}

		ZString GetFreeDaysByTransportMode(OrgHeader org, string mode)
		{
			return mode == Constants.TransportModes.Air ? "OM_IMAirDepotFreeDays" : "OM_IMSeaDepotFreeDays";
		}

		#endregion

		#endregion

		#region Implementation

		CFSShipment Shipment;
		CFSDocsAndCartage DocsAndCartage;

		protected override void SetUp()
		{
			Shipment = Factory.New<CFSShipment>();
			DocsAndCartage = Shipment.DocsAndCartage;
		}

		#endregion
	}
}
