using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class TTBTOBProcessingCodeListTest : TestCaseWithFactory
	{
		public void TestIsQuantityRequired()
		{
			var list = new TTBTOBProcessingCodeList().ToArray().Select(x => x.Code).ToList();
			list.Add(ZString.Empty);
			list.Add("!@#");
			var validList = new[]
			{
				TTBTOBProcessingCodeList.Codes.T51,
				TTBTOBProcessingCodeList.Codes.T52,
				TTBTOBProcessingCodeList.Codes.T54,
				TTBTOBProcessingCodeList.Codes.T55
			};
			foreach (var processingCode in validList)
			{
				list.Remove(processingCode);
				AssertEquals(processingCode, true, TTBTOBProcessingCodeList.IsQuantityRequired(processingCode));
			}
			foreach (var processingCode in list)
			{
				AssertEquals(processingCode, false, TTBTOBProcessingCodeList.IsQuantityRequired(processingCode));
			}
		}
	}
}
