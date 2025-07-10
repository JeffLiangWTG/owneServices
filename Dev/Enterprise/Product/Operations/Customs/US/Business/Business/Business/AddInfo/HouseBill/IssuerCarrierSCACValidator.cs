using System.Globalization;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class IssuerCarrierSCACValidator
	{
		public IssuerCarrierSCACValidator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public bool IsValidSCACCode(ZString scacCode)
		{
			var result = false;
			if (!scacCode.IsEmpty)
			{
					var query = new ZQuery(USCarrierCombinedSchema.UI_Code, scacCode);
					var usCarrier = factory.LoadTop1<USCarrierCombined>(query);
					if (usCarrier != null)
					{
						result = true;
					}
			}
			return result;
		}

		public void ValidateSCACCode(ZPropertyInfo sCACCodeInfo, ZString modeOfTransport, ZString organisationType, bool shouldCheckValidCode, bool isExport = false, bool showMessageForSCACRequest = true)
		{
			var sCAC = (ZString)sCACCodeInfo.Value;
			if (!sCAC.IsEmpty)
			{
				bool isSeaTruckRailMode = modeOfTransport == TransportTypeList.Codes.Rail ||
											modeOfTransport == TransportTypeList.Codes.Sea ||
											modeOfTransport == TransportTypeList.Codes.Truck;
				bool invalidFormat = modeOfTransport == TransportTypeList.Codes.Air && (sCAC.Length != 2 && (!isExport || sCAC.Length != 3)) ||
									isSeaTruckRailMode && sCAC.Length != 4;

				if (invalidFormat)
				{
					sCACCodeInfo.AddMessageError(isExport ? InvalidSCACFormatForExport : InvalidSCACFormat);
				}
				else
				{
					if (shouldCheckValidCode && !IsValidSCACCode(sCAC))
					{
						sCACCodeInfo.AddMessageError(!showMessageForSCACRequest ? organisationType + InvalidSCAC : organisationType + InvalidSCAC + string.Format(CultureInfo.CurrentCulture, SendSCACRequestMessagesAdvice, BrandingFactory.Instance.ProductName, organisationType) );
					}
				}
			}
		}
		internal const string InvalidSCAC = " Standard Carrier Alpha Code (SCAC) unable to be found.";
		internal const string SendSCACRequestMessagesAdvice = "When the Declaration is saved, {0} will automatically submit a request for the latest information relating to the {1} SCAC that has been entered. A response should be available in a few minutes. Note that SCAC Requests can also be sent manually, at any time, by selecting Customs Declarations > Actions > Reference Files Request > Carrier Codes.";
		internal const string InvalidSCACFormat = "The Standard Carrier Alpha Code (SCAC) is not valid for the selected mode of transport. SCAC should be 2 characters in length for Air and 4 characters in length for other modes of transport.";
		internal const string InvalidSCACFormatForExport = "The Standard Carrier Alpha Code (SCAC) is not valid for the selected mode of transport. SCAC should be 2 or 3 characters in length for Air and 4 characters in length for other modes of transport.";
	}
}
