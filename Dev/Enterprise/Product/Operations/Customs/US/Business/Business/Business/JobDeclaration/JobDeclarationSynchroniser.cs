using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
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

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!SyncChangesDetected)
			{
				AddTransportReferenceSynchroniser();
				Synchronisers.Add(new FieldSynchroniser(Destination.US_RL_NKPortOfExportInfo, GetPortOfExport, GetPortOfExportRelatedInfos));
				dateOfExportSynchroniser = new FieldSynchroniser(Destination.US_DateOfExportInfo, GetDateOfExport, GetDateOfExportRelatedInfos);
				Synchronisers.Add(dateOfExportSynchroniser);
				Synchronisers.Add(new FieldSynchroniser(Destination.US_UC_NKCountryOfExportInfo, GetCountryOfExport, GetCountryOfExportInfos));
				Synchronisers.Add(new FieldSynchroniser(Destination.US_ITDateInfo, GetITDate, GetITDateRelatedInfos));
				AddUS_SchDExportSynchroniser();
				AddUS_SchDLoadingSynchroniser();
				AddUS_SchDArrivalSynchroniser();
				DefaultForwarderToCurrentBranchOrganisationIfNotEmpty();
				AddUS_HazardousCargoSynchroniser();
				AddManufacturerSynchroniser();
				if (SyncChangesDetected)
				{
					return;
				}
				AddDateOfExportRelatedFieldChangeNotification(Source.AddTransportChangeNotifier);
			}
		}

		FieldSynchroniser dateOfExportSynchroniser;

		protected override IZType GetVoyageFlightNo()
		{
			var result = (ZString)base.GetVoyageFlightNo();
			if (Regex.IsMatch(result, @"^[a-zA-Z]{2}\d{0,3}$"))
			{
				{
					var letters = result.Left(2);
					var numbers = result.SubstringSafe(2);
					result = letters + numbers.PadLeft(3, '0');
				}
			}
			return result;
		}

		delegate void AddTransportChangeNotifier(TransportChangeNotifyType notifyType, TransportChangeNotifyHandler notifier);
		void AddDateOfExportRelatedFieldChangeNotification(AddTransportChangeNotifier addNotifier)
		{
			if (addNotifier != null)
			{
				addNotifier(TransportChangeNotifyType.ATD, DateOfExportRelatedFieldChanged);
				addNotifier(TransportChangeNotifyType.ETD, DateOfExportRelatedFieldChanged);
				addNotifier(TransportChangeNotifyType.Load, DateOfExportRelatedFieldChanged);
				addNotifier(TransportChangeNotifyType.Discharge, DateOfExportRelatedFieldChanged);
				addNotifier(TransportChangeNotifyType.LegOrder, DateOfExportRelatedFieldChanged);
			}
		}

		delegate void RemoveTransportChangeNotifier(TransportChangeNotifyType notifyType, TransportChangeNotifyHandler notifier);
		void RemoveDateOfExportRelatedFieldChangeNotification(RemoveTransportChangeNotifier removeNotifier)
		{
			if (removeNotifier != null)
			{
				removeNotifier(TransportChangeNotifyType.ATD, DateOfExportRelatedFieldChanged);
				removeNotifier(TransportChangeNotifyType.ETD, DateOfExportRelatedFieldChanged);
				removeNotifier(TransportChangeNotifyType.Load, DateOfExportRelatedFieldChanged);
				removeNotifier(TransportChangeNotifyType.Discharge, DateOfExportRelatedFieldChanged);
				removeNotifier(TransportChangeNotifyType.LegOrder, DateOfExportRelatedFieldChanged);
			}
		}

		protected override void AddJE_RL_NKPortOfFirstArrivalSynchroniser(ForwardingConsol consol)
		{
			// JE_RL_NKPortOfFirstArrival is not used in US
		}

		protected override void UnHookSynchronisers()
		{
			RemoveDateOfExportRelatedFieldChangeNotification(Source.RemoveTransportChangeNotifier);
			base.UnHookSynchronisers();
			dateOfExportSynchroniser = null;
		}

		void DateOfExportRelatedFieldChanged(Transport transport, IZType previousValue)
		{
			if (dateOfExportSynchroniser != null)
			{
				dateOfExportSynchroniser.Synchronise();
			}
		}

		void AddManufacturerSynchroniser()
		{
			var fieldSynchroniser = new FieldSynchroniser
				(
					Destination.JE_OA_ManufacturerAddressInfo,
					() => Source.ManufacturerDocAddress.E2_OA_Address,
					() => new[] { Source.ManufacturerDocAddress.E2_OA_AddressInfo },
					dontSetFieldsReadOnly: true,
					() => !Source.ManufacturerDocAddress.E2_AddressOverride && Source.ManufacturerDocAddress.HasRealAddress
				);
			Synchronisers.Add(fieldSynchroniser);
		}

		void AddUS_SchDExportSynchroniser()
		{
			FieldSynchroniser synchroniser = new FieldSynchroniser(Destination.US_SchDExportInfo, GetScheduleDCodeFromPortOfExport,
				() => new ZPropertyInfo[]
					{
						Destination.JE_TransportModeInfo,
						Destination.JE_MessageTypeInfo,
						Destination.US_RL_NKPortOfExportInfo
					});
			Synchronisers.Add(synchroniser);
		}

		void AddUS_SchDLoadingSynchroniser()
		{
			FieldSynchroniser synchroniser = new FieldSynchroniser(Destination.US_SchDLoadingInfo, GetScheduleCodeFromPortOfLoading,
				() => new ZPropertyInfo[]
					{
						Destination.JE_TransportModeInfo,
						Destination.JE_MessageTypeInfo,
						Destination.JE_RL_NKPortOfLoadingInfo
					});
			Synchronisers.Add(synchroniser);
		}

		void AddUS_SchDArrivalSynchroniser()
		{
			FieldSynchroniser synchroniser =
				new FieldSynchroniser(
					Destination.US_SchDArrivalInfo, GetScheduleCodeFromPortOfArrival,
					() => new ZPropertyInfo[] { Destination.JE_MessageTypeInfo, Destination.JE_TransportModeInfo, Destination.JE_RL_NKPortOfArrivalInfo, Destination.US_RN_NKCountryOfDestinationInfo },
					false
					);

			Synchronisers.Add(synchroniser);
		}

		IZType GetScheduleCodeFromPortOfArrival()
		{
			return GetEffectivePortCode(Destination.IsSchDArrivalAllowed ? Destination.PortOfArrivalRefLocoMappings : null, Destination.US_SchDArrivalInfo);
		}

		IZType GetScheduleCodeFromPortOfLoading()
		{
			return GetEffectivePortCode(Destination.PortOfLadingMappings, Destination.US_SchDLoadingInfo);
		}

		IZType GetScheduleDCodeFromPortOfExport()
		{
			return GetEffectivePortCode(Destination.PortOfExportRefLocoMappings, Destination.US_SchDExportInfo);
		}

		IZType GetEffectivePortCode(BusinessObjectCollection matchedPort, ZPropertyInfo propertyInfo)
		{
			var result = ZString.Empty;
			if (matchedPort != null && matchedPort.Count == 1)
			{
				var portCode = matchedPort.OfType<ICodeDescription>().First().Code;
				if (portCode.Length <= propertyInfo.MaxLength)
				{
					result = portCode;
				}
			}
			return result;
		}

		void AddUS_HazardousCargoSynchroniser()
		{
			if (Destination != null && !Destination.IsDeleted && Destination.IsExport && Source != null)
			{
				Source.OuterPackLines.CountChanged += OuterPackLines_CountChanged;
				hazardousSynchroniser = new FieldSynchroniser(Destination.US_HazardousCargoInfo, GetHazardousCargo, GetHazardousCargoInfos);
				Synchronisers.Add(hazardousSynchroniser);
			}
		}

		protected ZPropertyInfo[] GetHazardousCargoInfos()
		{
			var infos = new List<ZPropertyInfo>();
			foreach (ForwardingPackLine packLine in Source.OuterPackLines)
			{
				infos.Add(packLine.JL_RH_NKCommodityCodeInfo);
			}
			infos.Add(Destination.JE_MessageTypeInfo);
			return infos.ToArray();
		}

		IZType GetHazardousCargo()
		{
			IZType result = ZString.Empty;
			if (Destination.IsExport)
			{
				result = Source != null && Source.OuterPackLines.Cast<ForwardingPackLine>()
							.Any(x => x.CommodityCode != null && x.CommodityCode.RH_IsHazardous)
							? (ZString)YesNoDefaultList.Codes.Yes
							: (ZString)YesNoDefaultList.Codes.No;
			}
			return result;
		}

		void OuterPackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			ReSynchroniseHazardous();
		}

		FieldSynchroniser hazardousSynchroniser;

		void ReSynchroniseHazardous()
		{
			UnHookHazardousSynchroniser();
			AddUS_HazardousCargoSynchroniser();
			if (hazardousSynchroniser != null)
			{
				hazardousSynchroniser.Synchronise();
			}
		}

		void UnHookHazardousSynchroniser()
		{
			if (hazardousSynchroniser != null)
			{
				Source.OuterPackLines.CountChanged -= OuterPackLines_CountChanged;
				hazardousSynchroniser.SetEnabled(false, hazardousSynchroniser.DetectEnabled);
				hazardousSynchroniser = null;
			}
		}

		void AddTransportReferenceSynchroniser()
		{
			transportReferenceSynchroniser = new FieldSynchroniser(Destination.US_TransportReferenceInfo, delegate
				{
					var transportReference = ZString.Empty;
					if (Destination.IsExport)
					{
						if (Destination.IsSea && hookedConsol != null)
						{
							transportReference = hookedConsol.HasCoLoadData() ? hookedConsol.JK_CoLoadBookingReference : hookedConsol.JK_BookingReference;
						}
						else if (Destination.IsAir)
						{
							transportReference = hookedConsol != null ? hookedConsol.JK_MasterBillNum : ZString.Empty;
							if (transportReference.IsEmpty)
							{
								transportReference = Source.JS_HouseBill;
							}
							else if (transportReference.Length > 3 && !transportReference.Contains("-"))
							{
								transportReference = transportReference.Left(3) + "-" + transportReference.SubstringSafe(3);
							}
						}
					}
					return transportReference;
				}, GetZPropertyInfosRelatedToTransportReference);
			Synchronisers.Add(transportReferenceSynchroniser);
		}

		FieldSynchroniser transportReferenceSynchroniser;

		void UpdateTransportReferenceSynchroniser()
		{
			if (transportReferenceSynchroniser != null && Destination.ShouldSynchroniseWithShipment())
			{
				transportReferenceSynchroniser.Synchronise();
			}
		}

		ZPropertyInfo[] GetZPropertyInfosRelatedToTransportReference()
		{
			List<ZPropertyInfo> result = new List<ZPropertyInfo>();
			if (hookedConsol != null)
			{
				result.Add(hookedConsol.JK_BookingReferenceInfo);
				result.Add(hookedConsol.JK_MasterBillNumInfo);
			}
			result.Add(Destination.JE_MessageTypeInfo);
			result.Add(Source.JS_TransportModeInfo);
			result.Add(Source.JS_HouseBillInfo);
			return result.ToArray();
		}

		void DefaultForwarderToCurrentBranchOrganisationIfNotEmpty()
		{
			if (Destination.JE_OH_Forwarder.IsEmpty)
			{
				var forwarderPK = GetCurrentBranchOrgPKIfItsFlagAsForwarder();
				if (DetectEnabled)
				{
					if (!forwarderPK.IsEmpty)
					{
						SyncChangesDetected = true;
						return;
					}
				}
				else
				{
					Destination.JE_OH_Forwarder = forwarderPK;
				}
			}
		}

		ZGuid GetCurrentBranchOrgPKIfItsFlagAsForwarder()
		{
			OrgHeader org = GlbBranch.CurrentBranch.OrgProxy;
			if (org != null && org.OH_IsForwarder)
			{
				return org.PK;
			}
			return ZGuid.Empty;
		}

		protected override ZPropertyInfo[] GetPortOfLoadingRelatedInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_MessageTypeInfo);
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKLoadPortInfo);
				infos.Add(hookedConsol.JK_RL_NKLoadForImportTransportInfo);
			}
			return infos.ToArray();
		}

		ZPropertyInfo[] GetPortOfExportRelatedInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_RL_NKPortOfLoadingInfo);
			infos.Add(Destination.JE_MessageTypeInfo);
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKLoadForExportTransportInfo);
			}
			return infos.ToArray();
		}

		IZType GetPortOfExport()
		{
			var result = ZString.Empty;
			if (Destination.IsExport)
			{
				result = (hookedConsol != null) ? hookedConsol.JK_RL_NKLoadForExportTransport : Destination.JE_RL_NKPortOfLoading;
			}
			return result;
		}

		IZType GetCountryOfExport()
		{
			return Destination.IsImport && DataRegistry.Business.USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.GetFallBackValueAtAllLevels(Destination.RegistryCompanyPK, Destination.RegistryBranchPK, Guid.Empty) ? Source.JS_RL_NKOrigin.Left(2) : ZString.Empty;
		}

		ZPropertyInfo[] GetCountryOfExportInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_MessageTypeInfo);
			infos.Add(Source.JS_RL_NKOriginInfo);
			return infos.ToArray();
		}

		ZPropertyInfo[] GetDateOfExportRelatedInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_MessageTypeInfo);
			infos.Add(Destination.JE_ExportDateInfo);
			infos.Add(Source.JS_RL_NKOriginInfo);
			return infos.ToArray();
		}

		IZType GetDateOfExport()
		{
			if (Source.IsImport())
			{
				ZString countryOfOrigin = Source.JS_RL_NKOrigin.Left(2);
				List<Transport> transports = new List<Transport>(Source.TransportsIncludingRelated.ToArray<Transport>());
				transports.Sort(new Comparison<Transport>(DateOfExportTransportComparer));
				Transport firstInternationalTransport = null;
				foreach (Transport transport in transports)
				{
					if (!transport.IsDomestic && transport.JW_RL_NKLoadPort.Left(2).EqualsIgnoringCase(countryOfOrigin) && !transport.JW_RL_NKDiscPort.Left(2).EqualsIgnoringCase(countryOfOrigin))
					{
						firstInternationalTransport = transport;
						break;
					}
				}
				if (firstInternationalTransport != null)
				{
					if (!firstInternationalTransport.JW_ATD.IsEmpty)
					{
						return firstInternationalTransport.JW_ATD;
					}
					else if (!firstInternationalTransport.JW_ETD.IsEmpty)
					{
						return firstInternationalTransport.JW_ETD;
					}
				}
			}
			else
			{
				if (hookedConsol != null)
				{
					Transport transport = hookedConsol.Transports.ExportTransport;
					if (transport != null)
					{
						if (!transport.JW_ATD.IsEmpty)
						{
							return transport.JW_ATD;
						}
						else if (!transport.JW_ETD.IsEmpty)
						{
							return transport.JW_ETD;
						}
					}
				}
			}
			return Destination.JE_ExportDate;
		}

		int DateOfExportTransportComparer(Transport x, Transport y)
		{
			int result = x.JW_LegOrder.CompareTo(y.JW_LegOrder);
			if (result == 0)
			{
				ZDateTime xDate = x.JW_ATD.IsEmpty ? x.JW_ETD : x.JW_ATD;
				ZDateTime yDate = y.JW_ATD.IsEmpty ? y.JW_ETD : y.JW_ATD;
				result = xDate.CompareTo(yDate);
			}
			return result;
		}

		protected override IZType GetExportDate()
		{
			if (hookedConsol != null)
			{
				var transport = (Transport)Destination.MostInterestingLegProvider.GetOutboundLeg(new TypedEnumerable<IMovementLeg>(hookedConsol.Transports));
				if (transport != null)
				{
					if (!transport.JW_ATD.IsEmpty)
					{
						return transport.JW_ATD;
					}
					else if (!transport.JW_ETD.IsEmpty)
					{
						return transport.JW_ETD;
					}
				}
			}
			return base.GetExportDate();
		}

		protected override IZType GetCarrier()
		{
			return Destination.IsExport && hookedConsol.HasCoLoadData() ? hookedConsol.JK_OA_CreditorAddress_ZAddress.OrgPK : base.GetCarrier();
		}

		protected override IZType GetDateOfFirstArrival()
		{
			if (hookedConsol != null)
			{
				var transport = (Transport)Destination.MostInterestingLegProvider.GetOutboundLeg(new TypedEnumerable<IMovementLeg>(hookedConsol.Transports));
				if (transport != null)
				{
					if (!transport.JW_ATA.IsEmpty)
					{
						return transport.JW_ATA;
					}
					else if (!transport.JW_ETA.IsEmpty)
					{
						return transport.JW_ETA;
					}
				}
			}
			return base.GetDateOfFirstArrival();
		}

		#region IT Number and Date

		IZType GetITDate()
		{
			var result = Source.ITDateInfo.Value;
			if (result.IsEmpty)
			{
				result = (ITNumber != null && !ITNumber.IsDeleted) ? ITNumber.CE_IssueDate : ZDateTime.Empty;
			}
			return result;
		}

		ZPropertyInfo[] GetITDateRelatedInfos()
		{
			var infos = new List<ZPropertyInfo> { Source.ITDateInfo };
			if (ITNumber != null && !ITNumber.IsDeleted)
			{
				infos.Add(ITNumber.CE_IssueDateInfo);
			}
			return infos.ToArray();
		}

		CusEntryNumber ITNumber
		{
			get
			{
				if (itNumber == null)
				{
					var consol = Destination.RelevantConsol;
					if (consol != null)
					{
						itNumber = consol.Numbers.GetFirstReferenceNumberByType(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT);
					}
				}
				return itNumber;
			}
		}
		CusEntryNumber itNumber;

		protected override void RemovePackSynchFromRelevantCusEntryNums()
		{
			foreach (CusEntryNumber cusEntryNum in Source.Numbers)
			{
				cusEntryNum.PackLineSynchroniser = null;
			}
		}

		protected override void AddPackSynchToRelevantCusEntryNums(PackLineSynchronisers packLineSynchroniser)
		{
			foreach (CusEntryNumber cusEntryNum in Source.Numbers)
			{
				cusEntryNum.PackLineSynchroniser = packLineSynchroniser;
			}
		}

		#endregion

		ZPropertyInfo[] GetFirmsRelatedInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();

			var shipment = Destination.Shipment;
			if (shipment != null)
			{
				infos.Add(shipment.JS_OA_ImportReleaseDepotInfo);
			}
			else if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_OA_UnpackDepotAddressInfo);
			}

			return infos.ToArray();
		}

		IZType GetFirmsCode()
		{
			IZType result = ZString.Empty;

			var shipment = Destination.Shipment;

			if (shipment != null && shipment.ImportReleaseDepot != null)
			{
				result = shipment.ImportReleaseDepot.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);
			}
			else if (hookedConsol != null)
			{
				if (hookedConsol.UnpackDepotAddress != null)
				{
					result = hookedConsol.UnpackDepotAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);
				}
				else if (hookedConsol.ArrivalCTOAddress != null)
				{
					result = hookedConsol.ArrivalCTOAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);
				}
			}
			return result;
		}

		#region Bills

		protected override Customs.Business.BillsSynchroniser GetBillsSynchroniser()
		{
			return new BillsSynchroniser(Destination, GetHouseBillOfSpecificShipment);
		}

		protected override ZString GetHouseBillOfSpecificShipment(IBillDetails shipment)
		{
			return new BillSynchronisationDataCalculator(Destination, shipment).BillNumber;
		}

		#endregion

		protected override Customs.Business.PackingSynchroniser GetPackingSynchroniser()
		{
			return new PackingSynchroniser(this, Destination);
		}

		protected override void AddPacksSynchroniser()
		{
			if (!Source.IsBuyersConsolLead)
			{
				FieldSynchroniser synchroniser = new FieldSynchroniser(
					Destination.JE_TotalNoOfPacksInfo,
					delegate
					{
						return Destination.IsImport
							? Source.JS_TotalPackageCount > ZInt.Zero
							? Source.JS_TotalPackageCount
								: Source.JS_OuterPacks
								: Source.JS_OuterPacks;
					},
					() => new ZPropertyInfo[] { Destination.JE_MessageTypeInfo, Source.JS_TotalPackageCountInfo, Source.JS_OuterPacksInfo },
					false
				);
				Synchronisers.Add(synchroniser);
				FieldSynchroniser packTypeSyncroniser = new FieldSynchroniser(
					Destination.JE_TotalNoOfPacksPackTypeInfo,
					delegate
					{
						return Destination.IsImport
							? Source.JS_TotalPackageCount > ZInt.Zero
							? Source.JS_F3_NKTotalCountPackType
								: Source.JS_F3_NKPackType
								: Source.JS_F3_NKPackType;
					},
					() => new ZPropertyInfo[] { Destination.JE_MessageTypeInfo, Source.JS_F3_NKTotalCountPackTypeInfo, Source.JS_F3_NKPackTypeInfo },
					false
				);
				packTypeSyncroniser.Format += PackTypeSyncroniser_Format;
				Synchronisers.Add(packTypeSyncroniser);
			}
			else
			{
				base.AddPacksSynchroniser();
			}
		}

		protected override ZString GetCustomsUnitForThisPackType(ZString freightPackageType)
		{
			var mappings = USCustomsDataRegistry.Instance.USPackageTypesMapping.GetValueWithoutFallback(Destination.RegistryCompanyPK, Guid.Empty, Guid.Empty);
			return mappings.GetMappedPackageType(freightPackageType);
		}

		protected override void HookConsolToDeclarationSynchronisers()
		{
			base.HookConsolToDeclarationSynchronisers();
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.US_US_NKLocationOfGoodsInfo, GetFirmsCode, GetFirmsRelatedInfos));

			if (hookedConsol != null)
			{
				AddDateOfExportRelatedFieldChangeNotification(hookedConsol.AddTransportChangeNotifier);
			}
			UpdateTransportReferenceSynchroniser();
		}

		protected override void UnHookConsolToDeclarationSynchronisers()
		{
			if (hookedConsol != null)
			{
				RemoveDateOfExportRelatedFieldChangeNotification(hookedConsol.RemoveTransportChangeNotifier);
			}
			base.UnHookConsolToDeclarationSynchronisers();
			UpdateTransportReferenceSynchroniser();
		}

		protected override ZString GetConvertedTransportMode(ZString shipmentTransportMode)
		{
			var result = base.GetConvertedTransportMode(shipmentTransportMode);

			if (shipmentTransportMode == Core.Constants.TransportModes.Road)
			{
				result = TransportTypeList.Codes.Truck;
			}

			return result;
		}
	}
}
