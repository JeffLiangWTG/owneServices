using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderDocumentSupporter))]
	sealed class OrderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage_LandedCostHeader()
		{
			var order = Factory.New<Order>();
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "SGSIN";

			var contextArray = new[]
			{
				Constants.DataContext.LandedCostHeader,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This Order is not an Import so Landing Costing is not possible.";

			AssertNotFoundMessage(order, menu, contextArray, true, expectedMessage);

			order.JD_RL_NKPortOfLoading = "SGSIN";
			order.JD_RL_NKPortOfDischarge = "AUSYD";

			expectedMessage = "This Order does not have any Transport Logistics Costs entered within the Landed Costing tab.";

			AssertNotFoundMessage(order, menu, contextArray, true, expectedMessage);
		}

		public void TestControllingCustomerContactOrganisation()
		{
			var order = Factory.New<Order>();
			var controllingCustomer = Factory.New<OrgHeader>();

			order.ControllingCustomerDocAddress.OrganisationPK = controllingCustomer.PK;
			AssertEquals("Controlling Customer from order", controllingCustomer, (OrgHeader)order.DocumentSupporter.GetContactOrganisation("", ContactType.ControllingCustomer, DocumentDirection.ARV).OrgHeader);

			order.ControllingCustomerDocAddress.E2_AddressOverride = true;
			AssertNull((OrgHeader)order.DocumentSupporter.GetContactOrganisation("", ContactType.ControllingCustomer, DocumentDirection.ARV).OrgHeader);
		}

		public void TestControllingAgentContactOrganisation()
		{
			var order = Factory.New<Order>();
			var controllingAgent = Factory.New<OrgHeader>();

			var controllingAgentAddress = order.DocAddresses.AddNew(DocAddressType.ControllingAgent);
			controllingAgentAddress.OrganisationPK = controllingAgent.PK;
			AssertEquals("Controlling Agent from order", controllingAgent, (OrgHeader)order.DocumentSupporter.GetContactOrganisation("", ContactType.ControllingAgent, DocumentDirection.ARV).OrgHeader);

			controllingAgentAddress.E2_AddressOverride = true;
			AssertNull((OrgHeader)order.DocumentSupporter.GetContactOrganisation("", ContactType.ControllingAgent, DocumentDirection.ARV).OrgHeader);
		}

		protected override bool ShouldSkipWithContextAndMenu(Constants.DataContext context, IStmMenuItem menu)
		{
			var order = Factory.New<Order>();
			var supporter = order.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, menu);

			return wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null);
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForMessageNotPrintingTest
		{
			get { return new[] { Factory.New<Order>() }; }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYERTEST";
			buyer.MainAddress.OA_Address1 = "Buy Test Address1";

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;

			var header = (BusinessObject)Factory.New<Enterprise.Integration.LandedCosting.ILandedCostHeader>();
			header[LandedCostHeaderSchema.LT_ParentID] = order.PK;
			header[LandedCostHeaderSchema.LT_ParentTableCode] = JobOrderHeaderSchema.Constants.Prefix;

			Factory.Save();

			return order;
		}
	}
}
