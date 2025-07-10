using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageProcessor.Testing
{
	[TestedType(typeof(GOVGIOMessageHelper))]
	sealed class GOVGIOMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocumentNumber()
		{
			var helper = (GOVGIOMessageHelper)GetNewBusinessObject();
			AssertEquals("55E8E3293886428DB5DE455857C358F8", helper.DocumentNumber);
		}
		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<GOVGIOEDIMessage>();
			testMessage.EM_MessageText = GOVGIOTestMessage.Replace("\r\n", "");
			return GOVGIOMessageHelper.New(testMessage);
		}

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
	}
}
