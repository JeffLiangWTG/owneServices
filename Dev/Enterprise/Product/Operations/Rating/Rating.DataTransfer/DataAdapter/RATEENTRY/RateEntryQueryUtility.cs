using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer
{
	public class RateEntryQueryUtility
	{
		public RateEntryQueryUtility()
		{
			Initialise();
		}

		public RateEntryQueryUtility(RatingHeader parentRatingHeader)
		{
			Initialise();
			this.ParentRatingHeader = parentRatingHeader;
		}

		#region Properties

		public int Frequency { get; set; }
		public bool IsCrossTrade { get; set; }
		public ZString Category { get; set; }
		public ZString Mode { get; set; }
		public ZString Destination { get; set; }
		public ZString Origin { get; set; }
		public ZString CommodityCode { get; set; }
		public ZString TransitTime { get; set; }
		public ZString FrequencyUnits { get; set; }
		public ZString CartagePickupAddressPostCode { get; private set; }
		public ZString CartageDeliveryAddressPostCode { get; private set; }
		public ZString Via { get; set; }
		public ZString ServiceLevel { get; set; }
		public ZString CarrierServiceLevel { get; set; }
		public ZGuid TransportProviderPK { get; set; }
		public ZGuid SupplierPK { get; set; }
		public ZGuid ConsigneePK { get; set; }
		public ZGuid ConsignorPK { get; set; }
		public ZGuid CartagePickupAddressOverridePK { get; set; }
		public ZGuid CartageDeliveryAddressOverridePK { get; set; }
		public OrgAddress CartageDeliveryAddress { get; set; }
		public OrgAddress CartagePickupAddress { get; set; }
		public ZGuid ContainterTypePK { get; set; }
		public ZDateTime StartDate { get; set; }
		public ZDateTime EndDate { get; set; }

		void Initialise()
		{
			Frequency = 0;
			Category = "";
			Mode = "";
			Destination = "";
			Origin = "";
			CommodityCode = "";
			TransitTime = "";
			FrequencyUnits = "";
			CartagePickupAddressPostCode = "";
			CartageDeliveryAddressPostCode = "";
			Via = "";
			ServiceLevel = "";
			CarrierServiceLevel = "";
			TransportProviderPK = ZGuid.Empty;
			ConsigneePK = ZGuid.Empty;
			ConsignorPK = ZGuid.Empty;
			CartagePickupAddressOverridePK = ZGuid.Empty;
			CartageDeliveryAddressOverridePK = ZGuid.Empty;
			ContainterTypePK = ZGuid.Empty;
			SupplierPK = ZGuid.Empty;
			StartDate = ZDateTime.Empty;
			EndDate = ZDateTime.Empty;
		}

		#endregion

		public void SetAllParameters(Xsd.RateEntry rateEntryXSD, IValueObjectImportContext context)
		{
			Category = rateEntryXSD.Category;
			Mode = rateEntryXSD.Mode;
			Origin = GetPortCode(rateEntryXSD.Origin, context.Factory);
			Destination = GetPortCode(rateEntryXSD.Destination, context.Factory);
			Via = GetPortCode(rateEntryXSD.Via, context.Factory);

			if (rateEntryXSD.IsCrossTradeSpecified)
			{
				IsCrossTrade = (rateEntryXSD.IsCrossTrade == Xsd.TrueFalse.@true);
			}

			OrgHeader transportProvider = GetCachedOrgFromParentRatingHeader(rateEntryXSD.TransportProvider, OrganisationTypes.Carrier, context);
			TransportProviderPK = transportProvider == null ? ZGuid.Empty : transportProvider.PK;

			ServiceLevel = rateEntryXSD.ServiceLevel;
			CarrierServiceLevel = rateEntryXSD.CarrierServiceLevel;

			if (!rateEntryXSD.CommodityCode.IsEmpty)
			{
				CommodityCode = rateEntryXSD.CommodityCode;
			}

			OrgHeader supplier = GetCachedOrgFromParentRatingHeader(rateEntryXSD.ServiceProvider, OrganisationTypes.Creditor, context);
			SupplierPK = supplier == null ? ZGuid.Empty : supplier.PK;

			OrgHeader consignee = GetCachedOrgFromParentRatingHeader(rateEntryXSD.Consignee, OrganisationTypes.Consignee, context);

			if (consignee != null)
			{
				ConsigneePK = consignee.PK;

				if (rateEntryXSD.CartageDeliveryAddress.IsSpecified)
				{
					CartageDeliveryAddress = RateImportHelper.Instance.GetOrgAddress(rateEntryXSD.CartageDeliveryAddress, consignee);

					if (CartageDeliveryAddress != null)
					{
						CartageDeliveryAddressOverridePK = CartageDeliveryAddress.PK;
						CartageDeliveryAddressPostCode = CartageDeliveryAddress.OA_PostCode;
					}
				}
			}

			if (CartageDeliveryAddressOverridePK.IsEmpty && !rateEntryXSD.CartageDeliveryAddressPostCode.IsEmpty)
			{
				CartageDeliveryAddressPostCode = rateEntryXSD.CartageDeliveryAddressPostCode;
			}

			OrgHeader consignor = GetCachedOrgFromParentRatingHeader(rateEntryXSD.Consignor, OrganisationTypes.Consignor, context);

			if (consignor != null)
			{
				ConsignorPK = consignor.PK;

				if (rateEntryXSD.CartagePickupAddress.IsSpecified)
				{
					CartagePickupAddress = RateImportHelper.Instance.GetOrgAddress(rateEntryXSD.CartagePickupAddress, consignor);
					if (CartagePickupAddress != null)
					{
						CartagePickupAddressOverridePK = CartagePickupAddress.PK;
						CartagePickupAddressPostCode = CartagePickupAddress.OA_PostCode;
					}
				}
			}

			if (CartagePickupAddressOverridePK.IsEmpty && !rateEntryXSD.CartagePickupAddressPostCode.IsEmpty)
			{
				CartagePickupAddressPostCode = rateEntryXSD.CartagePickupAddressPostCode;
			}

			TransitTime = rateEntryXSD.TransitTime;
			Frequency = rateEntryXSD.Frequency;
			FrequencyUnits = rateEntryXSD.FrequencyUnit;

			if (!CartagePickupAddressOverridePK.IsEmpty)
			{
				CartagePickupAddressPostCode = rateEntryXSD.CartagePickupAddressPostCode;
			}
			CartageDeliveryAddressPostCode = rateEntryXSD.CartageDeliveryAddressPostCode;

			if (rateEntryXSD.ContainerType.IsSpecified)
			{
				RefContainer container = context.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, rateEntryXSD.ContainerType.ContainerCode);

				if (container != null)
				{
					ContainterTypePK = container.PK;
				}
			}

			if (rateEntryXSD.StartDate.IsValid)
			{
				StartDate = rateEntryXSD.StartDate;
			}

			if (rateEntryXSD.EndDate.IsValid)
			{
				EndDate = rateEntryXSD.EndDate;
			}
		}

		OrgHeader GetCachedOrgFromParentRatingHeader(Xsd.Organisation organisation, OrganisationTypes organisationType, IValueObjectImportContext context)
		{
			OrgHeader result = null;
			if (ParentRatingHeader != null && ParentRatingHeader.CachedOrgsForXMLImport.ContainsKey(organisation.EDICode + organisation.OwnerCode))
			{
				result = ParentRatingHeader.CachedOrgsForXMLImport[organisation.EDICode + organisation.OwnerCode];
			}
			else
			{
				result = context.FindOrganisation(organisation, null, organisationType);
				if (ParentRatingHeader != null)
				{
					ParentRatingHeader.CachedOrgsForXMLImport.Add(organisation.EDICode + organisation.OwnerCode, result);
				}
			}
			return result;
		}

		public ZQuery GenerateQuery(ZGuid ratingHeaderPK, RefCommodityCode defaultCommodityCode, bool includeEndDateNull, bool includeEndDateGteStartDate)
		{
			ZQuery query = new ZQuery(RateEntrySchema.TI_TH, ratingHeaderPK);
			query.AddToFilter(RateEntrySchema.TI_RateCategory, Category);
			query.AddToFilter(RateEntrySchema.TI_Mode, Mode);
			query.AddToFilter(RateEntrySchema.TI_OriginLRC, Origin);
			query.AddToFilter(RateEntrySchema.TI_DestinationLRC, Destination);
			query.AddToFilter(RateEntrySchema.TI_IsCrossTrade, IsCrossTrade);
			query.AddToFilter(RateEntrySchema.TI_ViaLRC, Via);

			if (CommodityCode.IsEmpty && defaultCommodityCode != null && defaultCommodityCode.RH_Code != CommodityCode)
			{
				query.AddToFilter(RateEntrySchema.TI_RH_NKCommodityCode, new ZString[] { CommodityCode, defaultCommodityCode.RH_Code });
			}
			else
			{
				query.AddToFilter(RateEntrySchema.TI_RH_NKCommodityCode, CommodityCode);
			}

			query.AddToFilter(RateEntrySchema.TI_RS_NKServiceLevel_NI, ServiceLevel);
			query.AddToFilter(RateEntrySchema.TI_PL_NKCarrierServiceLevel, CarrierServiceLevel);

			SetUniqueIdentifierFilter(RateEntrySchema.TI_OH_TransportProvider, TransportProviderPK, query);
			SetUniqueIdentifierFilter(RateEntrySchema.TI_OH_Supplier, SupplierPK, query);
			SetUniqueIdentifierFilter(RateEntrySchema.TI_OH_Consignee, ConsigneePK, query);
			SetUniqueIdentifierFilter(RateEntrySchema.TI_OH_Consignor, ConsignorPK, query);
			SetUniqueIdentifierFilter(RateEntrySchema.TI_OA_CartagePickupAddressOverride, CartagePickupAddressOverridePK, query);
			SetUniqueIdentifierFilter(RateEntrySchema.TI_OA_CartageDeliveryAddressOverride, CartageDeliveryAddressOverridePK, query);

			query.AddToFilter(RateEntrySchema.TI_TransitTime, TransitTime);
			query.AddToFilter(RateEntrySchema.TI_Frequency, Frequency);
			query.AddToFilter(RateEntrySchema.TI_FrequencyUnit, FrequencyUnits);
			query.AddToFilter(RateEntrySchema.TI_CartagePickupAddressPostCode, CartagePickupAddressPostCode);
			query.AddToFilter(RateEntrySchema.TI_CartageDeliveryAddressPostCode, CartageDeliveryAddressPostCode);

			SetUniqueIdentifierFilter(RateEntrySchema.TI_RC, ContainterTypePK, query);

			if (StartDate.IsValid)
			{
				query.AddToFilter(RateEntrySchema.TI_RateStartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, StartDate);

				if (includeEndDateNull && includeEndDateGteStartDate)
				{
					ZQuery endDateQuery = new ZQuery(RateEntrySchema.TI_RateEndDate, null);
					endDateQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, StartDate);
					query.AddToFilter(endDateQuery, JoinCondition.And);
				}
				else if (includeEndDateGteStartDate)
				{
					query.AddToFilter(RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, StartDate);
				}
				else if (includeEndDateNull)
				{
					query.AddToFilter(RateEntrySchema.TI_RateEndDate, null);
				}
			}

			return query;
		}

		public ZQuery GenerateFullQuery(ZGuid ratingHeaderPK, bool nullEndDate)
		{
			return GenerateQuery(ratingHeaderPK, null, nullEndDate, !nullEndDate);
		}

		#region Implementation

		void SetUniqueIdentifierFilter(SchemaColumn col, ZGuid guid, ZQuery query)
		{
			if (guid.IsEmpty)
			{
				query.AddToFilter(col, null);
			}
			else
			{
				query.AddToFilter(col, guid);
			}
		}

		public static string GetPortCode(string inputValue, BusinessObjectFactory factory)
		{
			string portCode = "";
			if (inputValue.Length == 3)
			{
				RefUNLOCO loco = RefUNLOCO.LoadFromIATA(factory, inputValue);
				if (loco != null)
				{
					portCode = loco.RL_Code;
				}
			}
			else
			{
				portCode = inputValue;
			}
			return portCode;
		}

		readonly IXMLImportOrgCache ParentRatingHeader;

		#endregion
	}
}
