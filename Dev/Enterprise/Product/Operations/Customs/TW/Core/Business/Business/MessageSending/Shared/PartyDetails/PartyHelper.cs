using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business
{
	static class PartyHelper
	{
		internal static IPartyDetails GetImporter(JobDeclaration declaration)
		{
			IPartyDetails partyDetails = null;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			if (importerDocumentaryAddress != null && importerDocumentaryAddress.E2_RN_NKCountryCode == CountryCodes.Taiwan && (importerDocumentaryAddress.E2_AddressOverride || importerDocumentaryAddress.HasRealOrganisation))
			{
				var mainAddress = importerDocumentaryAddress.Address;
				partyDetails = new NX5105ImporterWrapper(mainAddress, importerDocumentaryAddress.CBPCode, declaration.ImporterDocumentaryAddress, OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID);
			}
			return partyDetails;
		}

		internal static IPartyDetails GetExporter(JobDeclaration declaration)
		{
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			IPartyDetails result = null;
			if (supplierDocumentaryAddress != null && (supplierDocumentaryAddress.E2_AddressOverride || supplierDocumentaryAddress.HasRealOrganisation))
			{
				result = new Exporter(declaration, supplierDocumentaryAddress?.Organisation, supplierDocumentaryAddress);
			}
			return result;
		}

		internal static IPartyDetails GetAgent(JobDeclaration declaration)
		{
			var id = declaration.CusEntryInstruction.CEI_BoxNumber;
			var roleCode = MessageConstants.RoleCodes.CustomsBroker;
			var customsProfile = declaration.JE_CustomsProfile;
			return new AgentWrapper(id, roleCode, SharedHelper.ExtractSubBoxID(customsProfile), declaration.DeclarantAddress);
		}

		internal static (string TypeCode, string CustomsRegNo) GetConsigneeIdAndTypeCode(OrgHeader orgHeader)
		{
			var typeCode = ZString.Empty;
			var customsRegNo = orgHeader.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode);
			if (!customsRegNo.IsEmpty)
			{
				typeCode = PartyIdentifierCodeList.Codes._58;
			}
			else
			{
				customsRegNo = orgHeader.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.PID);
				if (!customsRegNo.IsEmpty)
				{
					typeCode = PartyIdentifierCodeList.Codes._174;
				}
				else
				{
					customsRegNo = orgHeader.GetCustomsRegNo(OrgCusCode.CodeTypes.PassportID);
					if (!customsRegNo.IsEmpty)
					{
						typeCode = PartyIdentifierCodeList.Codes._53;
					}
				}
			}
			return (typeCode, customsRegNo);
		}
	}
}
