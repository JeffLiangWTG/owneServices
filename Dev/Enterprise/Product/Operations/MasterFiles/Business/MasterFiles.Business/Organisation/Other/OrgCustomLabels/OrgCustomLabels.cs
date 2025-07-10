using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	[DebuggerDisplay("OT_FieldName = {OT_FieldName}, OT_Caption = {OT_Caption}")]
	public class OrgCustomLabels : AutoOrgCustomLabels
	{
		public OrgCustomLabels(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly OrgCustomLabelsTypeDecider TypeDecider = new OrgCustomLabelsTypeDecider();

		public ZBool OT_UseDefaultCaption
		{
			get { return OT_Caption.Length == 0; }
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log reference")]
		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString logReference = (NoResString)"Label";
				logReference += (NoResString)" Field Name: " + OT_FieldName + (OT_FieldNameInfo.HasChanges ? (NoResString)"(" + OT_FieldNameInfo.OriginalValue + (NoResString)")" : (NoResString)"");
				logReference += " Caption: " + OT_Caption + (OT_CaptionInfo.HasChanges ? "(" + OT_CaptionInfo.OriginalValue + ")" : "");

				return logReference;
			}
		}

		#endregion

		#region Properties

		[List("Lookups.OT_FieldName_List")]
		public override ZString OT_FieldName
		{
			get { return base.OT_FieldName; }
			set
			{
				base.OT_FieldName = value;
				ZString defaultCaption = GetDefaultCaptionOnField();
				if (!defaultCaption.IsEmpty)
				{
					OT_Caption = defaultCaption.SubstringSafe(0, OT_CaptionInfo.MaxLength);
				}
			}
		}

		public OrgHeader ParentOrg
		{
			get { return base.Header; }
		}

		#endregion

		#region Implementation

		protected override OrgCustomLabelsLookups GetNewLookups()
		{
			return new OrgCustomLabelsLookups(this);
		}

		protected ZString GetDefaultCaptionOnField()
		{
			switch (OT_FieldName)
			{
				case LandedCostingCustomDocumentLabelsList.Codes.LandedCostGroup1:
					return GetLandedCostGroupNameFromID(1);
				case LandedCostingCustomDocumentLabelsList.Codes.LandedCostGroup2:
					return GetLandedCostGroupNameFromID(2);
				case LandedCostingCustomDocumentLabelsList.Codes.LandedCostGroup3:
					return GetLandedCostGroupNameFromID(3);
				case LandedCostingCustomDocumentLabelsList.Codes.LandedCostGroup4:
					return GetLandedCostGroupNameFromID(4);
				case LandedCostingCustomDocumentLabelsList.Codes.LandedCostGroup5:
					return GetLandedCostGroupNameFromID(5);
				case LandedCostingCustomDocumentLabelsList.Codes.LandedCostGroup6:
					return GetLandedCostGroupNameFromID(6);
				case LandedCostingCustomDocumentLabelsList.Codes.LandedCostGroupMisc:
					return Res.GetString("2ed688cb-30c7-4d13-866d-c69e63795f88", "Misc Charges");
				case LandedCostingCustomDocumentLabelsList.ExtraCodes.SpecialTax1:
				case LandedCostingCustomDocumentLabelsList.ExtraCodes.SpecialTax2:
				case LandedCostingCustomDocumentLabelsList.ExtraCodes.SpecialTax3:
					return Lookups.OT_FieldName_List.GetDescriptionFromCode(OT_FieldName);
				default:
					return "";
			}
		}

		ZString GetLandedCostGroupNameFromID(ZByte groupID)
		{
			return PreferenceCalculator.GetLandedCostingGroupNameFromID(groupID);
		}

		LCPreferenceFallBackCalculator PreferenceCalculator
		{
			get
			{
				if (fPreferenceCalculator == null)
				{
					fPreferenceCalculator = new LCPreferenceFallBackCalculator(ParentOrg);
				}
				return fPreferenceCalculator;
			}
		}
		LCPreferenceFallBackCalculator fPreferenceCalculator;

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = false;
			var header = Header;
			if (header != null)
			{
				result = !header.SecurityProvider.HasModifyCustomSecurity;
			}
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
