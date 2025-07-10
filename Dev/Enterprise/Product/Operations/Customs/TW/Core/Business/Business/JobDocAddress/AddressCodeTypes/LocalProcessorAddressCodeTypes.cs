using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class LocalProcessorAddressCodeTypes : BaseAddressCodeTypes
	{
		public LocalProcessorAddressCodeTypes(TWJobDocAddress address) : base(address)
		{
		}

		protected override IEnumerable<string> GetIDCodeTypesCore()
		{
			yield return OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			yield return OrgCusCode.CodeTypes.VATCode;
			yield return OrgCusCode.TaiwanCodeTypes.PID;
			yield return OrgCusCode.CodeTypes.PassportID;
			yield return Constants.OrgCusCodeType.CustomCode;
		}

		protected override IEnumerable<string> GetFRICodeTypesCore()
		{
			return Enumerable.Empty<string>();
		}
	}
}
