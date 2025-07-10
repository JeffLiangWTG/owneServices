namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists
{
	public partial class YesNoUnknownList
	{
		public static bool? GetValueForCode(string code)
		{
			switch (code)
			{
				case Codes.Yes:
					return true;
				case Codes.No:
					return false;
			}
			return null;
		}
	}
}
