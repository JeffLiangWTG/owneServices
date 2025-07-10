using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	public class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CusEntryLineFee result = (CusEntryLineFee)base.GetNewBusinessObject();
			result.CF_ChargeAmount = 10m;
			result.EntryLine.Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			return result;
		}
	}
}
