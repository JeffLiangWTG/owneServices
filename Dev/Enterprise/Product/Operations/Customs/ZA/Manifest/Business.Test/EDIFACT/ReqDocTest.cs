using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT.Testing
{
	class ReqDocTest : TestCaseWithFactory
	{
		[TestDate(2016, 07, 19)]
		public void TestREQDOCProvider()
		{
			var message = Factory.New<CUSCAREDIMessage>();
			message.EM_MessageText = @"UNH+360+CUSCAR:D:16A:UN:RCG001'BGM+85:::ALM+4702A6AD067441149AAF7809692B4C2C+9'RFF+LO:MAN0000014'NAD+MS+12342342'NAD+DEG'TDT+20++++:172:20'LOC+60'CNI+1+1234345:BOL:123434'RFF+BM:11111'LOC+8'LOC+9'GID+1+0'FTX+AAA++9'MEA+AAE+AAB+KGM:0'PCI+24'UNT+16+360'";
			var reqdocDataProvider = new ReqDoc(message, null) as IREQDOCMessageDataProvider;
			CombineAssertions(() =>
			{
				AssertEquals("MessageType", DocumentNameCodeList.CargoManifest.ToString(), reqdocDataProvider.MessageType);
				AssertEquals("LocalReferenceNumber", "4702A6AD067441149AAF7809692B4C2C", reqdocDataProvider.LocalReferenceNumber);
				AssertEquals("SenderReference", ZString.Empty, reqdocDataProvider.SenderReference);
				AssertEquals("MessageFunction", Universal.MessageFunctionCodeList.Codes.Original, reqdocDataProvider.MessageFunction);
				AssertEquals("MessageTypeForDoc", DocumentNameCodeList.MasterBillOfLading.ToString(), reqdocDataProvider.MessageTypeForDoc);
				AssertEquals("ManifestDocumentType", "ALM", reqdocDataProvider.ManifestDocumentType);
				AssertEquals("FinancialAccountNumber", ZString.Empty, reqdocDataProvider.FinancialAccountNumber);
				AssertEquals("FinalMRN", "1234345", reqdocDataProvider.FinalMRN);
				AssertEquals("DocumentMessageSource", ZString.Empty, reqdocDataProvider.DocumentMessageSource);
				AssertEquals("RequestDate", ZDateTime.Today, reqdocDataProvider.RequestDate);
				AssertEquals("StartDate", ZDateTime.Invalid, reqdocDataProvider.StartDate);
				AssertEquals("EndDate", ZDateTime.Invalid, reqdocDataProvider.EndDate);
				AssertEquals("MessageSender", "12342342", reqdocDataProvider.MessageSender);
				AssertEquals("ReleaseAuthority", ZString.Empty, reqdocDataProvider.ReleaseAuthority);
				AssertSame("Branch", GlbBranch.CurrentBranch, reqdocDataProvider.Branch);
			});

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			reqdocDataProvider = new ReqDoc(message, header);
			AssertEquals("Branch from default header", Env.CurrentBranchPK, reqdocDataProvider.Branch.PK);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			AssertNotEquals("Pre-requisite: new branch should not have same PK as current branch", Env.CurrentBranchPK, branch.PK);

			header.AMA_GB = branch.PK;
			reqdocDataProvider = new ReqDoc(message, header);
			AssertEquals("Branch from header with different branch", branch.PK, reqdocDataProvider.Branch.PK);
		}
	}
}
