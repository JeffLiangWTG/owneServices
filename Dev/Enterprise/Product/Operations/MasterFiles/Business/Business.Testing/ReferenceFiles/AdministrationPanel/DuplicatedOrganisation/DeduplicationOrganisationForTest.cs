using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DeduplicationOrganisationForTest : DeduplicationOrganisation
	{
		public DeduplicationOrganisationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static IEnumerable<ScoringResult> DuplicationScoringResultForTest;

		protected override IEnumerable<ScoringResult> FindPotentialDuplicatesCore()
		{
			return DuplicationScoringResultForTest ?? base.FindPotentialDuplicatesCore();
		}

		public IEnumerable<ScoringResult> FindPotentialDuplicatesCore_ForTest()
		{
			return base.FindPotentialDuplicatesCore();
		}

		public ISupportDuplicationFinder DuplicationFinderForTest;

		protected override ISupportDuplicationFinder GetDuplicationFinder(OrgHeader org) => DuplicationFinderForTest ?? base.GetDuplicationFinder(org);
	}
}
