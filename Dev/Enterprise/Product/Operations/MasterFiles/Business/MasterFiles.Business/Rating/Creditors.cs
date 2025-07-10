using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Rating;

namespace Enterprise.MasterFiles.Business
{
	public class Creditors : IEnumerable<OrgPrioritizedList>
	{
		public void Add(string chargeCodeGroup, OrgPrioritizedList list)
		{
			this[chargeCodeGroup] = list;
		}

		public OrgPrioritizedList this[string chargeCodeGroup]
		{
			get
			{
				OrgPrioritizedList result;

				if (!inner.TryGetValue(chargeCodeGroup, out result) && (inner.Keys.Count != 1 || !inner.TryGetValue(string.Empty, out result)))
				{
					result = new OrgPrioritizedList();

					if (!SpecificBeingAddedToUnspecific(chargeCodeGroup) && !UnspecificBeingAddingToSpecific(chargeCodeGroup))
					{
						inner[chargeCodeGroup] = result;
					}
				}

				return result;
			}
			set
			{
				if (SpecificBeingAddedToUnspecific(chargeCodeGroup))
				{
					throw new ArgumentException("Transport providers already contains charge code group unspecific list. You cannot add charge code group specific list in this case.");
				}

				if (UnspecificBeingAddingToSpecific(chargeCodeGroup))
				{
					throw new ArgumentException("Transport providers already contains charge code group specific list. You cannot add charge code group unspecific list in this case.");
				}

				if (value != null)
				{
					inner[chargeCodeGroup] = value;
				}
			}
		}

		bool SpecificBeingAddedToUnspecific(string chargeCodeGroup)
		{
			return inner.ContainsKey(string.Empty) && !string.IsNullOrWhiteSpace(chargeCodeGroup);
		}

		bool UnspecificBeingAddingToSpecific(string chargeCodeGroup)
		{
			return !inner.ContainsKey(string.Empty) && string.IsNullOrWhiteSpace(chargeCodeGroup);
		}

		public IEnumerable<int> GetRank(string chargeCodeGroup, OrgWithSource org)
		{
			return this[chargeCodeGroup].GetPriorities(org);
		}

		public List<OrgHeader> AllOrgs => AllOrgsWithSource.Select(x => x.Org).ToList();
		public List<OrgWithSource> AllOrgsWithSource => inner.SelectMany(x => x.Value).Where(x => x.Org != null).Distinct(OrgWithSource.Comparer.OrgPK).ToList();
		public IEnumerable<string> ChargeCodeGroups => inner.Keys;

		public void Merge(Creditors other)
		{
			if (other != null)
			{
				foreach (var group in other.inner)
				{
					OrgPrioritizedList thisList;

					if (inner.TryGetValue(group.Key, out thisList))
					{
						thisList.Merge(group.Value);
					}
					else
					{
						inner[group.Key] = group.Value;
					}
				}
			}
		}

		readonly Dictionary<string, OrgPrioritizedList> inner = new Dictionary<string, OrgPrioritizedList>();

		#region Empty And Without Priority

		public static Creditors New(params OrgWithSource[] orgWithSources)
		{
			var list = new OrgPrioritizedList();
			list.Add(1, orgWithSources);

			var result = new Creditors();
			result.inner[""] = list;
			return result;
		}

		public static Creditors New(IEnumerable<OrgWithSource> orgs)
		{
			return New(orgs != null ? orgs.ToArray() : null);
		}

		public OrgHeader this[int index]
		{
			get { return this[string.Empty].ToList()[index]; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator<OrgPrioritizedList> IEnumerable<OrgPrioritizedList>.GetEnumerator()
		{
			return inner.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return inner.Values.GetEnumerator();
		}

		#endregion

		public string FindChargeCodeGroup(object list)
		{
			return list is OrgPrioritizedList && inner.ContainsValue((OrgPrioritizedList)list) ? inner.FirstOrDefault(x => x.Value == list).Key : string.Empty;
		}
	}

	public class OrgPrioritizedList : UniqueListWithPriority<OrgWithSource>
	{
		public OrgPrioritizedList() : base(new OrgWithSourceComparer()) { }

		public OrgPrioritizedList(params OrgWithSource[] orgsWithSource)
			: this()
		{
			Add(1, orgsWithSource);
		}

		public OrgPrioritizedList(IEnumerable<OrgWithSource> orgsWithSource) : this(orgsWithSource.ToArray()) { }
	}
}
