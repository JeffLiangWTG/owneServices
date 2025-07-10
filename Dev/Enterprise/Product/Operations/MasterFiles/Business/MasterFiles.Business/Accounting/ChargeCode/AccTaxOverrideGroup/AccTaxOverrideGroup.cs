using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AccTaxOverrideGroup.Schema.AX_Code)]
	[DescriptionProperty(AccTaxOverrideGroup.Schema.AX_Description)]
	public class AccTaxOverrideGroup : AutoAccTaxOverrideGroup, ITemplateCopyable, IDocManagerSupport, ICanDelete, IEDocsParsingSupport, IAuditParent
	{
		public AccTaxOverrideGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AX_RN_NKCountry = Env.CurrentCompany.Country.Code;
		}

		protected override AccTaxOverrideGroupValidation GetNewValidation()
		{
			return IsTaxFrameworkRelated ? new TaxFrameworkAccTaxOverrideGroupValidation(this) : base.GetNewValidation();
		}

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete && !(IsUsedInChargeCode || IsUsedForTaxFramework);
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var reason = base.ReasonForNotAbleToDelete;
				var chargeCodes = System.Array.Empty<AccChargeCode>();

				if (IsUsedForTaxFramework)
				{
					chargeCodes = ChargeCodesLinkedToTaxFrameworkConfiguration.ToArray();
				}
				else if (IsUsedInChargeCode)
				{
					chargeCodes = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_AX_TaxOverrideGroup, PK));
				}

				if (chargeCodes.Any())
				{
					var chargeCodesList = new ZStringBuilder(chargeCodes.Select(chargeCode => "'" + chargeCode.AC_Code + "'"));
					reason = ResString.GetMultilingualString("1debea90-151b-46dc-b66a-424c57f527e4", "The Tax Override Group cannot be deleted because it is used in charge codes: {0}.", chargeCodesList.ToStringWithDelimiterBetweenAppends(", "));
				}

				return reason;
			}
		}

		bool IsUsedInChargeCode
		{
			get
			{
				return Factory.Exists(typeof(AccChargeCode), new ZQuery(AccChargeCodeSchema.AC_AX_TaxOverrideGroup, PK));
			}
		}

		bool IsUsedForTaxFramework
		{
			get
			{
				return ChargeCodesLinkedToTaxFrameworkConfiguration.Count > 0;
			}
		}

		public override void Delete()
		{
			TaxOverrides.RemoveAndDeleteAll();
			TaxOverrideGroupTaxConfigurationPivots.DeleteAll();
			base.Delete();
		}

		[ChildEditable(true)]
		public AccChargeTaxOverrideCollection TaxOverrides
		{
			get
			{
				if (taxOverrides == null)
				{
					taxOverrides = new AccChargeTaxOverrideCollection(this);
					taxOverrides.Load();
					RegisterEditableChildObject(taxOverrides);
				}
				return taxOverrides;
			}
		}
		AccChargeTaxOverrideCollection taxOverrides;

		[ChildEditable(false)]
		public ChargeCodesLinkedToTaxOverrideGroupCollection ChargeCodes
		{
			get
			{
				if (ChargeCodes_innerValue == null)
				{
					ChargeCodes_innerValue = new ChargeCodesLinkedToTaxOverrideGroupCollection(this);
					RegisterEditableChildObject(ChargeCodes_innerValue);
				}
				return ChargeCodes_innerValue;
			}
		}
		ChargeCodesLinkedToTaxOverrideGroupCollection ChargeCodes_innerValue;

		[ChildEditable(false)]
		public ChargeCodesLinkedToTaxFrameworkConfigurationCollection ChargeCodesLinkedToTaxFrameworkConfiguration
		{
			get
			{
				if (chargeCodesLinkedToTaxFrameworkConfiguration == null)
				{
					chargeCodesLinkedToTaxFrameworkConfiguration = new ChargeCodesLinkedToTaxFrameworkConfigurationCollection(this);
					RegisterEditableChildObject(chargeCodesLinkedToTaxFrameworkConfiguration);
				}
				return chargeCodesLinkedToTaxFrameworkConfiguration;
			}
		}
		ChargeCodesLinkedToTaxFrameworkConfigurationCollection chargeCodesLinkedToTaxFrameworkConfiguration;

		public ZBool IsTaxFrameworkRelated => this.HasContext(BusinessContext.TaxFramework) || Factory.Exists(typeof(AccTaxOverrideGroupTaxConfigurationPivot), new ZQuery(AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_AX_TaxOverrideGroup, PK), false);

		[ChildEditable(true)]
		public AccTaxOverrideGroupTaxConfigurationPivotCollection TaxOverrideGroupTaxConfigurationPivots
		{
			get
			{
				if (taxOverrideGroupTaxConfigurationPivots == null)
				{
					taxOverrideGroupTaxConfigurationPivots = new AccTaxOverrideGroupTaxConfigurationPivotCollection(this);
					RegisterEditableChildObject(taxOverrideGroupTaxConfigurationPivots);
				}

				return taxOverrideGroupTaxConfigurationPivots;
			}
		}
		AccTaxOverrideGroupTaxConfigurationPivotCollection taxOverrideGroupTaxConfigurationPivots;

		public void DeleteDuplicateTaxOverridesInChargeCodes()
		{
			List<AccChargeTaxOverride> removingOverrides = new List<AccChargeTaxOverride>();
			foreach (AccChargeCode chargeCode in ChargeCodes)
			{
				foreach (AccChargeTaxOverride taxOverride in TaxOverrides)
				{
					foreach (AccChargeTaxOverride taxOverride2 in chargeCode.TaxOverrides)
					{
						if (taxOverride2.IsDuplicate(taxOverride))
						{
							removingOverrides.Add(taxOverride2);
						}
					}
					foreach (AccChargeTaxOverride removingOverride in removingOverrides)
					{
						chargeCode.TaxOverrides.RemoveAndDelete(removingOverride);
					}
					removingOverrides.Clear();
				}
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("84AE9F12-1843-4D3A-8A17-BF02030522B8", "Tax Override Groups - {0}", CalculateShortcutName());

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			AccTaxOverrideGroup copiedTaxOverrideGroup = Factory.New<AccTaxOverrideGroup>();
			copiedTaxOverrideGroup.CopyPersistentValuesFrom(this);

			BusinessObjectCloneArgs taxOverrideArgs = new BusinessObjectCloneArgs(new string[] { AccChargeTaxOverrideSchema.Constants.AO_ParentID, AccChargeTaxOverrideSchema.Constants.AO_ParentTableCode });
			foreach (AccChargeTaxOverride taxOverride in TaxOverrides)
			{
				AccChargeTaxOverride copiedOverride = copiedTaxOverrideGroup.TaxOverrides.AddNew();
				using (copiedOverride.GetDefaultingSuspender())
				{
					copiedOverride.CopyPersistentValuesFrom(taxOverride, taxOverrideArgs);
				}
			}
			return copiedTaxOverrideGroup;
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.TaxOverrideGroup)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		public enum BusinessContext
		{
			TaxFramework
		}

		#region AX_GroupType

		public ZString AX_GroupType
		{
			get
			{
				if (IsTaxFrameworkRelated)
				{
					return string.Join(", ", TaxOverrideGroupTaxConfigurationPivots.Select(x => x.TaxConfiguration?.ETC_Code));
				}
				return (ZString)GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription;
			}
		}

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(AccChargeTaxOverrideSchema.AO_ParentID, null);
				yield return new AuditChildInfo(AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_AX_TaxOverrideGroup, null);
			}
		}

		#endregion
	}
}
