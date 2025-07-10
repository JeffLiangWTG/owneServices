using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ExportCustomsManifestLinesCollectionTest : TestCaseWithFactory
	{
		public void TestNextInCollectionTakesNextAvailableLineNumber()
		{
			ExportCustomsManifestLines line1 = collection.AddNew();
			AssertEquals("Line Number", (short)1, line1.EL_LineNo);

			ExportCustomsManifestLines line2 = collection.AddNew();
			AssertEquals("Line Number", (short)2, line2.EL_LineNo);

			ExportCustomsManifestLines line3 = collection.AddNew();
			AssertEquals("Line Number", (short)3, line3.EL_LineNo);

			collection.Remove(line2);

			ExportCustomsManifestLines line4 = collection.AddNew();
			AssertEquals("Line Number", (short)4, line4.EL_LineNo);
		}

		public void TestLinesSortedByLineNumber()
		{
			ExportCustomsManifestLines line1 = collection.AddNew();
			ExportCustomsManifestLines line2 = collection.AddNew();
			ExportCustomsManifestLines line3 = collection.AddNew();
			line1.EL_LineNo = 2;
			line2.EL_LineNo = 1;
			line3.EL_LineNo = 3;
			Factory.Save();
			collection.Load();
			AssertEquals(line2, collection[0]);
			AssertEquals(line1, collection[1]);
			AssertEquals(line3, collection[2]);
		}

		public void TestTypeOfCANChanged()
		{
			ExportCustomsManifestLines line1 = collection.AddNew();
			collection.TypeOfCANChanged += new EventHandler(Collection_TypeOfCANChanged);
			ExportCustomsManifestLines line2 = collection.AddNew();
			ExportCustomsManifestLines line3 = collection.AddNew();
			AssertEquals(0, typeOfCANChangedCount);
			line1.EL_TypeOfCAN = "123";
			AssertEquals(1, typeOfCANChangedCount);
			line3.EL_TypeOfCAN = "123";
			AssertEquals(2, typeOfCANChangedCount);
			collection.Remove(line2);
			line2.EL_TypeOfCAN = "123";
			AssertEquals(2, typeOfCANChangedCount);
		}

		public void TestTypeOfCANChangedForLoadedCollection()
		{
			ExportCustomsManifestLines line1 = collection.AddNew();
			ExportCustomsManifestLines line2 = collection.AddNew();
			ExportCustomsManifestLines line3 = collection.AddNew();
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();

			ExportCustomsManifestHeader secondHeader = secondFactory.Load<ExportCustomsManifestHeader>(header.PK);
			secondHeader.Lines.TypeOfCANChanged += new EventHandler(Collection_TypeOfCANChanged);

			AssertEquals(0, typeOfCANChangedCount);
			secondHeader.Lines[0].EL_TypeOfCAN = "123";
			AssertEquals(1, typeOfCANChangedCount);
		}

		public void TestTotalContainers()
		{
			ExportCustomsManifestLines line1 = collection.AddNew();
			ExportCustomsManifestLines line2 = collection.AddNew();
			ExportCustomsManifestLines line3 = collection.AddNew();
			line1.EL_NumberOfContainers = 1;
			line2.EL_NumberOfContainers = 2;
			line3.EL_NumberOfContainers = 4;
			AssertEquals("TotalContainers", 7, collection.TotalContainers);
		}

		public void TestTotalPackages()
		{
			ExportCustomsManifestLines line1 = collection.AddNew();
			ExportCustomsManifestLines line2 = collection.AddNew();
			ExportCustomsManifestLines line3 = collection.AddNew();
			line1.EL_NumberOfPackages = 3;
			line2.EL_NumberOfPackages = 6;
			line3.EL_NumberOfPackages = 9;
			AssertEquals("TotalPackages", 18, collection.TotalPackages);
		}

		#region Implementation

		void Collection_TypeOfCANChanged(object sender, EventArgs e)
		{
			typeOfCANChangedCount++;
		}

		int typeOfCANChangedCount;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			collection = header.Lines;
		}

		ExportCustomsManifestHeader header;

		ExportCustomsManifestLinesCollection collection;

		#endregion
	}
}
