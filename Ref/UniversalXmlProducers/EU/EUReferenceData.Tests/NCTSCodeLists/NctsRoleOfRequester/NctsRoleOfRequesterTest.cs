using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsRoleOfRequesterTest : NctsCodeListDetailsAbstractTest
	{
		[Test]
		public void CodeType()
		{
			Assert.That(nctsRoleOfRequester.CodeType, Is.EqualTo(Constants.ZZRefCusCodeList.NctsRoleOfRequesterCodeType));
		}

		[Test]
		public void CodeListType()
		{
			Assert.That(nctsRoleOfRequester.CodeListType, Is.EqualTo(Constants.UccCodeListTypes.RoleOfRequester));
		}

		[Test]
		public void DataSource()
		{
			Assert.That(nctsRoleOfRequester.DataSource, Is.EqualTo(Constants.UccDataSources.RoleOfRequester));
		}

		[SetUp]
		public void Setup()
		{
			nctsRoleOfRequester = GetNctsCodeListDetails() as NctsRoleOfRequesterType;
		}

		NctsRoleOfRequesterType nctsRoleOfRequester;

		protected override IUCCExportCodeListDetail GetNctsCodeListDetails() => new NctsRoleOfRequesterType();
	}
}
