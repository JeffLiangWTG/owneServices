using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MergeByListReflectionRetrieverTest : TestCaseWithFactory
	{
		public void TestGetMergeByListByCountryCode()
		{
			var retriever = new MergeByListReflectionRetriever() as IMergeByListReflectionRetriever;
			AssertNotNull(retriever);

			AssertEquals("", ((CodeDescriptionPairList)retriever.GetMergeByListByCountryCode("")).CodesAsString);
			AssertEquals("", ((CodeDescriptionPairList)retriever.GetMergeByListByCountryCode("CN")).CodesAsString);
			AssertEquals("", ((CodeDescriptionPairList)retriever.GetMergeByListByCountryCode("TW")).CodesAsString);
			NewCompanyAndBranch();

			AssertEquals("NON, TRD, PNO, TRF", ((CodeDescriptionPairList)retriever.GetMergeByListByCountryCode("TW")).CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, PNO, PNP", ((CodeDescriptionPairList)retriever.GetMergeByListByCountryCode("CN")).CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP, TRM", ((CodeDescriptionPairList)retriever.GetMergeByListByCountryCode("CA")).CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP", ((CodeDescriptionPairList)retriever.GetMergeByListByCountryCode("TR")).CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP", ((CodeDescriptionPairList)retriever.GetMergeByListByCountryCode("FR")).CodesAsString);
			AssertEquals("", ((CodeDescriptionPairList)retriever.GetMergeByListByCountryCode("XX")).CodesAsString);
		}

		public void TestGetMergeByListByCompanyCode()
		{
			var retriever = new MergeByListReflectionRetriever() as IMergeByListReflectionRetriever;
			AssertNotNull(retriever);

			AssertEquals("", ((CodeDescriptionPairList)retriever.GetMergeByListByCompanyCode("")).CodesAsString);
			AssertEquals("", ((CodeDescriptionPairList)retriever.GetMergeByListByCompanyCode("CNX")).CodesAsString);
			NewCompanyAndBranch();

			AssertEquals("NON, TRD, PNO, TRF", ((CodeDescriptionPairList)retriever.GetMergeByListByCompanyCode("TWX")).CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, PNO, PNP", ((CodeDescriptionPairList)retriever.GetMergeByListByCompanyCode("CNX")).CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP, TRM", ((CodeDescriptionPairList)retriever.GetMergeByListByCompanyCode("CAX")).CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP", ((CodeDescriptionPairList)retriever.GetMergeByListByCompanyCode("TRX")).CodesAsString);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP", ((CodeDescriptionPairList)retriever.GetMergeByListByCompanyCode("FRX")).CodesAsString);
		}

		void NewCompanyAndBranch(string countryCode, string code)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			company.GC_Code = code;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = code;
		}

		void NewCompanyAndBranch()
		{
			NewCompanyAndBranch("TW", "TWX");
			NewCompanyAndBranch("CN", "CNX");
			NewCompanyAndBranch("CA", "CAX");
			NewCompanyAndBranch("FR", "FRX");
			NewCompanyAndBranch("TR", "TRX");
			Factory.Save();
		}
	}
}
