namespace Enterprise.Customs.US.Business
{
	public partial class BondDispositionCodeList
	{
		public static bool IsAdded(string bondDispositionCode)
		{
			return bondDispositionCode == Codes.CAB ||
				   bondDispositionCode == Codes.CEB ||
				   bondDispositionCode == Codes.CNB ||
				   bondDispositionCode == Codes.CUB ||
				   bondDispositionCode == Codes.EAB ||
				   bondDispositionCode == Codes.EEB ||
				   bondDispositionCode == Codes.ENB ||
				   bondDispositionCode == Codes.EUB;
		}
	}
}
