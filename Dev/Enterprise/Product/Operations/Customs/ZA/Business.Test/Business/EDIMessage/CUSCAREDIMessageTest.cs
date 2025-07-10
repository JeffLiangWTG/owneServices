using System;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CUSCAREDIMessage))]
	sealed class CUSCAREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMessageBranch()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "TST");
			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			company1.GC_Code = "DZA";
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "JNB";
			branch1.GB_BranchName = "Joburg Corporate Office";
			branch1.GB_OH_OrgProxy = orgHeader1.PK;
			Factory.Save();
			Env.Registry.ZACustoms.SetIsTestMode(branch1, true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var message = Factory.New<CUSCAREDIMessage>();
				AssertEquals(GlbBranch.CurrentBranch.PK, message.EM_GB);
				Assert(!message.EM_IsTestMessage);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				using (ZACustomsRegistry.Instance.DefaultBranchForManifestSubmission.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "JNB"))
				{
					var message = Factory.New<CUSCAREDIMessage>();
					AssertEquals(branch1.PK, message.EM_GB);
					Assert(message.EM_IsTestMessage);
				}
			}
		}

		public void TestLocalReferenceNumber()
		{
			var message = Factory.New<CUSCAREDIMessage>();
			message.EM_MessageText = "UNH+7616+CUSCAR:D:16A:UN:RCG001'BGM+85:::RFM+4A51B82278DA4B718435F6EFC75DB009+4'DTM+137:20180523:102'DTM+136:20180522:102'RFF+ACW:EACBF377B6804C00BA276C8B9D382844'RFF+LO:MAN0000315'NAD+RL+00655953'NAD+MS+00505655TST'NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2019041820160201:MHDSVRZAZAZA'TDT+20++3++00655953:172:20+++:::LIC                           'LOC+35+BW'LOC+36+ZA'LOC+17+KFN'DTM+132:20180522:102'GEI+5+24:71:ZZZ'GEI+5+DB:122:ZZZ'CNI+1+00655953ROADMAN1::::20180521'RFF+BM:THEONE:1'LOC+8+ZADUR'LOC+9+BWBBK'TDT+20'RFF+ABT:00505655KOM20180523007139'NAD+CN++DIGICORE ELECTRONICS (PTY) LTD:37 HILLCLIMB ROAD:DURBAN::3608'NAD+CZ++TIM EXPORT BLNS GROUP:1 FAR STREET'GID+1+5:BAG'FTX+AAA++9+DESC'MEA+AAE+AAB+KGM:5'PCI+24+GGG'UNT+29+7616'";
			AssertEquals("4A51B82278DA4B718435F6EFC75DB009", message.LocalReferenceNumber);
		}
	}
}
