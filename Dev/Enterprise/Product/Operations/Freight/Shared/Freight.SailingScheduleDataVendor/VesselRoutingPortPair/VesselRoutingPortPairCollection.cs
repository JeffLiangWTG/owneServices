using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class VesselRoutingPortPairCollection : BusinessObjectCollection<VesselRoutingPortPair>
	{
		public VesselRoutingPortPairCollection(VesselRoutingVoyage voyage)
			: base(voyage.Factory)
		{
			this.voyage = voyage;
		}

		public void SelectPortPairsMatchingPort(ZString portCode)
		{
			foreach (VesselRoutingPortPair portPair in this)
			{
				if (portPair.E9_RL_NKLoadPort == portCode ||
					portPair.E9_RL_NKDischargePort == portCode)
				{
					portPair.E9_IsSelected = true;
				}
			}
		}

		#region Loading Port Pairs

		internal VesselRoutingVoyage Voyage
		{
			get { return voyage; }
		}

		internal List<string> ForeignPortsOrEmptyPort
		{
			get
			{
				List<string> result = new List<string>(voyage.ForeignPorts);
				if (result.Count == 0)
				{
					result.Add("");
				}

				return result;
			}
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();

			Sort(new DefaultComparer());

			RemoveDuplicatePortPairs();

			CreatePortPairsArrivingFromOverseas();
			CreatePortPairsDepartingToOverseas();

			foreach (VesselRoutingPortPair portPair in new ArrayList(this))
			{
				if (!ShouldIncludeInCollection(portPair))
				{
					Remove(portPair);
				}
			}

			if (voyage != null)
			{
				voyage.E8_IsSelectedInfo.RefreshBinding();
			}
		}

		bool ShouldIncludeInCollection(VesselRoutingPortPair portPair)
		{
			bool result = Voyage.IsPortPairTypeInFilter(portPair.PortPairType);

			if (result)
			{
				if (portPair.E9_DataProvider == FreightConstants.VesselDataProviders.DAKOSY)
				{
					return !portPair.E9_RL_NKLoadPort.IsEmpty && !portPair.E9_RL_NKDischargePort.IsEmpty;
				}
				else
				{
					if (portPair.PortPairType == PortPairTypes.Import && !ForeignPortsOrEmptyPort.Contains(portPair.E9_RL_NKLoadPort))
					{
						result = false;
					}
					else if (portPair.PortPairType == PortPairTypes.Export && !ForeignPortsOrEmptyPort.Contains(portPair.E9_RL_NKDischargePort))
					{
						result = false;
					}
				}
			}

			return result;
		}

		void CreatePortPairsArrivingFromOverseas()
		{
			foreach (VesselRoutingPortPair port in ToArray())
			{
				foreach (string foreignPort in ForeignPortsOrEmptyPort)
				{
					if (foreignPort.Length != 0
						&& port.E9_RL_NKDischargePort.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
						&& !ContainsPortPair(foreignPort, port.E9_RL_NKDischargePort))
					{
						VesselRoutingPortPair newPortPair = port.Clone();
						newPortPair.E9_RL_NKLoadPort = foreignPort;
						newPortPair.E9_RL_NKDischargePort = port.E9_RL_NKDischargePort;
						newPortPair.E9_ETD = ZDateTime.Empty;
						newPortPair.E9_ATD = ZDateTime.Empty;
						newPortPair.E9_CargoCutOff = ZDateTime.Empty;
						newPortPair.E9_ExportReceivalCommences = ZDateTime.Empty;
						Add(newPortPair);
					}
				}
			}
		}

		void CreatePortPairsDepartingToOverseas()
		{
			foreach (VesselRoutingPortPair port in ToArray())
			{
				foreach (string foreignPort in ForeignPortsOrEmptyPort)
				{
					if (foreignPort.Length != 0
						&& port.E9_RL_NKLoadPort.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
						&& !ContainsPortPair(port.E9_RL_NKLoadPort, foreignPort))
					{
						VesselRoutingPortPair newPortPair = port.Clone();
						newPortPair.E9_RL_NKLoadPort = port.E9_RL_NKLoadPort;
						newPortPair.E9_RL_NKDischargePort = foreignPort;
						newPortPair.E9_ETA = ZDateTime.Empty;
						newPortPair.E9_ATA = ZDateTime.Empty;
						newPortPair.E9_ImportAvailability = ZDateTime.Empty;
						newPortPair.E9_ImportStorageCommences = ZDateTime.Empty;
						Add(newPortPair);
					}
				}
			}
		}

		void RemoveDuplicatePortPairs()
		{
			foreach (VesselRoutingPortPair portPair1 in new ArrayList(this))
			{
				foreach (VesselRoutingPortPair portPair2 in new ArrayList(this))
				{
					if (portPair1.E9_RL_NKLoadPort == portPair2.E9_RL_NKLoadPort &&
						portPair1.E9_RL_NKDischargePort == portPair2.E9_RL_NKDischargePort &&
						portPair1.PK != portPair2.PK)
					{
						if (Contains(portPair1))
						{
							Remove(portPair1);
						}
					}
				}
			}
		}

		internal bool ContainsPortPair(ZString loadPort, ZString dischargePort)
		{
			foreach (VesselRoutingPortPair portPair in this)
			{
				if (portPair.E9_RL_NKLoadPort == loadPort &&
					portPair.E9_RL_NKDischargePort == dischargePort)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region CreateRelationshipFilter

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = new ZQuery(ViewVesselRoutingPortPairsSchema.E9_LloydsNumber, voyage.E8_LloydsNumber);
			result.AddToFilter(ViewVesselRoutingPortPairsSchema.E9_Voyage, voyage.E8_Voyage);
			result.AddToFilter(ViewVesselRoutingPortPairsSchema.E9_LineOperator, voyage.E8_LineOperator);
			result.AddToFilter(ViewVesselRoutingPortPairsSchema.E9_DataProvider, voyage.E8_DataProvider);

			return result;
		}

		#endregion

		#region AllowNew / AllowSort

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		#endregion

		#region Default Comparer

		class DefaultComparer : IComparer<VesselRoutingPortPair>
		{
			public int Compare(VesselRoutingPortPair lhs, VesselRoutingPortPair rhs)
			{
				int result;
				if (lhs.PortPairType == rhs.PortPairType)
				{
					result = 0;
				}
				else if ( // use the following order: Import, Domestic, Export
					lhs.PortPairType == PortPairTypes.Import && rhs.PortPairType == PortPairTypes.Domestic || // Import < Domestic
					lhs.PortPairType == PortPairTypes.Import && rhs.PortPairType == PortPairTypes.Export ||   // Import < Export
					lhs.PortPairType == PortPairTypes.Domestic && rhs.PortPairType == PortPairTypes.Export)   // Domestic < Export
				{
					result = -1;
				}
				else
				{
					result = 1;
				}
				if (result == 0)
				{
					result = lhs.E9_ETD.CompareTo(rhs.E9_ETD);
				}

				if (result == 0)
				{
					result = lhs.E9_ETA.CompareTo(rhs.E9_ETA);
				}

				if (result == 0)
				{
					result = lhs.E9_RL_NKLoadPort.CompareTo(rhs.E9_RL_NKLoadPort);
				}

				if (result == 0)
				{
					result = lhs.E9_RL_NKDischargePort.CompareTo(rhs.E9_RL_NKDischargePort);
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		readonly VesselRoutingVoyage voyage;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			// just for test
#if DEBUG
			VesselRoutingPortPair portPair = (VesselRoutingPortPair)child;
			portPair.E9_Voyage = voyage.E8_Voyage;
			portPair.E9_LloydsNumber = voyage.E8_LloydsNumber;
#endif
		}

		#endregion
	}
}
