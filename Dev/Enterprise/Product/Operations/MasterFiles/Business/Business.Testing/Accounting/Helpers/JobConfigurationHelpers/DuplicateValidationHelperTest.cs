using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers.Testing
{
	sealed class DuplicateValidationHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCheckDuplicatesWhenAllItemsDuplicated()
		{
			AssertCheckDuplicates(true, Times.Exactly(2));
		}

		[ExpectNoExceptions]
		public void TestCheckDuplicatesWhenNoItemsDuplicated()
		{
			AssertCheckDuplicates(false, Times.Never());
		}

		void AssertCheckDuplicates(bool isItemDuplicated, Times times)
		{
			var parentMock = new Mock<IDuplicateValidationCollectionProvider<DummyBusinessObject>>();
			var item1Mock = Factory.New<DummyBizoWithDuplicateValidationItem>();
			var item2Mock = Factory.New<DummyBizoWithDuplicateValidationItem>();
			var item3Mock = Factory.New<DummyBizoWithDuplicateValidationItem>();

			parentMock.Setup(x => x.GetDuplicateValidationCollection()).Returns(new[] { item1Mock, item2Mock, item3Mock });

			item1Mock.DuplicateValidationItemMock.Setup(x => x.IsDuplicated(It.IsAny<DummyBusinessObject>())).Returns(isItemDuplicated);
			item2Mock.DuplicateValidationItemMock.Setup(x => x.IsDuplicated(It.IsAny<DummyBusinessObject>())).Returns(isItemDuplicated);
			item3Mock.DuplicateValidationItemMock.Setup(x => x.IsDuplicated(It.IsAny<DummyBusinessObject>())).Returns(isItemDuplicated);

			var expectedMessage = "Test";
			IDuplicateValidationHelper helper = new DuplicateValidationHelper();
			helper.CheckDuplicates(parentMock.Object, expectedMessage);

			item1Mock.DuplicateValidationItemMock.Verify(x => x.RemoveRowError(expectedMessage), Times.Once);
			item2Mock.DuplicateValidationItemMock.Verify(x => x.RemoveRowError(expectedMessage), Times.Once);
			item3Mock.DuplicateValidationItemMock.Verify(x => x.RemoveRowError(expectedMessage), Times.Once);

			item1Mock.DuplicateValidationItemMock.Verify(x => x.IsDuplicated(item1Mock), Times.Never);
			item1Mock.DuplicateValidationItemMock.Verify(x => x.IsDuplicated(item2Mock), Times.Once);
			item1Mock.DuplicateValidationItemMock.Verify(x => x.IsDuplicated(item3Mock), Times.Once);

			item2Mock.DuplicateValidationItemMock.Verify(x => x.IsDuplicated(item1Mock), Times.Never);
			item2Mock.DuplicateValidationItemMock.Verify(x => x.IsDuplicated(item2Mock), Times.Never);
			item2Mock.DuplicateValidationItemMock.Verify(x => x.IsDuplicated(item3Mock), Times.Once);

			item3Mock.DuplicateValidationItemMock.Verify(x => x.IsDuplicated(item1Mock), Times.Never);
			item3Mock.DuplicateValidationItemMock.Verify(x => x.IsDuplicated(item2Mock), Times.Never);
			item3Mock.DuplicateValidationItemMock.Verify(x => x.IsDuplicated(item3Mock), Times.Never);

			item1Mock.DuplicateValidationItemMock.Verify(x => x.AddRowError(expectedMessage), times);
			item2Mock.DuplicateValidationItemMock.Verify(x => x.AddRowError(expectedMessage), times);
			item3Mock.DuplicateValidationItemMock.Verify(x => x.AddRowError(expectedMessage), times);
		}

		class DummyBizoWithDuplicateValidationItem : DummyBusinessObject, IDuplicateValidationItem<DummyBusinessObject>
		{
			public DummyBizoWithDuplicateValidationItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Mock<IDuplicateValidationItem<DummyBusinessObject>> DuplicateValidationItemMock = new Mock<IDuplicateValidationItem<DummyBusinessObject>>();

			void IDuplicateValidationItem<DummyBusinessObject>.AddRowError(string message)
			{
				DuplicateValidationItemMock.Object.AddRowError(message);
			}

			bool IDuplicateValidationItem<DummyBusinessObject>.IsDuplicated(DummyBusinessObject anotherItem)
			{
				return DuplicateValidationItemMock.Object.IsDuplicated(anotherItem);
			}

			void IDuplicateValidationItem<DummyBusinessObject>.RemoveRowError(string message)
			{
				DuplicateValidationItemMock.Object.RemoveRowError(message);
			}
		}
	}
}
