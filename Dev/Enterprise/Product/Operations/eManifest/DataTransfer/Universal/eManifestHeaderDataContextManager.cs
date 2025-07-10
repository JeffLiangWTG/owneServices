using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eManifest.DataTransfer.Universal
{
	public class eManifestDataContextManager : ShipmentDataContextManager<SupplierBookingHeader>, IParentEventDataContextManager
	{
		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new SupplierBookingHeaderReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new SupplierBookingHeaderWriter(writeManager);
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();
			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new SupplierBookingHeaderEventParentFinder(factory, this, logger);
		}

		public override ZString DataContextKey
		{
			get
			{
				ZString orgCode = ZString.Empty;

				if (ParentBO.Consignor != null && !ParentBO.Consignor.OA_OH.IsEmpty)
				{
					var consignor = ParentBO.Factory.Load<OrgHeader>(ParentBO.Consignor.OA_OH);
					if (consignor != null)
					{
						orgCode = consignor.OH_Code;
					}
				}

				return string.Format("{0}~{1}", orgCode.EscapeTildas(), ParentBO.DH_SupplierReference.EscapeTildas());
			}
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.eManifest; }
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZDBOnlyQuery query = null;

			var dataContextKey = matchingValues.Key;
			if (!dataContextKey.IsEmpty)
			{
				var contextKey = dataContextKey.SplitIgnoringEscapedDelimiter('~', '!');

				if (contextKey.Length == 2)
				{
					var consignorCode = contextKey[0].UnEscapeTildas();
					var supplierReference = contextKey[1].UnEscapeTildas();

					var subQueryOrgHeader = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
					subQueryOrgHeader.AddToFilter(OrgHeaderSchema.OH_Code, consignorCode);

					var subQueryOrgAddress = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
					subQueryOrgAddress.AddSubQuery(OrgAddressSchema.OA_OH, subQueryOrgHeader, JoinCondition.And);
					query = new ZDBOnlyQuery(typeof(SupplierBookingHeader));
					query.AddToFilter(SupplierBookingHeaderSchema.DH_SupplierReference, supplierReference);
					query.AddSubQuery(SupplierBookingHeaderSchema.DH_OA_Consignor, subQueryOrgAddress, JoinCondition.And);
				}
			}

			return query;
		}

		IEnumerable<IEventDataContextManager> IParentEventDataContextManager.ChildContextManagers
		{
			get { return ParentBO.BookingLines.Select(line => line.GetUniversalDataContextManager()).OfType<IEventDataContextManager>(); }
		}
	}
}
