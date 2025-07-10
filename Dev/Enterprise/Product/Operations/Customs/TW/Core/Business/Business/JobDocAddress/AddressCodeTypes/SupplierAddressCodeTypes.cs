using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class SupplierAddressCodeTypes : BaseAddressCodeTypes
	{
		public SupplierAddressCodeTypes(TWJobDocAddress address) : base(address)
		{
		}

		protected override IEnumerable<string> GetIDCodeTypesCore()
		{
			if (Address.IsExport)
			{
				return new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID };
			}
			else
			{
				return base.GetIDCodeTypesCore();
			}
		}
	}
}
