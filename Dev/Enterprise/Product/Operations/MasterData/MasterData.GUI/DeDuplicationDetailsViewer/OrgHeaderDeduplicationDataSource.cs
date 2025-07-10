using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterData.Common.Deduplication.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.GUI
{
	public class OrgHeaderDeduplicationDataSource : DeduplicationDataSource<IOrgHeader, DuplicationOrganisationCandidate>
	{
		public OrgHeaderDeduplicationDataSource(DeduplicationOrgHeader master, IEnumerable<IDeduplicationGlowObject> targets, IEnumerable<DeduplicationPresenterModel> results, Dictionary<ScoringResult, PatternMatchingResult> resultDictionary = null)
			: base(master, targets.Cast<DeduplicationOrgHeader>(), results)
		{
			ResultDictionary = resultDictionary;
		}

		internal readonly Dictionary<ScoringResult, PatternMatchingResult> ResultDictionary;

		protected override string CountryCode
		{
			get
			{
				return ((DeduplicationOrgHeader)MasterGlow).CountryCode;
			}
		}

		protected override Guid GetPK(IOrgHeader target)
		{
			return target.OH_PK;
		}

		#region WPF

		[SuppressMessage("Microsoft.Maintainability", "CA1502")]
		protected override PotentialDuplicationModel GetPotentialDuplicationModel(IOrgHeader currentBizo, IGrouping<Guid, DeduplicationPresenterModel> target)
		{
			var dedupOrgHeader = currentBizo as DeduplicationOrgHeader;
			var model = target.FirstOrDefault();

			IEDIOrgHeader edi = null;

			if (ZArchitecture.Modules.ClientHookLoader.Instance?.Client == Clients.EDI)
			{
				edi = (IEDIOrgHeader)FactoryForLoadEDIOrgHeader.Load<OrgHeader>(dedupOrgHeader.OH_PK);
			}

			var result = new PotentialDuplicationModel
			{
				Code = dedupOrgHeader != null ? dedupOrgHeader.OH_Code : string.Empty,
				Confidence = model != null ? model.Confidence : ConfidenceRating.Undefined,
				Name = dedupOrgHeader?.OH_FullName,
				OrganisationType = dedupOrgHeader?.OrganisationTypesAsString.Length > 0 ? dedupOrgHeader.OrganisationTypesAsString : ZString.Empty,
				DebtorCompany = dedupOrgHeader?.DebtorCompany,
				CreditorCompany = dedupOrgHeader?.CreditorCompany,
				PK = model != null ? target.Key : Guid.Empty,
				Score = model != null ? model.MainScore : 0,
				UNLOCO = dedupOrgHeader?.UNLOCO?.RL_Code,
				DeduplicationPresenterModels = target,
				IsActive = (dedupOrgHeader?.OH_IsActive ?? false),
				IsDummy = (dedupOrgHeader?.IsDummy ?? false),
				EnterpriseId = edi?.LicenceEnterpriseID,
				EnterpriseCode = edi?.LicenceEnterpriseCode,
				CompanyCode = edi?.CompanyCode,
				ProductId = ProductStringFromList(edi?.ProductId),
			};

			if (ResultDictionary != null)
			{
				var matchingResult = ResultDictionary.Values.FirstOrDefault(r => !r.IsDeleted && r.PMT_TargetPK == target.Key);
				if (matchingResult != null && !string.IsNullOrEmpty(matchingResult.FinalStatus))
				{
					result.Status = matchingResult.FinalStatus;
					result.StatusDescription = matchingResult.FinalStatusDescription;
					result.IgnoredByStaff = matchingResult.ExcludedByList;
					result.IgnoredForEveryoneStaff = matchingResult.ExcludedByForEveryone;
				}
			}

			return result;
		}

		#endregion WPF

		#region WinForm

		protected override DuplicationOrganisationCandidate GetDuplicationCandidate(IOrgHeader currentBizo, IGrouping<Guid, DeduplicationPresenterModel> target)
		{
			var dedupOrgHeader = currentBizo as DeduplicationOrgHeader;
			var model = target.FirstOrDefault();

			var result = new DuplicationOrganisationCandidate(model, dedupOrgHeader)
			{
				TargetPK = model != null ? target.Key : Guid.Empty,
				DeduplicationPresenterModels = target,
			};

			if (ResultDictionary != null)
			{
				var matchingResult = ResultDictionary.Values.FirstOrDefault(r => !r.IsDeleted && r.PMT_TargetPK == target.Key);
				result.SetAdminPanelProperties(matchingResult);
			}

			return result;
		}

		protected override IEnumerable<DuplicationOrganisationCandidate> RatingCandidates(IEnumerable<DuplicationOrganisationCandidate> candidates)
		{
			var ratedCandidates = new List<DuplicationOrganisationCandidate>();

			if (candidates.Any())
			{
				var ratingGroups = candidates.OrderByDescending(x => x.Confidence).GroupBy(x => x.Confidence);

				foreach (var group in ratingGroups)
				{
					var groupSimilarToMaster = group.Where(x => !string.IsNullOrEmpty(x.UNLOCO) && x.UNLOCO.ToString().StartsWith(CountryCode, StringComparison.Ordinal)).OrderByDescending(x => x.Score);
					var sortedGroup = group.Where(x => !string.IsNullOrEmpty(x.UNLOCO) && !x.UNLOCO.ToString().StartsWith(CountryCode, StringComparison.Ordinal)).OrderByDescending(x => x.Score).ThenBy(x => x.UNLOCO);
					var invalidGroup = group.Where(x => string.IsNullOrEmpty(x.UNLOCO) && !x.IsDummy).OrderByDescending(x => x.Score);

					ratedCandidates.AddRange(groupSimilarToMaster);
					ratedCandidates.AddRange(sortedGroup);
					ratedCandidates.AddRange(invalidGroup);
				}
			}

			return ratedCandidates;
		}

		#endregion WinForm

		ReadOnlyBusinessObjectFactory factoryForLoadEDIOrgHeader;
		ReadOnlyBusinessObjectFactory FactoryForLoadEDIOrgHeader => factoryForLoadEDIOrgHeader ?? (factoryForLoadEDIOrgHeader = new ReadOnlyBusinessObjectFactory());

		ZString ProductStringFromList(List<ZString> list)
		{
			var array = list?.Distinct().ToArray();
			return array != null ? ZString.Join(", ", array) : null;
		}
	}
}
