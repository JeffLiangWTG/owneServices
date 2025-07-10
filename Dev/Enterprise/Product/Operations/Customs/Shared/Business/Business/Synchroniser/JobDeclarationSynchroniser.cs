using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]  // Oh, do be quiet. 
	public class JobDeclarationSynchroniser : BusinessObjectSynchroniser
	{
		public JobDeclarationSynchroniser(BaseJobDeclaration declaration)
			: base(declaration, declaration.Shipment)
		{
		}

		public new BaseJobDeclaration Destination
		{
			get { return (BaseJobDeclaration)base.Destination; }
		}

		public new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		#region Syncroniser Collections

		protected internal List<FieldSynchroniser> ConsolFieldSynchronisers
		{
			get { return consolFieldSynchronisers ?? (consolFieldSynchronisers = new List<FieldSynchroniser>()); }
		}
		List<FieldSynchroniser> consolFieldSynchronisers;

		protected PackLineSynchronisers PackLineSynchronisers
		{
			get
			{
				if (packLineSynchronisers == null)
				{
					var args = GetPackLineSynchronisersArgs();
					InitializePackLineSynchronisersArgs(args);
					packLineSynchronisers = new PackLineSynchronisers(args);

					Source.PackLineSynchroniser = packLineSynchronisers;
				}
				return packLineSynchronisers;
			}
		}
		PackLineSynchronisers packLineSynchronisers;

		protected virtual PackLineSynchronisersArg GetPackLineSynchronisersArgs()
		{
			return new PackLineSynchronisersArg();
		}

		protected virtual void InitializePackLineSynchronisersArgs(PackLineSynchronisersArg args)
		{
			args.SyncEnabled = delegate
			{ return IsEnabled; };
			args.SyncDetectEnabled = delegate
			{ return DetectEnabled; };
			args.GetContainerSynchroniser = GetContainersSynchroniser;
			args.GetPackingSynchroniser = GetPackingSynchroniser;
			args.GetBillsSynchroniser = GetBillsSynchroniser;
			args.AddPackSynchToForwardingContainers = AddPackSynchToForwardingContainers;
			args.RemovePackSynchFromForwardingContainers = RemovePackSynchFromForwardingContainers;

			args.AddPackSynchToRelevantCusEntryNums = AddPackSynchToRelevantCusEntryNums;
			args.RemovePackSynchFromRelevantCusEntryNums = RemovePackSynchFromRelevantCusEntryNums;

			args.CleanForConcurrency = CleanForConcurrency;
		}

		CusContainerCollectionSynchroniser GetContainersSynchroniser()
		{
			var result = new CusContainerCollectionSynchroniser(Destination);
#if DEBUG
			result.LoadJobContainerForTesting = this.LoadJobContainerForTesting;
#endif
			return result;
		}

		void RemovePackSynchFromForwardingContainers()
		{
			foreach (BaseCusContainer container in Destination.CusContainers)
			{
				if (container.JobContainer != null)
				{
					((ForwardingContainer)container.JobContainer).PackLineSynchroniser = null;
				}
			}
		}

		void AddPackSynchToForwardingContainers(PackLineSynchronisers packLineSynchroniser)
		{
			foreach (BaseCusContainer container in Destination.CusContainers)
			{
				if (container.JobContainer != null)
				{
					((ForwardingContainer)container.JobContainer).PackLineSynchroniser = packLineSynchroniser;
				}
			}
		}

		protected virtual void RemovePackSynchFromRelevantCusEntryNums()
		{
		}

		protected virtual void AddPackSynchToRelevantCusEntryNums(PackLineSynchronisers packLineSynchroniser)
		{
		}

		protected virtual BillsSynchroniser GetBillsSynchroniser()
		{
			return new BillsSynchroniser(Destination, GetHouseBillOfSpecificShipment);
		}

		protected virtual PackingSynchroniser GetPackingSynchroniser()
		{
			return new PackingSynchroniser(this, Destination);
		}

		void CleanForConcurrency()
		{
			Destination.Bills.Reload(true, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "error msg, Aerror msg")]
		protected override void OnSynchronise(SynchroniseEventArgs e)
		{
			base.OnSynchronise(e);
			if (!SyncChangesDetected)
			{
				var idx = 0;
				var count = ConsolFieldSynchronisers.Count;
				FieldSynchroniser previousSynchroniser = null;

				try
				{
					foreach (var consolFieldSynchroniser in ConsolFieldSynchronisers.ToList())
					{
						consolFieldSynchroniser.DetectEnabled = DetectEnabled;
						consolFieldSynchroniser.Synchronise(IsEnabled);
						if (DetectEnabled && consolFieldSynchroniser.SyncChangesDetected)
						{
							SyncChangesDetected = true;
							return;
						}
						previousSynchroniser = consolFieldSynchroniser;
						idx++;
					}
				}
				catch (InvalidOperationException invalidOperationException)
				{
					var destination = previousSynchroniser?.Destination;
					var msgStrBuilder = new ZStringBuilder();
					msgStrBuilder.Append(ZString.Format((NoResString)"Failed at index {0} in {1} Synchronisers.", idx, count));
					msgStrBuilder.Append(ZString.Format((NoResString)"Previous Synchroniser was {0}.", previousSynchroniser?.ToString() ?? "null"));
					msgStrBuilder.Append(ZString.Format("Destination PropertyInfo was {0} - {1}.", destination?.ToString() ?? "null", destination?.PropertyDescriptor.Name ?? "null"));
					msgStrBuilder.Append(ZString.Format("DetectEnabled is {0}, SyncChangesDetected is {1}, previousSynchroniser.SyncChangesDetected is {2}.", DetectEnabled, SyncChangesDetected, previousSynchroniser?.SyncChangesDetected ?? false));

					ErrorReporter.ReportOnce(msgStrBuilder.ToStringWithNewLineBetweenAppends());
					throw invalidOperationException;
				}

				PackLineSynchronisers.Synchronise(IsEnabled);
				if (DetectEnabled && PackLineSynchronisers.SyncChangesDetected)
				{
					SyncChangesDetected = true;
					return;
				}
			}
		}

		protected override void OnDetectEnabledChanged()
		{
			base.OnDetectEnabledChanged();
			foreach (var consolFieldSynchroniser in ConsolFieldSynchronisers)
			{
				consolFieldSynchroniser.DetectEnabled = DetectEnabled;
			}
		}

		#endregion

		protected virtual IZType GetFkForOhSupplier()
		{
			return Source.ConsignorDocumentaryAddress.E2_AddressOverride ? ZGuid.Empty : Source.ConsignorDocumentaryAddress.OrganisationPK;
		}

		protected virtual IZType GetFkForOASupplier()
		{
			return Source.ConsignorDocumentaryAddress.E2_AddressOverride ? ZGuid.Empty : Source.ConsignorDocumentaryAddress.E2_OA_Address;
		}

		protected ZPropertyInfo[] GetJobDocAddressRelatedInfos(JobDocAddress sourceAddress)
		{
			return new ZPropertyInfo[] { sourceAddress.E2_OA_AddressInfo, sourceAddress.E2_AddressOverrideInfo, sourceAddress.OrganisationPKInfo };
		}

		protected virtual IZType GetFkForOhImporter()
		{
			return Source.ConsigneeDocumentaryAddress.E2_AddressOverride ? ZGuid.Empty : Source.ConsigneeDocumentaryAddress.OrganisationPK;
		}

		protected virtual IZType GetFkForOAImporter()
		{
			return Source.ConsigneeDocumentaryAddress.E2_AddressOverride ? ZGuid.Empty : Source.ConsigneeDocumentaryAddress.E2_OA_Address;
		}

		#region Hook and Unhook FieldSynchronisers

		protected virtual bool ShouldSyncDocAddressBeEditable(JobDocAddress sourceAddress)
		{
			return sourceAddress != null && (sourceAddress.Organisation == null || sourceAddress.Organisation.IsMiscellaneous);
		}

		protected virtual internal ZBool ShouldSynchroniseContainerModeWithPackingMode => ZBool.True;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			HookSupplierAndImporter();

			Synchronisers.Add(new FieldSynchroniser(Destination.JE_TransportModeInfo, GetConvertedTransportMode, () => new ZPropertyInfo[] { Source.JS_TransportModeInfo }));
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_RS_NKServiceLevelInfo, Source.JS_RS_NKServiceLevelInfo));

			if (ShouldSynchroniseContainerModeWithPackingMode)
			{
				HookContainerMode();
			}

			HookPortsOfOriginAndFinalDestination();
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_DateAtOriginInfo, Source.JS_E_DEPInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_DateAtFinalDestinationInfo, Source.JS_E_ARVInfo));
			HookWeight();
			HookVolume();

			FieldSynchroniser containerCountSynchroniser = new FieldSynchroniser(Destination.JE_ContainerCountInfo, GetContainerCount, GetContainerCountInfos);
			containerCountSynchroniser.Format += ContainerCountSynchroniser_Format;
			Synchronisers.Add(containerCountSynchroniser);

			AddPacksSynchroniser();

			HookGoodsDescription();
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_ShipmentIncoTermInfo, Source.JS_INCOInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_ScreeningStatusInfo, Source.JS_ScreeningStatusInfo));

			HookConsolToDeclarationSynchronisers();
			HookOwnerRefSynchronisers();
			HookJobDocAddressSynchronisers();
			PopulateOwnersRefIfRequired();
			Destination.JE_MessageTypeInfo.ValueChanged += UpdateSourceSelection;

			HookCollectionSynchronisers();
		}

		protected virtual void HookContainerMode()
		{
			var packingModeSynchroniser = new FieldSynchroniser(Destination.JE_ContainerModeInfo, Source.JS_PackingModeInfo);
			packingModeSynchroniser.Format += PackingModeSynchroniser_Format;
			Synchronisers.Add(packingModeSynchroniser);
		}

		protected virtual void HookSupplierAndImporter()
		{
			if (Destination.UseSupplierAddress)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_OA_SupplierAddressInfo, GetFkForOASupplier, () => GetJobDocAddressRelatedInfos(Source.ConsignorDocumentaryAddress), () => ShouldSyncDocAddressBeEditable(Source.ConsignorDocumentaryAddress)));
			}
			else
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_OH_SupplierInfo, GetFkForOhSupplier, () => GetJobDocAddressRelatedInfos(Source.ConsignorDocumentaryAddress), () => ShouldSyncDocAddressBeEditable(Source.ConsignorDocumentaryAddress)));
			}
			if (Destination.UseImporterAddress)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_OA_ImporterAddressInfo, GetFkForOAImporter, () => GetJobDocAddressRelatedInfos(Source.ConsigneeDocumentaryAddress), () => ShouldSyncDocAddressBeEditable(Source.ConsigneeDocumentaryAddress)));
			}
			else
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.JE_OH_ImporterInfo, GetFkForOhImporter, () => GetJobDocAddressRelatedInfos(Source.ConsigneeDocumentaryAddress), () => ShouldSyncDocAddressBeEditable(Source.ConsigneeDocumentaryAddress)));
			}
		}

		protected virtual void HookGoodsDescription()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_GoodsDescriptionInfo, Source.JS_GoodsDescriptionInfo));
		}

		#region Hook Weight
		protected virtual void HookWeight()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalWeightInfo, GetShipmentActualWeight, GetInfosAffecting_ShipmentActualWeight));
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalWeightUnitInfo, GetShipmentWeightUQ, GetInfosAffecting_ShipmentWeightUQ));
		}

		IZType GetShipmentActualWeight()
		{
			if (Source.IsBuyersConsolLead)
			{
				List<ForwardingShipment> allShipments;
				var needToConvert = GetDistinctWeightUnitsForBuyersConsol(out allShipments).Count() > 1;
				var allWeights = new List<ZWeight>();
				allShipments = new List<ForwardingShipment>(Source.CoLoadShipments.ToArray<ForwardingShipment>());
				allShipments.Add(Source);
				foreach (var s in allShipments)
				{
					allWeights.Add(new ZWeight(s.JS_ActualWeight, s.JS_UnitOfWeight));
				}

				if (needToConvert)
				{
					return (ZDecimal)allWeights.Sum(w => (decimal)w.InKilogramsSafe);
				}
				else
				{
					return (ZDecimal)allWeights.Sum(w => (decimal)w.Amount);
				}
			}
			else
			{
				return Source.JS_ActualWeight;
			}
		}

		ZPropertyInfo[] GetInfosAffecting_ShipmentActualWeight()
		{
			var properties = new List<ZPropertyInfo> { Source.JS_ActualWeightInfo, Source.JS_UnitOfWeightInfo };
			if (Source.IsBuyersConsolLead)
			{
				foreach (ForwardingShipment worker in Source.CoLoadShipments)
				{
					properties.Add(worker.JS_ActualWeightInfo);
					properties.Add(worker.JS_UnitOfWeightInfo);
				}
			}

			return properties.ToArray();
		}

		IZType GetShipmentWeightUQ()
		{
			if (Source.IsBuyersConsolLead)
			{
				List<ForwardingShipment> allShipments;
				return GetDistinctWeightUnitsForBuyersConsol(out allShipments).Count() > 1
							? new ZString(Core.Constants.Weight.Kilograms) : allShipments.First().JS_UnitOfWeight;
			}
			else
			{
				return Source.JS_UnitOfWeight;
			}
		}

		IEnumerable<ForwardingShipment> GetDistinctWeightUnitsForBuyersConsol(out List<ForwardingShipment> allShipments)
		{
			allShipments = new List<ForwardingShipment>(Source.CoLoadShipments.ToArray<ForwardingShipment>());
			allShipments.Add(Source);
			return IEnumerableExtensions.DistinctBy(allShipments, p => p.JS_UnitOfWeight);
		}

		ZPropertyInfo[] GetInfosAffecting_ShipmentWeightUQ()
		{
			var properties = new List<ZPropertyInfo> { Source.JS_UnitOfWeightInfo };
			if (Source.IsBuyersConsolLead)
			{
				foreach (ForwardingShipment worker in Source.CoLoadShipments)
				{
					properties.Add(worker.JS_UnitOfWeightInfo);
				}
			}
			return properties.ToArray();
		}

		#endregion

		#region Hook Volume

		protected virtual void HookVolume()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalVolumeInfo, GetShipmentActualVolume, GetInfosAffecting_ShipmentActualVolume));
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalVolumeUnitInfo, GetShipmentVolumeUQ, GetInfosAffecting_ShipmentVolumeUQ));
		}

		IZType GetShipmentActualVolume()
		{
			if (Source.IsBuyersConsolLead)
			{
				List<ForwardingShipment> allShipments;
				var needToConvert = GetDistinctVolumeUnitsForBuyersConsol(out allShipments).Count() > 1;
				var allVolumes = new List<ZVolume>();
				allShipments = new List<ForwardingShipment>(Source.CoLoadShipments.ToArray<ForwardingShipment>());
				allShipments.Add(Source);
				foreach (var s in allShipments)
				{
					allVolumes.Add(new ZVolume(s.JS_ActualVolume, s.JS_UnitOfVolume));
				}

				if (needToConvert)
				{
					var totalConvertedVolume = (ZDecimal)allVolumes.Sum(w => (decimal)w.InCubicMetres);
					return totalConvertedVolume.Round(3);
				}
				else
				{
					return (ZDecimal)allVolumes.Sum(w => (decimal)w.Amount);
				}
			}
			else
			{
				return Source.JS_ActualVolume;
			}
		}

		IZType GetShipmentVolumeUQ()
		{
			if (Source.IsBuyersConsolLead)
			{
				List<ForwardingShipment> allShipments;
				return GetDistinctVolumeUnitsForBuyersConsol(out allShipments).Count() > 1
							? new ZString(Core.Constants.Volume.CubicMetres) : allShipments.First().JS_UnitOfVolume;
			}
			else
			{
				return Source.JS_UnitOfVolume;
			}
		}

		ZPropertyInfo[] GetInfosAffecting_ShipmentVolumeUQ()
		{
			var properties = new List<ZPropertyInfo> { Source.JS_UnitOfVolumeInfo };
			if (Source.IsBuyersConsolLead)
			{
				foreach (ForwardingShipment worker in Source.CoLoadShipments)
				{
					properties.Add(worker.JS_UnitOfVolumeInfo);
				}
			}
			return properties.ToArray();
		}

		IEnumerable<ForwardingShipment> GetDistinctVolumeUnitsForBuyersConsol(out List<ForwardingShipment> allShipments)
		{
			allShipments = new List<ForwardingShipment>(Source.CoLoadShipments.ToArray<ForwardingShipment>());
			allShipments.Add(Source);
			return IEnumerableExtensions.DistinctBy(
						allShipments,
						p => p.JS_UnitOfVolume);
		}

		ZPropertyInfo[] GetInfosAffecting_ShipmentActualVolume()
		{
			var properties = new List<ZPropertyInfo> { Source.JS_ActualVolumeInfo, Source.JS_UnitOfVolumeInfo };
			if (Source.IsBuyersConsolLead)
			{
				foreach (ForwardingShipment worker in Source.CoLoadShipments)
				{
					properties.Add(worker.JS_ActualVolumeInfo);
					properties.Add(worker.JS_UnitOfVolumeInfo);
				}
			}

			return properties.ToArray();
		}

		#endregion

		protected virtual void HookPortsOfOriginAndFinalDestination()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_RL_NKOriginInfo, Source.JS_RL_NKOriginInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_RL_NKFinalDestinationInfo, Source.JS_RL_NKDestinationInfo));
		}

		bool GetPortOfLoadingShouldBeEditable()
		{
			return Source.ConsignorDocumentaryAddress.E2_AddressOverride && GetPortOfLoading().IsEmpty;
		}

		bool GetPortOfArrivalShouldBeEditable()
		{
			return Source.ConsigneeDocumentaryAddress.E2_AddressOverride && GetPortOfArrival().IsEmpty;
		}

		protected virtual void HookPortsOfLoadingAndArrival()
		{
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_RL_NKPortOfLoadingInfo, GetPortOfLoading, GetPortOfLoadingRelatedInfos, GetPortOfLoadingShouldBeEditable));
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_RL_NKPortOfArrivalInfo, GetPortOfArrival, GetPortOfArrivalRelatedInfos, GetPortOfArrivalShouldBeEditable));
		}

		protected virtual void HookJobDocAddressSynchronisers()
		{
		}

		protected void AddJobDocAddressFieldSynchroniserForDetection(JobDocAddress docAddress, ZPropertyInfoGuid info, Action<FieldSynchroniser> addSynchroniser)
		{
			if (docAddress.E2_OA_Address != info.Value)
			{
				SyncChangesDetected = true;
			}
			if (!docAddress.IsInDatabase && docAddress.IsEmpty) // delete dbo.JobDocAddress created by the getter
			{
				docAddress.Delete();
			}
			else
			{
				addSynchroniser(new FieldSynchroniser(docAddress.E2_OA_AddressInfo, info) { JobDocAddressPersistingSyncronizedValue = docAddress });
			}
		}

		protected virtual internal ZString GetHouseBillOfSpecificShipment(IBillDetails shipment)
		{
			return (ZString)shipment.BillNumberInfo.Value;
		}

		protected virtual IZType GetContainerCount()
		{
			return Source.JS_Calc_ContainerCount;
		}

		protected virtual ZPropertyInfo[] GetContainerCountInfos()
		{
			return new ZPropertyInfo[] { Source.JS_Calc_ContainerCountInfo };
		}

		protected virtual void AddPacksSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_TotalNoOfPacksInfo, GetSourceTotalNumberOfPacks, GetInfosAffecting_ShipmentTotalNumberOfPacks));
			FieldSynchroniser packTypeSyncroniser = new FieldSynchroniser(Destination.JE_TotalNoOfPacksPackTypeInfo, GetSourcePackType, GetInfosAffecting_ShipmentPackType);
			packTypeSyncroniser.Format += PackTypeSyncroniser_Format;
			Synchronisers.Add(packTypeSyncroniser);
		}

		IEnumerable<ZPropertyInfo> GetInfosAffecting_ShipmentPackType()
		{
			var properties = new List<ZPropertyInfo>() { Source.JS_F3_NKPackTypeInfo };
			if (Source.IsBuyersConsolLead)
			{
				foreach (ForwardingShipment worker in Source.CoLoadShipments)
				{
					properties.Add(worker.JS_F3_NKPackTypeInfo);
				}
			}
			return properties;
		}

		IZType GetSourcePackType()
		{
			var result = ZString.Empty;

			if (Source.IsBuyersConsolLead)
			{
				var allShipments = new List<ForwardingShipment>(Source.CoLoadShipments.ToArray<ForwardingShipment>());
				allShipments.Add(Source);
				var hasMultiplePackTypes = IEnumerableExtensions.DistinctBy(
						allShipments,
						p => p.JS_F3_NKPackType)
					.Count() > 1;
				result = hasMultiplePackTypes ? ZString.Empty : Source.JS_F3_NKPackType;
			}
			else
			{
				result = Source.JS_F3_NKPackType;
			}

			return GetConvertedPackType(result);
		}

		protected virtual ZString GetConvertedPackType(ZString packType)
		{
			return packType;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffecting_ShipmentTotalNumberOfPacks()
		{
			var properties = new List<ZPropertyInfo>() { Source.JS_OuterPacksInfo, Source.JS_F3_NKPackTypeInfo };
			if (Source.IsBuyersConsolLead)
			{
				foreach (ForwardingShipment worker in Source.CoLoadShipments)
				{
					properties.Add(worker.JS_OuterPacksInfo);
					properties.Add(worker.JS_F3_NKPackTypeInfo);
				}
			}
			return properties;
		}

		IZType GetSourceTotalNumberOfPacks()
		{
			if (Source.IsBuyersConsolLead)
			{
				var allShipments = new List<ForwardingShipment>(Source.CoLoadShipments.ToArray<ForwardingShipment>());
				allShipments.Add(Source);
				var hasMultiplePackTypes = IEnumerableExtensions.DistinctBy(
						allShipments,
						p => p.JS_F3_NKPackType)
					.Count() > 1;
				return (ZInt)(hasMultiplePackTypes ? 0 : allShipments.Sum(s => (int)s.JS_OuterPacks));
			}
			else
			{
				return Source.JS_OuterPacks;
			}
		}

		void ContainerCountSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(ZShort))
			{
				e.Value = ZShort.ParseSafe(e.Value.ToString(), ZShort.Zero);
			}
		}

		protected void PackTypeSyncroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(ZString))
			{
				e.Value = GetCustomsUnitForThisPackType((ZString)e.Value);
			}
		}

		protected override void UnHookSynchronisers()
		{
			Destination.JE_MessageTypeInfo.ValueChanged -= UpdateSourceSelection;

			base.UnHookSynchronisers();

			UnHookConsolToDeclarationSynchronisers();
			UnHookOwnerRefSynchronisers();
			UnHookCollectionSynchronisers();
		}

		#endregion

		#region Hook and UnHook CollectionSynchronisers

		void HookCollectionSynchronisers()
		{
			Source.Consols.CountChanged += Consols_CountChanged;
		}

		void UnHookCollectionSynchronisers()
		{
			Source.Consols.CountChanged -= Consols_CountChanged;
		}

		#endregion

		#region Hook and UnHook ConsolToDeclarationSynchronisers

		protected ForwardingConsol hookedConsol;

		protected virtual void HookConsolToDeclarationSynchronisers()
		{
			if (!SyncChangesDetected)
			{
				var consol = Destination.RelevantConsol;
				if (consol != null)
				{
					hookedConsol = consol;
					hookedConsol.JK_RL_NKDischargePortInfo.ValueChanged += UpdateConsolSelection;
					ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_DateOfFirstArrivalInfo, GetDateOfFirstArrival, GetDateOfFirstArrivalInfos));
					ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_VoyageFlightNoInfo, GetVoyageFlightNo, GetVoyageFlightNoInfo));
					ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_OH_ShippingLineInfo, GetCarrier, GetCarrierInfos));
					if (consol.IsImport())
					{
						if (DetectEnabled)
						{
							AddJobDocAddressFieldSynchroniserForDetection(Destination.ContainerTerminalOperatorDocAddress, (ZPropertyInfoGuid)consol.JK_OA_ArrivalCTOAddressInfo, x => ConsolFieldSynchronisers.Add(x));
							if (SyncChangesDetected)
							{
								return;
							}
							AddJobDocAddressFieldSynchroniserForDetection(Destination.DepotDocAddress, (ZPropertyInfoGuid)consol.JK_OA_UnpackDepotAddressInfo, x => ConsolFieldSynchronisers.Add(x));
							if (SyncChangesDetected)
							{
								return;
							}
						}
						else
						{
							ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo, consol.JK_OA_ArrivalCTOAddressInfo) { JobDocAddressPersistingSyncronizedValue = Destination.ContainerTerminalOperatorDocAddress });
							ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.DepotDocAddress.E2_OA_AddressInfo, consol.JK_OA_UnpackDepotAddressInfo) { JobDocAddressPersistingSyncronizedValue = Destination.DepotDocAddress });
						}
					}
					else
					{
						if (DetectEnabled)
						{
							AddJobDocAddressFieldSynchroniserForDetection(Destination.ContainerTerminalOperatorDocAddress, (ZPropertyInfoGuid)consol.JK_OA_DepartureCTOAddressInfo, x => ConsolFieldSynchronisers.Add(x));
							if (SyncChangesDetected)
							{
								return;
							}
							AddJobDocAddressFieldSynchroniserForDetection(Destination.DepotDocAddress, (ZPropertyInfoGuid)consol.JK_OA_PackDepotAddressInfo, x => ConsolFieldSynchronisers.Add(x));
							if (SyncChangesDetected)
							{
								return;
							}
						}
						else
						{
							ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.ContainerTerminalOperatorDocAddress.E2_OA_AddressInfo, consol.JK_OA_DepartureCTOAddressInfo) { JobDocAddressPersistingSyncronizedValue = Destination.ContainerTerminalOperatorDocAddress });
							ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.DepotDocAddress.E2_OA_AddressInfo, consol.JK_OA_PackDepotAddressInfo) { JobDocAddressPersistingSyncronizedValue = Destination.DepotDocAddress });
						}
					}
					ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_VesselNameInfo, GetVessel, GetVesselInfo));
					AddJE_RL_NKPortOfFirstArrivalSynchroniser(consol);

					hookedConsol.OnUpdatedByDataRefreshForCustomsSynchronisation -= new EventHandler(HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation);
					hookedConsol.OnUpdatedByDataRefreshForCustomsSynchronisation += new EventHandler(HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation);
				}

				HookPortsOfLoadingAndArrival();
				HookDateOfArrivalSynchroniser();
				ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_ExportDateInfo, GetExportDate, GetExportDateRelatedInfos));
			}
		}

		protected virtual void AddJE_RL_NKPortOfFirstArrivalSynchroniser(ForwardingConsol consol)
		{
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_RL_NKPortOfFirstArrivalInfo, GetFirstArrivalPort, () => new[] { consol.JK_RL_NKPortOfFirstArrivalInfo }));
		}

		protected IEnumerable<ZPropertyInfo> GetCarrierInfos()
		{
			var leg = GetMostInterestingLeg();
			if (leg != null)
			{
				yield return leg.CarrierPKInfo;
			}
			var relevantConsol = Destination.RelevantConsol;
			if (relevantConsol != null)
			{
				yield return relevantConsol.ShippingLinePKInfo;
			}
		}

		protected virtual IZType GetCarrier()
		{
			var result = ZGuid.Empty;
			var leg = GetMostInterestingLeg();
			if (leg != null && !leg.CarrierPK.IsEmpty)
			{
				result = leg.CarrierPK;
			}
			else
			{
				var relevantConsol = Destination.RelevantConsol;
				if (relevantConsol != null)
				{
					result = relevantConsol.ShippingLinePK;
				}
			}
			return result;
		}

		void HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation(object sender, EventArgs e)
		{
			if (Destination.RelevantConsol != hookedConsol)
			{
				if (hookedConsol != null)
				{
					hookedConsol.OnUpdatedByDataRefreshForCustomsSynchronisation -= new EventHandler(HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation);
					UnHookConsolToDeclarationSynchronisers();
				}
			}
		}

		protected virtual IZType GetMasterBill()
		{
			ZString result = ZString.Empty;
			if (hookedConsol != null)
			{
				result = hookedConsol.JK_MasterBillNum;
			}
			return result;
		}

		protected virtual ZPropertyInfo[] GetMasterBillInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_MasterBillNumInfo);
			}
			return infos.ToArray();
		}

		protected virtual IZType GetFirstArrivalPort()
		{
			var result = ZString.Empty;

			if (Destination.IsFirstArrivalDateAndPortUsed && hookedConsol != null)
			{
				result = hookedConsol.JK_RL_NKPortOfFirstArrival;
			}

			return result;
		}

		protected virtual ZPropertyInfo[] GetDateOfFirstArrivalInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_DatePortOfFirstArrivalInfo);
			}
			return infos.ToArray();
		}

		protected virtual IZType GetDateOfFirstArrival()
		{
			IZType result = ZDateTime.Empty;

			if (Destination.IsFirstArrivalDateAndPortUsed && hookedConsol != null && !hookedConsol.JK_DatePortOfFirstArrival.IsEmpty)
			{
				result = hookedConsol.JK_DatePortOfFirstArrival;
			}

			return result;
		}

		protected virtual ZPropertyInfo[] GetExportDateRelatedInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			infos.Add(Source.JS_ShippedOnBoardDateInfo);
			infos.Add(Source.JS_E_DEPInfo);
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_JX_JA_A_DEPInfo);
				infos.Add(hookedConsol.JK_JX_JA_E_DEPInfo);
			}
			return infos.ToArray();
		}

		protected virtual IZType GetExportDate()
		{
			var leg = (IMovementLeg)GetMostInterestingOutboundLeg(ShouldGetTransportsFromShipmentIfNoHookedConsol);

			if (leg != null && !leg.DepartureDate.IsEmpty)
			{
				return leg.DepartureDate;
			}
			if (!Source.JS_ShippedOnBoardDate.IsEmpty)
			{
				return Source.JS_ShippedOnBoardDate;
			}
			else
			{
				return Source.JS_E_DEP;
			}
		}

		protected virtual void HookDateOfArrivalSynchroniser()
		{
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.JE_DateOfArrivalInfo, GetDateOfArrival, GetDateOfArrivalRelatedInfos));
		}

		protected virtual ZPropertyInfo[] GetDateOfArrivalRelatedInfos()
		{
			var infos = new List<ZPropertyInfo>();
			infos.Add(Source.JS_E_ARVInfo);
			if (hookedConsol != null)
			{
				var transport = GetMostInterestingInboundLeg(ShouldGetTransportsFromShipmentIfNoHookedConsol);
				if (transport != null)
				{
					infos.Add(transport.JW_ATAInfo);
					infos.Add(transport.JW_ETAInfo);
				}
			}
			return infos.ToArray();
		}

		protected virtual IZType GetDateOfArrival()
		{
			var leg = (IMovementLeg)GetMostInterestingInboundLeg(ShouldGetTransportsFromShipmentIfNoHookedConsol);

			return leg != null && !leg.ArrivalDate.IsEmpty ? leg.ArrivalDate : Source.JS_E_ARV;
		}

		protected virtual IZType GetPortOfLoading()
		{
			var leg = (IMovementLeg)GetMostInterestingOutboundLeg(ShouldGetTransportsFromShipmentIfNoHookedConsol);

			var result = ZString.Empty;

			if (leg != null)
			{
				result = leg.Load;
			}
			else if (Source.Consols.Count == 0)
			{
				if (Destination.Supplier != null)
				{
					result = Destination.Supplier.OH_RL_NKClosestPort;
				}
			}

			return result;
		}

		protected virtual bool ShouldGetTransportsFromShipmentIfNoHookedConsol => false;

		Transport GetMostInterestingInboundLeg(bool getTransportsFromShipmentIfNoHookedConsol = false) => GetMostInterestingLeg((provider, legs) => provider.GetInboundLeg(legs), getTransportsFromShipmentIfNoHookedConsol);

		Transport GetMostInterestingOutboundLeg(bool getTransportsFromShipmentIfNoHookedConsol = false) => GetMostInterestingLeg((provider, legs) => provider.GetOutboundLeg(legs), getTransportsFromShipmentIfNoHookedConsol);

		Transport GetMostInterestingLeg() => Destination.IsExport ? GetMostInterestingOutboundLeg() : GetMostInterestingInboundLeg();

		Transport GetMostInterestingLeg(Func<MostInterestingLegProvider, IEnumerable<IMovementLeg>, IMovementLeg> getMovementLegFun, bool getTransportsFromShipmentIfNoHookedConsol)
		{
			TransportCollection transports = null;
			if (hookedConsol != null)
			{
				transports = hookedConsol.Transports;
			}
			else if (getTransportsFromShipmentIfNoHookedConsol)
			{
				transports = Source.Transports;
			}

			return transports == null ? null : (Transport)getMovementLegFun.Invoke(Destination.MostInterestingLegProvider, new TypedEnumerable<IMovementLeg>(transports));
		}

		protected virtual ZPropertyInfo[] GetPortOfLoadingRelatedInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_MessageTypeInfo);
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKLoadForExportTransportInfo);
				infos.Add(hookedConsol.JK_RL_NKLoadForImportTransportInfo);
			}
			if (Source.ConsignorDocumentaryAddress != null)
			{
				infos.Add(Source.ConsignorDocumentaryAddress.E2_AddressOverrideInfo);
			}
			return infos.ToArray();
		}

		protected virtual IZType GetPortOfArrival()
		{
			var leg = (IMovementLeg)GetMostInterestingInboundLeg(ShouldGetTransportsFromShipmentIfNoHookedConsol);

			var result = ZString.Empty;

			if (leg != null)
			{
				result = leg.Discharge;
			}
			else if (Source.Consols.Count == 0)
			{
				if (Destination.Importer != null)
				{
					result = Destination.Importer.OH_RL_NKClosestPort;
				}
			}

			return result;
		}

		protected virtual ZPropertyInfo[] GetPortOfArrivalRelatedInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_MessageTypeInfo);
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKDiscForImportTransportInfo);
				infos.Add(hookedConsol.JK_RL_NKDiscForExportTransportInfo);
			}
			if (Source.ConsigneeDocumentaryAddress != null)
			{
				infos.Add(Source.ConsigneeDocumentaryAddress.E2_AddressOverrideInfo);
			}
			return infos.ToArray();
		}

		protected IZType GetVessel()
		{
			var leg = GetMostInterestingLeg();
			return GetVesselFromTransportLeg(leg);
		}

		protected virtual IZType GetVesselFromTransportLeg(Transport leg)
		{
			return leg != null && leg.JW_TransportMode == Core.Constants.TransportModes.Sea ? leg.JW_Vessel : ZString.Empty;
		}

		protected IEnumerable<ZPropertyInfo> GetVesselInfo()
		{
			yield return Destination.JE_MessageTypeInfo;

			var transport = GetMostInterestingLeg();

			if (transport != null)
			{
				yield return transport.JW_LegOrderInfo;
				yield return transport.JW_TransportModeInfo;
				yield return transport.JW_RL_NKLoadPortInfo;
				yield return transport.JW_RL_NKDiscPortInfo;
				yield return transport.JW_VesselInfo;
			}
		}

		protected virtual IZType GetVoyageFlightNo()
		{
			var leg = GetMostInterestingLeg();

			return leg != null ? leg.JW_VoyageFlight : ZString.Empty;
		}

		protected IEnumerable<ZPropertyInfo> GetVoyageFlightNoInfo()
		{
			yield return Destination.JE_MessageTypeInfo;

			var transport = GetMostInterestingLeg();

			if (transport != null)
			{
				yield return transport.JW_LegOrderInfo;
				yield return transport.JW_RL_NKLoadPortInfo;
				yield return transport.JW_RL_NKDiscPortInfo;
				yield return transport.JW_VoyageFlightInfo;
			}
		}

		protected virtual void UnHookConsolToDeclarationSynchronisers()
		{
			if (hookedConsol != null)
			{
				hookedConsol.JK_RL_NKDischargePortInfo.ValueChanged -= UpdateConsolSelection;
				hookedConsol = null;
			}

			foreach (var synchroniser in ConsolFieldSynchronisers)
			{
				synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
			}
			ConsolFieldSynchronisers.Clear();

			PackLineSynchronisers.Dispose();
			packLineSynchronisers = null;
		}

		#endregion

		#region Hook and UnHook OwnerRefSynchronisers
		protected void HookOwnerRefSynchronisers()
		{
			Destination.JE_OwnerRef_ReadOnly = RegistrySetupOrConsigneeOverrideRequiresOwnerRefToPopulateWithOrderNumbers();
			Destination.JE_JSInfo.ValueChanged += JE_JS_Changed;
			Destination.DocsAndCartageAccessed += OnJobDocsAndCartageAccessed;
			Destination.AttachedOrdersAccessed += OnAttachedOrdersAccessed;
			Source.ImportExportChanged += OnOwnerRefNeedsUpdate;
			Source.ConsigneeChanged += OnOwnerRefNeedsUpdate;
		}

		void UnHookOwnerRefSynchronisers()
		{
			Destination.JE_OwnerRef_ReadOnly = false;
			Destination.JE_JSInfo.ValueChanged -= JE_JS_Changed;
			if (fHookedOrderItems_ForOwnersRef != null)
			{
				fHookedOrderItems_ForOwnersRef.CountChanged -= DestinationDocsAndCartage_CountChanged;
				UnHookOrderItems(fHookedOrderItems_ForOwnersRef);
				fHookedOrderItems_ForOwnersRef = null;
			}
			if (fHookedAttachedOrders_ForOwnersRef != null)
			{
				fHookedAttachedOrders_ForOwnersRef.CollectionCountChange -= DestinationAttachedOrders_CollectionCountChange;
				UnHookAttachedOrders(fHookedAttachedOrders_ForOwnersRef);
				fHookedAttachedOrders_ForOwnersRef = null;
			}
			Destination.DocsAndCartageAccessed -= OnJobDocsAndCartageAccessed;
			Destination.AttachedOrdersAccessed -= OnAttachedOrdersAccessed;
			Source.ImportExportChanged -= OnOwnerRefNeedsUpdate;
			Source.ConsigneeChanged -= OnOwnerRefNeedsUpdate;
		}

		protected void PopulateOwnersRefIfRequired()
		{
			if (!SyncChangesDetected
				&& !Destination.AutoAssignImporterRef
				&& !((IBusinessObjectFactoryInternals)Destination.Factory).IsProcessingOnAllTransactionsCommitted
				&& RegistrySetupOrConsigneeOverrideRequiresOwnerRefToPopulateWithOrderNumbers())
			{
				var wouldUpdate = Destination.UpdateOwnerRefFromOrderNumbers(applyChanges: !DetectEnabled);
				if (wouldUpdate && DetectEnabled)
				{
					SyncChangesDetected = true;
				}
			}
		}

		bool RegistrySetupOrConsigneeOverrideRequiresOwnerRefToPopulateWithOrderNumbers()
		{
			return (Source.IsImport() || Source.IsExport()) && Destination.IsPopulateOwnerRefWithOrderNumbersEnabled(Source.Consignee);
		}

		#endregion

		#region Packing Lines Syncronisation

		IZType GetConvertedTransportMode()
		{
			ZString result = ZString.Empty;

			foreach (Transport leg in Source.TransportsInLegOrder)
			{
				if (Source.IsImport() && leg.JW_RL_NKDiscPort.StartsWith(Destination.CountryCode)//first leg with a local discharge port
				|| !Source.IsImport() && !leg.JW_RL_NKDiscPort.StartsWith(Destination.CountryCode))//first port with a foreign discharge port
				{
					result = leg.JW_TransportMode;
					break;
				}
			}

			if (result.IsEmpty)
			{
				var consol = Destination.RelevantConsol;

				if (consol != null)
				{
					result = consol.JK_TransportMode;
				}
			}

			if (result.IsEmpty)
			{
				result = Source.JS_TransportMode;
			}

			return GetConvertedTransportMode(result);
		}

		protected virtual ZString GetConvertedTransportMode(ZString shipmentTransportMode)
		{
			return shipmentTransportMode.ToString() switch
			{
				Core.Constants.TransportModes.AirSea => Destination.IsExport ? TransportTypeList.Codes.Air : TransportTypeList.Codes.Sea,
				Core.Constants.TransportModes.SeaAir => Destination.IsExport ? TransportTypeList.Codes.Sea : TransportTypeList.Codes.Air,
				Core.Constants.TransportModes.Courier => TransportTypeList.Codes.Mail,
				_ => shipmentTransportMode
			};
		}

		protected virtual void PackingModeSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(ZString))
			{
				var shipmentPackingMode = (ZString)e.Value;
				var containerMode = Destination.GetContainerModeForDeclaration(Source.JS_TransportMode, shipmentPackingMode);
				if (!containerMode.IsEmpty)
				{
					e.Value = containerMode;
				}
			}
		}

		#endregion

		protected virtual void Consols_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (Destination.RelevantConsol != hookedConsol)
			{
				UpdateConsolSelection(sender, e);
			}
		}

		protected virtual void UpdateSourceSelection(object sender, EventArgs e)
		{
			UpdateConsolSelection(sender, e);
			UpdateShipmentSelection();
		}

		protected virtual void UpdateConsolSelection(object sender, EventArgs e)
		{
			if (!SyncChangesDetected)
			{
				UnHookConsolToDeclarationSynchronisers();
				HookConsolToDeclarationSynchronisers();
				if (!SyncChangesDetected)
				{
					foreach (var consolFieldSynchroniser in ConsolFieldSynchronisers)
					{
						consolFieldSynchroniser.Synchronise(IsEnabled);
						if (DetectEnabled && consolFieldSynchroniser.SyncChangesDetected)
						{
							SyncChangesDetected = true;
							return;
						}
					}

					if (hookedConsol != null)
					{
						PackLineSynchronisers.Synchronise(IsEnabled);
						if (DetectEnabled && PackLineSynchronisers.SyncChangesDetected)
						{
							SyncChangesDetected = true;
							return;
						}
					}
				}
			}
		}

		protected virtual void UpdateShipmentSelection()
		{
		}

		#region Synchronising JE_OwnerRef

		void JE_JS_Changed(object sender, EventArgs e)
		{
			OnOwnerRefNeedsUpdate(sender, e);
		}

		void OnJobDocsAndCartageAccessed(object sender, EventArgs e)
		{
			Destination.DocsAndCartageAccessed -= OnJobDocsAndCartageAccessed;
			try
			{
				fHookedOrderItems_ForOwnersRef = Destination.DocsAndCartage.OrderItems;
				fHookedOrderItems_ForOwnersRef.CountChanged -= DestinationDocsAndCartage_CountChanged;
				HookOrderItems(fHookedOrderItems_ForOwnersRef);
				fHookedOrderItems_ForOwnersRef.CountChanged += DestinationDocsAndCartage_CountChanged;
			}
			finally
			{
				Destination.DocsAndCartageAccessed += OnJobDocsAndCartageAccessed;
			}
		}
		OrderItemCollection fHookedOrderItems_ForOwnersRef;

		void HookOrderItems(OrderItemCollection orderItems)
		{
			orderItems.OfType<OrderItem>().WhereNotNull().ForEach(x =>
			{
				x.JT_OrderReferenceInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
				x.JT_OrderReferenceInfo.ValueChanged += OnOwnerRefNeedsUpdate;
			});
		}

		void UnHookOrderItems(OrderItemCollection orderItems)
		{
			orderItems.OfType<OrderItem>().WhereNotNull().ForEach(x =>
			{
				x.JT_OrderReferenceInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
			});
		}

		void DestinationDocsAndCartage_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var order = (OrderItem)e.BizObject;
			if (e.ItemAdded)
			{
				order.JT_OrderReferenceInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
				order.JT_OrderReferenceInfo.ValueChanged += OnOwnerRefNeedsUpdate;
			}
			else if (e.ItemRemoved)
			{
				order.JT_OrderReferenceInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
			}
			OnOwnerRefNeedsUpdate(sender, e);
		}

		void OnAttachedOrdersAccessed(object sender, EventArgs e)
		{
			Destination.AttachedOrdersAccessed -= OnAttachedOrdersAccessed;
			try
			{
				fHookedAttachedOrders_ForOwnersRef = Destination.AttachedOrders;
				fHookedAttachedOrders_ForOwnersRef.CollectionCountChange -= DestinationAttachedOrders_CollectionCountChange;
				HookAttachedOrders(fHookedAttachedOrders_ForOwnersRef);
				fHookedAttachedOrders_ForOwnersRef.CollectionCountChange += DestinationAttachedOrders_CollectionCountChange;
			}
			finally
			{
				Destination.AttachedOrdersAccessed += OnAttachedOrdersAccessed;
			}
		}
		OrderCollection fHookedAttachedOrders_ForOwnersRef;

		void HookAttachedOrders(OrderCollection orders)
		{
			orders.WhereNotNull().ForEach(x =>
			{
				x.JD_OrderNumberInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
				x.JD_OrderNumberInfo.ValueChanged += OnOwnerRefNeedsUpdate;
			});
		}

		void UnHookAttachedOrders(OrderCollection orders)
		{
			orders.WhereNotNull().ForEach(x =>
			{
				x.JD_OrderNumberInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
			});
		}

		void DestinationAttachedOrders_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is Order order)
			{
				if (e.ItemAdded)
				{
					order.JD_OrderNumberInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
					order.JD_OrderNumberInfo.ValueChanged += OnOwnerRefNeedsUpdate;
				}
				else if (e.ItemRemoved)
				{
					order.JD_OrderNumberInfo.ValueChanged -= OnOwnerRefNeedsUpdate;
				}
			}

			OnOwnerRefNeedsUpdate(sender, e);
		}

		void OnOwnerRefNeedsUpdate(object sender, EventArgs e)
		{
			PopulateOwnersRefIfRequired();
		}

		#endregion

		#region PackageTypeMapping

		protected virtual ZString GetCustomsUnitForThisPackType(ZString freightPackType)
		{
			return freightPackType;
		}

		#endregion

		#region Test
#if DEBUG
		public bool LoadJobContainerForTesting { get; set; }
#endif
		#endregion
	}
}
