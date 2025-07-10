using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondHeaderShipmentSynchronizer : CusInBondHeaderCommonSynchronizer
	{
		public CusInBondHeaderShipmentSynchronizer(CusInBondHeader destination, ForwardingShipment source)
			: base(destination, source)
		{
			Source.OnJobDeclarationCreated += Shipment_OnJobDeclarationCreated;
		}

		public override ForwardingConsol RelevantConsol
		{
			get { return ConsolDataCalculator.RelevantConsol; }
		}

		protected new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		JobDeclaration Declaration
		{
			get { return Destination.Declaration; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			importerFieldSynchroniser = new FieldSynchroniser(Destination.BH_OA_ImporterInfo, GetImporter, GetImporterRelatedInfos);
			Synchronisers.Add(importerFieldSynchroniser);
			transportModeFieldSynchroniser = new FieldSynchroniser(Destination.BH_ImportTransportModeInfo, () => ConsolDataCalculator.GetInBondModeFromTransportAndPacking(), GetTransportModeRelatedInfos);
			Synchronisers.Add(transportModeFieldSynchroniser);
			carrierSCACFieldSynchroniser = new FieldSynchroniser(Destination.BH_CarrierSCACInfo, () => ConsolDataCalculator.GetCarrierSCAC(), GetGetCarrierSCACRelatedInfos);
			Synchronisers.Add(carrierSCACFieldSynchroniser);
			conveyanceNameFieldSynchroniser = new FieldSynchroniser(Destination.BH_ImportConveyanceNameInfo, () => ConsolDataCalculator.GetConveyanceName(), GetConveyanceNameRelatedInfos);
			Synchronisers.Add(conveyanceNameFieldSynchroniser);
			voyageFlightFieldSynchroniser = new FieldSynchroniser(Destination.BH_VoyageNumberInfo, () => ConsolDataCalculator.GetVoyageFlight(), GetVoyageFlightRelatedInfos);
			Synchronisers.Add(voyageFlightFieldSynchroniser);
			loadingPortFieldSynchroniser = new FieldSynchroniser(Destination.BH_ImportLoadPortKCodeInfo, () => ConsolDataCalculator.GetImportLoadingPort(), GetPortLoadingRelatedInfos);
			Synchronisers.Add(loadingPortFieldSynchroniser);
			sailingDateFieldSynchroniser = new FieldSynchroniser(Destination.BH_SailingDateInfo, () => ConsolDataCalculator.GetSailingDate(), GetSailingDateRelatedInfos);
			Synchronisers.Add(sailingDateFieldSynchroniser);
			eTADateFieldSynchroniser = new FieldSynchroniser(Destination.BH_ETAInfo, () => ConsolDataCalculator.GetETADate(), GetETADateRelatedInfos);
			Synchronisers.Add(eTADateFieldSynchroniser);
			exportCountryFieldSynchroniser = new FieldSynchroniser(Destination.BH_RN_NKFirstExportCountryInfo, () => ConsolDataCalculator.GetExportCountry(), GetExportCountryRelatedInfos);
			Synchronisers.Add(exportCountryFieldSynchroniser);
			firmsCodeSynchronuser = new FieldSynchroniser(Destination.BH_FIRMSInfo, () => ConsolDataCalculator.GetFIRMSCode(), GetFIRMSCodeRelatedInfos);
			Synchronisers.Add(firmsCodeSynchronuser);
			portOfArrivalFieldSynchroniser = new FieldSynchroniser(Destination.BH_PortUnladingDCodeInfo, () => ConsolDataCalculator.GetPortUnladingDCode(), GetPortUnladingDCodeRelatedInfos);
			Synchronisers.Add(portOfArrivalFieldSynchroniser);
			supplierFieldSynchroniser = new FieldSynchroniser(Destination.BH_OH_SupplierInfo, Source.ConsignorPKInfo);
			Synchronisers.Add(supplierFieldSynchroniser);

			AddMovementHeaderSynchroniser();

			AddBillSynchroniser();
		}
		FieldSynchroniser importerFieldSynchroniser;
		FieldSynchroniser transportModeFieldSynchroniser;
		FieldSynchroniser carrierSCACFieldSynchroniser;
		FieldSynchroniser conveyanceNameFieldSynchroniser;
		FieldSynchroniser voyageFlightFieldSynchroniser;
		FieldSynchroniser loadingPortFieldSynchroniser;
		FieldSynchroniser sailingDateFieldSynchroniser;
		FieldSynchroniser eTADateFieldSynchroniser;
		FieldSynchroniser exportCountryFieldSynchroniser;
		FieldSynchroniser firmsCodeSynchronuser;
		FieldSynchroniser portOfArrivalFieldSynchroniser;
		FieldSynchroniser supplierFieldSynchroniser;

		void AddBillSynchroniser()
		{
			var issuerCode = ConsolDataCalculator.MasterBillIssuerCode;
			var masterBill = ConsolDataCalculator.MasterBill;
			CusInBondBill bill = null;
			foreach (CusInBondBill existingBill in Destination.Bills.ToArray())
			{
				if (bill == null && existingBill.B0_MasterBillNumber == masterBill && existingBill.B0_IssuerCode == issuerCode)
				{
					bill = existingBill;
				}
				else if (!existingBill.ActiveInMessaging)
				{
					existingBill.Delete();
				}
			}
			var billSynchroniser = new CusInBondBillShipmentSynchronizer(bill ?? Destination.Bills.AddNew());
			Synchronisers.Add(billSynchroniser);
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
			var moveHeaderSynchroniser = new CusInBondMoveHeaderShipmentSynchronizer(firstMovementHeader);
			Synchronisers.Add(moveHeaderSynchroniser);
		}

		#region Implementation

		#region BH_FIRMS

		IEnumerable<ZPropertyInfo> GetFIRMSCodeRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.US_US_NKLocationOfGoodsInfo;
			}
		}

		#endregion

		#region BH_OA_Importer

		IZType GetImporter()
		{
			ZGuid result = ZGuid.Empty;
			var declaration = Declaration;
			if (declaration != null)
			{
				var importer = declaration.Importer;
				if (importer != null)
				{
					result = importer.MainAddress.PK;
				}
			}
			else
			{
				result = Source.ConsigneeDocumentaryAddress.E2_OA_Address;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetImporterRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_OH_ImporterInfo;
			}
			else
			{
				yield return Source.ConsigneeDocumentaryAddress.E2_OA_AddressInfo;
			}
		}

		#endregion

		#region BH_ImportTransportMode

		IEnumerable<ZPropertyInfo> GetTransportModeRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_TransportModeInfo;
				yield return declaration.JE_ContainerModeInfo;
			}
			else
			{
				yield return Source.JS_TransportModeInfo;
				yield return Source.JS_PackingModeInfo;
			}
		}

		#endregion

		#region BH_CarrierSCAC

		IEnumerable<ZPropertyInfo> GetGetCarrierSCACRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.US_UI_NKCarrierSCACInfo;
			}
			else
			{
				yield return Source.JS_RL_NKDestinationInfo;
				yield return Destination.BH_ImportTransportModeInfo;
			}
		}

		#endregion

		#region BH_ImportConveyanceName

		IEnumerable<ZPropertyInfo> GetConveyanceNameRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_VesselNameInfo;
			}
			else
			{
				foreach (var info in ConsolDataCalculator.GetInfosAffectingRelevantConsol())
				{
					yield return info;
				}
			}
		}

		#endregion

		#region BH_VoyageNumber

		IEnumerable<ZPropertyInfo> GetVoyageFlightRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_VoyageFlightNoInfo;
			}
			else
			{
				foreach (var info in ConsolDataCalculator.GetInfosAffectingRelevantConsol())
				{
					yield return info;
				}
			}
		}

		#endregion

		#region BH_ImportLoadPortKCode

		IEnumerable<ZPropertyInfo> GetPortLoadingRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.US_SchDLoadingInfo;
			}
			else
			{
				yield return Source.JS_TransportModeInfo;
				yield return Source.JS_RL_NKOriginInfo;
			}
		}

		#endregion

		#region BH_SailingDate

		IEnumerable<ZPropertyInfo> GetSailingDateRelatedInfos()
		{
			yield return Source.JS_E_DEPInfo;
		}

		#endregion

		#region BH_ETA

		IEnumerable<ZPropertyInfo> GetETADateRelatedInfos()
		{
			yield return Source.JS_E_ARVInfo;
		}

		#endregion

		#region BH_RN_NKFirstExportCountry

		IEnumerable<ZPropertyInfo> GetExportCountryRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.US_UC_NKCountryOfExportInfo;
			}
			else
			{
				yield return Source.JS_RL_NKOriginInfo;
			}
		}

		#endregion

		#region BH_PortUnladingDCode

		IEnumerable<ZPropertyInfo> GetPortUnladingDCodeRelatedInfos()
		{
			foreach (var info in GetDiscPortsInfos())
			{
				yield return info;
			}
		}

		IEnumerable<ZPropertyInfo> GetDiscPortsInfos()
		{
			var transports = ConsolDataCalculator.GetSortedTransports();

			foreach (Transport transport in transports)
			{
				if (transport.ZPropertyInfoHash.ContainsKey(Transport.Schema.JW_RL_NKDiscPort))
				{
					yield return transport.ZPropertyInfoHash[Transport.Schema.JW_RL_NKDiscPort];
				}
			}
		}

		#endregion

		internal IEnumerable<Transport> ConsolLegsToUS
		{
			get
			{
				var consol = RelevantConsol;
				if (consol != null)
				{
					consol.Transports.Load();
					foreach (var leg in consol.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder))
					{
						if (!leg.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.UnitedStates) && leg.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.UnitedStates))
						{
							yield return leg;
						}
					}
				}
			}
		}

		void Shipment_OnJobDeclarationCreated(object sender, JobDeclarationCreationEventArgs e)
		{
			Source.OnJobDeclarationCreated -= Shipment_OnJobDeclarationCreated;
			UpdateInfoEventsAndReSynchronise(importerFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(transportModeFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(carrierSCACFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(conveyanceNameFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(voyageFlightFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(loadingPortFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(sailingDateFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(eTADateFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(exportCountryFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(firmsCodeSynchronuser);
			UpdateInfoEventsAndReSynchronise(portOfArrivalFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(supplierFieldSynchroniser);
			Destination.DeclarationPKInfo.RefreshBinding(ZGuid.Empty);
		}

		internal CusInBondHeaderShipmentDataCalculator ConsolDataCalculator
		{
			get { return consolDataCalculator ?? (consolDataCalculator = new CusInBondHeaderShipmentDataCalculator(Source, Destination)); }
		}
		CusInBondHeaderShipmentDataCalculator consolDataCalculator;

		protected override void DisposeCore()
		{
			if (consolDataCalculator != null)
			{
				consolDataCalculator.Dispose();
				consolDataCalculator = null;
			}
			Source.OnJobDeclarationCreated -= Shipment_OnJobDeclarationCreated;
			base.DisposeCore();
		}

		#endregion
	}
}
