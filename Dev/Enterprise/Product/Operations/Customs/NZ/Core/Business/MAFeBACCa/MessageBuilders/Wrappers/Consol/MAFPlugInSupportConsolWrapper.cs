namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business.MultiLineAddInfos;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.DocumentEngineCore.DocumentSupport;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.ZArchitecture.Business;

	public partial class MAFPlugInSupportConsolWrapper : IMAFPlugInSupport, IMAFValidator
	{
		public MAFPlugInSupportConsolWrapper(ForwardingConsol consol)
		{
			Argument.NotNull(consol, "consol");
			this.consol = consol;
		}

		#region Implementation of IMAFPlugInSupport

		Logs IMAFPlugInSupport.Logs
		{
			get { return consol.Logs; }
		}

		bool IMAFPlugInSupport.IsPlugInNew
		{
			get
			{
				var collection = new CusAddInfoCollection<NZAddInfo>(consol);
				collection.Load();
				return collection.Count == 0 || !collection[0].IsInDatabase;
			}
		}

		bool IMAFPlugInSupport.PlugInVisible
		{
			get { return consol.IsImport() && (consol.IsSea || consol.IsAir); }
		}

		bool IMAFPlugInSupport.CargoTypeVisible
		{
			get { return true; }
		}

		event EventHandler IMAFPlugInSupport.PlugInVisibilityDataChanged
		{
			add
			{
				consol.JK_RL_NKLoadPortInfo.ValueChanged += value;
				consol.JK_RL_NKDischargePortInfo.ValueChanged += value;
				consol.JK_TransportModeInfo.ValueChanged += value;
			}
			remove
			{
				consol.JK_RL_NKLoadPortInfo.ValueChanged -= value;
				consol.JK_RL_NKDischargePortInfo.ValueChanged -= value;
				consol.JK_TransportModeInfo.ValueChanged -= value;
			}
		}

		IDocumentEvents IMAFPlugInSupport.DocumentEvents
		{
			get { return consol.DocumentSupporter; }
		}

		#endregion

		#region Implementation of IMAFMessagingSource

		BusinessObject IMAFMessagingSource.Master
		{
			get { return consol; }
		}

		IMAFOrganisation IMAFMessagingSource.Broker
		{
			get
			{
				return MAFOrganisationWrapper.GetMAFOrganisation(
					NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant(),
					GlbCompany.CurrentCompany,
					GlbBranch.CurrentBranch,
					GlbStaff.CurrentUser);
			}
		}

		ZString IMAFMessagingSource.OriginCountry
		{
			get { return consol.JK_RL_NKLoadPort.Left(2); }
		}

		IEnumerable<ZString> IMAFMessagingSource.DischargePorts
		{
			get
			{
				var transport = consol.Transports.ImportTransport;
				yield return transport == null ? ZString.Empty : transport.JW_RL_NKDiscPort;
			}
		}

		IEnumerable<ZString> IMAFMessagingSource.Destinations
		{
			get { return (from ForwardingShipment shipment in consol.Shipments select shipment.JS_RL_NKDestination).Distinct(); }
		}

		ZString IMAFMessagingSource.ShipName
		{
			get
			{
				var transport = consol.Transports.ImportTransport;
				return transport == null ? ZString.Empty : transport.JW_Vessel;
			}
		}

		ZString IMAFMessagingSource.VoyageNumber
		{
			get
			{
				var transport = consol.Transports.ImportTransport;
				return transport == null ? ZString.Empty : transport.JW_VoyageFlight;
			}
		}

		ZString IMAFMessagingSource.ShippingCompany
		{
			get { return consol.ShippingLine != null ? consol.ShippingLine.OH_FullNameTruncated : ZString.Empty; }
		}

		ZDateTime IMAFMessagingSource.VoyageArrivalDate
		{
			get
			{
				var transport = consol.Transports.ImportTransport;
				return transport == null ? ZDateTime.Empty : transport.JW_ATA;
			}
		}

		ZString IMAFMessagingSource.FlightNumber
		{
			get
			{
				if (consol.IsAir)
				{
					var transport = consol.Transports.ImportTransport;
					return transport == null ? ZString.Empty : transport.JW_VoyageFlight;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		ZDateTime IMAFMessagingSource.FlightArrivalDate
		{
			get
			{
				if (consol.IsAir)
				{
					var transport = consol.Transports.ImportTransport;
					return transport == null ? ZDateTime.Empty : transport.JW_ATA;
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		IEnumerable<ZString> IMAFMessagingSource.BillOfLadingNumbers
		{
			get { yield return consol.JK_MasterBillNum; }
		}

		IEnumerable<ZString> IMAFMessagingSource.SubBillOfLadingNumbers
		{
			get { return from ForwardingShipment shipment in consol.Shipments select shipment.JS_HouseBill; }
		}

		IEnumerable<IMAFContainer> IMAFMessagingSource.Containers
		{
			get { return from ForwardingContainer container in consol.Containers select (IMAFContainer)new MAFForwardingContainerWrapper(container); }
		}

		ZString IMAFMessagingSource.ConsignmentDescription
		{
			get { return CargoTypeList.Codes.Fak; }
		}

		IEnumerable<IMAFCommodity> IMAFMessagingSource.Commodities
		{
			get { yield return new MAFCommodityConsolWrapper(consol); }
		}

		ZInt IMAFMessagingSource.CustomsEntryNumber
		{
			get { return ZInt.Zero; }
		}

		bool IMAFMessagingSource.IsECIWriteoff
		{
			get { return false; }
		}

		ZString IMAFMessagingSource.ClientReferenceCoverSheet
		{
			get { return consol.JK_UniqueConsignRef + (consol.JK_AgentsReference.IsEmpty ? "" : " / " + consol.JK_AgentsReference); }
		}

		IMAFOrganisation IMAFMessagingSource.TransitionalFacility
		{
			get { return MAFOrganisationWrapper.GetMAFOrganisation(consol.UnpackDepotAddress, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, null, true); }
		}

		IMAFOrganisation IMAFMessagingSource.TreatmentProvider
		{
			get { return null; }
		}

		#endregion

		#region Implementation of IMAFMessagingFallback

		IMAFOrganisation IMAFMessagingFallback.Importer
		{
			get { return MAFOrganisationWrapper.GetMAFOrganisation(consol.ReceivingForwarder, OrgCusCode.CodeTypes.CustomsClientCode, ContactType.Consignee); }
		}

		IMAFOrganisation IMAFMessagingFallback.Exporter
		{
			get { return MAFOrganisationWrapper.GetMAFOrganisation(consol.SendingForwarder, OrgCusCode.CodeTypes.SupplierCode); }
		}

		ZString IMAFMessagingFallback.ProcessingOffice
		{
			get
			{
				var transport = consol.Transports.ImportTransport;
				return transport == null ? ZString.Empty : transport.JW_RL_NKDiscPort;
			}
		}

		ZString IMAFMessagingFallback.ConsignmentType
		{
			get { return ConsignmentTypeList.Codes.CommercialCargo; }
		}

		ZString IMAFMessagingFallback.CargoType
		{
			get { return CargoTypeList.Codes.Fak; }
		}

		ZString IMAFMessagingFallback.MeasurementUQ
		{
			get { return MeasurementUQList.Codes.unit; }
		}

		ZInt IMAFMessagingFallback.MeasurementValue
		{
			get { return consol.JK_TotalShipmentQuantity.ToZInt(); }
		}

		IMAFAccountDetails IMAFMessagingFallback.AccountDetails
		{
			get { return MAFAccountDetailsWrapper.GetAccountDetails(new Dictionary<string, OrgHeader> { { "Branch Organization", GlbBranch.CurrentBranch.OrgProxy } }); }
		}

		#endregion

		#region Implementation of IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return consol.DocManagerInfo; }
		}

		#endregion

		#region Implementation of IHaveNZAddInfo

		NZAddInfo IHaveNZAddInfo.AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					var collection = new CusAddInfoCollection<NZAddInfo>(consol);
					collection.Load();
					consol.RegisterEditableChildObject(collection);
					addInfo = (NZAddInfo)(collection.GetInners().FirstOrDefault()) ?? collection.AddNew().Data;
				}
				return addInfo;
			}
		}

		NZAddInfo addInfo;

		#endregion

		#region Implementation of IJobNumber

		string IJobNumber.JobNumber
		{
			get { return ((IJobNumber)consol).JobNumber.Replace("~", ""); }
		}

		#endregion

		#region Implementation of IMAFValidator

		ZString IMAFValidator.GetWarningMessage()
		{
			var containerMode = consol.JK_ConsolMode;
			if (consol.TransportMode != JobTransportModeList.Codes.Air)
			{
				if (containerMode != Core.Constants.ContainerModes.Groupage && containerMode != Core.Constants.ContainerModes.FreightAllKind)
				{
					return string.Format(@"Consol eBACCa is generally required for GRP (Groupage/Freight All Kinds) consols only.
But this Consol’s container mode is {0} ({1}).", containerMode, consol.JK_ConsolMode_List.GetDescriptionFromCode(containerMode));
				}
			}
			return ZString.Empty;
		}

		ZString IMAFValidator.GetErrorMessage()
		{
			return consol.Shipments.Count == 0 ? "You must enter at least one shipment." : string.Empty;
		}

		#endregion

		readonly ForwardingConsol consol;
	}
}
