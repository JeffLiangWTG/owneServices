using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public sealed class AirManifestDataContextManager : ShipmentDataContextManager<CusMAWB>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.AirManifest; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.CM_MessageReference; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(IsTargeted) && (dataSources == null || !dataSources.Any(HasAirManifestLineContextType));
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return GetDataObjectReader(universalShipment, logger, factory, false);
		}

		internal static ITopLevelDataObjectReader GetDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck)
		{
			var targetCountryCode = universalShipment.GetTargetCountryCode();
			var provider = factory.BOFactory.GetUniversalCustomsDataObjectProvider(targetCountryCode);
			var reader = provider != null ? MultipleTopLevelObjectReadersWrapper.CreateWrapper(universalShipment, universalShipment.HasRecipientRole(RecipientRoleType.HCA), hls =>
				provider.GetNewAirManifestDataObjectReaders(universalShipment, hls, logger, factory, singleHAWBCheck)) : null;
			if (reader == null)
			{
				logger.LogBoth(Integration.LogType.Error, Res.GetString("afac7255-93ff-4f90-b97c-0eee72c18010", "{1} is not currently supported for country '{0}'.", targetCountryCode, "AirManifest"));
			}
			return reader;
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
			var mawb = ParentBO ?? writeManager.Action?.ParentBO as CusMAWB;
			System.Diagnostics.Debug.Assert(
				mawb != null,
				(NoResString)"Unable to get CusMAWB from DataContextManager.ParentBO or IDataWritingManager.Action.ParentBO to retrieve country-specific writer.");

			return GetCusMAWBDataObjectWriter(writeManager, mawb) ?? new CusMAWBDataObjectWriter(writeManager);
		}

		static ITopLevelDataObjectWriter GetCusMAWBDataObjectWriter(IDataWritingManager writeManager, CusMAWB mawb)
		{
			ITopLevelDataObjectWriter result = null;
			if (mawb != null)
			{
				var countryCode = mawb.GetCountryCodeSafe();
				if (!countryCode.IsEmpty)
				{
					var provider = mawb.Factory.GetUniversalCustomsDataObjectProvider(countryCode);
					result = provider != null ? provider.GetNewAirManifestDataObjectWriter(writeManager) : null;
				}
			}
			return result;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				//For now AU and NZ use base DataEventContextReader to create the common event context.
				//We can change to get country specific DataEventContextReader when specific event context needed 
				var contextReader = new AirManifestDataEventContextReader(ParentBO);
				contextReader.AddEventContextValues(result);
			}

			return result.Count == 0 ? null : result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			//For now AU and NZ use base DataEventParentFinder to get event parent.
			//We can change to get country specific value when specific DataEventParentFinder needed 
			return new AirManifestDataEventParentFinder(factory, this, logger);
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.CustomDeclarationExportDirectory.Value; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var result = new ZQuery(CusMAWBSchema.CM_MessageReference, matchingValues.Key);
			result.AddToFilter(CusMAWBSchema.CM_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
			return result;
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
	}
}
