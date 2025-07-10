using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.TW.Business
{
	public static class LicensingMessageFunctionalReferenceIDFountain
	{
		public static ZString GetNext(BusinessObjectFactory factory, string vatNumber, bool isCMMessage = false)
		{
			ZString seqNo;
			if (isCMMessage)
			{
				seqNo = Env.NumberFountains.GetTWSWLicensingMessageFunctionalReferenceID(ZDateTime.Now.ToString("yyMMdd", CultureInfo.CurrentCulture)).GetNextFormatted(factory);
			}
			else
			{
				seqNo = Env.NumberFountains.GetTWLicensingMessageFunctionalReferenceID(ZDateTime.Now.ToString("yyMMdd", CultureInfo.CurrentCulture)).GetNextFormatted(factory);
			}
			return ZString.Format("{0}{1}", vatNumber, seqNo);
		}
	}
}
