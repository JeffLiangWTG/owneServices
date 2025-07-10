using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGCountryReferencePivot : AutoUNDGCountryReferencePivot
	{
		public UNDGCountryReferencePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid DCP_DG_Virtual
		{
			get
			{
				var query = new ZQuery();
				query.AddToFilter(UNDGSubstanceSchema.DG_UNNO, DCP_UNNO);
				query.AddToFilter(UNDGSubstanceSchema.DG_Variant, DCP_Variant);
				query.AddToFilter(UNDGSubstanceSchema.DG_Standard, DCP_Standard);
				return Factory.LoadTop1<UNDGSubstance>(query)?.PK ?? ZGuid.Empty;
			}
			set
			{
				var subs = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.PK, value));
				if (subs == null)
				{
					DCP_UNNO = ZString.Empty;
					DCP_Variant = ZString.Empty;
					DCP_Standard = ZString.Empty;
					return;
				}
				DCP_UNNO = subs.DG_UNNO;
				DCP_Variant = subs.DG_Variant;
				DCP_Standard = subs.DG_Standard;
			}
		}

		[ResourceStringData("Enterprise.MasterFiles.Business.UNDGCountryReferencePivot|DCP_StorageInstruction", Caption = "Storage Instruction Category", ShortCaption = "Storage Cat.")]
		[List("Lookups.StorageInstructions")]
		public override ZString DCP_StorageInstruction
		{
			get => base.DCP_StorageInstruction;
			set => base.DCP_StorageInstruction = value;
		}

		protected bool DCP_StorageInstruction_ReadOnly => StorageInstructionAndRetentionTrayReadOnly;

		[ResourceStringData("Enterprise.MasterFiles.Business.UNDGCountryReference|DCR_TankStorageInstructionRetentionTray", Caption = "Retention Tray Required", ShortCaption = "Retention Tray")]
		public override ZBool DCP_TankStorageInstructionRetentionTray
		{
			get => base.DCP_TankStorageInstructionRetentionTray;
			set => base.DCP_TankStorageInstructionRetentionTray = value;
		}

		protected bool DCP_TankStorageInstructionRetentionTray_ReadOnly => StorageInstructionAndRetentionTrayReadOnly;

		bool StorageInstructionAndRetentionTrayReadOnly
		{
			get
			{
				return !Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed
					|| !(UNDGCountryReference.DCR_RN_NKCountry == Core.Constants.CountryCodes.France && UNDGCountryReference.DCR_Type.EqualsIgnoringCase(Core.Constants.UNDGCountryReference.Type.ICPE));
			}
		}
	}
}
