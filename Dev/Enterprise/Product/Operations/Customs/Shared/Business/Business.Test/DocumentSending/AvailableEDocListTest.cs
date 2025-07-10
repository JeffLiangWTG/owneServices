using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business.Testing
{
	public class AvailableEDocListTest : TestCaseWithFactory
	{
		protected virtual List<ZString> ExtensionFilterForTest => new List<ZString> { };
		protected BusinessObjectFactory GetBizOFactory()
		{
			var provider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var fact = new BusinessObjectFactory();
			var docFactory = (BusinessObjectFactory)provider.GetFactory(fact);

			return docFactory;
		}

		public virtual void TestList()
		{
			var list = GetAvailableEdocsList(GetBizOFactory());

			var completeList = new ZStringBuilder();
			foreach (ICodeDescription data in list)
			{
				completeList.Append(data.Code + " | " + data.Description);
				AssertNotEquals("element.PK", ZGuid.Empty, data.PK);
				Assert(list.AvailableList.Any(x => x.UniqueKey == Guid.Parse(data.PK.ToString())));
			}

			AssertEquals(list.Count, list.AvailableList.Count);

			const string expectedCompleteList = @"
Declaration - AAA-File1.PDF | Added: 13-Dec-08 - Landed Fish
Declaration - BBB-File2.XLS | Added: 14-Dec-08 - XLS eDoc
Declaration - DDD-File4.PDF | Added: 16-Dec-08 - Crab Sticks
Declaration - EEE-File5.TIF | Added: 17-Dec-08 - TIF eDoc
Declaration - FFF-File6.BMP | Added: 18-Dec-08 - BMP eDoc
Declaration - GGG-File7.PNG | Added: 19-Dec-08 - PNG eDoc
Declaration - HHH-File8.JPG | Added: 20-Dec-08 - JPG eDoc
";
			AssertMultilineASCIIEquals("Complete List", expectedCompleteList.Trim(), completeList.ToStringWithNewLineBetweenAppends());
		}

		public virtual void TestListFromMultipleCollection()
		{
			var list = GetMultipleAvailableEdocsList(GetBizOFactory());

			var completeList = new ZStringBuilder();
			foreach (ICodeDescription data in list)
			{
				completeList.Append(data.Code + " | " + data.Description);
				AssertNotEquals("element.PK", ZGuid.Empty, data.PK);
			}

			const string expectedCompleteList = @"
Declaration - AAA-File1.PDF | Added: 13-Dec-08 - Landed Fish
Declaration - BBB-File2.XLS | Added: 14-Dec-08 - XLS eDoc
Declaration - DDD-File4.PDF | Added: 16-Dec-08 - Crab Sticks
Declaration - EEE-File5.TIF | Added: 17-Dec-08 - TIF eDoc
Declaration - FFF-File6.BMP | Added: 18-Dec-08 - BMP eDoc
Declaration - GGG-File7.PNG | Added: 19-Dec-08 - PNG eDoc
Declaration - HHH-File8.JPG | Added: 20-Dec-08 - JPG eDoc
Shipment - AAA-File21.PDF | Added: 13-Dec-08 - Landed Fish
Shipment - BBB-File22.XLS | Added: 14-Dec-08 - XLS eDoc
Shipment - DDD-File24.PDF | Added: 16-Dec-08 - Crab Sticks
Shipment - EEE-File25.TIF | Added: 17-Dec-08 - TIF eDoc
Shipment - FFF-File26.BMP | Added: 18-Dec-08 - BMP eDoc
Shipment - GGG-File27.PNG | Added: 19-Dec-08 - PNG eDoc
Shipment - HHH-File28.JPG | Added: 20-Dec-08 - JPG eDoc
";
			AssertMultilineASCIIEquals("Complete List", expectedCompleteList.Trim(), completeList.ToStringWithNewLineBetweenAppends());
		}

		protected virtual AvailableEDocList GetAvailableEdocsList(BusinessObjectFactory bizOFactory)
		{
			return new AvailableEDocList(ExtensionFilterForTest, PopulateEdocsCollection(bizOFactory));
		}

		protected virtual AvailableEDocList GetMultipleAvailableEdocsList(BusinessObjectFactory bizOFactory)
		{
			return new AvailableEDocList(ExtensionFilterForTest, PopulateEdocsCollection(bizOFactory), PopulateMultipleEdocsCollection(bizOFactory));
		}

		protected virtual eDocsCollectionForTesting PopulateEdocsCollection(BusinessObjectFactory bizOFactory)
		{
			var dec = bizOFactory.New<BaseJobDeclaration>();
			var eDoc1 = new eDocForTesting("AAA", "File1.PDF", "Landed Fish", new ZDateTime(2008, 12, 13), dec, bizOFactory, "DEC");
			var eDoc2 = new eDocForTesting("BBB", "File2.XLS", "XLS eDoc", new ZDateTime(2008, 12, 14), dec, bizOFactory, "DEC");
			var eDoc3 = new eDocForTesting("CCC", "File3.PDF", "Chocolate I", new ZDateTime(2008, 12, 15), dec, bizOFactory, "DEC") { IsDeleted = true };
			var eDoc4 = new eDocForTesting("DDD", "File4.PDF", "Crab Sticks", new ZDateTime(2008, 12, 16), dec, bizOFactory, "DEC");
			var eDoc5 = new eDocForTesting("EEE", "File5.TIF", "TIF eDoc", new ZDateTime(2008, 12, 17), dec, bizOFactory, "DEC");
			var eDoc6 = new eDocForTesting("FFF", "File6.BMP", "BMP eDoc", new ZDateTime(2008, 12, 18), dec, bizOFactory, "DEC");
			var eDoc7 = new eDocForTesting("GGG", "File7.PNG", "PNG eDoc", new ZDateTime(2008, 12, 19), dec, bizOFactory, "DEC");
			var eDoc8 = new eDocForTesting("HHH", "File8.JPG", "JPG eDoc", new ZDateTime(2008, 12, 20), dec, bizOFactory, "DEC");
			var eDocs = new eDocsCollectionForTesting(eDoc1, eDoc2, eDoc3, eDoc4, eDoc5, eDoc6, eDoc7, eDoc8);

			return eDocs;
		}

		protected virtual eDocsCollectionForTesting PopulateMultipleEdocsCollection(BusinessObjectFactory bizOFactory)
		{
			var shipment = bizOFactory.New<ForwardingShipment>();
			var secondeDoc1 = new eDocForTesting("AAA", "File21.PDF", "Landed Fish", new ZDateTime(2008, 12, 13), shipment, bizOFactory, "SHP");
			var secondeDoc2 = new eDocForTesting("BBB", "File22.XLS", "XLS eDoc", new ZDateTime(2008, 12, 14), shipment, bizOFactory, "SHP");
			var secondeDoc3 = new eDocForTesting("CCC", "File23.PDF", "Chocolate I", new ZDateTime(2008, 12, 15), shipment, bizOFactory, "SHP") { IsDeleted = true };
			var secondeDoc4 = new eDocForTesting("DDD", "File24.PDF", "Crab Sticks", new ZDateTime(2008, 12, 16), shipment, bizOFactory, "SHP");
			var secondeDoc5 = new eDocForTesting("EEE", "File25.TIF", "TIF eDoc", new ZDateTime(2008, 12, 17), shipment, bizOFactory, "SHP");
			var secondeDoc6 = new eDocForTesting("FFF", "File26.BMP", "BMP eDoc", new ZDateTime(2008, 12, 18), shipment, bizOFactory, "SHP");
			var secondeDoc7 = new eDocForTesting("GGG", "File27.PNG", "PNG eDoc", new ZDateTime(2008, 12, 19), shipment, bizOFactory, "SHP");
			var secondeDoc8 = new eDocForTesting("HHH", "File28.JPG", "JPG eDoc", new ZDateTime(2008, 12, 20), shipment, bizOFactory, "SHP");
			var secondeDocs = new eDocsCollectionForTesting(secondeDoc1, secondeDoc2, secondeDoc3, secondeDoc4, secondeDoc5, secondeDoc6, secondeDoc7, secondeDoc8);

			return secondeDocs;
		}
	}
}
