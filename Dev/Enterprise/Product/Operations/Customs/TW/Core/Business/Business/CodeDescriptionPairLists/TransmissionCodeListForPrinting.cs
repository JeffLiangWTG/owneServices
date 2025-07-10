using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class TransmissionCodeListForPrinting : CodeDescriptionPairList
	{
		protected TransmissionCodeListForPrinting()
		{
			AddPair(Codes.Auto, Descriptions.Auto);
			AddPair(Codes.CVT, Descriptions.CVT);
			AddPair(Codes.Hand, Descriptions.Hand);
			AddPair(Codes.Manual, Descriptions.Manual);
		}

		public static TransmissionCodeListForPrinting Instance
		{
			get { return instance ?? (instance = new TransmissionCodeListForPrinting()); }
		}
		[ThreadStatic]
		static TransmissionCodeListForPrinting instance;

		public abstract class Codes
		{
			public const string Auto = "A";
			public const string CVT = "C";
			public const string Hand = "H";
			public const string Manual = "M";
		}

		public abstract class Descriptions
		{
			public static MultilingualString Auto { get { return ResString.GetMultilingualString("TransmissionCodeListForPrinting|Auto", "AUTOMATIC"); } }
			public static MultilingualString CVT { get { return ResString.GetMultilingualString("TransmissionCodeListForPrinting|CVT", "CONTINUOUSLY VARIABLE"); } }
			public static MultilingualString Hand { get { return ResString.GetMultilingualString("TransmissionCodeListForPrinting|Hand", "MANUMATIC"); } }
			public static MultilingualString Manual { get { return ResString.GetMultilingualString("TransmissionCodeListForPrinting|Manual", "MANUAL"); } }
		}
	}
}
