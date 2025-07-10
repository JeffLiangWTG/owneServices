using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public partial class TWMessageStatusCodeList
	{
		public static ZBool IsValidCode(ZString code)
		{
			return new TWMessageStatusCodeList().ContainsCode(code);
		}
	}
}
