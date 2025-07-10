using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingShipment))]
	sealed class TrackingShipmentIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			TrackingShipment result = Factory.New<TrackingShipment>();

			BaseJobDeclaration expDeclaration = Factory.New<BaseJobDeclaration>();
			expDeclaration.JE_MessageType = "EXP";
			expDeclaration.JE_JS = result.PK;
			expDeclaration.JE_OwnerRef = "EXP";

			BaseJobDeclaration impDeclaration = Factory.New<BaseJobDeclaration>();
			impDeclaration.JE_MessageType = "IMP";
			impDeclaration.JE_GB = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew().PK;
			impDeclaration.JE_JS = result.PK;
			impDeclaration.JE_OwnerRef = "IMP";

			ForwardingConsol dummyConsol = result.Consols.AddNew();

			result.AttachedOrders.AddNew();
			result.AttachedOrders.AddNew();

			AssertEquals("Two Declarations", 2, result.Declarations.Length);
			AssertEquals("One Consol", 1, result.Consols.Count);
			AssertEquals("Two Orders", 2, result.AttachedOrders.Count);

			expDeclaration.CustomsEntryHeaders.AddNew();
			expDeclaration.CustomsEntryHeaders.AddNew();
			impDeclaration.CustomsEntryHeaders.AddNew();

			expDeclaration.Invoices.AddNew();
			impDeclaration.Invoices.AddNew();
			impDeclaration.Invoices.AddNew();

			fExpectedDocRelatedPKs = new ZGuid[] { expDeclaration.PK, impDeclaration.PK, expDeclaration.CustomsEntryHeaders[0].PK, expDeclaration.CustomsEntryHeaders[1].PK,
				impDeclaration.CustomsEntryHeaders[0].PK, expDeclaration.Invoices[0].PK, impDeclaration.Invoices[0].PK, impDeclaration.Invoices[0].PK, result.AttachedOrders[0].PK, result.AttachedOrders[1].PK, result.PK };

			return result;
		}

		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return fExpectedDocRelatedPKs; }
		}
		ZGuid[] fExpectedDocRelatedPKs = System.Array.Empty<ZGuid>();
	}
}
