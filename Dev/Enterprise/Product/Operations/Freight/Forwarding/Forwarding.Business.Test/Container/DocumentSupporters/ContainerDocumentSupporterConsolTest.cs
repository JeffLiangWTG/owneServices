using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ContainerDocumentSupporterConsol))]
	sealed class ContainerDocumentSupporterConsolTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage_NoConsol()
		{
			var container = Factory.New<ForwardingContainer>();

			var contextArray = new[]
			{
				Constants.DataContext.ForwardingConsol,
				Constants.DataContext.GenericFreightJob,
				Constants.DataContext.GenericFreightJobServices,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This container is not associated with a consolidation.";

			AssertNotFoundMessage(container, menu, contextArray, true, expectedMessage);

			var consol = Factory.New<ForwardingConsol>();
			container = consol.Containers.AddNew();
			container.Services.AddNew();

			AssertNotFoundMessage(container, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoServies()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();

			var contextArray = new[]
			{
				Constants.DataContext.GenericFreightJobServices,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This container does not have any services.";

			AssertNotFoundMessage(container, menu, contextArray, true, expectedMessage);

			container.Services.AddNew();

			AssertNotFoundMessage(container, menu, contextArray, false, ZString.Empty);
		}

		protected override bool ShouldSkipWithContextAndMenu(DataContext context, IStmMenuItem menu)
		{
			var container = Factory.New<ForwardingContainer>();
			var supporter = container.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, menu);

			return wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<ForwardingContainer>();
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			// only want to run the HIDDEN ones that point direct to a template, not the ones that point to other menus
			return !documentCommand.SU_MenuName.Contains("Cartage Advice") ||
				!documentCommand.SU_FilterList.ToString().Equals("\"<CurrentCompany.GC_Code>\"!=\"<CurrentCompany.GC_Code>\"") ||
				documentCommand.SU_MenuName.Contains("Combined Cartage Advice") ||
				documentCommand.SU_MenuName.Contains("Transhipment Cartage Advice");
		}

		public void TestGetDocBusinessObjects()
		{
			DocumentWrapper[] wrappers = CommonContainer.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.CartageAdvice, null);
			AssertEquals("Wrapper created for container", 1, wrappers.Length);
			AssertEquals("Wrapper is DocContainer", "DocContainer", wrappers[0].GetType().Name);
		}

		#region Get Contact Organisation To Cartage

		public void TestGetContactOrganisationAndAddressWithFallBacks_FCL()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.Containers.Add(CommonContainer);

			var shipment = consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();

			var pickupCartage = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryCartage = Factory.NewWithValidTestData<OrgHeader>();
			pickupCartage.OH_FullName = "ContainerPickupCartage";
			deliveryCartage.OH_FullName = "ContainerDeliveryCartage";

			var contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);

			AssertNull("Expected null as there's no local transport addresses", contact);

			contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.DEP);

			AssertNull("Expected null as there's no local transport addresses", contact);

			shipment.DocsAndCartage.DeliveryCartageCoPK = deliveryCartage.PK;
			contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);

			AssertEquals("Attentioned to: ContainerDeliveryCartage", deliveryCartage.OH_FullName, contact.OrgHeader.FullName);

			shipment.DocsAndCartage.PickupCartageCoPK = pickupCartage.PK;
			contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.DEP);

			AssertEquals("Attentioned to: ContainerPickupCartage", pickupCartage.OH_FullName, contact.OrgHeader.FullName);

			consol.JK_OA_ArrivalUnpackCFSTransportAddress = ChinaShipping.MainAddress.PK;
			contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);

			AssertEquals("Expected to prefer the address on the consol over the shipment address", ChinaShipping.OH_FullName, contact.OrgHeader.FullName);

			consol.JK_OA_DeparturePackCFSTransportAddress = AustraliaPost.MainAddress.PK;
			contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.DEP);

			AssertEquals("Expected to prefer the address on the consol over the shipment address", AustraliaPost.OH_FullName, contact.OrgHeader.FullName);

			shipment.DocsAndCartage.PickupCartageCoPK = ZGuid.Empty;
			shipment.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = ZGuid.Empty;
			consol.JK_OA_DeparturePackCFSTransportAddress = ZGuid.Empty;

			AssertNull(CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV));
			AssertNull(CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.DEP));
		}

		public void TestGetContactOrganisationAndAddressWithFallBacks_Groupage()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			consol.Containers.Add(CommonContainer);
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = ChinaShipping.MainAddress.PK;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			shipment1.OuterPackLines.AddNew();
			shipment2.OuterPackLines.AddNew();
			shipment1.DocsAndCartage.DeliveryCartageCoPK = AustraliaPost.PK;
			shipment2.DocsAndCartage.DeliveryCartageCoPK = AustraliaPost.PK;

			var contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);

			AssertEquals("Expected contact to default from CartageCompany1 as the consol contact takes priority", ChinaShipping.OH_FullName, contact.OrgHeader.FullName);

			consol.JK_OA_ArrivalUnpackCFSTransportAddress = ZGuid.Empty;
			contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);

			AssertEquals("Expected contact to fall back to shipment cartage company", AustraliaPost.OH_FullName, contact.OrgHeader.FullName);

			shipment2.DocsAndCartage.DeliveryCartageCoPK = ChinaShipping.PK;
			contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);

			AssertNull("Expected not to find any contact as shipments delivery companies don't match which is a requirement for BCN/GRP", contact);
		}

		public void TestGetContactOrganisationAndAddressWithFallBacks_LCL()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.Containers.Add(CommonContainer);
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = ChinaShipping.MainAddress.PK;

			var shipments = consol.Shipments.AddNew();
			shipments.OuterPackLines.AddNew();
			shipments.DocsAndCartage.DeliveryCartageCoPK = AustraliaPost.PK;

			var contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);

			AssertEquals("Expected contact to default from CartageCompany1 as the consol contact takes priority", ChinaShipping.OH_FullName, contact.OrgHeader.FullName);

			consol.JK_OA_ArrivalUnpackCFSTransportAddress = ZGuid.Empty;
			contact = CommonContainer.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV);

			AssertNull("Expected LCL consols to not fall back to shipment cartage company contact", contact);
		}

		#endregion

		public void TestSupportedDataContext()
		{
			AssertEquals("Core.Constants.DataContext.CartageAdvice is Supported", true, CommonContainer.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CartageAdvice)));
			AssertEquals("Core.Constants.DataContext.Service is Supported", true, CommonContainer.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Service)));
			AssertEquals("Core.Constants.DataContext.ForwardingConsol is Supported", true, CommonContainer.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ForwardingConsol)));
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.ForwardingContainer, CommonContainer.DocumentSupporter.BusinessContext);
		}

		#region Implementation

		ForwardingContainer CommonContainer;

		protected override void SetUp()
		{
			base.SetUp();
			CommonContainer = (ForwardingContainer)GetDocumentSupportableBusinessObject();
		}

		OrgHeader ChinaShipping
		{
			get
			{
				if (chinaShipping == null)
				{
					chinaShipping = Factory.NewWithValidTestData<OrgHeader>();
					chinaShipping.OH_FullName = "China Shipping";
				}

				return chinaShipping;
			}
		}

		OrgHeader chinaShipping;

		OrgHeader AustraliaPost
		{
			get
			{
				if (australiaPost == null)
				{
					australiaPost = Factory.NewWithValidTestData<OrgHeader>();
					australiaPost.OH_FullName = "Australia Post";
				}

				return australiaPost;
			}
		}

		OrgHeader australiaPost;

		#endregion
	}
}
