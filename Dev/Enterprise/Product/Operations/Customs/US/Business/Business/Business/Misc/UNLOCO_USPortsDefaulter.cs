using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class UNLOCO_USPortsDefaulter
	{
		public delegate List<RefLocoMap> GetRefLocoMapping();
		public delegate BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes);

		public UNLOCO_USPortsDefaulter(BusinessObjectFactory factory, ZPropertyInfo port, ZPropertyInfo unloco, GetRefLocoMapping getRefLocoMapping, GetEffectiveMappingPorts getEffectiveMappingPorts, bool isTakeMaxLength = false)
		{
			this.factory = factory;
			this.port = port;
			this.unloco = unloco;
			this.getRefLocoMapping = getRefLocoMapping;
			this.getEffectiveMappingPorts = getEffectiveMappingPorts;
			this.isTakeMaxLength = isTakeMaxLength;
		}
		readonly BusinessObjectFactory factory;
		readonly ZPropertyInfo port;
		readonly ZPropertyInfo unloco;
		readonly GetRefLocoMapping getRefLocoMapping;
		readonly GetEffectiveMappingPorts getEffectiveMappingPorts;
		readonly bool isTakeMaxLength;

		public void DefaultUNLOCO()
		{
			if (!port.Value.IsEmpty && !IsDefaultingInProgress)
			{
				using (new DefaulterLock(this))
				{
					unloco.Value = USScheduleResolver.MatchingUNLOCO((ZString)port.Value, factory);
				}
			}
		}

		public void DefaultPort()
		{
			var effectiveMappingPorts = MappingPorts;
			if (!unloco.Value.IsEmpty && effectiveMappingPorts != null && effectiveMappingPorts.Count == 1 && !IsDefaultingInProgress)
			{
				var portCode = (ZString)effectiveMappingPorts.OfType<ICodeDescription>().First().Code;
				if (portCode.Length <= port.MaxLength)
				{
					using (new DefaulterLock(this))
					{
						port.Value = portCode;
					}
				}
			}
		}

		public ZBool HasMultipleMappingPorts => !unloco.Value.IsEmpty && MappingPorts.Count > 1;

		List<RefLocoMap> RefLocoMaps
		{
			get
			{
				if (cachedRefLocoMaps == null)
				{
					cachedRefLocoMaps = new CachedProperty<List<RefLocoMap>>(factory, () =>
					{
						return getRefLocoMapping();
					});
				}

				return cachedRefLocoMaps.Value;
			}
		}
		CachedProperty<List<RefLocoMap>> cachedRefLocoMaps;

		public BusinessObjectCollection MappingPorts
		{
			get
			{
				if (cachedMappingPorts == null)
				{
					cachedMappingPorts = new CachedProperty<BusinessObjectCollection>(factory, () =>
					{
						BusinessObjectCollection result = null;
						var refLocoMaps = RefLocoMaps;
						if (refLocoMaps.Count > 0)
						{
							var refLocoMapCodes = isTakeMaxLength ? refLocoMaps.Select(x => x.RY_LocalPortCode.Left(port.MaxLength)).Distinct() : refLocoMaps.Select(x => x.RY_LocalPortCode);
							result = getEffectiveMappingPorts(refLocoMapCodes.ToList());
						}

						return result ?? new ZZRefCusCodeListCombinedCollection(factory);
					});
				}

				return cachedMappingPorts.Value;
			}
		}
		CachedProperty<BusinessObjectCollection> cachedMappingPorts;

		bool IsDefaultingInProgress
		{
			get { return defaultIndex > 0; }
		}

		class DefaulterLock : IDisposable
		{
			public DefaulterLock(UNLOCO_USPortsDefaulter defaulter)
			{
				this.defaulter = defaulter;
				defaulter.defaultIndex++;
			}

			readonly UNLOCO_USPortsDefaulter defaulter;

			void IDisposable.Dispose()
			{
				defaulter.defaultIndex--;
			}
		}
		int defaultIndex;
	}
}
