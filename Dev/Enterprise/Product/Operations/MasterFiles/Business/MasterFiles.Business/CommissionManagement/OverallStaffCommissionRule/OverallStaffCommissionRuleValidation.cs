using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OverallStaffCommissionRuleValidation : AutoOverallStaffCommissionRuleValidation
	{
		public OverallStaffCommissionRuleValidation(AutoOverallStaffCommissionRule parent)
			: base(parent)
		{
		}

		new OverallStaffCommissionRule Parent
		{
			get { return (OverallStaffCommissionRule)base.Parent; }
		}

		#region Properties

		protected override void CheckProduct()
		{
			base.CheckProduct();

			MandatoryValidation.CheckEntered(Parent.ProductInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ProductInfo);
			CheckUnique(Parent.ProductInfo);
		}

		protected override void CheckService()
		{
			base.CheckService();

			MandatoryValidation.CheckEntered(Parent.ServiceInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ServiceInfo);
			CheckUnique(Parent.ServiceInfo);
		}

		protected override void CheckSubModule()
		{
			base.CheckSubModule();

			MandatoryValidation.CheckEntered(Parent.SubModuleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.SubModuleInfo);
			CheckUnique(Parent.SubModuleInfo);
		}

		protected override void CheckMode()
		{
			base.CheckMode();

			if (CommissionRuleLookups.ProductSupportsTradeLane(Parent.Product))
			{
				MandatoryValidation.CheckEntered(Parent.ModeInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.ModeInfo);
			CheckUnique(Parent.ModeInfo);
		}

		protected override void CheckOrigin()
		{
			base.CheckOrigin();

			ListValidation.ErrorIfInvalidCode(Parent.OriginInfo);
			CheckUnique(Parent.OriginInfo);
		}

		protected override void CheckDestination()
		{
			base.CheckDestination();

			ListValidation.ErrorIfInvalidCode(Parent.DestinationInfo);
			CheckUnique(Parent.DestinationInfo);
		}

		protected override void CheckCommissionBasis()
		{
			base.CheckCommissionBasis();

			MandatoryValidation.CheckEntered(Parent.CommissionBasisInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CommissionBasisInfo);
		}

		protected override void CheckCommissionTriggerType()
		{
			base.CheckCommissionTriggerType();

			MandatoryValidation.CheckEntered(Parent.CommissionTriggerTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CommissionTriggerTypeInfo);
		}

		protected override void CheckStartDate()
		{
			base.CheckStartDate();
			CompareValidation.CheckDateIsBeforeAnotherDate(Parent.StartDateInfo, Parent.EndDateInfo);
			CheckUnique(Parent.StartDateInfo);
		}

		protected override void CheckEndDate()
		{
			base.CheckEndDate();
			CompareValidation.CheckDateIsAfterAnotherDate(Parent.EndDateInfo, Parent.StartDateInfo);
			CheckUnique(Parent.EndDateInfo);
		}

		protected override void CheckCompanyPk()
		{
			base.CheckCompanyPk();
			CheckUnique(Parent.CompanyPkInfo);
		}

		protected override void CheckGroupPk()
		{
			base.CheckGroupPk();
			CheckUnique(Parent.GroupPkInfo);
		}

		void CheckUnique(ZPropertyInfo propertyInfo)
		{
			var staff = Parent.Staff;
			if (staff != null)
			{
				var conflictingRule = staff.OverallCommissionRules.Where(rule => !rule.IsDeleted).Cast<OverallStaffCommissionRule>().FirstOrDefault(rule => rule.BaseRule.PK != Parent.BaseRule.PK
					&& rule.CompanyPk == Parent.CompanyPk
					&& rule.GroupPk == Parent.GroupPk
					&& rule.Product == Parent.Product
					&& rule.Service == Parent.Service
					&& rule.SubModule == Parent.SubModule
					&& rule.Mode == Parent.Mode
					&& rule.Origin == Parent.Origin
					&& rule.Destination == Parent.Destination
					&& rule.HasOverlappingDateRange(Parent));

				if (conflictingRule != null)
				{
					var team = conflictingRule.Group;
					var overlappingItemCombinationString = CommissionLookups.ShouldShowServicesAndSubModules ?
						Res.GetString("4f231b20-31b5-457e-95ca-1f5539bd9d4d", "Product, Service and Sub-Module") :
						Res.GetString("6053AD8A-60CC-4BD8-A492-067613DA97E1", "Product, Mode, Origin and Destination");

					if (team != null)
					{
						propertyInfo.AddError(ResString.GetMultilingualString("9665492d-a284-4558-8f9a-e5ad82a4f2ac", "Another rule with the same {0} has overlapping Start/End Dates for {1}.", overlappingItemCombinationString, team.HumanReadableName));
					}
					else
					{
						propertyInfo.AddError(ResString.GetMultilingualString("cb2ce6fd-b5b6-4cdc-a8cb-63a2139210be", "Another rule with the same {0} has overlapping Start/End Dates.", overlappingItemCombinationString));
					}
				}
			}
		}

		#endregion

		#region Row Notifications

		protected void CheckHasAtLeastOneRate()
		{
			string errorMessage = OrgCommissionAgreementRecipientValidation.EnterAtLeastOneRateErrorMessage;

			Parent.RemoveRowError(errorMessage);
			if (Parent.Rates.Count == 0)
			{
				Parent.AddRowError(errorMessage);
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckHasAtLeastOneRate();
		}

		#endregion
	}
}
