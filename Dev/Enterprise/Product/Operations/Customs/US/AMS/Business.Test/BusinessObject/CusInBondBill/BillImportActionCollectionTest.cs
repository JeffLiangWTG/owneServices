using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(BillImportActionCollection))]
	class BillImportActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BillImportActionCollection>
	{
		protected override BillImportActionCollection GetCollectionToTest()
		{
			return new BillImportActionCollection(Header.Bills);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BillImportAction(Header.Bills.AddNew());
		}

		CusInBondHeader Header
		{
			get
			{
				return fHeader ?? (fHeader = Factory.New<CusInBondHeader>());
			}
		}

		CusInBondHeader fHeader;
	}
}
