using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public sealed class AccTaxRateRegistry : RegistryItemSet
	{
		#region Construction

		public static AccTaxRateRegistry Instance
		{
			get { return instance ?? (instance = new AccTaxRateRegistry()); }
		}

		[ThreadStatic]
		static AccTaxRateRegistry instance;

		AccTaxRateRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem IsUKCompaniesVATRateUpdated20100101
		{
			get
			{
				return GetItem("IsUKCompaniesVATRateUpdated20100101",
								() => new BooleanRegistryItem(
										"IsUKCompaniesVATRateUpdated20100101",
										null,
										null,
										null,
										RegistryStorageFlags.Company,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsUKCompaniesVATRateUpdated20110104
		{
			get
			{
				return GetItem("IsUKCompaniesVATRateUpdated20110104",
								() => new BooleanRegistryItem(
										"IsUKCompaniesVATRateUpdated20110104",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsFinlandVATRateUpdated20100701
		{
			get
			{
				return GetItem("IsFinlandVATRateUpdated20100701",
								() => new BooleanRegistryItem(
										"IsFinlandVATRateUpdated20100701",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsNewZealandGSTRateUpdated20101001
		{
			get
			{
				return GetItem("IsNewZealandGSTRateUpdated20101001",
								() => new BooleanRegistryItem(
										"IsNewZealandGSTRateUpdated20101001",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsCanadianCompanyGSTANDQSTRateUpdated20110101
		{
			get
			{
				return GetItem("IsCanadianCompanyGSTANDQSTRateUpdated20110101",
								() => new BooleanRegistryItem(
										"IsCanadianCompanyGSTANDQSTRateUpdated20110101",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsCanadianCompanyGSTANDQSTRateUpdated20120101
		{
			get
			{
				return GetItem("IsCanadianCompanyGSTANDQSTRateUpdated20120101",
								() => new BooleanRegistryItem(
										"IsCanadianCompanyGSTANDQSTRateUpdated20120101",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsCanadianCompanyGSTANDQSTRateUpdated20130101
		{
			get
			{
				return GetItem("IsCanadianCompanyGSTANDQSTRateUpdated20130101",
								() => new BooleanRegistryItem(
										"IsCanadianCompanyGSTANDQSTRateUpdated20130101",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsIndiaCompanySERANDEDURateUpdated20120401
		{
			get
			{
				return GetItem("IsIndiaCompanySERANDEDURateUpdated20120401",
								() => new BooleanRegistryItem(
										"IsIndiaCompanySERANDEDURateUpdated20120401",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsFijiCompaniesVATRateUpdated20110101
		{
			get
			{
				return GetItem("IsFijiCompaniesVATRateUpdated20110101",
								() => new BooleanRegistryItem(
										"IsFijiCompaniesVATRateUpdated20110101",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsMalaysiaCompaniesGSTRateUpdated20110101
		{
			get
			{
				return GetItem("IsMalaysiaCompaniesGSTRateUpdated20110101",
								() => new BooleanRegistryItem(
										"IsMalaysiaCompaniesGSTRateUpdated20110101",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsMaldivesCompaniesGSTRateUpdated20120101
		{
			get
			{
				return GetItem("IsMaldivesCompaniesGSTRateUpdated20120101",
								() => new BooleanRegistryItem(
										"IsMaldivesCompaniesGSTRateUpdated20120101",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsSpainCompaniesMidIVARateUpdated20120901
		{
			get
			{
				return GetItem("IsSpainCompaniesMidIVARateUpdated20120901",
								() => new BooleanRegistryItem(
										"IsSpainCompaniesMidIVARateUpdated20120901",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsIsraelCompaniesVATRateUpdated20120901
		{
			get
			{
				return GetItem("IsIsraelCompaniesVATRateUpdated20120901",
								() => new BooleanRegistryItem(
										"IsIsraelCompaniesVATRateUpdated20120901",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}

		public BooleanRegistryItem IsNetherlandsCompaniesMidIVARateUpdated20121001
		{
			get
			{
				return GetItem("IsNetherlandsCompaniesMidIVARateUpdated20121001",
								() => new BooleanRegistryItem(
										"IsNetherlandsCompaniesMidIVARateUpdated20121001",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										false));
			}
		}
	}
}
