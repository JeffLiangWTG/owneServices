using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	static class NX5105Helper
	{
		public static ZString ConvertBoolToString(this ZBool value, ZString trueValue, string falseValue = "")
		{
			return value ? trueValue : new ZString(falseValue);
		}
	}
}
