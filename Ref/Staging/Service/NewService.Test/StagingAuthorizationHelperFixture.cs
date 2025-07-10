using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	class StagingAuthorizationHelperFixture
	{
		[Test]
		public void GetAuthorizationData()
		{
			var userAuth1 = new UserAuthorization { UA_User = "User1", UA_DataSetName = "All DataSets", UA_TableName = "*" };
			var userAuth2 = new UserAuthorization { UA_User = "User1", UA_DataSetName = "CodeList1", UA_TableName = "RefCusCodeList", UA_ColumnName = "ZZD_CodeType", UA_ColumnValue = "TST" };
			var userAuth3 = new UserAuthorization { UA_User = "User1", UA_DataSetName = "CodeType", UA_TableName = "RefCusCodeType" };
			var userAuth4 = new UserAuthorization { UA_User = "User2", UA_DataSetName = "All DataSets", UA_TableName = "*" };
			var userAuth5 = new UserAuthorization { UA_User = "User2", UA_DataSetName = "CodeList2", UA_TableName = "RefCusCodeList" };

			var repo = new Mock<ISafeRepository>();
			repo.Setup(x => x.GetLatest<UserAuthorization>()).Returns(new[] { userAuth1, userAuth2, userAuth3, userAuth4, userAuth5 }.AsQueryable());
			var helper = new StagingAuthorizationHelperForTest(repo.Object);
			var auths = helper.GetAuthorizationDataForTest("User1", typeof(RefCusCodeList)).OrderBy(x => x.UA_DataSetName).ToArray();

			repo.Verify(x => x.GetLatest<UserAuthorization>(), Times.Once);
			Assert.AreEqual(2, auths.Length);
			Assert.AreEqual("All DataSets", auths[0].UA_DataSetName);
			Assert.AreEqual("*", auths[0].UA_TableName);
			Assert.AreEqual("CodeList1", auths[1].UA_DataSetName);
			Assert.AreEqual("RefCusCodeList", auths[1].UA_TableName);
			Assert.AreEqual("ZZD_CodeType", auths[1].UA_ColumnName);
			Assert.AreEqual("TST", auths[1].UA_ColumnValue);
		}
	}

	class StagingAuthorizationHelperForTest : StagingAuthorizationHelper
	{
		public StagingAuthorizationHelperForTest(ISafeRepository repository) : base(repository)
		{
		}

		public IEnumerable<RefUserAuthorization> GetAuthorizationDataForTest(string user, Type type)
		{
			return base.GetAuthorizationData(user, type);
		}
	}
}
