using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CreditTermsCodeListDays : CodeDescriptionPairList
	{
		public CreditTermsCodeListDays()
		{
			AddNumberOfDays();
		}

		protected void AddNumberOfDays()
		{
			AddPair("1", "1 Day");
			for (int i = 2; i < 1000; i++)
			{
				AddPair(i.ToString(CultureInfo.InvariantCulture), i.ToString(CultureInfo.InvariantCulture) + " Days");
			}
		}
	}
}
