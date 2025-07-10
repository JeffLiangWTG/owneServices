using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.BusinessObjects.ZZRefCusCodeListCombined;

namespace Enterprise.Customs.Universal.Testing
{
	public class CusRefPackHelperTest : TestCaseWithFactory
	{
		public void TestMessageErrorIfNeeded()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var info = dummy.Z0_CalculatedInfo;
			using (var suspender = dummy.SuspendValidationTesting())
			{
				CusRefPackLoaderHelper.MessageErrorIfNeeded("ZA", "XXX", ZDateTime.BrettsBirthday, info, Factory);
				Assert(!info.HasMessageErrors());
				CusRefPackLoaderHelper.MessageErrorIfNeeded("US", "XXX", ZDateTime.BrettsBirthday, info, Factory);
				Assert(info.HasMessageError("Package type XXX does not map to a Customs package type for country US. Please add a mapping via Maintain > Customs > Customs Files > Packs Conversion."));
			}
		}
	}
}
