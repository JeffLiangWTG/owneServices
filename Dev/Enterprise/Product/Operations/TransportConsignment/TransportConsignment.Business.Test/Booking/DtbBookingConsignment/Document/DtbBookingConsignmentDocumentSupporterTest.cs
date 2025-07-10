using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbBookingConsignmentDocumentSupporter))]
	class DtbBookingConsignmentDocumentSupporterTest : DtbTransportDocumentSupporterTest<DtbBookingConsignment, DtbBookingConsignmentDocumentSupporter>
	{
		#region TestBusinessContext

		protected override BusinessContext ExpectedBusinessContext
		{
			get { return BusinessContext.DtbConsignment; }
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		protected override Security.SecurityCheckpoint ExpectedSecurityCheckpoint
		{
			get { return Env.Security.DtbBookingConsignmentCustomiseDocuments; }
		}

		#endregion

		#region TestDocumentWrapperCreated

		protected override void TestGetDocumentWrappersInternalCore(DtbBookingConsignmentDocumentSupporter documentSupporter, StmMenuItem menuItem)
		{
			var supporter = documentSupporter;
			AssertEquals("Precondition : DtbConsignment supports JobServices. No Job Service has been added at this stage.", 0, supporter.Consignment.Services.Count);

			var serviceWrappers = documentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobServices, menuItem);
			AssertEquals("There are no services, therefore no wrappers to produce.", 0, serviceWrappers.Length);

			supporter.Consignment.Services.AddNew();
			serviceWrappers = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobServices, menuItem);
			AssertEquals("1 Service has been added, 1 document wrappers should be produced.", 1, serviceWrappers.Length);

			supporter.Consignment.Services.AddNew();
			serviceWrappers = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobServices, menuItem);
			AssertEquals("2 Services has been added, 2 document wrappers should be produced.", 2, serviceWrappers.Length);
		}

		#endregion

		#region GetExpectedBizOWithValidData

		protected override DtbBookingConsignment GetExpectedBizOWithValidData()
		{
			return Helper.CreateBookingConsignmentWithTemplate();
		}

		#endregion

		#region TestGetContactOrganisation

		#region TestGetContactOrganisation

		public void TestGetContactOrganisation()
		{
			var client = Factory.New<OrgHeader>();
			client.OrganisationTypes = OrganisationTypes.Consignor;

			var consignment = Helper.CreateBookingConsignment();
			var pickupInstruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.PickUp, client.MainAddress);

			var documentContact = ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignor, DocumentDirection.ANY);
			AssertEquals("The contact should be the Pickup (Consignor) Org.", client.PK, documentContact.OrgHeader.PK);
		}

		#endregion

		#region TestGetContactOrganisation_WithNoAddressOnPickupInstruction

		public void TestGetContactOrganisation_WithNoAddressOnPickupInstruction()
		{
			var consignment = Helper.CreateBookingConsignment();
			var pickupInstruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.PickUp);

			AssertNoExceptionThrown(() => { ((IDocumentSupportable)consignment).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignor, DocumentDirection.ANY); });
		}

		#endregion

		#endregion

		#region DoSetupForDocument

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			((DtbBookingConsignment)documentSupportableBO).Services.AddNew(); // for Service Documents
			base.DoSetupForDocument(command, documentSupportableBO);
		}

		#endregion

		protected override bool GetShowReasonForNotPrintingCore()
		{
			return true;
		}

		protected override IEnumerable<Tuple<Constants.DataContext, string>> SupportedDataContextAndNotFoundMessageReasonPairs
		{
			get
			{
				return new List<Tuple<Constants.DataContext, string>>()
				{
					new Tuple<Constants.DataContext, string>(Constants.DataContext.GenericFreightJobServices, "Cannot find Job Service.")
				};
			}
		}
		#region Implementation

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
