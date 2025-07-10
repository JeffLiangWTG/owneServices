using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public static class ForwardingConsolExtensions
	{
		public static ZString GetUpperCaseAMSBill(this ForwardingConsol consol)
		{
			var ams = consol.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS);
			return ams?.CE_EntryNum.ToUpper() ?? ZString.Empty;
		}
	}
}
