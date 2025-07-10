using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business
{
	public class DataTransferCountryInfo : NonPersistentBusinessObject, IRefCountry
	{
		public DataTransferCountryInfo(ZString code, ZString desc)
		{
			RN_Code = code;
			RN_Desc = desc;
		}

		public ZString RN_Code { get; set; }
		public ZString RN_Desc { get; set; }
	}
}
