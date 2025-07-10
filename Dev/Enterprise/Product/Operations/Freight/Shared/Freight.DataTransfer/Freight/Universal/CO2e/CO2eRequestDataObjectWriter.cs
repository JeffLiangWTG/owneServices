using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class CO2eRequestDataObjectWriter<TBusinessObject> : TopLevelDataObjectWriter<TBusinessObject, UniversalShipment> where TBusinessObject : BusinessObject
	{
		readonly ICO2eCalculationSupporter HostSupporter;

		public CO2eRequestDataObjectWriter(IDataWritingManager manager, ICO2eCalculationSupporter hostSupporter) : base(manager)
		{
			HostSupporter = hostSupporter;
		}

		public CO2eRequestDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
		}

		protected override void PopulateDataObject(TBusinessObject bookingBO, UniversalShipment bookingDataObject)
		{
			PopulateInfoCollection(bookingDataObject);
		}

		protected void PopulateInfoCollection(UniversalShipment dataObject)
		{
			if (HostSupporter != null)
			{
				dataObject.SetAddInfoCollection(() =>
				{
					return new List<AddInfo>() {
						new AddInfo() {
							Key = "ParentJobType",
							Value = HostSupporter.GetType().GetAttribute<UniversalDataContextAttribute>()?.DataContextType.ToString()
						},
						new AddInfo() {
							Key = "ParentJobId",
							Value =  (HostSupporter as ICodeDescription).Code
						}
					};
				});
			}
		}
	}
}
