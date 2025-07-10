using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveHeaderLookups : US.Business.CusInBondMoveHeaderLookups
	{
		public CusInBondMoveHeaderLookups(CusInBondMoveHeader parent)
			: base(parent)
		{
		}

		protected new CusInBondMoveHeader Parent
		{
			get { return (CusInBondMoveHeader)base.Parent; }
		}

		public override ICodeDescriptionPairList MessageStatusList
		{
			get { return Factory.GetCachedValue<ImportMessageStatusList>(); }
		}

		public CodeDescriptionPairList InbondQPMessageStatusList => Factory.GetCachedValue("USInbondQPMessageStatusList", GenerateInbondQPMessageStatusList);

		public CodeDescriptionPairList InbondWPMessageStatusList => Factory.GetCachedValue("USInbondWPMessageStatusList", GenerateInbondWPMessageStatusList);

		public static CodeDescriptionPairList GenerateInbondQPMessageStatusList()
		{
			var result = new MessageStatusListIT();
			result.RemoveCode(ImportMessageStatusList.Codes.AwaitingArrival);
			result.RemoveCode(ImportMessageStatusList.Codes.AwaitingExportation);
			result.RemoveCode(ImportMessageStatusList.Codes.AwaitingTransferOfLiability);
			result.RemoveCode(ImportMessageStatusList.Codes.ClearArrival);
			result.RemoveCode(ImportMessageStatusList.Codes.ClearExportation);
			result.RemoveCode(ImportMessageStatusList.Codes.ClearTransferOfLiability);
			result.RemoveCode(ImportMessageStatusList.Codes.ErrorArrival);
			result.RemoveCode(ImportMessageStatusList.Codes.ErrorExportation);
			result.RemoveCode(ImportMessageStatusList.Codes.ErrorTransferOfLiability);
			return result;
		}

		public static CodeDescriptionPairList GenerateInbondWPMessageStatusList()
		{
			var result = new MessageStatusListIT();
			result.RemoveCode(ImportMessageStatusList.Codes.AwaitingDepartureAmendment);
			result.RemoveCode(ImportMessageStatusList.Codes.AwaitingDepartureOriginal);
			result.RemoveCode(ImportMessageStatusList.Codes.AwaitingDepartureWithdraw);
			result.RemoveCode(ImportMessageStatusList.Codes.ClearDepartureAmendment);
			result.RemoveCode(ImportMessageStatusList.Codes.ClearDepartureOriginal);
			result.RemoveCode(ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			result.RemoveCode(ImportMessageStatusList.Codes.ClearDeparturePartialAmendment);
			result.RemoveCode(ImportMessageStatusList.Codes.ClearDeparturePartialOriginal);
			result.RemoveCode(ImportMessageStatusList.Codes.ClearDeparturePartialWithdraw);
			result.RemoveCode(ImportMessageStatusList.Codes.ErrorDepartureAmendment);
			result.RemoveCode(ImportMessageStatusList.Codes.ErrorDepartureOriginal);
			result.RemoveCode(ImportMessageStatusList.Codes.ErrorDepartureWithdraw);
			return result;
		}

		public WarehouseTransactionStatusList WarehouseTransactionStatusList
		{
			get { return Factory.GetCachedValue<WarehouseTransactionStatusList>(); }
		}

		public USCarrierCombinedCollection SplitCarrierCollection
		{
			get
			{
				var result = new USCarrierCombinedCollection(Factory);
				if (Parent.Header != null && !Parent.Header.BH_ImportTransportMode.IsEmpty)
				{
					result.AdditionalFilter = new ZQuery(USCCarrierSchema.UI_ModeOfTransportation, SQLComparisonOperator.StartsWith, Parent.Header.BH_ImportTransportMode.SubstringSafe(0, 1));
				}
				return result;
			}
		}

		public ThreeLetterRefAirlineCollection AirlineCollection
		{
			get
			{
				var airlineCollection = new ThreeLetterRefAirlineCollection(Factory);

				airlineCollection.AdditionalFilter = new ZQuery(RefAirlineSchema.RM_ThreeLetterCode, SQLComparisonOperator.NotEqual, "");
				airlineCollection.AdditionalFilter.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, "");

				airlineCollection.AddNotificationWhenAdditionalFilterNotMetOverride = (StringCollectionX errors, BusinessObject selectedBusinessObject) =>
				{
					var airline = (ThreeLetterRefAirline)selectedBusinessObject;
					if (airline.RM_ThreeLetterCode.IsEmpty)
					{
						errors.Add("This Airline requires a Three Letter Code to be used as an Inbond Carrier.");
					}

					if (airline.RM_EagleAddedAirlinePrefixOrAccountingCode.IsEmpty)
					{
						errors.Add("This Airline requires an Airline Numeric Code to be used as an Inbond Carrier.");
					}
				};

				return airlineCollection;
			}
		}

		public new CodeDescriptionPairList TransportModeCodes
		{
			get
			{
				return Factory.GetCachedValue("USInBondTransportModeCodes", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(InBondTransportModeCodes.Codes.VesselNonContainer, InBondTransportModeCodes.Descriptions.VesselNonContainer);
					result.AddPair(InBondTransportModeCodes.Codes.VesselContainer, InBondTransportModeCodes.Descriptions.VesselContainer);
					return result;
				});
			}
		}

		public ZZRefCusCodeListCombinedCollection FIRMSCollection
		{
			get
			{
				return UniversalReferenceDataHelper.GetCachedRefCusCodeListCombinedCollection(Factory,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode,
					Parent != null,
					Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DistrictPortCode,
					Parent.BM_DestinationPortCode);
			}
		}

		public ZZRefCusCodeListCombinedCollection ForeignDestPortKCodeList
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = Parent;
				if (parent.BM_ForeignDestPortKCodeType == nameof(ZArchitecture.FieldType.TextDropEdit))
				{
					result = parent.ForeignDestinationRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
			}
		}
	}
}
