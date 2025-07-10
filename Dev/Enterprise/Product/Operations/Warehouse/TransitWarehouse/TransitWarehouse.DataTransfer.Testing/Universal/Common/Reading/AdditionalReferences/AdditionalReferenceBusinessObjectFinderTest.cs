using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AdditionalReferenceBusinessObjectFinder<CusEntryNumber>))]
	public class AdditionalReferenceBusinessObjectFinderTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObjectWithCusEntryReferences>();
			Helper.CreateAdditionalReference(parent, "Test Reference", "TES");

			var reference = helper.CreateAdditionalReference("TES", "Test Type TES", "Test Reference", string.Empty, ZDateTime.Empty);

			var finder = new AdditionalReferenceBusinessObjectFinder<CusEntryNumber>(reference);
			var matchingReference = finder.Find(parent);
			AssertEquals(matchingReference.PK, parent.CusEntryNumReferences.Cast<CusEntryNumber>().Single().PK);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}
}
