using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class StaffSecurityModuleFilter : ModuleGuidsFilter
	{
		public StaffSecurityModuleFilter(ZString description, GlbBranchCollection branchList, GlbDepartmentCollection departmentList)
			: this(description, ModuleIDs.GlbBranch, GlbStaffSchema.GS_GB_HomeBranch, branchList, GlbStaffSchema.GS_GE_HomeDepartment, departmentList)
		{
		}

		public StaffSecurityModuleFilter(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn1, GlbBranchCollection branchList, SchemaGuidColumn filterColumn2, GlbDepartmentCollection departmentList)
			: base(description, id, filterColumn1, branchList, filterColumn2, departmentList)
		{
			// The ModuleID and 2x SchemaColumns passed to the base are never used as we build our query ourselves.
			// But there is no base constructor that accepts just what we need.
		}

		#region Properties

		#region Security Filter Container

		SecurityFilterContainer securityFilterContainer;

		public SecurityFilterContainer SecurityFilterContainer
		{
			get
			{
				if (securityFilterContainer == null)
				{
					securityFilterContainer = new SecurityFilterContainer();
					RegisterEditableChildObject(securityFilterContainer);
				}
				return securityFilterContainer;
			}
		}

		#endregion

		#region Branch

		public ZGuid Branch
		{
			get { return Property1; }
			set { Property1 = value; }
		}

		#endregion

		#region Department

		public ZGuid Department
		{
			get { return Property2; }
			set { Property2 = value; }
		}

		#endregion

		#endregion

		#region Overrides

		protected override ModuleFilter ShallowCloneCore()
		{
			var result = (StaffSecurityModuleFilter)base.ShallowCloneCore();
			result.securityFilterContainer = null;
			return result;
		}

		#endregion

		#region Empty

		protected override bool IsEmptyCore => base.IsEmptyCore && SecurityFilterContainer.LookupKey.IsEmpty;

		#endregion

		#region Clear

		protected override void ClearCore()
		{
			base.ClearCore();
			SecurityFilterContainer.LookupKey = CheckpointLookupKey.Empty;
		}

		#endregion

		#region Query

		// Not used as the GlbStaffModule.LoadCollection() method does the filtering using business logic rather than DB Queries
		protected override ZQuery GetQuery()
		{
			ZQuery result = new ZQuery();
			if (!IsEmpty)
			{
				result.AddToFilter(GlbStaffSchema.GS_IsResource, ZBool.False);
			}

			return result;
		}

		protected override bool ShouldReevaluateQuery()
		{
			return true;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("SecurityRight", SecurityFilterContainer.LookupKey.Code);
			writer.WriteElementString("ItemGuid", SecurityFilterContainer.LookupKey.ItemGuid.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			string securityRight = reader.ReadElementString("SecurityRight");
			Guid itemGuid = (reader.Name == "ItemGuid") ? new Guid(reader.ReadElementString("ItemGuid")) : Guid.Empty;
			SecurityFilterContainer.LookupKey = new CheckpointLookupKey(securityRight, itemGuid);
		}

		#endregion
	}
}
