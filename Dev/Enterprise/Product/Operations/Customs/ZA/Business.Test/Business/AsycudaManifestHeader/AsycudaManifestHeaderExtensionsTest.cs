using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AsycudaManifestHeaderExtensionsTest : TestCaseWithFactory
	{
		public void TestZaOrgProxyForManifestMessaging()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy, AsycudaManifestHeaderExtensions.ZaOrgProxyForManifestMessaging(Factory));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var company1 = Factory.New<GlbCompany>();
				company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
				company1.GC_Code = "DZA";
				var proxy1 = Factory.New<OrgHeader>();
				proxy1.OH_Code = "Proxy1";
				proxy1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "12345", Core.Constants.CountryCodes.SouthAfrica);
				var branch1 = Factory.New<GlbBranch>();
				branch1.GB_GC = company1.PK;
				branch1.GB_Code = "JNB";
				branch1.GB_BranchName = "Joburg Corporate Office";
				branch1.GB_OH_OrgProxy = proxy1.PK;
				Factory.Save();

				using (ZACustomsRegistry.Instance.DefaultBranchForManifestSubmission.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "JNB"))
				{
					AssertEquals(proxy1, AsycudaManifestHeaderExtensions.ZaOrgProxyForManifestMessaging(Factory));
				}
			}
		}

		public void TestGetLastAcceptedCOSTCOEDIMessage()
		{
			OutturnTestHelper.SetupZZ(Factory);

			var header = Factory.New<AsycudaManifestHeader>();
			var costco = Factory.New<COSTCOEDIMessage>();
			costco.EM_MessageNum = "123";
			costco.EM_MessageText = COSTCOTestMessage.Replace("\r\n", "");
			costco.EM_LinkUniqueID = header.PK;
			costco.EM_LinkTable = header.TablePrefix;
			var cusres_costco = Factory.New<CUSRESEDIMessage>();
			cusres_costco.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cusres_costco.EM_MessageText = CUSRESTestMessage_COSTCO.Replace("\r\n", "");
			cusres_costco.EM_LinkUniqueID = header.PK;
			cusres_costco.EM_LinkTable = header.TablePrefix;
			AssertSame(costco, header.GetLastAcceptedCOSTCOEDIMessage());
		}

		public void TestGetLastAcceptedGOVGIOEDIMessage()
		{
			OutturnTestHelper.SetupZZ(Factory);

			var header = Factory.New<AsycudaManifestHeader>();
			var govgio = Factory.New<GOVGIOEDIMessage>();
			govgio.EM_MessageNum = "123";
			govgio.EM_MessageText = GOVGIOTestMessage.Replace("\r\n", "");
			govgio.EM_LinkUniqueID = header.PK;
			govgio.EM_LinkTable = header.TablePrefix;
			var cusres_govgio = Factory.New<CUSRESEDIMessage>();
			cusres_govgio.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cusres_govgio.EM_MessageText = CUSRESTestMessage_GOVGIO.Replace("\r\n", "");
			cusres_govgio.EM_LinkUniqueID = header.PK;
			cusres_govgio.EM_LinkTable = header.TablePrefix;
			header.Messages.AddRange(govgio, cusres_govgio);
			AssertSame(govgio, header.GetLastAcceptedGOVGIOEDIMessage());
		}

		const string COSTCOTestMessage = @"UNH+316+COSTCO:D:16A:UN:RCG001'
BGM+788+B9C73560F3A54797909A498BE37010B5+9'
FTX+ADI'
TDT+20++++:172:20+++:103'
RFF+ACL'
LOC+11+:139:6+::ZZZ'
NAD+MS+::ZZZ'
NAD+RL+::ZZZ'
EQD+BB'
SEL+NO SEAL NO'
CNT+8:0'";

		const string GOVGIOTestMessage = @"UNH+449+GOVCBR:D:16A:UN:RCG001+GOVGIO'
BGM+655:::ADI+55E8E3293886428DB5DE455857C358F8+9'
LOC+11'
LOC+34'
NAD+TB'
IFD+1'
NAD+CA'
NAD+DC'
DOC+706'
TDT+20++4'
DTM+133::102'
QTY+264:0'
POC'
UNS+D'
HYN+3'
CNI+1'
RFF+ACD:OGM0000007'
DOC+704'
DOC+703+111'
EQD+CN'
SEQ++1'
SEL'
SEQ++1'
TDT+20'
DTM+6::203'
GDS+BB:ZZZ'
LIN+1'
DOC+914'
UNS+S'
UNT+30+449'";

		const string CUSRESTestMessage_COSTCO = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+B9C73560F3A54797909A498BE37010B5:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+8:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:123'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";

		const string CUSRESTestMessage_GOVGIO = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+55E8E3293886428DB5DE455857C358F8:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+8:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:123'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";
	}

	public static class OutturnTestHelper
	{
		public static void SetupZZ(BusinessObjectFactory factory, string countryCode = "ZA")
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "Customs Manifest Status");
			var list6 = helper.CreateCusCodeList(countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "6", "Rejected", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var list8 = helper.CreateCusCodeList(countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(list6.PK, "CustomsRejected", "true");
			helper.CreateCusCodeListAttribute(list6.PK, "IUpdateCustomsStatus", "");
			helper.CreateCusCodeListAttribute(list6.PK, "INotify", "");
			helper.CreateCusCodeListAttribute(list8.PK, "CustomsCleared", "true");
			helper.CreateCusCodeListAttribute(list8.PK, "INotify", "");
			helper.CreateCusCodeListAttribute(list8.PK, "IAllowCancel", "true");
			helper.CreateCusCodeListAttribute(list8.PK, "IUpdateCustomsStatus", "");
			factory.Save();
		}
	}
}
