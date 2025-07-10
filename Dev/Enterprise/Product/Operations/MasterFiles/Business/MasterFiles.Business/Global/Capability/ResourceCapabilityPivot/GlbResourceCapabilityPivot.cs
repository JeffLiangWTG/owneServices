using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class GlbResourceCapabilityPivot : AutoGlbResourceCapabilityPivot, IObsoleteValidation
	{
		public GlbResourceCapabilityPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			G5_DateExperienceGained = ZDateTime.UtcNow;
			G5_SkillLevel = 1;
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				Capability.Logs.AddNew(Events.Attached, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Staff, Resource.GS_Code));
				Resource.Logs.AddNew(Events.Attached, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Capability, Capability.G4_Code));
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			if (IsInDatabase && !IsDeleting && Capability is { } capability && Resource is { } staff)
			{
				capability.Logs.AddNew(Events.Detached, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Staff, staff.GS_Code));
				staff.Logs.AddNew(Events.Detached, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Capability, capability.G4_Code));
			}
			base.Delete();
		}

		#region New properties

		#region SkillLevel

		[List("SkillLevels")]
		[ResourceStringData("GlbResourceCapabilityPivot.SkillLevel", Caption = "Skill Level")]
		public ZString SkillLevel
		{
			get { return G5_SkillLevel.ToString(); }
			set
			{
				Byte byteValue;
				if (byte.TryParse(value, out byteValue))
				{
					G5_SkillLevel = byteValue;
				}
				else
				{
					G5_SkillLevel = 0;
				}
				SkillLevelInfo.RefreshBinding();
				G5_SkillLevelInfo.RefreshBinding();
				SkillLevelDescriptionInfo.RefreshBinding();
			}
		}

		public ZWrappedPropertyInfo SkillLevelInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetWrappedZPropertyInfo(nameof(SkillLevel), (o) => G5_SkillLevelInfo); }
		}

		// NOTE: If these values change change please also update Report_ResourceCapabilityAssignment SkillLevel values to match
		public CodeDescriptionPairList SkillLevels
		{
			get
			{
				if (skillLevels == null)
				{
					skillLevels = new CodeDescriptionPairList();
					skillLevels.AddPair("1", Res.GetString("4c8939ce-2e65-49f5-a738-81076167e8b3", "Achieved"));
					skillLevels.AddPair("2", Res.GetString("9907c6ac-8291-41b1-b0fd-5d0906673e76", "Poor"));
					skillLevels.AddPair("3", Res.GetString("ac020ce8-b3b4-4783-b49d-85517662017a", "Average"));
					skillLevels.AddPair("4", Res.GetString("b1499b71-5e1f-4e03-9e75-14910a6c9f9d", "Excellent"));
				}
				return skillLevels;
			}
		}
		CodeDescriptionPairList skillLevels;

		#endregion

		#region SkillLevelDescription

		[ResourceStringData("GlbResourceCapabilityPivot.SkillLevelDescription", Caption = "Skill Level")]
		public ZString SkillLevelDescription
		{
			get
			{
				if (G5_SkillLevel > 0 && G5_SkillLevel <= SkillLevels.Count)
				{
					return SkillLevels[G5_SkillLevel - 1].Description;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo SkillLevelDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SkillLevelDescription)); }
		}

		#endregion

		#endregion
	}
}
