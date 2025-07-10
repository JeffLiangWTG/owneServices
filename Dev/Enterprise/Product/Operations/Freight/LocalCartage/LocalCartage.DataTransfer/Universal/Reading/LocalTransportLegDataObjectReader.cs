using CargoWise.Common;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	public class LocalTransportLegDataObjectReader : DataObjectReader<TransportLeg, CommonCartageLeg>
	{
		public LocalTransportLegDataObjectReader(TransportLeg legDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CommonCartageLeg leg)
			: base(legDataObject, logger, factory)
		{
			Argument.NotNull(leg, "leg");

			this.leg = leg;
		}

		readonly CommonCartageLeg leg;

		protected override CommonCartageLeg GetExistingBusinessObject()
		{
			return leg;
		}

		protected override void PopulateBusinessObject(CommonCartageLeg targetBO)
		{
			if (dataObject.CustomizedFieldCollection != null && dataObject.CustomizedFieldCollection.Count > 0)
			{
				PopulateWorkflowCustomFields(targetBO, dataObject);
			}
		}
	}
}
