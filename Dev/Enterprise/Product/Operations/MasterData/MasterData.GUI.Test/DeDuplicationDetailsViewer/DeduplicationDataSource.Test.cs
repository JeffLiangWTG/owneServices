using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Business;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	abstract class DeduplicationDataSourceTest<TBizO, TCandidate> : TestCaseWithFactory
		where TBizO : IBusiness
		where TCandidate : DuplicationCandidate
	{
		public abstract void TestGetDuplicates();

		public abstract void TestGetPotentialDuplicationModel();

		public abstract void TestGetDuplicationCandidate();

		[ExpectNoExceptions]
		public abstract void TestNullCurrentBizoParrameterInGetDuplication();

		[ExpectNoExceptions]
		public abstract void TestNullInTargetParameterInGetDuplication();

		public void TestGetPotentialDuplicates_Ordered()
		{
			var dataSource = GetDataSource();
			var result1 = dataSource.GetPotentialDuplicates();
			var result2 = dataSource.GetDuplicationCandidates();

			AssertResultsOrdered(result1.ToList());
			AssertResultsOrdered(result2.ToList());
		}

		public void TestGetCorrectConfidenceScore()
		{
			var dataSource = GetDataSource();
			var result1 = dataSource.GetPotentialDuplicates();
			var result2 = dataSource.GetDuplicationCandidates();

			foreach (var duplicate in result1)
			{
				var scoreStr = string.Format(CultureInfo.CurrentCulture, "{0}%", duplicate.Confidence == ConfidenceRating.None ? 0 : duplicate.Score * 100);

				AssertEquals("ConfidenceScore should be equal", scoreStr, duplicate.ConfidenceScore);
			}

			foreach (var duplicate in result2)
			{
				var scoreStr = string.Format(CultureInfo.CurrentCulture, "{0}%", duplicate.Confidence == ConfidenceRating.None ? 0 : duplicate.Score * 100);

				AssertEquals("ConfidenceScore should be equal", scoreStr, duplicate.ConfidenceScore);
			}
		}

		#region Implementation

		protected abstract IDeduplicationDataSource GetDataSource();

		protected abstract TBizO MasterBizO();

		protected abstract List<DeduplicationPresenterModel> PresenterModels();

		protected abstract void AssertResultsOrdered(List<PotentialDuplicationModel> duplicates);

		protected abstract void AssertResultsOrdered(List<DuplicationCandidate> duplicates);

		#endregion
	}
}
