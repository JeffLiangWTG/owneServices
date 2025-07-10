using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class ContractNumberComparer : BaseRateLineComparer
	{
		public ContractNumberComparer(RatingCriteria criteria, bool isCosting)
		{
			var numbers = isCosting
				? criteria.CarrierContractNumbers
				: criteria.ClientContractNumbers;
			NonBlankNumbers = numbers.Where(x => !x.IsEmpty).ToList();
		}

		public IEnumerable<ZString> NonBlankNumbers { get; }

		public override int Compare(FastLine line1, FastLine line2)
		{
			int result = 0;

			if (NonBlankNumbers.Any())
			{
				var number1 = line1.ParentRateEntry.TI_ContractNumber;
				var number2 = line2.ParentRateEntry.TI_ContractNumber;
				if (number2.IsEmpty)
				{
					if (!number1.IsEmpty && NonBlankNumbers.Any(x => x.EqualsIgnoringCase(number1)))
					{
						result = 1;
					}
				}
				else if (number1.IsEmpty)
				{
					if (NonBlankNumbers.Any(x => x.EqualsIgnoringCase(number2)))
					{
						result = -1;
					}
				}
			}

			return result;
		}

		protected override string GetName()
		{
			return (NoResString)"Contract Number"; // log message, subject to change, more for support people as of now
		}
	}
}
