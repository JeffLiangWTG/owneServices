using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbSecurityChangeOthersView : BusinessObjectCollectionView<GlbSecurity>, IReadOnlyFactoryForSecurityValidationProvider
	{
		public enum ChangeOthersMode
		{
			LocalAdministrator = 0,
			GroupOwner = 1,
			GroupOwnersForGroup = 2,
		}

		BusinessObjectFactory readOnlyFactory;
		readonly ISupportChangeOthersSecurity staffOrGroupWhichCanChangeOthers;
		ChangeOthersMode mode;
		public ChangeOthersMode Mode
		{
			get
			{
				return mode;
			}
			set
			{
				if (this.mode != value)
				{
					this.mode = value;
					Rebuild();
				}
			}
		}

		public GlbSecurityChangeOthersView(GlbSecurityCollection collectionToFilter, ISupportChangeOthersSecurity staffOrGroupWhichCanChangeOthers, ChangeOthersMode mode)
			: base(collectionToFilter)
		{
			this.staffOrGroupWhichCanChangeOthers = staffOrGroupWhichCanChangeOthers;
			this.mode = mode;
			Rebuild();
		}

		protected override void RebuildOnConstruction()
		{
			// Don't do anything here. We really shouldn't call virtual methods in constructors!
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
		void AddChangeOtherIfNotAlreadyPresent(BusinessObject staffOrGroupWhoseRightsCanBeChanged, string securityRight)
		{
			SchemaGuidColumn columnToUse = null;
			if (staffOrGroupWhoseRightsCanBeChanged != null)
			{
				ZQuery existingSecurityQuery = new ZQuery(GlbSecuritySchema.GU_ItemGUID, staffOrGroupWhoseRightsCanBeChanged.PK);
				if (mode == ChangeOthersMode.GroupOwnersForGroup)
				{
					columnToUse = ((ISupportChangeOthersSecurity)staffOrGroupWhoseRightsCanBeChanged).GlbSecurityRightHolderColumn;
					existingSecurityQuery = new ZQuery(columnToUse, staffOrGroupWhoseRightsCanBeChanged.PK);
					existingSecurityQuery.AddToFilter(GlbSecuritySchema.GU_ItemGUID, staffOrGroupWhichCanChangeOthers.PK);
				}

				existingSecurityQuery.AddToFilter(GlbSecuritySchema.GU_SecurityRight, securityRight);
				if (Find(existingSecurityQuery).Length == 0)
				{
					GlbSecurity newSecurity = AddNew();
					newSecurity.GU_SecurityItemIsAllowed = true;
					newSecurity.GU_SecurityRight = securityRight;
					if (mode == ChangeOthersMode.GroupOwnersForGroup)
					{
						newSecurity.GU_GG = ZGuid.Empty;
						newSecurity.GU_ItemGUID = staffOrGroupWhichCanChangeOthers.PK;
						newSecurity[columnToUse] = staffOrGroupWhoseRightsCanBeChanged.PK;
					}
					else
					{
						newSecurity.GU_ItemGUID = staffOrGroupWhoseRightsCanBeChanged.PK;
						newSecurity[staffOrGroupWhichCanChangeOthers.GlbSecurityRightHolderColumn] = staffOrGroupWhichCanChangeOthers.PK;
					}
				}
			}
		}

		public void AddSecurityToChangeOtherGroup(string code)
		{
			GlbGroup group = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, code);
			AddChangeOtherIfNotAlreadyPresent(group, mode == ChangeOthersMode.LocalAdministrator ? GlbSecurity.ChangeOtherGroupSecurityRightName : GlbSecurity.GroupOwnerSecurityRightName);
		}

		public void AddSecurityToChangeOtherStaff(string code)
		{
			if (mode == ChangeOthersMode.GroupOwner)
			{ throw new InvalidOperationException("You can't be a Group Owner of a staff."); }
			GlbStaff staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, code);
			AddChangeOtherIfNotAlreadyPresent(staff, mode == ChangeOthersMode.LocalAdministrator ? GlbSecurity.ChangeOtherStaffSecurityRightName : GlbSecurity.GroupOwnerSecurityRightName);
		}

		bool securityRightMatches(string securityRight)
		{
			if (mode == ChangeOthersMode.GroupOwner || mode == ChangeOthersMode.GroupOwnersForGroup)
			{ return securityRight == GlbSecurity.GroupOwnerSecurityRightName; }
			else
			{ return securityRight == GlbSecurity.ChangeOtherGroupSecurityRightName || securityRight == GlbSecurity.ChangeOtherStaffSecurityRightName; }
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			GlbSecurity security = (GlbSecurity)element;
			if (mode == ChangeOthersMode.GroupOwnersForGroup)
			{
				return staffOrGroupWhichCanChangeOthers != null &&
					staffOrGroupWhichCanChangeOthers.PK.IsValid &&
					securityRightMatches(security.GU_SecurityRight) &&
					(security.GU_ItemGUID == staffOrGroupWhichCanChangeOthers.PK) &&
					security.GU_SecurityItemIsAllowed;
			}
			else
			{
				return staffOrGroupWhichCanChangeOthers != null &&
					staffOrGroupWhichCanChangeOthers.PK.IsValid &&
					securityRightMatches(security.GU_SecurityRight) &&
					((security.GU_GG == staffOrGroupWhichCanChangeOthers.PK) || (security.GU_GS == staffOrGroupWhichCanChangeOthers.PK)) &&
					security.GU_SecurityItemIsAllowed;
			}
		}

		#region IReadOnlyFactoryForSecurityValidationProvider Members

		BusinessObjectFactory IReadOnlyFactoryForSecurityValidationProvider.ReadOnlyFactory
		{
			get { return readOnlyFactory ?? (readOnlyFactory = new BusinessObjectFactory()); }
		}

		#endregion
	}
}
