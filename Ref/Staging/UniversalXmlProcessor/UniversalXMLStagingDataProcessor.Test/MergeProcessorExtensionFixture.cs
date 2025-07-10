using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class MergeProcessorExtensionFixture
	{
		[Test]
		public async Task Retry()
		{
			var entities = new object[10000];
			int noOfCalls = 0;
			Func<IEnumerable<object>, int, Task<object[]>> funcToRetry = (x, y) =>
			{
				noOfCalls++;
				if (x.Count() == 10000)
				{
					return Task.FromResult(new object[1000]);
				}
				if (x.Count() == 1000)
				{
					return Task.FromResult(new object[100]);
				}
				if (x.Count() == 100)
				{
					return Task.FromResult(new object[10]);
				}
				return Task.FromResult(new object[1]);
			};
			var result = await MergeProcessorExtension.Retry(entities, funcToRetry, x => x, 1, 2, 100, Guid.NewGuid());
			Assert.AreEqual(100, result.Length);
			Assert.AreEqual(2, noOfCalls);

			noOfCalls = 0;
			result = await MergeProcessorExtension.Retry(entities, funcToRetry, x => x, 1, 3, 100, Guid.NewGuid());
			Assert.AreEqual(10, result.Length);
			Assert.AreEqual(3, noOfCalls);

			noOfCalls = 0;
			result = await MergeProcessorExtension.Retry(entities, funcToRetry, x => x, 1, 1000, 100, Guid.NewGuid());
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual(4, noOfCalls);
		}

		[Test]
		public void CalculateRetryMergeSize()
		{
			Assert.AreEqual(33, MergeProcessorExtension.CalculateRetryMergeSize(100, 5000));
			Assert.AreEqual(7, MergeProcessorExtension.CalculateRetryMergeSize(100, 70));
			Assert.AreEqual(3, MergeProcessorExtension.CalculateRetryMergeSize(10, 70));
			Assert.AreEqual(3, MergeProcessorExtension.CalculateRetryMergeSize(9, 100));
		}

		[TestCase(2)]
		[TestCase(3)]
		[TestCase(5)]
		public void CalculateMaxInitialMergeSize(int maximumRetries)
		{
			var mergeSize = MergeProcessorExtension.CalculateMaxInitialMergeSize(maximumRetries);
			for (var i = 0; i < maximumRetries; i++)
			{
				mergeSize = MergeProcessorExtension.CalculateRetryMergeSize(mergeSize, 5000);
			}
			Assert.AreEqual(1, mergeSize);

			mergeSize = MergeProcessorExtension.CalculateMaxInitialMergeSize(maximumRetries) + 1;
			for (var i = 0; i < maximumRetries; i++)
			{
				mergeSize = MergeProcessorExtension.CalculateRetryMergeSize(mergeSize, 5000);
			}
			Assert.True(mergeSize > 1);
		}

		[TestCase(10, 10, 10, 1, new int[] { 10 })]
		[TestCase(12, 10, 10, 2, new int[] { 10, 2 })]
		[TestCase(1000, 10, 10, 10, new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 })]
		[TestCase(5, 10, 10, 1, new int[] { 5 })]
		[TestCase(10, 1, 5, 5, new int[] { 1, 1, 1, 1, 1 })]
		[TestCase(100, 10, 10, 10, new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 })]
		public void TestSplitProcessingEntitiesIntoArray(int noOfEntities, int mergeSize, int maxNoOfBatches, int resultBatches, int[] mergedBatches)
		{
			var entities = CreateTuplesForTest(noOfEntities).ToList();
			var result = entities.SplitProcessingEntitiesIntoArray(mergeSize, maxNoOfBatches);

			Assert.That(result.Length, Is.EqualTo(resultBatches));
			for (int i = 0; i < mergedBatches.Length; i++)
			{
				Assert.That(result[i].Length, Is.EqualTo(mergedBatches[i]));
			}
		}

		Tuple<object, DataProcessingInformation>[] CreateTuplesForTest(int quantity)
		{
			Tuple<object, DataProcessingInformation>[] result = new Tuple<object, DataProcessingInformation>[quantity];
			for (int i = 0; i < quantity; i++)
			{
				result[i] = new Tuple<object, DataProcessingInformation>(new RefCusTariff(), new DataProcessingInformation());
			}
			return result;
		}
	}
}
