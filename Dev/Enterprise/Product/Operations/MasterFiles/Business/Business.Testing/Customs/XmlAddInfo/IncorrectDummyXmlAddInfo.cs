using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IncorrectDummyXmlAddInfo : AutoTestXmlAddInfo
	{
		public IncorrectDummyXmlAddInfo(ZPropertyInfoString parentPropertyInfo) : base(parentPropertyInfo.BizObj.Factory)
		{
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			{
				using (SuspendSettingHasChanges())
				{
					Deserialise();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			Z3_String = "Dummy";
		}
	}
}
