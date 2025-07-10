using System.Collections.Immutable;

namespace Enterprise.Packing.Business.Scanning
{
	public static class PackingBarcodes
	{
		public const string ID = "*";
		public const string AddOuter = ID + "ADD_OUTER" + ID;
		public const string AddInner = ID + "ADD_INNER" + ID;
		public const string ClosePackage = ID + "CLOSE_PACKAGE" + ID;
		public const string OpenPackage = ID + "OPEN_PACKAGE" + ID;
		public const string ChangePackMode = ID + "CHANGE_PACK_MODE" + ID;
		public const string ChangeScanMode = ID + "CHANGE_SCAN_MODE" + ID;

		public static readonly ImmutableArray<string> Barcodes = ImmutableArray.Create(AddOuter, AddInner, ClosePackage, OpenPackage, ChangePackMode, ChangeScanMode);
	}
}
