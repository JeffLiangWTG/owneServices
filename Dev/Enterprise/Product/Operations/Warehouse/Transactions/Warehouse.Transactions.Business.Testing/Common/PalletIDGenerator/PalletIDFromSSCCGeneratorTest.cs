using System;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[UseSnapshotProtection]
	class PalletIDFromSSCCGeneratorTest : TestCase
	{
		#region ObjectFactoryRegistration

		public void TestObjectFactoryRegistration()
		{
			AssertType<PalletIDFromSSCCGenerator>(ObjectFactory.Get<IPalletIDFromSSCCGenerator>());
		}

		#endregion

		#region GenerateIDs

		public void TestGenerateIDs_NullString_ShouldError()
		{
			var ssccGenerator = new PalletIDFromSSCCGenerator();

			AssertExceptionThrown<ArgumentException>(() => ssccGenerator.GenerateIDs(null, 1));
		}

		public void TestGenerateIDs_EmptyString_ShouldError()
		{
			var ssccGenerator = new PalletIDFromSSCCGenerator();

			AssertExceptionThrown<ArgumentException>(() => ssccGenerator.GenerateIDs("", 1));
		}

		public void TestGenerateIDs_NumberOfIDsLessThanOne_ShouldError()
		{
			var ssccGenerator = new PalletIDFromSSCCGenerator();

			AssertExceptionThrown<ArgumentOutOfRangeException>(() => ssccGenerator.GenerateIDs("prefix", 0));
		}

		public void TestGenerateIDs()
		{
			var ssccGenerator = new PalletIDFromSSCCGenerator();

			AssertContainsExactElementsInExactOrder(
				new ZString[] { "021111120000000012", "021111120000000029" },
				ssccGenerator.GenerateIDs("2111112", 2)
			);
		}

		public void TestGenerateIDs_WillCommitTransaction()
		{
			var ssccGenerator = new PalletIDFromSSCCGenerator();

			AssertContainsExactElementsInExactOrder(
				new ZString[] { "021111120000000012" },
				ssccGenerator.GenerateIDs("2111112", 1)
			);

			ssccGenerator = new PalletIDFromSSCCGenerator();

			AssertContainsExactElementsInExactOrder(
				new ZString[] { "021111120000000029" },
				ssccGenerator.GenerateIDs("2111112", 1)
			);
		}

		#endregion
	}
}
