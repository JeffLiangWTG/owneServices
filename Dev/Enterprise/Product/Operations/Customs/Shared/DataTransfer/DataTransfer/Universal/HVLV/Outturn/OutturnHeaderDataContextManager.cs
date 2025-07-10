using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class OutturnHeaderDataContextManager : ShipmentDataContextManager<CusOutturnHeader>
	{
		public override DataContextType DataContextType => DataContextType.SeaCargoOutturn;

		public override ZString DataContextKey => ParentBO.C6_SendersMessageReference;

		public override string DefaultOutputDirectory => "";

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources,
			IXmlSessionTracker importSessionLogger) => recipientRoles.Any(o => o.Code == RecipientRoleType.COA);

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			var outturnHeader = ParentBO ?? writeManager.Action?.ParentBO as CusOutturnHeader;
			System.Diagnostics.Debug.Assert(
				outturnHeader != null,
				(NoResString)"Unable to get CusOutturnHeader from DataContextManager.ParentBO or IDataWritingManager.Action.ParentBO to retrieve country-specific writer.");

			return GetCusOutturnHeaderDataObjectWriter(writeManager, outturnHeader) ?? new CusOutturnHeaderDataObjectWriter<CusOutturnHeader>(writeManager);
		}

		static ITopLevelDataObjectWriter GetCusOutturnHeaderDataObjectWriter(IDataWritingManager writeManager, CusOutturnHeader outturnHeader)
		{
			ITopLevelDataObjectWriter result = null;
			if (outturnHeader != null)
			{
				// When CusOutturnHeader has either _GB or _ApplicationCode then we can change this to support other type/country for now we only support AU.
				result = ObjectFactory.Get<IAUCusOutturnHeaderDataObjectWriter>("IAUCusOutturnHeaderDataObjectWriter", writeManager);
			}
			return result;
		}

		public override bool ManagesShipments => true;

		public override bool ManagesEvents => false;

		protected override bool TryGetMatchingDataTarget(Shipment universalShipment, out IDataTargetDataObject dataTarget)
		{
			dataTarget = null;
			return universalShipment.TransportMode.GetCodeAsUpperCase() == Core.Constants.TransportModes.Sea && base.TryGetMatchingDataTarget(universalShipment, out dataTarget);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return GetDataObjectReader(universalShipment, logger, factory);
		}

		// For now we only support AU.
		internal static ITopLevelDataObjectReader GetDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			ITopLevelDataObjectReader result = null;
			var targetCountryCode = universalShipment.GetTargetCountryCode();
			if (targetCountryCode == Core.Constants.CountryCodes.Australia)
			{
				if (universalShipment.IsHVLV())
				{
					result = ObjectFactory.Get<IAUCusOutturnHeaderDataObjectReader>("AUHVLVCusOutturnHeaderDataObjectReader", universalShipment, logger, factory);
				}
				else if (universalShipment.IsTWH())
				{
					result = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(universalShipment, logger, factory);
				}
				else
				{
					result = ObjectFactory.Get<IAUCusOutturnHeaderDataObjectReader>("IAUCusOutturnHeaderDataObjectReader", universalShipment, logger, factory);
				}
			}
			else
			{
				result = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(universalShipment, logger, factory);
			}

			return result;
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory,
			IXmlImportLogger logger)
		{
			ZQuery result = null;
			if (!matchingValues.Key.IsEmpty)
			{
				result = new ZQuery(CusOutturnHeaderSchema.C6_SendersMessageReference, matchingValues.Key);
			}
			return result;
		}
	}
}
