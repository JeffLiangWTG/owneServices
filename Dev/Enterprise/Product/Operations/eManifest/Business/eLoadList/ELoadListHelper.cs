using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.eManifest.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eManifest.Business
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification hides desired base class")]
	public class ELoadListHelper : IELoadListHelper
	{
		public ELoadListHelper()
		{ }

		public ELoadListHelper(ILogger logger)
		{
			this.logger = logger;
		}

		readonly ILogger logger;

		public Forwarding.IForwardingConsol CreateConsol(IELoadList eLoadList)
		{
			Argument.NotNull(eLoadList, "eLoadList");

			var sourceELoadList = (ELoadList)eLoadList;

			var destinationDepot = GetPortFromAddress(sourceELoadList.DestinationDepot);
			var originDepot = GetPortFromAddress(sourceELoadList.OriginDepot);

			var consol = (CommonConsol)eLoadList.Factory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = sourceELoadList.DO_TransportMode;
			consol.JK_MasterBillNum = sourceELoadList.DO_MasterBillNumber;
			consol.JK_RL_NKDischargePort = destinationDepot;
			consol.JK_RL_NKLoadPort = originDepot;
			consol.JK_ConsolMode = sourceELoadList.DO_TransportMode == Constants.TransportModes.Air ? Constants.ContainerModes.Loose : Constants.ContainerModes.Groupage;
			consol.JK_OA_PackDepotAddress = sourceELoadList.DO_OA_OriginDepot;
			consol.JK_OA_UnpackDepotAddress = sourceELoadList.DO_OA_DestinationDepot;

			Func<Transport, bool> withELoadListPorts = t =>
			{
				return t.JW_RL_NKLoadPort == originDepot && t.JW_RL_NKDiscPort == destinationDepot;
			};

			var transport = consol.Transports.Cast<Transport>().FirstOrDefault(withELoadListPorts) ?? consol.Transports.AddNew();

			transport.JW_ETA = sourceELoadList.DO_E_ARV;
			transport.JW_ETD = sourceELoadList.DO_E_DEP;
			transport.JW_VoyageFlight = sourceELoadList.DO_VoyageFlight;
			transport.JW_Vessel = sourceELoadList.DO_RV_NKVessel;
			transport.JW_RL_NKLoadPort = originDepot;
			transport.JW_RL_NKDiscPort = destinationDepot;

			if (!sourceELoadList.DO_OH_Carrier.IsEmpty)
			{
				consol.SetDefaultShippingLineAddress(sourceELoadList.DO_OH_Carrier);
			}

			return (Forwarding.IForwardingConsol)consol;
		}

		public void AttachELoadLists(Forwarding.IForwardingConsol consol, IEnumerable<IELoadList> eLoadLists)
		{
			Argument.NotNull(consol, "consol");
			Argument.NotNull(eLoadLists, "eLoadLists");

			if (eLoadLists.Any())
			{
				var sourceELoadLists = eLoadLists.Cast<ELoadList>().ToList();
				var commonConsol = (CommonConsol)consol;
				var consignorAddressShipmentMap = new Dictionary<ZGuid, CommonShipment>();

				foreach (var eLoadList in sourceELoadLists)
				{
					var originDepot = GetPortFromAddress(eLoadList.OriginDepot);
					var destinationDepot = GetPortFromAddress(eLoadList.DestinationDepot);

					// Containers
					var container = commonConsol.Containers.Cast<CommonContainer>().FirstOrDefault(c => StringComparer.InvariantCultureIgnoreCase.Compare(c.JC_ContainerNum, eLoadList.DO_ContainerNumber) == 0);
					if (container == null)
					{
						container = commonConsol.Containers.AddNew();
						container.JC_ContainerNum = eLoadList.DO_ContainerNumber;
						container.JC_RC = eLoadList.DO_RC_ContainerType;
						container.JC_ContainerMode = commonConsol.JK_TransportMode == Constants.TransportModes.Air ? Constants.ContainerModes.ULD : Constants.ContainerModes.Groupage;
					}

					// Shipments
					var headersGroupedByConsignorAddress = GetBookingHeaders(eLoadList).GroupBy(header => header.DH_OA_Consignor);
					foreach (var headersWithSameConsignorAddress in headersGroupedByConsignorAddress)
					{
						CommonShipment shipment;
						if (!consignorAddressShipmentMap.TryGetValue(headersWithSameConsignorAddress.Key, out shipment))
						{
							shipment = (CommonShipment)CreateShipment(headersWithSameConsignorAddress);
							shipment.JS_TransportMode = commonConsol.JK_TransportMode;
							shipment.JS_PackingMode = shipment.JS_TransportMode == Constants.TransportModes.Air ? Constants.ContainerModes.Loose : Constants.ContainerModes.LCL;
							shipment.JS_RL_NKOrigin = originDepot;
							shipment.JS_RL_NKDestination = destinationDepot;
							shipment.ConsigneeDocumentaryAddress.E2_OA_Address = commonConsol.JK_OA_ReceivingForwarderAddress;
							shipment.JS_INCO = Constants.IncoTerms.DeliveredDutyPaid;

							shipment.OuterPackLines.RemoveAndDeleteAll();

							if (Env.CurrentUser.IsBatchProcessor)
							{
								var branch = BranchLocator.GetMatchingBranch(originDepot, destinationDepot);
								shipment.CreateBillingJobFromBranch(branch);
							}

							commonConsol.Shipments.Add(shipment);
							consignorAddressShipmentMap[headersWithSameConsignorAddress.Key] = shipment;
						}

						// PackLines
						var bookingLines = GetBookingLines(eLoadList, headersWithSameConsignorAddress);
						if (bookingLines.Any())
						{
							var packLine = shipment.OuterPackLines.AddNew();
							packLine.JL_JC = container.PK;
							packLine.JL_PackageCount = bookingLines.Count();
							packLine.JL_ActualWeight = TotalCalculation.GetTotalWeight(bookingLines, SupplierBookingLine.Schema.DL_GrossWeight, SupplierBookingLine.Schema.DL_GrossWeightUQ, Env.Registry.FreightWeightUnit);
							packLine.JL_ActualWeightUQ = Env.Registry.FreightWeightUnit;
							packLine.JL_ActualVolume = TotalCalculation.GetTotalVolume(bookingLines, SupplierBookingLine.Schema.DL_Cubic, SupplierBookingLine.Schema.DL_CubicUQ, Env.Registry.FreightVolumeUnit);
							packLine.JL_ActualVolumeUQ = Env.Registry.FreightVolumeUnit;

							foreach (var line in bookingLines)
							{
								line.DL_JS_ApprovedShipment = shipment.PK;
							}

							FixValueIfInvalid(packLine, JobPackLinesSchema.JL_ActualWeight, JobPackLinesSchema.JL_ActualVolume);
						}
					}

					FixValueIfInvalid(container, JobContainerSchema.JC_GrossWeight, JobContainerSchema.JC_GrossVolume);

					eLoadList.DO_Status = Constants.ELoadListStatuses.Consolidated;

					((CommonConsol)consol).Logs.CreateOrRecreateEventLog(
						Events.ELoadListConsolidated,
						EstimateActual.Actual,
						ZDateTimeOffset.UtcNow,
						string.Format((NoResString)"eLoadList {0} has been consolidated", eLoadList.DO_UniqueReference));
				}

				// Shipment totals
				foreach (CommonShipment shipment in consignorAddressShipmentMap.Values)
				{
					shipment.JS_ActualWeight = TotalCalculation.GetTotalWeight(shipment.OuterPackLines, PackLine.Schema.JL_ActualWeight, PackLine.Schema.JL_ActualWeightUQ, Env.Registry.FreightWeightUnit);
					shipment.JS_UnitOfWeight = Env.Registry.FreightWeightUnit;
					shipment.JS_ActualVolume = TotalCalculation.GetTotalVolume(shipment.OuterPackLines, PackLine.Schema.JL_ActualVolume, PackLine.Schema.JL_ActualVolumeUQ, Env.Registry.FreightVolumeUnit);
					shipment.JS_UnitOfVolume = Env.Registry.FreightVolumeUnit;
					shipment.JS_OuterPacks = (int)TotalCalculation.GetTotal(shipment.OuterPackLines, PackLine.Schema.JL_PackageCount);
					shipment.JS_F3_NKPackType = Constants.PkgUnit.Package;

					var bookingLines = shipment.GetSupplierBookingLines();
					var linesByCurrency = bookingLines.GroupBy(l => l.DL_RX_NKGoodsValueCurrency).ToList();

					if (linesByCurrency.Count == 1)
					{
						shipment.JS_GoodsValue = bookingLines.Sum(l => l.DL_GoodsValue);
						shipment.JS_RX_NKGoodsValueCurr = linesByCurrency.First().Key;
					}
					else
					{
						var localCurrency = shipment.GetCurrencyAtDestination();
						var valueInLocalCurrency = 0m;

						foreach (var record in linesByCurrency)
						{
							var currency = shipment.Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, record.Key));
							if (currency != null)
							{
								var value = record.Sum(l => l.DL_GoodsValue);

								valueInLocalCurrency += currency.ConvertUsingSellRate(ZDateTime.Now, value, localCurrency);
							}
						}

						shipment.JS_GoodsValue = valueInLocalCurrency;
						shipment.JS_RX_NKGoodsValueCurr = localCurrency.RX_Code;
					}

					FixValueIfInvalid(shipment, JobShipmentSchema.JS_ActualChargeable, JobShipmentSchema.JS_DocumentedChargeable, JobShipmentSchema.JS_ManifestedChargeable);

					if (!shipment.IsValidationSuspended)
					{
						shipment.Validation.ValidateAll();
					}
				}
			}
		}

		static ZString GetPortFromAddress(OrgAddress depotAddress)
		{
			var depot = ZString.Empty;

			if (depotAddress != null)
			{
				depot = depotAddress.OA_RL_NKRelatedPortCode;

				if (depot.IsEmpty)
				{
					depot = depotAddress.Header.OH_RL_NKClosestPort;
				}
			}

			return depot;
		}

		Forwarding.IForwardingShipment CreateShipment(IEnumerable<SupplierBookingHeader> headers)
		{
			var shipment = (CommonShipment)headers.First().Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = headers.First().DH_OA_Consignor;

			shipment.JS_GoodsDescription = Res.GetString("936be0f7-0809-4920-9a53-23f887b21e38", "Various Cargo");

			var headerWithConsignmentBroker = headers.FirstOrDefault(header => !header.DH_OH_ConsignmentBroker.IsEmpty);
			if (headerWithConsignmentBroker != null)
			{
				var consignmentBroker = headerWithConsignmentBroker.Factory.Load<OrgHeader>(headerWithConsignmentBroker.DH_OH_ConsignmentBroker);
				shipment.PickupAgentDocumentaryAddress.E2_OA_Address = consignmentBroker.MainAddress.PK;
			}

			var headerWithBookedBy = headers.FirstOrDefault(header => !header.DH_OC_BookedBy.IsEmpty);
			if (headerWithBookedBy != null)
			{
				var bookContact = headerWithBookedBy.Factory.Load<OrgContact>(headerWithBookedBy.DH_OC_BookedBy);
				if (bookContact != null && bookContact.EffectiveContactAddress != null)
				{
					var addressRequirement = new JobDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress, ContactType.LocalClient);
					var bookingPartyDocumentaryAddress = shipment.DocAddresses.FindOrCreateWithRequirement(addressRequirement);

					bookingPartyDocumentaryAddress.E2_OA_Address = bookContact.EffectiveContactAddress.PK;
					bookingPartyDocumentaryAddress.E2_Contact = bookContact.OC_ContactName;
				}
			}

			return shipment as Forwarding.IForwardingShipment;
		}

		static IEnumerable<SupplierBookingHeader> GetBookingHeaders(ELoadList eLoadList)
		{
			var bookingLinesQuery = new ZDBOnlySubQuery(typeof(SupplierBookingLine), SupplierBookingLineSchema.DL_DH_BookingHeader);
			bookingLinesQuery.AddToFilter(SupplierBookingLineSchema.DL_DO_LoadList, eLoadList.PK);

			var headersQuery = new ZDBOnlyQuery(typeof(SupplierBookingHeader));
			headersQuery.AddSubQuery(bookingLinesQuery, JoinCondition.And);

			return eLoadList.Factory.Load<SupplierBookingHeader>(headersQuery);
		}

		static IEnumerable<SupplierBookingLine> GetBookingLines(ELoadList eLoadList, IEnumerable<SupplierBookingHeader> bookingHeaders)
		{
			var bookingLinesQuery = new ZQuery(SupplierBookingLineSchema.DL_DH_BookingHeader, bookingHeaders.Select(header => header.PK));
			bookingLinesQuery.AddToFilter(SupplierBookingLineSchema.DL_DO_LoadList, eLoadList.PK);

			return bookingHeaders.First().Factory.Load<SupplierBookingLine>(bookingLinesQuery);
		}

		#region FixValuesIfInvalid

		void FixValueIfInvalid(BusinessObject businessObject, params SchemaColumn[] columns)
		{
			Argument.NotNull(businessObject, "businessObject");
			Argument.NotNull(columns, "columns");

			if (!Env.CurrentUser.IsBatchProcessor)
			{
				return;
			}

			foreach (var column in columns)
			{
				var decimalColumn = column as SchemaDecimalColumn ?? throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, "Fixing the values of type '{0}' is not implemented", column.GetType()));

				object oldValue;
				object newValue;
				FixDecimalValueIfInvalid(businessObject, decimalColumn, out oldValue, out newValue);

				if (!oldValue.Equals(newValue) && logger != null)
				{
					var logMessage = string.Format(CultureInfo.InvariantCulture,
						(NoResString)"The value '{0}' of column '{1}' is out of allowed range. The default value '{2}' is used instead.",
						oldValue,
						column.Name,
						newValue);

					logger.Log(LogType.Warning, logMessage);
				}
			}
		}

		void FixDecimalValueIfInvalid(BusinessObject businessObject, SchemaDecimalColumn column, out object oldValue, out object newValue)
		{
			var row = ((INeedRow)businessObject).Row;
			oldValue = row[column.Name];

			var currentValue = new ZDecimal(oldValue);
			if (!currentValue.IsWithinSqlPrecisionAndScale(column.Precision, column.Scale))
			{
				row[column.Name] = InvalidDecimalValue;
			}

			newValue = row[column.Name];
		}

		const decimal InvalidDecimalValue = -1m;

		#endregion
	}
}
