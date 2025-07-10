using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class DeclarationApplicationCodeListForRegistry : DeclarationApplicationCodeList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Codes : DeclarationApplicationCodeList.Codes
		{
			public const string BothBuiltInDefaulted = "BTH";
			public const string BothInterfaceDefaulted = "BIT";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Descriptions : DeclarationApplicationCodeList.Descriptions
		{
			public static MultilingualString BothBuiltInDefaulted { get { return ResString.GetMultilingualString("02DF894C-7E07-4574-A50A-64A7D0185D9A", "Allow choosing entry submission method on Declaration. Built In ({0}) will default.", Codes.Builtin); } }
			public static MultilingualString BothInterfaceDefaulted { get { return ResString.GetMultilingualString("099EBE55-008F-427F-997A-975F283A57DF", "Allow choosing entry submission method on Declaration. Interfaced ({0}) will default.", Codes.Interfaced); } }
		}

		public DeclarationApplicationCodeListForRegistry()
			: base()
		{
			AddPair(Codes.BothBuiltInDefaulted, Descriptions.BothBuiltInDefaulted);
			AddPair(Codes.BothInterfaceDefaulted, Descriptions.BothInterfaceDefaulted);
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.Interfaced, ResString.GetMultilingualString("100449E3-FF46-4015-BC62-88051D6AFF6A", "Only allow entry submission through designated service provider interface")));
			AddOverwriteIfExists(new CodeDescriptionPair(Codes.Builtin, ResString.GetMultilingualString("B544F095-8055-446F-A4D8-92558EFFCBBE", "Only allow entry submission directly to Customs Authority")));
		}
	}
}
