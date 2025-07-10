using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.SailingDataVendor.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	[ProvideMetaDataProperty("PropertyReadOnly", MetaDataTypes.ReadOnly)]
	public class VesselRoutingVoyage : AutoViewVesselRoutingVoyages, IObsoleteValidation
	{
		public VesselRoutingVoyage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected bool GetPropertyReadOnly(PropertyDescriptor property)
		{
			bool result =
				property.Name != E8_ForeignPortToAddInfo.Name &&
				property.Name != E8_OH_LineOperatorInfo.Name;
			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		public void DeselectAllPortPairs()
		{
			if (portPairs != null)
			{
				foreach (VesselRoutingPortPair portPair in PortPairs)
				{
					portPair.E9_IsSelected = false;
				}
			}
		}

		#region Business Object Overrides

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		protected override void RunPreSaveValidationCore()
		{
			// only validate items that will be imported, for performance
			if (E8_IsSelected)
			{
				Validation.ValidateE8_OH_LineOperator();
				PortPairs.RunPreSaveValidation();
			}
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region User Entered Data Storage

		internal T GetUserEnteredValue<T>(object key, T defaultValue)
		{
			object result = defaultValue;

			UserEnteredValueKey userEnteredValueKey = new UserEnteredValueKey(this, key);
			if (UserEnteredData.Contains(userEnteredValueKey))
			{
				result = UserEnteredData[userEnteredValueKey];
			}

			return (T)result;
		}

		internal void SetUserEnteredValue<T>(object key, T value)
		{
			UserEnteredData[new UserEnteredValueKey(this, key)] = value;
		}

		[BusinessObjectTestExclude]
		public VesselRoutingVoyageCollection ParentCollection { get; set; }

		IDictionary UserEnteredData
		{
			get
			{
				if (ParentCollection != null)
				{
					return ParentCollection.userEnteredData;
				}
				ErrorReporter.ReportOnce("VesselRoutingVoyage.UserEnteredValue", "Make sure this business object is a part of a " + nameof(VesselRoutingVoyageCollection) + ". User Entered Data cannot be set at this time.");
				return new Hashtable();
			}
		}

		class UserEnteredValueKey
		{
			public UserEnteredValueKey(VesselRoutingVoyage voyage, object key)
			{
				this.voyage = voyage.E8_Voyage;
				this.lloydsNumber = voyage.E8_LloydsNumber;
				this.firstArrival = voyage.E8_FirstArrival;
				this.lastDeparture = voyage.E8_LastDeparture;
				this.key = key;
			}

			public override bool Equals(object obj)
			{
				UserEnteredValueKey rhs = obj as UserEnteredValueKey;
				return
					rhs != null &&
					voyage == rhs.voyage &&
					lloydsNumber == rhs.lloydsNumber &&
					firstArrival == rhs.firstArrival &&
					lastDeparture == rhs.lastDeparture &&
					object.Equals(key, rhs.key);
			}

			public override int GetHashCode()
			{
				return
					voyage.GetHashCode() ^
					lloydsNumber.GetHashCode() ^
					firstArrival.GetHashCode() ^
					lastDeparture.GetHashCode() ^
					key.GetHashCode();
			}

			readonly ZString voyage;
			readonly ZString lloydsNumber;
			readonly ZDateTime firstArrival;
			readonly ZDateTime lastDeparture;
			readonly object key;
		}

		#endregion

		#region Foreign Ports

		public List<string> ForeignPorts
		{
			get
			{
				List<string> result = GetUserEnteredValue<List<string>>("ForeignPorts", null);
				if (result == null)
				{
					result = new List<string>();
					SetUserEnteredValue("ForeignPorts", result);
					LoadOriginalForeignPortList();
				}

				return result;
			}
		}

		public void LoadOriginalForeignPortList()
		{
			ForeignPorts.Clear();

			if (E8_DataProvider == FreightConstants.VesselDataProviders.DAKOSY)
			{
				var query = new ZQuery();
				query.AddToFilter(JobVesselScheduleSchema.EV_IMOLloydsNumber, E8_LloydsNumber);
				query.AddToFilter(JobVesselScheduleSchema.EV_ShipOperatorVoyageIn, E8_Voyage);
				query.AddToFilter(JobVesselScheduleSchema.EV_LineOperator, E8_LineOperator);
				query.AddToFilter(JobVesselScheduleSchema.EV_DataProvider, E8_DataProvider);
				query.OrderBy = JobVesselScheduleSchema.EV_RL_NKPortCode.Name;

				var schedules = Factory.Load<JobVesselSchedule>(query);
				foreach (var schedule in schedules)
				{
					if (!schedule.EV_RL_NKPortCode.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
						&& !ForeignPorts.Contains(schedule.EV_RL_NKPortCode))
					{
						ForeignPorts.Add(schedule.EV_RL_NKPortCode);
					}
				}
			}
			else
			{
				var query = new ZQuery();
				query.AddToFilter(JobVesselRoutingSchema.E1_LloydsID, E8_LloydsNumber);
				query.AddToFilter(JobVesselRoutingSchema.E1_VoyageNumber, E8_Voyage);
				//Only simple way to make sure this routing is from 1-Stop
				query.AddToFilter(JobVesselRoutingSchema.E1_EV, null);
				query.AddToFilter(JobVesselRoutingSchema.E1_DataProviderReference, string.Empty);

				query.OrderBy = JobVesselRoutingSchema.E1_DischargePortName.Name;

				var oneStopForeignPorts = Factory.Load<JobVesselRouting>(query);
				foreach (var port in oneStopForeignPorts)
				{
					if (!port.E1_RL_NKDischargePortCode.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
						&& !ForeignPorts.Contains(port.E1_RL_NKDischargePortCode))
					{
						ForeignPorts.Add(port.E1_RL_NKDischargePortCode);
					}
				}
			}
		}

		#endregion

		#region Related Business Objects

		public VesselRoutingPortPairCollection PortPairs
		{
			get
			{
				if (portPairs == null)
				{
					portPairs = new VesselRoutingPortPairCollection(this);
					portPairs.Load();
				}

				return portPairs;
			}
		}
		VesselRoutingPortPairCollection portPairs;

		public RefVessel Vessel
		{
			get { return GetVessel(Factory); }
		}

		public RefVessel GetVessel(BusinessObjectFactory factory)
		{
			RefVessel result = null;
			if (!E8_LloydsNumber.IsEmpty)
			{
				RefVessel[] matches = factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, E8_LloydsNumber));

				if (matches.Length == 1)
				{
					result = matches[0];
				}
				else if (matches.Length > 1)
				{
					result = FindVesselByVesselName(matches);
				}
			}
			else if (!E8_VesselName.IsEmpty)
			{
				result = RefVessel.LookupVesselByFK(this, ViewVesselRoutingVoyagesSchema.E8_VesselName, factory);
			}

			return result;
		}

		RefVessel FindVesselByVesselName(RefVessel[] vessels)
		{
			foreach (RefVessel vessel in vessels)
			{
				if (vessel.RV_FK == E8_VesselName)
				{
					return vessel;
				}
			}

			return null;
		}

		#endregion

		#region New Properties

		#region E8_IsSelected

		public ZBool E8_IsSelected
		{
			get
			{
				if (portPairs != null)
				{
					foreach (VesselRoutingPortPair portPair in PortPairs)
					{
						if (portPair.E9_IsSelected)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		public ZPropertyInfo E8_IsSelectedInfo
		{
			get { return GetZPropertyInfo(nameof(E8_IsSelected)); }
		}

		#endregion

		#region E8_HasNoPortPairs

		public ZBool E8_HasNoPortPairs
		{
			get { return PortPairs.Count == 0; }
		}

		public ZPropertyInfo E8_HasNoPortPairsInfo
		{
			get { return GetZPropertyInfo(nameof(E8_HasNoPortPairs)); }
		}

		#endregion

		#region Just for debug/testing phase

		public ZString E8_EV_PK_String
		{
			get
			{
				return E8_EV_PK.ToString();
			}
		}

		public ZString E8_EV_ProviderReference
		{
			get
			{
				if (E8_EV_PK != ZGuid.Empty)
				{
					return Factory.Load<JobVesselSchedule>(E8_EV_PK).EV_DataProviderReference;
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region LineOperator

		public OrgHeader LineOperator
		{
			get { return Factory.Load<OrgHeader>(E8_OH_LineOperator); }
		}

		[List("E8_OH_LineOperator_List")]
		public ZGuid E8_OH_LineOperator
		{
			get
			{
				if (e8_OH_LineOperator.IsEmpty && !defaultedCarrierFromExternalCode)
				{
					defaultedCarrierFromExternalCode = DefaultLineOperatorOrgFromExternalCode();
				}

				return e8_OH_LineOperator;
			}
			set
			{
				if (e8_OH_LineOperator != value)
				{
					e8_OH_LineOperator = value;
					E8_OH_LineOperatorInfo.RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateE8_OH_LineOperator();
				}
			}
		}
		ZGuid e8_OH_LineOperator;
		bool defaultedCarrierFromExternalCode;

		public ZPropertyInfo E8_OH_LineOperatorInfo
		{
			get { return GetZPropertyInfo(nameof(E8_OH_LineOperator)); }
		}

		#endregion

		#region E8_ForeignPortToAdd

		[List("Ports")]
		[MaxLength(5)]
		public ZString E8_ForeignPortToAdd
		{
			get { return fE8_ForeignPortToAdd; }
			set
			{
				CheckMaximumLength(E8_ForeignPortToAddInfo, value);
				fE8_ForeignPortToAdd = value;
				E8_ForeignPortToAddInfo.RefreshBinding();

				Validation.ValidateE8_ForeignPortToAdd();
			}
		}
		ZString fE8_ForeignPortToAdd;

		public ZPropertyInfo E8_ForeignPortToAddInfo
		{
			get { return GetZPropertyInfo(nameof(E8_ForeignPortToAdd)); }
		}

		#endregion

		#endregion

		#region PortPairTypeFilter

		public PortPairTypes PortPairTypeFilter
		{
			get
			{
				if (ParentCollection != null)
				{
					return ParentCollection.PortPairTypeFilter;
				}
				ErrorReporter.ReportOnce("VesselRoutingVoyage.PortPairTypeFilter", "Make sure this business object is a part of a " + nameof(VesselRoutingVoyageCollection) + ". PortPairTypeFilter is not available at this time.");
				return PortPairTypes.None;
			}
		}

		public bool IsPortPairTypeInFilter(PortPairTypes portPairType)
		{
			return true;
		}

		#endregion

		#region Lookups

		#region E8_OH_LineOperator_List

		public SeaShippingProviderCollection E8_OH_LineOperator_List
		{
			get
			{
				if (fE8_OH_LineOperator_List == null)
				{
					fE8_OH_LineOperator_List = new SeaShippingProviderCollection(Factory);
					fE8_OH_LineOperator_List.Load();
				}

				return fE8_OH_LineOperator_List;
			}
		}
		SeaShippingProviderCollection fE8_OH_LineOperator_List;

		#endregion

		public RefUNLOCOCollection Ports
		{
			get
			{
				if (ports == null)
				{
					ports = new RefUNLOCOCollection(Factory);
				}

				return ports;
			}
		}
		RefUNLOCOCollection ports;

		#endregion

		#region Implementation

		bool DefaultLineOperatorOrgFromExternalCode()
		{
			e8_OH_LineOperator = SailingScheduleHelper.GetLineOperatorPKFromExternalCode(E8_LineOperator, E8_DataProvider, Factory);

			return !e8_OH_LineOperator.IsEmpty;
		}

		public bool LineOperatorOrgMatchesExternalCode()
		{
			return (SailingScheduleHelper.GetLineOperatorPKFromExternalCode(E8_LineOperator, E8_DataProvider, Factory) == E8_OH_LineOperator);
		}

		#endregion
	}
}
