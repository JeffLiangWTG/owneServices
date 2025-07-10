namespace Enterprise.MasterFiles.Business
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.ZArchitecture.Schema;

	[DebuggerDisplay("IATACityCode = {" + nameof(Code) + "}")]
	[CodeProperty(nameof(Code)), DescriptionProperty(nameof(Description))]
	public class IATACityCode : NonPersistentBusinessObject, ILocation, IEquatable<IATACityCode>
	{
		protected IATACityCode(BusinessObjectFactory factory, ZString regionCode)
			: base(factory)
		{
			relatedPorts = GetRelatedPorts(Factory, regionCode);
			Code = regionCode.ToUpperInvariant();
		}

		readonly Dictionary<string, object> fields = new Dictionary<string, object>();
		readonly RefUNLOCO[] relatedPorts;

		public IEnumerable<RefUNLOCO> RelatedPorts
		{
			get
			{
				foreach (var port in relatedPorts)
				{
					yield return port;
				}
			}
		}

		public bool IsValid => relatedPorts.Length > 0;

		public bool HasMoreThanOnePort => relatedPorts.Length > 1;

		public static RefUNLOCO[] GetRelatedPorts(BusinessObjectFactory factory, ZString regionCode)
		{
			if (LocationHelper.GetLocationType(regionCode) != LocationHelper.LocationType.IATACityCode)
			{
				return Array.Empty<RefUNLOCO>();
			}

			var query = new ZQuery(RefUNLOCOSchema.RL_IATARegionCode, SQLComparisonOperator.Equal, regionCode);

			return factory.Load<RefUNLOCO>(query);
		}

		public static IATACityCode GetValidOrDefault(BusinessObjectFactory factory, ZString regionCode)
		{
			var code = factory.GetCachedValue(regionCode, () =>
			{
				var iataCityCode = new IATACityCode(factory, regionCode);
				return iataCityCode.IsValid ? iataCityCode : null;
			});

			return code;
		}

		#region ILocation Members

		public ZString Code { get; }

		public ZString Description => RelatedPorts.SameOrDefault(x => x.RL_PortName);

		ZBool ILocation.IsActive => IsValid ? RelatedPorts.Max(x => x.IsActive) : ZBool.False;

		public RefCityTown CityTown => (RefCityTown)fields.GetOrAdd(nameof(CityTown), () =>
			RelatedPorts.AllSame(x => x.RL_PortName)
				? this.GetCityTown(RelatedPorts.First().RL_PortName, Factory)
				: null);

		public RefCountry Country => (RefCountry)fields.GetOrAdd(nameof(Country), () =>
			RelatedPorts.AllSame(x => x.RL_RN_NKCountryCode)
				? Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, RelatedPorts.First().RL_RN_NKCountryCode)
				: null);

		public RefCountryStates State => (RefCountryStates)fields.GetOrAdd(nameof(State), () =>
			RelatedPorts.AllSame(x => x.RL_RW)
				? Factory.Load(typeof(RefCountryStates), RelatedPorts.First().RL_RW)
				: null);

		public RefUNLOCO UNLOCO => (RefUNLOCO)fields.GetOrAdd(nameof(UNLOCO), () =>
			RelatedPorts.SameOrDefault(BusinessObjectEqualityComparer<RefUNLOCO>.PKOnlyComparer));

		IATACityCode ILocation.IATACityCode => IsValid ? this : null;

		public RefZoneHeader[] Zones => (RefZoneHeader[])fields.GetOrAdd(nameof(Zones), () =>
		{
			var seedZones = RelatedPorts.Select(x => ((ILocation)x).Zones).FirstOrDefault() ?? Enumerable.Empty<RefZoneHeader>();

			return RelatedPorts.Cast<ILocation>()
				.Aggregate(seedZones, (resultZones, nextPort) =>
					resultZones.Intersect(nextPort.Zones, BusinessObjectEqualityComparer<RefZoneHeader>.PKOnlyComparer))
				.ToArray();
		});

		bool ILocationReference.IsLocalInRelationTo(ZString locationCode)
		{
			return RelatedPorts.Cast<ILocation>().Any(x => x.Country != null && x.Country.ContainsUNLOCO(locationCode));
		}

		#endregion

		#region Equality

		public bool Equals(IATACityCode otherCityCode) => string.Equals(Code, otherCityCode?.Code, StringComparison.OrdinalIgnoreCase);

		public override bool Equals(object obj) => Equals(obj as IATACityCode);

		public override int GetHashCode() => Code.GetHashCode();

		#endregion
	}
}
