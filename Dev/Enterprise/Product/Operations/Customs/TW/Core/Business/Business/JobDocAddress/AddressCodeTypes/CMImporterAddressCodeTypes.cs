using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class CMImporterAddressCodeTypes : BaseAddressCodeTypes
	{
		public CMImporterAddressCodeTypes(TWJobDocAddress address) : base(address)
		{
		}

		protected override IEnumerable<string> GetIDCodeTypesCore() => new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID };
	}
}
