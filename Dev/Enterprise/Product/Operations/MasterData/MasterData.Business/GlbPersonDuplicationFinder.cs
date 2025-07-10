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
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Business
{
	public class GlbPersonDuplicationFinder : DuplicationFinder<GlbPerson, GlbPerson, IGlbPerson>
	{
		public GlbPersonDuplicationFinder(GlbPerson person, bool useMaxRecords, IDeduplicationStrategy strategy)
			: this(person, true, useMaxRecords)
		{
			Strategy = strategy;
		}

		public GlbPersonDuplicationFinder(GlbPerson person, bool useMaxRecords)
			: this(person, true, useMaxRecords)
		{
		}

		public GlbPersonDuplicationFinder(GlbPerson person, bool shouldUseCache, bool useMaxRecords)
			: base(person, shouldUseCache)
		{
			if (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value && person != null && ((IDeduplicatable)person).ShouldRunDeduplication)
			{
				MasterGlow = person.CreateIGlbPerson();
				MaxScoringResult = useMaxRecords ? maxResults : base.MaxScoringResult;
			}
		}

		GlbPersonDuplicationFinder(GlbPerson person, bool shouldUseCache, bool useMaxRecords, int timeout)
			: this(person, shouldUseCache, useMaxRecords)
		{
			DuplicateDetectionTimeout = timeout;
		}

		protected override IGlbPerson MasterGlow { get; }

		public static GlbPersonDuplicationFinder CreateInstance(GlbPerson person)
		{
			return new GlbPersonDuplicationFinder(person, true);
		}

		public static GlbPersonDuplicationFinder CreateInstance(GlbPerson person, bool useMaxRecords = false)
		{
			return new GlbPersonDuplicationFinder(person, true, useMaxRecords);
		}

		public static GlbPersonDuplicationFinder CreateInstance(GlbPerson person, bool useMaxRecords, int timeOut)
		{
			return new GlbPersonDuplicationFinder(person, true, useMaxRecords, timeOut);
		}

		public static GlbPersonDuplicationFinder CreateInstance(GlbPerson person, DeduplicationProxyConfig config)
		{
			config = config ?? new DeduplicationProxyConfig();
			config.ShouldInvokeDeduplicationEvents = true;

			return new GlbPersonDuplicationFinder(person, true, config.UseMaxRecords);
		}

		protected override Guid GetGlowPK(IGlbPerson glowModel)
		{
			return glowModel.PER_PK;
		}

		protected override ITargetFinderController CreateTargetFinderController(IGlbPerson glowModel)
		{
			return new TargetFinderController(glowModel);
		}

		IDeduplicationStrategy Strategy { get; } = new PersonDeduplicationStrategy();

		protected override IEnumerable<IGrouping<Guid, PatternMatchingResultModel>> FindTargetPKsAndResultModels(PatternMatchingResultModel[] patternMatchingResults)
		{
			if (MasterGlow == null)
			{
				var message = (NoResString)"You have setup duplication finder with No MasterGlow record. Please check and confirm your setup conditions";
				ExceptionReporter.Instance.ReportDeveloperException("efa7bf6c-ca3f-41b1-b378-1b19ca7a86b0", message, new ArgumentNullException(message));
				return Enumerable.Empty<IGrouping<Guid, PatternMatchingResultModel>>();
			}

			return patternMatchingResults
				.Where(x => x.PersonPK != MasterGlow.PER_PK)
				.GroupBy(x => x.PersonPK)
				.OrderByDescending(x => x.Count()).Take(Strategy.MaximumPKs())
				.ToArray();
		}

		public static bool IsPersonActive(GlbPerson bizo)
		{
			var isParentActive = bizo.PER_IsActive;
			var isAnyChildActive = bizo.ContactCollection.Cast<IContactable>()
				.Union(bizo.StaffCollection.Cast<IContactable>())
				.Any(child => child.IsActive);
			return (isAnyChildActive || (isParentActive && !isAnyChildActive));
		}

		protected override IEnumerable<GlbPerson> LoadTargetBizOs(IFactory factory, HashSet<Guid> candidatePKs)
		{
			var targetBizos = LoadPersonTargetBizOs(factory, candidatePKs, MasterGlow, bizo, exclusionManager);

			return GetOrderedList(targetBizos, candidatePKs);
		}

		public static GlbPerson[] LoadPersonTargetBizOs(IFactory factory, HashSet<Guid> candidatePKs, IGlbPerson masterGlow, GlbPerson bizo, DeduplicationExclusionManager<GlbPerson> exclusionManager)
		{
			if (masterGlow == null)
			{
				var message = (NoResString)"You have setup duplication finder with No MasterGlow record. Please check and confirm your setup conditions";
				ExceptionReporter.Instance.ReportDeveloperException("efa7bf6c-ca3f-41b1-b378-1b19ca7a86b0", message, new ArgumentNullException(message));
				return Array.Empty<GlbPerson>();
			}

			var regSettingExcludeInactive = SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.Value;
			return factory
				.Load<GlbPerson>(GetExclusionQuery(typeof(GlbPerson), candidatePKs, bizo, exclusionManager))
				.Where(person => person.PK != masterGlow.PER_PK &&
				(!regSettingExcludeInactive || IsPersonActive(person)))
				.ToArray();
		}

		protected override IGlbPerson ConvertMasterToGlowModel(GlbPerson targetBizo)
		{
			return ConvertPersonToGlowModel(targetBizo);
		}

		public static IGlbPerson ConvertPersonToGlowModel(GlbPerson targetBizo)
		{
			return targetBizo.CreateIGlbPerson();
		}

		protected override IGlbPerson ConvertTargetToGlowModel(GlbPerson targetBizo)
		{
			return ConvertMasterToGlowModel(targetBizo);
		}

		protected override ScoringResult ScoreGlowModel(IGlbPerson master, IGlbPerson target)
		{
			return TargetScorerController.Score(master, target, false);
		}

		protected override void StandardizeMaster()
		{
			if (MasterGlow == null)
			{
				throw new ArgumentException("Placeholder Removal cannot be called if MasterGlow is null");
			}

			MasterGlow.PER_FullName = TextStandardizerHelper.StandardizePersonName(MasterGlow.PER_FullName);

			MasterGlow.PER_EmailAddress = TextStandardizerHelper.StandardizeEmail(MasterGlow.PER_EmailAddress);
			MasterGlow.PER_EmailAddress2 = TextStandardizerHelper.StandardizeEmail(MasterGlow.PER_EmailAddress2);
			MasterGlow.PER_MobilePhone = TextStandardizerHelper.StandardizePhone(MasterGlow.PER_MobilePhone);
			MasterGlow.PER_MobilePhone2 = TextStandardizerHelper.StandardizePhone(MasterGlow.PER_MobilePhone2);
			MasterGlow.PER_FaxNumber = TextStandardizerHelper.StandardizePhone(MasterGlow.PER_FaxNumber);
			MasterGlow.PER_HomePhone = TextStandardizerHelper.StandardizePhone(MasterGlow.PER_HomePhone);

			if (TextStandardizerHelper.IsPlaceholderAddress(MasterGlow.PER_HomeAddress1) && (!string.IsNullOrWhiteSpace(MasterGlow.PER_HomeAddress1) || TextStandardizerHelper.IsPlaceholderAddress(MasterGlow.PER_HomeAddress2)))
			{
				MasterGlow.PER_HomeAddress1 = string.Empty;
				MasterGlow.PER_HomeAddress2 = string.Empty;
				MasterGlow.PER_City = string.Empty;
				MasterGlow.PER_Postcode = string.Empty;
				MasterGlow.PER_State = string.Empty;
			}

			foreach (var hrJobApplicant in MasterGlow.HRJobApplicants)
			{
				hrJobApplicant.HA_EmailAddress = TextStandardizerHelper.StandardizeEmail(hrJobApplicant.HA_EmailAddress);
				hrJobApplicant.HA_WorkPhone = TextStandardizerHelper.StandardizePhone(hrJobApplicant.HA_WorkPhone);
			}

			foreach (var glbStaff in MasterGlow.GlbStaffs)
			{
				glbStaff.GS_FullName = TextStandardizerHelper.StandardizePersonName(glbStaff.GS_FullName);
				glbStaff.GS_EmailAddress = TextStandardizerHelper.StandardizeEmail(glbStaff.GS_EmailAddress);
				glbStaff.GS_HomePhone = TextStandardizerHelper.StandardizePhone(glbStaff.GS_HomePhone);
				glbStaff.GS_MobilePhone = TextStandardizerHelper.StandardizePhone(glbStaff.GS_MobilePhone);

				if (TextStandardizerHelper.IsPlaceholderAddress(glbStaff.GS_UserAddress1) && (!string.IsNullOrWhiteSpace(glbStaff.GS_UserAddress1) || TextStandardizerHelper.IsPlaceholderAddress(glbStaff.GS_UserAddress2)))
				{
					glbStaff.GS_UserAddress1 = string.Empty;
					glbStaff.GS_UserAddress2 = string.Empty;
					glbStaff.GS_City = string.Empty;
					glbStaff.GS_Postcode = string.Empty;
					glbStaff.GS_State = string.Empty;
				}
			}

			foreach (var orgContact in MasterGlow.OrgContacts)
			{
				orgContact.OC_ContactName = TextStandardizerHelper.StandardizePersonName(orgContact.OC_ContactName);
				orgContact.OC_Email = TextStandardizerHelper.StandardizeEmail(orgContact.OC_Email);
				orgContact.OC_Phone = TextStandardizerHelper.StandardizePhone(orgContact.OC_Phone);
				orgContact.OC_Mobile = TextStandardizerHelper.StandardizePhone(orgContact.OC_Mobile);
				orgContact.OC_OtherPhone = TextStandardizerHelper.StandardizePhone(orgContact.OC_OtherPhone);
				orgContact.OC_HomePhone = TextStandardizerHelper.StandardizePhone(orgContact.OC_HomePhone);
				orgContact.OC_Fax = TextStandardizerHelper.StandardizePhone(orgContact.OC_Fax);
			}

			foreach (var code in MasterGlow.CertificateOrAccreditations)
			{
				code.XZ_RefNumber = TextStandardizerHelper.StandardizeRegCode(code.XZ_RefNumber);
			}
		}

		protected override (string Part1, string Part2) GenerateCacheSubkey(IGlbPerson targetGlow)
		{
			return (targetGlow.PER_FullName, string.Empty);
		}

		IEnumerable<GlbPerson> potentialResults;
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an async method.")]
		public async override Task<IEnumerable<GlbPerson>> GetPotentialTargetsByEmailDomainAsync(string emailAddress)
		{
			potentialResults = Enumerable.Empty<GlbPerson>();
			var instance = Task.Factory.StartNew(() =>
			{
				var result = Enumerable.Empty<GlbPerson>();
				var timeoutExecution = new DeduplicationTimeout<IEnumerable<GlbPerson>>(TimeSpan.FromSeconds(DuplicateDetectionTimeout));
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						result = potentialResults = timeoutExecution.DoWork(() => MasterDataMatchFinder.GetGlbPeronByEmailDomain(emailAddress));
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

			return await instance.WithExceptionHandler(defaultResult: Enumerable.Empty<GlbPerson>(), exception =>
			{
				HandleException(exception, nameof(GetPotentialTargetsByEmailDomainAsync));
			});
		}

		protected override Guid GetPatternMatchingResultModelParentPK(PatternMatchingResultModel patternMatchingResultModel)
		{
			return patternMatchingResultModel.PersonPK;
		}

		protected override int MaxScoringResult
		{
			get;
		}
		const int maxResults = 100;

		//int duplicateDetectionTimeout;
		protected override int RegistryTimeout => SystemDataRegistry.Instance.PersonsDuplicateDetectionTimeout.Value;

		protected override bool EndableDeduplicationFinderCheckpoint => SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
	}
}
