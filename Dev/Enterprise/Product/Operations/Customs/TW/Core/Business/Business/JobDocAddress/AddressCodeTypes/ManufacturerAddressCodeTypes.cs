using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ManufacturerAddressCodeTypes : BaseAddressCodeTypes
	{
		public ManufacturerAddressCodeTypes(TWJobDocAddress address) : base(address)
		{
		}

		protected override IEnumerable<string> GetIDCodeTypesCore()
		{
			if (Address.IsImport)
			{
				yield return OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			}
			else
			{
				yield return OrgCusCode.CodeTypes.VATCode;
				yield return OrgCusCode.CodeTypes.PassportID;
				yield return OrgCusCode.TaiwanCodeTypes.PID;
			}
		}

		protected override IEnumerable<string> GetFRICodeTypesCore()
		{
			yield return OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			yield return Constants.OrgCusCodeType.CustomCode;
		}
	}
}
