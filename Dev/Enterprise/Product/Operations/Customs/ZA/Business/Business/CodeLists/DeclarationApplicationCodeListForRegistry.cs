using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class DeclarationApplicationCodeListForRegistry : DeclarationApplicationCodeList
	{
		public new class Codes : DeclarationApplicationCodeList.Codes
		{
			public const string BothBuiltInDefaulted = "BTH";
			public const string BothInterfaceDefaulted = "BIT";
		}

		public new class Descriptions : DeclarationApplicationCodeList.Descriptions
		{
			public static MultilingualString BothBuiltInDefaulted { get { return ResString.GetMultilingualString("DeclarationApplicationCodeListForRegistry|Both Build In Defaulted", "Allow choosing entry submission method on Declaration. Built In ({0}) will default.", Codes.Builtin); } }
			public static MultilingualString BothInterfaceDefaulted { get { return ResString.GetMultilingualString("DeclarationApplicationCodeListForRegistry|Both Interfaced Defaulted", "Allow choosing entry submission method on Declaration. Interfaced ({0}) will default.", Codes.Interfaced); } }
		}

		public DeclarationApplicationCodeListForRegistry()
			: base()
		{
			AddPair(Codes.BothBuiltInDefaulted, Descriptions.BothBuiltInDefaulted);
			AddPair(Codes.BothInterfaceDefaulted, Descriptions.BothInterfaceDefaulted);
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.Interfaced, ResString.GetMultilingualString("DeclarationApplicationCodeListForRegistry|Interfaced", "Only allow entry submission through designated service provider interface")));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.Builtin, ResString.GetMultilingualString("DeclarationApplicationCodeListForRegistry|Builtin", "Only allow entry submission directly with SARS")));
		}
	}
}
