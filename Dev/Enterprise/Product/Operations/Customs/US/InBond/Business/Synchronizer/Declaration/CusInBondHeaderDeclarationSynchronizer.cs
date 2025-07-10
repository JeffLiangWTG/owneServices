using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondHeaderDeclarationSynchronizer : CusInBondHeaderCommonSynchronizer
	{
		internal CusInBondHeaderDeclarationSynchronizer(CusInBondHeader destination, JobDeclaration source)
			: base(destination, source)
		{
			destination.Factory.Saving += Factory_Saving;
		}

		public override ForwardingConsol RelevantConsol
		{
			get { return null; }
		}

		protected new JobDeclaration Source
		{
			get { return (JobDeclaration)base.Source; }
		}

		internal CusInBondCargoDescDeclarationSynchroniser GetOphantInBondCargoDescMatching(Package key)
		{
			CusInBondCargoDescDeclarationSynchroniser result = null;
			return OphantInBondCargoDesc.TryGetValue(key, out result) ? result : null;
		}

		internal void AddToOphantInBondCargoDesc(CusInBondCargoDescDeclarationSynchroniser value)
		{
			CusInBondCargoDescDeclarationSynchroniser existingValue = null;
			value.SetEnabled(false, value.DetectEnabled);
			var commodity = value.Destination;
			var key = value.Source;
			if (key.IsDeleting || key.IsDeleted)
			{
				if (!commodity.IsDeleted)
				{
					commodity.Delete();
				}
				RemoveFromOphantInBondCargoDesc(key);
			}
			else if (!commodity.IsDeleted && !commodity.IsDeleting)
			{
				commodity.BY_ParentID = ZGuid.Empty;
				if (OphantInBondCargoDesc.TryGetValue(key, out existingValue))
				{
					var existingCommodity = existingValue.Destination;
					if (!existingCommodity.IsDeleted && existingValue != value)
					{
						existingCommodity.Delete();
					}
					existingValue.Dispose();
					OphantInBondCargoDesc[key] = value;
				}
				else
				{
					OphantInBondCargoDesc.Add(key, value);
				}
			}
		}

		internal void RemoveFromOphantInBondCargoDesc(Package key)
		{
			if (OphantInBondCargoDesc.ContainsKey(key))
			{
				OphantInBondCargoDesc.Remove(key);
			}
		}

		Dictionary<Package, CusInBondCargoDescDeclarationSynchroniser> OphantInBondCargoDesc
		{
			get { return ophantInBondCargoDesc ?? (ophantInBondCargoDesc = new Dictionary<Package, CusInBondCargoDescDeclarationSynchroniser>()); }
		}
		Dictionary<Package, CusInBondCargoDescDeclarationSynchroniser> ophantInBondCargoDesc;

		void CleanUpOphantInBondCargoDesc()
		{
			if (ophantInBondCargoDesc != null)
			{
				foreach (var pair in ophantInBondCargoDesc)
				{
					var synchroniser = pair.Value;
					var commodity = synchroniser.Destination;
					if (!commodity.IsDeleted)
					{
						commodity.Delete();
					}
				}
				ophantInBondCargoDesc.Clear();
			}
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			CleanUpOphantInBondCargoDesc();
		}

		protected override void DisposeCore()
		{
			if (Destination != null)
			{
				Destination.Factory.Saving -= Factory_Saving;
			}
			base.DisposeCore();
		}

		protected override void ForceSynchroniseCore()
		{
			base.ForceSynchroniseCore();
			CleanUpOphantInBondCargoDesc();
		}

		protected override void UnHookSynchronisers()
		{
			CleanUpOphantInBondCargoDesc();
			base.UnHookSynchronisers();
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_OA_ImporterInfo, GetImporter, GetImporterRelatedInfos));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_ImportTransportModeInfo, GetInBondModeFromTransportAndContainerMode, GetTransportModeRelatedInfos));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_CarrierSCACInfo, Source.US_UI_NKCarrierSCACInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_ImportConveyanceNameInfo, Source.JE_VesselNameInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_VoyageNumberInfo, GetVoyageFlight, GetVoyageFlightRelatedInfos));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_ImportLoadPortKCodeInfo, Source.US_SchDLoadingInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_ETAInfo, Source.JE_DateOfArrivalInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_SailingDateInfo, Source.JE_DateAtOriginInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_FirstExportDateInfo, Source.US_DateOfExportInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_RN_NKFirstExportCountryInfo, Source.US_UC_NKCountryOfExportInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_PortUnladingDCodeInfo, Source.US_SchDArrivalInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_FIRMSInfo, Source.US_US_NKLocationOfGoodsInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_OH_SupplierInfo, Source.JE_OH_SupplierInfo));

				AddMovementHeaderSynchroniser();
				Synchronisers.Add(new CusInBondBillCollectionDeclarationSynchronizer(Source, Destination));

				Source.JE_TransportModeInfo.ValueChanged -= JE_TransportMode_ValueChanged;
				Source.JE_TransportModeInfo.ValueChanged += JE_TransportMode_ValueChanged;
			}
		}

		void JE_TransportMode_ValueChanged(object sender, EventArgs e)
		{
			Synchronise();
		}

		void AddMovementHeaderSynchroniser()
		{
			var firstMovementHeader = Destination.MovementHeader;
			foreach (CusInBondMoveHeader movementHeader in Destination.MovementHeaders.ToArray())
			{
				if (movementHeader != firstMovementHeader && !movementHeader.ActiveInMessaging)
				{
					movementHeader.Delete();
				}
			}
			var moveHeaderSynchroniser = new CusInBondMoveHeaderDeclarationSynchronizer(firstMovementHeader);
			Synchronisers.Add(moveHeaderSynchroniser);
		}

		#region Implementation

		#region BH_OA_Importer

		IZType GetImporter()
		{
			ZGuid result = ZGuid.Empty;
			var importer = Source.Importer;
			if (importer != null)
			{
				result = importer.MainAddress.PK;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetImporterRelatedInfos()
		{
			yield return Source.JE_OH_ImporterInfo;
		}

		#endregion

		#region BH_ImportTransportMode

		IZType GetInBondModeFromTransportAndContainerMode()
		{
			ZString result = ZString.Empty;

			switch (Source.JE_TransportMode)
			{
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Air:
					result = InBondTransportModeCodes.Codes.AirNonContainer;
					break;
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail:
					result = InBondTransportModeCodes.Codes.RailNonContainer;
					break;
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Truck:
					result = InBondTransportModeCodes.Codes.TruckNonContainer;
					break;
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea:
					result = Source.IsContainerised ? InBondTransportModeCodes.Codes.VesselContainer : InBondTransportModeCodes.Codes.VesselNonContainer;
					break;
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.FixedTransportInstallations:
					result = InBondTransportModeCodes.Codes.FixedTransportInstallations;
					break;
			}

			return result;
		}

		IEnumerable<ZPropertyInfo> GetTransportModeRelatedInfos()
		{
			yield return Source.JE_TransportModeInfo;
			yield return Source.JE_ContainerModeInfo;
		}

		#endregion

		#region BH_VoyageNumber

		IZType GetVoyageFlight()
		{
			var result = Source.JE_VoyageFlightNo;
			if (Source.IsAir)
			{
				result = GetFlightNumberWithoutSCAC(result);
			}
			return result;
		}

		ZString GetFlightNumberWithoutSCAC(ZString flightNumber)
		{
			var result = flightNumber;
			var scac = result.SubstringSafe(0, 2);
			var containsSCAC = false;
			if (!scac.IsEmpty)
			{
				containsSCAC = Source.Factory.Load<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, scac)).Length > 0;
			}
			if (containsSCAC)
			{
				result = result.SubstringSafe(2);
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetVoyageFlightRelatedInfos()
		{
			yield return Source.JE_VoyageFlightNoInfo;
			yield return Source.JE_TransportModeInfo;
		}

		#endregion

		#endregion
	}
}
