using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class NMFSDocumentValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Data()
		{
			Document.CY_Data = ZString.Empty;
			AssertHasMessageErrorContaining(Document.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);

			Document.CY_Data = "ABC";
			AssertNoMessageErrorContaining(Document.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#region Implementation

		NMFSDocument Document
		{
			get { return fDocument ?? (fDocument = Factory.New<NMFSDocument>()); }
		}
		NMFSDocument fDocument;

		#endregion
	}
}
