using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Module.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobConsolFilterControlSecurityStatusBashFetchTest : FilterControlBashFetchHintTest<ForwardingModuleConsol>
	{
		#region BashFetchTest

		public void TestBashFetchForView_JK_SecurityStatus()
		{
			// CusEntryNum: 1
			// ExportAWBHeader: 1
			// JobConShipLink: 1
			// JobConsolTransport: 1
			// JobSailing: 1
			// JobShipment: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// RefUNLOCO: 1

			BashFetchForView("JK_SecurityStatus", 10);
		}

		#endregion

		#region Implementation

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			for (var i = 0; i < 12; i++)
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;

				var voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "GBLHR";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
				voyage.GenerateSailings();

				var consol = factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "GBLHR";

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "TEST2017";
				container.JC_RC = refContainer.PK;

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "GBLON";
				shipment1.JS_RL_NKDestination = "USLAX";
				shipment1.JS_UniqueConsignRef = "shipment" + i;
				shipment1.JS_InspectionTypeCode = "NUC";

				var outerPackline = shipment1.OuterPackLines.AddNew();
				outerPackline.SetContainer(consol, container);

				var transport = consol.Transports[0];
				transport.JW_TransportMode = "AIR";
				transport.JW_RL_NKLoadPort = "GBLHR";
				transport.JW_RL_NKDiscPort = "USLAX";
				transport.JW_IsCargoOnly = false;
				transport.JW_IsLinked = true;
				transport.JW_JX = voyage.Sailings[0].PK;

				consol.JK_OverrideWaybillDefaults = i < 6;

				if (i < 6)
				{
					var exportAWBHeader = Factory.New<ConsolExportAWBHeader>();
					exportAWBHeader.EH_ParentID = consol.PK;
				}

				result.Add(consol.PK);
			}

			factory.Save();

			return result.ToArray();
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var consols = new ForwardingConsolCollection(Factory);
			var filterBo = new JobConsolFilterBusinessObject();
			return new JobConsolFilterControl(consols, filterBo);
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new ForwardingModuleConsolCollection(Factory);
		}

		protected override SchemaPKColumn PkColumn => JobConsolSchema.PK;

		protected override string[] GetExcludedColumnNamesForTestFetchHint()
		{
			using (var filterControl = GetNewFilterStripControl())
			{
				return filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Where(c => c.ColumnName != "JK_SecurityStatus")
					.Select(c => c.ColumnName)
					.ToArray();
			}
		}

		protected override void SetUp()
		{
			countryDispose = GlbCompany.CurrentCompany.TemporarilySetCountry("GB");

			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
			((ShipmentInspectionType)inspectionTypes.Types.FindByCode("NUC")).AllowedOnPassengerFlights = false;
			shipmentInspectionTypesDispose = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes);

			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();

			shipmentInspectionTypesDispose.Dispose();
			countryDispose.Dispose();
		}

		IDisposable countryDispose;
		IDisposable shipmentInspectionTypesDispose;

		#endregion
	}
}
