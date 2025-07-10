using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CMSupplierAddressCodeTypes : BaseAddressCodeTypes
	{
		public CMSupplierAddressCodeTypes(TWJobDocAddress address) : base(address)
		{
		}

		protected override IEnumerable<string> GetIDCodeTypesCore() => new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID };
	}
}
