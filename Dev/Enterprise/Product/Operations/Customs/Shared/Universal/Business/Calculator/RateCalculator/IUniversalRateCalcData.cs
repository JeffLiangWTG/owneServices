using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.Universal
{
	public interface IUniversalRateCalcData
	{
		DateTime DateOfValuation { get; }
		decimal ValueForDuty { get; }
		decimal CustomsValue { get; }

		IDictionary<string, decimal> UnitOfMeasureValueList { get; }
		IDictionary<string, decimal> CountrySpecificValueList { get; }
		IDictionary<string, string> MeursingExpressionList { get; }

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IList<Tuple<string, string>> AdditionalInformationList { get; }
	}
}
