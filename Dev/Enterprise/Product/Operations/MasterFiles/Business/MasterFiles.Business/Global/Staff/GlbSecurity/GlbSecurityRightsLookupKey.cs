using CargoWise.Types;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbSecurityRightsLookupKey
	{
		public static GlbSecurityRightsLookupKey ForLookup(SecurityCheckpoint checkPoint, ZGuid ownerPk, ZGuid departmentPk, ZGuid branchPk, ZGuid companyPk)
		{
			return new GlbSecurityRightsLookupKeyForLookup(checkPoint, ownerPk, departmentPk, branchPk, companyPk);
		}

		public abstract ZGuid OwnerPk { get; }
		public abstract ZString GU_SecurityRight { get; }
		public abstract ZGuid GU_ItemGUID { get; }
		public abstract ZGuid GU_GE { get; }
		public abstract ZGuid GU_GB { get; }
		public abstract ZGuid GU_GC { get; }

		public override bool Equals(object obj)
		{
			GlbSecurityRightsLookupKey key = obj as GlbSecurityRightsLookupKey;
			return key != null &&
				   OwnerPk == key.OwnerPk &&
				   GU_SecurityRight == key.GU_SecurityRight &&
				   GU_ItemGUID == key.GU_ItemGUID &&
				   GU_GE == key.GU_GE &&
				   GU_GB == key.GU_GB &&
				   GU_GC == key.GU_GC;
		}

		public override int GetHashCode()
		{
			if (hashCode == -1)
			{
				hashCode = OwnerPk.GetHashCode() ^
					   GU_SecurityRight.GetHashCode() ^
					   GU_ItemGUID.GetHashCode() ^
					   GU_GE.GetHashCode() ^
					   GU_GB.GetHashCode() ^
					   GU_GC.GetHashCode();
			}
			return hashCode;
		}

		int hashCode = -1;
	}

	class GlbSecurityRightsLookupKeyForGlbSecurityItem : GlbSecurityRightsLookupKey
	{
		public GlbSecurityRightsLookupKeyForGlbSecurityItem(GlbSecurity item, bool isGroupLookup, bool ignoreCBD = false)
		{
			this.item = item;
			this.isGroupLookup = isGroupLookup;
			this.ignoreCBD = ignoreCBD;
		}

		readonly GlbSecurity item;
		readonly bool isGroupLookup;
		readonly bool ignoreCBD;

		public override ZGuid OwnerPk
		{
			get { return isGroupLookup ? item.GU_GG : item.GU_GS; }
		}

		public override ZString GU_SecurityRight
		{
			get { return item.GU_SecurityRight; }
		}

		public override ZGuid GU_ItemGUID
		{
			get { return item.GU_ItemGUID; }
		}

		public override ZGuid GU_GE
		{
			get { return ignoreCBD ? ZGuid.Empty : item.GU_GE; }
		}

		public override ZGuid GU_GB
		{
			get { return ignoreCBD ? ZGuid.Empty : item.GU_GB; }
		}

		public override ZGuid GU_GC
		{
			get { return ignoreCBD ? ZGuid.Empty : item.GU_GC; }
		}
	}

	public class GlbSecurityRightsLookupKeyForLookup : GlbSecurityRightsLookupKey
	{
		public GlbSecurityRightsLookupKeyForLookup(SecurityCheckpoint checkPoint, ZGuid ownerPk, ZGuid departmentPk, ZGuid branchPk, ZGuid companyPk)
		{
			this.checkPoint = checkPoint;
			this.ownerPk = ownerPk;
			this.departmentPk = departmentPk;
			this.branchPk = branchPk;
			this.companyPk = companyPk;
		}

		readonly SecurityCheckpoint checkPoint;
		readonly ZGuid ownerPk;
		readonly ZGuid departmentPk;
		readonly ZGuid branchPk;
		readonly ZGuid companyPk;

		public override ZGuid OwnerPk
		{
			get { return ownerPk; }
		}

		public override ZString GU_SecurityRight
		{
			get { return checkPoint.Code; }
		}

		public override ZGuid GU_ItemGUID
		{
			get { return checkPoint.ItemGuid; }
		}

		public override ZGuid GU_GE
		{
			get { return departmentPk; }
		}

		public override ZGuid GU_GB
		{
			get { return branchPk; }
		}

		public override ZGuid GU_GC
		{
			get { return companyPk; }
		}
	}
}
