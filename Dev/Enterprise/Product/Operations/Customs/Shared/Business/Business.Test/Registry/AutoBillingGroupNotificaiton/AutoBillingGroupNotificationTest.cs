using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutoBillingGroupNotification))]
	sealed class AutoBillingGroupNotificationTest : RegistryBusinessObjectTemplateTestCase<AutoBillingGroupNotification>
	{
		public void TestValidateSendGroupPK()
		{
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Guid.NewGuid();
			AssertHasErrorContaining(groupNotification.SendGroupPKInfo, ListValidation.InvalidCodeError);

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			groupNotification.SendGroupPK = group.PK;
			AssertNoErrorContaining(groupNotification.SendGroupPKInfo, ListValidation.InvalidCodeError);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AutoBillingGroupNotification GetBusinessObjectToClone()
		{
			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = group.PK;
			groupNotification.SuppressUnpostARNotificaiton = true;
			return groupNotification;
		}

		protected override AutoBillingGroupNotification GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
