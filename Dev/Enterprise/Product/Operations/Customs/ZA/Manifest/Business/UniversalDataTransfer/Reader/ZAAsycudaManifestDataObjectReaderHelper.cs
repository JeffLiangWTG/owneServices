using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.ZA.Manifest.Business.UniversalDataTransfer
{
	public class ZAAsycudaManifestDataObjectReaderHelper : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestDataObjectReaderHelper
	{
		public ZAAsycudaManifestDataObjectReaderHelper(BusinessObjectFactory factory)
			: base(Core.Constants.CountryCodes.SouthAfrica, factory)
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

		protected override void FillAdditionalDatesCore(List<Date> dateCollection, IColumnIndexer header)
		{
			var headerBo = (AsycudaManifestHeader)header;
			var date = dateCollection.FirstOrDefault(DateType.LoadingDate, ZBool.True);
			if (date != null && date.Value.HasValue)
			{
				headerBo.EstimatedTimeOfLoading = date.Value.Value;
			}
		}
	}
}
