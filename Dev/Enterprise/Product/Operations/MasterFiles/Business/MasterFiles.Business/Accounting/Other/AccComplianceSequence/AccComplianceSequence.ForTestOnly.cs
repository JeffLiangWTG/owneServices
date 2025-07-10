#if DEBUG

namespace Enterprise.MasterFiles.Business
{
	public partial class AccComplianceSequence
	{
		public bool XD_ExpiryDate_ReadOnly_ForTestOnly => XD_ExpiryDate_ReadOnly;

		public bool XD_StartDate_ReadOnly_ForTestOnly => XD_StartDate_ReadOnly;
	}
}

#endif
