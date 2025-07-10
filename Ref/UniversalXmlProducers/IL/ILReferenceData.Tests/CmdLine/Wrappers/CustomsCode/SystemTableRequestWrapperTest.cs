using System;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.CmdLine.Wrappers
{
	[TestFixture]
	sealed class SYSTBL_NG_9000_MSG_SystemTableRequestWrapperTest
	{
		[Test]
		public void TestRequestContentHeader()
		{
			Assert.IsNotNull(wrapper.RequestContentHeader);
		}

		[Test]
		public void TestSelectOptions()
		{
			Assert.IsNotNull(wrapper.SelectOptions);
			Assert.IsInstanceOf<SYSTBL_NG_9000_MSG_SystemTableRequestSelectOptionsWrapper>(wrapper.SelectOptions);
			Assert.That(wrapper.SelectOptions.GetAsDataTable, Is.True);
			Assert.That(wrapper.SelectOptions.PageNumber, Is.EqualTo(1));
			Assert.That(wrapper.SelectOptions.PageSize, Is.EqualTo(999));
		}

		[Test]
		public void TestTableName()
		{
			Assert.AreEqual("2012", wrapper.TableName);
		}

		[SetUp]
		public void SetUp()
		{
			wrapper = new SYSTBL_NG_9000_MSG_SystemTableRequestWrapper("2012", new DateTime(2024, 06, 04));
		}

		ISYSTBL_NG_9000_MSG_SystemTableRequest wrapper;
	}
}
