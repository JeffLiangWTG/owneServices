using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// This utility class provides conversion from local / customer specific port code to UNLOCO.
	/// It refers to RefLocoMap table.
	/// </summary>
	public class RefLocoConverter
	{
		public RefLocoConverter(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public RefUNLOCO GetUNLOCO(ZString systemUsage, ZString localPortCode, ZGuid country)
		{
			ZQuery locoFilter = new ZQuery();
			locoFilter.AddToFilter(ZArchitecture.Schema.RefLocoMapSchema.RY_SystemUsage, systemUsage);
			locoFilter.AddToFilter(ZArchitecture.Schema.RefLocoMapSchema.RY_LocalPortCode, localPortCode);
			locoFilter.AddToFilter(ZArchitecture.Schema.RefLocoMapSchema.RY_RN, country);
			RefLocoMap[] locoMaps = (RefLocoMap[])Factory.Load(typeof(RefLocoMap), locoFilter);
			if (locoMaps == null || locoMaps.Length == 0)
			{
				return null;
			}
			else
			{
				return locoMaps[0].LocoPort;
			}
		}

		public RefUNLOCO GetUNLOCO(ZGuid pK)
		{
			RefLocoMap locoMap = (RefLocoMap)Factory.Load(typeof(RefLocoMap), pK);
			if (locoMap == null)
			{
				return null;
			}
			else
			{
				return locoMap.LocoPort;
			}
		}

		public ZString GetUNLOCOString(ZString systemUsage, ZString localPortCode, ZGuid country)
		{
			RefUNLOCO locoPort = GetUNLOCO(systemUsage, localPortCode, country);
			if (locoPort == null)
			{
				return ZString.Empty;
			}
			else
			{
				return locoPort.Code;
			}
		}

		public ZString GetUNLOCOString(ZGuid pK)
		{
			RefUNLOCO locoPort = GetUNLOCO(pK);
			if (locoPort == null)
			{
				return ZString.Empty;
			}
			else
			{
				return locoPort.Code;
			}
		}

		#region Implementation

		protected internal BusinessObjectFactory Factory;

		#endregion
	}
}
