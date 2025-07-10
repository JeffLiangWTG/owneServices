using CargoWise.EntityFramework;
using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDDataValueWrapperLookups : ZLookups
	{
		public CMDDataValueWrapperLookups(CMDDataValueWrapper parent)
			: base(parent)
		{
		}

		public CustomsEntryTypeList EntryTypeList
		{
			get { return Factory.GetCachedValue<CustomsEntryTypeList>(); }
		}
	}
}
