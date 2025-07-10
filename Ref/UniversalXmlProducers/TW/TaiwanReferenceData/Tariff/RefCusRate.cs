using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class RefCusRate
	{
		public DateTime ZZ2_StartDate { get; set; }

		public DateTime ZZ2_EndDate { get; set; }

		public string ZZ2_RateFormula { get; set; }

		public string ZZ2_RateFormulaDerivedFrom { get; set; }

		public string ZZ2_ZY1_NKRateCode { get; set; }

		public string ZZ2_ZZS_NKPreference { get; set; }

		public IEnumerable<IRefCusApplicability> RefCusApplicabilities => refCusApplicabilities;

		List<RefCusApplicability> refCusApplicabilities = new List<RefCusApplicability>();

		public IRefCusApplicability AddNewRefCusApplicability()
		{
			var refCusApp = new RefCusApplicability
			{
				Parent = this
			};
			refCusApplicabilities.Add(refCusApp);
			return refCusApp;
		}

		class RefCusApplicability : IRefCusApplicability
		{
			public RefCusRate Parent { get; set; }

			public DateTime ZZT_StartDate => Parent.ZZ2_StartDate;

			public DateTime ZZT_EndDate => Parent.ZZ2_EndDate.TrimSeconds();

			public string ZZT_ZZA_NKTradeGroup { get; set; }

			public string ZZT_OrderNumber { get; set; }

			public List<string> RefCusExcludedTradeGroup { get; set; }
		}
	}

	public interface IRefCusApplicability
	{
		RefCusRate Parent { get; }

		DateTime ZZT_StartDate { get; }

		DateTime ZZT_EndDate { get; }

		string ZZT_ZZA_NKTradeGroup { get; set; }

		string ZZT_OrderNumber { get; set; }

		List<string> RefCusExcludedTradeGroup { get; set; }
	}
}
