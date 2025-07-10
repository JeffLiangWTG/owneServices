using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ShipmentAdditionalReferenceCollectionReader<T> : CusEntryAdditionalReferenceCollectionReader<T> where T : CommonShipment
	{
		public ShipmentAdditionalReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, T shipment)
			: base(additionalReferenceDataObjects, logger, factory, shipment)
		{
		}

		#region Implementation

		protected override CusEntryNumAdditionalReferenceCollection Numbers
		{
			get { return Parent.Numbers; }
		}

		#endregion
	}
}
