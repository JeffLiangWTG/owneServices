using System;
using System.Text;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer
{
	public class RateEntryValueObjectDataAdapter : ValueObjectDataAdapter<RateEntry, Xsd.RateEntry>
	{
		public RateEntryValueObjectDataAdapter(RatingHeader rateHeader)
		{
			if (rateHeader == null)
			{
				throw new ArgumentNullException(nameof(rateHeader));
			}

			ratingHeader = rateHeader;
		}

		readonly RatingHeader ratingHeader;

		#region Factory

		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}

		BusinessObjectFactory factory;

		#endregion

		#region Properties

		public override string RootElementName
		{
			get { return "RateEntry"; }
		}

		public override string RootCollectionElementName
		{
			get { return "RateEntries"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.SingleRateEntrySchema; }
		}

		protected override bool AllowDifferentImportContextFactory
		{
			get { return true; }
		}

		Type RateEntryType
		{
			get { return ratingHeader.IsQuote() ? typeof(QuoteEntry) : typeof(RateEntry); }
		}

		#endregion

		#region Helpers

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected override RateEntry FindBusinessObject(Xsd.RateEntry rateEntryXSD, IValueObjectImportContext context)
		{
			RateEntry entry = null;
			utility = new RateEntryQueryUtility(ratingHeader);
			utility.SetAllParameters(rateEntryXSD, context);

			var defaultCommodityCode = context.Factory.Load<RefCommodityCode>(Env.Registry.CommodityCode);

			// SQL to count the matches for the various combinations of commodity code and end date in order of priority (1 is highest).
			// The highest priority with only a single match is the entry to return.
			// 1) commodity code and end date >= new start date
			// 2) commodity code and end date null
			// The following is only tried if the given commodity code is blank...
			// 3) default commodity code and end date >= new start date
			// 4) default commodity code and end date null
			// Note, the end date filter is not included if the given start date is empty.
			ZQuery anyQuery = utility.GenerateQuery(ratingHeader.PK, defaultCommodityCode, true, true);
			const string CommodityCodeParam = "@CommodityCode";
			const string DefaultCommodityCodeParam = "@DefCommodityCode";
			var sqlBuilder = new StringBuilder();
			sqlBuilder.Append((NoResString)"select "); // Is part of an SQL expression.
			AddResultColumns(sqlBuilder, CommodityCodeParam, rateEntryXSD.StartDate);

			bool tryMatchDefaultCommodityCode = utility.CommodityCode.IsEmpty && defaultCommodityCode != null && defaultCommodityCode.RH_Code != utility.CommodityCode;
			if (tryMatchDefaultCommodityCode)
			{
				sqlBuilder.Append(", ");
				AddResultColumns(sqlBuilder, DefaultCommodityCodeParam, rateEntryXSD.StartDate);
			}
			sqlBuilder.Append((NoResString)"from "); // Is part of an SQL expression.
			sqlBuilder.Append(RateEntrySchema.Constants.TableName);
			sqlBuilder.Append(" where ");
			sqlBuilder.Append(anyQuery.ParameterisedText.ParameterisedQueryText);

			int commodityAndDateCount;
			int commodityAndDateNullCount;
			int defaultCommodityAndDateCount = 0;
			int defaultCommodityAndDateNullCount = 0;
			var connection = ((CargoWise.Data.IDbConnected)context.Factory).Connection;
			using (var cmd = connection.Command(sqlBuilder.ToString()))
			{
				cmd.AddParameterBasedOnDbColumn(CommodityCodeParam, (string)utility.CommodityCode, RateEntrySchema.TI_RH_NKCommodityCode);
				if (tryMatchDefaultCommodityCode)
				{
					cmd.AddParameterBasedOnDbColumn(DefaultCommodityCodeParam, (string)defaultCommodityCode.RH_Code, RateEntrySchema.TI_RH_NKCommodityCode);
				}
				cmd.AddParameters(anyQuery.ParameterisedText.Parameters);

				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
					int columnIndex = 0;
					commodityAndDateCount = reader.GetInt32(columnIndex++);
					commodityAndDateNullCount = reader.GetInt32(columnIndex++);
					if (tryMatchDefaultCommodityCode)
					{
						defaultCommodityAndDateCount = reader.GetInt32(columnIndex++);
						defaultCommodityAndDateNullCount = reader.GetInt32(columnIndex++);
					}
				}
			}

			bool commodityCodeMatches = commodityAndDateCount == 1 || commodityAndDateNullCount == 1;
			if (commodityCodeMatches || defaultCommodityAndDateCount == 1 || defaultCommodityAndDateNullCount == 1)
			{
				bool nullEndDate;
				if (commodityCodeMatches)
				{
					nullEndDate = commodityAndDateCount != 1;
				}
				else
				{
					nullEndDate = defaultCommodityAndDateCount != 1;
					utility.CommodityCode = defaultCommodityCode.RH_Code;
				}

				ZQuery query = utility.GenerateFullQuery(ratingHeader.PK, nullEndDate);
				entry = (RateEntry)context.Factory.LoadTop1(RateEntryType, query);
			}

			return entry;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Is part of an SQL expression.")]
		void AddResultColumns(StringBuilder sqlBuilder, string commodityCodeParam, ZDateTime startDate)
		{
			string startQueryPart = (NoResString)"ISNULL(SUM(CASE when " + RateEntrySchema.Constants.TI_RH_NKCommodityCode + (NoResString)" = " + commodityCodeParam;
			const string endQueryPart = " then 1 else 0 end), 0)";

			sqlBuilder.Append(startQueryPart);
			if (startDate.IsValid)
			{
				sqlBuilder.Append((NoResString)" AND " + RateEntrySchema.Constants.TI_RateEndDate + (NoResString)" is not null");
			}
			sqlBuilder.Append(endQueryPart);
			sqlBuilder.AppendLine(",");

			if (startDate.IsValid)
			{
				sqlBuilder.Append(startQueryPart);
				sqlBuilder.Append((NoResString)" AND " + RateEntrySchema.Constants.TI_RateEndDate + (NoResString)" is null");
				sqlBuilder.AppendLine(endQueryPart);
			}
			else
			{
				// We only need one result column if we're not filtering on date.
				// Just make a dummy second column.
				sqlBuilder.AppendLine("0");
			}
		}

		protected override RateEntry NewBusinessObject(Xsd.RateEntry rateEntryXSD, IValueObjectImportContext context)
		{
			RateEntry entry = ratingHeader.EntryCollections[rateEntryXSD.Category.ToString()].LazyLoadingCollection.AddNew();
			entry.SuspendSettingRateLineTariff = true;
			return entry;
		}

		bool IsValidCartagePickUpOrDeliveryAddress(OrgAddress orgAddress, OrgHeader organisation)
		{
			return (orgAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery) ||
					orgAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office) ||
					(organisation.OH_IsConsignee && orgAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Delivery)) ||
					(organisation.OH_IsConsignor && orgAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Pickup)));
		}

		RateEntryQueryUtility utility;

		#endregion

		#region Import

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			notifications.Notify(new BusinessObjectCreatedOrUpdatedNotification(bizObj) { UpdateRecordCountOnlyWithoutMessage = true });
		}

		protected override void ImportFromValueObjectCore(RateEntry ratingEntry, Xsd.RateEntry rateEntryXSD, IValueObjectImportContext context)
		{
			string originCode = RateEntryQueryUtility.GetPortCode(rateEntryXSD.Origin, Factory);
			string destinationCode = RateEntryQueryUtility.GetPortCode(rateEntryXSD.Destination, Factory);
			string viaCode = RateEntryQueryUtility.GetPortCode(rateEntryXSD.Via, Factory);

			context.SetPropertyInfoValue(ratingEntry.TI_ModeInfo, rateEntryXSD.Mode, rateEntryXSD.ModeSpecified);
			context.SetPropertyInfoValue(ratingEntry.TI_RateCategoryInfo, rateEntryXSD.Category, rateEntryXSD.CategorySpecified);

			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_RateStartDateInfo, (ZDate)rateEntryXSD.StartDate);
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_RateEndDateInfo, (ZDate)rateEntryXSD.EndDate);
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_OriginLRCInfo, originCode, GetForeignKeyType(originCode));
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_DestinationLRCInfo, destinationCode, GetForeignKeyType(destinationCode));
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_ViaLRCInfo, viaCode, GetForeignKeyType(viaCode));
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_RX_NKCurrencyInfo, rateEntryXSD.Currency, ForeignKeyType.CurrencyNK, Res.GetString("fd255d00-a4c7-4d24-8e13-b4ab66df9cc8", "Rating Entry Currency"));
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_TransitTimeInfo, rateEntryXSD.TransitTime);
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_FrequencyInfo, rateEntryXSD.Frequency.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_FrequencyUnitInfo, rateEntryXSD.FrequencyUnit);

			ImportOrganisationRelatedFields(ratingEntry, rateEntryXSD, context);

			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_RH_NKCommodityCodeInfo, rateEntryXSD.CommodityCode);
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_RS_NKServiceLevel_NIInfo, rateEntryXSD.ServiceLevel, ForeignKeyType.RefServiceLevelNK);
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_PL_NKCarrierServiceLevelInfo, rateEntryXSD.CarrierServiceLevel);
			context.SetPropertyInfoValueIfValueNotEmpty(ratingEntry.TI_ContractNumberInfo, rateEntryXSD.ContractNumber);

			if (rateEntryXSD.IsCrossTradeSpecified)
			{
				ratingEntry.TI_IsCrossTrade = (rateEntryXSD.IsCrossTrade == Xsd.TrueFalse.@true);
			}

			if (rateEntryXSD.RateLines.Count > 0)
			{
				ImportRateLines(rateEntryXSD.RateLines, ratingEntry, context);
			}

			new ContainerValueObjectHelper(context).ImportContainerType(ratingEntry.TI_RCInfo, rateEntryXSD.ContainerType);

			if (rateEntryXSD.MatchContainerClassSpecified)
			{
				ratingEntry.TI_MatchContainerRateClass = rateEntryXSD.MatchContainerClass == Xsd.TrueFalse.@true;
			}
		}

		ForeignKeyType GetForeignKeyType(string code)
		{
			if (code.Length == 2)
			{
				return ForeignKeyType.CountryNK;
			}

			if (code.Length == 4)
			{
				return ForeignKeyType.IntZoneNK;
			}

			return ForeignKeyType.PortNK;
		}

		void ImportRateLines(Xsd.RateLineCollection rateLinesXSD, RateEntry ratingEntry, IValueObjectImportContext context)
		{
			var dataAdapter = new RateLineValueObjectDataAdapter(ratingEntry);

			foreach (Xsd.RateLine rateLineXSD in rateLinesXSD)
			{
				dataAdapter.CreateOrUpdateFromValueObject(rateLineXSD, context);
			}
		}

		void ImportOrganisationRelatedFields(RateEntry rateEntry, Xsd.RateEntry rateEntryXSD, IValueObjectImportContext context)
		{
			if (utility == null)
			{
				utility = new RateEntryQueryUtility();
				utility.SetAllParameters(rateEntryXSD, context);
			}

			rateEntry.TI_OH_Supplier = utility.SupplierPK;
			rateEntry.TI_OH_Consignee = utility.ConsigneePK;
			rateEntry.TI_OH_Consignor = utility.ConsignorPK;
			rateEntry.TI_OH_TransportProvider = utility.TransportProviderPK;

			bool hasCartageAddressError = false;

			if (rateEntry.Consignee != null && rateEntryXSD.CartageDeliveryAddress.IsSpecified)
			{
				OrgAddress cartageDeliveryAddress = utility.CartageDeliveryAddress;

				if (cartageDeliveryAddress != null)
				{
					if (IsValidCartagePickUpOrDeliveryAddress(cartageDeliveryAddress, rateEntry.Consignee))
					{
						rateEntry.TI_OA_CartageDeliveryAddressOverride = cartageDeliveryAddress.PK;
						rateEntry.TI_CartageDeliveryAddressPostCode = cartageDeliveryAddress.OA_PostCode;
					}
					else
					{
						ZString errorMesg = Res.GetString("3836d37d-b2b6-4a96-9f20-a6ce3a6c5cea", "Address({0}) is not a Valid Office or Delivery Address. Please Update the 'Address Capabilities' in Organization.", cartageDeliveryAddress.OA_Code);
						context.Notify(new ErrorNotification(RateErrorType.InvalidOrgAddressType, errorMesg));
						hasCartageAddressError = true;
					}
				}
				else
				{
					context.Notify(new ErrorNotification(RateErrorType.UnmatchOrgAddress, Res.GetString("b687db79-889f-4708-b957-b840cbef0868", "Delivery Address Does not Belong to Consignee")));
					hasCartageAddressError = true;
				}
			}

			if (rateEntry.CartageDeliveryAddressOverride == null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(rateEntry.TI_CartageDeliveryAddressPostCodeInfo, rateEntryXSD.CartageDeliveryAddressPostCode);
			}

			if (rateEntry.Consignor != null && rateEntryXSD.CartagePickupAddress.IsSpecified)
			{
				OrgAddress cartagePickupAddress = utility.CartagePickupAddress;

				if (cartagePickupAddress != null)
				{
					if (IsValidCartagePickUpOrDeliveryAddress(cartagePickupAddress, rateEntry.Consignor))
					{
						rateEntry.TI_OA_CartagePickupAddressOverride = cartagePickupAddress.PK;
						rateEntry.TI_CartagePickupAddressPostCode = cartagePickupAddress.OA_PostCode;
					}
					else
					{
						ZString errorMesg = Res.GetString("ca7cb329-7bfa-42e4-903a-9d5352e7b926", "Address({0}) is not a Valid Office or Pickup Address. Please Update the 'Address Capabilities' in Organization.", cartagePickupAddress.OA_Code);
						context.Notify(new ErrorNotification(RateErrorType.InvalidOrgAddressType, errorMesg));
						hasCartageAddressError = true;
					}
				}
				else
				{
					context.Notify(new ErrorNotification(RateErrorType.UnmatchOrgAddress, Res.GetString("9232cc6d-5733-49bb-95a5-d89f58ca1464", "Pickup Address Does not Belong To Consignor")));
					hasCartageAddressError = true;
				}
			}

			if (rateEntry.CartagePickupAddressOverride == null)
			{
				context.SetPropertyInfoValueIfValueNotEmpty(rateEntry.TI_CartagePickupAddressPostCodeInfo, rateEntryXSD.CartagePickupAddressPostCode);
			}

			if (hasCartageAddressError)
			{
				context.Notify(new ErrorNotification(RateErrorType.DataErrorPreventSave, Res.GetString("4cb8527b-55b1-4a47-80ce-8388e1397dcc", "Please Fix the Address Error(s) before continuing")));
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateEntry bizObj, Xsd.RateEntry rateEntryXSD, IValueObjectExportContext context)
		{
			ZString errorContext = "";

			rateEntryXSD.Mode = bizObj.TI_Mode;
			rateEntryXSD.Category = bizObj.TI_RateCategory;
			rateEntryXSD.StartDate = bizObj.TI_RateStartDate;
			rateEntryXSD.EndDate = bizObj.TI_RateEndDate;
			rateEntryXSD.Currency = bizObj.TI_RX_NKCurrency;
			rateEntryXSD.TransitTime = bizObj.TI_TransitTime;
			rateEntryXSD.CommodityCode = bizObj.TI_RH_NKCommodityCode;
			rateEntryXSD.CarrierServiceLevel = bizObj.TI_PL_NKCarrierServiceLevel;
			rateEntryXSD.ContractNumber = bizObj.TI_ContractNumber;

			if (!bizObj.TI_Frequency.IsEmpty)
			{
				rateEntryXSD.Frequency = bizObj.TI_Frequency;
			}

			rateEntryXSD.FrequencyUnit = bizObj.TI_FrequencyUnit;

			ExportOrganisationRelatedFields(bizObj, rateEntryXSD, errorContext, context);

			rateEntryXSD.ServiceLevel = bizObj.TI_RS_NKServiceLevel_NI;

			rateEntryXSD.Origin = bizObj.TI_OriginLRC;
			rateEntryXSD.Destination = bizObj.TI_DestinationLRC;

			if (bizObj.Via() != null)
			{
				rateEntryXSD.Via = bizObj.TI_ViaLRC;
			}

			if (!bizObj.TI_IsCrossTrade.IsEmpty)
			{
				rateEntryXSD.IsCrossTrade = bizObj.TI_IsCrossTrade ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			}

			if (bizObj.RateLines.Count > 0)
			{
				ExportRateLines(bizObj, rateEntryXSD, context);
			}

			if (bizObj.Container != null)
			{
				rateEntryXSD.ContainerType = new ContainerTypeValueObjectDataAdapter().ExportToValueObject(bizObj.Container, context);
			}

			if (!bizObj.TI_MatchContainerRateClass.IsEmpty)
			{
				rateEntryXSD.MatchContainerClass = bizObj.TI_MatchContainerRateClass ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			}
		}

		void ExportOrganisationRelatedFields(RateEntry ratingEntry, Xsd.RateEntry rateEntryXSD, ZString errorContext, IValueObjectExportContext context)
		{
			rateEntryXSD.ServiceProvider = ratingEntry.TI_OH_Supplier.IsEmpty ? null : new OrganisationValueObjectDataAdapter().ExportToValueObject(ratingEntry.Supplier, context);
			rateEntryXSD.TransportProvider = ratingEntry.TI_OH_TransportProvider.IsEmpty ? null : new OrganisationValueObjectDataAdapter().ExportToValueObject(ratingEntry.TransportProvider, context);
			rateEntryXSD.Consignee = ratingEntry.TI_OH_Consignee.IsEmpty ? null : new OrganisationValueObjectDataAdapter().ExportToValueObject(ratingEntry.Consignee, context);
			rateEntryXSD.Consignor = ratingEntry.TI_OH_Consignor.IsEmpty ? null : new OrganisationValueObjectDataAdapter().ExportToValueObject(ratingEntry.Consignor, context);

			if (ratingEntry.Consignee != null && ratingEntry.CartageDeliveryAddressOverride != null)
			{
				rateEntryXSD.CartageDeliveryAddress = new AddressValueObjectHelper(errorContext).ToAddressReference(ratingEntry.CartageDeliveryAddressOverride, context);
			}

			if (ratingEntry.CartageDeliveryAddressOverride == null)
			{
				rateEntryXSD.CartageDeliveryAddressPostCode = ratingEntry.TI_CartageDeliveryAddressPostCode;
			}

			if (ratingEntry.Consignor != null && ratingEntry.CartagePickupAddressOverride != null)
			{
				rateEntryXSD.CartagePickupAddress = new AddressValueObjectHelper(errorContext).ToAddressReference(ratingEntry.CartagePickupAddressOverride, context);
			}

			if (ratingEntry.CartagePickupAddressOverride == null)
			{
				rateEntryXSD.CartagePickupAddressPostCode = ratingEntry.TI_CartagePickupAddressPostCode;
			}
		}

		void ExportRateLines(RateEntry ratingEntry, Xsd.RateEntry rateEntryXSD, IValueObjectExportContext context)
		{
			var dataAdapter = new RateLineValueObjectDataAdapter(ratingEntry);

			foreach (RateLine rateLine in ratingEntry.RateLines)
			{
				Xsd.RateLine rateLineXSD = rateEntryXSD.RateLines.AddNew();
				dataAdapter.ExportToValueObject(rateLine, rateLineXSD, context);
			}
		}

		#endregion
	}
}

