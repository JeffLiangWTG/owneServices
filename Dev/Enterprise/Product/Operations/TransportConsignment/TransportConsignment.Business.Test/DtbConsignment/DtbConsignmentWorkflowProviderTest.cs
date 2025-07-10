using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignment))]
	class DtbConsignmentWorkflowProviderTest : WorkflowProviderTest<DtbConsignment, DtbConsignmentProcessTaskCollection>
	{
		#region TestGetTemplateSelectionCriteria

		public void TestGetTemplateSelectionCriteria()
		{
			var orgs = new OrgCollection(Factory);
			var consignment = Factory.New<DtbConsignment>();

			AssertTemplateCriteria(consignment);

			AddBookingParty(consignment, orgs.Org1);
			AddBillingClient(consignment, orgs.Org2);
			AddClientRequestedLocalClient(consignment, orgs.Org3);
			AddPickupAddress(consignment, orgs.Org4);
			AddDeliveryAddress(consignment, orgs.Org5);

			AssertTemplateCriteria(consignment,
				"Org 1 Booking Party",
				"Org 2 Local Client From Billing",
				"Org 3 Client Requested Local Client",
				"Org 4 Pickup",
				"Org 5 Delivery");

			RemoveBillingClient(consignment);
			RemoveDeliveryAddress(consignment);

			AssertTemplateCriteria(consignment,
				"Org 1 Booking Party",
				"Org 3 Client Requested Local Client",
				"Org 4 Pickup");
		}

		void AssertTemplateCriteria(IWorkflowProvider consignment, params string[] expectedCriteria)
		{
			var actualOrgPksInOrder = ((IColumnValueRankerInternals)consignment.GetTemplateSelectionCriteria()).ColumnValues
				.SelectMany(x => x.Values).Cast<ZGuid>().ToArray();

			AssertEquals("We must append an empty PK to the end of the list, so that generic templates are matched after anything more specific.", actualOrgPksInOrder.Last(), ZGuid.Empty);

			var validOrgPKsInOrder = actualOrgPksInOrder.Where(x => x.IsValid);
			var actualOrgsInAnyOrder = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, validOrgPKsInOrder));
			var actualOrgNamesInOrder = validOrgPKsInOrder.Select(x => actualOrgsInAnyOrder.Single(org => org.PK == x).OH_FullName);

			AssertContainsExactElementsInExactOrder(expectedCriteria, actualOrgNamesInOrder);
		}

		public void TestApplyTemplate_WithRegistryDefault()
		{
			var orgs = new OrgCollection(Factory);
			SetUpTemplatesForRegistryOrderTests(orgs);
			Factory.Save();

			var consignment = Factory.New<DtbConsignment>();
			ApplyWorkflowTemplates(consignment);
			AssertTasks("There are no matching clients at all so the template without any criteria should be applied.",
				consignment, "Template 6");

			AddDeliveryAddress(consignment, orgs.Org1);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 1");

			AddPickupAddress(consignment, orgs.Org2);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 2");

			AddClientRequestedLocalClient(consignment, orgs.Org3);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 3");

			AddBillingClient(consignment, orgs.Org4);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 4");

			AddBookingParty(consignment, orgs.Org5);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 5");
		}

		public void TestApplyTemplate_ShouldUseClientPrecedenceOrderFromRegistry_WhenOrderChanged()
		{
			var registryItem = (ClientInTemplateSelectionCriteriaCollection)WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value
				.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var criteria = registryItem.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.LandTransportConsignment);
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Booking Party",
				"Local Client",
				"Pickup Address Organization",
				"Delivery Address Organization",
			}, criteria.SelectedItems.Cast<ClientInTemplateSelectionCriteriaOrgType>().Select(x => x.OrgTypeDescription.ToString()));

			criteria.SelectedItems.MoveItem(0, 3);
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Local Client",
				"Pickup Address Organization",
				"Delivery Address Organization",
				"Booking Party",
			}, criteria.SelectedItems.Cast<ClientInTemplateSelectionCriteriaOrgType>().Select(x => x.OrgTypeDescription.ToString()));

			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem);

			var orgs = new OrgCollection(Factory);
			SetUpTemplatesForRegistryOrderTests(orgs);
			Factory.Save();

			var consignment = Factory.New<DtbConsignment>();
			ApplyWorkflowTemplates(consignment);
			AssertTasks("There are no matching clients at all so the template without any criteria should be applied.",
				consignment, "Template 6");

			AddDeliveryAddress(consignment, orgs.Org1);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 1");

			AddPickupAddress(consignment, orgs.Org2);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 2");

			AddClientRequestedLocalClient(consignment, orgs.Org3);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 3");

			AddBillingClient(consignment, orgs.Org4);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 4");

			AddBookingParty(consignment, orgs.Org5);
			ApplyWorkflowTemplates(consignment);
			AssertTasks("Booking Party is now lower priority than the other criteria, so it will only be selected if all the other organizations are blank.",
				consignment, "Template 4");

			RemoveDeliveryAddress(consignment);
			RemovePickupAddress(consignment);
			RemoveClientRequestedLocalClient(consignment);
			RemoveBillingClient(consignment);
			ApplyWorkflowTemplates(consignment);
			AssertTasks("Booking Party is now lower priority than the other criteria, so it will only be selected if all the other organizations are blank.",
				consignment, "Template 5");
		}

		public void TestApplyTemplate_ShouldUseClientPrecedenceOrderFromRegistry_WhenItemsChanged()
		{
			var registryItem = (ClientInTemplateSelectionCriteriaCollection)WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value
				.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var criteria = registryItem.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.LandTransportConsignment);
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Booking Party",
				"Local Client",
				"Pickup Address Organization",
				"Delivery Address Organization",
			}, criteria.SelectedItems.Cast<ClientInTemplateSelectionCriteriaOrgType>().Select(x => x.OrgTypeDescription.ToString()));

			var criterionToRemove = criteria.SelectedItems[2];
			criteria.SelectedItems.Remove(criterionToRemove);
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Booking Party",
				"Local Client",
				"Delivery Address Organization",
			}, criteria.SelectedItems.Cast<ClientInTemplateSelectionCriteriaOrgType>().Select(x => x.OrgTypeDescription.ToString()));

			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem);

			var orgs = new OrgCollection(Factory);
			SetUpTemplatesForRegistryOrderTests(orgs);
			Factory.Save();

			var consignment = Factory.New<DtbConsignment>();
			ApplyWorkflowTemplates(consignment);
			AssertTasks("There are no matching clients at all so the template without any criteria should be applied.",
				consignment, "Template 6");

			AddDeliveryAddress(consignment, orgs.Org1);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 1");

			AddPickupAddress(consignment, orgs.Org2);
			ApplyWorkflowTemplates(consignment);
			AssertTasks("Pickup Address Organization has been removed from the criteria in the registry, so it should not affect template application.",
				consignment, "Template 1");

			AddClientRequestedLocalClient(consignment, orgs.Org3);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 3");

			AddBillingClient(consignment, orgs.Org4);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 4");

			AddBookingParty(consignment, orgs.Org5);
			ApplyWorkflowTemplates(consignment);
			AssertTasks(consignment, "Template 5");
		}

		public void TestApplyTemplate_ShouldUseClientPrecedenceOrderFromRegistry_WhenCriteriaListIsEmpty()
		{
			var registryItem = (ClientInTemplateSelectionCriteriaCollection)WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value
				.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var criteria = registryItem.GetValueByCode(ClientInTemplateSelectionProcessTypeList.Codes.LandTransportConsignment);
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Booking Party",
				"Local Client",
				"Pickup Address Organization",
				"Delivery Address Organization",
			}, criteria.SelectedItems.Cast<ClientInTemplateSelectionCriteriaOrgType>().Select(x => x.OrgTypeDescription.ToString()));

			criteria.SelectedItems.RemoveAll();
			AssertContainsExactElementsInExactOrder(Array.Empty<string>(), criteria.SelectedItems.Cast<ClientInTemplateSelectionCriteriaOrgType>().Select(x => x.OrgTypeDescription.ToString()));

			WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem);

			var orgs = new OrgCollection(Factory);
			SetUpTemplatesForRegistryOrderTests(orgs);
			Factory.Save();

			var consignment = Factory.New<DtbConsignment>();
			ApplyWorkflowTemplates(consignment);
			AssertTasks("There are no matching clients at all so the template without any criteria should be applied.",
				consignment, "Template 6");

			AddDeliveryAddress(consignment, orgs.Org1);
			ApplyWorkflowTemplates(consignment);
			AssertTasks("No criteria are listed in the registry so the generic template will always be applied.",
				consignment, "Template 6");

			AddPickupAddress(consignment, orgs.Org2);
			ApplyWorkflowTemplates(consignment);
			AssertTasks("No criteria are listed in the registry so the generic template will always be applied.",
				consignment, "Template 6");

			AddClientRequestedLocalClient(consignment, orgs.Org3);
			ApplyWorkflowTemplates(consignment);
			AssertTasks("No criteria are listed in the registry so the generic template will always be applied.",
				consignment, "Template 6");

			AddBillingClient(consignment, orgs.Org4);
			ApplyWorkflowTemplates(consignment);
			AssertTasks("No criteria are listed in the registry so the generic template will always be applied.",
				consignment, "Template 6");

			AddBookingParty(consignment, orgs.Org5);
			ApplyWorkflowTemplates(consignment);
			AssertTasks("No criteria are listed in the registry so the generic template will always be applied.",
				consignment, "Template 6");
		}

		public void TestApplyTemplate_DbHits()
		{
			var orgs = new OrgCollection(Factory);
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment();
			var pickupAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			pickupAddress.LTS_Sequence = 1;
			deliveryAddress.LTS_Sequence = 2;

			AssertTemplateCriteria(consignment);

			AddBookingParty(consignment, orgs.Org1);
			AddBillingClient(consignment, orgs.Org2);
			AddClientRequestedLocalClient(consignment, orgs.Org3);

			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedConsignment = newFactory.Load<DtbConsignment>(consignment.PK);

			var hits = new Dictionary<string, int>
			{
				{ DtbConsignmentAddressSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsForAllFactories(hits))
			{
				ApplyWorkflowTemplates(loadedConsignment);
			}
		}

		public void TestPickupAddress_WhenMultipleAddressesPresent_ShouldReportError()
		{
			var orgs = new OrgCollection(Factory);
			var consignment = Factory.New<DtbConsignment>();
			AddPickupAddress(consignment, orgs.Org1);
			AddPickupAddress(consignment, orgs.Org2);
			_ = consignment.PickupAddress;

			AssertEquals(@"If this assertion fails, it is likely that you are trying to implement a use case that allows for multiple pickup addresses on a consignment.
This is fine, however please note that template application is currently designed to assume that there is only one pickup address.
To expand this functionality, you will need to update DtbConsignment.GetTemplateSelectionCriteria and the registry item that this accesses in order to build its list.
This change in functionality will need a design. Please discuss with team leadership before continuing.",
				"Consignment contains 2 Pickup Address.", ErrorReporter.LastMessageReported);

			if (ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestDeliveryAddress_WhenMultipleAddressesPresent_ShouldThrowException()
		{
			var orgs = new OrgCollection(Factory);
			var consignment = Factory.New<DtbConsignment>();
			AddDeliveryAddress(consignment, orgs.Org1);
			AddDeliveryAddress(consignment, orgs.Org2);

			AssertExceptionThrown<InvalidOperationException>(@"If this assertion fails, it is likely that you are trying to implement a use case that allows for multiple delivery addresses on a consignment.
This is fine, however please note that template application is currently designed to assume that there is only one delivery address.
To expand this functionality, you will need to update DtbConsignment.GetTemplateSelectionCriteria and the registry item that this accesses in order to build its list.
This change in functionality will need a design. Please discuss with team leadership before continuing.",
				() => _ = consignment.DeliveryAddress);
		}

		static void AddBookingParty(DtbConsignment consignment, OrgHeader org)
		{
			consignment.DocAddresses.AddNew(org.MainAddress, DocAddressType.BookingPartyDocumentaryAddress);
		}

		static void AddBillingClient(DtbConsignment consignment, OrgHeader org)
		{
			new JobHeader.Loader(consignment).TryCreate();
			consignment.Job.JH_OA_LocalChargesAddr = org.MainAddress.PK;
		}

		static void RemoveBillingClient(DtbConsignment consignment)
		{
			consignment.Job.JH_OA_LocalChargesAddr = ZGuid.Empty;
		}

		static void AddClientRequestedLocalClient(DtbConsignment consignment, OrgHeader org)
		{
			consignment.DocAddresses.AddNew(org.MainAddress, DocAddressType.ClientRequestedBillingParty);
		}

		static void RemoveClientRequestedLocalClient(DtbConsignment consignment)
		{
			RemoveDocAddress(consignment, DocAddressType.ClientRequestedBillingParty);
		}

		static void AddPickupAddress(DtbConsignment consignment, OrgHeader org)
		{
			var pickupAddress = consignment.Addresses.AddNew(ConsignmentAddressTypes.Codes.PickUp);
			pickupAddress.Address.OrganisationPK = org.PK;
		}

		static void RemovePickupAddress(DtbConsignment consignment)
		{
			RemoveConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
		}

		static void AddDeliveryAddress(DtbConsignment consignment, OrgHeader org)
		{
			var pickupAddress = consignment.Addresses.AddNew(ConsignmentAddressTypes.Codes.Delivery);
			pickupAddress.Address.OrganisationPK = org.PK;
		}

		static void RemoveDeliveryAddress(DtbConsignment consignment)
		{
			RemoveConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
		}

		static void RemoveDocAddress(DtbConsignment consignment, DocAddressType type)
		{
			var addressToRemove = consignment.DocAddresses.FindByDocAddressType(type);
			consignment.DocAddresses.Remove(addressToRemove);
		}

		static void RemoveConsignmentAddress(DtbConsignment consignment, ZString type)
		{
			var addressToRemove = consignment.Addresses.Single(address => address.ConsignmentAddressType == type);
			consignment.Addresses.RemoveFromRelationship(addressToRemove);
		}

		void ApplyWorkflowTemplates(DtbConsignment consignment)
		{
			consignment.WorkflowItems.Tasks.RemoveAndDeleteAll();
			new ProcessTask.Loader(consignment.Factory).CreateTasksAndMilestonesFromTemplateIfRequired(consignment, TemplateApplicationParameters.ApplyIgnoreHasChanges());
		}

		void SetUpTemplatesForRegistryOrderTests(OrgCollection config)
		{
			CreateTemplate(1, config.Org1);
			CreateTemplate(2, config.Org2);
			CreateTemplate(3, config.Org3);
			CreateTemplate(4, config.Org4);
			CreateTemplate(5, config.Org5);
			CreateTemplate(6, null);

			void CreateTemplate(int templateNumber, OrgHeader client)
			{
				var template = Factory.New<ProcessTaskTemplate>();
				template.P0_Name = "Template " + templateNumber;
				template.P0_ProcessType = WorkflowDescriptors.DtbConsignmentWorkflowDescriptorCode;
				var task = template.WorkflowItems.Tasks.AddNew();
				task.P9_Description = "Template " + templateNumber;
				template.P0_OH_Client = client?.PK ?? ZGuid.Empty;
			}
		}

		static void AssertTasks(string message, DtbConsignment consignment, params string[] expectedTaskDescriptions)
		{
			var actualTaskDescriptions = consignment.WorkflowItems.Tasks.Select(x => x.P9_Description);
			AssertContainsExactElementsInAnyOrder(message, expectedTaskDescriptions, actualTaskDescriptions);
		}

		static void AssertTasks(DtbConsignment consignment, params string[] expectedTaskDescriptions)
		{
			AssertTasks(null, consignment, expectedTaskDescriptions);
		}

		class OrgCollection
		{
			public OrgHeader Org1 { get; set; }
			public OrgHeader Org2 { get; set; }
			public OrgHeader Org3 { get; set; }
			public OrgHeader Org4 { get; set; }
			public OrgHeader Org5 { get; set; }

			public OrgCollection(BusinessObjectFactory factory)
			{
				Org1 = factory.NewWithValidTestData<OrgHeader>();
				Org2 = factory.NewWithValidTestData<OrgHeader>();
				Org3 = factory.NewWithValidTestData<OrgHeader>();
				Org4 = factory.NewWithValidTestData<OrgHeader>();
				Org5 = factory.NewWithValidTestData<OrgHeader>();

				Org1.OH_FullName = "Org 1 Booking Party";
				Org2.OH_FullName = "Org 2 Local Client From Billing";
				Org3.OH_FullName = "Org 3 Client Requested Local Client";
				Org4.OH_FullName = "Org 4 Pickup";
				Org5.OH_FullName = "Org 5 Delivery";
			}
		}

		#endregion

		#region TestGetWorkflowInformationProvider

		public void TestGetWorkflowInformationProvider()
		{
			IWorkflowProvider consignment = GetNewBusinessObject(Factory);
			AssertNull(consignment.GetWorkflowInformationProvider());
		}

		protected override DtbConsignment GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<DtbConsignment>();
		}

		#endregion

		#region ExpectedWorkflowType

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.DtbConsignmentWorkflowDescriptorCode; }
		}

		#endregion
	}
}
