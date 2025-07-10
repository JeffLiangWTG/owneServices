using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business
{
	[HttpContextEnabledTest]
	sealed class OrgRestrictionFilterFactoryTest : TestCaseWithFactory
	{
		public void Instance()
		{
			AssertNotNull("OrgRestrictionFilterFactory.Instance should not return null", OrgRestrictionFilterFactory.Instance);
		}

		public void TestBillOfLadingsOrgRestrictionFilter()
		{
			BillOfLading testBOL1 = Factory.NewWithValidTestData<BillOfLading>();
			BillOfLading testBOL2 = Factory.NewWithValidTestData<BillOfLading>();
			BillOfLading testBOL3 = Factory.NewWithValidTestData<BillOfLading>();

			ZWebTestHelper helper = new ZWebTestHelper(Factory);
			OrgHeader testOrg = helper.TestOrg;
			testOrg.OH_Code = "XXXYYYZZZ";
			testOrg.OH_IsForwarder = true;
			OrgContact testContact = helper.TestContact;
			testContact.OC_Email = "test@testcompany.com";
			testContact.SetHashedPassword("testpassword");
			testContact.OC_WebAccessEnabled = true;

			testBOL1.JS_OH_DeliveryAgent = testOrg.PK;

			testBOL3.DocAddresses.AddNew(testOrg.MainAddress, DocAddressTypes.GetDocAddressTypeFromCode(Factory, AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress));

			Factory.Save();

			WebEnv.AppInstance.SiteUser.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			AssertEquals("Logged in user should be TestContact", testContact.PK, WebEnv.AppInstance.SiteUser.LoggedInUser.PK);

			BillOfLading[] results = Factory.Load<BillOfLading>(OrgRestrictionFilterFactory.Instance.GetFilter<BillOfLading>());
			AssertEquals(2, results.Length);
			Assert("Expected BillOfLadding 1", results[0] == testBOL1 || results[1] == testBOL1);
			Assert("Expected BillOfLadding 3", results[0] == testBOL3 || results[1] == testBOL3);
		}

		public void TestTrackingOrderOrgRestrictionFilter()
		{
			var order1 = Factory.NewWithValidTestData<TrackingOrder>();
			var order2 = Factory.NewWithValidTestData<TrackingOrder>();
			var order3 = Factory.NewWithValidTestData<TrackingOrder>();
			var order4 = Factory.NewWithValidTestData<TrackingOrder>();
			var order5 = Factory.NewWithValidTestData<TrackingOrder>();
			var order6 = Factory.NewWithValidTestData<TrackingOrder>();
			var order7 = Factory.NewWithValidTestData<TrackingOrder>();

			var helper = new ZWebTestHelper(Factory);
			var org = helper.TestOrg;
			org.OH_Code = "XXXYYYZZZ";
			org.OH_IsForwarder = true;

			var contact = helper.TestContact;
			contact.OC_Email = "test@testcompany.com";
			contact.SetHashedPassword("testpassword");
			contact.OC_WebAccessEnabled = true;

			order1.BuyerPK = org.PK;
			order2.SupplierPK = org.PK;
			order3.JD_OH_Carrier = org.PK;
			order4.JD_OH_ReceivingAgent = org.PK;
			order5.JD_OH_SendingAgent = org.PK;
			order6.DocAddresses.AddNew(org.MainAddress, DocAddressTypes.GetDocAddressTypeFromCode(Factory, AutoDocAddressTypes.Codes.ControllingCustomer));

			Factory.Save();

			WebEnv.AppInstance.SiteUser.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			AssertEquals("Logged in user should be contact", contact.PK, WebEnv.AppInstance.SiteUser.LoggedInUser.PK);

			var results = Factory.Load<TrackingOrder>(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingOrder>());
			AssertEquals(6, results.Length);
			AssertContainsExactElementsInAnyOrder(new[] { order1.PK, order2.PK, order3.PK, order4.PK, order5.PK, order6.PK }, results.Select(x => x.PK));
		}

		public void TestLoadContainersFilteredByRelatedStandAloneDeclaration()
		{
			bool result = false;
			TestHelper helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			//Helper.TestSiteUser.Logout();

			var declaration = Factory.New<Customs.US.Business.JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_OH_Forwarder = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, helper.TestOrg.PK)).PK;

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "N123";
			cusContainer.CO_Seal = "S123";
			cusContainer.CO_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			ZGuid matchingContainerPK = cusContainer.JobContainer.PK;

			Factory.Save();

			var containersResult = Factory.Load<TrackingContainer>(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingContainer>());
			foreach (var container in containersResult)
			{
				result = container.PK == matchingContainerPK;
				if (result)
				{
					break;
				}
			}
			Assert("Should not have MatchingConteiner because declaration is not related", !result);

			declaration.JE_OH_Supplier = helper.TestOrg.PK;
			Factory.Save();

			containersResult = Factory.Load<TrackingContainer>(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingContainer>());
			foreach (var container in containersResult)
			{
				result = container.PK == matchingContainerPK;
				if (result)
				{
					break;
				}
			}
			Assert("Should have container because declaration is related", result);
		}

		public void TestLoadFilteredByContact()
		{
			TestHelper helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			helper.TestSiteUser.Logout();

			TrackingOrder matchingOrder = Factory.New<TrackingOrder>();
			matchingOrder.JD_OrderNumber = "S123456789";
			matchingOrder.SupplierPK = helper.TestOrg.PK;
			matchingOrder.BuyerPK = helper.TestOrg.PK;

			OrgHeader incorrectOrgHeader = Factory.NewWithValidTestData<OrgHeader>();

			TrackingOrder nonMatchingOrder = Factory.New<TrackingOrder>();
			nonMatchingOrder.JD_OrderNumber = "S123456790";
			nonMatchingOrder.SupplierPK = incorrectOrgHeader.PK;
			nonMatchingOrder.BuyerPK = incorrectOrgHeader.PK;

			Factory.Save();

			AssertNull(OrgRestrictionFilterFactory.LoadFilteredByContact<TrackingOrder>(Factory, JobOrderHeaderSchema.PK, matchingOrder.PK));
			AssertNull(OrgRestrictionFilterFactory.LoadFilteredByContact<TrackingOrder>(Factory, JobOrderHeaderSchema.PK, matchingOrder.PK));

			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			TrackingOrder orderFromPK = OrgRestrictionFilterFactory.LoadFilteredByContact<TrackingOrder>(Factory, JobOrderHeaderSchema.PK, matchingOrder.PK);

			AssertEquals(matchingOrder, orderFromPK);
			AssertEquals("Order AutoCreatedLogDefaultSL_Reference", helper.TestSiteUser.ContactAndCompanyReference, orderFromPK.Logs.AutoCreatedLogDefaultSL_Reference);

			AssertNull(OrgRestrictionFilterFactory.LoadFilteredByContact<TrackingOrder>(Factory, JobOrderHeaderSchema.PK, nonMatchingOrder.PK));
		}

		public void TestSkipOHFields()
		{
			AssertQueryHasNoOHFields<TrackingShipment>();
			AssertQueryHasNoOHFields<TrackingDeclaration>();
			AssertQueryHasNoOHFields<BaseJobDeclaration>();
			AssertQueryHasNoOHFields<TrackingOrder>();
			AssertQueryHasNoOHFields<InvoicingBase>();
			AssertQueryHasNoOHFields<OrgPartRelation>();
			AssertQueryHasNoOHFields<TrackingContainer>();
			AssertQueryHasNoOHFields<AgencyBooking>();
			AssertQueryHasNoOHFields<TrackingLinerAndAgencyBooking>();
			AssertQueryHasNoOHFields<BillOfLading>();
			AssertQueryHasNoOHFields<TrackingBillOfLading>();

			AssertQueryHasOHFields<TrackingCFSShipment>();
		}

		public void AssertQueryHasNoOHFields<T>()
			where T : BusinessObject
		{
			var query = OrgRestrictionFilterFactory.Instance.GetFilter<T>();
			var sqlText = query.LiteralTextADO;
			var field = Freight.Common.Business.AutoJobShipment.Schema.JS_OH_HandledOnBehalfOfForwarder;
			var hasField = sqlText.Contains($" {field} ") || sqlText.Contains($".{field} ");

			AssertEquals($"The following field should not be used for filtering web data of type {typeof(T)}: {System.Environment.NewLine}{field}", false, hasField);
		}

		public void AssertQueryHasOHFields<T>()
			where T : BusinessObject
		{
			var query = OrgRestrictionFilterFactory.Instance.GetFilter<T>();
			var sqlText = query.LiteralTextADO;
			var field = Freight.Common.Business.AutoJobShipment.Schema.JS_OH_HandledOnBehalfOfForwarder;
			var hasField = sqlText.Contains($" {field} ") || sqlText.Contains($".{field} ");

			AssertEquals($"The following field should be used for filtering web data of type {typeof(T)}: {System.Environment.NewLine}{field}", true, hasField);
		}

		public void TestFiltersDoNotReturnNull()
		{
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<BaseJobDeclaration>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingOrder>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingWhsOrder>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingWhsInventory>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<InvoicingBase>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<ForwardingConsol>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<OrgPartRelation>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingContainer>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<AgencyBooking>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingLinerAndAgencyBooking>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<BillOfLading>());
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingBillOfLading>());

			try
			{
				AssertEquals("No developer errors currently reported", "", ErrorReporter.LastMessageReported);
				ZQuery unknownBizOFilter = OrgRestrictionFilterFactory.Instance.GetFilter<BusinessObject>();
				AssertNotNull(unknownBizOFilter);
				AssertEquals("Should be NoResultQuery", true, unknownBizOFilter.IsNoResultQuery);
				AssertEquals("Should have reported a developer error", "OrgRestrictionFilterGetColumns_CargoWise.EntityFramework.BusinessObject", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestFilterReturnCorrectColumnCount()
		{
			AssertEquals("Number of columns return differ from those expected", JobShipmentSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(TrackingShipment)).Count);
			AssertEquals("Number of columns return differ from those expected", JobDeclarationSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(TrackingDeclaration)).Count);
			AssertEquals("Number of columns return differ from those expected", JobDeclarationSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(BaseJobDeclaration)).Count);
			AssertEquals("Number of columns return differ from those expected", JobOrderHeaderSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(TrackingOrder)).Count);
			AssertEquals("Number of columns return differ from those expected", WhsDocketSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(TrackingWhsOrder)).Count);
			AssertEquals("Number of columns return differ from those expected", WhsInventoryViewSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(TrackingWhsInventory)).Count);
			AssertEquals("Number of columns return differ from those expected", AccTransactionHeaderSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(InvoicingBase)).Count);
			AssertEquals("Number of columns return differ from those expected", JobConsolSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(ForwardingConsol)).Count);
			AssertEquals("Number of columns return differ from those expected", OrgPartRelationSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(OrgPartRelation)).Count);
			AssertEquals("Number of columns return differ from those expected", WhsDocketSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(TrackingWhsReceive)).Count);
			AssertEquals("Number of columns return differ from those expected", JobContainerSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(TrackingContainer)).Count);
			AssertEquals("Number of columns return differ from those expected", JobShipmentSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(AgencyBooking)).Count);
			AssertEquals("Number of columns return differ from those expected", JobShipmentSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(TrackingLinerAndAgencyBooking)).Count);
			AssertEquals("Number of columns return differ from those expected", JobShipmentSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(BillOfLading)).Count);
			AssertEquals("Number of columns return differ from those expected", JobShipmentSchema.All.Count, OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(TrackingBillOfLading)).Count);

			try
			{
				AssertEquals("No developer errors currently reported", "", ErrorReporter.LastMessageReported);
				AssertNull("Number of columns return differ from those expected", OrgRestrictionFilterFactory.Instance.GetAllSchemaColumnsForType(typeof(BusinessObject)));
				AssertEquals("Should have reported a developer error", "OrgRestrictionFilterGetColumns_CargoWise.EntityFramework.BusinessObject", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestGetSubQuery()
		{
			AssertNotNull(OrgRestrictionFilterFactory.Instance.GetSubQuery(typeof(MasterFiles.Business.OrgSupplierPart), typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP));
		}

		public void TestGetFilterForForwardingConsol()
		{
			ZQuery filter = OrgRestrictionFilterFactory.Instance.GetFilter<ForwardingConsol>();

			AssertJobConsolQuery(filter.LiteralTextADO);
		}

		public void TestGetFilterForForwardingConsol_OrgAddressSubQuery()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("0");

				var query = OrgRestrictionFilterFactory.Instance.GetFilter<ForwardingConsol>();

				CombineAssertions(() =>
				{
					settings.TVPRule = new TVPRule("0");
					AssertOrganisationRelatedOrgAddressPKs(query, JobConsolSchema.JK_OA_SendingForwarderAddress);
					AssertOrganisationRelatedOrgAddressPKs(query, JobConsolSchema.JK_OA_ReceivingForwarderAddress);
				});
			}
		}

		public void TestGetJobConsolSubQuery()
		{
			ZDBOnlySubQuery subQuery = OrgRestrictionFilterFactory.Instance.GetJobConsolSubQuery();
			AssertNotNull(subQuery);
			Assert("Has additional subqueries", subQuery.LiteralTextADO.Contains(JobConShipLinkSchema.Constants.TableName) && subQuery.LiteralTextADO.Contains(JobConsolSchema.Constants.TableName));

			AssertJobConsolQuery(subQuery.LiteralTextADO);
		}

		public void TestGetJobContainerSubQuery()
		{
			ZQuery subQuery = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingContainer>();

			AssertNotNull(subQuery);

			Assert("Includes Consol SubQuery", subQuery.LiteralTextADO.Contains(JobContainerSchema.Constants.TableName));
			Assert("Includes Shipment SubQuery", subQuery.LiteralTextADO.Contains(JobShipmentSchema.Constants.TableName));
			Assert("Includes Orders SubQuery", subQuery.LiteralTextADO.Contains(JobOrderHeaderSchema.Constants.TableName));
		}

		public void TestGetJobContainerSubQuery_OrgAddressSubQuery()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("0");

				var subQuery = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingContainer>();

				CombineAssertions(() =>
				{
					AssertOrganisationRelatedOrgAddressPKs(subQuery, JobConsolSchema.JK_OA_SendingForwarderAddress);
					AssertOrganisationRelatedOrgAddressPKs(subQuery, JobConsolSchema.JK_OA_ReceivingForwarderAddress);
					AssertOrganisationRelatedOrgAddressPKs(subQuery, JobConsolSchema.JK_OA_ShippingLineAddress);
					AssertOrganisationRelatedOrgAddressPKs(subQuery, JobHeaderSchema.JH_OA_LocalChargesAddr);
					AssertOrganisationRelatedOrgAddressPKs(subQuery, JobHeaderSchema.JH_OA_AgentCollectAddr);
					AssertOrganisationRelatedOrgAddressPKs(subQuery, JobOrderHeaderSchema.JD_OA_BuyerAddress);
					AssertOrganisationRelatedOrgAddressPKs(subQuery, JobOrderHeaderSchema.JD_OA_SupplierAddress);
				});
			}
		}

		public void TestGetJobContainerQuery_DoNotLoadBlobs()
		{
			var subQuery = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingContainer>();

			AssertEquals(0, subQuery.LoadSmallBlobs);
		}

		public void TestGetLinerAndAgencyContainerSubQuery()
		{
			var subQuery = OrgRestrictionFilterFactory.Instance.GetFilter<LinerAndAgencyContainer>();

			AssertNotNull(subQuery);

			Assert("Includes Shipment SubQuery", subQuery.LiteralTextADO.Contains(JobShipmentSchema.Constants.TableName));
		}

		public void TestGetControllingCustomerSubQuery()
		{
			ZDBOnlyQuery controllingCustomerSubQuery = OrgRestrictionFilterFactory.Instance.GetControllingCustomerSubQuery();
			ZString controllingCustomerAddressType = AutoDocAddressTypes.Codes.ControllingCustomer;

			AssertNotNull(controllingCustomerSubQuery);

			Assert("Is JobDocAddress Subquery", controllingCustomerSubQuery.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
			Assert("Includes OCP Address Type Condition", controllingCustomerSubQuery.LiteralTextADO.Contains(controllingCustomerAddressType));
		}

		public void TestGetShipmentQuery()
		{
			var cachedRegistryValue = WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.Value;

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var query = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>();
			AssertNotNull(query);
			Assert("Includes JobHeader SubQuery", query.LiteralTextADO.Contains(JobHeaderSchema.Constants.TableName));
			Assert("Includes JobConShipLink SubQuery", query.LiteralTextADO.Contains(JobConShipLinkSchema.Constants.TableName));
			Assert("Includes JobDocAddress SubQuery", query.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
			Assert("Includes SCP Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.ControllingCustomer));
			Assert("Includes CED Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress));
			Assert("Includes CEG Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress));
			Assert("Includes NPP Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.NotifyParty));
			Assert("Includes N2D Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.NotifyParty2));
			Assert("Includes N3D Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.NotifyParty3));
			Assert("Includes BKD Address Type Condition", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress));

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			query = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>();
			AssertNotNull(query);
			Assert("Does not include SCP Address Type Condition", !query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.ControllingCustomer));

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryValue);
		}

		public void TestGetShipmentQuery_OrgAddressSubQuery()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("0");
				var query = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingShipment>();

				CombineAssertions(() =>
				{
					AssertOrganisationRelatedOrgAddressPKs(query, JobHeaderSchema.JH_OA_LocalChargesAddr);
					AssertOrganisationRelatedOrgAddressPKs(query, JobHeaderSchema.JH_OA_AgentCollectAddr);
				});
			}
		}

		public void TestGetAgencyShipmentQuery()
		{
			var query = OrgRestrictionFilterFactory.Instance.GetFilter<AgencyShipment>();

			AssertNotNull(query);

			Assert("Includes JobDocAddress SubQuery", query.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
			Assert("Includes JobHeader SubQuery", query.LiteralTextADO.Contains(JobHeaderSchema.Constants.TableName));
		}

		public void TestGetBookingQuery()
		{
			ZQuery query = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingBooking>();

			AssertNotNull(query);

			Assert("Includes JobDocAddress SubQuery", query.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
			Assert("Includes JobHeader SubQuery", query.LiteralTextADO.Contains(JobHeaderSchema.Constants.TableName));
			Assert("Includes RatingHeader SubQuery", query.LiteralTextADO.Contains(RatingHeaderSchema.Constants.TableName));
			Assert("Includes JobShipment Delivery Agent", query.LiteralTextADO.Contains(JobShipmentSchema.Constants.JS_OH_DeliveryAgent));
			Assert("Includes JobShipment Export Broker", query.LiteralTextADO.Contains(JobShipmentSchema.Constants.JS_OH_ExportBroker));
			Assert("Includes Pickup Agent address", query.LiteralTextADO.Contains(AutoDocAddressTypes.Codes.PickupAgent));
			Assert("Should allow deactivated bookings", !query.LiteralTextADO.Contains(string.Format("{0} = 0", JobShipmentSchema.Constants.JS_IsCancelled)));
		}

		public void TestGetDeclarationQuery()
		{
			var helper = new ZWebTestHelper(Factory);
			var testOrg = helper.TestOrg;
			testOrg.OH_Code = "XXXYYYZZZ";
			testOrg.OH_IsForwarder = true;
			OrgContact testContact = helper.TestContact;
			testContact.OC_Email = "test@testcompany.com";
			testContact.SetHashedPassword("testpassword");
			testContact.OC_WebAccessEnabled = true;

			var declarationSupplier = GetNewTrackingDeclaration();
			declarationSupplier.Declaration.JE_OH_Supplier = testOrg.PK;
			declarationSupplier.Declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			var declarationImporter = GetNewTrackingDeclaration();
			declarationImporter.Declaration.JE_OH_Importer = testOrg.PK;
			declarationImporter.Declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var declarationForwarder = GetNewTrackingDeclaration();
			declarationForwarder.Declaration.JE_OH_Forwarder = testOrg.PK;

			var declarationShippingLine = GetNewTrackingDeclaration();
			declarationShippingLine.Declaration.JE_OH_ShippingLine = testOrg.PK;

			var declarationBillToParty = GetNewTrackingDeclaration();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "hello";
			job.JH_ParentID = declarationBillToParty.Declaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			declarationBillToParty.Declaration.Job.JH_OA_LocalChargesAddr = testOrg.MainAddress.PK;

			var declarationIOR = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declarationIOR.JE_OA_DeclarantAddress = testOrg.MainAddress.PK;
			declarationIOR.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			Factory.Save();

			WebEnv.AppInstance.SiteUser.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			AssertEquals("Logged in user should be TestContact", testContact.PK, WebEnv.AppInstance.SiteUser.LoggedInUser.PK);

			var resultsPks = Factory.Load<BaseJobDeclaration>(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>()).Select(declaration => declaration.PK).ToArray();
			AssertEquals(3, resultsPks.Length);
			AssertCollectionContains(declarationSupplier.Declaration.PK, resultsPks);
			AssertCollectionContains(declarationImporter.Declaration.PK, resultsPks);
			AssertCollectionContains(declarationBillToParty.Declaration.PK, resultsPks);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetDeclarationQuery_CanGetShipmentWithLocalClientMatched()
		{
			ZArchitecture.Environment.Globals.IsWeb = true;
			var helper = new ZWebTestHelper(Factory);
			var loginOrg = helper.TestOrg;
			loginOrg.OH_Code = "XXXYYYZZZ";
			loginOrg.OH_IsForwarder = true;
			var loginUser = helper.TestContact;
			loginUser.OC_Email = "test@testcompany.com";
			loginUser.SetHashedPassword("testpassword");
			loginUser.OC_WebAccessEnabled = true;
			var loginOrgAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, loginOrg.PK));

			//AU Org, company & branch
			var orgAU = Factory.NewWithValidTestData<OrgHeader>();
			orgAU.OH_Code = "XYZ AU";
			orgAU.OH_RL_NKClosestPort = "AUMEL";
			var companyAU = Factory.NewWithValidTestData<GlbCompany>();
			companyAU.GC_OH_OrgProxy = orgAU.PK;
			companyAU.GC_RN_NKCountryCode = "AU";
			var branchAU = Factory.NewWithValidTestData<GlbBranch>();
			companyAU.Branches.Add(branchAU);
			branchAU.GB_OH_OrgProxy = orgAU.PK;
			branchAU.GB_RL_NKHomePort = "AUMEL";

			//US Org, company & branch
			var orgUS = Factory.NewWithValidTestData<OrgHeader>();
			orgUS.OH_Code = "ABC US";
			orgAU.OH_RL_NKClosestPort = "USNYC";
			var companyUS = Factory.NewWithValidTestData<GlbCompany>();
			companyUS.GC_OH_OrgProxy = orgUS.PK;
			companyUS.GC_RN_NKCountryCode = "US";
			var branchUS = Factory.NewWithValidTestData<GlbBranch>();
			companyUS.Branches.Add(branchUS);
			branchUS.GB_OH_OrgProxy = orgUS.PK;
			branchUS.GB_RL_NKHomePort = "USNYC";

			Factory.Save();

			//Shipment
			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgAU.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = orgUS.PK;

			//Shipment's AU Job
			var jobAU = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobAU.JH_JobNum = "AU Job";
			jobAU.JH_ParentTableCode = "JS";
			jobAU.JH_ParentID = shipment.PK;
			jobAU.JH_GB = branchAU.PK;
			jobAU.JH_GC = companyAU.PK;

			//Shipment's AU declaration			
			var decAU = Factory.NewWithValidTestData<BaseJobDeclaration>();
			decAU.JE_JS = shipment.PK;
			decAU.JE_OH_Importer = orgAU.PK;
			decAU.JE_GB = branchAU.PK;
			decAU.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			decAU.DontReAssignReferenceNoForUnitTest = true;

			//Shipment's US Job
			var jobUS = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobUS.JH_ParentTableCode = "JS";
			jobUS.JH_ParentID = shipment.PK;
			jobUS.JH_GB = branchUS.PK;
			jobUS.JH_GC = companyUS.PK;

			//Shipment's US declaration
			var decUS = Factory.NewWithValidTestData<BaseJobDeclaration>();
			decUS.JE_JS = shipment.PK;
			decUS.JE_OH_Supplier = orgUS.PK;
			decUS.JE_GB = branchUS.PK;
			decUS.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			decUS.DontReAssignReferenceNoForUnitTest = true;

			Factory.Save();

			WebEnv.AppInstance.SiteUser.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			AssertEquals("Logged in user should be TestContact", loginUser.PK, WebEnv.AppInstance.SiteUser.LoggedInUser.PK);

			var trackingDeclarationCollection = new TrackingDeclarationCollection(Factory);

			//AU job with login org as local client
			jobAU.JH_OA_LocalChargesAddr = loginOrgAddress.PK;
			Factory.Save();
			trackingDeclarationCollection.Load((OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>()));
			var resultsPks = trackingDeclarationCollection.Cast<TrackingDeclaration>().Select(trackDec => trackDec.Declaration.PK);
			AssertCollectionContains("AU shipment declaration should be shown", decAU.PK, resultsPks);
			AssertCollectionNotContains("US shipment declaration should not be shown", decUS.PK, resultsPks);

			//Both AU & US jobs with login org as local client
			jobUS.JH_OA_LocalChargesAddr = loginOrgAddress.PK;
			Factory.Save();
			trackingDeclarationCollection.Load((OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>()));
			resultsPks = trackingDeclarationCollection.Cast<TrackingDeclaration>().Select(trackDec => trackDec.Declaration.PK);
			AssertCollectionContains("AU shipment declaration should be shown", decAU.PK, resultsPks);
			AssertCollectionContains("US shipment declaration should be shown", decUS.PK, resultsPks);

			//Only US job with login org as local client
			jobAU.JH_OA_LocalChargesAddr = ZGuid.Empty;
			Factory.Save();
			trackingDeclarationCollection.Load((OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>()));
			resultsPks = trackingDeclarationCollection.Cast<TrackingDeclaration>().Select(trackDec => trackDec.Declaration.PK);
			AssertCollectionNotContains("AU shipment declaration should not be shown", decAU.PK, resultsPks);
			AssertCollectionContains("US shipment declaration should be shown", decUS.PK, resultsPks);

			//none with login org as local client
			jobUS.JH_OA_LocalChargesAddr = ZGuid.Empty;
			Factory.Save();
			trackingDeclarationCollection.Load((OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>()));
			resultsPks = trackingDeclarationCollection.Cast<TrackingDeclaration>().Select(trackDec => trackDec.Declaration.PK);
			AssertCollectionNotContains("AU shipment declaration should not be shown", decAU.PK, resultsPks);
			AssertCollectionNotContains("US shipment declaration should not be shown", decUS.PK, resultsPks);
		}

		public void TestGetDeclarationQuery_USImporter()
		{
			var helper = new ZWebTestHelper(Factory);
			var loginOrg = helper.TestOrg;
			loginOrg.OH_Code = "XXXYYYZZZ";
			loginOrg.OH_IsForwarder = true;
			var loginUser = helper.TestContact;
			loginUser.OC_Email = "test@testcompany.com";
			loginUser.SetHashedPassword("testpassword");
			loginUser.OC_WebAccessEnabled = true;

			var branchUs = Factory.NewWithValidTestData<GlbBranch>();
			branchUs.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var declarationUs = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declarationUs.JE_OA_DeclarantAddress = loginOrg.MainAddress.PK;
			declarationUs.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declarationUs.JE_GB = branchUs.PK;

			var branchNonUs = Factory.NewWithValidTestData<GlbBranch>();
			branchNonUs.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;

			var declarationNonUs = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declarationNonUs.JE_OA_DeclarantAddress = loginOrg.MainAddress.PK;
			declarationNonUs.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declarationNonUs.JE_GB = branchNonUs.PK;

			Factory.Save();

			WebEnv.AppInstance.SiteUser.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			AssertEquals("Logged in user should be TestContact", loginUser.PK, WebEnv.AppInstance.SiteUser.LoggedInUser.PK);

			var trackingDeclarationCollection = new TrackingDeclarationCollection(Factory);
			trackingDeclarationCollection.Load((OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>()));
			var resultsPks = trackingDeclarationCollection.Cast<TrackingDeclaration>().Select(trackDec => trackDec.Declaration.PK);
			AssertCollectionContains("US importer of record declaration should be shown", declarationUs.PK, resultsPks);
			AssertCollectionNotContains("Non-US importer of record declaration should not be shown", declarationNonUs.PK, resultsPks);
		}

		TrackingDeclaration GetNewTrackingDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			return new TrackingDeclaration(declaration);
		}

		public void TestGetTrackingOrderFilter()
		{
			ZQuery trackingOrderFilter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingOrder>();

			AssertNotNull(trackingOrderFilter);

			Assert("TrackingOrderFilter should include JobDocAddress SubQuery", trackingOrderFilter.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
		}

		public void TestGetTrackingOrderFilter_OrgAddressSubQuery()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("0");
				var query = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingOrder>();

				CombineAssertions(() =>
				{
					AssertOrganisationRelatedOrgAddressPKs(query, JobOrderHeaderSchema.JD_OA_BuyerAddress);
					AssertOrganisationRelatedOrgAddressPKs(query, JobOrderHeaderSchema.JD_OA_SupplierAddress);
				});
			}
		}

		public void TestGetAgencyBookingFilter()
		{
			ZQuery agencyBookingFilter = OrgRestrictionFilterFactory.Instance.GetFilter<AgencyBooking>();

			AssertNotNull(agencyBookingFilter);

			Assert("AgencyBookingFilter should include JobDocAddress SubQuery", agencyBookingFilter.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
			//Assert("AgencyBookingFilter should include OrgAddress SubQuery", AgencyBookingFilter.LiteralTextADO.Contains(OrgAddressSchema.Constants.TableName));
		}

		public void TestGetTrackingLinerAndAgencyBookingFilter()
		{
			var trackingLinerAndAgencyBookingFilter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingLinerAndAgencyBooking>();

			AssertNotNull(trackingLinerAndAgencyBookingFilter);

			Assert("TrackingLinerAndAgencyBookingFilter should include JobDocAddress SubQuery", trackingLinerAndAgencyBookingFilter.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
		}

		public void TestGetAgencyBillOfLadingFilter()
		{
			ZQuery agencyBillOfLadingFilter = OrgRestrictionFilterFactory.Instance.GetFilter<BillOfLading>();

			AssertNotNull(agencyBillOfLadingFilter);

			Assert(String.Format("AgencyBookingFilter should include JobDocAddress SubQuery /r/n{0}", agencyBillOfLadingFilter.LiteralTextADO), agencyBillOfLadingFilter.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
			//Assert("AgencyBookingFilter should include OrgAddress SubQuery", AgencyBillOfLadingFilter.LiteralTextADO.Contains(OrgAddressSchema.Constants.TableName));
			Assert("AgencyBookingFilter should include JobHeader SubQuery", agencyBillOfLadingFilter.LiteralTextADO.Contains(JobHeaderSchema.Constants.TableName));
		}

		public void TestGetTrackingBillOfLadingFilter()
		{
			ZQuery trackingBillOfLadingFilter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingBillOfLading>();

			AssertNotNull(trackingBillOfLadingFilter);

			Assert(String.Format("TrackingBookingFilter should include JobDocAddress SubQuery /r/n{0}", trackingBillOfLadingFilter.LiteralTextADO), trackingBillOfLadingFilter.LiteralTextADO.Contains(JobDocAddressSchema.Constants.TableName));
			Assert("TrackingBookingFilter should include JobHeader SubQuery", trackingBillOfLadingFilter.LiteralTextADO.Contains(JobHeaderSchema.Constants.TableName));
		}

		public void TestGetColumnsReportsDeveloperExceptionAndReturnsNoResultQueryOnUnknownBizO()
		{
			try
			{
				AssertEquals("No developers errors currently reported", "", ErrorReporter.LastMessageReported);
				ZQuery filter = OrgRestrictionFilterFactory.Instance.GetFilter<DummyBusinessObject>();
				AssertEquals("A developer error should have been reported.", "OrgRestrictionFilterGetColumns_CargoWise.EntityFramework.Testing.DummyBusinessObject", ErrorReporter.LastKeyReported);
				ZString expectedMessage = "OrgRestrictionFilterFactory is not aware of the type CargoWise.EntityFramework.Testing.DummyBusinessObject. Please add column selection logic to OrgRestrictionFilterFactory to return the correct columns to filter by.";
				AssertEquals("A developer error should have been reported.", expectedMessage, ErrorReporter.LastMessageReported);
				AssertEquals("The Filter returned should be a NoResultQuery", true, filter.IsNoResultQuery);
				ZQuery mainFilter = new ZQuery();
				mainFilter.AddToFilter(filter);
				AssertEquals("Adding this filter to any other filter should make it a NoResultQuery", true, mainFilter.IsNoResultQuery);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#region TestGetOrderedProhibitedWarehouseQuery()

		public void TestGetOrderedProhibitedWarehouseQuery()
		{
			var envHelper = new WhsTestHelperFunctionsEnv(Factory);
			var staffWarehouse = envHelper.CreateWarehouse("STAFF", "STF", "AA");
			staffWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var groupWarehouse = envHelper.CreateWarehouse("GROUP", "GRP", "BB");
			groupWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var badWarehouse = envHelper.CreateWarehouse("BAD", "BAD", "CC");
			badWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;

			var whsHepler = new WhsTestHelperFunctions(Factory);
			var client = whsHepler.CreateClient();
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "123456@qq.com";
			var password = "123";
			contact.SetHashedPassword(password);
			contact.OC_WebAccessEnabled = true;

			var trackingHepler = new TestHelper(Factory);
			trackingHepler.TestSiteUser.Login(client.OH_Code, contact.OC_Email, password);
			AssertNotNull("Precondition: SiteUser has been set up", trackingHepler.TestSiteUser);
			Assert("WebUser should be logged in", trackingHepler.TestSiteUser.IsLoggedIn);

			whsHepler.CreateWhsOrder(client, staffWarehouse);

			whsHepler.ProhibitWarehouseAccessForOrgContact(badWarehouse, contact);

			Factory.Save();

			var warehouseQuery = OrgRestrictionFilterFactory.GetOrderedProhibitedWarehouseQuery();

			AssertContainsExactElementsInAnyOrder("Return the warehouse that is orderd and not prohibited.",
				new[] { staffWarehouse.PK }, new BusinessObjectFactory().Load<WhsWarehouse>(warehouseQuery).Select(w => w.PK));
		}

		#endregion

		#region TestGetOrderedProhibitedWarehouseQuery_DoesNotLoginTrackingSiteUser()

		public void TestGetOrderedProhibitedWarehouseQuery_DoesNotLoginTrackingSiteUser()
		{
			var envHelper = new WhsTestHelperFunctionsEnv(Factory);
			var staffWarehouse = envHelper.CreateWarehouse("STAFF", "STF", "AA");
			staffWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var groupWarehouse = envHelper.CreateWarehouse("GROUP", "GRP", "BB");
			groupWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var badWarehouse = envHelper.CreateWarehouse("BAD", "BAD", "CC");
			badWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;

			var whsHepler = new WhsTestHelperFunctions(Factory);
			var client = whsHepler.CreateClient();
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "123456@qq.com";
			var password = "123";
			contact.SetHashedPassword(password);
			contact.OC_WebAccessEnabled = true;

			var trackingHepler = new TestHelper(Factory);
			trackingHepler.TestSiteUser.Login(client.OH_Code, contact.OC_Email, password);
			AssertNotNull("Precondition: SiteUser has been set up", trackingHepler.TestSiteUser);
			Assert("WebUser should be logged in", trackingHepler.TestSiteUser.IsLoggedIn);

			whsHepler.CreateWhsOrder(client, staffWarehouse);

			whsHepler.ProhibitWarehouseAccessForOrgContact(badWarehouse, contact);

			Factory.Save();

			trackingHepler.TestSiteUser.Logout();
			Assert("WebUser should not be logged in", !trackingHepler.TestSiteUser.IsLoggedIn);
			var warehouseQuery = OrgRestrictionFilterFactory.GetOrderedProhibitedWarehouseQuery();

			Assert("Return no warehouse.", !new BusinessObjectFactory().Load<WhsWarehouse>(warehouseQuery).Any());
		}

		#endregion

		#region TestGetProhibitedWarehouseSubQuery

		public void TestGetProhibitedWarehouseSubQuery()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var org = helper.CreateClient("O1");
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "ContactName";
			contact.OC_Email = "test@cargowise.com";
			contact.SetHashedPassword("abc123");
			contact.OC_WebAccessEnabled = true;

			var whs1 = helper.CreateWarehouse("WH1");
			var whs2 = helper.CreateWarehouse("WH2");
			Factory.Save();

			var warehouseQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));
			var genPivotSubQuery = OrgRestrictionFilterFactory.GetProhibitedWarehouseSubQuery(contact.PK);
			warehouseQuery.AddSubQuery(genPivotSubQuery, JoinCondition.And);

			AssertContainsExactElementsInAnyOrder("Precondition - All warehouses must be returned.",
				new[] { whs1.PK, whs2.PK }, new BusinessObjectFactory().Load<WhsWarehouse>(warehouseQuery).Select(w => w.PK));

			helper.ProhibitWarehouseAccessForOrgContact(whs1, contact);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(
				"Only WH2 must be returned since current contact has been denied access to WH1.",
				new[] { whs2.PK }, new BusinessObjectFactory().Load<WhsWarehouse>(warehouseQuery).Select(w => w.PK));
		}

		#endregion

		#region TestGetWarehouseQuery

		public void TestGetWarehouseQuery()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			ZGuid warehouse1PK = CreateWarehouse();
			CreateOrder(warehouse1PK, helper.TestSiteUser.LoggedInOrganisation.PK);

			ZGuid warehouse2PK = CreateWarehouse();
			OrgHeader anotherOrg = Factory.NewWithValidTestData<OrgHeader>();
			CreateOrder(warehouse2PK, anotherOrg.PK);

			Factory.Save();

			WhsWarehouse[] warehouses = Factory.Load<WhsWarehouse>(OrgRestrictionFilterFactory.Instance.GetFilter<WhsWarehouse>());

			AssertEquals("Expected 1 warehouse", 1, warehouses.Length);
		}

		ZGuid CreateWarehouse()
		{
			WhsWarehouse warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_IsActive = true;

			return warehouse.PK;
		}

		void CreateOrder(ZGuid warehousePK, ZGuid orgPK)
		{
			WhsOrder order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehousePK;
			order.WD_OH_Client = orgPK;
		}

		#endregion

		#region Warehouse RelatedOrg Tests

		public void TestTrackingWhsOrder()
		{
			InitWarehouseTest();
			bool oldRegistryValue = WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.Value;

			AssertWarehouseRelatedOrg<TrackingWhsOrder>(WhsDocketSchema.WD_OH_Client, true);
			AssertWarehouseRelatedOrg<TrackingWhsOrder>(WhsDocketSchema.WD_OH_Client, false);

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldRegistryValue);
		}

		public void TestTrackingWhsOrder_Transport()
		{
			InitWarehouseTest();

			var helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(OrgHeader1.OH_Code, OrgContact0.OC_Email, OrgContactPassword);

			var order = Factory.NewWithValidTestData<WhsOrder>();
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_OA_Address = helper.TestSiteUser.OrganisationRelatedOrgAddressPKs[0];
			address.E2_ParentID = order.PK;
			address.E2_ParentTableCode = order.TablePrefix;
			address.E2_AddressType = AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress;

			Factory.Save();

			var query = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingWhsOrder>();
			var results = Factory.Load<WhsOrder>(query);
			AssertEquals(0, results.Length);
		}

		public void TestTrackingWhsReceive()
		{
			InitWarehouseTest();
			bool oldRegistryValue = WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.Value;

			AssertWarehouseRelatedOrg<TrackingWhsReceive>(WhsDocketSchema.WD_OH_Client, true);
			AssertWarehouseRelatedOrg<TrackingWhsReceive>(WhsDocketSchema.WD_OH_Client, false);

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldRegistryValue);
		}

		public void TestTrackingWhsReceive_Transport()
		{
			InitWarehouseTest();

			var helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(OrgHeader1.OH_Code, OrgContact0.OC_Email, OrgContactPassword);

			var receive1 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			var address1 = Factory.NewWithValidTestData<JobDocAddress>();
			address1.E2_OA_Address = helper.TestSiteUser.OrganisationRelatedOrgAddressPKs[0];
			address1.E2_ParentID = receive1.WhsReceive.PK;
			address1.E2_ParentTableCode = receive1.WhsReceive.TablePrefix;
			address1.E2_AddressType = AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress;

			var receive2 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			var address2 = Factory.NewWithValidTestData<JobDocAddress>();
			address2.E2_OA_Address = helper.TestSiteUser.OrganisationRelatedOrgAddressPKs[0];
			address2.E2_ParentID = receive2.WhsReceive.PK;
			address2.E2_ParentTableCode = receive2.WhsReceive.TablePrefix;
			address2.E2_AddressType = AutoDocAddressTypes.Codes.ArrivalCFSAddress;

			var receive3 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			var address4 = Factory.NewWithValidTestData<JobDocAddress>();
			address4.E2_ParentID = receive3.WhsReceive.PK;
			address4.E2_ParentTableCode = receive3.WhsReceive.TablePrefix;
			address4.E2_AddressType = AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress;

			Factory.Save();

			var query = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingWhsReceive>();
			var results = Factory.Load<WhsReceive>(query);
			AssertEquals(1, results.Length);
			AssertEquals(true, results.Contains(receive1.WhsReceive));
		}

		public void TestTrackingWhsInventory()
		{
			InitWarehouseTest();
			bool oldRegistryValue = WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.Value;

			AssertWarehouseRelatedOrg<TrackingWhsInventory>(WhsInventoryViewSchema.WI_OH_Client, true);
			AssertWarehouseRelatedOrg<TrackingWhsInventory>(WhsInventoryViewSchema.WI_OH_Client, false);

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldRegistryValue);
		}

		public void TestOrgPartRelation()
		{
			InitWarehouseTest();
			bool oldRegistryValue = WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.Value;

			AssertWarehouseRelatedOrg<OrgPartRelation>(OrgPartRelationSchema.OU_OH, true);
			AssertWarehouseRelatedOrg<OrgPartRelation>(OrgPartRelationSchema.OU_OH, false);

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldRegistryValue);
		}

		void AssertWarehouseRelatedOrg<T>(SchemaGuidColumn guidColumn, bool allowAccessFromMgmtGroup) where T : BusinessObject
		{
			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowAccessFromMgmtGroup);

			var helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(OrgHeader1.OH_Code, OrgContact0.OC_Email, OrgContactPassword);

			var queryText = OrgRestrictionFilterFactory.Instance.GetFilter<T>().LiteralTextADO;

			Assert(string.Concat("Contains ", guidColumn.Name), queryText.Contains(guidColumn.Name));
			AssertEquals("Contains OrgHeader0 PK", allowAccessFromMgmtGroup, queryText.Contains(OrgHeader0.PK.ToString()));
			Assert("Contains OrgHeader1 PK", queryText.Contains(OrgHeader1.PK.ToString()));
		}

		OrgHeader OrgHeader0;
		OrgHeader OrgHeader1;
		OrgContact OrgContact0;
		const string OrgContactPassword = "pswrd";

		void InitWarehouseTest()
		{
			OrgHeader0 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = OrgHeader0.PK;
			relatedParty.PR_OH_RelatedParty = OrgHeader1.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			OrgContact0 = OrgHeader1.Contacts.AddNew();
			OrgContact0.OC_ContactName = "contactname";
			OrgContact0.OC_Email = "email@email.em";
			OrgContact0.SetHashedPassword(OrgContactPassword);
			OrgContact0.OC_WebAccessEnabled = true;

			Factory.Save();
		}

		#endregion

		#region Implementation

		void AssertJobConsolQuery(string queryText)
		{
			Assert("Contains JK_OA_ReceivingForwarderAddress", queryText.Contains(JobConsolSchema.JK_OA_ReceivingForwarderAddress.Name));
			Assert("Contains JK_OA_SendingForwarderAddress", queryText.Contains(JobConsolSchema.JK_OA_SendingForwarderAddress.Name));
			AssertEquals("Does not contain JK_OA_ArrivalUnpackCFSTransportAddress", false, queryText.Contains(JobConsolSchema.JK_OA_ArrivalUnpackCFSTransportAddress.Name));
			AssertEquals("Does not contain JK_OA_CreditorAddress", false, queryText.Contains(JobConsolSchema.JK_OA_CreditorAddress.Name));
			AssertEquals("Does not contain JK_OA_DeparturePackCFSTransportAddress", false, queryText.Contains(JobConsolSchema.JK_OA_DeparturePackCFSTransportAddress.Name));
			AssertEquals("Does not contain JK_OA_ShippingLineAddress", false, queryText.Contains(JobConsolSchema.JK_OA_ShippingLineAddress.Name));
		}

		void AssertOrganisationRelatedOrgAddressPKs(ZQuery query, SchemaColumn relatedOrgAddressPKsColumn)
		{
			var param = query.Params.Where(x => x.SchemaColumn.Name == relatedOrgAddressPKsColumn.Name).FirstOrDefault();

			Assert("Related org address pks params should be TVP", param.IsTableValued);
			AssertContainsExactElementsInAnyOrder("TVP values should be related org address pks", OrgRestrictionFilterFactory.SiteUser.OrganisationRelatedOrgAddressPKs, (IEnumerable<object>)param.Value);
		}

		#endregion
	}
}
