using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.DIS.Testing
{
	sealed class BondDataValueProviderTest : TestCaseWithFactory
	{
		public void TestBondDataValueProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondProducerAccNo = "123";
			declaration.US_BondAmount = 1500m;

			declaration.US_BondType2 = BondTypeList.Codes.ContinuousBond;
			declaration.US_ADDCVDSuretyCode = "890";

			var bondData = new BondDataValueProvider(declaration).BondData;
			AssertEquals(2, bondData.Count());

			var bondData1 = bondData.ElementAt(0);
			AssertEquals("123", bondData1.BondNumber);
			AssertEquals(1500m, bondData1.BondAmount);
			AssertEquals("", bondData1.SuretyCode);
			AssertEquals(BondTypeList.Codes.SingleTransactionBond, bondData1.BondType);
			AssertEquals(MasterFiles.Business.DIS.BondNameType.Single, bondData1.BondName);
			AssertEquals("Bond Type: 9, Account No: 123, Bond Amount: 1500.00", bondData1.Description);

			var bondData2 = bondData.ElementAt(1);
			AssertEquals("", bondData2.BondNumber);
			AssertEquals(0m, bondData2.BondAmount);
			AssertEquals("890", bondData2.SuretyCode);
			AssertEquals(BondTypeList.Codes.ContinuousBond, bondData2.BondType);
			AssertEquals(MasterFiles.Business.DIS.BondNameType.Other, bondData2.BondName);
			AssertEquals("Bond Type: 8, Surety Code: 890", bondData2.Description);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.ReconDeclaration = new ReconDeclaration(declaration);
			bondData = new BondDataValueProvider(declaration).BondData;
			AssertEquals(0, bondData.Count());
		}
	}
}
