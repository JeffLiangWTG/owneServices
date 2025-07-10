using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSRESEDIMessagePrettier))]
sealed class CUSRESEDIMessagePrettierTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertArgumentExceptionThrown("message", () => _ = new CUSRESEDIMessagePrettier(null));
	}

	public void TestMakeHumanReadable()
	{
		ArrangeErrorCodes(
			($"{UniversalReferenceConstants.ErrorCodesPrefix}435", "description of 435!"),
			($"{UniversalReferenceConstants.ErrorCodesPrefix}437", "description of 437"),
			($"{UniversalReferenceConstants.ErrorCodesPrefix}540", "description of 540"),
			($"{UniversalReferenceConstants.ErrorCodesPrefix}559", "description of 559"));
		var message = Factory.New<CUSRESEDIMessage>();
		var prettier = message.Prettier;
		AssertType<CUSRESEDIMessagePrettier>("[PRE-CONDITION] Prettier strategy", prettier);
		message.EM_MessageText = new ZStringBuilder()
			.Append("UNH+901113030227+CUSRES:1:902:UN:NEP-I+123456789'")
			.Append("BGM++X08 22334450'")
			.Append("ERP+8'")
			.Append("ERC+437:111:NO1'")
			.Append("ERP+10:SHOULD SUPPORT ERP WITH MULTIPLE SEGMENTS'")
			.Append("ERC+559:111:NO1'")
			.Append("ERC+435:111:NO1'")
			.Append("ERP+10:SHOULD SUPPORT ERC WITH MULTIPLE ELEMENTS, BUT NOT YET'")
			.Append("ERC+540:111:NO1'")
			//.Append("ERC+540:111:NO1+437:111:NO1'") // TODO: fix EDIFACT impl ERCSegment should support multiple elements
			.Append("NAD+83+99-9999999AB:55:USC'")
			.Append("LOC+16:1303:34:USC'")
			.Append("RFF+50:2233445'")
			.Append("RFF+289::1'")
			.Append("MEA+43+7+LB:38'")
			.Append("GIS+3'")
			.Append("FTX+AAP+++420:HI!'")
			.Append("FTX+AAP+++555:THIS WO:RD IS SPL:IT IN TWO'")
			.Append("FTX+FOO+++666:ONLY PRINT AAP/CIP LINES'")
			.Append("FTX+CIP+++777:BYE =)'")
			.Append("UNT+20+901113030227'")
			.ToString();
		AssertMultilineASCIIEquals(new ZStringBuilder()
			.AppendLine("437 description of 437")
			.AppendLine("")
			.AppendLine("SHOULD SUPPORT ERP WITH MULTIPLE SEGMENTS")
			.AppendLine("559 description of 559")
			.AppendLine("435 description of 435!")
			.AppendLine("")
			.AppendLine("SHOULD SUPPORT ERC WITH MULTIPLE ELEMENTS, BUT NOT YET")
			.AppendLine("540 description of 540")
			//.AppendLine("437 description of 437")
			.AppendLine("")
			.AppendLine("420 HI!")
			.AppendLine("555 THIS WORD IS SPLIT IN TWO")
			.AppendLine("777 BYE =)")
			.ToString(), prettier!.MakeHumanReadable());
	}

	void ArrangeErrorCodes(params (string code, string description)[] codes)
	{
		var codeList = new CodeDescriptionPairList();
		foreach (var (code, description) in codes)
		{
			codeList.AddPair(code, description);
		}
		Factory.SetCachedValue<ICodeDescriptionPairList>("664C116C-3A5E-2DA7-25E1-6A064C09071D", codeList);
	}
}
