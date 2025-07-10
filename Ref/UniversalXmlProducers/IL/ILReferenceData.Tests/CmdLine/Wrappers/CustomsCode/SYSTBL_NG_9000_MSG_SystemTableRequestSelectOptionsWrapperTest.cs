using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.CmdLine.Wrappers
{
	[TestFixture]
	sealed class SYSTBL_NG_9000_MSG_SystemTableRequestSelectOptionsWrapperTest
	{
		[Test]
		public void TestGetAsDataTable()
		{
			var wrapper = new SYSTBL_NG_9000_MSG_SystemTableRequestSelectOptionsWrapper();
			Assert.IsTrue(wrapper.GetAsDataTable);
		}

		[Test]
		public void TestPageNumber()
		{
			var wrapper = new SYSTBL_NG_9000_MSG_SystemTableRequestSelectOptionsWrapper();
			Assert.AreEqual(1, wrapper.PageNumber);
		}

		[Test]
		public void TestPageSize()
		{
			var wrapper = new SYSTBL_NG_9000_MSG_SystemTableRequestSelectOptionsWrapper();
			Assert.AreEqual(999, wrapper.PageSize);
		}
	}
}
