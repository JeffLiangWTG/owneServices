using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Business.Testing
{
	public class FixSeedRandom : CartonisationRandom
	{
		public FixSeedRandom(int generatorSeed)
		{
			SeedGenerator = new Random(generatorSeed);
		}

		readonly Random SeedGenerator;

		protected override Random GetNewRandom()
		{
			lock (lockObject)
			{
				return new Random(SeedGenerator.Next());
			}
		}

		static readonly object lockObject = new object();
	}

	#region FixSeedRandomTest class

	public class FixSeedRandomTest : TransactionedTestCase
	{
		#region TestConstructor_AlwaysReturnSameResult

		public void TestConstructor_AlwaysReturnSameResult()
		{
			AssertContainsExactElementsInAnyOrder(GenerateRandomInMultiThread(100), GenerateRandomInMultiThread(100));
			AssertContainsExactElementsInAnyOrder(GenerateRandomInMultiThread(123), GenerateRandomInMultiThread(123));
			AssertContainsExactElementsInAnyOrder(GenerateRandomInMultiThread(125), GenerateRandomInMultiThread(125));
		}

		List<int> GenerateRandomInMultiThread(int seed)
		{
			var rnds = new List<int>();
			var numberofThreads = 15;
			var rndsInEachThread = 2;
			var rnd = new FixSeedRandom(seed);

			ThreadStart generateRandoms = () =>
			{
				var maxRandomRange = 200;
				for (int j = 0; j < rndsInEachThread; j++)
				{
					rnds.Add(rnd.Next(maxRandomRange));
				}
			};

			for (var i = 0; i < numberofThreads; i++)
			{
				var thread = new Thread(generateRandoms);
				thread.Start();
				thread.Join();
			}
			AssertEquals(numberofThreads * rndsInEachThread, rnds.Count);
			return rnds;
		}

		#endregion
	}

	#endregion
}
