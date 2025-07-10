using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using NUnit.Framework;

	[TestedType(typeof(MAFFile))]
	public class MAFFileTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZF_FileNameChangesExtensionToPDF()
		{
			var declaration = Factory.New<JobDeclaration>();

			var eDocPdf = declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Invoice.pdf", DocumentTypeList.Codes.Invoice);
			var eDocTif = declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ExporterDeclaration.tif", DocumentTypeList.Codes.ExporterDeclaration);
			var lengthyName = new ZStringBuilder();
			for (int i = 0; i < 129; i++)
			{
				lengthyName.Append("A");
			}
			lengthyName.Append(".pdf");
			var eDocLengthyNameFile = declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, lengthyName.ToString(), "XXX");

			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			var filePdf = mafMessaging.Files.AddNew().Data;
			filePdf.ZF_EDocsUniqueID = eDocPdf.UniqueKey;
			AssertEquals("file.ZF_FileName", "INVOICE.PDF", filePdf.ZF_FileName.ToUpper());

			var fileTif = mafMessaging.Files.AddNew().Data;
			fileTif.ZF_EDocsUniqueID = eDocTif.UniqueKey;
			AssertEquals("file.ZF_FileName", "EXPORTERDECLARATION.PDF", fileTif.ZF_FileName.ToUpper());

			var lengthyNameFile = mafMessaging.Files.AddNew().Data;
			lengthyNameFile.ZF_EDocsUniqueID = eDocLengthyNameFile.UniqueKey;
			AssertEquals(lengthyNameFile.ZF_FileNameInfo.MaxLength, lengthyNameFile.ZF_FileName.Length);
			AssertContains(".PDF", lengthyNameFile.ZF_FileName);
		}

		public void TestWhenEDocIsSetFilenameDefaults()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mafMessaging = TestDataBuilder.GetMAFMessaging(declaration);
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "LALALA.pdf", "AAA");
			var file = mafMessaging.Files.AddNew().Data;
			file.ZF_EDocsUniqueID = eDoc.UniqueKey;
			AssertEquals("file.ZF_FileName", "LALALA.PDF", file.ZF_FileName);
		}

		public void TestZF_DocumentTypeDescription()
		{
			var file = (MAFFile)GetNewBusinessObject();
			AssertEquals("file.ZF_DocumentTypeDescription when object created blank", "", file.ZF_DocumentTypeDescription);

			file.ZF_DocumentType = DocumentTypeList.Codes.AnimalImportPermitSingle;
			AssertEquals("file.ZF_DocumentTypeDescription", DocumentTypeList.Descriptions.AnimalImportPermitSingle, file.ZF_DocumentTypeDescription);

			file.ZF_DocumentType = DocumentTypeList.Codes.MicrobiologicalPermitMulti;
			AssertEquals("file.ZF_DocumentTypeDescription", DocumentTypeList.Descriptions.MicrobiologicalPermitMulti, file.ZF_DocumentTypeDescription);
		}

		//public void TestAllListAttributesAreEvaluable()
		//{
		//  BusinessObject bo = GetNewBusinessObject();
		//  Type type = bo.GetType();

		//  ZStringBuilder failures = new ZStringBuilder();

		//  foreach (PropertyInfo pi in type.GetProperties())
		//  {
		//    if (typeof(IZType).IsAssignableFrom(pi.PropertyType))
		//    {
		//      object[] listAttributes = pi.GetCustomAttributes(typeof(ListAttribute), false);
		//      foreach (ListAttribute listAttribute in listAttributes)
		//      {
		//        string attributeID = "[List(\"" + listAttribute.ListDataSourceMember + "\")] attribute on public " + pi.PropertyType.Name + " " + pi.Name + " -:- ";
		//        try
		//        {
		//         object list = MetaData.GetMetaData(bo, PropertyDescriptorCollectionWithMetaData.FromType(type)[pi.Name], MetaDataTypes.ListDataSource);
		//         if (list == null)
		//         {
		//           failures.Append(attributeID + "List was evaluted as [null].");
		//         }
		//         else if (!(list is IList))
		//         {
		//           failures.Append(attributeID + "List specified is not an IList.");
		//         }
		//        }
		//        catch (Exception ex)
		//        {
		//          failures.Append(attributeID + "Exception Thrown: " + ex.Message);
		//        }
		//      }
		//    }
		//  }

		//  Assert("Tried to evaluate all [List()] attributes and found the following problems:\r\n\r\n" + failures.ToStringWithNewLineBetweenAppends(), failures.IsEmpty);
		//}

		protected override BusinessObject GetNewBusinessObject()
		{
			var messagingBO = TestDataBuilder.GetMAFMessaging(Factory.New<JobDeclaration>());
			var list = new CusAddInfoCollection<MAFFile>(messagingBO);
			list.Load();
			return list.AddNew().Data;
		}
	}
}
