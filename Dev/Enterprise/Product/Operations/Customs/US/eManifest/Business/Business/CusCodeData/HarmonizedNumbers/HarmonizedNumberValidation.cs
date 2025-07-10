using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class HarmonizedNumberValidation : CusCodeDataValidation
	{
		public HarmonizedNumberValidation(AutoCusCodeData parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			var number = Parent.CY_Data;
			if (!number.IsEmpty)
			{
				var validLengths = new[] { 6, 8, 10 };
				if (!validLengths.Contains(number.Length))
				{
					Parent.CY_DataInfo.AddMessageError("The harmonized number has invalid length, it should be either 6, 8 or 10 digits.");
				}
				else
				{
					var query = new ZQuery(USCTariffSchema.UE_Tariff, SQLComparisonOperator.StartsWith, number);
					query.AddToFilter(USCTariffSchema.UE_DateFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
					query.AddToFilter(USCTariffSchema.UE_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
					if (Parent.Factory.LoadTop1<USCTariff>(query) == null)
					{
						Parent.CY_DataInfo.AddMessageError("The harmonized number is not recognized as a valid tariff. Please check the tariff or, " +
														   "if necessary, send a query to customs for the latest tariff information (Customs Declarations->Actions->Reference File Request->Tariff).");
					}
				}
			}

			var commodity = (Commodity)((CusCodeData)Parent).Parent;
			if (commodity != null)
			{
				commodity.Validation.ValidateBY_HarmonizedNumbers();
			}
		}
	}
}
