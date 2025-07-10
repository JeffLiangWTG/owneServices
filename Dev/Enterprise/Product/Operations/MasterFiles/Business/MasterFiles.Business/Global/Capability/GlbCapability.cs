using System.Data;
using System.Diagnostics;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(GlbCapability.Schema.G4_Code), DescriptionProperty(GlbCapability.Schema.G4_Description)]
	[DebuggerDisplay("'{G4_Code} - {G4_Description}")]
	public class GlbCapability : AutoGlbCapability, IDocManagerSupport, IGlbCapability
	{
		public GlbCapability(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		public GlbResourceCapabilityPivot ResourcePivot { get; set; }

		public GlbStaffCapabilityMembersCollection ResourcesWithCapability
		{
			get
			{
				if (resourcesWithCapability == null)
				{
					resourcesWithCapability = new GlbStaffCapabilityMembersCollection(this);
					resourcesWithCapability.Load();
				}

				return resourcesWithCapability;
			}
		}
		GlbStaffCapabilityMembersCollection resourcesWithCapability;

		[ChildEditable]
		public GlbCapabilityGroupPivotCollection ReleaseGroupPivots
		{
			get
			{
				if (releaseGroupPivot == null)
				{
					releaseGroupPivot = new GlbCapabilityGroupPivotCollection(this);
					RegisterEditableChildObject(releaseGroupPivot);
				}

				return releaseGroupPivot;
			}
		}

		GlbCapabilityGroupPivotCollection releaseGroupPivot;

		#endregion

		#region Properties

		#region G4_AutoAssignTasksAge

		[ZDateTimeDurationValue]
		[ReadOnlyMember(nameof(AutoTaskAssignUnavailable))]
		public override ZDateTime G4_AutoAssignTasksAge
		{
			get { return base.G4_AutoAssignTasksAge; }
			set { base.G4_AutoAssignTasksAge = value.ConvertToDurationBasedDate(G4_AutoAssignTasksAgeInfo); }
		}

		ZBool AutoTaskAssignUnavailable
		{
			get { return !G4_AllowTaskAutoAssignment; }
		}

		#endregion

		[List("Lookups.ScopeCodes")]
		public override ZString G4_CapacityScope
		{
			get { return base.G4_CapacityScope; }
			set { base.G4_CapacityScope = value; }
		}

		#endregion

		#region BusinessObject overrides

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("4d156df0-fb3a-4e9c-8a9f-c784c2655205", "Resource Capability - {0}", G4_Code); }
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}: {1} - {2}", nameof(GlbCapability), G4_Code, G4_Description);
		}

		public override void Delete()
		{
			base.Delete();

			ReleaseGroupPivots.DeleteAll();
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Capability)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion
	}
}
