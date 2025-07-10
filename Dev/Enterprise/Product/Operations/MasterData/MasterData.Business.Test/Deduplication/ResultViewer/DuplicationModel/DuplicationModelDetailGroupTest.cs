using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Test
{
	[TestedType(typeof(DuplicationModelDetailGroup))]
	public class DuplicationModelDetailGroupTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsSupportMergeOrNot()
		{
			var creator = new Func<string, DuplicationModelDetailGroup>(groupType => new DuplicationModelDetailGroup(
				groupType,
				Enumerable.Empty<DuplicationModelDetail>(),
				Enumerable.Empty<DuplicationModelDetail>(),
				Enumerable.Empty<string>(),
				Enumerable.Empty<string>()));

			Assert(creator.Invoke(DeduplicationProvider.Constants.Addresses).IsSupportMerge);
			Assert(creator.Invoke(DeduplicationProvider.Constants.Contacts).IsSupportMerge);

			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.Emails).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.Birthdays).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.Organisations).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.RegistrationCodes).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.Websites).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.Domains).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.PhoneNumbers).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.OrganisationNames).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.PersonNames).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.Person).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.Staff).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.Applicant).IsSupportMerge);
			AssertEquals(false, creator.Invoke(DeduplicationProvider.Constants.ActiveAssociations).IsSupportMerge);
		}

		public void TestOverallConfidence_Undefined()
		{
			var creator = new Func<string, DuplicationModelDetailGroup>(groupType => new DuplicationModelDetailGroup(
				groupType,
				Enumerable.Empty<DuplicationModelDetail>(),
				Enumerable.Empty<DuplicationModelDetail>(),
				Enumerable.Empty<string>(),
				Enumerable.Empty<string>()));

			AssertEquals(ConfidenceRating.Undefined, creator.Invoke(DeduplicationProvider.Constants.ActiveAssociations).ConfidenceRating);
			AssertEquals(string.Empty, creator.Invoke(DeduplicationProvider.Constants.ActiveAssociations).Confidence);
			AssertNull(creator.Invoke(DeduplicationProvider.Constants.ActiveAssociations).Score);
		}

		public void TestOverallConfidence()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "OrganisationNames",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildScoringResultGroupRating = ConfidenceRating.Medium,
					ChildScore = 0.7
				},
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = "OrganisationNames",
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name",
					ChildScoringResultGroupRating = ConfidenceRating.Low,
					ChildScore = 0.3
				}
			};

			var group = DuplicationModelDetail.GetDuplicationModelDetails(models).Single();

			AssertEquals(ConfidenceRating.Low, group.ConfidenceRating);
			AssertEquals(DedupeTranslationHelper.GetConfidenceDescription(group.ConfidenceRating.ToString()), group.Confidence);
			AssertEquals("50%", group.Score);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DuplicationModelDetailGroup(
				string.Empty,
				Enumerable.Empty<DuplicationModelDetail>(),
				Enumerable.Empty<DuplicationModelDetail>(),
				Enumerable.Empty<string>(),
				Enumerable.Empty<string>());
		}
	}
}
