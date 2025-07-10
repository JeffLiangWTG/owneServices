using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public sealed class AirManifestLineDataContextManager : ShipmentDataContextManager<CusHAWB>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.AirManifestLine; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.CS_MessageReference; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(IsTargeted) && dataSources != null && dataSources.Any(HasAirManifestLineContextType);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return universalShipment.GetMatchingDataTarget(DataContextType.AirManifest) == null ? AirManifestDataContextManager.GetDataObjectReader(universalShipment, logger, factory, true)
				: null; // Should let the mawb context manager process this universal shipment and hence stopping the system from processing this twice.
		}

		protected override bool TryGetMatchingDataTarget(UniversalShipment universalShipment, out IDataTargetDataObject dataTarget)
		{
			dataTarget = null;
			return universalShipment.TransportMode.GetCodeAsUpperCase() == Core.Constants.TransportModes.Air && base.TryGetMatchingDataTarget(universalShipment, out dataTarget);
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		public override bool ManagesEvents
		{
			get { return true; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			var hawb = ParentBO ?? writeManager.Action?.ParentBO as CusHAWB;
			System.Diagnostics.Debug.Assert(
				hawb != null,
				(NoResString)"Unable to get CusHAWB from DataContextManager.ParentBO or IDataWritingManager.Action.ParentBO to retrieve country-specific writer.");

			return GetCusHAWBDataObjectWriter(writeManager, hawb) ?? new CusHAWBDataObjectWriter(writeManager, null);
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();
			if (ParentBO != null)
			{
				AirManifestLineDataEventContextReader contextReader;

				if (typeof(Integration.Customs.NZ.ICusHAWB).IsAssignableFrom(ParentBO.GetType()))
				{
					contextReader = ObjectFactory.GetCountrySpecificOrDefault<AirManifestLineDataEventContextReader>(Core.Constants.CountryCodes.NewZealand, ParentBO);
				}
				else
				{
					contextReader = new AirManifestLineDataEventContextReader(ParentBO);
				}

				contextReader.AddEventContextValues(result);
			}

			return result.Count == 0 ? null : result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			//For now AU and NZ use base DataEventParentFinder to get event parent.
			//We can change to get country specific value when specific DataEventParentFinder needed
			return new AirManifestLineDataEventParentFinder(factory, this, logger);
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.CustomDeclarationExportDirectory.Value; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		#region Implementation

		static bool IsTargeted(IRecipientRoleDataObject role)
		{
			switch (role.Code)
			{
				case RecipientRoleType.AAD:
				case RecipientRoleType.ARP:
				case RecipientRoleType.HCA:
					return true;
				default:
					return false;
			}
		}

		static Func<IDataSourceDataObject, bool> HasAirManifestLineContextType
		{
			get
			{
				var contextTypeName = nameof(DataContextType.AirManifestLine);
				return source => source.Type.GetValueOrDefault().EqualsIgnoringCase(contextTypeName);
			}
		}

		#endregion // Implementation

		static ITopLevelDataObjectWriter GetCusHAWBDataObjectWriter(IDataWritingManager writeManager, CusHAWB hawb)
		{
			ITopLevelDataObjectWriter result = null;
			var mawb = hawb != null ? hawb.MAWB : null;
			if (mawb != null)
			{
				var countryCode = mawb.GetCountryCodeSafe();
				if (!countryCode.IsEmpty)
				{
					var provider = hawb.Factory.GetUniversalCustomsDataObjectProvider(countryCode);
					result = provider != null ? provider.GetNewAirManifestLineDataObjectWriter(writeManager, null) : null;
				}
			}
			return result;
		}
	}
}
