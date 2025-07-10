using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class EngineTypeCodeListForPrinting : CodeDescriptionPairList
	{
		protected EngineTypeCodeListForPrinting()
		{
			AddPair(Codes.CG, Descriptions.CG);
			AddPair(Codes.DE, Descriptions.DE);
			AddPair(Codes.DS, Descriptions.DS);
			AddPair(Codes.ED, Descriptions.ED);
			AddPair(Codes.EG, Descriptions.EG);
			AddPair(Codes.EL, Descriptions.EL);
			AddPair(Codes.GA, Descriptions.GA);
			AddPair(Codes.GE, Descriptions.GE);
			AddPair(Codes.LG, Descriptions.LG);
			AddPair(Codes.OT, Descriptions.OT);
		}

		public static EngineTypeCodeListForPrinting Instance
		{
			get { return instance ?? (instance = new EngineTypeCodeListForPrinting()); }
		}
		[ThreadStatic]
		static EngineTypeCodeListForPrinting instance;

		public abstract class Codes
		{
			public const string CG = "CG";
			public const string DE = "DE";
			public const string DS = "DS";
			public const string ED = "ED";
			public const string EG = "EG";
			public const string EL = "EL";
			public const string GA = "GA";
			public const string GE = "GE";
			public const string LG = "LG";
			public const string OT = "OT";
		}

		public abstract class Descriptions
		{
			public static MultilingualString CG { get { return ResString.GetMultilingualString("EngineTypeCodeListForPrinting|CG", "NATURAL GAS"); } }
			public static MultilingualString DE { get { return ResString.GetMultilingualString("EngineTypeCodeListForPrinting|DE", "HYBRID ELECTRIC DIESEL"); } }
			public static MultilingualString DS { get { return ResString.GetMultilingualString("EngineTypeCodeListForPrinting|DS", "DIESEL FUEL"); } }
			public static MultilingualString ED { get { return ResString.GetMultilingualString("EngineTypeCodeListForPrinting|ED", "DIESEL-POWERED RANGE EXTENDER"); } }
			public static MultilingualString EG { get { return ResString.GetMultilingualString("EngineTypeCodeListForPrinting|EG", "GASOLINE-POWERED RANGE EXTENDER"); } }
			public static MultilingualString EL { get { return ResString.GetMultilingualString("EngineTypeCodeListForPrinting|EL", "ELECTRIC"); } }
			public static MultilingualString GA { get { return ResString.GetMultilingualString("EngineTypeCodeListForPrinting|GA", "UNLEADED GASOLINE"); } }
			public static MultilingualString GE { get { return ResString.GetMultilingualString("EngineTypeCodeListForPrinting|GE", "HYBRID ELECTRIC GASOLINE"); } }
			public static MultilingualString LG { get { return ResString.GetMultilingualString("EngineTypeCodeListForPrinting|LG", "LIQUIFIED PETROLEUM GAS ENGINE"); } }
			public static MultilingualString OT { get { return ResString.GetMultilingualString("EngineTypeCodeListForPrinting|OT", "OTHER"); } }
		}
	}
}
