using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.ACEManifest.Business.UniversalDataTransfer
{
	public class ACEAsycudaManifestHeaderDataObjectWriterHelper : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriterHelper
	{
		public ACEAsycudaManifestHeaderDataObjectWriterHelper(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override IEnumerable<GenAddOnDetail> GetAsycudaBillGenAddOnColumnListCore(ASYCUDA.Business.AsycudaBill baseBillBO)
		{
			var billBO = (AsycudaBill)baseBillBO;
			yield return new GenAddOnDetail
			{
				TypeCode = AddOnColumnDataType.GetCodeFromObject(billBO.FDAIndicator),
				AddInfoKey = AddInfoConstants.BillCountry.FDAIndicator,
				GenAddOnColumnName = AsycudaBill.Schema.FDAIndicator,
				PropertyName = AsycudaBill.Schema.FDAIndicator
			};

			foreach (var detail in base.GetAsycudaBillGenAddOnColumnListCore(billBO))
			{
				yield return detail;
			}
		}

		protected override IEnumerable<Date> GetHeaderAdditionalDateAddInfosCore(ASYCUDA.Business.AsycudaManifestHeader baseHeader)
		{
			var header = (AsycudaManifestHeader)baseHeader;
			yield return new Date { Type = DateType.FirstArrivalInCountry, IsEstimate = ZBool.False, Value = header.EstDateAtFirstArrival };

			foreach (var detail in base.GetHeaderAdditionalDateAddInfosCore(header))
			{
				yield return detail;
			}
		}
	}
}
