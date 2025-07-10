using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ShipmentUNDGDataObjectReaderTest : TestCaseWithFactory
	{
		public void TestStandardWithFallback()
		{
			var universalObjectFactory = new UniversalObjectFactory(Factory);
			AssertStandardWithFallBackResult(Factory, null);
			AssertStandardWithFallBackResult(null, universalObjectFactory);
		}

		void AssertStandardWithFallBackResult(BusinessObjectFactory factory, UniversalObjectFactory universalObjectFactory)
		{
			var logger = new TestErrorLogger();
			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			var reader = factory == null ? new ShipmentUNDGDataObjectReaderForTest(Constants.TransportModes.Air, undg, logger, universalObjectFactory) : new ShipmentUNDGDataObjectReaderForTest(Constants.TransportModes.Air, undg, logger, factory);
			AssertNull(undg.Standard);
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, reader.StandardWithFallbackExpose);

			reader = factory == null ? new ShipmentUNDGDataObjectReaderForTest(Constants.TransportModes.Other, undg, logger, universalObjectFactory) : new ShipmentUNDGDataObjectReaderForTest(Constants.TransportModes.Other, undg, logger, factory);
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, reader.StandardWithFallbackExpose);

			undg.Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, reader.StandardWithFallbackExpose);

			undg.Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, reader.StandardWithFallbackExpose);
		}

		class ShipmentUNDGDataObjectReaderForTest : ShipmentUNDGDataObjectReader
		{
			public ShipmentUNDGDataObjectReaderForTest(ZString transportMode, UNDG dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Func<UNDGDataItem> undgDataItemBizObjProvider = null) : base(transportMode, dataObject, logger, factory, undgDataItemBizObjProvider)
			{
			}

			public ShipmentUNDGDataObjectReaderForTest(ZString transportMode, UNDG dataObject, IXmlImportLogger logger, BusinessObjectFactory factory, Func<UNDGDataItem> undgDataItemBizObjProvider = null) : base(transportMode, dataObject, logger, factory, undgDataItemBizObjProvider)
			{
			}

			public ZString StandardWithFallbackExpose => StandardWithFallback;
		}
	}
}
