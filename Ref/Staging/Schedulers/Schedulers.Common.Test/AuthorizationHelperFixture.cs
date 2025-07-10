using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common.Test
{
	[TestFixture]
	public class AuthorizationHelperFixture
	{
		[Test]
		public void IsAuthorized()
		{
			string group = null;
			string command = null;
			var formCollectionService = new Mock<IFormCollectionService>();
			formCollectionService.Setup(x => x.IsAllowedCommand).Returns(true);

			Assert.True(authorizationHelper.IsAuthorized("whatever user", formCollectionService.Object));
			Assert.True(authorizationHelper.IsAuthorized("copr\\userA", formCollectionService.Object));
			command = "get_date";
			formCollectionService.Setup(x => x.Command).Returns(command);
			formCollectionService.Setup(x => x.IsGetCommand).Returns(true);
			Assert.True(authorizationHelper.IsAuthorized("userA", formCollectionService.Object));
			Assert.True(authorizationHelper.IsAuthorized("copr\\userA", formCollectionService.Object));
			command = "execute_job";
			formCollectionService.Setup(x => x.Command).Returns(command);
			formCollectionService.Setup(x => x.IsGetCommand).Returns(false);
			Assert.False(authorizationHelper.IsAuthorized("corp\\userA", formCollectionService.Object));
			Assert.False(authorizationHelper.IsAuthorized("corp\\userB", formCollectionService.Object));

			group = "GroupB";
			formCollectionService.Setup(x => x.Group).Returns(group);
			Assert.True(authorizationHelper.IsAuthorized("corp\\userA", formCollectionService.Object));
			Assert.False(authorizationHelper.IsAuthorized("corp\\userB", formCollectionService.Object));
			group = "GroupC";
			formCollectionService.Setup(x => x.Group).Returns(group);
			Assert.False(authorizationHelper.IsAuthorized("corp\\userA", formCollectionService.Object));
			Assert.True(authorizationHelper.IsAuthorized("corp\\userB", formCollectionService.Object));

			command = "puase_trigger";
			var trigger = "trigger 1";
			formCollectionService.Setup(x => x.Command).Returns(command);
			formCollectionService.Setup(x => x.Trigger).Returns(trigger);
			formCollectionService.Setup(x => x.IsTriggerRelatedCommand).Returns(true);
			Assert.False(authorizationHelper.IsAuthorized("corp\\userA", formCollectionService.Object));
			Assert.False(authorizationHelper.IsAuthorized("corp\\userB", formCollectionService.Object));

			command = "delete_trigger";
			trigger = "trigger 2";
			formCollectionService.Setup(x => x.Command).Returns(command);
			formCollectionService.Setup(x => x.Trigger).Returns(trigger);
			Assert.True(authorizationHelper.IsAuthorized("corp\\userA", formCollectionService.Object));
			Assert.False(authorizationHelper.IsAuthorized("corp\\userB", formCollectionService.Object));

			command = "add_trigger";
			trigger = null;
			formCollectionService.Setup(x => x.Command).Returns(command);
			formCollectionService.Setup(x => x.Trigger).Returns(trigger);
			formCollectionService.Setup(x => x.Group).Returns("GroupC");
			Assert.False(authorizationHelper.IsAuthorized("corp\\userA", formCollectionService.Object));
			Assert.True(authorizationHelper.IsAuthorized("corp\\userB", formCollectionService.Object));
			safeRepo.Verify(x => x.GetLatest<UserAuthorization>(), Times.AtLeastOnce);
		}

		[Test]
		public void IsAuthorized_CheckAuthorization()
		{
			authorizationHelper = new AuthorizationHelper(safeRepo.Object, stagingRepo.Object, false);
			var formCollectionService = new Mock<IFormCollectionService>();
			formCollectionService.Setup(x => x.IsAllowedCommand).Returns(true);
			var command = "get_date";
			formCollectionService.Setup(x => x.Command).Returns(command);
			formCollectionService.Setup(x => x.IsGetCommand).Returns(true);
			Assert.True(authorizationHelper.IsAuthorized("whatever user", formCollectionService.Object));
			Assert.True(authorizationHelper.IsAuthorized("corp\\userA", formCollectionService.Object));
			command = "execute_job";
			formCollectionService.Setup(x => x.Command).Returns(command);
			formCollectionService.Setup(x => x.IsGetCommand).Returns(false);
			Assert.True(authorizationHelper.IsAuthorized("userA", formCollectionService.Object));
			var group = "Reference Data";
			formCollectionService.Setup(x => x.Group).Returns(group);
			Assert.False(authorizationHelper.IsAuthorized("userA", formCollectionService.Object));

			group = "DEFAULT";
			formCollectionService.Setup(x => x.Group).Returns(group);
			formCollectionService.Setup(x => x.IsSchedulerOrGroupRelatedCommand).Returns(true);
			command = "pause_group";
			formCollectionService.Setup(x => x.Command).Returns(command);
			Assert.False(authorizationHelper.IsAuthorized("userA", formCollectionService.Object));
			command = "stop_scheduler";
			formCollectionService.Setup(x => x.Command).Returns(command);
			Assert.False(authorizationHelper.IsAuthorized("userA", formCollectionService.Object));
		}

		[SetUp]
		public void Setup()
		{
			safeRepo = new Mock<ISafeRepository>();
			safeRepo.Setup(x => x.GetLatest<UserAuthorization>()).Returns(authorizations.AsQueryable());
			stagingRepo = new Mock<IStagingRepository>();
			stagingRepo.Setup(x => x.Get<QRTZ_TRIGGERS>()).Returns(triggers.AsQueryable());
			authorizationHelper = new AuthorizationHelper(safeRepo.Object, stagingRepo.Object, true);
		}

		Mock<ISafeRepository> safeRepo;
		Mock<IStagingRepository> stagingRepo;
		IAuthorizationHelper authorizationHelper;
		readonly UserAuthorization[] authorizations = new[]
		{
			new UserAuthorization { UA_User = "corp\\userA", UA_DataSetName = "DatasetA", UA_TableName = "GroupA" },
			new UserAuthorization { UA_User = "corp\\userA", UA_DataSetName = "Quartz", UA_TableName = "GroupA" },
			new UserAuthorization { UA_User = "corp\\userA", UA_DataSetName = "Quartz", UA_TableName = "GroupB" },
			new UserAuthorization { UA_User = "corp\\userB", UA_DataSetName = "Quartz", UA_TableName = "GroupC" }
		};
		readonly QRTZ_TRIGGERS[] triggers = new[]
		{
			new QRTZ_TRIGGERS { TRIGGER_NAME = "trigger 1", TRIGGER_GROUP = "Default", JOB_NAME = "job 1", JOB_GROUP = "Default" },
			new QRTZ_TRIGGERS { TRIGGER_NAME = "trigger 2", TRIGGER_GROUP = "Default", JOB_NAME = "job 2", JOB_GROUP = "GroupA" }
		};
	}
}
