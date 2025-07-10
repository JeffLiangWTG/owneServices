using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.ZA.Manifest.Business.UniversalDataTransfer
{
	public class ZAAsycudaManifestHeaderDataObjectWriterHelper : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriterHelper
	{
		public ZAAsycudaManifestHeaderDataObjectWriterHelper(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override IEnumerable<GenAddOnDetail> GetAsycudaManifestHeaderGenAddOnColumnListCore(ASYCUDA.Business.AsycudaManifestHeader baseHeaderBO)
		{
			var headerBo = (AsycudaManifestHeader)baseHeaderBO;
			foreach (var detail in base.GetAsycudaManifestHeaderGenAddOnColumnListCore(headerBo))
			{
				yield return detail;
			}
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBo.PlaceOfEntry), AddInfoKey = AsycudaManifestHeader.Schema.PlaceOfEntry, GenAddOnColumnName = AsycudaManifestHeader.Schema.PlaceOfEntry, PropertyName = nameof(headerBo.PlaceOfEntry) };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(headerBo.PlaceOfExit), AddInfoKey = GenAddOnHelper.PlaceOfExitCode, GenAddOnColumnName = GenAddOnHelper.PlaceOfExitCode, PropertyName = nameof(headerBo.PlaceOfExit) };
		}

		protected override IEnumerable<Date> GetHeaderAdditionalDateAddInfosCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var head = (AsycudaManifestHeader)header;
			yield return new Date { Type = DateType.LoadingDate, IsEstimate = ZBool.True, Value = head.EstimatedTimeOfLoading };

			foreach (var detail in base.GetHeaderAdditionalDateAddInfosCore(head))
			{
				yield return detail;
			}
		}
	}
}
