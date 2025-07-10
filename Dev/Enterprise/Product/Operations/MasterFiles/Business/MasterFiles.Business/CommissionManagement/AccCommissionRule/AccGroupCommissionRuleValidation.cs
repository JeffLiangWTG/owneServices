using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccGroupCommissionRuleValidation : AccCommissionRuleValidation
	{
		public AccGroupCommissionRuleValidation(AccGroupCommissionRule parent)
			: base(parent)
		{
		}

		new AccGroupCommissionRule Parent
		{
			get { return (AccGroupCommissionRule)base.Parent; }
		}

		protected override void CheckACM_EndDate()
		{
			base.CheckACM_EndDate();
			CheckUnique(Parent.ACM_EndDateInfo);
		}

		protected override void CheckACM_GC()
		{
			base.CheckACM_GC();
			CheckUnique(Parent.ACM_GCInfo);
		}

		protected override void CheckACM_Product()
		{
			base.CheckACM_Product();
			CheckUnique(Parent.ACM_ProductInfo);
		}

		protected override void CheckACM_Service()
		{
			base.CheckACM_Service();
			CheckUnique(Parent.ACM_ServiceInfo);
		}

		protected override void CheckACM_Mode()
		{
			base.CheckACM_Mode();
			if (CommissionRuleLookups.ProductSupportsTradeLane(Parent.ACM_Product))
			{
				MandatoryValidation.CheckEntered(Parent.ACM_ModeInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.ACM_ModeInfo);
			CheckUnique(Parent.ACM_ModeInfo);
		}

		protected override void CheckACM_NKOrigin()
		{
			base.CheckACM_NKOrigin();
			CheckUnique(Parent.ACM_NKOriginInfo);
		}

		protected override void CheckACM_NKDestination()
		{
			base.CheckACM_NKDestination();
			CheckUnique(Parent.ACM_NKDestinationInfo);
		}

		protected override void CheckACM_StartDate()
		{
			base.CheckACM_StartDate();
			CheckUnique(Parent.ACM_StartDateInfo);
		}

		protected override void CheckACM_SubModule()
		{
			base.CheckACM_SubModule();
			CheckUnique(Parent.ACM_SubModuleInfo);
		}

		protected void CheckUnique(ZPropertyInfo propertyInfo)
		{
			var salesTeam = Parent.Group as SalesTeam;
			if (salesTeam != null)
			{
				var hasConflictingRule = salesTeam.CommissionRules.Any(rule => rule.PK != Parent.PK
					&& rule.ACM_Product == Parent.ACM_Product
					&& rule.ACM_Service == Parent.ACM_Service
					&& rule.ACM_SubModule == Parent.ACM_SubModule
					&& rule.ACM_Mode == Parent.ACM_Mode
					&& rule.ACM_NKOrigin == Parent.ACM_NKOrigin
					&& rule.ACM_NKDestination == Parent.ACM_NKDestination
					&& rule.HasOverlappingDateRange(Parent));

				if (hasConflictingRule)
				{
					var overlappingItemCombinationString = CommissionLookups.ShouldShowServicesAndSubModules ?
						Res.GetString("4f231b20-31b5-457e-95ca-1f5539bd9d4d", "Product, Service and Sub-Module") :
						Res.GetString("089322db-790e-40dc-b74a-c551c0b246d3", "Product, Mode, Origin and Destination");

					propertyInfo.AddError(Res.GetString("f1e506a9-3116-4860-91ec-c142574520b1", "Another rule with the same {0} has overlapping Start/End Dates.", overlappingItemCombinationString));
				}
			}
		}
	}
}
