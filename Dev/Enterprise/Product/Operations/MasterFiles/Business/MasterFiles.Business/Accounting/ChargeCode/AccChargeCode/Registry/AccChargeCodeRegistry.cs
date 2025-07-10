using System;
using System.Globalization;
using CargoWise.BrandManager;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public sealed class AccChargeCodeRegistry : RegistryItemSet
	{
		#region Construction

		public static AccChargeCodeRegistry Instance
		{
			get { return instance ?? (instance = new AccChargeCodeRegistry()); }
		}

		[ThreadStatic]
		static AccChargeCodeRegistry instance;

		AccChargeCodeRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Accounting_TaxConfigurations { get { return CombineCategories(Accounting, ResString.GetMultilingualString("1B9133CD-311A-4F50-BF9F-B2075E9DA92B", "Tax Configurations")); } }
		}

		#endregion

		#region Tax Configurations
		#region SuppressResourceStringsCheckRegion

		public EUTaxIDDefaultingRegistryItem EUTaxIDDefaulting
		{
			get
			{
				return GetItem("EUTaxIDDefaulting",
								() => new EUTaxIDDefaultingRegistryItem(
										"EUTaxIDDefaulting",
										Categories.Accounting_TaxConfigurations,
										(NoResString)"EU Tax ID Defaulting",
										(NoResString)string.Format(CultureInfo.InvariantCulture, @"This registry is only referenced by European Union login companies.
It defines a set of fallback rules used to decide which Tax ID will be defaulted against a charge line as it is added to a job or invoice.
When logged into a European Union country AND when posting job related charges, {0} first attempts to use a charge code’s Tax override configuration to decide what Tax ID to default against a job related charge line. When no appropriate override can be identified on a charge code, {0} will fall back to use the rules defined in this registry.
Note: When a rule within these grids has no Tax ID  AND when {0} falls back to use that rule, the main Tax ID recorded against a charge code will be used.", BrandingFactory.Instance.ProductName),
										RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport));
			}
		}

		#endregion
		#endregion
	}
}
