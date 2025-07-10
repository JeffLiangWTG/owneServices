using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public sealed class CusSCAOceanBillDataContextManager : ShipmentDataContextManager<BaseCusSCAOceanBill>
	{
		public static ITopLevelDataObjectReader GetDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var targetCountryCode = universalShipment.GetTargetCountryCode();
			var provider = factory.BOFactory.GetUniversalCustomsDataObjectProvider(targetCountryCode);
			var reader = provider != null ? MultipleTopLevelObjectReadersWrapper.CreateWrapper(universalShipment, universalShipment.HasRecipientRole(RecipientRoleType.HSA), hls =>
				provider.GetNewCusSCAOceanBillDataObjectReaders(universalShipment, hls, logger, factory)) : null;
			if (reader == null)
			{
				logger.Log(Integration.LogType.Error, Res.GetString("afac7255-93ff-4f90-b97c-0eee72c18010", "{1} is not currently supported for country '{0}'.", targetCountryCode, "CusSCAOceanBill"));
			}

			return reader;
		}

		public override ZString DataContextKey
		{
			get { return ParentBO?.CB_MessageReference ?? ZString.Empty; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.SeaOceanBill; }
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.CustomDeclarationExportDirectory.Value; }
		}

		public override bool ManagesEvents
		{
			get { return true; }
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();
			if (ParentBO != null)
			{
				var branch = ParentBO.Branch;
				var company = branch != null ? branch.Company : null;
				var countryCode = company != null ? company.GC_RN_NKCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var contextReader = ObjectFactory.GetCountrySpecificOrDefault<CusSCAOceanBillDataEventContextReader>(countryCode, ParentBO);
				contextReader.AddEventContextValues(result);
			}
			return result.Count == 0 ? null : result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var countryCode = logger.TopLevelDataContext != null ? logger.TopLevelDataContext.CountryCodeToImportInto : ZString.Empty;
			return ObjectFactory.GetCountrySpecificOrDefault<CusSCAOceanBillDataEventParentFinder>(countryCode, factory, this, logger);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return GetDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			var ocean = ParentBO ?? writeManager.Action?.ParentBO as BaseCusSCAOceanBill;
			System.Diagnostics.Debug.Assert(
				ocean != null,
				(NoResString)"Unable to get CusSCAOceanBill from DataContextManager.ParentBO or IDataWritingManager.Action.ParentBO to retrieve country-specific writer.");

			return GetSCAOceanBillDataObjectWriter(writeManager, ocean) ?? new CusSCAOceanBillDataObjectWriter(writeManager);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(IsTargeted);
		}

		bool IsTargeted(IRecipientRoleDataObject role)
		{
			switch (role.Code)
			{
				case RecipientRoleType.HSA:
				case RecipientRoleType.SRP:
					return true;
				default:
					return false;
			}
		}

		static ITopLevelDataObjectWriter GetSCAOceanBillDataObjectWriter(IDataWritingManager writeManager, BaseCusSCAOceanBill ocean)
		{
			ITopLevelDataObjectWriter result = null;
			if (ocean != null)
			{
				var countryCode = ocean.GetCountryCodeSafe();
				if (!countryCode.IsEmpty)
				{
					var provider = ocean.Factory.GetUniversalCustomsDataObjectProvider(countryCode);
					result = provider != null ? provider.GetNewCusSCAOceanBillDataObjectWriter(writeManager) : null;
				}
			}
			return result;
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var result = new ZQuery(CusSCAOceanBillSchema.CB_MessageReference, matchingValues.Key);
			result.AddToFilter(CusSCAOceanBillSchema.CB_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
			return result;
		}
	}
}
