using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// For messaging requirement, PNC or PND or SLN (exist exclusively)
	/// </summary>
	public class AffirmationCodeComparer : IComparer<KeyValuePair<ZString, ZString>>
	{
		public int Compare(KeyValuePair<ZString, ZString> x, KeyValuePair<ZString, ZString> y)
		{
			if (ShouldThisComeFirst(x.Key) && !ShouldThisComeFirst(y.Key))
			{
				return -1;
			}
			else if (!ShouldThisComeFirst(x.Key) && ShouldThisComeFirst(y.Key))
			{
				return 1;
			}

			int result = string.Compare(x.Key, y.Key, true);
			if (result == 0)
			{
				result = string.Compare(x.Value, y.Value);
			}
			return result;
		}

		bool ShouldThisComeFirst(ZString cY_Code)
		{
			return string.Compare(cY_Code, AffirmationCodeConstants.Codes.PNC, true) == 0 || string.Compare(cY_Code, AffirmationCodeConstants.Codes.PND, true) == 0 || string.Compare(cY_Code, AffirmationCodeConstants.Codes.SLN, true) == 0;
		}
	}
}
