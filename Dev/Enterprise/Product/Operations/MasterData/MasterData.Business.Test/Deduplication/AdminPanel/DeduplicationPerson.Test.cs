using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
//using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(DeduplicationPerson))]
	public class DeduplicationPersonTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDocumentMacroIgnore_Password()
		{
			var passwordHashInfo = typeof(DeduplicationPerson).GetProperty("DPE_PasswordHash");
			var passwordSaltInfo = typeof(DeduplicationPerson).GetProperty("DPE_PasswordSalt");

			Assert("DPE_PasswordHash should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(passwordHashInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
			Assert("DPE_PasswordSalt should add DocumentMacroIgnoreAttribute for ignoring the document macro translate", Attribute.IsDefined(passwordSaltInfo, typeof(DocumentEngineIntegration.DocumentParsing.DocumentMacroIgnoreAttribute), false));
		}

		public void TestNoTimeOut()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var personMaster = Factory.NewWithValidTestData<GlbPerson>();
				var personTarget = Factory.NewWithValidTestData<GlbPerson>();
				((IDeduplicatable)personMaster).ShouldRunDeduplication = true;

				Factory.Save();

				var dedupPerson = Factory.Load<DeduplicationPersonForTest>(personMaster.PK);
				var personDuplicationFinder_ThatAlwaysTimesOut = new GlbPersonDuplicationFinder_ThatTimesOutForTest(personMaster, true);
				Factory.Save();

				dedupPerson.DuplicationFinderForTest = personDuplicationFinder_ThatAlwaysTimesOut;

				dedupPerson.DuplicateSearchingFinished += (_, e) =>
				{
					AssertEquals(DuplicationStatus.OK, ((ISupportDuplicationFinder)personDuplicationFinder_ThatAlwaysTimesOut).LastRunStatus);
				};
				dedupPerson.FindPotentialDuplicates();
			}
		}

		public void TestMultipleCallToSavePatternMatchingResults()
		{
			Factory.NewWithPrimaryKey<GlbPerson>(DedupPerson.PK.ToGuid());
			AssertNoExceptionThrown(() => DedupPerson.SaveDeduplicationResults(Enumerable.Empty<ScoringResult>()));
			AssertNoExceptionThrown(() => DedupPerson.SaveDeduplicationResults(Enumerable.Empty<ScoringResult>()));
		}

		public void TestSavingPatternMatchingResultsWithEmptyTargetPK()
		{
			Factory.NewWithPrimaryKey<GlbPerson>(DedupPerson.PK.ToGuid());
			var scoringResult = new ScoringResult();
			scoringResult.TargetPK = Guid.Empty;
			scoringResult.Score = .123;
			var scoringResults = new List<ScoringResult>() { scoringResult };
			AssertNoExceptionThrown(() => DedupPerson.SaveDeduplicationResults(scoringResults));

			AssertSaveException();
		}

		public void TestSavePatternMatchingResultWithEXCStatus()
		{
			Factory.NewWithPrimaryKey<GlbPerson>(DedupPerson.PK.ToGuid()).PER_FullName = "AAA";
			var pmt = Factory.New<PatternMatchingResult>();
			pmt.PMT_MasterPK = DedupPerson.PK;
			pmt.PMT_MasterTableCode = "PER";
			pmt.PMT_Status = PatternMatchingResult.StatusCodes.Excluded;
			pmt.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			pmt.PMT_ScorePercent = 0;
			Factory.Save();
			AssertNoExceptionThrown(() => DedupPerson.SaveDeduplicationResults(Enumerable.Empty<ScoringResult>()));

			AssertEquals(DeduplicationHelper.StatusConstants.Excluded, DedupPerson.DPE_Status);

			AssertSaveException();
		}

		public void TestSavePatternMatchingResultWithERRStatus()
		{
			Factory.NewWithPrimaryKey<GlbPerson>(DedupPerson.PK.ToGuid()).PER_FullName = "AAA";
			var pmt = Factory.New<PatternMatchingResult>();
			pmt.PMT_MasterPK = DedupPerson.PK;
			pmt.PMT_MasterTableCode = "PER";
			pmt.PMT_Status = PatternMatchingResult.StatusCodes.Error;
			pmt.PMT_FoundTimeUtc = ZDateTime.UtcNow;
			pmt.PMT_ScorePercent = 0;
			Factory.Save();
			AssertNoExceptionThrown(() => DedupPerson.SaveDeduplicationResults(Enumerable.Empty<ScoringResult>()));

			AssertEquals(DeduplicationHelper.StatusConstants.Error, DedupPerson.DPE_Status);

			AssertSaveException();
		}

		void AssertSaveException()
		{
			AssertType<ZSaveException>(ErrorReporter.LastExceptionReported);
			AssertEquals("DeduplicationHelper|SaveDeduplicationResults|ZSaveException", ErrorReporter.LastKeyReported);
			AssertEquals("Failed to save PatternMatchingResults", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCollectionDoesNotContainNDUResults()
		{
			Factory.NewWithPrimaryKey<GlbPerson>(DedupPerson.PK.ToGuid());
			DeduplicationPersonForTest.DuplicationScoringResultForTest = new List<ScoringResult>();
			DedupPerson.DuplicateSearchingFinished += (_, e) =>
			{
				DedupPerson.SaveDeduplicationResults(e.ScoringResults);
				Assert(Factory.Exists(typeof(PatternMatchingResult), new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, DedupPerson.PK).AddToFilter(PatternMatchingResultSchema.PMT_Status, "NDU")));
				AssertEquals(0, DedupPerson.MatchingResultCollection.Count);
			};
			DedupPerson.FindPotentialDuplicates();
		}

		public void TestFindPotentialDuplicates()
		{
			var personMaster = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();
			var dedupPerson = Factory.Load<DeduplicationPersonForTest>(personMaster.PK);
			DeduplicationPersonForTest.DuplicationScoringResultForTest = new List<ScoringResult>() { new ScoringResult()
			{
				MasterPK = personMaster.PK.ToGuid(),
				MasterType = typeof(GlbPerson),
				Score = 1,
				TargetPK = personTarget.PK.ToGuid(),
				TargetType = typeof(GlbPerson),
			} };
			dedupPerson.DuplicateSearchingFinished += (_, e) =>
			{
				dedupPerson.SaveDeduplicationResults(e.ScoringResults);
				Assert("PDU result saved in DB", Factory.Exists(typeof(PatternMatchingResult), new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, dedupPerson.PK).AddToFilter(PatternMatchingResultSchema.PMT_Status, "PDU")));
				AssertEquals("Collection has 1 PatternMatchingResult", 1, dedupPerson.MatchingResultCollection.Count);
				var result = dedupPerson.MatchingResultCollection[0];
				AssertEquals(personMaster.PK, result.PMT_MasterPK);
				AssertEquals(personTarget.PK, result.PMT_TargetPK);
				AssertEquals((ZByte)100, result.PMT_ScorePercent);
				AssertEquals("PDU", result.PMT_Status);
				AssertEquals("PER", result.PMT_TargetTableCode);
				AssertEquals("PER", result.PMT_MasterTableCode);
			};
			dedupPerson.FindPotentialDuplicates();
		}

		public void TestMatchConfidenceResults()
		{
			var personMaster = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget1 = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget2 = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget3 = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget4 = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget5 = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget6 = Factory.NewWithValidTestData<GlbPerson>();

			Factory.Save();

			var result1 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget1, 70);
			var result2 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget2, 85);
			var result3 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget3, 80);
			var result4 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget4, 99);
			var result5 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget5, 50);
			var result6 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget6, 40);

			Factory.Save();

			var dedupePerson = Factory.Load<DeduplicationPersonForTest>(personMaster.PK);
			CombineAssertions(() =>
			{
				AssertEquals("High results (>=80%)", 3, dedupePerson.DPE_HighDuplicates);
				AssertEquals("Medium results (>=50%)", 2, dedupePerson.DPE_MediumDuplicates);
				AssertEquals("Low results (<50%)", 1, dedupePerson.DPE_LowDuplicates);
			});
		}

		public void TestMatchMaxResult()
		{
			var personMaster = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget1 = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget2 = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget3 = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget4 = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget5 = Factory.NewWithValidTestData<GlbPerson>();
			var personTarget6 = Factory.NewWithValidTestData<GlbPerson>();

			Factory.Save();

			var result1 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget1, 70);
			var result2 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget2, 85);
			var result3 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget3, 80);
			var result4 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget4, 99);
			var result5 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget5, 50);
			var result6 = CreatePersonPatternMatchingResult(Factory, personMaster, personTarget6, 40);

			Factory.Save();

			var dedupePerson = Factory.Load<DeduplicationPersonForTest>(personMaster.PK);
			AssertEquals(99, dedupePerson.DPE_MaxResult);
		}

		static PatternMatchingResult CreatePersonPatternMatchingResult(BusinessObjectFactory factory, GlbPerson master, GlbPerson target, ZByte scorePercent)
		{
			var patternMatchingResult = factory.New<PatternMatchingResult>();
			patternMatchingResult.PMT_MasterPK = master.PK;
			patternMatchingResult.PMT_ScorePercent = scorePercent;
			patternMatchingResult.PMT_TargetPK = target.PK;
			patternMatchingResult.PMT_FoundTimeUtc = DateTime.Now;
			patternMatchingResult.PMT_Status = PatternMatchingResult.StatusCodes.PotentialDuplicate;
			patternMatchingResult.PMT_TargetTableCode = GlbPersonSchema.Constants.Prefix;
			patternMatchingResult.PMT_MasterTableCode = GlbPersonSchema.Constants.Prefix;
			return patternMatchingResult;
		}

		public void TestMatchingResultCollection()
		{
			var result1 = Factory.New<PatternMatchingResult>();
			var result2 = Factory.New<PatternMatchingResult>();
			result1.PMT_Status = result2.PMT_Status = "PDU";
			result1.PMT_MasterPK = result2.PMT_MasterPK = DedupPerson.PK;
			AssertCollectionContains("Contains result1", result1, DedupPerson.MatchingResultCollection);
			AssertCollectionContains("Contains result2", result2, DedupPerson.MatchingResultCollection);
		}

		DeduplicationPersonForTest DedupPerson;

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This is a view, doesn't need to save", true);
		}

		public void TestAssignDeDuplicationPersonData()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			var person = GlbPerson.CreateFromContact(Factory, contact);

			contact.ParentOrg.OH_FullName = "MadeUpCorp";
			contact.ParentOrg.MainAddress.OA_RL_NKRelatedPortCode = "EDCBA";
			contact.OC_Title = "ExampleTitle";

			Factory.Save();

			AssertEquals("Precondition: Organisation should link to contact", org.PK, contact.ParentOrg.PK);
			AssertEquals("Precondition: Contact should link to person", contact.OC_PER, person.PK);

			var deDupPerson = Factory.Load<DeduplicationPerson>(person.PK);
			Factory.Save();

			CombineAssertions("Should correctly assign data to deduplication person", () =>
			{
				AssertEquals("MadeUpCorp", deDupPerson.Workplace);
				AssertEquals("MADEUP", deDupPerson.WorkplaceCode);
				AssertEquals("EDCBA", deDupPerson.Location);
				AssertEquals("ExampleTitle", deDupPerson.JobTitle);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			DedupPerson = Factory.New<DeduplicationPersonForTest>();
			DedupPerson.DPE_Status = DeduplicationHelper.StatusConstants.ToBeProcessed;
			((INeedRow)DedupPerson).Row.AcceptChanges();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return DedupPerson;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return DedupPerson;
		}

		protected override void TearDown()
		{
			DeduplicationPersonForTest.DuplicationScoringResultForTest = null;
			base.TearDown();
		}
	}

	public class DeduplicationPersonWithoutFactoryTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestFindPotentialDuplicates_FindsLowConfidenceMatches_RegardlessOfRegistryThreshold()
		{
			var factory = new BusinessObjectFactory();

			var masterPerson = factory.NewWithValidTestData<GlbPerson>();
			masterPerson.PER_FullName = "Harry Potter";
			masterPerson.PER_HomeAddress1 = "4 Privet Drive, Little Whinging";
			masterPerson.PER_City = "SURREY";
			masterPerson.PER_EmailAddress = "Harry.Potter@Hogwarts.com";
			masterPerson.PER_HomePhone = "+61 444 753 159";

			var hashedMasterName = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizePersonName(masterPerson.PER_FullName));

			var mediumMatchPerson = factory.NewWithValidTestData<GlbPerson>();
			mediumMatchPerson.PER_FullName = "Barry Trotter";
			mediumMatchPerson.PER_HomeAddress1 = "4 Privet Drive, Little Whinging";
			mediumMatchPerson.PER_City = "SURREY";
			mediumMatchPerson.PER_EmailAddress = "Barry.Trotter@Hogwarts.com";
			mediumMatchPerson.PER_HomePhone = "+61 444 753 159";
			CreatePatternMatchingName(factory, mediumMatchPerson, hashedMasterName);

			var lowMatchPerson = factory.NewWithValidTestData<GlbPerson>();
			lowMatchPerson.PER_FullName = "Larry Snotter";
			lowMatchPerson.PER_HomeAddress1 = "4 Privet Drive, Little Whinging";
			lowMatchPerson.PER_City = "SURREY";
			lowMatchPerson.PER_EmailAddress = "Larry.Snotter@Hogwarts.com";
			lowMatchPerson.PER_HomePhone = "+61 444 456 321";
			CreatePatternMatchingName(factory, lowMatchPerson, hashedMasterName);

			var noneMatchPerson = factory.NewWithValidTestData<GlbPerson>();
			noneMatchPerson.PER_FullName = "Ron Weasley";
			noneMatchPerson.PER_HomeAddress1 = "The Burrow - Ottery St Catchpole";
			noneMatchPerson.PER_City = "DEVON";
			noneMatchPerson.PER_EmailAddress = "Ron.Weasley@Hogwarts.com";
			noneMatchPerson.PER_HomePhone = "+61 444 789 654";
			CreatePatternMatchingName(factory, noneMatchPerson, hashedMasterName);

			factory.Save();

			var registryEnableDuplicationFinder = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			var registryMinimumConfidence = SystemDataRegistry.Instance.PersonsDeduplicationMinimumConfidenceResult.Value;

			try
			{
				SystemDataRegistry.Instance.PersonsDeduplicationMinimumConfidenceResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium);
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var dedupePerson = factory.Load<DeduplicationPersonForTest>(masterPerson.PK);
				var duplicates = dedupePerson.FindPotentialDuplicatesCore_ForTest();

				AssertContainsExactElementsInAnyOrder("Should return results for medium and low matches, even though registry threshold is set to > medium",
					new[] { mediumMatchPerson.PK, lowMatchPerson.PK },
					duplicates.Select(match => match.TargetPK));

				AssertEquals(ConfidenceRating.Low, duplicates.Single(match => match.TargetPK == lowMatchPerson.PK).ConfidenceRating);
				AssertEquals(ConfidenceRating.Medium, duplicates.Single(match => match.TargetPK == mediumMatchPerson.PK).ConfidenceRating);
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsDeduplicationMinimumConfidenceResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryMinimumConfidence);
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEnableDuplicationFinder);
			}
		}

		[UseSnapshotProtection]
		public void TestFindPotentialDuplicates_IsNotAffectedByMaximumPotentialTargets_SetInRegistry()
		{
			var registryTimeout = SystemDataRegistry.Instance.PersonsDuplicateDetectionTimeout.Value;
			var registryEnableDuplicationFinder = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			var registryMaxPotentialTargets = SystemDataRegistry.Instance.PersonsMaximumPotentialTargets.Value;

			try
			{
				SystemDataRegistry.Instance.PersonsMaximumPotentialTargets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
				SystemDataRegistry.Instance.PersonsDuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 180);
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var factory = new BusinessObjectFactory();
				var org = factory.NewWithValidTestData<OrgHeader>();
				var person = factory.NewWithValidTestData<GlbPerson>();
				var personTargets = new GlbPerson[52];

				for (int i = 0; i < personTargets.Length; i++)
				{
					personTargets[i] = factory.NewWithValidTestData<GlbPerson>();

					personTargets[i].PER_FullName = "TOMMY FAN";
					personTargets[i].PER_HomeAddress1 = "72 O'Riordan Street";
					personTargets[i].PER_HomePhone = "11111111";
					personTargets[i].PER_EmailAddress = "TOMMY.FAN@AAA.COM";

					var patternMatchingName = factory.NewWithValidTestData<PatternMatchingName>();
					patternMatchingName.PMN_PER = personTargets[i].PK;
					patternMatchingName.PMN_ParentId = personTargets[i].PK;
					patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizePersonName(personTargets[i].PER_FullName));
					patternMatchingName.PMN_ParentTableCode = "PER";
					patternMatchingName.PMN_IsActive = true;
				}

				person.PER_FullName = "TOMMY FAN";
				person.PER_HomeAddress1 = "72 O'Riordan Street";
				person.PER_HomePhone = "11111111";
				person.PER_EmailAddress = "TOMMY.FAN@AAA.COM";
				person.PER_ValidationStatus = "MAN";

				factory.Save();

				var deDupPerson = factory.Load<DeduplicationPersonForTest>(person.PK);
				var result = deDupPerson.FindPotentialDuplicatesCore_ForTest();

				AssertEquals(52, result.Count());
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsMaximumPotentialTargets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryMaxPotentialTargets);
				SystemDataRegistry.Instance.PersonsDuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryTimeout);
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEnableDuplicationFinder);
			}
		}

		#region Implementation 

		PatternMatchingName CreatePatternMatchingName(BusinessObjectFactory factory, GlbPerson person, int hashValue)
		{
			var patternMatchingName = factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_PER = person.PK;
			patternMatchingName.PMN_ParentId = person.PK;
			patternMatchingName.PMN_HashedValue = hashValue;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = person.TablePrefix;
			patternMatchingName.PMN_IsActive = true;
			return patternMatchingName;
		}

		#endregion
	}

	public class DeduplicationPersonForTest : DeduplicationPerson
	{
		public DeduplicationPersonForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

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

		protected override ISupportDuplicationFinder GetDuplicationFinder(GlbPerson person) => DuplicationFinderForTest ?? base.GetDuplicationFinder(person);
	}

	public class GlbPersonDuplicationFinder_ThatTimesOutForTest : ISupportDuplicationFinder
	{
		public GlbPersonDuplicationFinder_ThatTimesOutForTest(GlbPerson header, bool useMaxRecords)
		{
		}

		public Type TargetType => throw new NotImplementedException();

		public bool ShouldStopProcessing { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public List<ScoringResult> ScoringResults => throw new NotImplementedException();

		protected int RegistryTimeout => 0;

		public DuplicationStatus LastRunStatus { get; set; }

		public Task FindDuplicates()
		{
			throw new NotImplementedException();
		}

		public void RequestToCancel()
		{
			throw new NotImplementedException();
		}
	}
}
