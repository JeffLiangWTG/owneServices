using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.MasterFiles.Business;
	sealed class NotificationUpdaterTest : TestCaseWithFactory
	{
		public void TestStaticNewConstructorCoversAllTypesOfNotificationMessage()
		{
			var message = new EBACCANotificationType();
			foreach (EBACCANotificationTypeMessageType messageType in Enum.GetValues(typeof(EBACCANotificationTypeMessageType)))
			{
				message.MessageType = messageType;
				AssertNotNull(messageType + " type should be catered for.", NotificationUpdater.New(message, new ZStringBuilder()));
			}
		}
	}

	abstract class NotificationUpdaterTestCase : TestCaseForMessageTesting
	{
		protected override void SetUp()
		{
			base.SetUp();
			var acknowledgementGroup = GetGroupForDelivery("acknowledgement");
			var impedimentsGroup = GetGroupForDelivery("impediments");
			var errorsGroup = GetGroupForDelivery("errors");
			Factory.Save();

			NZCustomsDataRegistry.Instance.MAFeBACCaSendAcknowledgementsToGroup.SetValue(Guid.Empty, Branch.PK.ToGuid(), Guid.Empty, acknowledgementGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.MAFeBACCaSendImpedimentsToGroup.SetValue(Guid.Empty, Branch.PK.ToGuid(), Guid.Empty, impedimentsGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.MAFeBACCaSendErrorsToGroup.SetValue(Guid.Empty, Branch.PK.ToGuid(), Guid.Empty, errorsGroup.PK.ToGuid());
		}

		GlbGroup GetGroupForDelivery(string name)
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = name.Substring(0, 3);
			group.GG_Desc = name;
			var staff = group.Staff.AddNew();
			staff.GS_Code = name.Substring(0, 3);
			staff.GS_FullName = name;
			staff.GS_LoginName = name;
			staff.GS_EmailAddress = name + "@nowhere.com";
			return group;
		}
	}
}
