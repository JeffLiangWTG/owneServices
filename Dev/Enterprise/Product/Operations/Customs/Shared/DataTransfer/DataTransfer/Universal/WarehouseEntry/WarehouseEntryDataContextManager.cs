using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public sealed class WarehouseEntryDataContextManager : ShipmentDataContextManager<CusEntryHeader>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseCustomsEntry; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.CH_BGMReference.Left(35); }
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		public override bool ManagesShipments
		{
			get { return false; }
		}

		public override bool ManagesEvents
		{
			get { return true; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WarehouseEntryDataObjectWriter(writeManager);
		}

		public override string DefaultOutputDirectory
		{
			get { return ""; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var warehouseEntryDataEventContextCreator = GetCountrySpecificEventContextCreator(ParentBO);
				warehouseEntryDataEventContextCreator.AddCusEntryHeaderContextValues(result);
			}

			return result.Count == 0 ? null : result;
		}

		protected override IEnumerable<KeyValuePair<IZType, IZType>> GetAdditionalFieldsToUpdateValues()
		{
			var result = new List<KeyValuePair<IZType, IZType>>();

			if (ParentBO != null)
			{
				var warehouseEntryDataEventContextCreator = GetCountrySpecificEventContextCreator(ParentBO);
				warehouseEntryDataEventContextCreator.AddAdditionalFieldsToUpdateValues(result);
			}

			return result.Count == 0 ? null : result;
		}

		WarehouseEntryDataEventContextCreator GetCountrySpecificEventContextCreator(CusEntryHeader entry)
		{
			WarehouseEntryDataEventContextCreator creator = null;
			var countryOfJurisdiction = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(entry.CountryCode);
			var creators = ObjectFactory.Get<Hashtable>("WarehouseEntryDataEventContextCreators");
			var objectHandle = (ObjectHandle)creators[countryOfJurisdiction];
			if (objectHandle != null)
			{
				creator = (WarehouseEntryDataEventContextCreator)objectHandle.GetObject(entry);
			}
			else if (Core.Constants.CountryCodes.IsInEuropeanCustomsUnion(countryOfJurisdiction))
			{
				objectHandle = (ObjectHandle)creators[Core.Constants.CountryCodes.EuropeanUnion];
				creator = (WarehouseEntryDataEventContextCreator)objectHandle?.GetObject(entry);
			}

			return creator ?? new WarehouseEntryDataEventContextCreator(entry);
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZQuery result = null;
			if (matchingValues.DataObject is UniversalEvent)
			{
				var cusEntryHeaders = factory.Load<CusEntryHeader>(new ZQuery(CusEntryHeaderSchema.CH_BGMReference, matchingValues.Key));

				var selected = cusEntryHeaders;

				if (selected.Length != 1 && !matchingValues.CompanyCode.IsEmpty)
				{
					selected = cusEntryHeaders.Where(e => e.Declaration.Company.GC_Code == matchingValues.CompanyCode).Take(2).ToArray();
				}

				if (selected.Length != 1 && matchingValues.DataObject?.DataContext?.CountryCodeToImportInto is ZString countryCode && !countryCode.IsEmpty)
				{
					selected = cusEntryHeaders.Where(e => e.Declaration.Company.GC_RN_NKCountryCode == countryCode).Take(2).ToArray();
				}

				if (selected.Length != 1 && matchingValues.CompanyCode != GlbCompany.CurrentCompany.GC_Code)
				{
					selected = cusEntryHeaders.Where(e => e.Declaration.Company.GC_Code == GlbCompany.CurrentCompany.GC_Code).Take(2).ToArray();
				}

				if (selected.Length == 1)
				{
					result = new ZQuery(CusEntryHeaderSchema.PK, selected[0].PK);
				}
			}
			else
			{
				// Universal Architecture has already setup the correct environment
				var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
				query.AddToFilter(CusEntryHeaderSchema.CH_BGMReference, matchingValues.Key);
				var subQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
				subQuery.AddToFilter(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
				query.AddSubQuery(CusEntryHeaderSchema.CH_JE, subQuery, JoinCondition.And);
				result = query;
			}

			return result;
		}
	}
}
