using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TrademarkEDocListTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestTrademarkEDocList()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PRO1";
			part1.OP_Desc = "product 1";
			var decDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			var shipmentDocManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
			var partDocManagerInfo = part1.DocManagerInfo();
			var doc1 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), MessageConstants.DocumentTypes.CAT);
			var doc2 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"), MessageConstants.DocumentTypes.TDM);
			var doc3 = partDocManagerInfo.AddFileOrDocument(doc2.ImageData, "sample1.pdf", MessageConstants.DocumentTypes.CAT);
			var doc4 = partDocManagerInfo.AddFileOrDocument(doc2.ImageData, "TestBitmap1.png", MessageConstants.DocumentTypes.TDM);
			var doc5 = shipmentDocManagerInfo.AddFileOrDocument(doc2.ImageData, "sample2.pdf", MessageConstants.DocumentTypes.CAT);
			var doc6 = shipmentDocManagerInfo.AddFileOrDocument(doc2.ImageData, "TestBitmap2.jpg", MessageConstants.DocumentTypes.TDM);
			Factory.Save();
			var trademarkEDocList = new TrademarkEDocList(new IStorageDocsBaseCollection[] { partDocManagerInfo.AllEDocs, shipmentDocManagerInfo.AllEDocs, decDocManagerInfo.AllEDocs, null, null });
			var trademarkEDocs = trademarkEDocList.Cast<ICodeDescription>();
			NUnit.Framework.Assert.That(trademarkEDocList.DefaultDocPk, NUnit.Framework.Is.EqualTo(doc4.UniqueKey));
			NUnit.Framework.Assert.That(trademarkEDocList.Count, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(!trademarkEDocs.Any(x => (ZGuid)x.PK == doc1.UniqueKey), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(trademarkEDocs.Any(x => (ZGuid)x.PK == doc2.UniqueKey), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!trademarkEDocs.Any(x => (ZGuid)x.PK == doc3.UniqueKey), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(trademarkEDocs.Any(x => (ZGuid)x.PK == doc4.UniqueKey), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(!trademarkEDocs.Any(x => (ZGuid)x.PK == doc5.UniqueKey), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(trademarkEDocs.Any(x => (ZGuid)x.PK == doc6.UniqueKey), NUnit.Framework.Is.True);
		}
	}
}
