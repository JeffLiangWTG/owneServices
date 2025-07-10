using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrgHeaderDuplicationFinder : DuplicationFinder<OrgHeader, OrgHeader, IOrgHeader>
	{
		public OrgHeaderDuplicationFinder(OrgHeader header, bool useMaxRecords, IDeduplicationStrategy strategy)
			: this(header, true, useMaxRecords)
		{
			Strategy = strategy;
		}

		public OrgHeaderDuplicationFinder(OrgHeader header, bool shouldUseCache, bool useMaxRecords)
			: base(header, shouldUseCache)
		{
			if (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value && header != null && ((IDeduplicatable)header).ShouldRunDeduplication)
			{
				MasterGlow = new DeduplicationOrgHeader(header);
				MaxScoringResult = useMaxRecords ? MaxRecords : MinRecords;
			}
		}

		OrgHeaderDuplicationFinder(OrgHeader header, bool shouldUseCache, bool useMaxRecords, int timeout)
			: this(header, shouldUseCache, useMaxRecords)
		{
			DuplicateDetectionTimeout = timeout;
		}

		// Tested in MasterFiles.DataTransfer
		public OrgHeaderDuplicationFinder(DeduplicationOrgHeader deduplicationOrgHeader, bool shouldUseCache, bool useMaxRecords)
			: base(null, shouldUseCache)
		{
			forUXMLMatching = true;
			MasterGlow = deduplicationOrgHeader;
			MaxScoringResult = useMaxRecords ? MaxRecords : MinRecords;
			DuplicateDetectionTimeout = 120;
		}

		readonly bool forUXMLMatching;

		protected override bool ShouldFindDuplications => forUXMLMatching ? MasterGlow != null : base.ShouldFindDuplications;

		protected override int MaxScoringResult
		{
			get;
		}
		const int MaxRecords = 100;
		const int MinRecords = 4;

		protected override IOrgHeader MasterGlow { get; }

		public static OrgHeaderDuplicationFinder CreateInstance(OrgHeader header, DeduplicationProxyConfig config)
		{
			config = config ?? new DeduplicationProxyConfig();
			config.ShouldInvokeDeduplicationEvents = true;

			return new OrgHeaderDuplicationFinder(header, true, config.UseMaxRecords);
		}

		public static OrgHeaderDuplicationFinder CreateInstance(OrgHeader header, bool useMaxRecords = false)
		{
			return new OrgHeaderDuplicationFinder(header, true, useMaxRecords);
		}

		public static OrgHeaderDuplicationFinder CreateInstance(OrgHeader header, bool useMaxRecords, int timeOut)
		{
			return new OrgHeaderDuplicationFinder(header, true, useMaxRecords, timeOut);
		}

		protected override Guid GetGlowPK(IOrgHeader glowModel)
		{
			return glowModel.OH_PK;
		}

		protected override ITargetFinderController CreateTargetFinderController(IOrgHeader glowModel)
		{
			return new TargetFinderController(glowModel);
		}

		IDeduplicationStrategy Strategy { get; } = new OrganisationDeduplicationStrategy();

		protected override IEnumerable<IGrouping<Guid, PatternMatchingResultModel>> FindTargetPKsAndResultModels(PatternMatchingResultModel[] patternMatchingResults)
		{
			if (MasterGlow == null)
			{
				var message = (NoResString)"You have setup duplication finder with No MasterGlow record. Please check and confirm your setup conditions";
				ExceptionReporter.Instance.ReportDeveloperException("efa7bf6c-ca3f-41b1-b378-1b19ca7a86b0", message, new ArgumentNullException(message));
				return Enumerable.Empty<IGrouping<Guid, PatternMatchingResultModel>>();
			}

			return patternMatchingResults
				.Where(result =>
					result.ParentTablePrefix != JobDocAddressSchema.Constants.Prefix &&
					result.OrgPK != MasterGlow.OH_PK)
				.GroupBy(x => x.OrgPK)
				.OrderByDescending(x => x.Count()).Take(Strategy.MaximumPKs())
				.ToArray();
		}

		protected override IEnumerable<OrgHeader> LoadTargetBizOs(IFactory factory, HashSet<Guid> candidatePKs)
		{
			var results = LoadOrgTargetBizOs(factory, candidatePKs, MasterGlow, bizo, forUXMLMatching, exclusionManager);

			return GetOrderedList(results, candidatePKs);
		}

		public static OrgHeader[] LoadOrgTargetBizOs(IFactory factory, HashSet<Guid> candidatePKs, IOrgHeader masterGlow, OrgHeader bizo, bool forUXMLMatching, DeduplicationExclusionManager<OrgHeader> exclusionManager)
		{
			if (masterGlow == null)
			{
				var message = (NoResString)"You have setup duplication finder with No MasterGlow record. Please check and confirm your setup conditions";
				ExceptionReporter.Instance.ReportDeveloperException("efa7bf6c-ca3f-41b1-b378-1b19ca7a86b0", message, new ArgumentNullException(message));
				return Array.Empty<OrgHeader>();
			}

			OrgHeader[] results;

			if (!forUXMLMatching)
			{
				var query = GetExclusionQuery(typeof(OrgHeader), candidatePKs, bizo, exclusionManager);

				// exclude all children, relation = any
				var queryRelatedParty = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent, true);
				queryRelatedParty.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, masterGlow.OH_PK);
				query.AddSubQuery(queryRelatedParty, JoinCondition.And);

				exclusionManager?.Builder.BuildExclusion(
					DeduplicationExclusionQueries.GetRelatedPartyAsChildren(GetExclusionQueryBase(typeof(OrgHeader), candidatePKs, bizo), masterGlow.OH_PK),
					ResString.GetMultilingualString("ee4cb03e-84bd-4ad0-92fe-4bc2ebc1279b", "The record is a related party")
					);

				// exclude all parent, relation = any
				var queryRelatedPartyReversed = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_RelatedParty, true);
				queryRelatedPartyReversed.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, masterGlow.OH_PK);
				query.AddSubQuery(queryRelatedPartyReversed, JoinCondition.And);

				exclusionManager?.Builder.BuildExclusion(
					DeduplicationExclusionQueries.GetRelatedPartyAsParent(GetExclusionQueryBase(typeof(OrgHeader), candidatePKs, bizo), masterGlow.OH_PK),
					ResString.GetMultilingualString("ee4cb03e-84bd-4ad0-92fe-4bc2ebc1279b", "The record is a related party")
					);

				var masterGlowDeduplicationOrgHeader = (DeduplicationOrgHeader)masterGlow;
				// exclude parent, children and siblings in all levels, relation = management
				var relatedOrgs = masterGlowDeduplicationOrgHeader.AllRelatedOrganizationsPK;
				if (relatedOrgs.Count > 0)
				{
					var queryAllRelatedOrgs = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK, true);
					queryAllRelatedOrgs.AddToFilter(OrgHeaderSchema.PK, relatedOrgs);
					query.AddSubQuery(queryAllRelatedOrgs, JoinCondition.And);
				}

				var regSettingExcludeOtherCountries = OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.Value;
				var regSettingExcludeInactive = OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.Value;
				var resultTargets = factory.Load<OrgHeader>(query);

				exclusionManager?.Builder.BuildExclusion(
					resultTargets.AsQueryable(),
					ResString.GetMultilingualString("d80cf321-0683-4cc0-bf1a-af21d71aafd5", "The record is from another country/region"),
					org => org.PK != masterGlow.OH_PK && regSettingExcludeOtherCountries && GetOrgCountryCodeWithFallbackLogic(org) != masterGlowDeduplicationOrgHeader.CountryCode);

				exclusionManager?.Builder.BuildExclusion(
					resultTargets.AsQueryable(),
					ResString.GetMultilingualString("42581a58-c809-4d3d-a331-20359b07764a", "The record is inactive"),
					org => org.PK != masterGlow.OH_PK && regSettingExcludeInactive && !org.OH_IsActive);

				results = factory
					.Load<OrgHeader>(query)
					.Where(org => org.PK != masterGlow.OH_PK &&
					(!regSettingExcludeOtherCountries || GetOrgCountryCodeWithFallbackLogic(org) == masterGlowDeduplicationOrgHeader.CountryCode) &&
					(!regSettingExcludeInactive || org.OH_IsActive == regSettingExcludeInactive))
					.ToArray();
			}
			else
			{
				var queryForCandidatePKs = new ZQuery(OrgHeaderSchema.PK, candidatePKs);
				results = factory.Load<OrgHeader>(queryForCandidatePKs);
			}

			return results;
		}

		IEnumerable<OrgHeader> potentialResults;

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an async method.")]
		public override async Task<IEnumerable<OrgHeader>> GetPotentialTargetsByEmailDomainAsync(string emailAddress)
		{
			potentialResults = Enumerable.Empty<OrgHeader>();
			var instance = Task.Factory.StartNew(() =>
			{
				var result = Enumerable.Empty<OrgHeader>();
				var timeoutExecution = new DeduplicationTimeout<IEnumerable<OrgHeader>>(TimeSpan.FromSeconds(DuplicateDetectionTimeout));
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						result = potentialResults = timeoutExecution.DoWork(() => MasterDataMatchFinder.GetOrgHeaderByEmailDomain(emailAddress));
					}
				}
				catch (TimeoutException)
				{
					ShouldStopProcessing = true;
					result = potentialResults;
					((ISupportDuplicationFinder)this).LastRunStatus = DuplicationStatus.Timeout;
				}
				return result;
			}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, Scheduler);

			return await instance.WithExceptionHandler(defaultResult: Enumerable.Empty<OrgHeader>(), exception =>
			{
				HandleException(exception, nameof(GetPotentialTargetsByEmailDomainAsync));
			});
		}

		protected override IOrgHeader ConvertMasterToGlowModel(OrgHeader targetBizo)
		{
			return ConvertOrgToGlowModel(targetBizo);
		}

		public static IOrgHeader ConvertOrgToGlowModel(OrgHeader targetBizo)
		{
			return new DeduplicationOrgHeader(targetBizo);
		}

		protected override IOrgHeader ConvertTargetToGlowModel(OrgHeader targetBizo)
		{
			return ConvertMasterToGlowModel(targetBizo);
		}

		protected override ScoringResult ScoreGlowModel(IOrgHeader master, IOrgHeader target)
		{
			return TargetScorerController.Score(master, target, false);
		}

		protected override (string Part1, string Part2) GenerateCacheSubkey(IOrgHeader targetGlow)
		{
			var targetDeduplicationOrgHeader = (DeduplicationOrgHeader)targetGlow;
			return (targetDeduplicationOrgHeader.OH_FullName, targetDeduplicationOrgHeader.CountryCode);
		}

		public static void StandardizeMaster(IOrgHeader masterGlow, string country)
		{
			if (masterGlow == null)
			{
				throw new ArgumentException("Deduplication cannot be called if MasterGlow is null");
			}

			foreach (var address in masterGlow.OrgAddresses)
			{
				if (address != null)
				{
					if (TextStandardizerHelper.IsPlaceholderAddress(address))
					{
						address.OA_Address1 = string.Empty;
						address.OA_Address2 = string.Empty;
						address.OA_City = string.Empty;
						address.OA_PostCode = string.Empty;
						address.OA_State = string.Empty;
						address.OA_ValidationStatus = MasterFiles.Integration.AddressValidationStatus.Invalid;
					}

					address.OA_Email = TextStandardizerHelper.StandardizeEmail(address.OA_Email);
					address.OA_Phone = TextStandardizerHelper.StandardizePhone(address.OA_Phone);
					address.OA_Mobile = TextStandardizerHelper.StandardizePhone(address.OA_Mobile);
					address.OA_Fax = TextStandardizerHelper.StandardizePhone(address.OA_Fax);
				}
			}

			foreach (var contact in masterGlow.OrgContacts)
			{
				if (contact != null)
				{
					contact.OC_ContactName = TextStandardizerHelper.IsPlaceholderPersonName(contact.OC_ContactName) ? contact.OC_ContactName : TextStandardizerHelper.StandardizePersonName(contact.OC_ContactName);
					contact.OC_Email = TextStandardizerHelper.StandardizeEmail(contact.OC_Email);
					contact.OC_Phone = TextStandardizerHelper.StandardizePhone(contact.OC_Phone);
					contact.OC_Mobile = TextStandardizerHelper.StandardizePhone(contact.OC_Mobile);
					contact.OC_OtherPhone = TextStandardizerHelper.StandardizePhone(contact.OC_OtherPhone);
					contact.OC_HomePhone = TextStandardizerHelper.StandardizePhone(contact.OC_HomePhone);
					contact.OC_Fax = TextStandardizerHelper.StandardizePhone(contact.OC_Fax);
				}
			}

			foreach (var url in masterGlow.OrgWebURLs)
			{
				url.PU_URL = TextStandardizerHelper.IsPlaceholderUrl(url.PU_URL) ? string.Empty : url.PU_URL;
			}

			foreach (var code in masterGlow.CusCodes)
			{
				code.OK_CustomsRegNo = TextStandardizerHelper.StandardizeRegCode(code.OK_CustomsRegNo);
			}

			foreach (var brandName in masterGlow.OrgBrandOrRelatedNames)
			{
				brandName.P1_RelatedName = TextStandardizerHelper.StandardizeCompanyName(brandName.P1_RelatedName, country);
			}

			masterGlow.OH_FullName = TextStandardizerHelper.StandardizeCompanyName(masterGlow.OH_FullName, country);
		}

		protected override void StandardizeMaster()
		{
			var masterDeduplicationOrgHeader = (DeduplicationOrgHeader)MasterGlow;
			StandardizeMaster(masterDeduplicationOrgHeader, masterDeduplicationOrgHeader.CountryCode);
		}

		protected override bool IsBizoDirty(OrgHeader obj)
		{
			return IsBizoDirty(obj, new DirtyRecordFinder(obj));
		}

		protected bool IsBizoDirty(OrgHeader obj, IDirtyRecordFinder dirtyRecordFinder)
		{
			using (var timer = new DeduplicationPerformanceMonitor())
			{
				var isDirty = dirtyRecordFinder.IsOrgDirtyForDeduplication();

				if (isDirty)
				{
					DebuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName,
						new List<DeduplicationCustomMessage>
						{
							new DeduplicationCustomMessage
							{
								Message = (NoResString)"Record is marked as dirty",
								Reason = dirtyRecordFinder.GetDirtyReason(),
								Org = string.Join(" ", obj.OH_FullName, "(" + obj.PK.ToGuid() + ")" )
							}
						}, nameof(IsBizoDirty), timer.ElapsedDuration, DeduplicationDebuggerParticipant.OrganizationPrefix);
					obj.PropagateDeduplicationEnded(null, null, null, DuplicationStatus.OK, new DeduplicationExclusionManager<OrgHeader>());   //	null parameters should ensure duplicates are not run
				}

				return isDirty;
			}
		}

		public static ZString GetOrgCountryCodeWithFallbackLogic(OrgHeader org)
		{
			return org.CountryCode.IsEmpty ? org.MainAddress.OA_RN_NKCountryCode : org.CountryCode;
		}

		protected override Guid GetPatternMatchingResultModelParentPK(PatternMatchingResultModel patternMatchingResultModel)
		{
			return patternMatchingResultModel.OrgPK;
		}

		protected override int RegistryTimeout => OrganisationsDataRegistry.Instance.DuplicateDetectionTimeout.Value;

		protected override bool EndableDeduplicationFinderCheckpoint => OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
	}
}
