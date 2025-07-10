using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	partial class BaseJobDeclaration : IRoutingSupport, ITransportParent
	{
		#region IRoutingSupport Implementation

		class BaseJobDeclarationRoutingSupport : IRoutingSupport
		{
			public BaseJobDeclarationRoutingSupport(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
			}
			readonly BaseJobDeclaration declaration;

			BusinessObjectFactory IRoutingSupport.Factory
			{
				get { return declaration.Factory; }
			}

			TransportCollection IRoutingSupport.Transports
			{
				get { return declaration.Transports; }
			}

			RoutingCollection IRoutingSupport.TransportsIncludingRelated
			{
				get { return declaration.TransportsIncludingRelated; }
			}

			ZString IRoutingSupport.TransportMode
			{
				get { return declaration.JE_TransportMode; }
			}

			string IRoutingSupport.AdditionalETAUpdateMsg
			{
				get { return Res.GetString("529e7e6e-4686-493b-9f3f-9f6607318c86", @"Note: A declaration on the same schedule cannot have a Destination date earlier than the Discharge date entered on this declaration.
The Destination date will be automatically updated for these declarations."); }
			}

			string IRoutingSupport.AdditionalETDUpdateMsg
			{
				get { return Res.GetString("18103bd1-6879-426f-ae1c-c2bdc3572809", @"Note: A declaration on the same schedule cannot have an Origin date later than the Loading date entered on this declaration.
The Origin date will be automatically updated for these declarations."); }
			}
		}

		TransportCollection IRoutingSupport.Transports
		{
			get { return RoutingSupportProvider.Transports; }
		}

		RoutingCollection IRoutingSupport.TransportsIncludingRelated
		{
			get { return RoutingSupportProvider.TransportsIncludingRelated; }
		}

		ZString IRoutingSupport.TransportMode
		{
			get { return RoutingSupportProvider.TransportMode; }
		}

		string IRoutingSupport.AdditionalETAUpdateMsg
		{
			get { return RoutingSupportProvider.AdditionalETAUpdateMsg; }
		}

		string IRoutingSupport.AdditionalETDUpdateMsg
		{
			get { return RoutingSupportProvider.AdditionalETDUpdateMsg; }
		}

		IRoutingSupport RoutingSupportProvider
		{
			get { return routingSupportProvider ?? (routingSupportProvider = GetRoutingSupportProvider()); }
		}
		IRoutingSupport routingSupportProvider;

		IRoutingSupport GetRoutingSupportProvider()
		{
			return (IRoutingSupport)Shipment ?? new BaseJobDeclarationRoutingSupport(this);
		}

		[ChildEditable]
		public DeclarationTransportCollection Transports
		{
			get
			{
				if (transports == null)
				{
					transports = GetNewDeclarationTransportCollection();
					transports.Load();
					RegisterEditableChildObject(transports);
				}

				return transports;
			}
		}
		DeclarationTransportCollection transports;

		protected virtual DeclarationTransportCollection GetNewDeclarationTransportCollection() => new DeclarationTransportCollection(this);

		public RoutingCollection TransportsIncludingRelated
		{
			get { return transportsIncludingRelated ?? (transportsIncludingRelated = GetNewTransportsIncludingRelated()); }
		}
		RoutingCollection transportsIncludingRelated;

		protected virtual RoutingCollection GetNewTransportsIncludingRelated()
		{
			return new RoutingCollection(this);
		}

		#endregion

		#region ITransportParent Basic Properties

		ZString ITransportParentCommon.TypeCode
		{
			get { return Constants.TransportParentTypes.Declaration; }
		}

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return GetNewTransportSupporter(); }
		}

		TransportCollection ITransportParent.Transports
		{
			get { return Transports; }
		}

		protected virtual TransportSupporter GetNewTransportSupporter()
		{
			return new BaseJobDeclarationTransportSupporter<BaseJobDeclaration>(this);
		}
		#endregion

		#region ITransportChangeNotifier Members

		void ITransportChangeNotifier.NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue)
		{
		}

		#endregion

		public void UpdateRoutingDefaultIfAllowed(ZPropertyInfo propertyInfo, IZType value)
		{
			UpdateRoutingDefaultIfAllowedCore(() => propertyInfo.Value = value);
		}

		public void UpdateRoutingDefaultIfAllowedAndSingleLeg(ZPropertyInfo propertyInfo, IZType value, Transport transport)
		{
			UpdateRoutingDefaultIfAllowedAndSingleLeg(() => propertyInfo.Value = value, transport);
		}

		void UpdateRoutingDefaultIfAllowedCore(Action doUpdate)
		{
			if (!RoutingDefaultInProgress && ShouldSynchroniseDatesWithRouting)
			{
				using (new RoutingDefaultLock(this))
				{
					doUpdate();
				}
			}
		}

		internal void UpdateRoutingDefaultIfAllowedAndSingleLeg(Action doUpdate, Transport transport)
		{
			if (Transports.Count == 1 && (transport == null || Transports[0] == transport))
			{
				UpdateRoutingDefaultIfAllowedCore(doUpdate);
			}
		}

		internal void UpdateRoutingDefaultIfAllowedEvenIfMultiLeg(Action doUpdate, Transport transport)
		{
			if (Transports.Count > 0 && (transport == null || Transports.Contains(transport)))
			{
				UpdateRoutingDefaultIfAllowedCore(doUpdate);
			}
		}

		bool ShouldSynchroniseDatesWithRouting
		{
			get
			{
				return !DeclarationMessagesHaveBeenSent() &&
					!ShouldSynchroniseWithShipment() &&
					!IsPluggedIntoShipment &&
					!AreTransportDetailsIrrelevant &&
					!HasUpdatedDatesLog;
			}
		}

		protected virtual bool HasUpdatedDatesLog
		{
			get { return false; }
		}

		public bool HasRoutingSupportDirectOnDeclaration
		{
			get { return RoutingSupportProvider is BaseJobDeclarationRoutingSupport; }
		}

		void UpdateRoutingIfRequired(ZPropertyInfo fieldChanged)
		{
			if (!IsCopying && !IsSettingDefaultValues && isPersistent && HasRoutingSupportDirectOnDeclaration)
			{
				if (!RoutingDefaultInProgress && ShouldSynchroniseDatesWithRouting)
				{
					using (new RoutingDefaultLock(this))
					{
						Transport mainTransport = null;
						if (Transports.Count == 0 && ShouldCreateRoutingRecord)
						{
							mainTransport = Transports.AddNew();
							mainTransport.JW_LegOrder = 1;

							if (ShouldLinkRouting)
							{
								mainTransport.JW_IsLinked = true;
							}

							DefaultTransportFromDeclaration(mainTransport, fieldChanged.Name);
						}

						if (Transports.Count == 1)
						{
							mainTransport = Transports[0];
						}
						else if (Transports.Count > 1)
						{
							mainTransport = FindMainTransport(fieldChanged);
						}

						if (mainTransport != null)
						{
							TransportUpdater updater = new TransportUpdater(fieldChanged);
							updater.Update(mainTransport);
						}
					}
				}
			}
		}

		Transport FindMainTransport(ZPropertyInfo fieldChanged)
		{
			if (fieldChanged.Name == JobDeclarationSchema.Constants.JE_ExportDate)
			{
				foreach (Transport transport in Transports)
				{
					if (transport.JW_RL_NKLoadPort == JE_RL_NKPortOfLoading)
					{
						return transport;
					}
				}
			}
			else if (fieldChanged.Name == JobDeclarationSchema.Constants.JE_DateOfArrival)
			{
				foreach (Transport transport in Transports)
				{
					if (transport.JW_RL_NKDiscPort == JE_RL_NKPortOfArrival)
					{
						return transport;
					}
				}
			}
			return null;
		}

		bool ShouldCreateRoutingRecord
		{
			get
			{
				var options = Customs.DataRegistry.Business.CustomsDataRegistry.Instance.RoutingIntegrationOptions.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
				return options.ConditionalLink ? HasMandatoryFieldsEntered : HasVoyageKeyFieldsEntered;
			}
		}

		bool ShouldLinkRouting
		{
			get
			{
				var options = Customs.DataRegistry.Business.CustomsDataRegistry.Instance.RoutingIntegrationOptions.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
				return options.AlwaysLink || options.ConditionalLink;
			}
		}

		void DefaultTransportFromDeclaration(Transport mainTransport, string fieldChanged)
		{
			if (!JE_RL_NKPortOfLoading.IsEmpty)
			{
				new TransportUpdater(JE_RL_NKPortOfLoadingInfo).Update(mainTransport);
			}

			if (!JE_RL_NKPortOfArrival.IsEmpty)
			{
				new TransportUpdater(JE_RL_NKPortOfArrivalInfo).Update(mainTransport);
			}

			if (!JE_VesselName.IsEmpty && fieldChanged != JobDeclarationSchema.JE_VesselName.Name)
			{
				new TransportUpdater(JE_VesselNameInfo).Update(mainTransport);
			}

			if (!JE_VoyageFlightNo.IsEmpty && fieldChanged != JobDeclarationSchema.JE_VoyageFlightNo.Name)
			{
				new TransportUpdater(JE_VoyageFlightNoInfo).Update(mainTransport);
			}

			if (!JE_OH_ShippingLine.IsEmpty && fieldChanged != JobDeclarationSchema.JE_OH_ShippingLine.Name)
			{
				new TransportUpdater(JE_OH_ShippingLineInfo).Update(mainTransport);
			}

			if (!JE_ExportDate.IsEmpty && fieldChanged != JobDeclarationSchema.JE_ExportDate.Name)
			{
				new TransportUpdater(JE_ExportDateInfo).Update(mainTransport);
			}

			if (!JE_DateOfArrival.IsEmpty && fieldChanged != JobDeclarationSchema.JE_DateOfArrival.Name)
			{
				new TransportUpdater(JE_DateOfArrivalInfo).Update(mainTransport);
			}

			if (!JE_TransportMode.IsEmpty && fieldChanged != JobDeclarationSchema.JE_TransportMode.Name)
			{
				new TransportUpdater(JE_TransportModeInfo).Update(mainTransport);
			}
		}

		bool HasVoyageKeyFieldsEntered
		{
			get
			{
				return !JE_TransportMode.IsEmpty
					&& !JE_VoyageFlightNo.IsEmpty
					&&
					(
						((IsAir || IsRoad) && !JE_ExportDate.IsEmpty)
						|| (IsRail && !JE_VesselName.IsEmpty)
						|| (IsSea && !JE_VesselName.IsEmpty && (!ShouldLinkRouting || !JE_OH_ShippingLine.IsEmpty))
					);
			}
		}

		bool HasMandatoryFieldsEntered
		{
			get
			{
				var result = !JE_TransportMode.IsEmpty && PortOfArrival != null && PortOfLoading != null;

				if (result)
				{
					if (IsAir)
					{
						result &= !JE_VoyageFlightNo.IsEmpty;
					}
					else if (IsRail || IsSea)
					{
						result &= !JE_VoyageFlightNo.IsEmpty && !JE_VesselName.IsEmpty;

						if (IsSea)
						{
							result &= Vessel != null;
							result &= ShippingLine != null;
						}
					}

					if (ImportExportHelper.IsImport(JE_RL_NKPortOfLoading, JE_RL_NKPortOfArrival))
					{
						result &= JE_DateOfArrival.IsValid;
					}
					else
					{
						result &= JE_ExportDate.IsValid;
					}
				}

				return result;
			}
		}

		class TransportUpdater
		{
			internal TransportUpdater(ZPropertyInfo fieldChanged)
			{
				this.fieldName = fieldChanged.Name;
				this.newValue = fieldChanged.Value;
			}

			readonly string fieldName;
			readonly IZType newValue;

			internal void Update(Transport mainTransport)
			{
				switch (fieldName)
				{
					case JobDeclarationSchema.Constants.JE_TransportMode:
						SetValue(mainTransport.JW_TransportModeInfo, delegate
						{ return mainTransport.JW_TransportMode_List.ContainsCode((ZString)newValue); });
						break;

					case JobDeclarationSchema.Constants.JE_VesselName:
						SetValue(mainTransport.JW_VesselInfo, delegate
						{ return !mainTransport.IsSea || !mainTransport.JW_IsLinked || VesselIsValid((ZString)newValue, mainTransport.Factory); });
						break;

					case JobDeclarationSchema.Constants.JE_VoyageFlightNo:
						SetValue(mainTransport.JW_VoyageFlightInfo, delegate
						{ return !mainTransport.IsAir || FlightCodeValidator.IsValid((ZString)newValue); });
						break;

					case JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading:
						SetValue(mainTransport.JW_RL_NKLoadPortInfo, delegate
						{ return PortIsValid((ZString)newValue, mainTransport.Factory); });
						break;

					case JobDeclarationSchema.Constants.JE_ExportDate:
						var departureFieldToUpdate = mainTransport.JW_ATD.IsValid ? mainTransport.JW_ATDInfo : mainTransport.JW_ETDInfo;
						SetValue(departureFieldToUpdate, null);
						break;

					case JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival:
						SetValue(mainTransport.JW_RL_NKDiscPortInfo, delegate
						{ return PortIsValid((ZString)newValue, mainTransport.Factory); });
						break;

					case JobDeclarationSchema.Constants.JE_DateOfArrival:
						var arrivalFieldToUpdate = mainTransport.JW_ATA.IsValid ? mainTransport.JW_ATAInfo : mainTransport.JW_ETAInfo;
						SetValue(arrivalFieldToUpdate, null);
						break;

					case JobDeclarationSchema.Constants.JE_OH_ShippingLine:
						SetValue(mainTransport.CarrierPKInfo, null);
						break;
				}
			}

			static bool PortIsValid(ZString portCode, BusinessObjectFactory factory)
			{
				ZQuery query = new ZQuery(RefUNLOCOSchema.RL_Code, portCode);
				query.AddToFilter(RefUNLOCOSchema.RL_IsActive, true);
				return factory.LoadTop1<RefUNLOCO>(query) != null;
			}

			static bool VesselIsValid(ZString vesselName, BusinessObjectFactory factory)
			{
				ZQuery query = new ZQuery(RefVesselSchema.RV_Code, vesselName);
				query.AddToFilter(RefVesselSchema.RV_IsActive, true);
				return factory.LoadTop1<RefVessel>(query) != null;
			}

			delegate bool ListValidator();

			void SetValue(ZPropertyInfo newField, ListValidator listValidator)
			{
				if (newValue.IsValid && (listValidator == null || listValidator()))
				{
					newField.Value = newValue;
				}
			}
		}

		public void UpdateFromRoutingTabOnLoadIfNoMessagesSentYet()
		{
			UpdateRoutingDefaultIfAllowedEvenIfMultiLeg(delegate
			{
				Transport transport = FindMainTransport(JE_ExportDateInfo);
				if (transport != null)
				{
					ZDateTime departure = transport.JW_ATD.IsValid ? transport.JW_ATD : transport.JW_ETD;
					if (departure.IsValid && JE_ExportDate != departure)
					{
						JE_ExportDate = departure;
					}
				}

				transport = FindMainTransport(JE_DateOfArrivalInfo);
				if (transport != null)
				{
					ZDateTime arrival = transport.JW_ATA.IsValid ? transport.JW_ATA : transport.JW_ETA;
					if (arrival.IsValid && JE_DateOfArrival != arrival)
					{
						JE_DateOfArrival = arrival;
					}
				}
			}, null);
		}

		#region RoutingDefaultLock

		bool RoutingDefaultInProgress
		{
			get { return routingDefaultIndex > 0; }
		}

		class RoutingDefaultLock : IDisposable
		{
			public RoutingDefaultLock(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
				declaration.routingDefaultIndex++;
			}

			readonly BaseJobDeclaration declaration;

			void IDisposable.Dispose()
			{
				declaration.routingDefaultIndex--;
			}
		}
		int routingDefaultIndex;

		#endregion
	}
}
