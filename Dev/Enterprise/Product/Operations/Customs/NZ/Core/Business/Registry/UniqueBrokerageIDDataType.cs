using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Customs.NZ.Business.Res;

namespace Enterprise.Customs.NZ.Registry
{
	public class UniqueBrokerageIDDataType : StringRegistryDataType
	{
		public UniqueBrokerageIDDataType() : base()
		{
		}

		public UniqueBrokerageIDDataType(int minLength, int maxLength)
			: base(minLength, maxLength)
		{
		}

		public UniqueBrokerageIDDataType(CharacterCase characterCase)
			: base(characterCase)
		{
		}

		public UniqueBrokerageIDDataType(CharacterCase characterCase, int minLength, int maxLength)
			: base(characterCase, minLength, maxLength)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var registryItemInternals = (IRegistryItemInternals)registryItem;
			var companies = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.NewZealand);

			foreach (var company in companies)
			{
				var currentCompanyPk = company.PK.ToGuid();

				if (currentCompanyPk != companyPK)
				{
					var comparedValue = registryItemInternals.GetCurrentValueFromProposedValueAccessor(currentCompanyPk, Guid.Empty, Guid.Empty);
					if (comparedValue != null && proposedValue.Equals(comparedValue))
					{
						var errorMessage = Res.GetString("4f8ab40f-e446-4056-b162-0fdefea8369a", "Company: {0}, has entered the same Brokerage ID.", company.HumanReadableNameForRegistry);
						throw new RegistryValidationException(errorMessage);
					}
				}
			}

			ValidateForBrokerFormatting(proposedValue);
		}

		void ValidateForBrokerFormatting(ZString proposedValue)
		{
			if (!proposedValue.IsLettersAndNumbersOnlyOrEmpty)
			{
				var errorMessage = Res.GetString("B1BABC1C-6FF3-4DC0-8001-7A810DEB9E98", "The Brokerage ID. must only contain alphanumeric values.");
				throw new RegistryValidationException(errorMessage);
			}
		}
	}
}
