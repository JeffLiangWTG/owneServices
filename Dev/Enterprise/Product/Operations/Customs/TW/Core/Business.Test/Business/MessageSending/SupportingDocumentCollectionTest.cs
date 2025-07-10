using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	sealed class SupportingDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SupportingDocumentCollection>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestStorageDocsCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			IeDoc GetEDoc(string fileName, string documentType)
			{
				var fullFileName = Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\{fileName}");
				return ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(fullFileName, documentType);
			}

			var doc1 = GetEDoc("sample.pdf", Core.Constants.FileFormats.PDF);
			var doc2 = GetEDoc("Test.xls", Core.Constants.FileFormats.XLS);
			var doc3 = GetEDoc("small.jpg", Core.Constants.FileFormats.JPG);
			GetEDoc("small.gif", Core.Constants.FileFormats.GIF);
			var filter = new List<ZString> { Core.Constants.FileFormats.PDF, Core.Constants.FileFormats.JPG, Core.Constants.FileFormats.GIF, Core.Constants.FileFormats.TIF, Core.Constants.FileFormats.TIFF };
			var supportingDocs = new AvailableEDocList(filter, new IStorageDocsBaseCollection[] { declaration.DocManagerInfo.AllEDocs });
			var supportingDocuments = new SupportingDocumentCollection(Factory, supportingDocs, new IStorageDocsBaseCollection[] { declaration.DocManagerInfo.AllEDocs });
			NUnit.Framework.Assert.That(supportingDocuments.StorageDocs[0].PK, NUnit.Framework.Is.EqualTo(doc1.UniqueKey).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(supportingDocuments.GetDocs().FirstIeDoc().UniqueKey, NUnit.Framework.Is.EqualTo(doc1.UniqueKey));
			NUnit.Framework.Assert.That(supportingDocuments.StorageDocs[doc2.UniqueKey], NUnit.Framework.Is.EqualTo(default(CargoWise.Integration.ICodeDescription)));
			NUnit.Framework.Assert.That(supportingDocuments.StorageDocs[doc3.UniqueKey], NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Integration.ICodeDescription)));
			NUnit.Framework.Assert.That(supportingDocuments.GetDocs().GetFromUniqueKey(doc3.UniqueKey.ToGuid()), NUnit.Framework.Is.EqualTo(doc3));
			NUnit.Framework.Assert.That(supportingDocuments.StorageDocs.Count, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(supportingDocuments.GetDocs().Sum(x => x.Count), NUnit.Framework.Is.EqualTo(4));
		}

		protected override SupportingDocumentCollection GetCollectionToTest() => new SupportingDocumentCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new SupportingDocument(Factory);
	}
}
