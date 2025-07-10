using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESMessageNumberFountainTest : TestCaseWithFactory
	{
		public void TestGetNext()
		{
			DbConnection connection = ((IDbConnected)Factory).Connection;
			try
			{
				connection.BeginTransaction();
				ZString licenceCompanyCode = GlbCompany.CurrentCompany.GC_Code;
				ZString aes = AESTIREDIMessage.MessageTypes.AESDirect;
				string expectedNumber1 = Env.NumberFountains.EDIFACTNumberFountain("M", licenceCompanyCode, aes).PeekPreliminaryFormatted(Factory);
				string expectedNumber2 = Env.NumberFountains.EDIFACTNumberFountain("M", aes, licenceCompanyCode).PeekPreliminaryFormatted(Factory);
				AssertEquals(expectedNumber1, AESMessageNumberFountain.GetNext(Factory, true, GlbCompany.CurrentCompany));
				AssertEquals(expectedNumber2, AESMessageNumberFountain.GetNext(Factory, false, GlbCompany.CurrentCompany));
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}
	}
}
