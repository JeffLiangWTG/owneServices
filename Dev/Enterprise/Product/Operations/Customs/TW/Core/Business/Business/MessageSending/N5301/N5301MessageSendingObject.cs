using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.N5301
{
	public partial class N5301MessageSendingObject : IN5301Declaration
	{
		public ZString ID => Header.EntryNumberForSendingObject;

		public ZDecimal TotalGrossMassMeasure => Header.ArrivalBill?.B0_Weight ?? ZDecimal.Zero;

		public ZInt TotalPackageQuantity => Header.ArrivalBill?.B0_ManifestQty ?? ZInt.Zero;

		public ZString TypeCode => Header.MovementHeader?.BM_InBondEntryType ?? ZString.Empty;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => new List<IAdditionalInformation>();

		public IPartyDetails Agent => new PartyDetails(Header.TW_BoxNumber, subBoxID: SharedHelper.ExtractSubBoxID(Header.BH_CustomsProfile), roleCode: "CB");

		public ITransportMeans BorderTransportMeans => new BorderTransportMeans(Header);

		public IPartyDetails Carrier => new PartyDetails(Header.Carrier.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode));

		public IConsignment Consignment => new Consignment(Header);

		public IPartyDetails Deconsolidator => null;

		public ZString LoadingLocation => Header.MovementHeader?.BM_PlaceOfLoading ?? ZString.Empty;

		public ZString RepresentativePersonName
		{
			get
			{
				var certificate = Header.CusAgent?.Certificates?.Find(cert =>
					cert.XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.Taiwan &&
					cert.XZ_Type == Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK &&
					(cert.XZ_ExpiryOrDueDate >= ZDateTime.Today || cert.XZ_ExpiryOrDueDate.IsEmpty)
				).OrderBy(x => x.XZ_IssueDate).FirstOrDefault();

				return certificate?.XZ_RefNumber ?? ZString.Empty;
			}
		}

		public IPartyDetails Applicant
		{
			get
			{
				IPartyDetails applicantParty = null;
				if (Header.Importer is OrgAddress orgAddress && Header.ImporterOrg is OrgHeader importerOrg)
				{
					var orgCusCodes = new[]
					{
						(CodeType: OrgCusCode.CodeTypes.VATCode, Code: PartyIdentifierCodeList.Codes._58),
						(CodeType: OrgCusCode.CodeTypes.PassportID, Code: PartyIdentifierCodeList.Codes._53),
						(CodeType: OrgCusCode.TaiwanCodeTypes.PID, Code: PartyIdentifierCodeList.Codes._174)
					};
					foreach (var (codeType, code) in orgCusCodes)
					{
						var customsRegNo = importerOrg.GetCustomsRegNo(codeType);
						if (!customsRegNo.IsEmpty)
						{
							var codesToLookFor = new string[] { OrgCusCode.CodeTypes.ControlledPremisesID, OrgCusCode.CodeTypes.WarehouseControlledPremisesID, OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.TaiwanCodeTypes.EPZ, OrgCusCode.TaiwanCodeTypes.FTZ };
							var customsControlID = orgAddress.GetCustomsRegNo(codesToLookFor);
							applicantParty = new PartyDetails(SharedHelper.GetIDStartWithNO(customsRegNo, code), customsControlID: customsControlID, name: GetName(orgAddress), chineseName: orgAddress.GetChineseName(), typeCode: code);
							break;
						}
					}
				}

				return applicantParty;
			}
		}

		public ZString UnloadingLocation
		{
			get
			{
				var result = Header.MovementHeader?.BM_ForeignDestPortKCode ?? ZString.Empty;
				if (result.IsEmpty)
				{
					result = Header.MovementHeader?.BM_RL_NKForeignDestPort ?? ZString.Empty;
				}
				return result;
			}
		}

		ZString GetName(OrgAddress orgAddress)
		{
			var result = ZString.Empty;
			if (orgAddress != null)
			{
				if (orgAddress.IsEnglish)
				{
					result = orgAddress.CompanyName;
				}
				if (result.IsEmpty)
				{
					result = orgAddress.GetTranslatedAddressInSpecificLanguage(Core.SharedConstants.Languages.English)?.CompanyName ?? ZString.Empty;
				}
			}
			return result;
		}
	}
}
