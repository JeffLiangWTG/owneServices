using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Integration.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbBookingConsignmentDocumentSupporter))]
	class DtbBookingConsignmentDocumentSupporterDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Helper.CreateBookingConsignmentWithTemplate();
		}

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			GetDocumentSupportableBusinessObject();
			base.DoSetupForDocument(command, documentSupportableBO);
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuName.StartsWith("Authorization for Service") ||
				documentCommand.SU_MenuName.StartsWith("Request for Service");
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;
	}
}
