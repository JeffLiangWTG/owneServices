using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefLocoMapCollection : ActiveBusinessObjectCollection<RefLocoMap>
	{
		public RefLocoMapCollection(BusinessObject parent) : base(parent.Factory)
		{
			this.Parent = parent as RefUNLOCO;
		}

		public RefLocoMapCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefLocoMapCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public RefLocoMapCollection(BusinessObject parent, ZQuery relationshipFilter)
			: base(parent.Factory, relationshipFilter)
		{
			this.Parent = parent as RefUNLOCO;
		}

		public ZString LocalCodeForUsage(ZString systemUsage)
		{
			ZString result = "";
			foreach (RefLocoMap locoMap in this)
			{
				if (locoMap.RY_SystemUsage == systemUsage)
				{
					result = locoMap.RY_LocalPortCode;
					break;
				}
			}
			return result;
		}

		public ZString LocalCodeForUsageAndCountry(ZString systemUsage, ZString countryCode)
		{
			return LocalCodeForUsageAndCountry(systemUsage, RefCountry.LoadFromCountryCode(Factory, countryCode));
		}

		public ZString LocalCodeForUsageAndCountry(ZString systemUsage, RefCountry country)
		{
			ZString result = "";
			if (country != null)
			{
				foreach (RefLocoMap locoMap in this)
				{
					if (locoMap.RY_SystemUsage == systemUsage && locoMap.RY_RN == country.PK)
					{
						result = locoMap.RY_LocalPortCode;
						break;
					}
				}
			}
			return result;
		}

		#region Implementation

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		protected RefUNLOCO Parent;

		protected override void SetDefaultsForNewElementCore(RefLocoMap newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			RefLocoMap locoMap = newElement;
			if (locoMap != null && Parent != null)
			{
				locoMap.RY_RL_NKLocoPort = Parent.RL_Code;
				locoMap.RY_RN = Parent.Country != null ? Parent.Country.PK : ZGuid.Empty;
			}
		}

		#endregion
	}
}
