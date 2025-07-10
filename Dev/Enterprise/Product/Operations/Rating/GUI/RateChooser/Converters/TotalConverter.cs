using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.GUI.RateChooser.ViewModel;

namespace Enterprise.Rating.GUI.RateChooser.Converters
{
	public class TotalConverter
	{
		public string Convert(IEnumerable<ChargeViewModel> charges, ChargesViewModel parentGroup)
		{
			var sum = charges?
				.Where(charge => charge.IsActive)
				.Select(c => c.CalculatedPriceInDefaultCurrency())
				.Where(m => m.IsValid)
				.Sum(m => m.Amount);

			if (sum.HasValue && sum.Value > 0)
			{
				if (parentGroup != null)
				{
					return parentGroup.ConvertToCurrentCompanyFormat(sum.Value);
				}
				return sum.Value.ToString();
			}
			return string.Empty;
		}
	}
}
