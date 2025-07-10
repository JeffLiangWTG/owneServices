using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(ExportAWBAccountingInformationCollection))]
	sealed class ExportAWBAccountingInformationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSequence()
		{
			var collection = ((ExportAWBAccountingInformationCollection)Collection);
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();

			AssertEquals((ZByte)0, collection[0].EA_Sequence);
			AssertEquals((ZByte)1, collection[1].EA_Sequence);
			AssertEquals((ZByte)2, collection[2].EA_Sequence);
			AssertEquals((ZByte)3, collection[3].EA_Sequence);

			collection[1].EA_Sequence = 5;
			collection.AddNew().EA_InformationID = "AAE";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var master2 = factory.Load<ExportAWBHeader>(AWBHeader.PK);
			var collection2 = new ExportAWBAccountingInformationCollection(master2, factory);
			collection2.Load();

			AssertEquals((ZByte)0, collection2[0].EA_Sequence);
			AssertEquals((ZByte)2, collection2[1].EA_Sequence);
			AssertEquals((ZByte)3, collection2[2].EA_Sequence);
			AssertEquals((ZByte)5, collection2[3].EA_Sequence);
			AssertEquals((ZByte)6, collection2[4].EA_Sequence);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ExportAWBAccountingInformationCollection(AWBHeader, Factory);
		}

		ExportAWBHeader _awbHeader;
		ExportAWBHeader AWBHeader
		{
			get { return _awbHeader ?? (_awbHeader = Factory.New<ExportAWBHeader>()); }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ExportAWBAccountingInformation>();
		}

		#endregion
	}
}
