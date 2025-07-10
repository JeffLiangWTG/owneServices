using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public static class ETradeMessageHelper
	{
		public static ZString VehicleTransportType(ZString transportMode, ZBool vehicleType)
		{
			switch (transportMode)
			{
				case TransportTypeList.Codes.Air:
					return vehicleType ? "5" : "40";
				case TransportTypeList.Codes.Sea:
					return vehicleType ? "3" : "10";
				case TransportTypeList.Codes.Road:
					return vehicleType ? "4" : "30";
				default:
					return ZString.Empty;
			}
		}

		public static ZString CountryCodeOfTRCustoms(BusinessObjectFactory factory, ZString cw1CountryCode, ZDateTime effectiveDate)
		{
			var trCountryCode = ZString.Empty;
			if (!cw1CountryCode.IsEmpty)
			{
				trCountryCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.Turkey, TurkishConst.CountryMapType, cw1CountryCode.SubstringSafe(0, 2), effectiveDate);
			}
			return trCountryCode;
		}

		public static ZString VerficationCode(ZString verficationCode)
		{
			return verficationCode == "EXS" ? "V" : "Y";
		}

		public static class TurkishConst
		{
			public const string PackTypeBin = "BI";
			public const string CountryMapType = "CNTRY";
			public const string ContainerMode = "CNT";
			public const string TaxPaymentType = "P";
		}

		public static Credentials GetLastCredentials(AsycudaManifestHeader header)
		{
			var credentials = new Credentials();
			var linkedMessage = header.Messages.Cast<ETradeEDIMessage>().
				Where(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && x.EM_MessageType == TRMessageTypes.Codes.TRE).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();

			if (linkedMessage != null)
			{
				var glbStaff = header.Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, linkedMessage.EM_MessageOwner)).FirstOrDefault();
				var trUserInfo = TRGlbStaffWrapper.Get(glbStaff)?.TRBPassword;
				if (trUserInfo != null)
				{
					credentials.Username = trUserInfo.GP_UserID;
					credentials.Password = TRManifestMessageBuilderHelper.MD5Hash(trUserInfo.CurrentDecryptedPassword);
				}
			}

			return credentials;
		}

		public class Credentials
		{
			public Credentials()
			{
				Username = ZString.Empty;
				Password = ZString.Empty;
			}

			public ZString Username { get; set; }
			public ZString Password { get; set; }
		}
	}
}
