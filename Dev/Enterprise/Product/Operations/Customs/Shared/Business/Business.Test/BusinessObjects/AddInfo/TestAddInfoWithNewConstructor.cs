using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TestAddInfoWithNewConstructor : AutoTestAddInfo
	{
		public TestAddInfoWithNewConstructor(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			Parent = addInfoProperty.BizObj;
			AddInfoProperty = addInfoProperty;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				LoadPropertiesFromString((ZString)addInfoProperty.Value);
			}
			isInitialised = true;
		}
	}
}
