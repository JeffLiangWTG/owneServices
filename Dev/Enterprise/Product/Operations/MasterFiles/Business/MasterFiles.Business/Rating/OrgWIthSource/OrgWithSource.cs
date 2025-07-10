using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[System.Diagnostics.DebuggerDisplay("{Org.OH_Code} ({SourceText})")]
	public class OrgWithSource : IEquatable<OrgWithSource>
	{
		public static OrgWithSource NewFrom<T>(ZPropertyInfo propertyInfo) where T : BusinessObject
		{
			if (!(typeof(T) == typeof(OrgHeader)) && !(typeof(T) == typeof(OrgAddress)))
			{
				throw new ArgumentException("Only allowed for OrgHeader or OrgAddress");
			}

			if (propertyInfo != null)
			{
				if (!(propertyInfo.Value is ZGuid))
				{
					throw new ArgumentException(string.Format("Only {0} ZPropertyInfo is allowerd here which would have a value of ZGuid type.", typeof(T)));
				}

				var value = (ZGuid)propertyInfo.Value;
				if (!value.IsValid)
				{
					return null;
				}

				var bizO = propertyInfo.BizObj.Factory.Load<T>(value);
				if (bizO != null)
				{
					var org = bizO is OrgHeader ? bizO as OrgHeader : (bizO as OrgAddress).Header;

					return New(org, new List<string>
									{
										propertyInfo.BizObj.HumanReadableName,
										propertyInfo.HumanReadableName
									});
				}

				throw new OrgWithSourceNotFoundException(propertyInfo, value);
			}

			return null;
		}

		public static OrgWithSource New(OrgHeader org, List<string> source)
		{
			if (org == null)
			{
				return null;
			}

			if (source == null || !source.Any() || source.All(x => string.IsNullOrWhiteSpace(x)))
			{
				throw new ArgumentException("Unable to create OrgWithSource because source list is null, empty or invalid.");
			}

			return new OrgWithSource(org, source);
		}

		OrgWithSource(OrgHeader org, List<String> source)
		{
			this.Org = org;
			this.Source = source;
		}

		public List<string> Source { get; }

		public OrgHeader Org { get; }

		public void PrependSource(params string[] sourceStrings)
		{
			if (Source != null)
			{
				Source.InsertRange(0, sourceStrings);
			}
		}

		public bool Equals(OrgWithSource other)
		{
			return Comparer.OrgPK.Equals(other);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as OrgWithSource);
		}

		public override int GetHashCode()
		{
			return Comparer.OrgPK.GetHashCode(this);
		}

		public string SourceText
		{
			get { return Source == null ? string.Empty : string.Join(" --> ", Source); }
		}

		public static implicit operator OrgHeader(OrgWithSource orgWithSource)
		{
			return orgWithSource?.Org;
		}

		public class Comparer : IEqualityComparer<OrgWithSource>
		{
			public static Comparer OrgPK
			{
				get { return new Comparer(); }
			}

			public bool Equals(OrgWithSource x, OrgWithSource y)
			{
				return BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer.Equals(x?.Org, y?.Org);
			}

			public int GetHashCode(OrgWithSource obj)
			{
				return BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer.GetHashCode(obj);
			}
		}
	}
}
