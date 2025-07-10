using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.TW;

namespace Enterprise.Customs.TW.Business
{
	public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, IOrgCusCodeValidation
	{
		readonly OrgCusCode parent;
		public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
		{
			this.parent = (OrgCusCode)parent;
		}

		protected override void CheckOK_OA_PremisesAddress()
		{
			base.CheckOK_OA_PremisesAddress();

			if (parent.PremisesAddressIsAllowed || parent.OK_OA_PremisesAddress.IsEmpty)
			{
				var targetInfo = parent.OK_OA_PremisesAddressInfo;
				var codeType = parent.OK_CodeType;
				if (parent.OK_OA_PremisesAddress.IsEmpty)
				{
					if (codeType == OrgCusCode.CodeTypes.VATCode)
					{
						targetInfo.AddErrorIfEnforced(Res.GetString("5614EBF2-2F40-465A-9B73-F34E301753D0", "Taiwan VAT (Government VAT Code) should have a Premises Address specified."), OrgCusCode.CodeTypes.VATCode);
					}
				}
				else if (NoDuplicateAddressCodes.Contains(codeType))
				{
					if (parent.Header.CustomsCodes.Cast<OrgCusCode>().Any(x => x != parent && NoDuplicateAddressCodes.Contains(x.OK_CodeType) && x.OK_OA_PremisesAddress == parent.OK_OA_PremisesAddress))
					{
						targetInfo.AddError(Res.GetString("C8FA0EC6-163A-46DC-B385-C592427F5D88", "One address can only have one bonded premise code."));
					}
				}
				else if (codeType == OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber)
				{
					if (parent.Header.CustomsCodes.Cast<OrgCusCode>().Any(x => x != parent && x.OK_CodeType == OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber && x.OK_OA_PremisesAddress == parent.OK_OA_PremisesAddress))
					{
						targetInfo.AddError(Res.GetString("B8F45E45-74DB-4E33-A65B-AE689135AA67", "One address can only have one factory premise code."));
					}
				}
			}
		}

		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();
			var parent = this.parent;
			ValidateCustomsCode(new CustomsRegistrationNumberValidation(parent), parent.OK_RN_NKCodeCountry, parent.OK_CodeType, parent.OK_CustomsRegNo, parent.OK_CustomsRegNoInfo);
		}

		ICollection<ZString> NoDuplicateAddressCodes
		{
			get
			{
				if (noDuplicateAddressCodes == null)
				{
					noDuplicateAddressCodes = new Collection<ZString>
					{
						OrgCusCode.TaiwanCodeTypes.EPZ,
						OrgCusCode.TaiwanCodeTypes.FTZ,
						OrgCusCode.TaiwanCodeTypes.CBF,
						OrgCusCode.CodeTypes.WarehouseControlledPremisesID,
						OrgCusCode.CodeTypes.ControlledPremisesID
					};
				}
				return noDuplicateAddressCodes;
			}
		}
		Collection<ZString> noDuplicateAddressCodes;

		#region Validate CustomsCode
		public static void ValidateCustomsCode(CustomsRegistrationNumberValidation customsRegistrationNumberValidation, ZString codeCountry, ZString codeType, ZString customsRegNo, ZPropertyInfo customsRegNoInfo)
		{
			if (codeCountry == Core.Constants.CountryCodes.Taiwan)
			{
				switch (codeType)
				{
					case OrgCusCode.CodeTypes.VATCode:
						customsRegistrationNumberValidation.CheckVATCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.CodeTypes.PassportID:
						customsRegistrationNumberValidation.CheckPASCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.TPC:
						customsRegistrationNumberValidation.CheckTPCCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.PID:
						customsRegistrationNumberValidation.CheckPIDCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.PBR:
						customsRegistrationNumberValidation.CheckPBRCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.CodeTypes.TaxFileCode:
						customsRegistrationNumberValidation.CheckGTXCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.PIG:
						customsRegistrationNumberValidation.CheckPIGCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.MCI:
						customsRegistrationNumberValidation.CheckMCICustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.EPZ:
						customsRegistrationNumberValidation.CheckEPZCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.CBF:
						customsRegistrationNumberValidation.CheckCBFCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.FTZ:
						customsRegistrationNumberValidation.CheckFTZCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.CodeTypes.WarehouseControlledPremisesID:
						customsRegistrationNumberValidation.CheckCPWCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.CodeTypes.ControlledPremisesID:
						customsRegistrationNumberValidation.CheckCCPCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.CodeTypes.FDAEstablishmentIdentifier:
						customsRegistrationNumberValidation.CheckFEICustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber:
						customsRegistrationNumberValidation.CheckFRICustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark:
						customsRegistrationNumberValidation.CheckATPCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.TaiwanCodeTypes.SciencePark:
						customsRegistrationNumberValidation.CheckSPKCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					case OrgCusCode.CodeTypes.CarrierCode:
						customsRegistrationNumberValidation.CheckCCCCCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
						break;
					default:
						break;
				}
			}

			if (codeType == OrgCusCode.TaiwanCodeTypes.AEO)
			{
				customsRegistrationNumberValidation.CheckAEOCustomsRegistrationNumber(customsRegNoInfo, customsRegNo, codeCountry);
			}
		}
		#endregion

		protected override HashSet<string>[] GetCodesCannotCoexist()
		{
			return new[] { new HashSet<string>() { OrgCusCode.TaiwanCodeTypes.MCI, OrgCusCode.TaiwanCodeTypes.PIG } };
		}
	}
}
