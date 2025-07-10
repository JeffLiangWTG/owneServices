using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business
{
	[ModuleID(ModuleId.TWSpecialCode)]
	public class TWSpecialCodeCollection : ZZRefCusCodeListWrapperCollection<TWSpecialCode>
	{
		public TWSpecialCodeCollection(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.Taiwan, Codes.SpecialCodesForExemptionOfControllingAgencies, ZDateTime.Now)
		{
		}
	}
}
