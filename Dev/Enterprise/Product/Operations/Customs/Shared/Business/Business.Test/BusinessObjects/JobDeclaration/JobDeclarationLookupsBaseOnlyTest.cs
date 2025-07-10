using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationLookupsBaseOnlyTest : JobDeclarationLookupsAbstractTest<JobDeclarationLookups, BaseJobDeclaration>
	{
		protected override void AssertEntryStatusList_IntegratedAndABMInterface()
		{
			Assert("EntryStatusListForCustomsWare is empty by default in shared", true);
		}

		public void TestDeclarationLanguagesList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(string.Empty, declaration.Lookups.DeclarationLanguageList.CodesAsString);
		}

		public void TestInlandVesselNamesOrLloyds()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertType<RefVesselCollection>("InlandVesselNamesOrLloyds", declaration.Lookups.InlandVesselNamesOrLloyds);
		}

		public void TestPackingUnitTypesListCore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("UNPKG", "Packagings", "UNE");
			helper.CreateCusCodeList("UNE", "UNPKG", "BOX", "Box", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var declaration = Factory.New<BaseJobDeclaration>();

			Factory.Save();

			var packingUnitTypesList = declaration.Lookups.PackingUnitTypesList;

			CombineAssertions("packingUnitTypeList", () =>
			{
				AssertEquals(1, packingUnitTypesList.Count);
				AssertEquals("BOX", packingUnitTypesList[0].Code);
				AssertEquals("Box", packingUnitTypesList[0].Description);
			});
		}

		public void TestPaidByList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var list = declaration.Lookups.PaidByList;
			AssertEquals(typeof(MasterFiles.Business.Customs.PaidByCodeList), list.GetType());
			Assert(list.ContainsCode(MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK));
			Assert(list.ContainsCode(MasterFiles.Business.Customs.PaidByCodeList.Codes.CLI));
		}

		public void TestOrganisations()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertNotNull(declaration.Lookups.Organisations);
			AssertType<OrgHeaderCollection>("Organisations", declaration.Lookups.Organisations);
		}

		public void TestCarrierOrganisations()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertNotNull(declaration.Lookups.CarrierOrganisations);
			AssertType<ShippingProviderCollection>("CarrierOrganisations", declaration.Lookups.CarrierOrganisations);
		}

		public void TestConsigneeOrganisations()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertNotNull(declaration.Lookups.ConsigneeOrganisations);
			AssertType<ConsigneeCollection>("ConsigneeOrganisations", declaration.Lookups.ConsigneeOrganisations);
		}

		public void TestConsigneeList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertNotNull(declaration.Lookups.ConsigneeList);
			AssertType<OrganisationsFindBoxCollection>("ConsigneeList", lookups.ConsigneeList);
		}

		public void TestControllingAgents()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertNotNull(declaration.Lookups.ControllingAgents);
			AssertType<ControllingAgentCollection>("ControllingAgents", declaration.Lookups.ControllingAgents);
		}

		public void TestControllingCustomers()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertNotNull(declaration.Lookups.ControllingCustomers);
			AssertType<ControllingCustomerCollection>("ControllingCustomers", declaration.Lookups.ControllingCustomers);
		}

		public void TestExternalBrokers()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertNotNull(declaration.Lookups.ExternalBrokers);
			AssertType<BrokerCollection>("ExternalBrokers", declaration.Lookups.ExternalBrokers);
		}

		public void TestIInvoicesProviderLookups()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(declaration.Lookups.InvoicesToAttach.GetType(), ((IInvoicesProviderLookups)declaration.Lookups).InvoicesToAttach.GetType());
		}

		public void TestBillFilterByList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(typeof(BillFilterByList), declaration.Lookups.BillFilterByList.GetType());
		}

		public void TestScreeningStatusesList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var list = declaration.Lookups.ScreeningStatusesList;
			AssertEquals(typeof(ScreeningStatusesList), list.GetType());
			Assert(list.ContainsCode(ScreeningStatusesList.Codes.NotScreened));
		}

		public void TestCargoIdTypeList()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			AssertNotNull(dec.Lookups.CargoIdTypeList);
		}

		public void TestTransportMeansList()
		{
			CombineAssertions(() =>
			{
				var transportMeansList = lookups.TransportMeansList;
				AssertEquals("Codes", "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", transportMeansList.CodesAsString);
				AssertSame("Cached", transportMeansList, lookups.TransportMeansList);
			});
		}

		public void TestMessageStatusCategoryList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(typeof(MessageStatusCategoryList), declaration.Lookups.MessageStatusCategoryList.GetType());
		}

		public void TestSortedInvoiceList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			AssertEquals("SortedInvoiceList", declaration.SortedInvoiceList, declaration.Lookups.SortedInvoiceList);
		}

		public void TestBranchCollection()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			testDec.Lookups.BranchCollection.Load();
			AssertEquals("Branch collection does not have Branch created", false, testDec.Lookups.BranchCollection.Contains(branch));
			AssertEquals("AdditionalFilterNotMatch message", JobDeclarationLookups.BranchesAdditionalFilterNotMatchedError, testDec.Lookups.BranchCollection.GetAllNotificationsWhenAdditionalFilterNotMet(branch));
		}

		public void TestMessageTypeListIsCachedCorrectly()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var mockLookups = new Mock<JobDeclarationLookups>(declaration) { CallBase = true };
			var lookups = mockLookups.Object;
			var list1 = lookups.MessageTypeList;
			var list2 = lookups.MessageTypeList;
			AssertEquals("Is cached", true, object.ReferenceEquals(list1, list2));
			AssertEquals(true, list1.ContainsCode(JobMessageTypeList.Codes.Refund));
			mockLookups
				.Protected()
				.Setup<System.Collections.Generic.IEnumerable<ZString>>("GetNonSupportedMessageTypeCodes")
				.Returns(new ZString[] { JobMessageTypeList.Codes.Refund });
			var list3 = lookups.MessageTypeList;
			var list4 = lookups.MessageTypeList;
			AssertEquals("Should be different", false, object.ReferenceEquals(list1, list3));
			AssertEquals("Is cached", true, object.ReferenceEquals(list3, list4));
			AssertEquals(false, list3.ContainsCode(JobMessageTypeList.Codes.Refund));
		}

		public void TestRemovalOfDeclarationByExternalBroker()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			Assert("No IMX", !testDec.Lookups.MessageTypeList.ContainsCode(JobMessageTypeList.Codes.ImportDeclarationByExternalBroker));
			Assert("No EXX", !testDec.Lookups.MessageTypeList.ContainsCode(JobMessageTypeList.Codes.ExportDeclarationByExternalBroker));
		}

		public void TestFilterBusinessObjectDefaultsForInvoicesToAttach()
		{
			var testDec = Factory.New<BaseJobDeclaration>();

			testDec.JE_OH_Supplier = ZGuid.NewZGuid();
			testDec.JE_OH_Importer = ZGuid.NewZGuid();
			AssertEquals("Default set", testDec.JE_OH_Supplier, testDec.Lookups.InvoicesToAttach.FilterBusinessObjectDefaults[JobDeclarationLookups.ImporterSupplierFilterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"].Value);
			AssertEquals("Default set", testDec.JE_OH_Importer, testDec.Lookups.InvoicesToAttach.FilterBusinessObjectDefaults[JobDeclarationLookups.ImporterSupplierFilterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"].Value);

			testDec.JE_OH_Supplier = ZGuid.Invalid;
			testDec.JE_OH_Importer = ZGuid.Invalid;
			Assert("No default", !testDec.Lookups.InvoicesToAttach.FilterBusinessObjectDefaults.ContainsDefaultFor(JobDeclarationLookups.ImporterSupplierFilterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			Assert("No default", !testDec.Lookups.InvoicesToAttach.FilterBusinessObjectDefaults.ContainsDefaultFor(JobDeclarationLookups.ImporterSupplierFilterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));

			testDec.JE_OH_Supplier = ZGuid.NewZGuid();
			testDec.JE_OH_Importer = ZGuid.NewZGuid();
			AssertEquals("Default changed", testDec.JE_OH_Supplier, testDec.Lookups.InvoicesToAttach.FilterBusinessObjectDefaults[JobDeclarationLookups.ImporterSupplierFilterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"].Value);
			AssertEquals("Default changed", testDec.JE_OH_Importer, testDec.Lookups.InvoicesToAttach.FilterBusinessObjectDefaults[JobDeclarationLookups.ImporterSupplierFilterName + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"].Value);
		}

		public void TestWarningsForInvoicesToAttach()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.MakeNonPersistent();
			testDec.JE_MessageType = "EXP";
			testDec.JE_OH_Supplier = ZGuid.NewZGuid();
			testDec.JE_OH_Importer = ZGuid.NewZGuid();

			var invoice1 = GetNewInvoiceMatchingDec(testDec);
			invoice1.JZ_MessageType = "IMP";

			var invoice2 = GetNewInvoiceMatchingDec(testDec);
			invoice2.JZ_OH_Supplier = ZGuid.NewZGuid();

			var invoice3 = GetNewInvoiceMatchingDec(testDec);
			invoice3.JZ_OH_Buyer = ZGuid.NewZGuid();

			var invoice4 = GetNewInvoiceMatchingDec(testDec);

			var invoice5 = GetNewInvoiceMatchingDec(testDec);
			invoice5.JZ_MessageType = "ASN";

			var invoice6 = GetNewInvoiceMatchingDec(testDec);
			((ICustomsFileParent)invoice6).LockFile("HELLO");

			var collection = new AttachInvoiceCollection(testDec);

			AssertEquals("Loaded correctly", 6, collection.Count);
			Assert("Loaded correctly", collection.Contains(invoice1));
			Assert("Loaded correctly", collection.Contains(invoice2));
			Assert("Loaded correctly", collection.Contains(invoice3));
			Assert("Loaded correctly", collection.Contains(invoice6));

			Assert("Correct warning", invoice1.RowWarnings.ToUniqueMessageListString().IndexOf("message type") != -1);
			Assert("Correct warning", invoice2.RowWarnings.ToUniqueMessageListString().IndexOf("supplier") != -1);
			Assert("Correct warning", invoice3.RowWarnings.ToUniqueMessageListString().IndexOf("importer") != -1);
			Assert("No warnings", !invoice4.HasRowWarnings);
			Assert("Correct warning", invoice5.RowWarnings.ToUniqueMessageListString().IndexOf("message type") != -1);
			AssertHasRowError("invoice is locked", invoice6, "This invoice is Locked and cannot be added to a Customs Declaration until it has been Unlocked.");

			testDec.JE_MessageType = "IMP";
			invoice5.ClearRowNotifications();
			Assert("No warnings", !invoice5.HasRowWarnings);
		}

		public void TestMergeByList()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			AssertEquals("MergeByList.Count", 8, new JobDeclarationLookups(dec).MergeByList.Count);
		}

		public void TestOrganisationsFindBoxListDefaultFromUnmatchOrgNotes()
		{
			var jobDec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobDec.JE_TransportMode = Core.Constants.TransportModes.Rail;
			var helper = new UnmatchOrgRecordTestHelper(jobDec, ImporterRec, SupplierRec, CarrierRec, ForwarderRec);

			//Importer
			var importerList = jobDec.Lookups.ImportersList;
			helper.PopulateOrgDefaultsFromUnmatchedNote(jobDec.Lookups.GetType(), "ImportersList", importerList);
			AssertEquals("Forwarder should have conditional defaults from notes", true, importerList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(importerList.DefaultsForNewChild, ImporterRec);

			//Supplier
			var suppliersList = jobDec.Lookups.SuppliersList;
			helper.PopulateOrgDefaultsFromUnmatchedNote(jobDec.Lookups.GetType(), "SuppliersList", suppliersList);
			AssertEquals("Forwarder should have conditional defaults from notes", true, suppliersList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(suppliersList.DefaultsForNewChild, SupplierRec);

			//AirOrSeaShippingLineList
			var airOrSeaShippingLineList = jobDec.Lookups.AirOrSeaShippingLineList;
			helper.PopulateOrgDefaultsFromUnmatchedNote(jobDec.Lookups.GetType(), "AirOrSeaShippingLineList", airOrSeaShippingLineList);
			AssertEquals("Forwarder should have conditional defaults from notes", true, airOrSeaShippingLineList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(airOrSeaShippingLineList.DefaultsForNewChild, CarrierRec);

			//AirShippingLineList
			jobDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var airShippingLineList = jobDec.Lookups.AirShippingLineList;
			helper.PopulateOrgDefaultsFromUnmatchedNote(jobDec.Lookups.GetType(), "AirShippingLineList", airShippingLineList);
			AssertEquals("Forwarder should have conditional defaults from notes", true, airShippingLineList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(airShippingLineList.DefaultsForNewChild, CarrierRec);

			//SeaShippingLineList
			jobDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var seaShippingLineList = jobDec.Lookups.SeaShippingLineList;
			helper.PopulateOrgDefaultsFromUnmatchedNote(jobDec.Lookups.GetType(), "SeaShippingLineList", seaShippingLineList);
			AssertEquals("Forwarder should have conditional defaults from notes", true, seaShippingLineList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(seaShippingLineList.DefaultsForNewChild, CarrierRec);

			//Forwarder
			var forwarderList = jobDec.Lookups.ForwarderList;
			helper.PopulateOrgDefaultsFromUnmatchedNote(jobDec.Lookups.GetType(), "ForwarderList", forwarderList);
			AssertEquals("Forwarder should have conditional defaults from notes", true, forwarderList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(forwarderList.DefaultsForNewChild, ForwarderRec);
		}

		public void TestFinalDestinations()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, dec.CountryCode));
			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, dec.CountryCode));
			dec.JE_RL_NKOrigin = foreignPort.RL_Code;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var filter = dec.Lookups.FinalDestinations.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(false, foreignPort.MatchesFilter(filter));

			dec.JE_RL_NKOrigin = ZString.Empty;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(true, foreignPort.MatchesFilter(filter));

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			AssertEquals(false, localPort.MatchesFilter(filter));
			AssertEquals(true, foreignPort.MatchesFilter(filter));

			dec.JE_MessageType = "Z!Z";
			filter = dec.Lookups.FinalDestinations.CompleteFilter;
			AssertEquals(true, localPort.MatchesFilter(filter));
			AssertEquals(true, foreignPort.MatchesFilter(filter));
		}

		public void TestPortOfArrivals()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var lookup = new JobDeclarationLookups(dec);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = TransportTypeGenericList.Codes.Road;
			var ports = lookup.PortOfArrivals;

			var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, "ZMCGJ"));

			Assert("ZMCGJ is Road", ports.Contains(port));
		}

		public void TestApplicationCodeList()
		{
			var country1 = Factory.New<RefCountry>();
			country1.RN_Code = "A1";
			country1.RN_Desc = "A1 DESC";
			var country2 = Factory.New<RefCountry>();
			country2.RN_Code = "A2";
			country2.RN_Desc = "A2 DESC";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, "Asycuda Customs Country");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, "A1", "A1 Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().ResetCachingForTest();
			var declaration = Factory.New<BaseJobDeclaration>();
			var lookups = declaration.Lookups;
			AssertType<JobDeclarationLookups>(lookups);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				AssertEquals("PreCondition: CountryHasBuiltInDeclaration(NZ)", true, IntegratedCountryHelper.CountryHasBuiltInDeclaration(Core.Constants.CountryCodes.NewZealand));
				AssertSame(Factory.GetCachedValue<DeclarationApplicationCodeList>(), lookups.ApplicationCodeList);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Denmark))
			{
				AssertEquals("PreCondition: CountryHasDeclarationInDevelopment(DK)", true, IntegratedCountryHelper.CountryHasDeclarationInDevelopment(Core.Constants.CountryCodes.Denmark));
				AssertSame(Factory.GetCachedValue<DeclarationApplicationCodeList>(), lookups.ApplicationCodeList);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("A1"))
			{
				AssertEquals("PreCondition: IsAsycudaCustomsCountryCode(A1)", true, IntegratedCountryHelper.IsAsycudaCustomsCountryCode("A1"));
				AssertSame(Factory.GetCachedValue<DeclarationApplicationCodeList>(), lookups.ApplicationCodeList);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("A2"))
			{
				AssertEquals("PreCondition: CountryHasBuiltInDeclaration(A2)", false, IntegratedCountryHelper.CountryHasBuiltInDeclaration("A2"));
				AssertEquals("PreCondition: CountryHasDeclarationInDevelopment(A2)", false, IntegratedCountryHelper.CountryHasDeclarationInDevelopment("A2"));
				AssertEquals("PreCondition: IsAsycudaCustomsCountryCode(A2)", false, IntegratedCountryHelper.IsAsycudaCustomsCountryCode("A2"));
				var applicationCodeList = lookups.ApplicationCodeList;
				AssertEquals("applicationCodeList.Count", 1, applicationCodeList.Count);
				AssertEquals("applicationCodeList.GetDescriptionFromCode(DeclarationApplicationCodeList.Codes.Interfaced)", DeclarationApplicationCodeList.Descriptions.Interfaced, applicationCodeList.GetDescriptionFromCode(DeclarationApplicationCodeList.Codes.Interfaced));
				AssertSame(applicationCodeList, lookups.ApplicationCodeList);
			}
		}

		public void TestApplicationCodeList_ITFOnlyCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Vatican))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var lookups = declaration.Lookups;
				var applicationCodeList = lookups.ApplicationCodeList;
				AssertEquals("applicationCodeList.Count", 1, applicationCodeList.Count);
				AssertEquals("Description", DeclarationApplicationCodeList.Descriptions.Interfaced, applicationCodeList.GetDescriptionFromCode(DeclarationApplicationCodeList.Codes.Interfaced));
				AssertSame(applicationCodeList, lookups.ApplicationCodeList);
				AssertSame("ITFOnlyApplicationCodeList", applicationCodeList, lookups.ITFOnlyApplicationCodeList);
			}
		}

		public void TestDistributors()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertType<OrgHeaderCollection>(declaration.Lookups.Distributors);
		}

		public void TestShippers()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertType<OrgHeaderCollection>(declaration.Lookups.Shippers);
		}

		public void TestPackagers()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertType<OrgHeaderCollection>(declaration.Lookups.Packagers);
		}

		public void TestDeclarantOfficeList()
		{
			var lookups = new JobDeclarationLookups(Factory.New<BaseJobDeclaration>());
			var list = lookups.DeclarantOfficeList;
			AssertType<OrganisationsFindBoxCollection>(list);
			AssertEquals("DeclarantOfficeList should not be loaded by the property", false, list.IsLoaded);
		}

		BaseJobComInvoiceHeader GetNewInvoiceMatchingDec(BaseJobDeclaration dec)
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = dec.JE_MessageType;
			invoice.JZ_OH_Buyer = dec.JE_OH_Importer;
			invoice.JZ_OH_Supplier = dec.JE_OH_Supplier;
			return invoice;
		}

		#region Implementation

		UnmatchOrgRecord ImporterRec
		{
			get
			{
				if (importerRec == null)
				{
					importerRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Consignee,
						nameof(OrganisationTypes.Consignee),
						"importer addr 1",
						"importer addr 2",
						"importerName",
						"2222",
						"NSW",
						"sydney",
						"importer",
						"impOwnerCode",
						"");
				}
				return importerRec;
			}
		}
		UnmatchOrgRecord importerRec;

		UnmatchOrgRecord SupplierRec
		{
			get
			{
				if (supplierRec == null)
				{
					supplierRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Consignor,
						nameof(OrganisationTypes.Consignor),
						"supplier addr 1",
						"supplier addr 2",
						"supplierName",
						"1111",
						"NSW",
						"city",
						"supplier",
						"SupownerCode",
						"");
				}
				return supplierRec;
			}
		}
		UnmatchOrgRecord supplierRec;

		UnmatchOrgRecord CarrierRec
		{
			get
			{
				if (carrierRec == null)
				{
					carrierRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Carrier,
						nameof(OrganisationTypes.Carrier),
						"carrier addr 1",
						"carrier addr 2",
						"carrierName",
						"3333",
						"VIC",
						"city",
						"carrier",
						"CarownerCode",
						"");
				}
				return carrierRec;
			}
		}
		UnmatchOrgRecord carrierRec;

		UnmatchOrgRecord ForwarderRec
		{
			get
			{
				if (forwarderRec == null)
				{
					forwarderRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Forwarder,
						nameof(OrganisationTypes.Forwarder),
						"forwarder addr 1",
						"forwarder addr 2",
						"forwarderName",
						"4444",
						"VIC",
						"city",
						"forwarder",
						"ForwarderCode",
						"");
				}
				return forwarderRec;
			}
		}
		UnmatchOrgRecord forwarderRec;

		#endregion
	}
}
