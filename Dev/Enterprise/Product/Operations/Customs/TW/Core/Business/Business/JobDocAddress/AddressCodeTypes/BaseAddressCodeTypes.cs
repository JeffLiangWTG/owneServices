using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class BaseAddressCodeTypes : IAddressCodeTypes
	{
		protected readonly TWJobDocAddress Address;

		public BaseAddressCodeTypes(TWJobDocAddress address)
		{
			Address = address;
		}

		public IEnumerable<string> IDCodeTypes => GetIDCodeTypesCore();

		protected virtual IEnumerable<string> GetIDCodeTypesCore()
		{
			yield return OrgCusCode.CodeTypes.VATCode;
			yield return OrgCusCode.CodeTypes.PassportID;
			yield return OrgCusCode.TaiwanCodeTypes.PID;
			yield return Constants.OrgCusCodeType.CustomCode;
		}

		public IEnumerable<string> AEOCodeTypes => GetAEOCodeTypesCore();

		protected virtual IEnumerable<string> GetAEOCodeTypesCore()
		{
			yield return OrgCusCode.TaiwanCodeTypes.AEO;
		}

		public IEnumerable<string> CBPCodeTypes => GetCBPCodeTypesCore();

		protected virtual IEnumerable<string> GetCBPCodeTypesCore()
		{
			yield return OrgCusCode.TaiwanCodeTypes.EPZ;
			yield return OrgCusCode.TaiwanCodeTypes.CBF;
			yield return OrgCusCode.TaiwanCodeTypes.FTZ;
			yield return OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark;
			yield return OrgCusCode.TaiwanCodeTypes.SciencePark;
		}

		public IEnumerable<string> TPCCodeTypes => GetTPCCodeTypesCore();

		protected virtual IEnumerable<string> GetTPCCodeTypesCore()
		{
			yield return OrgCusCode.TaiwanCodeTypes.TPC;
		}

		public IEnumerable<string> FRICodeTypes => GetFRICodeTypesCore();

		protected virtual IEnumerable<string> GetFRICodeTypesCore()
		{
			yield return OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
		}
	}
}
