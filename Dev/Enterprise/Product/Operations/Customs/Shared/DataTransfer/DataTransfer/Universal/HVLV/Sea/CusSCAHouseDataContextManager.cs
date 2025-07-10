using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public sealed class CusSCAHouseDataContextManager : ShipmentDataContextManager<BaseCusSCAHouse>
	{
		#region Overrides

		public override ZString DataContextKey => ParentBO?.CA_BGMReference ?? ZString.Empty;

		public override DataContextType DataContextType => DataContextType.SeaHouseBill;

		public override bool ManagesEvents => true;

		public override bool ManagesShipments => false;

		public override string DefaultOutputDirectory => throw new NotImplementedException();

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();
			if (ParentBO != null)
			{
				//For now AU use base DataEventContextReader to create the common event context.
				//We can change to ObjectFactory.GetCountrySpecificOrDefault<CusSCAHouseDataEventContextReader> when specific event context needed 
				var contextReader = new CusSCAHouseDataEventContextReader(ParentBO);
				contextReader.AddEventContextValues(result);
			}
			return result.Count == 0 ? null : result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			//For now AU use base DataEventParentFinder to get event parent.
			//We can change to ObjectFactory.GetCountrySpecificOrDefault<CusSCAHouseDataEventContextReader> when specific DataEventParentFinder needed 
			return new CusSCAHouseDataEventParentFinder(factory, this, logger);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			throw new NotImplementedException();
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			throw new NotImplementedException();
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			throw new NotImplementedException();
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		#endregion
	}
}
