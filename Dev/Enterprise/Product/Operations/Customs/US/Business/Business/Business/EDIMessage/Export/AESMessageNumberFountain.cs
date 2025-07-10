using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class AESMessageNumberFountain
	{
		public static ZString GetNext(BusinessObjectFactory factory, bool isTransmitMessage, GlbCompany company)
		{
			string sender = "";
			string receiver = "";
			if (isTransmitMessage)
			{
				sender = company.GC_Code;
				receiver = AESTIREDIMessage.MessageTypes.AESDirect;
			}
			else
			{
				sender = AESTIREDIMessage.MessageTypes.AESDirect;
				receiver = company.GC_Code;
			}
			return Env.NumberFountains.EDIFACTNumberFountain("M", sender, receiver).GetNextFormatted(factory);
		}
	}
}
