using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutoBillingGroupNotificationRegistryItemDataType))]
	sealed class AutoBillingGroupNotificationRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AutoBillingGroupNotificationRegistryItemDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "AutoBillingGroupNotificationRegistryItemEditor"; }
		}

		protected override AutoBillingGroupNotificationRegistryItemDataType GetNewDataType()
		{
			return new AutoBillingGroupNotificationRegistryItemDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var businessObjectFactory = new BusinessObjectFactory();
			var group = businessObjectFactory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification1 = new AutoBillingGroupNotification();
			groupNotification1.SendGroupPK = group.PK;
			var groupNotification2 = new AutoBillingGroupNotification();
			groupNotification2.SendGroupPK = group.PK;
			groupNotification2.SuppressUnpostARNotificaiton = true;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(groupNotification1, new AutoBillingGroupNotificationRegistryItemDataType().Serialise(groupNotification1)),
				new ValidSampleAndBinaryValueInDB(groupNotification2, new AutoBillingGroupNotificationRegistryItemDataType().Serialise(groupNotification2))
			};
		}
	}
}
