using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business.DocumentWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public static class SADDocumentWrapperHelper
	{
		public static BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> ConsolidateDutiesIfNeeded(BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper> currentCollectionWrapper)
		{
			var result = currentCollectionWrapper;

			var duties = currentCollectionWrapper.Cast<DutyFeeInformationDocWrapper>().Where(x =>
			{
				var code = x.Code;
				return code != "VAT" && code != "12B" && code != "PP's";
			}).ToArray();

			if (currentCollectionWrapper.Count > CountOfRow
				|| (duties.Any(x => x.Code != "DTY") && duties.Any(x => x.Code == "DTY")))
			{
				var dtyWrapper = new DutyFeeInformationDocWrapper("DTY", duties.Sum(x => x.Value));

				foreach (var dutyFeeInformationDocWrapper in duties)
				{
					currentCollectionWrapper.Remove(dutyFeeInformationDocWrapper);
				}

				var remainingWrappersExcludingDTY = currentCollectionWrapper
					.Cast<DutyFeeInformationDocWrapper>().ToArray();

				var newCollection = new[] { dtyWrapper }
					.Append(remainingWrappersExcludingDTY);

				result = new BusinessObjectCollectionWrapper<DutyFeeInformationDocWrapper>(newCollection);
			}

			return result;
		}

		const int CountOfRow = 5; //Fixed row count for "47. CALC OF DUTIES & TAXES" in SAD500, SAD501 template. WI00219947
	}
}
