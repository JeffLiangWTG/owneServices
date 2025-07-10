
namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class RelationshipIndicatorListPartialTest : TestCaseWithFactory
	{
		public void TestRelationshipIndicatorList()
		{
			AssertEquals("Related", true, new RelationshipIndicatorList(false).ContainsCode(RelationshipIndicatorList.Codes.Related));
			AssertEquals("NotRelated", true, new RelationshipIndicatorList(false).ContainsCode(RelationshipIndicatorList.Codes.NotRelated));
			AssertEquals("RelatedDoesNotAffectPrice", false, new RelationshipIndicatorList(false).ContainsCode(RelationshipIndicatorList.Codes.RelatedDoesNotAffectPrice));

			AssertEquals("RelatedDoesNotAffectPrice", true, new RelationshipIndicatorList(true).ContainsCode(RelationshipIndicatorList.Codes.RelatedDoesNotAffectPrice));
		}
	}
}
