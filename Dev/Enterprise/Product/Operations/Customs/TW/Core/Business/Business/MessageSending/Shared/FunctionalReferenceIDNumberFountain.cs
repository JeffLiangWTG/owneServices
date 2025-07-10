using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.TW.Business
{
	public static class FunctionalReferenceIDNumberFountain
	{
		public static ZString GetNext(BusinessObjectFactory factory, string declarationId)
		{
			var seqNo = Env.NumberFountains.GetTWFunctionalReferenceIDNumber().GetNext(factory);
			return ZString.Format("{0}{1}", declarationId, seqNo.ToString("D5", CultureInfo.InvariantCulture));
		}
	}
}
