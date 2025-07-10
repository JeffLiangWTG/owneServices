using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NMFSDocumentCollection))]
	public class NMFSDocumentCollectionTest : CusCodeDataCollectionTest<NMFSDocument>
	{
		public void TestAllowAddNew()
		{
			var collection = NMFSLine.DocumentDetails;
			AssertEquals(typeof(NMFSDocumentCollection), collection.GetType());
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			AssertEquals(false, collection.AllowNew);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertEquals(true, collection.AllowNew);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			AssertEquals(true, collection.AllowNew);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			AssertEquals(false, collection.AllowNew);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			AssertEquals(false, collection.AllowNew);
		}

		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<NMFSDocument> GetCusCodeDataCollection()
		{
			return NMFSLine.DocumentDetails;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var document = Factory.New<NMFSDocument>();
			document.CY_Type = CusCodeDataTypeList.Codes.NMFSDocument;
			document.CY_ParentID = NMFSLine.PK;
			document.CY_ParentTableCode = USNMFSLineAddInfoSchema.Constants.Prefix;
			return document;
		}

		NMFSLine NMFSLine
		{
			get { return nmfsLine ?? (nmfsLine = Factory.New<NMFSLine>()); }
		}
		NMFSLine nmfsLine;
	}
}
