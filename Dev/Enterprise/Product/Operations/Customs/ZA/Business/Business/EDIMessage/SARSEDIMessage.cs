using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class SARSEDIMessage : ZAMessage
	{
		#region Constructor
		protected SARSEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		#endregion

		public static class MessageTypes
		{
			public const string CONTRL = "CTL";
			public const string CUSDEC = "DEC";
			public const string CUSRES = "RES";
			public const string CUSRES_REQDOC = "RSQ";
			public const string EXPORT = "EXP";
			public const string REQDOC = "REQ";
			public const string STATAC = "STA";
			public const string CUSCAR = "CAR";
			public const string COSTCO = "CTO";
			public const string GOVGIO = "GIO";
			public const string CALINF = "CAL";
			public const string GENRAL = "GEN";
		}

		public static class MessageTypeNames
		{
			public const string CONTRL = "CONTRL";
			public const string CUSDEC = "CUSDEC";
			public const string CUSRES = "CUSRES";
			public const string CUSRES_CALINF = "CUSRES-CALINF";
			public const string CUSRES_COSTCO = "CUSRES-COSTCO";
			public const string CUSRES_CUSCAR = "CUSRES-CUSCAR";
			public const string CUSRES_GOVGIO = "CUSRES-GOVGIO";
			public const string CUSRES_GIO = "CUSRES-GIO";
			public const string CUSRES_REQDOC = "CUSRES-REQDOC";
			public const string EXPORT = "EXPORT";
			public const string REQDOC = "REQDOC";
			public const string CUSCAR = "CUSCAR";
			public const string GOVGIO = "GOVGIO";
			public const string STATAC_DETAIL = "STATAC-DETAIL";
			public const string STATAC_DAILY = "STATAC-DAILY";
			public const string COSTCO = "COSTCO";
			public const string CALINF = "CALINF";
			public const string CUSRES_EXP_RA = "CUSRES-EXP-RA";
			public const string GENRAL = "GENRAL";
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.ZACustomsEDIFACTNumberFountain("M", "SARS").GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ReceiveTransmit = Direction.Transmit;
			EM_IsTestMessage = Env.Registry.ZACustoms.GetIsTestMode(Branch);
		}
	}
}
