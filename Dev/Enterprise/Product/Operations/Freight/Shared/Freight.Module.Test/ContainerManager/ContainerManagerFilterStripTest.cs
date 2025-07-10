using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(ContainerManagerFilterStrip))]
	sealed class ContainerManagerFilterStripTest : FilterStripBusinessObjectTestCase
	{
		#region TestFilterByRelatedConsol

		public void TestFilterByRelatedConsol()
		{
			var containers = new ContainerNonDependentCollection(Factory);

			var commonConsol1 = Factory.New<CommonConsol>();
			commonConsol1.JK_AgentsReference = "AAA";
			commonConsol1.JK_TransportMode = Constants.TransportModes.Air;

			var commonContainer1 = commonConsol1.Containers.AddNew();

			var commonConsol2 = Factory.New<CommonConsol>();
			commonConsol2.JK_AgentsReference = "BBB";
			commonConsol2.JK_TransportMode = Constants.TransportModes.Sea;

			var commonContainer2 = commonConsol2.Containers.AddNew();

			var commonContainer3 = Factory.New<CommonContainer>();

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)Strip[ContainerManagerFilterStrip.Descriptions.RelatedConsol];
			filter.IsActive = true;

			AssertEquals("Should default to FiltersMatch", ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch, filter.ComparisonOperator);

			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			filter.Property = commonConsol1.PK;

			containers.Load(Strip.Filter);

			var pks = containers.GetPKs();
			AssertCollectionContains("The commonContainer1 belong to commonConsol1", commonContainer1.PK, pks);
			AssertCollectionNotContains("The commonContainer2 does not belong to commonConsol1", commonContainer2.PK, pks);
			AssertCollectionNotContains("The commonContainer3's JC_JK is null", commonContainer3.PK, pks);

			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;

			containers.Load(Strip.Filter);

			pks = containers.GetPKs();
			AssertCollectionContains("The commonContainer2 does not belong to commonConsol1", commonContainer2.PK, pks);
			AssertCollectionNotContains("The commonContainer1 belong to commonConsol1", commonContainer1.PK, pks);
			AssertCollectionContains("The commonContainer3's JC_JK is null", commonContainer3.PK, pks);

			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;

			containers.Load(Strip.Filter);

			pks = containers.GetPKs();
			AssertCollectionContains("The commonContainer3's JC_JK is null", commonContainer3.PK, pks);
			AssertCollectionNotContains("The commonContainer1's JC_JK is not null", commonContainer1.PK, pks);
			AssertCollectionNotContains("The commonContainer2's JC_JK is not null", commonContainer2.PK, pks);

			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank;

			containers.Load(Strip.Filter);

			pks = containers.GetPKs();
			AssertCollectionContains("The commonContainer1's JC_JK is not null", commonContainer1.PK, pks);
			AssertCollectionContains("The commonContainer2's JC_JK is not null", commonContainer2.PK, pks);
			AssertCollectionNotContains("The commonContainer3's JC_JK is null", commonContainer3.PK, pks);
		}

		#endregion

		#region TestFilterByContainerNum

		public void TestFilterByContainerNum()
		{
			SetUpContainers();

			ModuleNumberFilter filter = (ModuleNumberFilter)Strip[ContainerManagerFilterStrip.Descriptions.ContainerNumber];
			CommonContainer[] containers;

			filter.Property = "cn123456";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find container1", container1, containers);
			AssertCollectionContains("Should find container2", container2, containers);
			AssertCollectionNotContains("Should not find container3", container3, containers);
			AssertCollectionNotContains("Should not find container4", container4, containers);

			filter.Property = "cn12345";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find container1", container1, containers);
			AssertCollectionContains("Should find container2", container2, containers);
			AssertCollectionContains("Should find container3", container3, containers);
			AssertCollectionNotContains("Should not find container4", container4, containers);
		}

		#endregion

		#region TestFilterBySealNum

		public void TestFilterBySealNum()
		{
			SetUpContainers();

			ModuleNumberFilter filter = (ModuleNumberFilter)Strip[ContainerManagerFilterStrip.Descriptions.Seal];
			CommonContainer[] containers;

			filter.Property = "sn123456";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find Container1", container1, containers);
			AssertCollectionContains("Should find Container2 ", container2, containers);
			AssertCollectionNotContains("Should not find container3", container3, containers);
			AssertCollectionNotContains("Should not find container4", container4, containers);
			AssertCollectionNotContains("Should not find container5", container5, containers);
			AssertCollectionNotContains("Should not find container6", container6, containers);

			filter.Property = "sn12345";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find container1", container1, containers);
			AssertCollectionContains("Should find container2", container2, containers);
			AssertCollectionContains("Should find Container3", container3, containers);
			AssertCollectionNotContains("Should not find container4", container4, containers);
			AssertCollectionNotContains("Should not find container5", container5, containers);
			AssertCollectionNotContains("Should not find container6", container6, containers);
		}

		#endregion

		#region TestFilterByArrivalSlotReference

		public void TestFilterByArrivalSlotReference()
		{
			SetUpContainers();

			ModuleNumberFilter filter = (ModuleNumberFilter)Strip[ContainerManagerFilterStrip.Descriptions.ArrivalSlotReference];
			CommonContainer[] containers;

			filter.Property = "as123456";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find Container1", container1, containers);
			AssertCollectionContains("Should find Container2 ", container2, containers);
			AssertCollectionNotContains("Should not find container3", container3, containers);
			AssertCollectionNotContains("Should not find container4", container4, containers);

			filter.Property = "as12345";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find container1", container1, containers);
			AssertCollectionContains("Should find container2", container2, containers);
			AssertCollectionContains("Should find Container3", container3, containers);
			AssertCollectionNotContains("Should not find container4", container4, containers);
		}

		#endregion

		#region TestFilterByDepartureSlotReference

		public void TestFilterByDepartureSlotReference()
		{
			SetUpContainers();

			ModuleNumberFilter filter = (ModuleNumberFilter)Strip[ContainerManagerFilterStrip.Descriptions.DepartureSlotReference];
			CommonContainer[] containers;

			filter.Property = "ds123456";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find Container1", container1, containers);
			AssertCollectionContains("Should find Container2 ", container2, containers);
			AssertCollectionNotContains("Should not find container3", container3, containers);
			AssertCollectionNotContains("Should not find container4", container4, containers);

			filter.Property = "ds12345";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should not find container1", container1, containers);
			AssertCollectionContains("Should not find container2", container2, containers);
			AssertCollectionContains("Should find Container3", container3, containers);
			AssertCollectionNotContains("Should not find container4", container4, containers);
		}

		#endregion

		#region TestFilterByAdditionalReferences

		public void TestFilterByAdditionalReferences()
		{
			SetUpContainers();

			NewReferenceNumber(container1, "AU", "COC", "MUNDANE");
			NewReferenceNumber(container2, "US", "COC", "MAGIC");
			NewReferenceNumber(container3, "AU", "COC", "MAGIC");
			NewReferenceNumber(container4, "AU", "COC", "APPLE");
			NewReferenceNumber(container5, "AU", "ASL", "MAGIC");
			NewReferenceNumber(container6, "AU", "ASL", "");
			NewReferenceNumber(container7, "US", "COC", "MAGIC");
			NewReferenceNumber(container8, "CN", "COC", "APPLE");

			Factory.Save();

			var filter = (ReferenceNumberFilter)Strip[ContainerManagerFilterStrip.Descriptions.AdditionalReferenceNumbers];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			var containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContainsOnly("Should include all containers", containers, new[]
				{ container1, container2, container3, container4, container5, container6, container7, container8, container9 });

			SetFilter(filter, "AU", "COC", "MAGIC");
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContainsOnly("AU:COC:MAGIC", containers, new[] { container3 });

			SetFilter(filter, "US", "COC", "MAGIC");
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContainsOnly("US:COC:MAGIC", containers, new[] { container2, container7 });

			SetFilter(filter, string.Empty, "COC", "MAGIC");
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContainsOnly("COC:MAGIC*", containers, new[] { container2, container3, container7 });

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			SetFilter(filter, string.Empty, "ASL", string.Empty);
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContainsOnly("Without ASL", containers, new[]
				{ container1, container2, container3, container4, container6, container7, container8, container9 });

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			SetFilter(filter, string.Empty, "ASL", string.Empty);
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContainsOnly("With ASL", containers, new[] { container5 });

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			SetFilter(filter, "", "COC", "MAGIC");
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContainsOnly("NotEqual COC:MAGIC", containers, new[] { container1, container4, container5, container6, container8, container9 });

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			SetFilter(filter, "", "COC", "ANE");
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContainsOnly("NotContains COC:ANE", containers, new[]
				{ container2, container3, container4, container5, container6, container7, container8, container9 });

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			SetFilter(filter, "", "COC", "M");
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContainsOnly("DoesNotStartWith COC:M", containers, new[] { container4, container5, container6, container8, container9 });
		}

		void AssertCollectionContainsOnly(string message, IEnumerable<CommonContainer> foundContainers, IEnumerable<CommonContainer> expectedContainers)
		{
			foreach (var container in new[]
				{
					container1, container2, container3, container4, container5, container6, container7, container8, container9
				})
			{
				if (expectedContainers.Contains(container))
				{
					AssertCollectionContains(message, container, foundContainers);
				}
				else
				{
					AssertCollectionNotContains(message, container, foundContainers);
				}
			}
		}

		static CusEntryNumber NewReferenceNumber(CommonContainer container, string countryCode, string type, string number)
		{
			var result = container.AdditionalReferenceNumbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
			return result;
		}

		static void SetFilter(ReferenceNumberFilter filter, string country, string type, string number)
		{
			filter.Country = country;
			filter.Type = type;
			filter.Property = number;
		}

		#endregion

		#region TestFilterByPortOrigin

		public void TestFilterByPortOrigin()
		{
			SetUpContainers();

			ModuleLocationFilter filter = (ModuleLocationFilter)Strip[ContainerManagerFilterStrip.Descriptions.OriginDestination];
			CommonContainer[] containers;

			filter.Property1 = "";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find Container1 on empty Port Origin", container1, containers);
			AssertCollectionContains("Should find Container2 on empty Port Origin", container2, containers);
			AssertCollectionContains("Should find Container3 on empty Port Origin", container3, containers);
			AssertCollectionContains("Should find Container4 on empty Port Origin", container4, containers);
			AssertCollectionContains("Should find Container5 on empty Port Origin", container5, containers);
			AssertCollectionContains("Should find container6 on empty Port Origin", container6, containers);

			filter.Property1 = "An invalid Port Origin";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionNotContains("Should not find Container1 on empty Port Origin", container1, containers);
			AssertCollectionNotContains("Should not find Container2 on empty Port Origin", container2, containers);
			AssertCollectionNotContains("Should not find Container3 on empty Port Origin", container3, containers);
			AssertCollectionNotContains("Should not find Container4 on empty Port Origin", container4, containers);
			AssertCollectionNotContains("Should not find Container5 on empty Port Origin", container5, containers);
			AssertCollectionNotContains("Should not find container6 on empty Port Origin", container6, containers);

			filter.Property1 = "AA111";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find Container1", container1, containers);
			AssertCollectionNotContains("Should not find Container2", container2, containers);
			AssertCollectionContains("Should find Container4", container4, containers);
			AssertCollectionContains("Should find Container5", container5, containers);
			AssertCollectionContains("Should find Container6", container6, containers);
			AssertCollectionContains("Should find Container7", container7, containers);
			AssertCollectionContains("Should find Container8", container8, containers);
			AssertCollectionContains("Should find Container9", container9, containers);
		}

		#endregion

		#region TestFilterByPortDestination

		public void TestFilterByPortDestination()
		{
			SetUpContainers();

			ModuleLocationFilter filter = (ModuleLocationFilter)Strip[ContainerManagerFilterStrip.Descriptions.OriginDestination];
			CommonContainer[] containers;

			filter.Property2 = "";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find Container1 on empty Port Destination", container1, containers);
			AssertCollectionContains("Should find Container2 on empty Port Destination", container2, containers);
			AssertCollectionContains("Should find Container3 on empty Port Destination", container3, containers);
			AssertCollectionContains("Should find Container4 on empty Port Destination", container4, containers);
			AssertCollectionContains("Should find Container5 on empty Port Destination", container5, containers);
			AssertCollectionContains("Should find Container6 on empty Port Destination", container6, containers);

			filter.Property2 = "An invalid Port Destination";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionNotContains("Should not find Container1 on empty Port Destination", container1, containers);
			AssertCollectionNotContains("Should not find Container2 on empty Port Destination", container2, containers);
			AssertCollectionNotContains("Should not find Container3 on empty Port Destination", container3, containers);
			AssertCollectionNotContains("Should not find Container4 on empty Port Destination", container4, containers);
			AssertCollectionNotContains("Should not find Container5 on empty Port Destination", container5, containers);
			AssertCollectionNotContains("Should not find container6 on empty Port Destination", container6, containers);

			filter.Property2 = "BB111";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find Container1", container1, containers);
			AssertCollectionNotContains("Should not find Container2", container2, containers);
			AssertCollectionContains("Should find Container4", container4, containers);
			AssertCollectionContains("Should find Container5", container5, containers);
			AssertCollectionContains("Should find Container6", container6, containers);
			AssertCollectionContains("Should find Container7", container7, containers);
			AssertCollectionContains("Should find Container8", container8, containers);
			AssertCollectionContains("Should find Container9", container9, containers);
		}

		#endregion

		#region TestFilterByContainerLocation

		public void TestFilterByContainerLocation()
		{
			SetUpContainers();

			ModuleTextFilter filter = (ModuleTextFilter)Strip[ContainerManagerFilterStrip.Descriptions.ContainerLocation];
			CommonContainer[] containers;

			filter.Property = ContainerManagerFilterStrip.ContainerLocation.ImpNotReturnToCY;
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find Container4", container4, containers);
			AssertCollectionNotContains("Should not find Container5", container5, containers);

			filter.Property = ContainerManagerFilterStrip.ContainerLocation.ImpReturnToCY;
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionNotContains("Should not find Container4", container4, containers);
			AssertCollectionContains("Should find Container5", container5, containers);

			filter.Property = ContainerManagerFilterStrip.ContainerLocation.ExpNotOnboard;
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find Container4", container4, containers);
			AssertCollectionNotContains("Should not find Container5", container5, containers);

			filter.Property = ContainerManagerFilterStrip.ContainerLocation.ExpOnboard;
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionNotContains("Should not find Container4", container4, containers);
			AssertCollectionContains("Should find Container5", container5, containers);
		}

		#endregion

		#region TestFilterByDateContainerYardGateIn

		public void TestFilterByDateContainerYardGateIn()
		{
			SetUpContainers();

			TestFilterByDateRange(ContainerManagerFilterStrip.Descriptions.ContainerYardGateIn, JobContainerSchema.JC_ContainerYardEmptyReturnGateIn);
		}

		#endregion

		#region TestFilterByDateContainerYardGateOut

		public void TestFilterByDateContainerYardGateOut()
		{
			SetUpContainers();

			TestFilterByDateRange(ContainerManagerFilterStrip.Descriptions.ContainerYardGateOut, JobContainerSchema.JC_ContainerYardEmptyPickupGateOut);
		}

		#endregion

		#region TestFilterByDateWharfGateIn

		public void TestFilterByDateWharfGateIn()
		{
			SetUpContainers();

			TestFilterByDateRange(ContainerManagerFilterStrip.Descriptions.WharfGateIn, JobContainerSchema.JC_FCLWharfGateIn);
		}

		#endregion

		#region TestFilterByDateImpUnloaded

		public void TestFilterByDateImpUnloaded()
		{
			SetUpContainers();

			TestFilterByDateRange(ContainerManagerFilterStrip.Descriptions.Unloaded, JobContainerSchema.JC_FCLUnloadFromVessel);
		}

		#endregion

		#region TestFilterByDateExpOnboard

		public void TestFilterByDateExpOnboard()
		{
			SetUpContainers();

			TestFilterByDateRange(ContainerManagerFilterStrip.Descriptions.OnBoard, JobContainerSchema.JC_FCLOnBoardVessel);
		}

		#endregion

		#region TestFilterByDateWharfGateOut

		public void TestFilterByDateWharfGateOut()
		{
			SetUpContainers();

			TestFilterByDateRange(ContainerManagerFilterStrip.Descriptions.WharfGateOut, JobContainerSchema.JC_FCLWharfGateOut);
		}

		#endregion

		#region TestFilterByDateAvailable

		public void TestFilterByDateAvailable()
		{
			SetUpContainers();

			TestFilterByDateRange(ContainerManagerFilterStrip.Descriptions.Availiable, JobContainerSchema.JC_FCLAvailable);
		}

		public void TestFilterByDateAvailableWithSailing()
		{
			SetUpContainers();

			consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "AUBNE";
			consol1.JK_RL_NKDischargePort = "AUSYD";
			consol1.Transports[0].JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Name;
			consol1.Transports[0].JW_VoyageFlight = "1111";

			consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "SGSIN";
			consol2.Transports[0].JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Name;
			consol2.Transports[0].JW_VoyageFlight = "2222";

			container1.JC_OverrideFCLAvailableStorage = false;
			container2.JC_OverrideFCLAvailableStorage = false;

			consol1.Containers.Add(container1);
			consol2.Containers.Add(container2);

			Factory.Save();

			AssertEquals("Prequisite", 1, consol1.Containers.Count);
			AssertEquals("Prequisite", 1, consol2.Containers.Count);
			AssertNotNull("Prequisite", consol1.Schedule);
			AssertNotNull("Prequisite", consol2.Schedule);
			AssertNotNull("Prequisite", consol1.VoyDestination);
			AssertNotNull("Prequisite", consol2.VoyDestination);

			TestFilterByDateRange(ContainerManagerFilterStrip.Descriptions.Availiable,
				(date) => consol1.VoyDestination.JB_AvailabilityDate = date,
				(date) => consol2.VoyDestination.JB_AvailabilityDate = date);
		}

		public void TestFilterByDateAvailableWithCustomDeclaration()
		{
			SetUpContainers();

			container1.JC_OverrideFCLAvailableStorage = false;
			container2.JC_OverrideFCLAvailableStorage = false;

			var destination1 = Factory.NewWithValidTestData<VoyageDestination>();
			var destination2 = Factory.NewWithValidTestData<VoyageDestination>();

			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			sailing1.JX_JB = destination1.PK;

			var transport1 = (Transport)((BusinessObjectCollection)decl1["Transports"]).AddNew();
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;

			var sailing2 = Factory.NewWithValidTestData<JobSailing>();
			sailing2.JX_JB = destination2.PK;

			var transport2 = (Transport)((BusinessObjectCollection)decl2["Transports"]).AddNew();
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;

			Factory.Save();

			TestFilterByDateRange(ContainerManagerFilterStrip.Descriptions.Availiable,
				(date) => destination1.JB_AvailabilityDate = date,
				(date) => destination2.JB_AvailabilityDate = date);
		}

		#endregion

		#region TestFilterByDateStorage

		public void TestFilterByDateStorage()
		{
			SetUpContainers();

			TestFilterByDateRange(ContainerManagerFilterStrip.Descriptions.Storage, JobContainerSchema.JC_ArrivalCTOStorageStartDate);
		}

		#endregion

		#region TestFilterByDateRange

		public void TestFilterByDateRange(String filterName, SchemaDateTimeColumn column)
		{
			TestFilterByDateRange(filterName,
				(date) => container1[column.Name] = date,
				(date) => container2[column.Name] = date);
		}

		public void TestFilterByDateRange(String filterName, Action<ZDateTime> container1DateUpdater, Action<ZDateTime> container2DateUpdater)
		{
			ZDateTime now = ZDateTime.Now;

			ModuleDateFilter filter = (ModuleDateFilter)Strip[filterName];
			CommonContainer[] containers;
			filter.PropertySearch = "Date range";

			container1DateUpdater(now.AddDays(-1));
			container2DateUpdater(now.AddDays(1));

			Factory.Save();

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find container1", container1, containers);
			AssertCollectionContains("Should find container2", container2, containers);

			filter.Property1 = now;
			filter.Property2 = ZDateTime.Empty;
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionNotContains("Should not find container1", container1, containers);
			AssertCollectionContains("Should find container2", container2, containers);

			filter.Property1 = now;
			filter.Property2 = now;
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionNotContains("Should not find container1", container1, containers);
			AssertCollectionNotContains("Should not find container2", container2, containers);

			filter.Property1 = now.AddDays(-1);
			filter.Property2 = now;
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find container1", container1, containers);
			AssertCollectionNotContains("Should not find container2", container2, containers);

			filter.Property1 = now.AddDays(-1);
			filter.Property2 = now.AddDays(1);
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find container1", container1, containers);
			AssertCollectionContains("Should find container2", container2, containers);
		}

		#endregion

		#region TestFilterByVerifedStatus

		public void TestFilterByVGMStatus()
		{
			SetUpContainers();

			var filter = (ModuleTextFilter)Strip[ContainerManagerFilterStrip.Descriptions.VGMStatus];
			CommonContainer[] containers;

			filter.Property = "NON";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find Container7", container7, containers);
			AssertCollectionContains("Should find Container8", container8, containers);
			AssertCollectionContains("Should find Container9", container9, containers);
			AssertCollectionNotContains("Should not find container1", container1, containers);
			AssertCollectionNotContains("Should not find container2", container2, containers);
			AssertCollectionNotContains("Should not find container3", container1, containers);
			AssertCollectionNotContains("Should not find container4", container2, containers);
			AssertCollectionNotContains("Should not find container5", container1, containers);
			AssertCollectionNotContains("Should not find container6", container2, containers);

			filter.Property = "SNT";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find container1", container1, containers);
			AssertCollectionContains("Should find container2", container2, containers);
			AssertCollectionContains("Should find Container3", container3, containers);
			AssertCollectionNotContains("Should not find container4", container4, containers);
			AssertCollectionNotContains("Should not find container5", container5, containers);
			AssertCollectionNotContains("Should not find container6", container6, containers);
			AssertCollectionNotContains("Should not find container7", container7, containers);
			AssertCollectionNotContains("Should not find container8", container8, containers);
			AssertCollectionNotContains("Should not find container9", container9, containers);

			filter.Property = "REJ";
			containers = Factory.Load<CommonContainer>(filter.Query);
			AssertCollectionContains("Should find container4", container4, containers);
			AssertCollectionContains("Should find container5", container5, containers);
			AssertCollectionContains("Should find Container6", container6, containers);
			AssertCollectionNotContains("Should not find container1", container1, containers);
			AssertCollectionNotContains("Should not find container2", container2, containers);
			AssertCollectionNotContains("Should not find container3", container3, containers);
			AssertCollectionNotContains("Should not find container7", container7, containers);
			AssertCollectionNotContains("Should not find container8", container8, containers);
			AssertCollectionNotContains("Should not find container9", container9, containers);
		}

		#endregion

		#region TestFilterByServiceTypeDate

		public void TestFilterByServiceTypeDate()
		{
			var consol = Factory.New<CommonConsol>();
			var container = Factory.New<CommonContainer>();
			container.JC_JK = consol.PK;
			var jobService = container.Services.AddNew();
			jobService.ES_Booked = ZDateTime.BrettsBirthday;
			jobService.ES_Completed = ZDateTime.BrettsBirthday.AddDays(10);
			jobService.ES_ServiceCode = "FUM";
			Factory.Save();

			var bookedFilter = (ServiceTypeDateFilter)Strip["Service Type / Date Booked"];
			bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			bookedFilter.IsActive = true;
			var completedFilter = (ServiceTypeDateFilter)Strip["Service Type / Date Completed"];
			completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			completedFilter.IsActive = true;

			var results = new CommonContainerCollection(consol, Factory);

			bookedFilter.JobServiceType = "MUF";
			results.Load(Strip.Filter);
			AssertEquals("ES_ServiceCode is not matched so there is no data.", false, results.Any());

			bookedFilter.JobServiceType = "FUM";
			bookedFilter.Property2 = ZDateTime.BrettsBirthday.AddDays(-1);
			results.Load(Strip.Filter);
			AssertEquals("ES_Booked is not matched so there is no data.", false, results.Any());

			bookedFilter.Property2 = ZDateTime.BrettsBirthday;
			completedFilter.Property1 = ZDateTime.BrettsBirthday.AddDays(11);
			results.Load(Strip.Filter);
			AssertEquals("ES_Completed is not matched so there is no data.", false, results.Any());

			completedFilter.Property1 = ZDateTime.BrettsBirthday.AddDays(10);
			results.Load(Strip.Filter);
			AssertEquals("Should have one data", 1, results.Count);
			AssertEquals(true, results.Contains(container));
		}

		#endregion

		#region TestFilterByAllocationId

		public void TestFilterByAllocationId()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var container = Factory.New<CommonContainer>();
				var allocationLine = container.AllocationLineCollection.AddNew();
				allocationLine.FillWithValidTestData();
				allocationLine[nameof(RatingContractAllocationLineSchema.RCA_AllocationLineID)] = "ABC";
				container.JC_RCA_AllocationLine = allocationLine.PK;
				Factory.Save();
				ModuleNumberFilter filter = (ModuleNumberFilter)Strip[ContainerManagerFilterStrip.Descriptions.AllocationID];
				filter.Property = "ABC";
				CommonContainer[] containers = Factory.Load<CommonContainer>(filter.Query);
				AssertCollectionContains("Should find container", container, containers);
			}
		}

		#endregion

		#region TestFilterByAllocationId_ExceedLength_NoException

		[ExpectNoExceptions]
		public void TestFilterByAllocationId_ExceedLength_NoException()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var container = Factory.New<CommonContainer>();

				ModuleNumberFilter filter = (ModuleNumberFilter)Strip[ContainerManagerFilterStrip.Descriptions.AllocationID];
				filter.Property = "D00148347";
				CommonContainer[] containers = Factory.Load<CommonContainer>(filter.Query);
				AssertCollectionNotContains("Should not find container", container, containers);
			}
		}

		public void TestAllocationIDFilterHiddenForCCAWithPenaltiesOnly()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNull(Strip[ContainerManagerFilterStrip.Descriptions.AllocationID]);
			}
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ContainerManagerFilterStrip();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();

			result.Add(TableFilter(JobConsolSchema.Constants.TableName, ContainerManagerFilterStrip.Descriptions.RelatedConsol));
			result.Add(TableFilter(CusContainerSchema.Constants.TableName, ContainerManagerFilterStrip.Descriptions.OriginDestination));
			result.Add(TableFilter(JobBookedCtgMoveSchema.Constants.TableName, ContainerManagerFilterStrip.Descriptions.OriginDestination));
			result.Add(TableFilter(JobCartageSchema.Constants.TableName, ContainerManagerFilterStrip.Descriptions.OriginDestination));
			result.Add(TableFilter(JobConsolSchema.Constants.TableName, ContainerManagerFilterStrip.Descriptions.OriginDestination));
			result.Add(TableFilter(JobContainerSchema.Constants.TableName, ContainerManagerFilterStrip.Descriptions.OriginDestination));
			result.Add(TableFilter(JobDeclarationSchema.Constants.TableName, ContainerManagerFilterStrip.Descriptions.OriginDestination));
			result.Add(TableFilter(JobSailingSchema.Constants.TableName, ContainerManagerFilterStrip.Descriptions.OriginDestination));
			result.Add(TableFilter(JobShipmentSchema.Constants.TableName, ContainerManagerFilterStrip.Descriptions.OriginDestination));
			result.Add(TableFilter(JobVoyOriginSchema.Constants.TableName, ContainerManagerFilterStrip.Descriptions.OriginDestination));

			return result;
		}

		#region Strip

		ContainerManagerFilterStrip Strip
		{
			get { return strip ?? (strip = new ContainerManagerFilterStrip()); }
		}
		ContainerManagerFilterStrip strip;

		#endregion

		#region SetUpContainers

		void SetUpContainers()
		{
			comp0 = Factory.New<OrgHeader>();
			comp0.OH_Code = "INVALID COMP";
			comp1 = Factory.New<OrgHeader>();
			comp1.OH_Code = "COMP1";
			comp2 = Factory.New<OrgHeader>();
			comp2.OH_Code = "COMP2";

			container1 = Factory.New<CommonContainer>();
			container1.JC_ContainerNotes = "Container1";
			container1.JC_ContainerNum = "cn123456";
			container1.JC_SealNum = "sn123456";
			container1.JC_AdditionalSealNum = "sa123456";
			container1.JC_Additional2SealNum = "sb123456";
			container1.JC_DepartureSlotReference = "ds123456";
			container1.JC_ArrivalSlotReference = "as123456";
			container1.JC_GrossWeightVerificationStatus = "SNT";

			decl1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			BusinessObject cusContainer1 = ((BusinessObjectCollection)decl1["CusContainers"]).AddNew();
			cusContainer1[CusContainerSchema.Constants.CO_JC] = container1.PK;

			decl1[JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading] = "AA111";
			decl1[JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival] = "BB111";

			container1.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Empty;
			container1.JC_FCLAvailable = date1;
			container1.JC_ArrivalCTOStorageStartDate = date1;
			container1.JC_FCLWharfGateIn = date1;
			container1.JC_FCLWharfGateOut = date1;
			container1.JC_ContainerYardEmptyReturnGateIn = date1;
			container1.JC_ContainerYardEmptyPickupGateOut = date1;
			container1.JC_FCLUnloadFromVessel = date1;
			container1.JC_FCLOnBoardVessel = date1;

			container2 = Factory.New<CommonContainer>();
			container2.JC_ContainerNotes = "Container2";
			container2.JC_ContainerNum = "cn123456";
			container2.JC_SealNum = "sn123456";
			container2.JC_AdditionalSealNum = "sa123456";
			container2.JC_Additional2SealNum = "sb123456";
			container2.JC_DepartureSlotReference = "ds123456";
			container2.JC_ArrivalSlotReference = "as123456";
			container2.JC_GrossWeightVerificationStatus = "SNT";

			decl2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			BusinessObject cusContainer2 = ((BusinessObjectCollection)decl2["CusContainers"]).AddNew();
			cusContainer2["CO_JC"] = container2.PK;
			decl2[JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading] = "BB222";
			decl2[JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival] = "AA222";
			container2.JC_ContainerYardEmptyReturnGateIn = date1;
			container2.JC_FCLAvailable = date1;
			container2.JC_ArrivalCTOStorageStartDate = date1;

			container3 = Factory.New<CommonContainer>();
			container3.JC_ContainerNotes = "Container3";
			container3.JC_ContainerNum = "cn12345";
			container3.JC_SealNum = "sn12345";
			container3.JC_AdditionalSealNum = "sa12345";
			container3.JC_Additional2SealNum = "sb12345";
			container3.JC_DepartureSlotReference = "ds12345";
			container3.JC_ArrivalSlotReference = "as12345";
			container3.JC_FCLOnBoardVessel = ZDateTime.Empty;
			container3.JC_GrossWeightVerificationStatus = "SNT";

			BusinessObject decl3 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			BusinessObject cusContainer3 = ((BusinessObjectCollection)decl3["CusContainers"]).AddNew();
			cusContainer3[CusContainerSchema.Constants.CO_JC] = container3.PK;
			decl3[JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading] = "AA111";
			decl3[JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival] = "BB111";
			container3.JC_FCLAvailable = date2;
			container3.JC_ArrivalCTOStorageStartDate = date2;
			container3.JC_FCLWharfGateIn = date2;
			container3.JC_FCLWharfGateOut = date2;
			container3.JC_ContainerYardEmptyReturnGateIn = date2;
			container3.JC_ContainerYardEmptyPickupGateOut = date2;
			container3.JC_FCLUnloadFromVessel = date2;
			container3.JC_FCLOnBoardVessel = date2;

			container4 = Factory.New<CommonContainer>();
			container4.JC_ContainerNotes = "Container4";
			container4.JC_ContainerNum = "cn1234";
			container4.JC_SealNum = "sn";
			container4.JC_AdditionalSealNum = "sa";
			container4.JC_Additional2SealNum = "sb1234";
			container4.JC_DepartureSlotReference = "ds1234";
			container4.JC_ArrivalSlotReference = "as1234";
			container4.JC_FCLOnBoardVessel = date1;
			container4.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Empty;
			container4.JC_FCLOnBoardVessel = ZDateTime.Empty;
			container4.JC_GrossWeightVerificationStatus = "REJ";

			CommonShipment ship1 = Factory.New<CommonShipment>();
			container4.JC_JS_FCLBookingOnlyLink = ship1.PK;
			ship1.JS_RL_NKOrigin = "AA111";
			ship1.JS_RL_NKDestination = "BB111";

			container5 = Factory.New<CommonContainer>();
			container5.JC_ContainerNotes = "Container5";
			container5.JC_ContainerNum = "cn123";
			container5.JC_SealNum = "sn123";
			container5.JC_AdditionalSealNum = "sa123";
			container5.JC_Additional2SealNum = "sb123";
			container5.JC_DepartureSlotReference = "ds123";
			container5.JC_ArrivalSlotReference = "as123";
			container5.JC_ContainerYardEmptyReturnGateIn = date1;
			container5.JC_FCLOnBoardVessel = date1;
			container5.JC_GrossWeightVerificationStatus = "REJ";

			JobSailing sail1 = Factory.New<JobSailing>();
			container5.JC_JX = sail1.PK;

			JobVoyage voy1 = Factory.New<JobVoyage>();

			VoyageOrigin voyorig1 = Factory.New<VoyageOrigin>();
			sail1.JX_JA = voyorig1.PK;
			voyorig1.JA_RL_NKPortOfLoading = "AA111";
			voyorig1.JA_JV = voy1.PK;

			VoyageDestination voydest1 = Factory.New<VoyageDestination>();
			sail1.JX_JB = voydest1.PK;
			voydest1.JB_RL_NKPortOfDischarge = "BB111";
			voydest1.JB_JV = voy1.PK;

			container6 = Factory.New<CommonContainer>();
			container6.JC_ContainerNotes = "Container6";
			container6.JC_GrossWeightVerificationStatus = "REJ";

			BusinessObject decl4 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			decl4[JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading] = "AA111";
			decl4[JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival] = "BB111";

			BusinessObject cart1 = (BusinessObject)Factory.New<ICommonCartage>();
			cart1[JobCartageSchema.JJ_ConsignmentID] = "12345";
			cart1[JobCartageSchema.JJ_ParentID] = decl4.PK;
			cart1[JobCartageSchema.JJ_ParentTableCode] = "JE";

			BusinessObject cartpivot1 = (BusinessObject)Factory.New<ICommonBookedCtgMove>();
			cartpivot1[JobBookedCtgMoveSchema.EW_JC_Container] = container6.PK;
			cartpivot1[JobBookedCtgMoveSchema.EW_JJ] = cart1.PK;

			container7 = Factory.New<CommonContainer>();
			container7.JC_ContainerNotes = "Container7";

			CommonShipment ship2 = Factory.New<CommonShipment>();
			ship2.JS_RL_NKOrigin = "AA111";
			ship2.JS_RL_NKDestination = "BB111";

			BusinessObject cart2 = (BusinessObject)Factory.New<ICommonCartage>();
			cart2[JobCartageSchema.JJ_ConsignmentID] = "12346";
			cart2[JobCartageSchema.JJ_ParentID] = ship2.PK;
			cart2[JobCartageSchema.JJ_ParentTableCode] = "JS";

			BusinessObject cartpivot2 = (BusinessObject)Factory.New<ICommonBookedCtgMove>();
			cartpivot2[JobBookedCtgMoveSchema.EW_JC_Container] = container7.PK;
			cartpivot2[JobBookedCtgMoveSchema.EW_JJ] = cart2.PK;

			container8 = Factory.New<CommonContainer>();
			container8.JC_ContainerNotes = "Container8";

			consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "AA111";
			consol1.JK_RL_NKDischargePort = "BB111";
			container8.JC_JK = consol1.PK;

			Transport tran1 = consol1.Transports[0];
			tran1.JW_IsLinked = false;
			tran1.JW_RL_NKLoadPort = "AA111";

			Transport tran2 = consol1.Transports.AddNew();
			tran2.JW_IsLinked = false;
			tran2.JW_RL_NKDiscPort = "BB111";

			container9 = Factory.New<CommonContainer>();
			container9.JC_ContainerNotes = "Container9";

			consol2 = Factory.New<CommonConsol>();
			consol2.JK_RL_NKLoadPort = "AA111";
			consol2.JK_RL_NKDischargePort = "BB111";
			container9.JC_JK = consol2.PK;

			Transport tran3 = consol2.Transports[0];
			tran3.JW_IsLinked = false;
			tran3.JW_RL_NKLoadPort = "AA222";

			Transport tran4 = consol2.Transports.AddNew();
			tran4.JW_IsLinked = false;
			tran4.JW_RL_NKDiscPort = "BB222";

			JobSailing sail2 = Factory.New<JobSailing>();
			tran3.JW_JX = sail2.PK;

			JobVoyage voy3 = Factory.New<JobVoyage>();

			VoyageOrigin voyorig2 = Factory.New<VoyageOrigin>();
			sail2.JX_JA = voyorig2.PK;
			voyorig2.JA_RL_NKPortOfLoading = "AA111";
			voyorig2.JA_JV = voy3.PK;

			VoyageDestination voydest2 = Factory.New<VoyageDestination>();
			sail2.JX_JB = voydest2.PK;
			voydest2.JB_RL_NKPortOfDischarge = "XX111";
			voydest2.JB_JV = voy3.PK;

			JobSailing sail3 = Factory.New<JobSailing>();
			tran4.JW_JX = sail3.PK;

			VoyageOrigin voyorig3 = Factory.New<VoyageOrigin>();
			sail3.JX_JA = voyorig3.PK;
			voyorig3.JA_RL_NKPortOfLoading = "YY111";
			voyorig3.JA_JV = voy3.PK;

			VoyageDestination voydest3 = Factory.New<VoyageDestination>();
			sail3.JX_JB = voydest3.PK;
			voydest3.JB_RL_NKPortOfDischarge = "BB111";
			voydest3.JB_JV = voy3.PK;

			Factory.Save();
		}

		#endregion

		readonly ZDateTime date1 = new ZDateTime(2004, 1, 1);
		readonly ZDateTime date2 = new ZDateTime(2004, 1, 2);

		OrgHeader comp0;
		OrgHeader comp1;
		OrgHeader comp2;

		CommonContainer container1;
		CommonContainer container2;
		CommonContainer container3;
		CommonContainer container4;
		CommonContainer container5;
		CommonContainer container6;
		CommonContainer container7;
		CommonContainer container8;
		CommonContainer container9;

		CommonConsol consol1;
		CommonConsol consol2;

		BusinessObject decl1;
		BusinessObject decl2;

		#endregion
	}
}
