using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ImportJobDocAddressValidation : JobDocAddressValidation
	{
		public ImportJobDocAddressValidation(AutoJobDocAddress parent, JobDeclaration declaration)
			: base(parent)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (declaration.IsWarehouseEntryType || declaration.IsACECargoRelease)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.CustomsWarehouseAddress:
						ValidateWarehouseAddress();
						break;
				}
			}
		}

		void ValidateWarehouseAddress()
		{
			if (Parent.OrganisationPK.IsEmpty)
			{
				if (declaration.IsACECargoRelease)
				{
					if (EntryTypeList.IsWarehouseType(declaration.US_EntryType))
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, "Bonded Warehouse");
					}
					else if (declaration.IsFTZWeeklyEstimateIntegrationEnabled)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, "FTZ");
					}
				}
				else if (!declaration.SupportsBondedWarehousing)
				{
					Parent.OrganisationPKInfo.AddWarning(NoBondedWarehouse(declaration.TermNameForBondedWarehouse));
				}
			}
			else
			{
				var warehouseAddress = Parent.Address;
				if (warehouseAddress != null)
				{
					var warehouseFirmsCode = warehouseAddress.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);
					if (warehouseFirmsCode == null)
					{
						Parent.OrganisationPKInfo.AddMessageError(NoFIRMSCodeForWarehouse(declaration.TermNameForBondedWarehouse));
					}
					else if (!declaration.IsWarehouseFirmsCodeUniqueForWeeklyEstimateIntegration)
					{
						Parent.OrganisationPKInfo.AddMessageError(FIRMSCodeShouldBeUnique);
					}
				}
			}
		}

		internal static string NoBondedWarehouse(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "No {0} has been added for this Warehouse entry.", term);
		}
		internal static string NoFIRMSCodeForWarehouse(string term)
		{
			return string.Format(CultureInfo.InvariantCulture, "No FIRMS Code has been entered for this {0}. \r\nPress F3 on this organization to enter the FIRMS code under Details > Config > Registration Numbers / Codes, (using type 'FRM')", term);
		}
		internal const string FIRMSCodeShouldBeUnique = "The FIRMS Code entered on the FTZ should not be entered on other active addresses.";
	}
}
