using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class TransportCollection : DependentBusinessObjectCollection<Transport, BusinessObject>, ITransportCollection
	{
		public TransportCollection(ITransportParentCommon parent)
			: base((BusinessObject)parent)
		{
			this.parent = parent;
			this.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(ResetParentScreeningStatus);
		}
		readonly ITransportParentCommon parent;

		Transport ITransportCollection.this[int index]
		{
			get { return (Transport)(Elements[index]); }
		}

		public Transport AddNew(string from, string to)
		{
			var transport = AddNew();
			transport.JW_RL_NKLoadPort = from;
			transport.JW_RL_NKDiscPort = to;

			return transport;
		}

		#region Overrides

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobConsolTransportSchema.JW_ParentGUID; }
		}

		public bool HasLoadPortOutsideOfIcs2Zone
		{
			get
			{
				return this.Cast<Transport>().Any(t =>
				{
					if (t.IsAir)
					{
						var loadPortRefUnloco = t.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, t.JW_RL_NKLoadPort);
						return loadPortRefUnloco != null && !loadPortRefUnloco.IsInIcs2Zone;
					}

					return false;
				});
			}
		}

		public bool HasIcs2ZoneAirDischarge
		{
			get
			{
				return this.Cast<Transport>().Any(t =>
				{
					if (t.IsAir)
					{
						var dischargeRefUnloco = t.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, t.JW_RL_NKDiscPort);
						return dischargeRefUnloco != null && dischargeRefUnloco.IsInIcs2Zone;
					}

					return false;
				});
			}
		}

		public bool IsAirImportOrTransitToICS2Zone
		{
			get
			{
				return this.Cast<Transport>().Any(t =>
					{
						if (t.IsAir)
						{
							var loadPortRefUnloco = t.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, t.JW_RL_NKLoadPort);
							var dischargeRefUnloco = t.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, t.JW_RL_NKDiscPort);

							return loadPortRefUnloco != null && !loadPortRefUnloco.IsInIcs2Zone && dischargeRefUnloco != null && dischargeRefUnloco.IsInIcs2Zone;
						}

						return false;
					});
			}
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			var transport = (Transport)dependent;
			transport.ParentType = parent.GetType();
			transport.JW_ParentType = parent.TypeCode;
			transport.ParentTypeDebugLog.AppendLine("SetCollectionRelationships has been called: "
													+ "\r\nJW_ParentGUID: " + transport.JW_ParentGUID
													+ "\r\nJW_ParentType: " + transport.JW_ParentType
													+ "\r\nJW_IsLinked: " + transport.JW_IsLinked);

			base.SetCollectionRelationships(dependent);
		}
		
		protected override void SetDefaultsForNewChild(BusinessObject newChild)
		{
			base.SetDefaultsForNewChild(newChild);

			var newTransport = (Transport)newChild;
			var supporter = newTransport.TransportSupporter;
			if (supporter != null)
			{
				var newTransportMode = supporter.TransportMode;
				if (!newTransportMode.IsValid || !newTransport.JW_TransportMode_List.ContainsCode(newTransportMode))
				{
					newTransportMode = "";
				}

				newTransport.JW_TransportMode = newTransportMode;

				if (newTransport.JW_TransportMode == TransportModes.Air)
				{
					newTransport.JW_Status = TransportStatus.Planned;
				}
			}

			newTransport.JW_IsCargoOnly = FreightDataRegistry.Instance.CargoOnlyVoyageDefault.Value;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JobConsolTransportSchema.JW_ParentType, parent.TypeCode);
			return result;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			Transport transport = (Transport)bizOAdded;
			base.OnAdded(transport);

			transport.ParentType = parent.GetType();
		}

		internal void SetDefaultsAsThoughForNewChild(Transport transport)
		{
			SetDefaultsForNewChild(transport);
		}

		#endregion

		#region FindTransportByLoadPort / FindTransportByDischargePort

		public Transport FindTransportByLoadPort(ZString loadPort)
		{
			foreach (Transport transport in this)
			{
				if (transport.JW_RL_NKLoadPort == loadPort)
				{
					return transport;
				}
			}
			return null;
		}

		public Transport FindTransportByDischargePort(ZString dischargePort)
		{
			foreach (Transport transport in this)
			{
				if (transport.JW_RL_NKDiscPort == dischargePort)
				{
					return transport;
				}
			}
			return null;
		}

		#endregion

		public bool IsAnyDischargeInCountry(string countryCode)
		{
			foreach (Transport transport in this)
			{
				if (transport.JW_RL_NKDiscPort.SubstringSafe(0, 2) == countryCode)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsAnyLoadInCountry(string countryCode)
		{
			foreach (Transport transport in this)
			{
				if (transport.JW_RL_NKLoadPort.SubstringSafe(0, 2) == countryCode)
				{
					return true;
				}
			}
			return false;
		}

		void ResetParentScreeningStatus(object sender, EventArgs e)
		{
			if (HasChanges)
			{
				var parentProvider = parent as IShouldUpdateScreeningStatus;
				if (parentProvider != null)
				{
					var parentStatusProvider = parent as IScreeningStatusProvider;
					var parentBizo = parent as BusinessObject;
					var clearStatus = new List<string>() { ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.PermanentClear, ScreeningStatusesList.Codes.JobCleared };
					foreach (Transport transport in this)
					{
						var transportI = transport as IShouldUpdateScreeningStatus;
						if (transportI != null && transportI.ShouldUpdateScreeningStatus)
						{
							if (parentStatusProvider != null && parentBizo != null && !parentBizo.IsDeleted && (parentStatusProvider.ScreeningStatus == ScreeningStatusesList.Codes.JobCleared))
							{
								if (SetParentShouldUpdateScreeningStatus(transport, parentProvider, clearStatus))
								{ return; }
							}
							else
							{
								parentProvider.ShouldUpdateScreeningStatus = true;
								return;
							}
						}
					}
				}
			}
		}

		bool SetParentShouldUpdateScreeningStatus(Transport transport, IShouldUpdateScreeningStatus parentProvider, List<string> exceptedStatus)
		{
			var result = false;
			if (!transport.IsDeleted)
			{
				if (transport.JW_IsLinked)
				{
					if (transport.Vessel != null && !transport.Vessel.IsDeleted)
					{
						if (TryChangeParentShouldUpdateScreeningStatus(transport.Vessel, parentProvider, exceptedStatus))
						{ result = true; }
					}
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(transport.JW_Vessel))
					{
						if (TryChangeParentShouldUpdateScreeningStatus(transport, parentProvider, exceptedStatus))
						{ result = true; }
					}
				}
			}
			return result;
		}

		bool TryChangeParentShouldUpdateScreeningStatus(BusinessObject bizo, IShouldUpdateScreeningStatus parentProvider, List<string> exceptedStatus)
		{
			var result = false;
			var transportStatusProvider = bizo as IScreeningStatusProvider;
			if (transportStatusProvider != null && !exceptedStatus.Contains(transportStatusProvider.ScreeningStatus))
			{
				parentProvider.ShouldUpdateScreeningStatus = true;
				result = true;
			}

			return result;
		}
	}
}
