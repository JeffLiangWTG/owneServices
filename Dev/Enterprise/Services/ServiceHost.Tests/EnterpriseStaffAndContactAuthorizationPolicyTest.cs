using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Security.Principal;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	[UseSnapshotProtection]
	public class EnterpriseStaffAndContactAuthorizationPolicyTest : TestCase
	{
		public void TestUsingDbConnectionInEvaluate()
		{
			var exMsg = "";
			var lastMessageReported = "";
			ErrorReporter.Clear();

			var thread = new Thread(() =>
			{
				try
				{
					var authorizationPolicy = new EnterpriseStaffAndContactAuthorizationPolicyForTest();
					object state = null;
					authorizationPolicy.Evaluate(null, ref state);
				}
				catch (Exception ex)
				{
					exMsg = ex.Message;
					lastMessageReported = ErrorReporter.LastMessageReported;
				}
			});

			thread.Start();
			thread.Join();

			ErrorReporter.Clear();
			AssertEquals("", lastMessageReported);
			AssertEquals("throw exception from GetIdentities", exMsg);
		}

		class EnterpriseStaffAndContactAuthorizationPolicyForTest : EnterpriseStaffAndContactAuthorizationPolicy
		{
			protected override IEnumerable<IIdentity> GetIdentities(EvaluationContext evaluationContext)
			{
				var connection = Db.Connection;
				throw new Exception("throw exception from GetIdentities");
			}
		}
	}

	public class EnterpriseStaffAndContactAuthorizationPolicyFactoryTest : TestCaseWithFactory
	{
		public void TestEvaluate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "jim";
			Factory.Save();
			var heartbeatPk = CreateHeartbeatForStaff(staff);

			var id = new Mock<IIdentity>();
			id.Setup(m => m.AuthenticationType).Returns(nameof(EnterpriseStaffAndContactValidator));
			id.Setup(m => m.Name).Returns(heartbeatPk.ToString());

			var idList = new List<IIdentity>();
			idList.Add(id.Object);

			var props = new Dictionary<string, object>();
			props.Add("Identities", idList);
			var context = new Mock<EvaluationContext>();
			context.Setup(m => m.Properties).Returns(props);

			var policy = new EnterpriseStaffAndContactAuthorizationPolicy();
			object state = null;
			AssertEquals(true, policy.Evaluate(context.Object, ref state));
			var newId = (GenericIdentity)((List<IIdentity>)props["Identities"])[0];
			AssertEquals("jim", newId.Name);
		}

		Guid CreateHeartbeatForStaff(GlbStaff staff)
		{
			var heartbeatPk = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery("insert into dbo.StmServiceHeartbeat (SV_PK, SV_ParentId, SV_ParentTableCode, SV_ExpiresAtUtc) VALUES ('" + heartbeatPk + "', '" + staff.PK + "', 'GS', DateAdd(hour, 1, GetUtcDate()))");

			return heartbeatPk;
		}
	}
}
