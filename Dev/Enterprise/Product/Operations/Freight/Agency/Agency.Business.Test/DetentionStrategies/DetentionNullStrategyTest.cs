using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(DetentionNullStrategy))]
	internal class DetentionNullStrategyTest : DetentionStrategyBaseTest<DetentionNullStrategy>
	{
		public void TestDefaultDetentionDays()
		{
			AssertEquals(null, MovementType.GetDefaultDetentionDays(Movement));
		}

		public void TestStartOfDetentionFreePeriod()
		{
			AssertEquals(ZDateTime.Empty, MovementType.GetStartOfDetentionFreePeriod(Movement));
		}

		public void TestStartOfDetentionPeriod()
		{
			AssertEquals(ZDateTime.Empty, MovementType.GetStartOfDetentionPeriod(Movement));
		}

		public void TestDetentionFreeDays()
		{
			AssertEquals((short)0, MovementType.GetDetentionFreeDays(Movement));
		}

		#region Implementation
		protected override string[] GetDetentionTypes()
		{
			return new string[] { "" };
		}
		#endregion
	}
}
