#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business
{
	public class TestXmlAddInfo : AutoTestXmlAddInfo
	{
		public TestXmlAddInfo(DummyBusinessObject dummy)
			: base(dummy.Factory)
		{
			this.ParentPropertyInfo = (ZPropertyInfoString)dummy.Z0_DescriptionInfo;

			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}
	}
}
#endif
