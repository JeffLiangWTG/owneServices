using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(GlbPersonDuplicationFinder))]
	public class GlbPersonDuplicationFinderTest : DuplicationFinderBaseTest<GlbPersonDuplicationFinder, GlbPerson, GlbPerson, IGlbPerson>
	{
		public void TestSettingRegistryTimeoutShouldSetDeuplcateDetectionTimeout()
		{
			//Arrange
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var per = list[0];
			var testTimeout1 = 123;
			var testTimeout2 = 300;
			var duplicationFinder = new GlbPersonDuplicationFinderForTest(per, true);

			//Act
			SystemDataRegistry.Instance.PersonsDuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testTimeout1);

			//Assert
			AssertEquals(testTimeout1, duplicationFinder.DuplicateDetectionTimeoutExposed);

			//Act
			SystemDataRegistry.Instance.PersonsDuplicateDetectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testTimeout2);

			//Assert
			AssertEquals(testTimeout2, duplicationFinder.DuplicateDetectionTimeoutExposed);
		}

		public override void TestFindPotentialDuplicates()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var per = list[0];
			var per2 = list[1];
			var per3 = list[2];
			per3.PER_IsActive = false;

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_PER = per.PK;
			patternMatchingName.PMN_ParentId = per.PK;
			patternMatchingName.PMN_HashedValue = -383906410;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";
			patternMatchingName.PMN_ParentTableCode = per.TablePrefix;
			patternMatchingName.PMN_IsActive = true;

			Factory.Save();

			var targetResult = new List<PatternMatchingResultModel>
			{
				GetNewPatternMatchingResultModel(per, per2, -383906410),
				GetNewPatternMatchingResultModel(per, per3, -383906410, "US")
			};

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)per).ShouldRunDeduplication = true;

				var duplicationFinder = new GlbPersonDuplicationFinderForTest(per, false);
				duplicationFinder.StandardizeMasterForTest();
				duplicationFinder.GenerateTargetGlows(new[] { per2, per3 });
				var duplicates = duplicationFinder.ScoreResultsForTest(targetResult).ToList();

				AssertEquals(2, duplicates.Count);
				AssertContainsExactElementsInAnyOrder(new[] { per2.PK, per3.PK }, duplicates.Select(scoringResult => scoringResult.TargetPK));
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public void TestFindTargetPKs()
		{
			var masterPer = Factory.NewWithValidTestData<GlbPerson>();
			var masterContact = masterPer.ContactCollection.AddNew();
			var masterOrg = Factory.NewWithValidTestData<OrgHeader>();

			masterContact.OC_OH = masterOrg.PK;
			masterPer.PER_IsActive = false;

			var targetPer = Factory.NewWithValidTestData<GlbPerson>();
			var targetStaff = targetPer.StaffCollection.AddNew();
			var targetApplicant = targetPer.ApplicantCollection.AddNew();
			targetPer.PER_IsActive = false;

			Factory.Save();

			var targetResults = new List<PatternMatchingResultModel>
			{
				createResultModel(targetStaff, targetPer, -383906410),
				createResultModel(targetApplicant, targetPer, -383906410),
				createResultModel(masterContact, masterPer, -383906410)
			};

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)masterPer).ShouldRunDeduplication = true;

				var duplicationFinder = new GlbPersonDuplicationFinderForTest(masterPer, false);
				duplicationFinder.StandardizeMasterForTest();
				var resultsPKsAndResultModels = duplicationFinder.FindTargetPKsAndResultModelsTest(targetResults.ToArray()).ToList();
				AssertEquals("Candidate Pattern Matching Tables should be grouped by and returned according to PersonPK", targetPer.PK.ToGuid(), resultsPKsAndResultModels.Single().Key);
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public void TestFindTargetPKs_MaximumValue_IsInRegistrySettings()
		{
			var masterPerson = Factory.NewWithValidTestData<GlbPerson>();
			var personPks = new List<Guid>();

			personPks.Add(masterPerson.PK.ToGuid());

			for (int i = 0; i < 60; i++)
			{
				personPks.Add(Guid.NewGuid());
			}

			var patternMatchingResultsModel = new List<PatternMatchingResultModel>();

			for (int i = 0; i < personPks.Count; i++)
			{
				patternMatchingResultsModel.Add(new PatternMatchingResultModel { PersonPK = personPks[i] });
			}

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsMaximumPotentialTargets.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			{
				((IDeduplicatable)masterPerson).ShouldRunDeduplication = true;

				var duplicationFinder = new GlbPersonDuplicationFinderForTest(masterPerson, false);
				var result = duplicationFinder.FindTargetPKsAndResultModelsTest(patternMatchingResultsModel.ToArray());

				AssertEquals("Maximum TargetPKs to load is got from registry", 50, result.Count());
			}
		}

		PatternMatchingResultModel createResultModel(BusinessObject parentObject, GlbPerson targetPerson, int hash, string countryCode = "AU")
		{
			return new PatternMatchingResultModel
			{
				CountryCode = countryCode,
				HashedValue = hash,
				ParentID = parentObject.PK.ToGuid(),
				ParentTablePrefix = parentObject.TablePrefix,
				PersonPK = targetPerson.PK.ToGuid()
			};
		}

		public override void TestGetOrderedList()
		{
			var duplicationFinder = new GlbPersonDuplicationFinderForTest(null, false);
			AssertNoExceptionThrown(() => duplicationFinder.GetOrderedListForTest(null, null));
			AssertNoExceptionThrown(() => duplicationFinder.GetOrderedListForTest(new GlbPerson[1], null));

			var org1 = Factory.New<GlbPerson>();
			var org2 = Factory.New<GlbPerson>();
			var org3 = Factory.New<GlbPerson>();
			var org4 = Factory.New<GlbPerson>();
			var org5 = Factory.New<GlbPerson>();
			var orderedList = duplicationFinder.GetOrderedListForTest(new[] { org5, org1, org2, org4, org3 }, new HashSet<Guid>(new[] { Guid.NewGuid(), org1.PK.ToGuid(), org2.PK.ToGuid(), org3.PK.ToGuid(), Guid.NewGuid(), Guid.NewGuid(), org4.PK.ToGuid(), org5.PK.ToGuid() })).ToArray();

			AssertEquals(5, orderedList.Length);
			AssertEquals(org1.PK, orderedList[0].PK);
			AssertEquals(org2.PK, orderedList[1].PK);
			AssertEquals(org3.PK, orderedList[2].PK);
			AssertEquals(org4.PK, orderedList[3].PK);
			AssertEquals(org5.PK, orderedList[4].PK);
		}

		public override void TestFindPotentialDuplicatesWithMultiLanguage()
		{
			Assert("Tested in OrgHeaderDuplicationFinderTest", true);
		}

		public void TestScoringResultWithNoTargetWithChilden()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var per = list[3];
			var per2 = list[4];

			Factory.Save();

			var targetResult = new List<PatternMatchingResultModel>
			{
				GetNewPatternMatchingResultModel(per2, per2, -383906410)
			};

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;

			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)per).ShouldRunDeduplication = true;

				var duplicationFinder = new GlbPersonDuplicationFinderForTest(per, false);
				duplicationFinder.StandardizeMasterForTest();
				duplicationFinder.GenerateTargetGlows(new[] { per2 });
				var scoringResults = duplicationFinder.ScoreResultsForTest(targetResult);

				AssertEquals(scoringResults.First().Score, 1.0);
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public override void TestScoringResultWithNoTarget_ReturnsEmptyList()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var per = list[0];
			var per2 = list[1];

			Factory.Save();

			var targetResult = new List<PatternMatchingResultModel>
			{
				GetNewPatternMatchingResultModel(per2, per2, -383906410)
			};

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;

			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)per).ShouldRunDeduplication = true;

				var duplicationFinder = new GlbPersonDuplicationFinderForTest(per, false);
				duplicationFinder.StandardizeMasterForTest();
				var duplicates = duplicationFinder.ScoreResultsForTest(targetResult);

				AssertEquals(0, actual: duplicates.Count());
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		public override void TestLoadTargetBizosForTIG()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var person = list[0];
			var person1 = list[1];
			var person2 = list[2];
			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				person1.PK.ToGuid(),
				person2.PK.ToGuid(),
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinderForTest(person, false);
				duplicationFinder.GenerateTargetGlows(new[] { person1, person2 });

				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals(2, duplicates.Length);
				AssertEquals(person1.PK, duplicates[0].PK);
				AssertEquals(person2.PK, duplicates[1].PK);

				var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
				pmt.PMT_MasterPK = person1.PK;
				pmt.PMT_TargetPK = person.PK;
				pmt.PMT_Status = "TIG";
				pmt.PMT_GS_NKExcludeBy = GlbStaff.CurrentUser.GS_Code;
				pmt.PMT_TargetTableCode = pmt.PMT_MasterTableCode = person.TablePrefix;
				Factory.Save();

				duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals(1, duplicates.Length);
				AssertEquals(person2.PK, duplicates[0].PK);
			}
		}

		public override void TestLoadTargetBizosForPIG()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var person = list[0];
			var person1 = list[1];
			var person2 = list[2];

			var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
			pmt.PMT_MasterPK = person.PK;
			pmt.PMT_TargetPK = person1.PK;
			pmt.PMT_Status = "PIG";
			pmt.PMT_TargetTableCode = pmt.PMT_MasterTableCode = person.TablePrefix;
			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				person1.PK.ToGuid(),
				person2.PK.ToGuid()
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinderForTest(person, false);
				duplicationFinder.GenerateTargetGlows(new[] { person1, person2 });

				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals(1, duplicates.Length);
				AssertEquals(person2.PK, duplicates[0].PK);
			}
		}

		public void TestLoadTargetBizos_WhenRegSettingExcludeInactivePotentialDuplicatesIsTrue_ShouldHideInactiveStaff()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_IsActive = false;
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			orgContact.OC_PER = person.PK;
			orgContact.OC_IsActive = true;
			glbStaff.GS_PER = person1.PK;
			glbStaff.GS_IsActive = false;
			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				person1.PK.ToGuid()
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinderForTest(person, false);
				duplicationFinder.GenerateTargetGlows(new[] { person1 });

				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals(0, duplicates.Length);
			}
		}

		public void TestLoadTargetBizos_WhenRegSettingExcludeInactivePotentialDuplicatesIsFalse_ShouldShowInactiveStaff()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			orgContact.OC_PER = person.PK;
			orgContact.OC_IsActive = true;
			glbStaff.GS_PER = person1.PK;
			glbStaff.GS_IsActive = false;
			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				person1.PK.ToGuid()
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinderForTest(person, false);
				duplicationFinder.GenerateTargetGlows(new[] { person1 });

				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals(1, duplicates.Length);
				AssertEquals(person1.PK, duplicates[0].PK);
			}
		}

		public void TestLoadTargetBizos_WhenRegSettingExcludeInactivePotentialDuplicatesIsFalseAndNoChildrenExist_ShouldShowInactivePerson()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_IsActive = false;

			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				person1.PK.ToGuid()
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinderForTest(person, false);
				duplicationFinder.GenerateTargetGlows(new[] { person1 });

				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals(1, duplicates.Length);
				AssertEquals(person1.PK, duplicates[0].PK);
			}
		}

		public void TestLoadTargetBizos_WhenRegSettingExcludeInactivePotentialDuplicatesIsTrueAndNoChildrenExist_ShouldHideInactivePerson()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_IsActive = true;
			person2.PER_IsActive = false;

			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				person1.PK.ToGuid(),
				person2.PK.ToGuid()
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinderForTest(person, false);
				duplicationFinder.GenerateTargetGlows(new[] { person1, person2 });

				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals(1, duplicates.Length);
			}
		}

		public void TestShouldFindDuplications()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var person = list[0];
			((IDeduplicatable)person).ShouldRunDeduplication = true;

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var duplicationFinder = new GlbPersonDuplicationFinderForTest(person, false);
				var shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("Registry DISABLED; non-web environment; valid master BizO: should NOT search for duplicates", !shouldRun);
			}

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var duplicationFinder = new GlbPersonDuplicationFinderForTest(person, false);
				var shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("Registry enabled; non-web environment; valid master BizO: should search for duplicates", shouldRun);

				duplicationFinder = new GlbPersonDuplicationFinderForTest(null, false);
				shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("Registry enabled; non-web environment; NULL master BizO: should NOT search for duplicates", !shouldRun);

				duplicationFinder = new GlbPersonDuplicationFinderForTest(person, false);
				Globals.IsWeb = true;
				shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("Registry enabled; WEB environment; valid master BizO: should search for duplicates", shouldRun);

				Globals.IsWeb = false;
				shouldRun = duplicationFinder.GetShouldFindDuplications();
				Assert("Registry enabled; non-web environment; valid master BizO: should search for duplicates", shouldRun);
			}
		}

		public void TestStandardizeMasterNamesScoreExact()
		{
			var personName = "Michael Jackson PHD";
			var personPhone = "0449743938";

			var glbPersonMaster = Factory.New<GlbPerson>();
			var glbPersonTarget = Factory.New<GlbPerson>();
			glbPersonMaster.PER_FullName = personName;
			glbPersonTarget.PER_FullName = personName;
			glbPersonMaster.PER_HomePhone = personPhone;
			glbPersonTarget.PER_HomePhone = personPhone;

			((IDeduplicatable)glbPersonMaster).ShouldRunDeduplication = true;

			var standardizePersonName = TextStandardizerHelper.StandardizePersonName(glbPersonMaster.PER_FullName);
			var patternMatchingName = Factory.New<PatternMatchingName>();
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(standardizePersonName);
			patternMatchingName.PMN_PER = glbPersonTarget.PK;
			patternMatchingName.PMN_ParentId = glbPersonTarget.PK;
			patternMatchingName.PMN_ParentTableCode = GlbPersonSchema.Constants.Prefix;

			Factory.Save();

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var duplicationFinder = new GlbPersonDuplicationFinderForTest(glbPersonMaster, false);
				var result = duplicationFinder.FindPotentialDuplicates(false).ToList();
				AssertEquals("Should be exact match", ConfidenceRating.Exact, result.Single().ConfidenceRating);
			}
		}

		public override void TestLoadTargetBizosForEXC()
		{
			var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
			var person = list[0];
			var person1 = list[1];
			var person2 = list[2];

			var pmt = Factory.NewWithValidTestData<PatternMatchingResult>();
			pmt.PMT_MasterPK = person1.PK;
			pmt.PMT_Status = "EXC";
			pmt.PMT_MasterTableCode = person.TablePrefix;
			Factory.Save();

			var targetList = new HashSet<Guid>
			{
				person1.PK.ToGuid(),
				person2.PK.ToGuid()
			};

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinderForTest(person, false);
				duplicationFinder.GenerateTargetGlows(new[] { person1, person2 });

				var duplicates = duplicationFinder.LoadTargetBizosTest(Factory, targetList).ToArray();

				AssertEquals(1, duplicates.Length);
				AssertEquals(person2.PK, duplicates[0].PK);
			}
		}

		public override void AssertCompareBizos<TMaster, TTarget, TFinder>()
		{
			Assert(true);
		}

		public override void AssertAddIgnore<TMaster, TTarget, TFinder>()
		{
			Assert(true);
		}

		public override void AssertAddExclusion<TMaster, TTarget, TFinder>()
		{
			Assert(true);
		}

		public override void AssertRemoveExclusion<TMaster, TTarget, TFinder>()
		{
			Assert(true);
		}

		public override void AssertRemoveIgnore<TMaster, TTarget, TFinder>()
		{
			Assert(true);
		}

		public void TestGetPatternMatchingResultModelParentPK()
		{
			var person = Factory.New<GlbPerson>();
			var duplicationFinder = new GlbPersonDuplicationFinderForTest(person, true);
			var parentPK = duplicationFinder.GetPatternMatchingResultModelParentPKForTest(new PatternMatchingResultModel()
			{
				PersonPK = person.PK.ToGuid(),
				OrgPK = Guid.Empty
			});

			AssertEquals(person.PK.ToGuid(), parentPK);
		}

		public void TestCreateInstanceDoNotSetTimeoutFromConfig()
		{
			var person = Factory.New<GlbPerson>();
			using (SystemDataRegistry.Instance.PersonsDuplicateDetectionTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 101))
			{
				var config = new DeduplicationProxyConfig();
				var personDuplicationFinder = GlbPersonDuplicationFinder.CreateInstance(person, new DeduplicationProxyConfig());
				AssertEquals(20, config.DuplicationFinderTimeout.Seconds);
				AssertEquals(101, (int)personDuplicationFinder.GetType().GetProperty("DuplicateDetectionTimeout", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(personDuplicationFinder));
			}
		}

		public override void AssertAddExclude_ShouldNotThrowConcurrencyError<TMaster, TTarget, TFinder>()
		{
			Assert(true);
		}

		public void TestFindPotentialDuplicates_WithInvalidOperationExceptions_NoExceptionThrown()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
				var person = list[0];
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinderForTestWithException(person, true);
				var message = new IOExceptionMessages();

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.ReadWhenNoDataIsPresent) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.ReadWhenReaderIsClosedt) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.TheConnectionIsClosed) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.NextResultWhenReaderIsClosed) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.FieldCoiuntWhenReaderIsClosed) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.InternalConnectionError) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.CheckDataIsReadyWhenReaderIsClosed) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));

				duplicationFinder.exceptionFromStandardizeMaster = new InvalidOperationException(message.OpenAndAvailableConnectionError) { Source = "System.Data" };
				AssertNoExceptionThrown(() => duplicationFinder.FindPotentialDuplicates(false));
			}

			Assert(ExceptionReporterTestListener.Instance.Count == 0);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestFindPotentialDuplicates_WithInvalidOperationExceptions_HandledByDeveloperException()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
				var person = list[0];
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinderForTestWithException(person, true)
				{
					exceptionFromStandardizeMaster = new InvalidOperationException("Some invalid operation exception has occured.") { Source = "System.Data" }
				};
				duplicationFinder.FindPotentialDuplicates(false);
			}

			Assert(ExceptionReporterTestListener.Instance.Count == 1);
			AssertEquals("Some invalid operation exception has occured.", ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestFindPotentialDuplicates_WithSqlExceptions_HandledByDeveloperException()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var list = GlbPersonDeduplicationTestData.NewValidTestData(Factory);
				var person = list[0];
				((IDeduplicatable)person).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinderForTestWithException(person, true)
				{
					exceptionFromStandardizeMaster = GetNewSqlException()
				};
				duplicationFinder.FindPotentialDuplicates(false);
			}

			Assert(ExceptionReporterTestListener.Instance.Count == 1);
			AssertType<SqlException>(ExceptionReporterTestListener.Instance[0]);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[ExpectNoExceptions]
		public void TestGetPotentialTargetsByEmailDomainAsync_GetRegistryTimeoutFailed()
		{
			var duplicationFinder = new GlbPersonDuplicationFinderForTestWithException(null, false)
			{
				throwSqlExceptionFromRegistryTimeout = true
			};

			var result = AsyncTaskSynchronizer.Run(() => duplicationFinder.GetPotentialTargetsByEmailDomainAsync(null));

			AssertNotNull(result);
			AssertEquals(0, result.Count());

			Assert(ExceptionReporterTestListener.Instance.Count == 1);
			AssertType<SqlException>(ExceptionReporterTestListener.Instance[0]);
			ExceptionReporterTestListener.Instance.Clear();
		}

		class GlbPersonDuplicationFinderForTestWithException : GlbPersonDuplicationFinder
		{
			internal Exception exceptionFromStandardizeMaster;
			internal bool throwSqlExceptionFromRegistryTimeout;

			public GlbPersonDuplicationFinderForTestWithException(GlbPerson bizo, bool shouldUseCache) : base(bizo, shouldUseCache)
			{
			}

			protected override bool ShouldFindDuplications => true;

			protected override void StandardizeMaster()
			{
				if (exceptionFromStandardizeMaster != null)
				{
					throw exceptionFromStandardizeMaster;
				}

				base.StandardizeMaster();
			}

			protected override int RegistryTimeout
			{
				get
				{
					if (throwSqlExceptionFromRegistryTimeout)
					{
						throw GetNewSqlException();
					}

					return base.RegistryTimeout;
				}
			}
		}
	}

	public class GlbPersonDuplicationFinderForTest : GlbPersonDuplicationFinder
	{
		public GlbPersonDuplicationFinderForTest(GlbPerson bizo, bool shouldUseCache) : base(bizo, shouldUseCache)
		{
		}

		public IEnumerable<GlbPerson> LoadTargetBizosTest(IFactory factory, HashSet<Guid> pkList)
		{
			return LoadTargetBizOs(factory, pkList);
		}

		public void GenerateTargetGlows(GlbPerson[] targetHeaderBizos)
		{
			var list = new List<IGlbPerson>();
			foreach (var target in targetHeaderBizos)
			{
				list.Add(ConvertMasterToGlowModel(target));
			}
			TargetGlows = list;
		}

		public bool GetShouldFindDuplications()
		{
			return ShouldFindDuplications;
		}

		public IEnumerable<IGrouping<Guid, PatternMatchingResultModel>> FindTargetPKsAndResultModelsTest(PatternMatchingResultModel[] patternMatchingResults)
		{
			return FindTargetPKsAndResultModels(patternMatchingResults);
		}

		public IEnumerable<GlbPerson> GetOrderedListForTest(GlbPerson[] targetBizos, HashSet<Guid> candidatePKs) => GetOrderedList(targetBizos, candidatePKs);

		public int DuplicateDetectionTimeoutExposed => DuplicateDetectionTimeout;

		public Guid GetPatternMatchingResultModelParentPKForTest(PatternMatchingResultModel patternMatchingResultModel) => GetPatternMatchingResultModelParentPK(patternMatchingResultModel);

		public void StandardizeMasterForTest()
		{
			StandardizeMaster();
		}

		public IEnumerable<ScoringResult> ScoreResultsForTest(IEnumerable<PatternMatchingResultModel> patternMatchingResults)
		{
			var resultScorer = new DuplicationFinderResultsScorer<IGlbPerson>(shouldUseCache, MasterGlow, MaxScoringResult, DebuggerParticipant, this);
			resultScorer.ScoringResults(TargetGlows, patternMatchingResults, GetGlowPK, GetPatternMatchingResultModelParentPK, GenerateCacheSubkey, ScoreGlowModel, TokenSource.Token);

			return ScoringResults;
		}
	}

	public class GlbPersonDuplicationFinderWithoutFactoryTest : DuplicationFinderWithoutFactoryBaseTest
	{
		[UseSnapshotProtection]
		public override void TestWithoutPlaceholders()
		{
			AssertFindResults(false);
		}

		[UseSnapshotProtection]
		public override void TestWithoutPlaceholders_WithoutStandardizationRules()
		{
			AssertFindResults(false, false);
		}

		[UseSnapshotProtection]
		public override void TestWithPlaceholders()
		{
			AssertFindResults(true);
		}

		[UseSnapshotProtection]
		public override void TestGetPotentialTargets()
		{
			var address1 = "PADDINGTON NSW";
			var email = "ABCD@TEST.COM";
			var factory = new BusinessObjectFactory();
			var list = GlbPersonDeduplicationTestData.NewValidTestData(factory);

			var masterPerson = list[0];
			masterPerson.PER_EmailAddress = email;
			masterPerson.Address1 = address1;

			var targetPerson = list[1];
			targetPerson.PER_EmailAddress = email;
			targetPerson.Address1 = address1;

			var targetName = factory.New<PatternMatchingName>();
			targetName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(masterPerson.PER_FullName);
			targetName.PMN_PER = targetPerson.PK;
			targetName.PMN_ParentTableCode = "PER";
			targetName.PMN_ParentId = targetPerson.PK;
			targetName.PMN_RN_NKCountryCode = "AU";

			var targetAddress = factory.New<PatternMatchingAddress>();
			targetAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(address1);
			targetAddress.PMA_PER = targetPerson.PK;
			targetAddress.PMA_ParentTableCode = "PER";
			targetAddress.PMA_ParentId = targetPerson.PK;
			targetAddress.PMA_RN_NKCountryCode = "AU";

			var targetEmail = factory.New<PatternMatchingEmail>();
			targetEmail.PME_HashedValue = TextStandardizerHelper.ComputeStringHashFast(email);
			targetEmail.PME_PER = targetPerson.PK;
			targetEmail.PME_ParentTableCode = "PER";
			targetEmail.PME_ParentId = targetPerson.PK;
			targetEmail.PME_RN_NKCountryCode = "AU";

			factory.Save();

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)masterPerson).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinder(masterPerson, true);
				var duplicates = ((IDuplicationFinder<GlbPerson, GlbPerson>)duplicationFinder).GetPotentialTargets(ZString.Empty);

				AssertEquals(1, duplicates.Count());
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}

		[UseSnapshotProtection]
		public override void TestGetPotentialTargetsAsync()
		{
			var pk = ZGuid.Empty;
			var instance = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var address1 = "PADDINGTON NSW";
					var email = "ABCD@TEST.COM";
					var factory = new BusinessObjectFactory();
					var list = GlbPersonDeduplicationTestData.NewValidTestData(factory);

					var masterPerson = list[0];
					masterPerson.PER_EmailAddress = email;
					masterPerson.Address1 = address1;

					pk = masterPerson.PK;

					var targetPerson = list[1];
					targetPerson.PER_EmailAddress = email;
					targetPerson.Address1 = address1;

					var pmt = factory.NewWithValidTestData<PatternMatchingResult>();
					pmt.PMT_MasterPK = masterPerson.PK;
					pmt.PMT_TargetPK = targetPerson.PK;
					pmt.PMT_TargetTableCode = pmt.PMT_MasterTableCode = "PER";
					pmt.PMT_Status = "TIG";
					pmt.PMT_GS_NKExcludeBy = "STD";

					var targetName = factory.New<PatternMatchingName>();
					targetName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(masterPerson.PER_FullName);
					targetName.PMN_PER = targetPerson.PK;
					targetName.PMN_ParentTableCode = "PER";
					targetName.PMN_ParentId = targetPerson.PK;
					targetName.PMN_RN_NKCountryCode = "AU";

					var targetAddress = factory.New<PatternMatchingAddress>();
					targetAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(address1);
					targetAddress.PMA_PER = targetPerson.PK;
					targetAddress.PMA_ParentTableCode = "PER";
					targetAddress.PMA_ParentId = targetPerson.PK;
					targetAddress.PMA_RN_NKCountryCode = "AU";

					var targetEmail = factory.New<PatternMatchingEmail>();
					targetEmail.PME_HashedValue = TextStandardizerHelper.ComputeStringHashFast(email);
					targetEmail.PME_PER = targetPerson.PK;
					targetEmail.PME_ParentTableCode = "PER";
					targetEmail.PME_ParentId = targetPerson.PK;
					targetEmail.PME_RN_NKCountryCode = "AU";

					factory.Save();

					var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
					try
					{
						SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
						((IDeduplicatable)masterPerson).ShouldRunDeduplication = true;

						IDuplicationFinder<GlbPerson, GlbPerson> duplicationFinder = new GlbPersonDuplicationFinder(masterPerson, false);

						return duplicationFinder.GetPotentialDuplicatesAsync("STD").Result;
					}
					finally
					{
						SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
					}
				}
			}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current);

			var result2 = instance.Result.ToList();

			AssertEquals(0, result2.Count);

			instance.Dispose();

			instance = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var masterPerson = new BusinessObjectFactory().Load<GlbPerson>(pk);
					var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
					try
					{
						SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
						((IDeduplicatable)masterPerson).ShouldRunDeduplication = true;

						IDuplicationFinder<GlbPerson, GlbPerson> duplicationFinder = new GlbPersonDuplicationFinder(masterPerson, false);

						return duplicationFinder.GetPotentialDuplicatesAsync("OST").Result;
					}
					finally
					{
						SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
					}
				}
			}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current);

			result2 = instance.Result.ToList();

			AssertEquals(1, result2.Count);
		}

		[UseSnapshotProtection]
		public override void TestGetPotentialTargetsByEmailDomainAsync()
		{
			var factory = new BusinessObjectFactory();
			var list = GlbPersonDeduplicationTestData.NewValidTestData(factory);
			var per = list[0];
			var per2 = list[1];
			var mail = "Anyone@wisetechglobal.com";
			var hash = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.ExtractEmailDomain(mail));
			var pmd = factory.NewWithValidTestData<PatternMatchingDomain>();
			pmd.PMD_PER = per.PK;
			pmd.PMD_HashedValue = hash;
			var pmd1 = factory.NewWithValidTestData<PatternMatchingDomain>();
			pmd1.PMD_PER = per2.PK;
			pmd1.PMD_HashedValue = hash;
			factory.Save();

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var instance = Task.Factory.StartNew(() =>
				{
					var duplicationFinder = new GlbPersonDuplicationFinder(null, false);
					return duplicationFinder.GetPotentialTargetsByEmailDomainAsync(mail);
				}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current);
				var result = instance.Result;
				var result2 = result.Result;
				AssertEquals(2, result2.Count());
				AssertContainsExactElementsInAnyOrder(new[] { per.PK, per2.PK }, result2.Select(x => x.PK));
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}
		void AssertFindResults(bool shouldUsePlaceholder, bool shouldLoadStandardizationRules = true)
		{
			const string placeholder = "N/A";
			var address1 = shouldUsePlaceholder ? placeholder : "PADDINGTON NSW";
			var email = shouldUsePlaceholder ? placeholder : "ABCD@TEST.COM";
			var factory = new BusinessObjectFactory();
			var list = GlbPersonDeduplicationTestData.NewValidTestData(factory);
			var masterPerson = list[0];
			var targetPerson = list[1];
			masterPerson.Address1 = address1;
			targetPerson.Address1 = address1;
			masterPerson.PER_EmailAddress = email;
			targetPerson.PER_EmailAddress = email;

			var targetName = factory.New<PatternMatchingName>();
			targetName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(masterPerson.PER_FullName);
			targetName.PMN_PER = targetPerson.PK;
			targetName.PMN_ParentTableCode = "PER";
			targetName.PMN_ParentId = targetPerson.PK;
			targetName.PMN_RN_NKCountryCode = "AU";

			var targetAddress = factory.New<PatternMatchingAddress>();
			targetAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(address1);
			targetAddress.PMA_PER = targetPerson.PK;
			targetAddress.PMA_ParentTableCode = "PER";
			targetAddress.PMA_ParentId = targetPerson.PK;
			targetAddress.PMA_RN_NKCountryCode = "AU";

			var targetEmail = factory.New<PatternMatchingEmail>();
			targetEmail.PME_HashedValue = TextStandardizerHelper.ComputeStringHashFast(email);
			targetEmail.PME_PER = targetPerson.PK;
			targetEmail.PME_ParentTableCode = "PER";
			targetEmail.PME_ParentId = targetPerson.PK;
			targetEmail.PME_RN_NKCountryCode = "AU";

			factory.Save();

			var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
			try
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, shouldLoadStandardizationRules);
				((IDeduplicatable)masterPerson).ShouldRunDeduplication = true;
				var duplicationFinder = new GlbPersonDuplicationFinder(masterPerson, false);
				var duplicates = duplicationFinder.FindPotentialDuplicates(true);

				if (shouldUsePlaceholder)
				{
					AssertEquals(0, duplicates.Count);
				}
				else if (!shouldLoadStandardizationRules)
				{
					AssertEquals(0, duplicates.Count);
				}
				else
				{
					AssertEquals(1, duplicates.Count);
				}
			}
			finally
			{
				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			}
		}
	}
}
