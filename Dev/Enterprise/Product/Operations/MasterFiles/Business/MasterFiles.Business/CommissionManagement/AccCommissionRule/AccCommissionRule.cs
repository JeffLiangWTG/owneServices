using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRule : AutoAccCommissionRule, ICommissionRule
	{
		public AccCommissionRule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new AccCommissionRuleTypeDecider();

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (!CommissionLookups.ShouldShowServicesAndSubModules)
			{
				ACM_Service = CommissionRuleLookups.AnyServicesCode;
				ACM_SubModule = CommissionRuleLookups.AnySubModulesCode;
				ACM_Mode = OrgCommissionAgreementItem.AllItemCode;
			}
		}

		#endregion

		#region Properties

		#region ACM_GC

		[List("Lookups.Companies")]
		public override ZGuid ACM_GC
		{
			get
			{
				if (ACM_GG.IsValid)
				{
					var group = Group;
					return group != null ? group.GG_GC : ZGuid.Empty;
				}
				else
				{
					return base.ACM_GC;
				}
			}
			set
			{
				if (ACM_GG.IsValid)
				{
					throw new InvalidOperationException("ACM_GC can not be set on the rule level while it is linked to a group. It is inherited from the linked group.");
				}

				base.ACM_GC = value;
			}
		}

		protected bool ACM_GC_ReadOnly
		{
			get { return ACM_GG.IsValid; }
		}

		#endregion

		#region ACM_GG

		[List("Lookups.SalesTeams")]
		public override ZGuid ACM_GG
		{
			get { return base.ACM_GG; }
			set
			{
				if (base.ACM_GG != value)
				{
					base.ACM_GG = value;

					if (!value.IsEmpty)
					{
						base.ACM_GC = ZGuid.Empty;
					}
				}
			}
		}

		#endregion

		#region ACM_Product

		[List("Lookups.Products")]
		public override ZString ACM_Product
		{
			get { return base.ACM_Product; }
			set
			{
				base.ACM_Product = value;
				if (!CommissionRuleLookups.ProductSupportsTradeLane(ACM_Product))
				{
					ACM_Mode = OrgCommissionAgreementItem.AllItemCode;
					ACM_NKOrigin = ZString.Empty;
					ACM_NKDestination = ZString.Empty;
				}
			}
		}

		#endregion

		#region ACM_Service

		[List("Lookups.Services")]
		public override ZString ACM_Service
		{
			get { return base.ACM_Service; }
			set { base.ACM_Service = value; }
		}

		#endregion

		#region ACM_SubModule

		[List("Lookups.SubModules")]
		public override ZString ACM_SubModule
		{
			get { return base.ACM_SubModule; }
			set { base.ACM_SubModule = value; }
		}

		#endregion

		#region ACM_Mode

		[List("Lookups.Modes")]
		public override ZString ACM_Mode
		{
			get { return base.ACM_Mode; }
			set { base.ACM_Mode = value; }
		}

		public bool ACM_Mode_ReadOnly
		{
			get
			{
				return !CommissionRuleLookups.ProductSupportsTradeLane(ACM_Product);
			}
		}

		#endregion

		#region Origin

		[List("Lookups.Locations")]
		public override ZString ACM_NKOrigin
		{
			get { return base.ACM_NKOrigin; }
			set { base.ACM_NKOrigin = value; }
		}

		public bool ACM_NKOrigin_ReadOnly
		{
			get
			{
				return !CommissionRuleLookups.ProductSupportsTradeLane(ACM_Product);
			}
		}

		#endregion

		#region Destination

		[List("Lookups.Locations")]
		public override ZString ACM_NKDestination
		{
			get { return base.ACM_NKDestination; }
			set { base.ACM_NKDestination = value; }
		}

		public bool ACM_NKDestination_ReadOnly
		{
			get
			{
				return !CommissionRuleLookups.ProductSupportsTradeLane(ACM_Product);
			}
		}

		#endregion

		#region ACM_CommissionBasis

		[List("Lookups.CommissionBasisType")]
		public override ZString ACM_CommissionBasis
		{
			get { return base.ACM_CommissionBasis; }
			set { base.ACM_CommissionBasis = value; }
		}

		#endregion

		#region ACM_CommissionTriggerType

		[List("Lookups.TriggerTypes")]
		public override ZString ACM_CommissionTriggerType
		{
			get { return base.ACM_CommissionTriggerType; }
			set { base.ACM_CommissionTriggerType = value; }
		}

		#endregion

		#endregion

		#region Related Business Objects

		[ChildEditable]
		public AccCommissionRuleRateCollection Rates
		{
			get
			{
				if (rates == null)
				{
					rates = new AccCommissionRuleRateCollection(this);
					RegisterEditableChildObject(rates);
				}

				return rates;
			}
		}
		AccCommissionRuleRateCollection rates;

		#endregion

		#region Delete

		public override void Delete()
		{
			Rates.DeleteAll();

			base.Delete();
		}

		#endregion

		#region ICommissionRule Members

		ZGuid ICommissionRule.GroupPk
		{
			get { return ACM_GG; }
		}

		GlbGroup ICommissionRule.Group
		{
			get { return Group; }
		}

		ZGuid ICommissionRule.CompanyPk
		{
			get { return ACM_GC; }
		}

		ZString ICommissionRule.Product
		{
			get { return ACM_Product; }
		}

		ZString ICommissionRule.Service
		{
			get { return ACM_Service; }
		}

		ZString ICommissionRule.SubModule
		{
			get { return ACM_SubModule; }
		}

		ZString ICommissionRule.Mode
		{
			get { return ACM_Mode; }
		}

		ZString ICommissionRule.Origin
		{
			get { return ACM_NKOrigin; }
		}

		ZString ICommissionRule.Destination
		{
			get { return ACM_NKDestination; }
		}

		ZDate ICommissionRule.StartDate
		{
			get { return ACM_StartDate; }
		}

		ZDate ICommissionRule.EndDate
		{
			get { return ACM_EndDate; }
		}

		ZString ICommissionRuleRatesProvider.CommissionBasis
		{
			get { return ACM_CommissionBasis; }
			set { ACM_CommissionBasis = value; }
		}

		ZString ICommissionRuleRatesProvider.CommissionTriggerType
		{
			get { return ACM_CommissionTriggerType; }
			set { ACM_CommissionTriggerType = value; }
		}

		#endregion

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (ACM_CommissionTriggerType.IsEmpty)
			{
				ACM_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			}

			if (ACM_CommissionBasis.IsEmpty)
			{
				ACM_CommissionBasis = CommissionBasisType.Codes.REV;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
