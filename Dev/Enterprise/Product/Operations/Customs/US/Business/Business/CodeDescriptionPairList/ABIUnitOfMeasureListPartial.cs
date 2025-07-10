namespace Enterprise.Customs.US.Business
{
	public partial class ABIUnitOfMeasureList
	{
		public static bool IsPackageType(string code)
		{
			return code == Codes.Barrels ||
			code == Codes.Case ||
			code == Codes.Dozen ||
			code == Codes.DozenPairs ||
			code == Codes.DozenPieces ||
			code == Codes.Number ||
			code == Codes.Packs ||
			code == Codes.Pairs ||
			code == Codes.Pieces;
		}
	}
}
