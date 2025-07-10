using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class ManifestMessageProvider : ISummaryDeclarationInformation
	{
		public ManifestMessageProvider(AsycudaManifestHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}
		protected readonly AsycudaManifestHeader Header;

		ZDateTime EffectiveDate => Header.ApplicationBusinessProvider.GetEffectiveDateForDutyRate(Header);

		public ZString BusinessRegNo => Header.Branch?.Company?.GC_BusinessRegNo ?? ZString.Empty;
		public ZString ManifestType => TRManifestMessageHelper.TRManifestType(Header.AMA_ManifestType);
		public ZString Other => Header.AMA_ManifestDescription;
		public ZString Trailer1RegNo => TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_Trailer1RegNo, Header.AMA_ManifestType) ? ZString.Empty : Header.AMA_Trailer1RegNo;
		public ZString Trailer1RegCountry
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (!Header.AMA_RN_NKTrailer1RegCountry.IsEmpty && !TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_RN_NKTrailer1RegCountry, Header.AMA_ManifestType))
				{
					returnValue = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Header.Factory, Core.Constants.CountryCodes.Turkey, TurkishConstants.CountryMapType, Header.AMA_RN_NKTrailer1RegCountry, EffectiveDate);
				}
				return returnValue;
			}
		}
		public ZString Trailer2RegNo => TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_Trailer2RegNo, Header.AMA_ManifestType) ? ZString.Empty : Header.AMA_Trailer2RegNo;
		public ZString Trailer2RegCountry
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (!TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_RN_NKTrailer2RegCountry, Header.AMA_ManifestType) && !Header.AMA_RN_NKTrailer2RegCountry.IsEmpty)
				{
					returnValue = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Header.Factory, Core.Constants.CountryCodes.Turkey, TurkishConstants.CountryMapType, Header.AMA_RN_NKTrailer2RegCountry, EffectiveDate);
				}
				return returnValue;
			}
		}
		public ZInt NumberofBillsInDeclaration => ZInt.Zero;
		public ZString SafetySecurity => ZString.Empty;
		public ZString GroupBillofLadingNumber => TRManifestMessageHelper.IsForbidden(CusEntryNumber.Schema.CE_EntryNum, Header.AMA_ManifestType) ? Header.TIRNumber : ZString.Empty;
		public ZString PresentationCustomsOffice => TRManifestMessageHelper.IsForbidden(AsycudaManifestHeader.Schema.TR_GM_PresentationCustomsOffice, Header.AMA_ManifestType) ? ZString.Empty : TRMessageHelper.RemoveCountryCodePrefix(Header.TR_GM_PresentationCustomsOffice);
		GlbExternalPassword_TR TRBPassword => TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
		public ZString UserID => TRBPassword != null ? TRBPassword.GP_UserID : ZString.Empty;
		public ZString AgentType => TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_AgentType, Header.AMA_ManifestType) ? string.Empty : (Header.AMA_AgentType == Core.Constants.AgentType.Courier) ? TurkishConstants.AnswerYes : TurkishConstants.AnswerNo;
		public ZString CustomsDischargePort => ConvertPortCode(Header.AMA_TransportMode, (Header.AMA_RL_NKPortOfDischarge.StartsWith(Core.Constants.CountryCodes.Turkey, System.StringComparison.OrdinalIgnoreCase) && ((IVisitedPortParent)Header).SupportsCustomsPorts) ? Header.AMA_CustomsDischargePort : Header.AMA_RL_NKPortOfDischarge);
		public ZString CustomsLoadPort => ConvertPortCode(Header.AMA_TransportMode, (Header.AMA_RL_NKPortOfLoading.StartsWith(Core.Constants.CountryCodes.Turkey, System.StringComparison.OrdinalIgnoreCase) && ((IVisitedPortParent)Header).SupportsCustomsPorts) ? Header.AMA_CustomsLoadPort : Header.AMA_RL_NKPortOfLoading);
		public ZString PreviousBillNumber => Header.AMA_MasterBill;
		public ZString Voyage => TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_Voyage, Header.AMA_ManifestType) ? ZString.Empty : Header.AMA_Voyage;
		public ZString LloydsNumber => Header.AMA_LloydsNumber;
		public ZString RegistrationNumber => Header?.RegistrationEntryNumber?.CE_EntryLineReference ?? ZString.Empty;
		public ZString Nature => Header.AMA_Nature == ShipmentTypeList.Codes.Import23 ? TurkishConstants.ShipmentTypeImport : TurkishConstants.ShipmentTypeExport;
		public ZString TransportType
		{
			get
			{
				bool isIncludeField = Header.AMA_TransportMode == TransportTypeList.Codes.Sea && Header.AMA_ManifestType == TRManifestTypes.Codes.GRUPAJ;
				return !isIncludeField ? Header.TransportType : ZString.Empty;
			}
		}
		public ZString Vessel => Header.AMA_VesselName;
		public ZString CarrierBusinessRegNo => Header.Carrier?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) ?? ZString.Empty;
		public ZString TruckATAScorecardNumber => TRManifestMessageHelper.IsForbidden(CusEntryNumber.Schema.CE_EntryNum, Header.AMA_ManifestType) ? ZString.Empty : Header.TIRNumber;
		public ZString ConveyanceNationality => Header.AMA_RN_NKConveyanceNationality.IsEmpty && TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_RN_NKConveyanceNationality, Header.AMA_ManifestType) ? ZString.Empty : ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Header.Factory, Core.Constants.CountryCodes.Turkey, TurkishConstants.CountryMapType, Header.AMA_RN_NKConveyanceNationality, EffectiveDate);

		public ZString CustomsDischargeCountryCode
		{
			get
			{
				ZString portCode = (Header.AMA_RL_NKPortOfDischarge.StartsWith(Core.Constants.CountryCodes.Turkey, System.StringComparison.OrdinalIgnoreCase) && ((IVisitedPortParent)Header).SupportsCustomsPorts) ? Header.AMA_CustomsDischargePort : Header.AMA_RL_NKPortOfDischarge;
				return portCode.IsEmpty ? ZString.Empty : ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Header.Factory, Core.Constants.CountryCodes.Turkey, TurkishConstants.CountryMapType, portCode.SubstringSafe(0, 2), EffectiveDate);
			}
		}
		public ZString CustomsLoadCountryCode
		{
			get
			{
				ZString portCode = (Header.AMA_RL_NKPortOfLoading.StartsWith(Core.Constants.CountryCodes.Turkey, System.StringComparison.OrdinalIgnoreCase) && ((IVisitedPortParent)Header).SupportsCustomsPorts) ? Header.AMA_CustomsLoadPort : Header.AMA_RL_NKPortOfLoading;
				return portCode.IsEmpty ? ZString.Empty : ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Header.Factory, Core.Constants.CountryCodes.Turkey, TurkishConstants.CountryMapType, portCode.SubstringSafe(0, 2), EffectiveDate);
			}
		}
		public ZString LoadingUnloadingPlace => ZString.Empty;
		public ZString CustomsOffice => TRMessageHelper.RemoveCountryCodePrefix(Header.AMA_CustomsOffice);
		public ZDateTime DateAtCustomsOffice => Header.AMA_DateAtCustomsOffice;
		public ZString JobReference => TRMessageConstants.ReferencePrefix + Header.AMA_JobReference;
		public ZString XmlRefId => Header.PK.ToString();
		public IEnumerable<IBillofLading> BillofLadings
		{
			get
			{
				var bills = Header.Bills.Cast<AsycudaBill>().OrderBy(x => x.ABL_SequenceNumber);
				foreach (var bill in bills)
				{
					yield return new BillProvider(bill, EffectiveDate);
				}
			}
		}

		public IEnumerable<IOpeningSummaryDeclaration> OpeningSummaryDeclaration
		{
			get
			{
				var manifestToOpensManifestLevel = Header.ManifestsToOpenList.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == SubTypeListForManifestToOpen.Codes.Manifestlevel);
				foreach (var manifestToOpen in manifestToOpensManifestLevel)
				{
					yield return new OpeningSummaryDeclarationProvider(manifestToOpen, SubTypeListForManifestToOpen.Codes.Manifestlevel, null, null);
				}

				var manifestToOpensBillLevel = Header.ManifestsToOpenList.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == SubTypeListForManifestToOpen.Codes.Billlevel).GroupBy(x => x.CSI_ReferenceNumber2);
				foreach (var manifestToOpen in manifestToOpensBillLevel)
				{
					var listOfManifestToOpen = manifestToOpen.ToList();
					yield return new OpeningSummaryDeclarationProvider(listOfManifestToOpen[0], SubTypeListForManifestToOpen.Codes.Billlevel, listOfManifestToOpen, null);
				}

				var manifestToOpensBillLineLevel = Header.ManifestsToOpenList.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == SubTypeListForManifestToOpen.Codes.Billlinelevel).GroupBy(x => new { x.CSI_ReferenceNumber2, x.CSI_ReferenceNumber });
				foreach (var manifestToOpen in manifestToOpensBillLineLevel)
				{
					var listOfManifestToOpen = manifestToOpen.ToList();
					yield return new OpeningSummaryDeclarationProvider(listOfManifestToOpen[0], SubTypeListForManifestToOpen.Codes.Billlinelevel, null, listOfManifestToOpen);
				}
			}
		}

		public IEnumerable<IVehicleVisitedCountry> VehicleVisitedCountry
		{
			get
			{
				var ports = Header.VisitedPorts.Cast<VisitedPort>();
				foreach (var port in ports)
				{
					yield return new VisitedCountryProvider(port, EffectiveDate, Header.AMA_TransportMode, Header.AMA_ManifestType);
				}
			}
		}
		public IEnumerable<ICarrierCompany> CarrierCompany
		{
			get
			{
				var orgAddress = Header.Carrier;
				if (orgAddress != null)
				{
					yield return new CarrierCompanyProvider(orgAddress, EffectiveDate);
				}
			}
		}
		public BusinessObject Parent => Header;
		public IBusinessObjectCollection Messages => Header.Messages;

		ZString ConvertPortCode(ZString transportMode, ZString portCode)
		{
			var code = portCode;

			if (transportMode == TransportTypeList.Codes.Air && !portCode.IsEmpty)
			{
				var loco = Header.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, portCode);
				code = loco?.RL_IATA ?? ZString.Empty;
			}

			return code;
		}
	}
}
