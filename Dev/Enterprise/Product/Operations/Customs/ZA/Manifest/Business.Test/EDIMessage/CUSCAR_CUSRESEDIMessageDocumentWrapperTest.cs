using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class CUSCAR_CUSRESEDIMessageDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var cusres = Factory.New<CUSRESEDIMessage>();
			AssertNotNull(CUSCAR_CUSRESEDIMessageDocumentWrapper.New(cusres, Factory));
		}

		public void TestCUSRESMessage()
		{
			var cusres = Factory.New<CUSRESEDIMessage>();
			AssertEquals(cusres, new CUSCAR_CUSRESEDIMessageDocumentWrapper(cusres, Factory).CUSRESMessage);
		}

		public void TestManifest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_LinkUniqueID = header.PK;
			cusres.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;
			header.Messages.Add(cusres);
			AssertEquals(header, new CUSCAR_CUSRESEDIMessageDocumentWrapper(cusres, Factory).Manifest);
			cusres.EM_LinkUniqueID = header.PK;
			cusres.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;
			AssertEquals(header, new CUSCAR_CUSRESEDIMessageDocumentWrapper(cusres, Factory).Manifest);
			cusres.EM_LinkUniqueID = bill.PK;
			cusres.EM_LinkTable = AsycudaBill.Schema.TableName;
			AssertEquals(header, new CUSCAR_CUSRESEDIMessageDocumentWrapper(cusres, Factory).Manifest);
		}

		public void TestCUSCARMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var cuscar = Factory.New<CUSCAREDIMessage>();
			cuscar.EM_MessageText = "UNH+6392+CUSCAR:D:16A:UN:RCG01'BGM+85:::ALM+AEF5840B836D4F71B342D6B93E0692A3+9'DTM+136:20180325:102'RFF+LO:MAN0000214'RFF+ACL:S1234'NAD+RL+MSC'NAD+FZ+00291323'NAD+MS+00505655TST'NAD+DEG+MSC++MSC AU ()'TDT+20+S1234+1++MSC:172:20+++3FSG8:103:::ZA'LOC+60+ZADUR'DTM+132:20180406:102'DTM+369:201803250000:203'EQD+CN+MSCU3214234+2000:102:5++3+5'SEL+SEAL1CONT1+CA'SEL+SEAL2CONT1+CA'SEL+SEAL3CONT1+CA'MEA+AAE+AAM+KGM:100'MEA+AAE+VGM+KGM:200'CNT+16:1'CNI+1+MASTERBILL:BOL'RFF+BM:HOUSE1'LOC+8+ZADUR'LOC+9+DEHAM'NAD+CN++GLENN CORP:3 NEW ROAD:MIDRAND::1682++3 NEW ROAD:ERAND AH:MIDRAND:1682  ZA'NAD+CZ++TIM EXPORT CO:1 BAYERN STREET:HAMBURG:HH:22765++1 BAYERN STREET:HAMBURG:HAMBURG:22765 HAMBURG (HANSESTADT) DE'GID+1+1:BAG'FTX+AAA++9+PACKDESC1'MEA+AAE+AAW+MTQ:11'MEA+AAE+AAB+KGM:100'SGP+MSCU3214234+1'PCI+24+PACKMARK1'UNT+33+6392'";
			header.Messages.Add(cuscar);
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_LinkUniqueID = header.PK;
			cusres.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;
			cusres.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+963+AEF5840B836D4F71B342D6B93E0692A3'DTM+178:20170715:102'TDT+20+V345+1'LOC+22+DUR::ZZZ'GIS+6:120:ZZZ'NAD+AG+00000000'RFF+BH:BILL1'RFF+AAS:MASTERBILLLXXX'DTM+137:20170718:102'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++CON GROSS MASS / WEIGHT where CCC=00053592,MCC=SAFM,TDN=BILL1,MTD=MAST:ERBILLLXXX,value=?'null?'?: Field is required: : : 'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++BOL CONSIGNOR NAME where CCC=00053592,MCC=SAFM,TDN=BILL1,MTD=MASTERBIL:LLXXX,value=?'null?'?: Field is required: : : 'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++BOL CONSIGNOR ADDRESS LINE - 1 where value=?'null?'?: Field is required: : : : 'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++BLL GROSS MASS / WEIGHT where CCC=00053592,MCC=SAFM,TDN=BILL1,MTD=MAST:ERBILLLXXX,value=?'null?'?: Field is required: : : 'UNT+23+1'";
			header.Messages.Add(cusres);
			AssertEquals(cuscar, new CUSCAR_CUSRESEDIMessageDocumentWrapper(cusres, Factory).CUSCARMessage);
		}
	}
}
