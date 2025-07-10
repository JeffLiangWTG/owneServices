using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class SGAsycudaBillDataObjectReader : AsycudaBillDataObjectReader
	{
		public SGAsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ASYCUDA.Business.AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory, header, helper, isUpdateEnabled)
		{
		}

		protected override IEnumerable<ZString> GetBillPropertiesToSuspendSetting()
		{
			foreach (var property in base.GetBillPropertiesToSuspendSetting())
			{
				yield return property;
			}
			var partyID = BillCountryAdditionalData?.AddInfoCollection.GetZStringValue(Constants.EventContext.PartyIndicator, logger);
			if (partyID.HasValue)
			{
				yield return AsycudaBill.Schema.SG_PartyID;
			}
		}
	}
}
