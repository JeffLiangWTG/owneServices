using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbSecurityCollection : BusinessObjectCollection<GlbSecurity>, IZGlbSecurityCollection, IReadOnlyFactoryForSecurityValidationProvider, IGlbSecurityCollectionWithSecurity
	{
		readonly GlbGroup group;
		BusinessObjectFactory readOnlyFactory;
		SecurityCore security;
		readonly GlbStaff staff;

		public GlbSecurityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbSecurityCollection(GlbGroup group, BusinessObjectFactory factory)
			: base(factory)
		{
			this.group = group;
		}

		public GlbSecurityCollection(GlbStaff staff, BusinessObjectFactory factory)
			: base(factory)
		{
			this.staff = staff;
		}

		public BusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (readOnlyFactory == null)
				{
					readOnlyFactory = new BusinessObjectFactory();
				}
				return readOnlyFactory;
			}
		}

		public SecurityCore Security
		{
			get { return security; }
			set { security = value; }
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			if (bizO.IsInDatabase)
			{
				((IBusinessObjectCollectionInternals)this).HasChangesFromDelete = true;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			GlbSecurity security = (GlbSecurity)child;
			if (group != null)
			{
				security.GU_GG = group.PK;
			}
			else if (staff != null)
			{
				security.GU_GS = staff.PK;
			}
		}

		public void ResetToGroupsDefaults()
		{
			int length = Elements.ToArray().Length;

			for (int i = length - 1; i >= 0; i--)
			{
				GlbSecurity security = Elements[i] as GlbSecurity;

				if (security != null && staff != null && security.GU_GS == staff.PK)
				{
					RemoveAndDelete(security);
				}
			}
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var zQuery = base.CreateAdditionalFilter();
			if (staff != null)
			{
				zQuery.AddToFilter(GlbSecuritySchema.GU_GS, staff.PK);
				zQuery.AddToFilter(new ZQuery(GlbSecuritySchema.GU_GG, staff.Groups.Select(x => x.PK)), JoinCondition.Or);
			}
			if (group != null)
			{
				zQuery.AddToFilter(GlbSecuritySchema.GU_GG, group.PK);
			}
			return zQuery;
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			lookupKeyTable = null;
			lookupKeyTableIgnoreCBD = null;
		}

		public GlbSecurity[] Find(GlbSecurityRightsLookupKey key, bool ignoreCBD = false)
		{
			if (!ignoreCBD && lookupKeyTable == null)
			{
				lookupKeyTable = new Dictionary<GlbSecurityRightsLookupKey, List<GlbSecurity>>(this.Count);
				foreach (GlbSecurity item in this)
				{
					if (!item.GU_GS.IsEmpty)
					{
						var itemKey = new GlbSecurityRightsLookupKeyForGlbSecurityItem(item, false);
						List<GlbSecurity> itemList;
						if (!lookupKeyTable.TryGetValue(itemKey, out itemList))
						{
							itemList = new List<GlbSecurity>();
							lookupKeyTable.Add(itemKey, itemList);
						}
						itemList.Add(item);
					}
					if (!item.GU_GG.IsEmpty)
					{
						var itemKey = new GlbSecurityRightsLookupKeyForGlbSecurityItem(item, true);
						List<GlbSecurity> itemList;
						if (!lookupKeyTable.TryGetValue(itemKey, out itemList))
						{
							itemList = new List<GlbSecurity>();
							lookupKeyTable.Add(itemKey, itemList);
						}
						itemList.Add(item);
					}
				}
			}

			if (ignoreCBD && lookupKeyTableIgnoreCBD == null)
			{
				lookupKeyTableIgnoreCBD = new Dictionary<GlbSecurityRightsLookupKey, List<GlbSecurity>>(this.Count);
				foreach (GlbSecurity item in this)
				{
					if (!item.GU_GS.IsEmpty)
					{
						var itemKey = new GlbSecurityRightsLookupKeyForGlbSecurityItem(item, false, true);
						List<GlbSecurity> itemList;
						if (!lookupKeyTableIgnoreCBD.TryGetValue(itemKey, out itemList))
						{
							itemList = new List<GlbSecurity>();
							lookupKeyTableIgnoreCBD.Add(itemKey, itemList);
						}
						itemList.Add(item);
					}
					if (!item.GU_GG.IsEmpty)
					{
						var itemKey = new GlbSecurityRightsLookupKeyForGlbSecurityItem(item, true, true);
						List<GlbSecurity> itemList;
						if (!lookupKeyTableIgnoreCBD.TryGetValue(itemKey, out itemList))
						{
							itemList = new List<GlbSecurity>();
							lookupKeyTableIgnoreCBD.Add(itemKey, itemList);
						}
						itemList.Add(item);
					}
				}
			}

			GlbSecurity[] matches;
			List<GlbSecurity> matchList;
			var table = ignoreCBD ? lookupKeyTableIgnoreCBD : lookupKeyTable;
			if (table.TryGetValue(key, out matchList))
			{
				matches = matchList.ToArray();
			}
			else
			{
				matches = System.Array.Empty<GlbSecurity>();
			}
			return matches;
		}

		Dictionary<GlbSecurityRightsLookupKey, List<GlbSecurity>> lookupKeyTable;
		Dictionary<GlbSecurityRightsLookupKey, List<GlbSecurity>> lookupKeyTableIgnoreCBD;

		#region IGlbSecurityCollectionWithSecurity Members

		ISecurityMap IGlbSecurityCollectionWithSecurity.SecurityMap
		{
			get
			{
				if (securityMap == null)
				{
					securityMap = ObjectFactory.Get<ISecurityMap>();
				}

				return securityMap;
			}
		}
		ISecurityMap securityMap;

		#endregion
	}
}
