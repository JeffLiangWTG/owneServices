using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class CashAdvanceDefaultingOption
	{
		public const string All = "ALL";
		public const string None = "NON";
		public const string SelectedChargeCodesAndGroups = "INC";

		public static string AllDescription => ResString.GetMultilingualString("e48f7c5c-9ef2-4e5f-a52a-9fb50793e310", "Advance Payment Required will default for all charge codes");
		public static string NoneDescription => ResString.GetMultilingualString("b9e7b193-78ab-4cf7-9ddc-387310bbac47", "Advance Payment Required will not default for any charge codes");
		public static string SelectedChargeCodesAndGroupsDescription => ResString.GetMultilingualString("f2f1a3d0-d62b-4e27-b88d-024bcd20f92e", "Advance Payment Required will only default for the charge groups and charges codes selected below");

		public static CodeDescriptionPairList CodesList
		{
			get
			{
				var codes = new CodeDescriptionPairList();
				codes.AddPair(All, AllDescription);
				codes.AddPair(None, NoneDescription);
				codes.AddPair(SelectedChargeCodesAndGroups, SelectedChargeCodesAndGroupsDescription);
				return codes;
			}
		}
	}
}
