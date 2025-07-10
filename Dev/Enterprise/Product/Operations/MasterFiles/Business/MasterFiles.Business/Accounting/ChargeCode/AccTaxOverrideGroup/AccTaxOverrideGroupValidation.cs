//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxOverrideGroupValidation
//
//    This class should be used for overriding validation in AutoAccTaxOverrideGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.Application;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business.Accounting;
	using Enterprise.ZArchitecture.Schema;

	public class AccTaxOverrideGroupValidation : AutoAccTaxOverrideGroupValidation
	{
		public AccTaxOverrideGroupValidation(AutoAccTaxOverrideGroup parent) : base(parent)
		{
		}

		protected new AccTaxOverrideGroup Parent
		{
			get { return (AccTaxOverrideGroup)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateDuplicateTaxOverrides();
		}

		protected override void CheckAX_Code()
		{
			base.CheckAX_Code();
			MandatoryValidation.CheckEntered(Parent.AX_CodeInfo);

			if (!Parent.AX_CodeInfo.HasErrors())
			{
				ZQuery filter = new ZQuery(AccTaxOverrideGroupSchema.AX_Code, Parent.AX_Code);
				filter.AddToFilter(AccTaxOverrideGroupSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter = DependencyFactory.GetTaxFrameworkConfigurationHelper().GetCompanyTaxOverrideGroup(filter, GlbCompany.CurrentCompany);
				AccTaxOverrideGroup taxOverrideGroup = Parent.Factory.LoadTop1<AccTaxOverrideGroup>(filter);
				if (taxOverrideGroup != null)
				{
					Parent.AX_CodeInfo.AddError(Res.GetString("A9A11239-7785-4b36-B082-D2EEB4F1E786", "Tax Override Group with the same code already exists."));
				}
			}
		}

		protected override void CheckAX_Description()
		{
			base.CheckAX_Description();
			MandatoryValidation.CheckEntered(Parent.AX_DescriptionInfo);
		}

		public void ValidateDuplicateTaxOverrides()
		{
			bool duplicateFound = false;
			var errorMessage = Res.GetString("5F6947B7-4398-4a10-BE47-6E143809722E", "You cannot have identical tax overrides.");

			foreach (AccChargeTaxOverride @override in Parent.TaxOverrides)
			{
				foreach (AccChargeTaxOverride override2 in Parent.TaxOverrides)
				{
					override2.RemoveRowError(errorMessage);
					if (@override.PK != override2.PK && override2.IsDuplicate(@override))
					{
						override2.AddRowError(errorMessage);
						duplicateFound = true;
						break;
					}
				}
				if (duplicateFound)
				{
					break;
				}
			}

			if (!duplicateFound)
			{
				errorMessage = Res.GetString("5b5bffef-5b87-4c39-a942-67a76eb1391d", "You cannot have identical tax overrides with tax override group.");
				foreach (var chargeCode in Parent.ChargeCodes)
				{
					chargeCode.RemoveRowError(errorMessage);
					foreach (AccChargeTaxOverride override1 in Parent.TaxOverrides)
					{
						foreach (AccChargeTaxOverride override2 in chargeCode.TaxOverrides)
						{
							if (override1.PK != override2.PK && override2.IsDuplicate(override1))
							{
								chargeCode.AddRowError(errorMessage);
								duplicateFound = true;
								break;
							}
						}
						if (duplicateFound)
						{
							break;
						}
					}
				}
			}
		}

		IAccountingMasterFilesDependencyFactory DependencyFactory => dependencyFactory ?? (dependencyFactory = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>());
		IAccountingMasterFilesDependencyFactory dependencyFactory;
	}
}
