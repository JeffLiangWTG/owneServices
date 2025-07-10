using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CorrectDummyXmlAddInfo : AutoTestXmlAddInfo
	{
		public CorrectDummyXmlAddInfo(ZPropertyInfoString parentPropertyInfo) : base(parentPropertyInfo.BizObj.Factory)
		{
			// This is simulating the way we do in OrgImpAddInfo. like EUOrgImpAddInfo, FROrgImpAddInfo, etc.
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			{
				using (SuspendSettingHasChanges())
				{
					Deserialise(false);
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
