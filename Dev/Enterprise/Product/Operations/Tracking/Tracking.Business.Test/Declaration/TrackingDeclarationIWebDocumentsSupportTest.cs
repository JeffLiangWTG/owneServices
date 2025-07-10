using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingDeclaration))]
	sealed class TrackingDeclarationIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			BaseJobDeclaration testJobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			testJobDeclaration.JE_DeclarationReference = "B123456789";

			testJobDeclaration.CustomsEntryHeaders.AddNew();
			testJobDeclaration.CustomsEntryHeaders.AddNew();

			testJobDeclaration.Invoices.AddNew();
			testJobDeclaration.Invoices.AddNew();

			var testShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testJobDeclaration.JE_JS = testShipment.PK;

			fExpectedDocRelatedPKs = new ZGuid[] { testJobDeclaration.CustomsEntryHeaders[0].PK, testJobDeclaration.CustomsEntryHeaders[1].PK, testJobDeclaration.Invoices[0].PK, testJobDeclaration.Invoices[1].PK, testShipment.PK };

			return new TrackingDeclaration(testJobDeclaration);
		}

		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return fExpectedDocRelatedPKs; }
		}
		ZGuid[] fExpectedDocRelatedPKs = System.Array.Empty<ZGuid>();
	}
}
