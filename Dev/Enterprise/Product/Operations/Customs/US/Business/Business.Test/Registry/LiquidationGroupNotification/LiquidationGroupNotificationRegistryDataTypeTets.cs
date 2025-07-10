using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(LiquidationGroupNotificationRegistryDataType))]
	sealed class LiquidationGroupNotificationRegistryDataTypeTets : NonPersistentBusinessObjectRegistryDataTypeTestCase<LiquidationGroupNotificationRegistryDataType>
	{
		protected override LiquidationGroupNotificationRegistryDataType GetNewDataType()
		{
			return new LiquidationGroupNotificationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var liquidationGroupNotification1 = LiquidationGroupNotification.Default;
			var liquidationGroupNotification2 = new LiquidationGroupNotification(GroupNotification.StaffMemberOrNominatedGroup, Core.Constants.Groups.AllPK, true);

			return
			[
				new ValidSampleAndBinaryValueInDB(liquidationGroupNotification1, new LiquidationGroupNotificationRegistryDataType().Serialise(liquidationGroupNotification1)),
				new ValidSampleAndBinaryValueInDB(liquidationGroupNotification2, new LiquidationGroupNotificationRegistryDataType().Serialise(liquidationGroupNotification2))
			];
		}

		protected override string ExpectedEditorName
		{
			get { return "LiquidationGroupNotificationRegistryItemEditor"; }
		}
	}
}
