using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.AutoRating.Testing
{
	public class JobDateTypeRetrieverTest : TestCaseWithFactory
	{
		public void TestAllChargeCodeGroupsCorrespondToNotEmptyJobDateType()
		{
			var chargeCodeGroupExclusions = new[] { ChargeCodeGroupList.Codes.NonJobRelated, ChargeCodeGroupList.Codes.NotGrouped };

			var chargeCodeGroups = new ChargeCodeGroupList()
				.Cast<CodeDescriptionPair>()
				.Where(x => !chargeCodeGroupExclusions.Contains(x.Code));

			foreach (var chargeCodeGroup in chargeCodeGroups)
			{
				Assert($"{chargeCodeGroup.CodeAndDescription} is expected to correspond to a non-empty JobDateType",
					JobDateTypeRetriever.GetStandardJobDateTypeByChargeGroup(chargeCodeGroup.Code).DateType != ZString.Empty);
			}
		}
	}
}
