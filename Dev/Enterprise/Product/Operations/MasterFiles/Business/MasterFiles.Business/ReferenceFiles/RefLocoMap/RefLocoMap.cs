using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefLocoMap : AutoRefLocoMap
	{
		public RefLocoMap(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static RefLocoMap Load(BusinessObjectFactory factory, ZString rY_RN_NKPort, ZGuid rY_RN, ZString systemUsage)
		{
			RefLocoMap result = null;

			if (!rY_RN_NKPort.IsEmpty)
			{
				ZQuery query = new ZQuery(RefLocoMapSchema.RY_RL_NKLocoPort, rY_RN_NKPort);
				query.AddToFilter(RefLocoMapSchema.RY_RN, rY_RN);

				if (!systemUsage.IsEmpty)
				{
					query.AddToFilter(RefLocoMapSchema.RY_SystemUsage, systemUsage);
				}

				result = factory.LoadTop1<RefLocoMap>(query);
			}
			return result;
		}
		bool RY_LocalPortCodeIsReadOnly => this.RY_IsSystem;
		protected bool RY_RL_NKLocoPort_ReadOnly => this.RY_IsSystem;
		bool RY_RNIsReadOnly => this.RY_IsSystem;
		bool RY_SystemUsageIsReadOnly => this.RY_IsSystem;
		[List("Lookups.CountryCollection")]
		[ReadOnlyMember(nameof(RY_RNIsReadOnly))]
		public override ZGuid RY_RN
		{
			get { return base.RY_RN; }
			set
			{
				bool hasChanged = base.RY_RN != value;
				base.RY_RN = value;
				if (hasChanged)
				{
					DefaultSystemUsage();
				}
			}
		}

		[List("Lookups.RY_SystemUsage_List")]
		[ReadOnlyMember(nameof(RY_SystemUsageIsReadOnly))]
		public override ZString RY_SystemUsage
		{
			get
			{
				return base.RY_SystemUsage;
			}
			set
			{
				base.RY_SystemUsage = value;
			}
		}

		public bool IsUSScheduleDUsage
		{
			get { return RY_RN == Core.Constants.CountryGuids.UnitedStates && IsScheduleDUsageType; }
		}

		bool IsScheduleDUsageType
		{
			get
			{
				return RY_SystemUsage == USLocoMapSystemUsageList.Codes.All ||
						 RY_SystemUsage == USLocoMapSystemUsageList.Codes.Air ||
						 RY_SystemUsage == USLocoMapSystemUsageList.Codes.Sea ||
						 RY_SystemUsage == USLocoMapSystemUsageList.Codes.SCD;
			}
		}

		public bool IsCACustomsPortUsage
		{
			get { return RY_RN == Core.Constants.CountryGuids.Canada && IsCACustomsPortType; }
		}

		bool IsCACustomsPortType
		{
			get
			{
				return RY_SystemUsage == CALocoMapSystemUsageList.Codes.All ||
				 RY_SystemUsage == CALocoMapSystemUsageList.Codes.Air ||
				 RY_SystemUsage == CALocoMapSystemUsageList.Codes.Sea ||
				 RY_SystemUsage == CALocoMapSystemUsageList.Codes.Oth;
			}
		}

		[List("Lookups.RY_LocalPortCode_List")]
		[ReadOnlyMember(nameof(RY_LocalPortCodeIsReadOnly))]
		public override ZString RY_LocalPortCode
		{
			get
			{
				return base.RY_LocalPortCode;
			}
			set
			{
				base.RY_LocalPortCode = value;
			}
		}

		public FieldType LocalPortCodeFiledType
		{
			get
			{
				if (RY_SystemUsage == LocoMapSystemUsageList.Codes.PCS)
				{
					return FieldType.TextDropEdit;
				}
				else
				{
					return FieldType.Text;
				}
			}
		}

		void DefaultSystemUsage()
		{
			if (!Lookups.RY_SystemUsage_List.ContainsCode(RY_SystemUsage))
			{
				RY_SystemUsage = "";
			}
		}

		public override RefUNLOCO LocoPort
		{
			get { return (RefUNLOCO)Factory.LoadTop1(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, RY_RL_NKLocoPort)); }
		}
	}
}
