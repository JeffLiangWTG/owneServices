using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public class DeclarationDataImporterTest : TestCaseWithFactory
	{
		public virtual void TestImport()
		{
			importer.Import();
			AssertEquals(false, importer.CancelImport);
		}

		[ExpectNoExceptions()]
		public void TestSave()
		{
			importer.Save();
		}

		public void TestPercentageComplete()
		{
			AssertEquals(90, importer.PercentageComplete);
		}

		protected BaseJobDeclaration declaration;
		DeclarationDataImporter importer;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetNewJobDeclaration();
			importer = new DeclarationDataImporterTestClass(declaration);
		}

		protected virtual BaseJobDeclaration GetNewJobDeclaration() => Factory.New<BaseJobDeclaration>();

		sealed class DeclarationDataImporterTestClass : DeclarationDataImporter
		{
			public DeclarationDataImporterTestClass(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			public override int TotalRecordCount => 10;

			public override int ProcessedRecordCount => 9;

			public override int FailedRecordCount => 2;
		}
	}
}
