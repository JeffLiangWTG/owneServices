using System;
using System.ServiceModel;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	[UseSnapshotProtection]
	public class EnterpriseUserNamePasswordValidatorInThreadTest : TestCase
	{
		public void TestValidateInThread()
		{
			var factory = new BusinessObjectFactory();

			var activeStaff = factory.NewWithValidTestData<GlbStaff>();
			activeStaff.GS_LoginName = "active";
			var activeStafHeartbeatPk = EnterpriseUserNamePasswordValidatorTest.CreateHeartbeatForStaff(activeStaff);

			var inactiveStaff = factory.NewWithValidTestData<GlbStaff>();
			inactiveStaff.GS_IsActive = false;
			inactiveStaff.GS_LoginName = "inactive";
			var inactiveStaffHeartbeatPk = EnterpriseUserNamePasswordValidatorTest.CreateHeartbeatForStaff(inactiveStaff);

			var lockedOutStaff = factory.NewWithValidTestData<GlbStaff>();
			var lockedOutPerson = factory.NewWithValidTestData<GlbPerson>();
			lockedOutStaff.GS_PER = lockedOutPerson.PK;
			lockedOutStaff.Person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddDays(1);
			lockedOutStaff.GS_LoginName = "locked";
			var lockedOutStaffHeartbeatPk = EnterpriseUserNamePasswordValidatorTest.CreateHeartbeatForStaff(lockedOutStaff);

			var notLockedOutStaff = factory.NewWithValidTestData<GlbStaff>();
			var notLockedOutPerson = factory.NewWithValidTestData<GlbPerson>();
			notLockedOutStaff.GS_PER = notLockedOutPerson.PK;
			notLockedOutStaff.Person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddDays(-1);
			notLockedOutStaff.GS_LoginName = "not_locked";
			var notLockedOutStaffHeartbeatPk = EnterpriseUserNamePasswordValidatorTest.CreateHeartbeatForStaff(notLockedOutStaff);

			factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.PasswordForTest = "myRegistrationPassword";

			var thread = new Thread(() =>
			{
				var validator = new EnterpriseUserNamePasswordValidator();
				AssertExceptionThrown<FaultException>(() => validator.Validate(activeStafHeartbeatPk.ToString(), "test"));
				AssertNoExceptionThrown(() => validator.Validate(activeStafHeartbeatPk.ToString(), "myRegistrationPassword"));
				AssertExceptionThrown<FaultException>(() => validator.Validate(inactiveStaffHeartbeatPk.ToString(), "myRegistrationPassword"));
				AssertExceptionThrown<FaultException>(() => validator.Validate(lockedOutStaffHeartbeatPk.ToString(), "myRegistrationPassword"));
				AssertExceptionThrown<FaultException>(() => validator.Validate(Guid.NewGuid().ToString(), "myRegistrationPassword"));
				AssertNoExceptionThrown(() => validator.Validate(notLockedOutStaffHeartbeatPk.ToString(), "myRegistrationPassword"));
			});

			thread.Start();
			thread.Join();
		}
	}

	public class EnterpriseUserNamePasswordValidatorTest : TestCaseWithFactory
	{
		public void TestValidate_Staff()
		{
			var activeStaff = Factory.NewWithValidTestData<GlbStaff>();
			activeStaff.GS_LoginName = "active";
			var activeStafHeartbeatPk = CreateHeartbeatForStaff(activeStaff);

			var inactiveStaff = Factory.NewWithValidTestData<GlbStaff>();
			inactiveStaff.GS_IsActive = false;
			inactiveStaff.GS_LoginName = "inactive";
			var inactiveStaffHeartbeatPk = CreateHeartbeatForStaff(inactiveStaff);

			var lockedOutStaff = Factory.NewWithValidTestData<GlbStaff>();
			var lockedOutPerson = Factory.NewWithValidTestData<GlbPerson>();
			lockedOutStaff.GS_PER = lockedOutPerson.PK;
			lockedOutStaff.Person.PER_LoginDisabledUntilUtc = ZDateTime.Now.AddDays(1);
			lockedOutStaff.GS_LoginName = "locked";
			var lockedOutStaffHeartbeatPk = CreateHeartbeatForStaff(lockedOutStaff);

			var notLockedOutStaff = Factory.NewWithValidTestData<GlbStaff>();
			var notLockedOutPerson = Factory.NewWithValidTestData<GlbPerson>();
			notLockedOutStaff.GS_PER = notLockedOutPerson.PK;
			notLockedOutStaff.Person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddDays(-1);
			notLockedOutStaff.GS_LoginName = "not_locked";
			var notLockedOutStaffHeartbeatPk = CreateHeartbeatForStaff(notLockedOutStaff);

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.PasswordForTest = "myRegistrationPassword";

			Factory.Save();

			var validator = new EnterpriseUserNamePasswordValidator();
			AssertExceptionThrown<FaultException>(() => validator.Validate(activeStafHeartbeatPk.ToString(), "test"));
			AssertNoExceptionThrown(() => validator.Validate(activeStafHeartbeatPk.ToString(), "myRegistrationPassword"));
			AssertExceptionThrown<FaultException>(() => validator.Validate(inactiveStaffHeartbeatPk.ToString(), "myRegistrationPassword"));
			AssertExceptionThrown<FaultException>(() => validator.Validate(lockedOutStaffHeartbeatPk.ToString(), "myRegistrationPassword"));
			AssertExceptionThrown<FaultException>(() => validator.Validate(Guid.NewGuid().ToString(), "myRegistrationPassword"));
			AssertNoExceptionThrown(() => validator.Validate(notLockedOutStaffHeartbeatPk.ToString(), "myRegistrationPassword"));
		}

		public void TestValidate_Contact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var contactHeartbeatPk = CreateHeartbeatForContact(contact);

			var lockedContact = Factory.NewWithValidTestData<OrgContact>();
			var lockedContactHeartbeatPk = CreateHeartbeatForContact(lockedContact);
			var lockedOutPerson = Factory.NewWithValidTestData<GlbPerson>();
			lockedContact.OC_PER = lockedOutPerson.PK;
			lockedContact.Person.PER_LoginDisabledUntilUtc = ZDateTime.Now.AddDays(1);

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.PasswordForTest = "myRegistrationPassword";

			Factory.Save();

			var validator = new EnterpriseStaffAndContactValidator();
			AssertNoExceptionThrown(() => validator.Validate(contactHeartbeatPk.ToString(), "myRegistrationPassword"));
			AssertExceptionThrown<FaultException>(() => validator.Validate(lockedContactHeartbeatPk.ToString(), "myRegistrationPassword"));
		}

		internal static Guid CreateHeartbeatForStaff(GlbStaff staff)
		{
			var heartbeatPk = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery("insert into dbo.StmServiceHeartbeat (SV_PK, SV_ParentId, SV_ParentTableCode, SV_ExpiresAtUtc) VALUES ('" + heartbeatPk + "', '" + staff.PK + "', 'GS', DateAdd(hour, 1, GetUtcDate()))");

			return heartbeatPk;
		}

		internal static Guid CreateHeartbeatForContact(OrgContact contact)
		{
			var heartbeatPk = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery("insert into dbo.StmServiceHeartbeat (SV_PK, SV_ParentId, SV_ParentTableCode, SV_ExpiresAtUtc) VALUES ('" + heartbeatPk + "', '" + contact.PK + "', 'OC', DateAdd(hour, 1, GetUtcDate()))");

			return heartbeatPk;
		}
	}
}
