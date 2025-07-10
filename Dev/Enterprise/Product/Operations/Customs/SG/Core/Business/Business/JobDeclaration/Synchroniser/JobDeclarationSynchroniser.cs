using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationSynchroniser : Customs.Business.JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniser(JobDeclaration destination)
			: base(destination)
		{
		}

		public new JobDeclaration Destination
		{
			get { return (JobDeclaration)base.Destination; }
		}

		public ForwardingShipment Shipment
		{
			get { return Source; }
		}

		protected override void HookSynchronisers()
		{
			if (!SyncChangesDetected)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_OH_ImporterInfo, GetImporter, () => new ZPropertyInfo[] { Shipment.ConsigneeDocumentaryAddress.OrganisationPKInfo, Shipment.JS_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo }));
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_OH_SupplierInfo, GetExporter, () => new ZPropertyInfo[] { Shipment.ConsignorDocumentaryAddress.OrganisationPKInfo, Shipment.JS_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo }));
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_OH_ExporterInfo, GetExporter, () => new ZPropertyInfo[] { Shipment.ConsignorDocumentaryAddress.OrganisationPKInfo, Shipment.JS_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo }));

				FieldSynchroniser packingModeSynchroniser = new FieldSynchroniser(Destination.JE_ContainerModeInfo, Shipment.JS_PackingModeInfo);
				packingModeSynchroniser.Format += PackingModeSynchroniser_Format;
				Synchronisers.Add(packingModeSynchroniser);

				Synchronisers.Add(new FieldSynchroniser(Destination.JE_RL_NKOriginInfo, Shipment.JS_RL_NKOriginInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_RL_NKFinalDestinationInfo, Shipment.JS_RL_NKDestinationInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_MessageTypeInfo, delegate
				{ if (Shipment.IsImport()) { return (ZString)MessageTypeCodeList.Codes.INP; } else if (Shipment.IsExport()) { return (ZString)MessageTypeCodeList.Codes.OUT; } else { return (ZString)MessageTypeCodeList.Codes.TNP; } }, () => new ZPropertyInfo[] { Destination.JE_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo }));

				Synchronisers.Add(new FieldSynchroniser(Destination.JE_HouseBillInfo, Shipment.JS_HouseBillInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.SG_OutwardHAWBInfo, Shipment.JS_HouseBillInfo));

				Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalWeightInfo, Shipment.JS_ActualWeightInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalWeightUnitInfo, Shipment.JS_UnitOfWeightInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalVolumeInfo, Shipment.JS_ActualVolumeInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalVolumeUnitInfo, Shipment.JS_UnitOfVolumeInfo));

				Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalNoOfPacksDecimalInfo, delegate
				{ return ZDecimal.ParseSafe(Shipment.JS_OuterPacks.ToString(), 0); }, () => new ZPropertyInfo[] { Shipment.JS_OuterPacksInfo }));
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalNoOfPacksPackTypeInfo, GetPackType, () => new ZPropertyInfo[] { Shipment.JS_F3_NKPackTypeInfo }));

				Synchronisers.Add(new FieldSynchroniser(Destination.JE_GoodsDescriptionInfo, Shipment.JS_GoodsDescriptionInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_ShipmentIncoTermInfo, GetIncoTerm, () => new ZPropertyInfo[] { Shipment.JS_INCOInfo }));

				HookConsolToDeclarationSynchronisers();
				HookOwnerRefSynchronisers();
				HookJobDocAddressSynchronisers();
				PopulateOwnersRefIfRequired();
				HookCollectionSynchronisers();
			}
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			UnHookCollectionSynchronisers();
		}

		protected override void HookConsolToDeclarationSynchronisers()
		{
			LinkForwarder();

			ZPropertyInfo[] inwardTransportInfos = InwardConsol != null ? new ZPropertyInfo[] { InwardConsol.JK_TransportModeInfo, Shipment.JS_TransportModeInfo, Shipment.JS_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo } : new ZPropertyInfo[] { Shipment.JS_TransportModeInfo, Shipment.JS_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo };
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_TransportModeInfo, GetInwardTransportMode, () => inwardTransportInfos));

			ZPropertyInfo[] outwardTransportInfos = OutwardConsol != null ? new ZPropertyInfo[] { OutwardConsol.JK_TransportModeInfo, Shipment.JS_TransportModeInfo, Shipment.JS_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo } : new ZPropertyInfo[] { Shipment.JS_TransportModeInfo, Shipment.JS_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo };
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.SG_OutwardTransportModeInfo, GetOutwardTransportMode, () => outwardTransportInfos));

			ZPropertyInfo[] loadingInfos = InwardConsol != null ? new ZPropertyInfo[] { InwardConsol.JK_RL_NKLoadPortInfo, Shipment.JS_RL_NKOriginInfo } : new ZPropertyInfo[] { Shipment.JS_RL_NKOriginInfo };
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_RL_NKPortOfLoadingInfo, GetPortOfLoading, () => loadingInfos));

			ZPropertyInfo[] dischargeInfos = OutwardConsol != null ? new ZPropertyInfo[] { OutwardConsol.JK_RL_NKDischargePortInfo, Shipment.JS_RL_NKDestinationInfo } : new ZPropertyInfo[] { Shipment.JS_RL_NKDestinationInfo };
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_RL_NKPortOfArrivalInfo, GetPortOfDischarge, () => dischargeInfos));

			ZPropertyInfo[] arrivalDateInfos = InwardConsol != null ? new ZPropertyInfo[] { Shipment.JS_E_ARVInfo, InwardConsol.JK_JX_JB_A_ARVInfo, InwardConsol.JK_JX_JB_E_ARVInfo } : new ZPropertyInfo[] { Shipment.JS_E_ARVInfo };
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_DateOfArrivalInfo, GetDateOfArrival, () => arrivalDateInfos));

			ZPropertyInfo[] departureDateInfos = OutwardConsol != null ? new ZPropertyInfo[] { Shipment.JS_ShippedOnBoardDateInfo, Shipment.JS_E_DEPInfo, OutwardConsol.JK_JX_JA_A_DEPInfo, OutwardConsol.JK_JX_JA_E_DEPInfo } : new ZPropertyInfo[] { Shipment.JS_ShippedOnBoardDateInfo, Shipment.JS_E_DEPInfo };
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_ExportDateInfo, GetExportDate, () => departureDateInfos));

			if (InwardConsol != null)
			{
				ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_VoyageFlightNoInfo, InwardConsol.JK_JX_JV_VoyageFlightInfo));
				ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_OH_ShippingLineInfo, GetInwardShippingLine, () => new[] { Destination.JE_TransportModeInfo, InwardConsol.JK_OA_ShippingLineAddressInfo }));
				ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_VesselNameInfo, InwardConsol.JK_JX_JV_NKVesselInfo));
				if (InwardConsol.IsSea)
				{
					ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_OH_InwardCarrierAgentInfo, GetInwardShippingLine, () => new[] { Destination.JE_TransportModeInfo, InwardConsol.JK_OA_ShippingLineAddressInfo }));
				}
			}

			if (OutwardConsol != null)
			{
				ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.SG_OutwardVoyageFlightNoInfo, OutwardConsol.JK_JX_JV_VoyageFlightInfo));
				if (InwardConsol == null)
				{
					ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_OH_ShippingLineInfo, GetOutwardShippingLine, () => new[] { Destination.JE_TransportModeInfo, OutwardConsol.JK_OA_ShippingLineAddressInfo }));
				}

				ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.SG_OutwardVesselNameInfo, OutwardConsol.JK_JX_JV_NKVesselInfo));
				if (OutwardConsol.IsSea)
				{
					ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.OutwardShippingLineForwarderPKInfo, GetOutwardShippingLine, () => new[] { Destination.SG_OutwardTransportModeInfo, OutwardConsol.JK_OA_ShippingLineAddressInfo }));
				}
			}

			ForwardingConsol effectiveConsol = InwardConsol ?? OutwardConsol;
			if (effectiveConsol != null)
			{
				ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo, effectiveConsol.JK_OA_ArrivalCTOAddressInfo));
				ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.DepotDocAddress.E2_OA_AddressInfo, effectiveConsol.JK_OA_UnpackDepotAddressInfo));
			}
		}

		protected override Customs.Business.BillsSynchroniser GetBillsSynchroniser()
		{
			return new BillsSynchroniser(Destination, this.GetHouseBillOfSpecificShipment, delegate
			{ return OutwardConsol; }, delegate
			{ return InwardConsol; });
		}

		void LinkForwarder()
		{
			ZPropertyInfo[] forwarderInfos = null;

			if (InwardConsol != null && OutwardConsol != null)
			{
				forwarderInfos = new[] { InwardConsol.JK_OA_ReceivingForwarderAddressInfo, OutwardConsol.JK_OA_SendingForwarderAddressInfo, Shipment.JS_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo };
			}
			else if (InwardConsol != null)
			{
				forwarderInfos = new[] { InwardConsol.JK_OA_ReceivingForwarderAddressInfo, Shipment.JS_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo };
			}
			else if (OutwardConsol != null)
			{
				forwarderInfos = new[] { OutwardConsol.JK_OA_SendingForwarderAddressInfo, Shipment.JS_RL_NKOriginInfo, Shipment.JS_RL_NKDestinationInfo };
			}

			if (forwarderInfos != null)
			{
				ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_OH_ForwarderInfo, GetForwarder, () => forwarderInfos));
			}
		}

		#region Consols

		#region Inward Consol

		protected ForwardingConsol InwardConsol
		{
			get
			{
				if (inwardConsol == null)
				{
					foreach (ForwardingConsol consol in Shipment.Consols)
					{
						if (consol.JK_RL_NKDischargePort.Left(2) == Core.Constants.CountryCodes.Singapore && consol.JK_RL_NKLoadPort.Left(2) != Core.Constants.CountryCodes.Singapore)
						{
							inwardConsol = consol;
							break;
						}
					}
				}

				return inwardConsol;
			}
		}
		ForwardingConsol inwardConsol;

		#endregion

		#region Outward Consol

		protected ForwardingConsol OutwardConsol
		{
			get
			{
				if (outwardConsol == null)
				{
					foreach (ForwardingConsol consol in Shipment.Consols)
					{
						if (consol.JK_RL_NKLoadPort.Left(2) == Core.Constants.CountryCodes.Singapore && consol.JK_RL_NKDischargePort.Left(2) != Core.Constants.CountryCodes.Singapore)
						{
							outwardConsol = consol;
							break;
						}
					}
				}

				return outwardConsol;
			}
		}
		ForwardingConsol outwardConsol;

		#endregion

		#endregion

		protected override void UnHookConsolToDeclarationSynchronisers()
		{
			base.UnHookConsolToDeclarationSynchronisers();

			if (inwardConsol != null)
			{
				InwardConsol.JK_RL_NKDischargePortInfo.ValueChanged -= new EventHandler(UpdateConsolSelection);
				inwardConsol = null;
			}

			if (outwardConsol != null)
			{
				OutwardConsol.JK_RL_NKDischargePortInfo.ValueChanged -= new EventHandler(UpdateConsolSelection);
				outwardConsol = null;
			}
		}

		void HookCollectionSynchronisers()
		{
			Synchronisers.Add(new CusContainerCollectionSynchroniser(Destination));
			Shipment.Consols.CountChanged += new CollectionCountChangedEventHandler(UpdateConsolSelection);
		}

		void UnHookCollectionSynchronisers()
		{
			Shipment.Consols.CountChanged -= new CollectionCountChangedEventHandler(UpdateConsolSelection);
		}

		#region Transport Modes

		IZType GetInwardTransportMode()
		{
			IZType result = ZString.Empty;

			if (Shipment.IsCrossTrade() || Shipment.IsImport())
			{
				result = InwardConsol != null ? InwardConsol.JK_TransportMode : ShipmentTransportMode;
			}

			return result;
		}

		IZType GetOutwardTransportMode()
		{
			IZType result = ZString.Empty;

			if (Shipment.IsCrossTrade() || Shipment.IsExport())
			{
				result = OutwardConsol != null ? OutwardConsol.JK_TransportMode : ShipmentTransportMode;
			}

			return result;
		}

		ZString ShipmentTransportMode
		{
			get
			{
				ZString result = "";

				if (Shipment.JS_TransportMode == Core.Constants.TransportModes.AirSea)
				{
					result = TransportModeCodeList.Codes.TransportMode_4_Air;
				}
				else if (Shipment.JS_TransportMode == Core.Constants.TransportModes.SeaAir)
				{
					result = TransportModeCodeList.Codes.TransportMode_1_SEA;
				}
				else if (Shipment.JS_TransportMode != Core.Constants.TransportModes.Courier)
				{
					result = Shipment.JS_TransportMode;
				}

				return result;
			}
		}

		#endregion

		#region Organisations

		IZType GetImporter()
		{
			IZType result = ZGuid.Empty;

			if (Shipment.IsImport())
			{
				bool isMiscellaneous = Shipment.ConsigneeDocumentaryAddress.Organisation != null && Shipment.ConsigneeDocumentaryAddress.Organisation.IsMiscellaneous;
				if (!isMiscellaneous)
				{
					result = Shipment.ConsigneeDocumentaryAddress.OrganisationPK;
				}
			}

			return result;
		}

		IZType GetExporter()
		{
			IZType result = ZGuid.Empty;

			if (Shipment.IsExport())
			{
				bool isMiscellaneous = Shipment.ConsignorDocumentaryAddress.Organisation != null && Shipment.ConsignorDocumentaryAddress.Organisation.IsMiscellaneous;
				if (!isMiscellaneous)
				{
					result = Shipment.ConsignorDocumentaryAddress.OrganisationPK;
				}
			}

			return result;
		}

		IZType GetInwardShippingLine()
		{
			return InwardConsol.ShippingLinePK;
		}

		IZType GetOutwardShippingLine()
		{
			return OutwardConsol.ShippingLinePK;
		}

		IZType GetForwarder()
		{
			IZType result = ZGuid.Empty;

			if (Shipment.IsImport())
			{
				if (InwardConsol != null)
				{
					result = InwardConsol.ReceivingForwarderPK;
				}
			}
			else if (Shipment.IsExport())
			{
				if (OutwardConsol != null)
				{
					result = OutwardConsol.SendingForwarderPK;
				}
			}

			return result;
		}

		#endregion

		#region Locations

		protected override IZType GetPortOfLoading()
		{
			var result = InwardConsol != null ? InwardConsol.JK_RL_NKLoadPort : Shipment.JS_RL_NKOrigin;
			var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Destination.Factory, result, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
			if (cusCode == null && result.Length > 2)
			{
				result = result.Substring(0, 2) + "ZZZ";
			}
			return result;
		}

		IZType GetPortOfDischarge()
		{
			var result = OutwardConsol != null ? OutwardConsol.JK_RL_NKDischargePort : Shipment.JS_RL_NKDestination;
			var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Destination.Factory, result, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
			if (cusCode == null && result.Length > 2)
			{
				result = result.Substring(0, 2) + "ZZZ";
			}
			return result;
		}

		#endregion

		#region Dates

		protected override IZType GetDateOfArrival()
		{
			ZDateTime result = ZDateTime.Empty;

			if (InwardConsol != null)
			{
				if (!InwardConsol.JK_JX_JB_A_ARV.IsEmpty)
				{
					result = InwardConsol.JK_JX_JB_A_ARV;
				}
				else if (!InwardConsol.JK_JX_JB_E_ARV.IsEmpty)
				{
					result = InwardConsol.JK_JX_JB_E_ARV;
				}
			}

			if (result == ZDateTime.Empty)
			{
				result = Shipment.JS_E_ARV;
			}

			return result;
		}

		protected override IZType GetExportDate()
		{
			ZDateTime result = ZDateTime.Empty;

			if (OutwardConsol != null)
			{
				if (!OutwardConsol.JK_JX_JA_A_DEP.IsEmpty)
				{
					result = OutwardConsol.JK_JX_JA_A_DEP;
				}
				else if (!OutwardConsol.JK_JX_JA_E_DEP.IsEmpty)
				{
					result = OutwardConsol.JK_JX_JA_E_DEP;
				}
			}

			if (result == ZDateTime.Empty)
			{
				result = Shipment.JS_ShippedOnBoardDate.IsEmpty ? Shipment.JS_E_DEP : Shipment.JS_ShippedOnBoardDate;
			}

			return result;
		}

		#endregion

		#region Other

		IZType GetPackType()
		{
			switch (Shipment.JS_F3_NKPackType)
			{
				case Core.Constants.PkgUnit.Pallet:
					return (ZString)UnitOfQuantityCodeList.Codes.PAT;
				case Core.Constants.PkgUnit.Piece:
					return (ZString)UnitOfQuantityCodeList.Codes.NMB;
				case Core.Constants.PkgUnit.Container:
					return (ZString)UnitOfQuantityCodeList.Codes.UNT;
				default:
					return Shipment.JS_F3_NKPackType;
			}
		}

		IZType GetIncoTerm()
		{
			switch (Shipment.JS_INCO)
			{
				case Core.Constants.IncoTerms.CostAndInsurance:
					return (ZString)UnitPriceTermTypeCodeList.Codes.CNI;
				default:
					return Shipment.JS_INCO;
			}
		}

		#endregion
	}
}
