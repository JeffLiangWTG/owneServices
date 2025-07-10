using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterFiles.Business
{
	public interface IMasterDataProvider
	{
		ISupportDuplicationFinder CreateOrgDuplicationFinder(OrgHeader header);
		ISupportDuplicationFinder CreatePersonDuplicationFinder(GlbPerson person, bool forAdminPanel);
		void FindOrgDuplicates(OrgHeader header);
		void FindPersonDuplicates(GlbPerson person);
		List<ISupportDuplicationFinder> GetPersonSupportedDuplicationFinders(GlbPerson person, DeduplicationProxyConfig config);
		IEnumerable<ScoringResult> FindPotentialOrgDuplicatesForAdminPanel(OrgHeader orgHeader);
		IEnumerable<ScoringResult> FindPotentialPersonDuplicatesForAdminPanel(GlbPerson person);
		void ComputeIsExcludedFromDeduplication(OrgHeader header, bool value);
		void ComputeIsExcludedFromDeduplication(GlbPerson person, bool value);
		IPatternMatchingRegenerator<OrgHeader>[] GetOrganisationPatternMatchingRegenerationEntities(PatternMatchingRecalculator<OrgHeader> recalculator);
		List<ISupportDuplicationFinder> GetOrganisationSupportedDuplicationFinders(OrgHeader header, Type targetType, DeduplicationProxyConfig config);
		List<IOrgHeader> GetTargetLists(OrgHeader orgTarget1, OrgHeader orgTarget2);
		List<IOrgContact> GetContactTargetLists(OrgContact target1, OrgContact target2);
		string GetPersonDuplicationFinderMessageForTest(OrgContact contact, OrgContact target);
		IPatternMatchingRegenerator<GlbPerson>[] GetPersonPatternMatchingRegenerationEntities(PatternMatchingRecalculator<GlbPerson> recalculator);
		IDeduplicationMaster GetDeduplicationOrgHeader(OrgHeader header);
		IDeduplicationMaster GetDeduplicationGlbPerson(GlbPerson person);
		IGlbPerson CreateIGlbPerson(GlbPerson person, bool isDummy);
	}
}
