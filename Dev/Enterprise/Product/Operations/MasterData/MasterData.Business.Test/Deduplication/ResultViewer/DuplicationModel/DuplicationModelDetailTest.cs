using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Test
{
	public abstract class DuplicationModelDetailTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetDuplicationModelDetails_SourceColumns_OrganisationNames()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "OrganisationNames",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterValue = "Dummy Master Org",
					ChildTargetValue = "Dummy Candidate Org",
					ChildScoringResultGroupRating = ConfidenceRating.Low,
					ChildScore = 0.1
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(1, groups.Count());
			var group = groups.Single();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.OrganisationNames), group.Header);
			AssertContainsExactElementsInExactOrder(new[] { "Name", "Source" }, group.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "Name", "Source" }, group.CandidateColumns);

			AssertEquals(1, group.MasterModels.Count);
			var masterModel = (DuplicationModelDetail)group.MasterModels.Single();
			AssertEquals("Dummy Master Org", masterModel.Name);
			AssertEquals("Name", masterModel.Source);
			AssertEquals("Low", masterModel.Similarity);
			AssertEquals("10%", masterModel.ConfidenceScore);

			AssertEquals(1, group.CandidateModels.Count);
			var candidateModel = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals("Dummy Candidate Org", candidateModel.Name);
			AssertEquals("Name", candidateModel.Source);
			AssertEquals("Low", candidateModel.Similarity);
			AssertEquals("10%", candidateModel.ConfidenceScore);
		}

		public void TestGetDuplicationModelDetails_SourceColumns_PersonNames()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "PersonNames",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterValue = "Dummy Master Person",
					ChildTargetValue = "Dummy Candidate Person",
					ChildScoringResultGroupRating = ConfidenceRating.Low,
					ChildScore = 0.2
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(1, groups.Count());
			var group = groups.Single();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.PersonNames), group.Header);
			AssertContainsExactElementsInExactOrder(new[] { "Name", "Source" }, group.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "Name", "Source" }, group.CandidateColumns);

			AssertEquals(1, group.MasterModels.Count);
			var masterModel = (DuplicationModelDetail)group.MasterModels.Single();
			AssertEquals("Dummy Master Person", masterModel.Name);
			AssertEquals("Name", masterModel.Source);
			AssertEquals("Low", masterModel.Similarity);
			AssertEquals("20%", masterModel.ConfidenceScore);

			AssertEquals(1, group.CandidateModels.Count);
			var candidateModel = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals("Dummy Candidate Person", candidateModel.Name);
			AssertEquals("Name", candidateModel.Source);
			AssertEquals("Low", candidateModel.Similarity);
			AssertEquals("20%", candidateModel.ConfidenceScore);
		}

		public void TestGetDuplicationModelDetails_SourceColumns_PhoneNumbers()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "PhoneNumbers",
					ChildDisplayNameForMasterColumns = "Number",
					ChildDisplayNameForTargetColumns = "Number",
					ChildMasterValue = "123 123 123",
					ChildTargetValue = "321 321 321",
					ChildScoringResultGroupRating = ConfidenceRating.Low,
					ChildScore = 0.3
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(1, groups.Count());
			var group = groups.Single();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.PhoneNumbers), group.Header);
			AssertContainsExactElementsInExactOrder(new[] { "Number", "Source" }, group.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "Number", "Source" }, group.CandidateColumns);

			AssertEquals(1, group.MasterModels.Count);
			var masterModel = (DuplicationModelDetail)group.MasterModels.Single();
			AssertEquals("123 123 123", masterModel.Number);
			AssertEquals("Number", masterModel.Source);
			AssertEquals("Low", masterModel.Similarity);
			AssertEquals("30%", masterModel.ConfidenceScore);

			AssertEquals(1, group.CandidateModels.Count);
			var candidateModel = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals("321 321 321", candidateModel.Number);
			AssertEquals("Number", candidateModel.Source);
			AssertEquals("Low", candidateModel.Similarity);
			AssertEquals("30%", candidateModel.ConfidenceScore);
		}

		public void TestGetDuplicationModelDetails_SourceColumns_Websites()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Websites",
					ChildDisplayNameForMasterColumns = "Website",
					ChildDisplayNameForTargetColumns = "Website",
					ChildMasterValue = "www.master.com",
					ChildTargetValue = "www.candidate.com",
					ChildScoringResultGroupRating = ConfidenceRating.Low,
					ChildScore = 0.4
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(1, groups.Count());
			var group = groups.Single();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.Websites), group.Header);
			AssertContainsExactElementsInExactOrder(new[] { "Website", "Source" }, group.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "Website", "Source" }, group.CandidateColumns);

			AssertEquals(1, group.MasterModels.Count);
			var masterModel = (DuplicationModelDetail)group.MasterModels.Single();
			AssertEquals("www.master.com", masterModel.Website);
			AssertEquals("Website", masterModel.Source);
			AssertEquals("Low", masterModel.Similarity);
			AssertEquals("40%", masterModel.ConfidenceScore);

			AssertEquals(1, group.CandidateModels.Count);
			var candidateModel = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals("www.candidate.com", candidateModel.Website);
			AssertEquals("Website", candidateModel.Source);
			AssertEquals("Low", candidateModel.Similarity);
			AssertEquals("40%", candidateModel.ConfidenceScore);
		}

		public void TestGetDuplicationModelDetails_SourceColumns_Domains()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Domains",
					ChildDisplayNameForMasterColumns = "Domain",
					ChildDisplayNameForTargetColumns = "Domain",
					ChildMasterValue = "Dummy Master Domain",
					ChildTargetValue = "Dummy Candidate Domain",
					ChildScoringResultGroupRating = ConfidenceRating.Medium,
					ChildScore = 0.5
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(1, groups.Count());
			var group = groups.Single();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.Domains), group.Header);
			AssertContainsExactElementsInExactOrder(new[] { "Domain", "Source" }, group.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "Domain", "Source" }, group.CandidateColumns);

			AssertEquals(1, group.MasterModels.Count);
			var masterModel = (DuplicationModelDetail)group.MasterModels.Single();
			AssertEquals("Dummy Master Domain", masterModel.Domain);
			AssertEquals("Domain", masterModel.Source);
			AssertEquals("Medium", masterModel.Similarity);
			AssertEquals("50%", masterModel.ConfidenceScore);

			AssertEquals(1, group.CandidateModels.Count);
			var candidateModel = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals("Dummy Candidate Domain", candidateModel.Domain);
			AssertEquals("Domain", candidateModel.Source);
			AssertEquals("Medium", candidateModel.Similarity);
			AssertEquals("50%", candidateModel.ConfidenceScore);
		}

		public void TestGetDuplicationModelDetails_SourceColumns_Emails()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Emails",
					ChildDisplayNameForMasterColumns = "Email",
					ChildDisplayNameForTargetColumns = "Email",
					ChildMasterValue = "master@email.com",
					ChildTargetValue = "candidate@email.com",
					ChildScoringResultGroupRating = ConfidenceRating.Medium,
					ChildScore = 0.6
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(1, groups.Count());
			var group = groups.Single();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.Emails), group.Header);
			AssertContainsExactElementsInExactOrder(new[] { "Email", "Source" }, group.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "Email", "Source" }, group.CandidateColumns);

			AssertEquals(1, group.MasterModels.Count);
			var masterModel = (DuplicationModelDetail)group.MasterModels.Single();
			AssertEquals("master@email.com", masterModel.Email);
			AssertEquals("E-mail", masterModel.Source);
			AssertEquals("Medium", masterModel.Similarity);
			AssertEquals("60%", masterModel.ConfidenceScore);

			AssertEquals(1, group.CandidateModels.Count);
			var candidateModel = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals("candidate@email.com", candidateModel.Email);
			AssertEquals("E-mail", candidateModel.Source);
			AssertEquals("Medium", candidateModel.Similarity);
			AssertEquals("60%", candidateModel.ConfidenceScore);
		}

		public void TestGetDuplicationModelDetails_SourceColumns_Birthdays()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Birthdays",
					ChildDisplayNameForMasterColumns = "Birthday",
					ChildDisplayNameForTargetColumns = "Birthday",
					ChildMasterValue = "20230101",
					ChildTargetValue = "20230102",
					ChildScoringResultGroupRating = ConfidenceRating.Medium,
					ChildScore = 0.7
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(1, groups.Count());
			var group = groups.Single();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.Birthdays), group.Header);
			AssertContainsExactElementsInExactOrder(new[] { "Birthday", "Source" }, group.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "Birthday", "Source" }, group.CandidateColumns);

			AssertEquals(1, group.MasterModels.Count);
			var masterModel = (DuplicationModelDetail)group.MasterModels.Single();
			AssertEquals("20230101", masterModel.Birthday);
			AssertEquals("Birthday", masterModel.Source);
			AssertEquals("Medium", masterModel.Similarity);
			AssertEquals("70%", masterModel.ConfidenceScore);

			AssertEquals(1, group.CandidateModels.Count);
			var candidateModel = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals("20230102", candidateModel.Birthday);
			AssertEquals("Birthday", candidateModel.Source);
			AssertEquals("Medium", candidateModel.Similarity);
			AssertEquals("70%", candidateModel.ConfidenceScore);
		}

		public void TestGetDuplicationModelDetails_DynamicColumns()
		{
			var masterId = Guid.NewGuid();
			var candidateId = Guid.NewGuid();

			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildMasterID = masterId,
					ChildTargetID = candidateId,
					ChildDisplayNameForMasterColumns = "Address",
					ChildDisplayNameForTargetColumns = "Address",
					ChildMasterValue = "Master Address",
					ChildTargetValue = "Candidate Address",
					ChildScoringResultGroupRating = ConfidenceRating.High,
					ChildScore = 0.8
				},
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildMasterID = masterId,
					ChildTargetID = candidateId,
					ChildDisplayNameForMasterColumns = "AddressPhone",
					ChildDisplayNameForTargetColumns = "AddressPhone",
					ChildMasterValue = "111 222 333",
					ChildTargetValue = "333 222 111",
					ChildScoringResultGroupRating = ConfidenceRating.High,
					ChildScore = 0.8
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(1, groups.Count());
			var group = groups.Single();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.Addresses), group.Header);
			AssertContainsExactElementsInExactOrder(new[] { "Address", "AddressPhone" }, group.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "Address", "AddressPhone" }, group.CandidateColumns);

			AssertEquals(1, group.MasterModels.Count);
			var masterModel = (DuplicationModelDetail)group.MasterModels.Single();
			AssertEquals("Master Address", masterModel.Address);
			AssertEquals("111 222 333", masterModel.AddressPhone);
			AssertEquals("High", masterModel.Similarity);
			AssertEquals("80%", masterModel.ConfidenceScore);

			AssertEquals(1, group.CandidateModels.Count);
			var candidateModel = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals("Candidate Address", candidateModel.Address);
			AssertEquals("333 222 111", candidateModel.AddressPhone);
			AssertEquals("High", candidateModel.Similarity);
			AssertEquals("80%", candidateModel.ConfidenceScore);
		}

		public void TestGetDuplicationModelDetails_DynamicColumns_ActiveAssociations()
		{
			var masterId = Guid.NewGuid();
			var candidateId = Guid.NewGuid();

			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "ActiveAssociations",
					ChildMasterID = masterId,
					ChildTargetID = candidateId,
					ChildDisplayNameForMasterColumns = "PrimaryWorkplace",
					ChildDisplayNameForTargetColumns = "PrimaryWorkplace",
					ChildMasterValue = "TRUE",
					ChildTargetValue = "TRUE",
				},
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "ActiveAssociations",
					ChildMasterID = masterId,
					ChildTargetID = candidateId,
					ChildDisplayNameForMasterColumns = "Active",
					ChildDisplayNameForTargetColumns = "Active",
					ChildMasterValue = "FALSE",
					ChildTargetValue = "FALSE",
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(1, groups.Count());
			var group = groups.Single();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.ActiveAssociations), group.Header);
			AssertContainsExactElementsInExactOrder(new[] { "PrimaryWorkplace", "Active" }, group.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "PrimaryWorkplace", "Active" }, group.CandidateColumns);

			AssertEquals(1, group.MasterModels.Count);
			var masterModel = (DuplicationModelDetail)group.MasterModels.Single();
			AssertEquals(true, masterModel.PrimaryWorkplace);
			AssertEquals(false, masterModel.Active);

			AssertEquals(1, group.CandidateModels.Count);
			var candidateModel = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals(true, candidateModel.PrimaryWorkplace);
			AssertEquals(false, candidateModel.Active);
		}

		public void TestGetDuplicationModelDetails_DynamicColumns_UnHandledGroup_UnHandledColumn()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "UnHandledGroup",
					ChildMasterID = Guid.NewGuid(),
					ChildTargetID = Guid.NewGuid(),
					ChildDisplayNameForMasterColumns = "UnHandledColumn1",
					ChildDisplayNameForTargetColumns = "UnHandledColumn2",
					ChildScoringResultGroupRating = ConfidenceRating.High,
					ChildScore = 0.9
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(1, groups.Count());
			var group = groups.Single();
			AssertEquals("UnHandledGroup", group.Header);
			AssertContainsExactElementsInExactOrder(new[] { "UnHandledColumn1" }, group.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "UnHandledColumn2" }, group.CandidateColumns);

			AssertEquals(1, group.MasterModels.Count);
			var masterModel = (DuplicationModelDetail)group.MasterModels.Single();
			AssertEquals("High", masterModel.Similarity);
			AssertEquals("90%", masterModel.ConfidenceScore);

			AssertEquals(1, group.CandidateModels.Count);
			var candidateModel = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals("High", candidateModel.Similarity);
			AssertEquals("90%", candidateModel.ConfidenceScore);

			AssertEquals(2, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("A new column [UnHandledColumn1] has been added or an existing column has been renamed", ExceptionReporterTestListener.Instance[0].Message);
			AssertEquals("A new column [UnHandledColumn2] has been added or an existing column has been renamed", ExceptionReporterTestListener.Instance[1].Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestGetDuplicationModelDetails_MultipleGridsMultipleRows_WithEmptyRow()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "PhoneNumbers",
					ChildDisplayNameForMasterColumns = "Number",
					ChildDisplayNameForTargetColumns = "Number",
					ChildMasterValue = "123 123 123",
					ChildTargetValue = "123 123 123",
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildScore = 1
				},
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "PhoneNumbers",
					ChildDisplayNameForMasterColumns = "Number",
					ChildDisplayNameForTargetColumns = "Number",
					ChildMasterValue = "123 123 123",
					ChildTargetValue = "321 321 321",
					ChildScoringResultGroupRating = ConfidenceRating.Low,
					ChildScore = 0.3
				},
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildMasterID = Guid.NewGuid(),
					ChildTargetID = Guid.Empty,
					ChildDisplayNameForMasterColumns = "Address",
					ChildDisplayNameForTargetColumns = "Address",
					ChildMasterValue = "Master Address",
					ChildTargetValue = string.Empty,
					ChildScoringResultGroupRating = ConfidenceRating.Low,
					ChildScore = 0.1
				},
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildMasterID = Guid.NewGuid(),
					ChildTargetID = Guid.NewGuid(),
					ChildDisplayNameForMasterColumns = "Address",
					ChildDisplayNameForTargetColumns = "Address",
					ChildMasterValue = "Same Address",
					ChildTargetValue = "Same Address",
					ChildScoringResultGroupRating = ConfidenceRating.Exact,
					ChildScore = 1
				}
			};

			var groups = DuplicationModelDetail.GetDuplicationModelDetails(models);

			AssertEquals(2, groups.Count());
			var group1 = groups.First();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.PhoneNumbers), group1.Header);
			AssertContainsExactElementsInExactOrder(new[] { "Number", "Source" }, group1.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "Number", "Source" }, group1.CandidateColumns);

			AssertEquals(2, group1.MasterModels.Count);
			AssertEquals(2, group1.CandidateModels.Count);
			var group1masterModel1 = (DuplicationModelDetail)group1.MasterModels.First();
			AssertEquals("123 123 123", group1masterModel1.Number);
			AssertEquals("Number", group1masterModel1.Source);
			AssertEquals("Exact", group1masterModel1.Similarity);
			AssertEquals("100%", group1masterModel1.ConfidenceScore);
			var group1candidateModel1 = (DuplicationModelDetail)group1.CandidateModels.First();
			AssertEquals("123 123 123", group1candidateModel1.Number);
			AssertEquals("Number", group1candidateModel1.Source);
			AssertEquals("Exact", group1candidateModel1.Similarity);
			AssertEquals("100%", group1candidateModel1.ConfidenceScore);
			var group1masterModel2 = (DuplicationModelDetail)group1.MasterModels.Last();
			AssertEquals("123 123 123", group1masterModel2.Number);
			AssertEquals("Number", group1masterModel2.Source);
			AssertEquals("Low", group1masterModel2.Similarity);
			AssertEquals("30%", group1masterModel2.ConfidenceScore);
			var group1candidateModel2 = (DuplicationModelDetail)group1.CandidateModels.Last();
			AssertEquals("321 321 321", group1candidateModel2.Number);
			AssertEquals("Number", group1candidateModel2.Source);
			AssertEquals("Low", group1candidateModel2.Similarity);
			AssertEquals("30%", group1candidateModel2.ConfidenceScore);

			var group2 = groups.Last();
			AssertEquals(DedupeTranslationHelper.GetModelHeaderCaption(DeduplicationProvider.Constants.Addresses), group2.Header);
			AssertContainsExactElementsInExactOrder(new[] { "Address" }, group2.MasterColumns);
			AssertContainsExactElementsInExactOrder(new[] { "Similarity", "ConfidenceScore", "Address" }, group2.CandidateColumns);

			AssertEquals(2, group2.MasterModels.Count);
			AssertEquals(2, group2.CandidateModels.Count);
			var group2masterModel1 = (DuplicationModelDetail)group2.MasterModels.First();
			AssertEquals("Master Address", group2masterModel1.Address);
			AssertEquals("Low", group2masterModel1.Similarity);
			AssertEquals("10%", group2masterModel1.ConfidenceScore);
			var group2candidateModel1 = (DuplicationModelDetail)group2.CandidateModels.First();
			AssertEquals(string.Empty, group2candidateModel1.Address);
			AssertEquals("Low", group2candidateModel1.Similarity);
			AssertEquals("10%", group2candidateModel1.ConfidenceScore);
			var group2masterModel2 = (DuplicationModelDetail)group2.MasterModels.Last();
			AssertEquals("Same Address", group2masterModel2.Address);
			AssertEquals("Exact", group2masterModel2.Similarity);
			AssertEquals("100%", group2masterModel2.ConfidenceScore);
			var group2candidateModel2 = (DuplicationModelDetail)group2.CandidateModels.Last();
			AssertEquals("Same Address", group2candidateModel2.Address);
			AssertEquals("Exact", group2candidateModel2.Similarity);
			AssertEquals("100%", group2candidateModel2.ConfidenceScore);
		}

		public void TestBusinessObjectPkForMerge()
		{
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();

			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterID = guid1,
					ChildTargetID = guid2
				}
			};

			var group = DuplicationModelDetail.GetDuplicationModelDetails(models).Single();
			var master = (DuplicationModelDetail)group.MasterModels.Single();
			var candidate = (DuplicationModelDetail)group.CandidateModels.Single();
			AssertEquals(guid1, master.SelfBizoPk);
			AssertEquals(guid2, master.OpponentBizoPk);
			AssertEquals(guid2, candidate.SelfBizoPk);
			AssertEquals(guid1, candidate.OpponentBizoPk);
		}

		public void TestDefaultMergeMode_NoBizO()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterID = Guid.Empty
				}
			};

			var group = DuplicationModelDetail.GetDuplicationModelDetails(models).Single();
			var master = (DuplicationModelDetail)group.MasterModels.Single();

			AssertEquals(DedupeMergeMode.NotSupported, master.MergeMode);
		}

		public void TestDefaultMergeMode_GroupNotSupport()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "ActiveAssociations",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterID = Guid.NewGuid()
				}
			};

			var group = DuplicationModelDetail.GetDuplicationModelDetails(models).Single();
			var master = (DuplicationModelDetail)group.MasterModels.Single();

			AssertEquals(DedupeMergeMode.NotSupported, master.MergeMode);
		}

		public void TestDefaultMergeMode_LowOrNoneConfidence()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterID = Guid.NewGuid(),
					ChildScoringResultGroupRating = ConfidenceRating.Low
				},
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterID = Guid.NewGuid(),
					ChildScoringResultGroupRating = ConfidenceRating.None
				}
			};

			var group = DuplicationModelDetail.GetDuplicationModelDetails(models).Single();
			var masters = group.MasterModels.OfType<DuplicationModelDetail>();

			Assert(masters.All(x => x.MergeMode == DedupeMergeMode.Add));
		}

		public void TestDefaultMergeMode_Merge()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterID = Guid.NewGuid(),
					ChildScoringResultGroupRating = ConfidenceRating.Medium
				},
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterID = Guid.NewGuid(),
					ChildScoringResultGroupRating = ConfidenceRating.High
				},
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "Addresses",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildMasterID = Guid.NewGuid(),
					ChildScoringResultGroupRating = ConfidenceRating.Exact
				}
			};

			var group = DuplicationModelDetail.GetDuplicationModelDetails(models).Single();
			var masters = group.MasterModels.OfType<DuplicationModelDetail>();

			Assert(masters.All(x => x.MergeMode == DedupeMergeMode.Merge));
		}

		protected DeduplicationPresenterModel[] GetDeduplicationPresenterModels()
		{
			return new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = string.Empty,
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name"
				}
			};
		}
	}

	[TestedType(typeof(DuplicationModelDetail.DuplicationModelMasterDetail))]
	public class DuplicationModelMasterDetailTest : DuplicationModelDetailTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return DuplicationModelDetail.GetDuplicationModelDetails(GetDeduplicationPresenterModels()).Single().MasterModels.Single();
		}
	}

	[TestedType(typeof(DuplicationModelDetail.DuplicationModelCandidateDetail))]
	public class DuplicationModelCandidateDetailTest : DuplicationModelDetailTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return DuplicationModelDetail.GetDuplicationModelDetails(GetDeduplicationPresenterModels()).Single().CandidateModels.Single();
		}
	}
}
