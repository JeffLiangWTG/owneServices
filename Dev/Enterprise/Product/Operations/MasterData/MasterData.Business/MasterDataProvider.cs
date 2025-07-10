using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class MasterDataProvider : IMasterDataProvider
	{
		public void ComputeIsExcludedFromDeduplication(OrgHeader header, bool value)
		{
			DeduplicationResponseStatus result;

			if (value)
			{
				result = OrgHeaderDuplicationFinderProxy.GetInstance(header).AddExclusion(GlbStaff.CurrentUser.GS_Code);
			}
			else
			{
				result = OrgHeaderDuplicationFinderProxy.GetInstance(header).RemoveExclusion();
			}

			if (result.Message != DuplicationResponseMessages.Success.ToString())
			{
				throw new InvalidOperationException(result.Message);
			}
		}

		public void ComputeIsExcludedFromDeduplication(GlbPerson person, bool value)
		{
			DeduplicationResponseStatus result;

			if (value)
			{
				result = GlbPersonDuplicationFinderProxy.GetInstance(person).AddExclusion(GlbStaff.CurrentUser.GS_Code);
			}
			else
			{
				result = GlbPersonDuplicationFinderProxy.GetInstance(person).RemoveExclusion();
			}

			if (result.Message != DuplicationResponseMessages.Success.ToString())
			{
				throw new InvalidOperationException(result.Message);
			}
		}

		public IGlbPerson CreateIGlbPerson(GlbPerson person, bool isDummy)
		{
			return new DeduplicationGlbPerson(person, isDummy);
		}

		public ISupportDuplicationFinder CreateOrgDuplicationFinder(OrgHeader header)
		{
			return OrgHeaderDuplicationFinderProxy.GetInstance(header, new DeduplicationProxyConfig() { UseMaxRecords = false });
		}

		public ISupportDuplicationFinder CreatePersonDuplicationFinder(GlbPerson person, bool forAdminPanel)
		{
			if (!forAdminPanel)
			{
				return GlbPersonDuplicationFinderProxy.GetInstance(person, new DeduplicationProxyConfig() { UseMaxRecords = false });
			}

			return DoCreatePersonDuplicationFinderForAdminPanel(person);
		}

		public void FindOrgDuplicates(OrgHeader header)
		{
			OrgHeaderDuplicationFinderProxy
				.GetInstance(header, new DeduplicationProxyConfig() { UseMaxRecords = false })
				.GetPotentialDuplicatesAsync(GlbStaff.CurrentUser.GS_Code)
				.ConfigureAwait(false);
		}

		public void FindPersonDuplicates(GlbPerson person)
		{
			GlbPersonDuplicationFinderProxy.GetInstance(person, new DeduplicationProxyConfig() { UseMaxRecords = false })
				.GetPotentialDuplicatesAsync(GlbStaff.CurrentUser.GS_Code)
				.ConfigureAwait(false);
		}

		public IEnumerable<ScoringResult> FindPotentialOrgDuplicatesForAdminPanel(OrgHeader orgHeader)
		{
			IEnumerable<ScoringResult> result = null;

			if (!new DirtyRecordFinder(orgHeader).IsOrgDirtyForDeduplication())
			{
				result = new OrgHeaderDuplicationFinderWithThresholdOverride(orgHeader, useMaxRecords: true, new AdminPanelDeduplicationStrategy()).FindPotentialDuplicates(true);
			}

			return result;
		}

		public IEnumerable<ScoringResult> FindPotentialPersonDuplicatesForAdminPanel(GlbPerson person)
		{
			var finder = DoCreatePersonDuplicationFinderForAdminPanel(person);
			return finder.FindPotentialDuplicates(true);
		}

		GlbPersonDuplicationFinderWithThresholdOverride DoCreatePersonDuplicationFinderForAdminPanel(GlbPerson person)
		{
			return new GlbPersonDuplicationFinderWithThresholdOverride(person, useMaxRecords: true, new AdminPanelDeduplicationStrategy());
		}

		public List<IOrgContact> GetContactTargetLists(OrgContact target1, OrgContact target2)
		{
			return new List<IOrgContact>
			{
				new DeduplicationOrgContact(target1, true),
				new DeduplicationOrgContact(target2, true)
			};
		}

		public IDeduplicationMaster GetDeduplicationGlbPerson(GlbPerson person)
		{
			return new DeduplicationGlbPerson(person);
		}

		public IDeduplicationMaster GetDeduplicationOrgHeader(OrgHeader header)
		{
			return new DeduplicationOrgHeader(header);
		}

		public IPatternMatchingRegenerator<OrgHeader>[] GetOrganisationPatternMatchingRegenerationEntities(PatternMatchingRecalculator<OrgHeader> recalculator)
		{
			var list = new List<IPatternMatchingRegenerator<OrgHeader>>
					{
						new OrganisationPatternMatchingAddressRegenerator(recalculator),
						new OrganisationPatternMatchingDomainRegenerator(recalculator),
						new OrganisationPatternMatchingEmailRegenerator(recalculator),
						new OrganisationPatternMatchingNameRegenerator(recalculator),
						new OrganisationPatternMatchingPhoneRegenerator(recalculator),
						new OrganisationPatternMatchingRegCodeRegenerator(recalculator)
					};

			return list.ToArray();
		}

		public List<ISupportDuplicationFinder> GetOrganisationSupportedDuplicationFinders(OrgHeader header, Type targetType, DeduplicationProxyConfig config)
		{
			return new List<ISupportDuplicationFinder>
			{
				OrgHeaderDuplicationFinder.CreateInstance(header, config)
			};
		}

		public string GetPersonDuplicationFinderMessageForTest(OrgContact contact, OrgContact target)
		{
			var finder = GlbPersonDuplicationFinderProxy.GetInstance(contact.Person);

			return finder.CompareBizOs(target.Person, "E").Message;
		}

		public IPatternMatchingRegenerator<GlbPerson>[] GetPersonPatternMatchingRegenerationEntities(PatternMatchingRecalculator<GlbPerson> recalculator)
		{
			var list = new List<IPatternMatchingRegenerator<GlbPerson>>
			{
				new PersonPatternMatchingAddressRegenerator(recalculator),
				new PersonPatternMatchingEmailRegenerator(recalculator),
				new PersonPatternMatchingNameRegenerator(recalculator),
				new PersonPatternMatchingPhoneRegenerator(recalculator),
				new PersonPatternMatchingRegCodeRegenerator(recalculator)
			};

			return list.ToArray();
		}

		public List<ISupportDuplicationFinder> GetPersonSupportedDuplicationFinders(GlbPerson person, DeduplicationProxyConfig config)
		{
			return new List<ISupportDuplicationFinder>
			{
				GlbPersonDuplicationFinder.CreateInstance(person, config)
			};
		}

		public List<IOrgHeader> GetTargetLists(OrgHeader orgTarget1, OrgHeader orgTarget2)
		{
			return new List<IOrgHeader>()
			{
				new DeduplicationOrgHeader(orgTarget1),
				new DeduplicationOrgHeader(orgTarget2)
			};
		}
	}
}
