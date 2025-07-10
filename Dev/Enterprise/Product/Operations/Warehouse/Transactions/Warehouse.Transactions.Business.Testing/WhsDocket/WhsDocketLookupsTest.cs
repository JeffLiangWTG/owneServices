using CargoWise.Application;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsDocketLookupsTest<TDocket> : WhsBusinessObjectLookupsTestCase
		where TDocket : WhsDocket
	{
		#region TestCarrierServiceLevels

		public void TestCarrierServiceLevels()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				var transportCo = job.GetTransportCo();
				AssertNull("Pre-condition", transportCo);
				AssertEquals(0, docket.Lookups.CarrierServiceLevels.Count);

				var transportCo1 = Factory.New<OrgHeader>();
				job.TransportCoPK = transportCo1.PK;
				AssertEquals("OrgCarrierLevelCollection adds the 'STD' service when an Org is present", 1, docket.Lookups.CarrierServiceLevels.Count);

				var serv1 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
				var serv2 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
				var serv3 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
				AssertEquals("OrgCarrierLevelCollection adds the 'STD' service when an Org is present", 4, docket.Lookups.CarrierServiceLevels.Count);
				AssertCollectionContains(serv1, docket.Lookups.CarrierServiceLevels);
				AssertCollectionContains(serv2, docket.Lookups.CarrierServiceLevels);
				AssertCollectionContains(serv3, docket.Lookups.CarrierServiceLevels);
			}
			else
			{
				AssertEquals(0, docket.Lookups.CarrierServiceLevels.Count);
			}
		}

		#endregion

		#region TestDropModes

		public virtual void TestDropModes()
		{
			CheckLCLDropModes();
			CheckFCLDropModes();
		}

		protected virtual void CheckLCLDropModes()
		{
			Docket.Containers.DeleteAll();
			AssertEquals("Precondition: Should have no containers", false, Docket.Containers.Count > 0);
			AssertNotNull(Lookups.DropModes);

			CodeDescriptionPairList expectedList = new LCLAIREquipmentNeededList(true);
			foreach (CodeDescriptionPair pair in Lookups.DropModes)
			{
				AssertEquals("Invalid CodeDesriptionPair found", true, expectedList.ContainsCode(pair));
			}

			foreach (CodeDescriptionPair pair in expectedList)
			{
				AssertEquals("Missing CodeDescriptionPair", true, Lookups.DropModes.ContainsCode(pair));
			}
		}

		protected virtual void CheckFCLDropModes()
		{
			Docket.Containers.Add(Factory.New<WhsDocketContainer>());
			AssertEquals("Precondition: Should have containers", true, Docket.Containers.Count > 0);
			AssertNotNull(Lookups.DropModes);

			CodeDescriptionPairList expectedList = new FCLEquipmentNeededList(true);
			foreach (CodeDescriptionPair pair in Lookups.DropModes)
			{
				AssertEquals("Invalid CodeDesriptionPair found", true, expectedList.ContainsCode(pair));
			}

			foreach (CodeDescriptionPair pair in expectedList)
			{
				AssertEquals("Missing CodeDescriptionPair", true, Lookups.DropModes.ContainsCode(pair));
			}
		}

		#endregion

		#region TestSuppliers

		public void TestSuppliers()
		{
			AssertEquals(typeof(ConsignorCollection), Lookups.Suppliers.GetType());
		}

		#endregion

		#region TestClients

		public void TestClients()
		{
			var docket = GetNewBusinessObject();
			Factory.New<OrgHeader>().OH_IsWarehouseClient = true;

			AssertNotNull(docket.Lookups.Clients);
			AssertEquals(typeof(WarehouseClientCollectionWithSecurityCheck), docket.Lookups.Clients.GetType());
			AssertEquals("Collection should not be loaded", 0, docket.Lookups.Clients.Count);

			var helper = new UnmatchOrgRecordTestHelper(docket, WarehouseClientRec);

			OrganisationsFindBoxCollection clients = docket.Lookups.Clients as OrganisationsFindBoxCollection;
			helper.PopulateOrgDefaultsFromUnmatchedNote(docket.Lookups.GetType(), "Clients", clients);
			AssertEquals("Clients should have defaults from unmatchorgnotes", true, clients.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(clients.DefaultsForNewChild, WarehouseClientRec);
		}

		UnmatchOrgRecord WarehouseClientRec
		{
			get
			{
				if (warehouseClientRec == null)
				{
					warehouseClientRec = new UnmatchOrgRecord()
					{
						OrganisationType = nameof(OrganisationTypes.WarehouseClient),
						OrganisationSubType = nameof(OrganisationTypes.WarehouseClient),
						AddressLine1 = "whsClient addr 1",
						AddressLine2 = "whsClient addr 2",
						OrganisationName = "warehouseClientName",
						PostCode = "2222",
						StateOrProvince = "NSW",
						City = "sydney",
						EDICode = "warehouseClient",
						OwnerCode = "whsOwnerCode"
					};
				}
				return warehouseClientRec;
			}
		}
		UnmatchOrgRecord warehouseClientRec;

		#endregion

		#region TestDistributionCentre

		public void TestDistributionCentre()
		{
			var distributionCentre = CreateOrgHeaderWithServiceType("O1", true, true);
			var nonDistributionCentre = CreateOrgHeaderWithServiceType("O2", true, false);
			var nonServiceCentre = CreateOrgHeaderWithServiceType("O3", false, false);
			Factory.Save();

			var distributionCentres = Lookups.DistributionCentres;
			distributionCentres.Load();
			AssertCollectionContains(distributionCentre, distributionCentres);
			AssertCollectionNotContains(nonDistributionCentre, distributionCentres);
			AssertCollectionNotContains(nonServiceCentre, distributionCentres);
		}

		OrgHeader CreateOrgHeaderWithServiceType(string orgCode, bool isService, bool isDistributionCentre)
		{
			var org = Helper.CreateClient(orgCode);
			org.OH_IsMiscFreightServices = isService;
			org.OH_IsDistributionCentre = isDistributionCentre;
			return org;
		}

		#endregion

		#region TestWarehouses

		public virtual void TestWarehouses()
		{
			AssertNotNull(Lookups.Warehouses);
		}

		#endregion

		#region TestTransportZones

		public virtual void TestTransportZones()
		{
			AssertNotNull(Lookups.TransportZones);
		}

		#endregion

		#region TestConsignees

		public void TestConsigneesDefaultFilter()
		{
			Assert("Default filter 'Consignee - Related Consignor:Property' is expected"
				, Lookups.Consignees.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor:Property")
			);
		}

		public void TestConsignees()
		{
			var order = Factory.New<WhsOrder>();
			Factory.New<OrgHeader>().OH_IsConsignee = true;

			AssertNotNull(order.Lookups.Consignees);
			AssertEquals(typeof(ConsigneeCollection), order.Lookups.Consignees.GetType());
			AssertEquals("Collection should not be loaded", 0, order.Lookups.Consignees.Count);

			var helper = new UnmatchOrgRecordTestHelper(order, ConsigneeRec);
			var consignees = order.Lookups.Consignees as OrganisationsFindBoxCollection;
			helper.PopulateOrgDefaultsFromUnmatchedNote(order.Lookups.GetType(), "Consignees", consignees);
			AssertEquals("Consignees should have defaults from unmatchorgnotes", true, consignees.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(consignees.DefaultsForNewChild, ConsigneeRec);
		}

		UnmatchOrgRecord ConsigneeRec
		{
			get
			{
				if (consigneeRec == null)
				{
					consigneeRec = new UnmatchOrgRecord()
					{
						OrganisationType = nameof(OrganisationTypes.Consignee),
						OrganisationSubType = nameof(OrganisationTypes.Consignee),
						AddressLine1 = "consignee addr 1",
						AddressLine2 = "consignee addr 2",
						OrganisationName = "consigneeName",
						PostCode = "2222",
						StateOrProvince = "NSW",
						City = "sydney",
						EDICode = "consignee",
						OwnerCode = "whsOwnerCode"
					};
				}
				return consigneeRec;
			}
		}
		UnmatchOrgRecord consigneeRec;

		#endregion

		#region TestDebtors

		public virtual void TestDebtors()
		{
			var org1 = Helper.CreateClient("ORG1");
			var org2 = Helper.CreateClient("ORG2");
			var org3 = Helper.CreateClient("ORG3");
			org1.OH_IsShippingProvider = true;
			org2.OH_IsWarehouseClient = true;
			org3.OH_IsDebtor = true;

			Factory.Save();

			var collection = Lookups.Debtors;
			collection.Load();
			AssertCollectionNotContains(org1, collection);
			AssertCollectionNotContains(org2, collection);
			AssertCollectionContains(org3, collection);
			AssertEquals(typeof(DebtorCollection), collection.GetType());
			Assert("Default filter 'Organisation Types:Property0' is expected", collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:Property0"));
			AssertEquals("Incorrect error message", "An Organization selected from here must have an Organization type of Receivables selected.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(org1));
		}

		#endregion

		#region TestPackingTasks

		public void TestPackingTasks()
		{
			AssertEquals("Should be a no result query, to avoid accidental loads of many records.", true, GetNewBusinessObject().Lookups.PackingTasks.CompleteFilter.IsNoResultQuery);
		}

		#endregion

		#region TestPickUp

		public virtual void TestPickUp()
		{
			AssertEquals(typeof(OrganisationsFindBoxCollection), Lookups.PickUps.GetType());
			AssertEquals(0, Lookups.PickUps.Count);
		}

		#endregion

		#region TestDropOff

		public virtual void TestDropOff()
		{
			AssertEquals(typeof(OrganisationsFindBoxCollection), Lookups.DropOffs.GetType());
			AssertEquals(0, Lookups.DropOffs.Count);
		}

		#endregion

		#region TestWeightUnitTypes

		public virtual void TestWeightUnitTypes()
		{
			AssertNotNull(Lookups.WeightUnitTypes);
		}

		#endregion

		#region TestCubicUnitTypes

		public virtual void TestCubicUnitTypes()
		{
			AssertNotNull(Lookups.CubicUnitTypes);
		}

		#endregion

		#region TestWhsOrderFulfillmentRules

		public void TestWhsOrderFulfillmentRules()
		{
			AssertNotNull(Lookups.WhsOrderFulfillmentRules);
		}

		#endregion

		#region TestPickOptions

		public void TestPickOptions()
		{
			AssertEquals(3, Lookups.PickOptions.Count);
			AssertEquals(true, Lookups.PickOptions.ContainsCode(WhsPickOption.Codes.Auto));
			AssertEquals(true, Lookups.PickOptions.ContainsCode(WhsPickOption.Codes.Manual));
			AssertEquals(true, Lookups.PickOptions.ContainsCode(WhsPickOption.Codes.ManualWithAutoAllocate));
		}

		#endregion

		#region TestProductUQ

		public virtual void TestProductUQ()
		{
			AssertNotNull(Lookups.ProductUQ);
		}

		#endregion

		#region TestShipperCODPaymentTypes

		public virtual void TestShipperCODPaymentTypes()
		{
			ReadOnlyCodeDescriptionPairList expectedList = FreightDataRegistry.Instance.ShipperCODPaymentTypes.Value;
			foreach (CodeDescriptionPair pair in Docket.Lookups.ShipperCODPaymentTypes)
			{
				AssertEquals("Invalid CodeDesriptionPair found", true, expectedList.ContainsCode(pair));
			}

			foreach (CodeDescriptionPair pair in expectedList)
			{
				AssertEquals("Missing CodeDescriptionPair", true, Docket.Lookups.ShipperCODPaymentTypes.ContainsCode(pair));
			}
		}

		#endregion

		#region TestINCOTerms

		public virtual void TestINCOTerms()
		{
			var expectedDomesticList = new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms);

			Assert(Docket.IsDomesticFreight);
			foreach (CodeDescriptionPair pair in Docket.Lookups.INCOTerms)
			{
				AssertEquals("Invalid CodeDesriptionPair found", true, expectedDomesticList.ContainsCode(pair));
			}

			foreach (CodeDescriptionPair pair in expectedDomesticList)
			{
				AssertEquals("Missing CodeDescriptionPair", true, Docket.Lookups.INCOTerms.ContainsCode(pair));
			}
		}

		#endregion

		#region TestTransportModes

		public void TestTransportModes()
		{
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.TransportType), Lookups.TransportModes);
		}

		#endregion

		#region TestContainerModes

		public void TestContainerModes()
		{
			AssertContainsExactElementsInAnyOrder(ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(Docket.WD_TransportMode), Lookups.ContainerModes);
		}

		#endregion

		#region TestScreeningStatuses

		public void TestScreeningStatuses()
		{
			AssertContainsExactElementsInAnyOrder(new ScreeningStatusesList(), Lookups.ScreeningStatusesList);
		}

		#endregion

		#region TestContainerModesFromTransportMode

		public void TestContainerModesFromTransportMode()
		{
			AssertContainsExactElementsInAnyOrder(ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(Constants.TransportCodes.Air), Lookups.ContainerModesFromTransportMode(Constants.TransportCodes.Air));
		}

		#endregion

		#region TestSubTypes

		public void TestSubTypes() => TestSubTypesCore();

		protected virtual void TestSubTypesCore() => AssertNull(Docket.Lookups.SubTypes);

		#endregion

		#region TestServiceLevels

		public void TestServiceLevels()
		{
			var srv1 = Factory.New<RefServiceLevel>();
			var srv2 = Factory.New<RefServiceLevel>();
			AssertEquals(typeof(RefServiceLevelCollection), Docket.Lookups.ServiceLevels.GetType());
			AssertCollectionContains(srv1, Docket.Lookups.ServiceLevels);
			AssertCollectionContains(srv2, Docket.Lookups.ServiceLevels);
		}

		#endregion

		#region TestPicksForReplenishment

		public void TestPicksForReplenishment() => TestPicksForReplenishmentCore();

		protected virtual void TestPicksForReplenishmentCore() => AssertNull(GetNewBusinessObject().Lookups.PicksForReplenishment);

		#endregion

		#region Implementation

		protected TDocket GetNewBusinessObject()
		{
			return Factory.New<TDocket>();
		}

		protected TDocket Docket
		{
			get => docket ?? (docket = GetNewBusinessObject());
		}

		protected WhsDocketLookups Lookups
		{
			get { return Docket.Lookups; }
		}

		TDocket docket;

		#endregion
	}
}
