using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceDefaultingConfiguration : AutoAccCashAdvanceDefaultingConfigurationView, IJobConfiguration
	{
		public AccCashAdvanceDefaultingConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool ReadOnly
		{
			get
			{
				var result = base.ReadOnly;

				var parentCollections = ((IBusinessObjectInternals)this).ParentCollections.OfType<AccCashAdvanceDefaultingConfigurationCollection>();
				if (parentCollections != null)
				{
					result = parentCollections.Any(x => x.Level != Level);
				}

				return result;
			}
			set => base.ReadOnly = value;
		}

		#region Properties

		#region CAC_Ledger

		[ResourceStringData("AccCashAdvanceDefaultingConfiguration|CAC_Ledger", Caption = "Ledger")]
		[List("Lookups.LedgerList")]
		[MaxLength(Schema.CAC_LedgerMaxLength)]
		public override ZString CAC_Ledger
		{
			get => base.CAC_Ledger;
			set => base.CAC_Ledger = value;
		}

		protected bool CAC_Ledger_ReadOnly => Level == AccCashAdvanceDefaultingLevel.Debtor || Level == AccCashAdvanceDefaultingLevel.Creditor;

		#endregion

		#region CAC_JobType

		[ResourceStringData("AccCashAdvanceDefaultingConfiguration|CAC_JobType", Caption = "Job Type")]
		[List("Lookups.JobTypeList")]
		public override ZString CAC_JobType
		{
			get => base.CAC_JobType;
			set => base.CAC_JobType = value;
		}

		#endregion

		#region CAC_ServiceDirection

		[ResourceStringData("AccCashAdvanceDefaultingConfiguration|CAC_ServiceDirection", Caption = "Service Direction", ShortCaption = "Direction")]
		[List("Lookups.DirectionsList")]
		public override ZString CAC_ServiceDirection
		{
			get => base.CAC_ServiceDirection;
			set => base.CAC_ServiceDirection = value;
		}

		#endregion

		#region CAC_TransportMode

		[ResourceStringData("AccCashAdvanceDefaultingConfiguration|CAC_TransportMode", Caption = "Transport Mode")]
		[List("Lookups.TransportModesList")]
		public override ZString CAC_TransportMode
		{
			get => base.CAC_TransportMode;
			set => base.CAC_TransportMode = value;
		}

		#endregion

		#region CAC_DefaultingOption

		[ResourceStringData("AccCashAdvanceDefaultingConfiguration|CAC_DefaultingOption", Caption = "Defaulting Charges")]
		[List("Lookups.DefaultingOptions")]
		public override ZString CAC_DefaultingOption
		{
			get => base.CAC_DefaultingOption;
			set
			{
				base.CAC_DefaultingOption = value;
				RemoveAndDeleteChargeCodesAndChargeGroups();
			}
		}

		void RemoveAndDeleteChargeCodesAndChargeGroups()
		{
			var shouldRemoveAndDelete = CAC_DefaultingOption != CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;
			if (shouldRemoveAndDelete)
			{
				ChargeCodes.RemoveAndDeleteAll();
				LinkedChargeCodes.RemoveAll();
				ChargeGroups.RemoveAndDeleteAll();
			}
		}

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("d1cf4d8f-d9b9-4348-9099-a3fe24d2550e", "Advance Payment Configuration");

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CAC_ConfigType = JobConfiguration.TypeCodes.CashAdvanceDefaulting;
			CAC_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			CAC_TransportMode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.All;
			CAC_GC = GlbCompany.CurrentCompany.PK;
			CAC_DefaultingOption = CashAdvanceDefaultingOption.All;
		}

		#endregion

		public override void Delete()
		{
			ChargeCodes.RemoveAndDeleteAll();
			LinkedChargeCodes.RemoveAll();
			ChargeGroups.RemoveAndDeleteAll();
			base.Delete();
		}

		public void SyncChargeCodes(IEnumerable<ZGuid> chargeCodesToSync)
		{
			foreach (var chargeCodePk in chargeCodesToSync)
			{
				var chargeCodeToSync = ChargeCodes.AddNew();
				chargeCodeToSync.JCT_ParentId = chargeCodePk;
			}
		}

		public void UnsyncChargeCodes(IEnumerable<ZGuid> chargeCodesToUnsync)
		{
			foreach (var chargeCodePk in chargeCodesToUnsync)
			{
				var chargeCodeToUnsync = ChargeCodes.Cast<CashAdvanceDefaultingChargeCode>().FirstOrDefault(x => x.JCT_ParentId == chargeCodePk);
				if (chargeCodeToUnsync != null)
				{
					ChargeCodes.RemoveAndDelete(chargeCodeToUnsync);
				}
			}
		}

		#region Child Collections

		#region ChargeCodes

		[ChildEditable(false)]
		public CashAdvanceDefaultingChargeCodeCollection ChargeCodes
		{
			get
			{
				if (chargeCodes == null)
				{
					chargeCodes = new CashAdvanceDefaultingChargeCodeCollection(this);
					chargeCodes.Load();
					RegisterEditableChildObject(chargeCodes);
				}
				return chargeCodes;
			}
		}
		CashAdvanceDefaultingChargeCodeCollection chargeCodes;

		[ChildEditable(false)]
		[List("Lookups.ChargeCodeFindBoxCollection")]
		public AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection LinkedChargeCodes
		{
			get
			{
				if (linkedChargeCodes == null)
				{
					linkedChargeCodes = new AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection(this);
					linkedChargeCodes.Load();
					RegisterEditableChildObject(linkedChargeCodes);
				}

				return linkedChargeCodes;
			}
		}
		AccCashAdvanceDefaultingConfigurationLinkedChargeCodeCollection linkedChargeCodes;

		#endregion

		#region Charge Groups

		[ChildEditable(true)]
		public CashAdvanceDefaultingChargeGroupCollection ChargeGroups
		{
			get
			{
				if (chargeGroups == null)
				{
					chargeGroups = new CashAdvanceDefaultingChargeGroupCollection(this);
					chargeGroups.Load();
					RegisterEditableChildObject(chargeGroups);
				}
				return chargeGroups;
			}
		}
		CashAdvanceDefaultingChargeGroupCollection chargeGroups;

		#endregion

		#endregion

		#region Level

		public AccCashAdvanceDefaultingLevel Level => GetLevelByParentTableCode(CAC_ParentTableCode);

		public ZString LevelName => Level.GetLevelName();

		AccCashAdvanceDefaultingLevel GetLevelByParentTableCode(ZString parentTableCode)
		{
			switch (parentTableCode)
			{
				case "":
					return AccCashAdvanceDefaultingLevel.Company;
				case GlbBranchSchema.Constants.Prefix:
					return AccCashAdvanceDefaultingLevel.Branch;
				case OrgHeaderSchema.Constants.Prefix:
					if (CAC_Ledger == LedgerTypes.AccountsReceivable)
					{
						return AccCashAdvanceDefaultingLevel.Debtor;
					}
					else if (CAC_Ledger == LedgerTypes.AccountsPayable)
					{
						return AccCashAdvanceDefaultingLevel.Creditor;
					}
					else
					{
						return AccCashAdvanceDefaultingLevel.Null;
					}
				default:
					return AccCashAdvanceDefaultingLevel.Null;
			}
		}

		#endregion

		#region IsDuplicateOf()

		public bool IsDuplicateOf(AccCashAdvanceDefaultingConfiguration other)
			=> PK != other.PK
			&& CAC_GC == other.CAC_GC
			&& CAC_ParentTableCode == other.CAC_ParentTableCode
			&& CAC_ParentId == other.CAC_ParentId
			&& CAC_Ledger == other.CAC_Ledger
			&& CAC_JobType == other.CAC_JobType
			&& CAC_ServiceDirection == other.CAC_ServiceDirection
			&& CAC_TransportMode == other.CAC_TransportMode;

		#endregion

		#region IJobConfiguration Members

		ZString IJobConfiguration.JobType => CAC_JobType;

		ZString IJobConfiguration.ServiceDirection => CAC_ServiceDirection;

		ZString IJobConfiguration.TransportMode => CAC_TransportMode;

		bool IJobConfiguration.IncludeOptionsForAllJobTypes => true;

		#endregion
	}
}
