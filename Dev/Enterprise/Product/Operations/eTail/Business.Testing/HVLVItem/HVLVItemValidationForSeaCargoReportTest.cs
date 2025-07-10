using CargoWise.EntityFramework.Testing;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVItemValidationForSeaCargoReportTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHVI_ContainerNumber()
		{
			var item = Factory.New<HVLVItem>();
			using (item.MarkValidatingForSeaCargoReport())
			{
				item.Validation.ValidateHVI_ContainerNumber();
				AssertMandatoryValidationError(item.HVI_ContainerNumberInfo, true);

				item.HVI_ContainerNumber = "TestContainer";
				AssertMandatoryValidationError(item.HVI_ContainerNumberInfo, false);
			}
		}
	}
}
