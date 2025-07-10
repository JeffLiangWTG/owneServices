using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ACEManifest.Business.UniversalDataTransfer
{
	public class ACEAsycudaManifestDataObjectReaderHelper : AsycudaManifestDataObjectReaderHelper
	{
		public ACEAsycudaManifestDataObjectReaderHelper(BusinessObjectFactory factory)
			: base(Core.Constants.CountryCodes.UnitedStates, factory)
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

		protected override AsycudaBillDataObjectReader GetBillDataObjectReaderCore(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			AsycudaBillDataObjectReader reader = null;
			if (dataObject.ShipmentType?.Code.ToString() == Core.Constants.ShipmentTypes.HighVolumeLowValue)
			{
				reader = new HVLVACEAsycudaBillDataObjectReader(dataObject, logger, factory, (AsycudaManifestHeader)header, (ACEAsycudaManifestDataObjectReaderHelper)helper, isUpdateEnabled);
			}
			else
			{
				reader = base.GetBillDataObjectReaderCore(dataObject, logger, factory, header, helper, isUpdateEnabled);
			}

			return reader;
		}

		protected override void FillAdditionalDatesCore(List<Date> dateCollection, IColumnIndexer baseHeaderBO)
		{
			var headerBo = (AsycudaManifestHeader)baseHeaderBO;
			var date = dateCollection.FirstOrDefault(DateType.FirstArrivalInCountry, false);
			if (date != null && date.Value.HasValue)
			{
				headerBo.EstDateAtFirstArrival = date.Value.Value;
			}
		}

		protected override bool ShouldReadColumn(SchemaColumn column, Shipment dataObject)
		{
			if (column == AsycudaManifestHeaderSchema.AMA_ContainerMode)
			{
				return !TransportTypeList.Codes.Air.Equals(dataObject.TransportMode?.Code);
			}
			else
			{
				return base.ShouldReadColumn(column, dataObject);
			}
		}
	}
}
