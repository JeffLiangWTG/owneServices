using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.ASYCUDA;
using GenPivot = Enterprise.ZArchitecture.Business.GenPivot;
using RegsitrationNumberDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public delegate void AdditionalWriterSettingAction(DeclarationDataObjectWriter writer);

	public class DeclarationDataObjectWriterConfiguration
	{
		public ZBool ShouldPopulateAttachedDocumentCollection { get; set; } = ZBool.False;
		public IEnumerable<ZGuid> EntryHeaderPKsToPopulate { get; set; } = Enumerable.Empty<ZGuid>();

		public AdditionalWriterSettingAction AdditionalWriterSettingActions { get; set; }
	}

	public static class Extensions
	{
		public static Shipment GetUniversalShipment(this BaseJobDeclaration declaration
			, DeclarationDataObjectWriterConfiguration writerConfiguration = null
			, RecipientRoleType roleType = RecipientRoleType.ORP
			, DataContextType? filteredDataContextType = null)
		{
			return declaration.GetUniversalShipmentCore(roleType, filteredDataContextType, writer =>
			{
				if (writerConfiguration != null)
				{
					writer.ShouldPopulateAttachedDocumentCollection = writerConfiguration.ShouldPopulateAttachedDocumentCollection;
					if (writerConfiguration.EntryHeaderPKsToPopulate.Any())
					{
						writer.SetEntryHeaderPKsToPopulate(writerConfiguration.EntryHeaderPKsToPopulate);
					}
					writerConfiguration.AdditionalWriterSettingActions?.Invoke(writer);
				}
			});
		}

		static Shipment GetUniversalShipmentCore(this BaseJobDeclaration declaration
			, RecipientRoleType roleType = RecipientRoleType.ORP
			, DataContextType? filteredDataContextType = null
			, Action<DeclarationDataObjectWriter> writerConfigurator = null)
		{
			Shipment result = null;
			if (declaration != null)
			{
				var manager = (IShipmentDataContextManager)declaration.GetUniversalDataContextManager();
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(roleType, declaration)) { FilteredDataContextType = filteredDataContextType });
				if (writer is DeclarationDataObjectWriter declarationDataObjectWriter)
				{
					writerConfigurator?.Invoke(declarationDataObjectWriter);
					using (((IExternalFetchHintSupporter)declaration.Factory).SetupCreator())
					{
						result = declarationDataObjectWriter.GetDataObject(declaration);
					}
				}
			}

			return result;
		}

		public static bool IsEntryNumberPlaceHolderType(this UniversalDataBuss.DataObjects.Universal.EntryNumber entryNumber)
		{
			return entryNumber != null && entryNumber.CountryOfIssue == null && entryNumber.Type.GetCodeAsUpperCase() == Constants.EntryNumberPlaceHolderType;
		}

		public static AddInfo AddOrUpdate(this List<AddInfo> addInfos, ZString key, ZString value, bool removeIfEmpty = true)
		{
			AddInfo result = null;
			if (addInfos != null)
			{
				result = addInfos.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
				if (removeIfEmpty && value.IsEmpty)
				{
					if (result != null)
					{
						addInfos.Remove(result);
						result = null;
					}
				}
				else
				{
					if (result == null)
					{
						addInfos.Add(result = new AddInfo() { Key = key });
					}
					result.Value = value;
				}
			}
			return result;
		}

		public static AddInfo AddIfMissing(this List<AddInfo> addInfos, ZString key, ZString value)
		{
			AddInfo result = null;
			if (addInfos != null)
			{
				result = addInfos.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
				if (result == null)
				{
					addInfos.Add(result = new AddInfo() { Key = key, Value = value });
				}
			}
			return result;
		}

		public static ZString? GetColoadBill(this Shipment dataObject, Shipment hvlvShipment)
		{
			ZString? result = null;

			if (hvlvShipment != null)
			{
				var wayBillType = hvlvShipment.WayBillType?.Code.GetValueOrDefault();
				if (wayBillType.HasValue && wayBillType.Value == WayBillTypeList.Codes.House)
				{
					result = hvlvShipment.WayBillNumber.GetValueOrDefault();
				}
			}
			else
			{
				ZString? masterHouse = null;
				if (dataObject.AdditionalBillCollection != null)
				{
					var additionalBill = dataObject.AdditionalBillCollection.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.MasterHouse && FormatMasterBill(x.ParentBillNumber.GetValueOrDefault()) == dataObject.GetMasterBill());
					if (additionalBill != null)
					{
						masterHouse = additionalBill.BillNumber.GetValueOrDefault();
					}
				}
				if (!masterHouse.HasValue)
				{
					var shipmentType = dataObject.ShipmentType.GetCodeAsUpperCase();
					if (shipmentType == Core.Constants.AgentType.CoLoad)
					{
						masterHouse = dataObject.BookingConfirmationReference.GetValueOrDefault();
					}
				}
				if (masterHouse.HasValue)
				{
					result = masterHouse.Value;
				}
			}

			return result;
		}

		static ZString FormatMasterBill(ZString masterBill)
		{
			return masterBill.Replace(" ", "").Replace("-", "");
		}

		public static ZString GetMasterBill(this Shipment dataObject)
		{
			return FormatMasterBill(dataObject.WayBillNumber.GetValueOrDefault());
		}

		public static ZDateTime? GetLoadingDateForAir(this Shipment shipment, TransportLeg relatedFlight)
		{
			return shipment.GetDateForFreight(DateType.LoadingDate, x => x.ActualDeparture, x => x.EstimatedDeparture, relatedFlight);
		}

		public static ZDateTime? GetLoadingDateForSea(this Shipment shipment, TransportLeg relatedVoyage)
		{
			return shipment.GetDateForFreight(DateType.Departure, x => x.ActualDeparture, x => x.EstimatedDeparture, relatedVoyage);
		}

		public static ZDateTime? GetDischargeDateForAir(this Shipment shipment, TransportLeg relatedFlight)
		{
			return shipment.GetDateForFreight(DateType.DischargeDate, x => x.ActualArrival, x => x.EstimatedArrival, relatedFlight);
		}

		public static ZDateTime? GetDischargeDateForSea(this Shipment shipment, TransportLeg relatedVoyage)
		{
			return shipment.GetDateForFreight(DateType.DischargeDate, x => x.ActualArrival, x => x.EstimatedArrival, relatedVoyage);
		}

		static ZDateTime? GetDateForFreight(this Shipment shipment, DateType dateType, Func<TransportLeg, ZDateTime?> getActualDate, Func<TransportLeg, ZDateTime?> getEstimatedDate, TransportLeg relatedTransportLeg)
		{
			ZDateTime? result = null;
			Date dateObject = null;
			if (shipment.DateCollection != null)
			{
				dateObject = shipment.DateCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == dateType && !x.IsEstimate.GetValueOrDefault());
				if (dateObject == null || dateObject?.Value == ZDateTime.Empty)
				{
					dateObject = shipment.DateCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == dateType && x.IsEstimate.GetValueOrDefault());
				}
			}
			if (dateObject != null && dateObject?.Value != ZDateTime.Empty)
			{
				result = dateObject.Value;
			}
			else if (relatedTransportLeg != null)
			{
				var actualDate = getActualDate(relatedTransportLeg);
				result = actualDate.HasValue && !actualDate.GetValueOrDefault().IsEmpty ? actualDate : getEstimatedDate(relatedTransportLeg);
			}
			return result;
		}

		public static IUniversalCustomsDataObjectProvider GetApplicationSpecificUniversalCustomsDataObjectProvider(this BusinessObjectFactory factory, string applicationCode)
		{
			return factory.GetCachedValue($"{applicationCode}-IUniversalCustomsDataObjectProvider.ApplicationSpecific", () =>
			{
				var types = ObjectFactory.Get<Hashtable>("UniversalCustomsDataObjectProviders.ApplicationSpecific");
				var objectHandle = (ObjectHandle)types[applicationCode];
				return (IUniversalCustomsDataObjectProvider)objectHandle?.GetObject();
			});
		}

		public static IUniversalCustomsDataObjectProvider GetUniversalCustomsDataObjectProvider(this BusinessObjectFactory factory, string countryCode)
		{
			return factory.GetCachedValue($"{countryCode}-IUniversalCustomsDataObjectProvider", () =>
			{
				var types = ObjectFactory.Get<Hashtable>("UniversalCustomsDataObjectProviders");
				var objectHandle = (ObjectHandle)types[countryCode];
				switch (objectHandle)
				{
					case null when ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryCode):
						objectHandle = (ObjectHandle)types[Core.Constants.CountryCodes.EuropeanUnion];
						break;
					case null when ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(countryCode):
						objectHandle = (ObjectHandle)types["AsycudaCustoms"];
						break;
				}
				return (IUniversalCustomsDataObjectProvider)objectHandle?.GetObject();
			});
		}

		public static RefUNLOCOCollection GetRefUNLOCOList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Universal|RefUNLOCOCollection", () => new RefUNLOCOCollection(factory));
		}

		public static RefCountryCollection GetRefCountryList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Universal|RefCountryCollection", () => new RefCountryCollection(factory));
		}

		public static OrgAddress GetMatchedUsingCodes(this OrganizationAddress orgAddressData, BusinessObjectFactory factory)
		{
			ZString orgCode = orgAddressData.OrganizationCode.GetValueOrDefault();
			if (!orgCode.IsEmpty)
			{
				var organisation = factory.LoadFromUniqueKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgCode);
				if (organisation != null)
				{
					var addressCode = orgAddressData.AddressShortCode.GetValueOrDefault();
					if (!addressCode.IsEmpty)
					{
						var addressQuery = new ZQuery(OrgAddressSchema.OA_OH, organisation.PK);
						addressQuery.AddToFilter(OrgAddressSchema.OA_Code, addressCode);
						var orgAddress = factory.LoadTop1<OrgAddress>(addressQuery);
						if (orgAddress != null)
						{
							return orgAddress;
						}
					}

					return organisation.MainAddress;
				}
			}
			return null;
		}

		public static ZString GetCountryCodeSafe(this IBranchProvider provider)
		{
			var company = provider != null && provider.Branch != null ? provider.Branch.Company : null;
			return company != null ? company.GC_RN_NKCountryCode : ZString.Empty;
		}

		public static OrganizationAddress AddDummyOrganizationForCodeOnly(this IOrganizationAddressCollectionParent collectionParent, IDataObjectWriterStrategy writerStrategy, DocAddressType addressType, ZString companyName, RefCountry countryOfIssue, ZString codeType, ZString codeValue)
		{
			OrganizationAddress result = null;
			if (collectionParent != null && !codeValue.IsEmpty && !codeValue.IsEmpty && countryOfIssue != null)
			{
				result = new OrganizationAddress(writerStrategy)
				{
					AddressType = addressType.ToString(),
				};
				result.SetRegistrationNumberCollection(() => new List<RegsitrationNumberDataObject>()
				{
					new RegsitrationNumberDataObject()
					{
						CountryOfIssue = Country.New(countryOfIssue),
						Type = ListHelper.GetWithDescription<RegistrationNumberType>(codeType, countryOfIssue.Factory.GetCachedValue(countryOfIssue.Code + "_OrgCodeLists", () => { return new OrgCodeLists().CustomsCodes_List(countryOfIssue); })),
						Value = codeValue
					}
				});
				if (!companyName.IsEmpty)
				{
					result.CompanyName = companyName;
				}
				collectionParent.SetOrganizationAddressCollection(() => collectionParent.OrganizationAddressCollection ?? new List<OrganizationAddress>());
				collectionParent.OrganizationAddressCollection?.Add(result);
			}
			return result;
		}

		public static bool IsUSCATAIRMessageEvent(this Event eventDataObject, DataContextType dataContextType)
		{
			var dataContext = eventDataObject.DataContext;
			return dataContext != null
				   && dataContext.ActionPurposeCode == Constants.CATAIRMessage.ActionPurposeCode
				   && dataContext.DataProviderForCodeMapping == Constants.CATAIRMessage.DataProvider
				   && dataContext.DataTargetCollection != null
				   && dataContext.DataTargetCollection.Any(x => x.Type.GetValueOrDefault() == dataContextType.ToString());
		}

		public static bool IsAirImportCustomsMessageEvent(this Event eventDataObject)
		{
			var eventValueObject = (IXmlEventValueObject)eventDataObject;
			var masterbill = eventValueObject.Context.MAWBNumber;
			var dataContext = eventDataObject.DataContext;
			var eventParameters = eventDataObject.EventParameters;
			return dataContext != null
				&& eventParameters != null
				&& eventDataObject.EventType.GetValueOrDefault() == Enterprise.ZArchitecture.Business.Events.ReceivedCode
				&& eventParameters.Department.GetValueOrDefault() == Constants.JobDeclarationUniversalDepartment.Customs
				&& eventParameters.Quantity.GetValueOrDefault() > 0
				&& !masterbill.IsEmpty;
		}

		public static bool IsHVLV(this Shipment universalShipment)
		{
			var result = false;
			var dataSourceCollectionTypes = universalShipment.DataContext?.DataSourceCollection?.Select(x => x.Type).ToHashSet();
			if (dataSourceCollectionTypes != null
				&& dataSourceCollectionTypes.Contains(nameof(DataContext.ForwardingConsol))
				&& dataSourceCollectionTypes.Contains(nameof(DataContext.ForwardingShipment)))
			{
				result = universalShipment.SubShipmentCollection?.SingleOrDefault()?.ShipmentType.GetCodeAsUpperCase().Equals(ShipmentTypes.HighVolumeLowValue) ?? false;
			}
			return result;
		}

		public static bool IsTWH(this Shipment universalShipment)
		{
			var dataSourceCollectionTypes = universalShipment.DataContext?.DataSourceCollection?.Select(x => x.Type).ToHashSet();
			return dataSourceCollectionTypes != null && (dataSourceCollectionTypes.Contains(nameof(DataContextType.TransitReceiveHeader)) || dataSourceCollectionTypes.Contains(nameof(DataContextType.TransitReceive)));
		}

		public static CommercialInvoiceLine FindMatchingCommercialInvoiceLine(this PackedItem packedItem, Shipment dataObject)
		{
			return dataObject
				.CommercialInfo?
				.CommercialInvoiceCollection?
				.SelectMany(invoices => invoices.CommercialInvoiceLineCollection)?
				.FirstOrDefault(line => line.Link == packedItem?.CommercialInvoiceLineLink);
		}

		public static TCustomsJob FindRelatedCustomsJobFromHVLVShipment<TCustomsJob>(this Shipment universalShipment, BusinessObjectFactory factory) where TCustomsJob : BusinessObject
		{
			var result = default(TCustomsJob);

			if (universalShipment.IsHVLV())
			{
				var shipmentDataObject = universalShipment.SubShipmentCollection.First(subShipment => subShipment.ShipmentType != null && subShipment.ShipmentType.Code.Value == ShipmentTypes.HighVolumeLowValue);
				var shipmentJobNumber = shipmentDataObject.DataContext?.DataSourceCollection?.FirstOrDefault(dataSource => dataSource.Type.Equals(nameof(DataContextType.ForwardingShipment)))?.Key;

				if (!string.IsNullOrEmpty(shipmentJobNumber))
				{
					var shipment = factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentJobNumber));
					if (shipment != null && shipment.HVLVConsignmentHeader is BusinessObject hvlvConsignmentHeader)
					{
						var relation2TableCode = BusinessObjectFactory.GetTableCodeFromType(typeof(TCustomsJob));
						var applicationCode = universalShipment.MessagingApplicationCode?.Code ?? default(ZString?);
						var manifestType = universalShipment.MessageType?.Code ?? default(ZString?);

						var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.HighVolumeLowValue);
						pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, HVLVConsignmentHeaderSchema.Constants.Prefix);
						pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, hvlvConsignmentHeader.PK);
						pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, relation2TableCode);

						if (applicationCode.HasValue || manifestType.HasValue)
						{
							var relatedCustomsJobPivots = factory.Load<GenPivot>(pivotQuery);
							foreach (var pivot in relatedCustomsJobPivots)
							{
								if (pivot.Relation2Object is IAsycudaManifestHeader job
									&& (!applicationCode.HasValue || job.AMA_ApplicationCode == applicationCode.Value)
									&& (!manifestType.HasValue || job.AMA_ManifestType == manifestType.Value))
								{
									result = job as TCustomsJob;
									break;
								}
							}
						}
						else
						{
							var relatedCustomsJobPivot = factory.LoadTop1<GenPivot>(pivotQuery);
							if (relatedCustomsJobPivot != null)
							{
								result = relatedCustomsJobPivot.Relation2Object as TCustomsJob;
							}
						}
					}
				}
			}

			return result;
		}
	}
}
