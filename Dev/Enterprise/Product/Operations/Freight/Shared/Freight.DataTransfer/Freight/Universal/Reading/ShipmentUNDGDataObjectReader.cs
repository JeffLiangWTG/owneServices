using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ShipmentUNDGDataObjectReader : UNDGDataObjectReader
	{
		public ShipmentUNDGDataObjectReader(ZString transportMode, UNDG dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Func<UNDGDataItem> undgDataItemBizObjProvider = null) : base(dataObject, logger, factory, undgDataItemBizObjProvider)
		{
			this.transportMode = transportMode;
		}

		public ShipmentUNDGDataObjectReader(ZString transportMode, UNDG dataObject, IXmlImportLogger logger, BusinessObjectFactory factory, Func<UNDGDataItem> undgDataItemBizObjProvider = null) : base(dataObject, logger, factory, undgDataItemBizObjProvider)
		{
			this.transportMode = transportMode;
		}

		readonly ZString transportMode;

		protected override ZString StandardWithFallback => base.StandardWithFallback.IsEmpty ? StandardFallbackToValue : base.StandardWithFallback;

		ZString StandardFallbackToValue
		{
			get
			{
				switch (transportMode)
				{
					case Constants.TransportModes.Air:
						return UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
					default:
						return UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				}
			}
		}
	}
}
