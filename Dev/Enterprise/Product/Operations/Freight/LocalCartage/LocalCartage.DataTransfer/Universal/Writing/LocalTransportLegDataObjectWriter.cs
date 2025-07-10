using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	class LocalTransportLegDataObjectWriter : DataObjectWriter<CommonCartageLeg, TransportLeg>
	{
		internal LocalTransportLegDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override TransportLeg PopulateDataObject(CommonCartageLeg leg)
		{
			var legData = new TransportLeg(writeManager.WriterStrategy);

			legData.LegType = LegType.LocalTransport;
			legData.TransportMode = TransportMode.Road;
			legData.LegOrder = ZByte.TryParse(leg.JU_DisplayOrder.ToString(), out ZByte legOrder) ? legOrder : ZByte.Zero;

			var carrier = leg.TransportCo;
			if (carrier != null)
			{
				legData.Carrier = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Carrier)).GetDataObject(carrier.MainAddress);
			}

			return legData;
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(CommonCartageLeg sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}
	}
}
