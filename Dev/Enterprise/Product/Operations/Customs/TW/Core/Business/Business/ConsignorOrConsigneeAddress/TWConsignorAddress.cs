using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class TWConsignorAddress : TWConsignorOrConsigneeAddress
	{
		public TWConsignorAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void DefaultValueWhenAddressChanged()
		{
			var organisationPK = OrganisationPK;
			if (Parent.JE_OH_Exporter != organisationPK)
			{
				Parent.JE_OH_Exporter = organisationPK;
			}
			if (Address is OrgAddress address)
			{
				DefaultConsignorGovRegNum(address);
			}
		}

		protected override void DefaultValueWhenGovRegNumTypeChanged(ZString govRegNumType)
		{
			if (govRegNumType == Constants.CCPPrefix)
			{
				DefaultConsignorGovRegNumByCEI_Style();
			}
			else
			{
				E2_GovRegNum = Parent?.Exporter?.GetCustomsRegNo(govRegNumType) ?? ZString.Empty;
			}
		}

		void DefaultConsignorGovRegNum(OrgAddress address)
		{
			if (address.OA_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan && Parent.JE_OH_Exporter != Parent.JE_OH_Supplier && Parent.Exporter is OrgHeader exporter)
			{
				var consignorOrgCusCode = exporter.GetOrgCusCode(new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID });
				E2_GovRegNumType = consignorOrgCusCode?.OK_CodeType ?? ZString.Empty;
				E2_GovRegNum = consignorOrgCusCode?.OK_CustomsRegNo ?? ZString.Empty;
			}
			else if (address.OA_RN_NKCountryCode != Core.Constants.CountryCodes.Taiwan || Parent.JE_OH_Exporter == Parent.JE_OH_Supplier)
			{
				DefaultConsignorGovRegNumByCEI_Style(address);
			}
		}

		void DefaultConsignorGovRegNumByCEI_Style(OrgAddress address = null)
		{
			var cusEntryInstruction = Parent.CusEntryInstruction;
			switch (cusEntryInstruction.CEI_Style)
			{
				case Constants.DeclarationTypes.Export.D5:
				case Constants.DeclarationTypes.Import.D2:
				case Constants.DeclarationTypes.Import.D7:
					var fromWarehouseCode = cusEntryInstruction.FromWarehouseCode;
					E2_GovRegNumType = Constants.CCPPrefix;
					E2_GovRegNum = fromWarehouseCode.IsEmpty ? ZString.Empty : $"{Constants.CCPPrefix}{fromWarehouseCode}";
					break;
				case Constants.DeclarationTypes.Export.F4:
				case Constants.DeclarationTypes.Export.F5:
				case Constants.DeclarationTypes.Import.B6:
				case Constants.DeclarationTypes.Import.D8:
				case Constants.DeclarationTypes.Import.F2:
				case Constants.DeclarationTypes.Import.F3:
					var bondedID = Parent.SupplierDocumentaryAddress.CBPCode;
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
