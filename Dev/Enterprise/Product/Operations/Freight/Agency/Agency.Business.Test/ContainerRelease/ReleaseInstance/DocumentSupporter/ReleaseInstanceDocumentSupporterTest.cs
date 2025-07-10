using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ReleaseInstanceDocumentSupporter))]
	internal class ReleaseInstanceDocumentSupporterTest : DocumentSupporterTest
	{
		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuName == "Container Release HACK" || base.ExcludeDocumentCommandTest(documentCommand);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			var booking = Factory.New<AgencyBooking>();
			booking.JS_OH_DeliveryAgent = deliveryAgent.PK;
			var header = new ReleaseHeader(booking, false);
			var container = booking.BookedContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 2;
			foreach (ReleaseDetail detail in header.Details)
			{
				detail.ReleaseCount = detail.Container.JC_ContainerCount;
			}

			return header.Instances.AddNew();
		}
	}
}
