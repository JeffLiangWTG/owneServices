using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class TWConsigneeAddress : TWConsignorOrConsigneeAddress
	{
		public TWConsigneeAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void DefaultValueWhenAddressChanged()
		{
			var organisationPK = OrganisationPK;
			if (Parent.JE_OH_Consignee != organisationPK)
			{
				Parent.JE_OH_Consignee = organisationPK;
			}
			if (Address is OrgAddress address)
			{
				DefaultConsigneeGovRegNum(address);
			}
		}

		protected override void DefaultValueWhenGovRegNumTypeChanged(ZString govRegNumType)
		{
			if (E2_GovRegNumType == Constants.CCPPrefix)
			{
				DefaultConsigneeGovRegNumByCEI_Style();
			}
			else
			{
				E2_GovRegNum = Parent?.IntermConsignee?.GetCustomsRegNo(govRegNumType) ?? ZString.Empty;
			}
		}

		void DefaultConsigneeGovRegNum(OrgAddress address)
		{
			if (address.OA_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan && Parent.JE_OH_Consignee != Parent.JE_OH_Importer && Parent.IntermConsignee is OrgHeader consignee)
			{
				var consigneeOrgCusCode = consignee.GetOrgCusCode(new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID });
				E2_GovRegNumType = consigneeOrgCusCode?.OK_CodeType ?? ZString.Empty;
				E2_GovRegNum = consigneeOrgCusCode?.OK_CustomsRegNo ?? ZString.Empty;
			}
			else if (address.OA_RN_NKCountryCode != Core.Constants.CountryCodes.Taiwan || Parent.JE_OH_Consignee == Parent.JE_OH_Importer)
			{
				DefaultConsigneeGovRegNumByCEI_Style(address);
			}
		}

		void DefaultConsigneeGovRegNumByCEI_Style(OrgAddress address = null)
		{
			var cusEntryInstruction = Parent.CusEntryInstruction;
			switch (cusEntryInstruction.CEI_Style)
			{
				case Constants.DeclarationTypes.Export.B2:
				case Constants.DeclarationTypes.Import.D7:
					var bondedID = Parent.ImporterDocumentaryAddress.CBPCode;
					var govRegNum = bondedID.IsEmpty ? cusEntryInstruction.ToWarehouseCode : bondedID;
					E2_GovRegNumType = Constants.CCPPrefix;
					E2_GovRegNum = govRegNum.IsEmpty ? ZString.Empty : $"{Constants.CCPPrefix}{govRegNum}";
					break;
				case Constants.DeclarationTypes.Export.D1:
				case Constants.DeclarationTypes.Import.D8:
					govRegNum = cusEntryInstruction.ToWarehouseCode;
					E2_GovRegNumType = Constants.CCPPrefix;
					E2_GovRegNum = govRegNum.IsEmpty ? ZString.Empty : $"{Constants.CCPPrefix}{govRegNum}";
					break;
				case Constants.DeclarationTypes.Export.B8:
				case Constants.DeclarationTypes.Export.B9:
				case Constants.DeclarationTypes.Export.D5:
				case Constants.DeclarationTypes.Export.F4:
				case Constants.DeclarationTypes.Import.F1:
				case Constants.DeclarationTypes.Import.F2:
					bondedID = Parent.ImporterDocumentaryAddress.CBPCode;
					E2_GovRegNumType = Constants.CCPPrefix;
					E2_GovRegNum = bondedID.IsEmpty ? ZString.Empty : $"{Constants.CCPPrefix}{bondedID}";
					break;
			}

			if (E2_GovRegNum.IsEmpty && address != null)
			{
				DefaultValueFromCompanyName(address);
			}
		}
	}
}
