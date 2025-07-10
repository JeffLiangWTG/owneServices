using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	public sealed class TWDataObjectReaderHelperTest : Enterprise.UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		[ExpectNoExceptions]
		public void TestTaiwanGetCustomsBillType()
		{
			var helper = new TWDataObjectReaderHelper(Factory, Enterprise.Core.Constants.CountryCodes.Taiwan);
			NUnit.Framework.Assert.That(helper.GetCustomsBillType(new WayBillType { Code = "CNN" }), Is.EqualTo("CN").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(helper.GetCustomsBillType(new WayBillType { Code = "HWB" }), Is.EqualTo("HB").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(helper.GetCustomsBillType(new WayBillType { Code = "MWB" }), Is.EqualTo("MB").Using(CustomComparers.TypeComparison));
		}
	}
}
