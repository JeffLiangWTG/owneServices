using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.SG.V4.Business
{
	[SystemDefinedValues]
	partial class AutoJobDeclaration
	{
		protected internal AddInfoJobDeclaration AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AddInfoJobDeclaration(JE_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AddInfoJobDeclaration fAddInfo;
	}
}
