using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	class PaymentTermsList : CodeDescriptionPairList
	{
		public PaymentTermsList()
		{
			for (var i = -99; i < 1000; i++)
			{
				AddPair($"{i:000;-00;000}", i.ToString(CultureInfo.InvariantCulture) + " Day" + (i == 1 ? "" : "s"));
			}
		}
	}
}
