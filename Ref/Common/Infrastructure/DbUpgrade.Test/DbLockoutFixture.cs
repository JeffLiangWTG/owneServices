using System.Data;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.DbUpgrade.Test
{
	[TestFixture]
	class DbLockoutFixture
	{
		[Test]
		public void LockOut()
		{
			var connection = new Mock<IDbConnection>();
			var cmd = new Mock<IDbCommand>();
			connection.Setup(x => x.CreateCommand()).Returns(cmd.Object);
			var lockout = new DbLockout(connection.Object, new[] { "User1", "User2" });
			var release = lockout.Acquire();
			cmd.VerifySet(x => x.CommandText = It.Is<string>(y => y.Contains("REVOKE CONNECT FROM User1")));
			cmd.VerifySet(x => x.CommandText = It.Is<string>(y => y.Contains("REVOKE CONNECT FROM User2")));
			release.Dispose();
			cmd.VerifySet(x => x.CommandText = It.Is<string>(y => y.Contains("GRANT CONNECT TO User1")));
			cmd.VerifySet(x => x.CommandText = It.Is<string>(y => y.Contains("GRANT CONNECT TO User2")));
		}
	}
}
