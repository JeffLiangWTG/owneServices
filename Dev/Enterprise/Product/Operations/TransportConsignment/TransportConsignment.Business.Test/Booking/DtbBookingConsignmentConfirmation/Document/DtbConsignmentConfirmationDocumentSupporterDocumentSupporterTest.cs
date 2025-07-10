using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Integration.DocumentEngine;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentConfirmationDocumentSupporter))]
	public class DtbConsignmentConfirmationDocumentSupporterDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			var instruction = consignment.PickupInstruction;
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp, ZDateTime.Now, ZDateTime.Now.AddDays(3));
			return confirmation;
		}

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			GetDocumentSupportableBusinessObject();
			base.DoSetupForDocument(command, documentSupportableBO);
		}

		protected TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;
	}
}
