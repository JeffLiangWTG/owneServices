using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class OrgCusAccountValidation : MasterFiles.Business.OrgCusAccountValidation
	{
		public OrgCusAccountValidation(MasterFiles.Business.OrgCusAccount cusAccount) : base(cusAccount)
		{
		}

		protected override void CheckCZ_Account()
		{
			base.CheckCZ_Account();
			if (!Parent.CZ_Account.IsEmpty)
			{
				ZQuery query = new ZQuery(OrgCusAccountSchema.CZ_OH, Parent.CZ_OH);
				query.AddToFilter(OrgCusAccountSchema.CZ_RN_NKCountryCode, Parent.CZ_RN_NKCountryCode);

				var fans = Parent.Factory.Load<MasterFiles.Business.OrgCusAccount>(query);

				if (fans != null && fans.Any(fan => fan.PK != Parent.PK && fan.CZ_Account == Parent.CZ_Account))
				{
					Parent.CZ_AccountInfo.AddError(Res.GetString("34E234A6-E73D-44AF-9C8E-6E8DC46A684F", "This number is duplicated. Only one occurrence of each FAN is allowed."));
				}
				else
				{
					var customsOfficeCodes = GetCustomsOfficeCodesByFAN(Parent.CZ_Account);
					if (fans != null && fans.Any(fan => fan.PK != Parent.PK && GetCustomsOfficeCodesByFAN(fan.CZ_Account).Any(officeCode => customsOfficeCodes.Contains(officeCode))))
					{
						Parent.CZ_AccountInfo.AddError(Res.GetString("D32296AA-E903-4C67-BF5C-D9E08A72A335", "Cannot have two (or more) FAN Numbers against an organization for the same Customs office."));
					}
				}
			}
		}
		static IEnumerable<ZString> GetCustomsOfficeCodesByFAN(ZString fan)
		{
			var fullMaps = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.Value.Cast<FinancialAccountNumberPortMap>().Where(map => !map.ImporterPays && !map.Cash);
			return fullMaps.Where(x => x.FinancialAccountNumber == fan).Select(x => x.CustomsOfficeCode);
		}

		protected override bool IsCZ_PasswordMandatory => false;
	}
}
