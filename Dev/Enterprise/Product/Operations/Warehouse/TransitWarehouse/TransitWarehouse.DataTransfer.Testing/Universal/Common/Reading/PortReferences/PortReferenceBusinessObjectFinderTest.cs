using System.Linq;
using CargoWise.Definitions;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(PortReferenceBusinessObjectFinder<CusEntryNumber>))]
	public class PortReferenceBusinessObjectFinderTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectPortReferences>();
			Helper.CreateCustomsAdditionalReference(parent, "PAN", "PANReference", TransitWarehouseReferenceCategories.Codes.PortReference, "HLD", "AU");
			var reference1 = Helper.CreatePortReference("PAN", "Port reference Desc", "AU", "PANRef", "CLR");

			var finder = new PortReferenceBusinessObjectFinder<CusEntryNumber>(reference1);
			var matchingReference = finder.Find(parent);
			AssertEquals(matchingReference.PK, parent.PortReferences.Cast<CusEntryNumber>().Single().PK);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}
}
