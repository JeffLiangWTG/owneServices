using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class InstructionOrPackageDivotCodeDescriptionPairTest : TestCase
	{
		public void TestConstructor()
		{
			var pk = ZGuid.NewZGuid();
			var pair = new InstructionOrPackageDivotCodeDescriptionPair(pk, "JGU", (NoResString)"JGUDESC");

			AssertEquals(pk, pair.Identifier);
			AssertEquals(pk, (pair as ICodeDescription).PK);
			AssertEquals("JGU", pair.Code);
			AssertEquals("JGUDESC", pair.Description);
		}

		public void TestEquality()
		{
			var pk = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();
			var pair = new InstructionOrPackageDivotCodeDescriptionPair(pk, "JGU", (NoResString)"JGUDESC");
			var pair2 = new InstructionOrPackageDivotCodeDescriptionPair(pk, "JGU1", (NoResString)"JGUDESC1");
			var pair3 = new InstructionOrPackageDivotCodeDescriptionPair(pk2, "JGU", (NoResString)"JGUDESC");

			AssertEquals("Should equal to itself", pair, pair);
			AssertEquals("Should be equal", pair, pair2);
			AssertNotEquals("Should not be equal", pair, pair3);
		}

		public void TestGetHashCode()
		{
			var pk = ZGuid.NewZGuid();
			var pair = new InstructionOrPackageDivotCodeDescriptionPair(pk, "JGU", (NoResString)"JGUDESC");
			var pair2 = new InstructionOrPackageDivotCodeDescriptionPair(pk, "JGU1", (NoResString)"JGUDESC1");

			AssertEquals(pair.GetHashCode(), pair2.GetHashCode());
			AssertEquals(pk.GetHashCode(), pair.GetHashCode());
		}
	}
}
