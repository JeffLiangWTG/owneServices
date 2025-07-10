using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusConfiguration : AutoZZRefCusConfiguration
	{
		public ZZRefCusConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static ZZRefCusConfiguration New(GlbCompany company)
		{
			var configuration = company.Factory.New<ZZRefCusConfiguration>();
			configuration.ZZC_GC = company.PK;
			return configuration;
		}

		public static ZZRefCusConfiguration Get(GlbCompany company)
		{
			return company?.Factory.LoadTop1<ZZRefCusConfiguration>(new ZQuery(ZZRefCusConfigurationSchema.ZZC_GC, company.PK));
		}

		[List(nameof(Lookups) + "." + nameof(ZZRefCusConfigurationLookups.VATValueCodeList))]
		public override ZString ZZC_VATValueCode { get => base.ZZC_VATValueCode; set => base.ZZC_VATValueCode = value; }

		[List(nameof(Lookups) + "." + nameof(ZZRefCusConfigurationLookups.CustomsValueCodeList))]
		public override ZString ZZC_CustomsValueCode { get => base.ZZC_CustomsValueCode; set => base.ZZC_CustomsValueCode = value; }

		[List(nameof(Lookups) + "." + nameof(ZZRefCusConfigurationLookups.IsReciprocalExchangeRateList))]
		public override ZString ZZC_IsReciprocalExchangeRate { get => base.ZZC_IsReciprocalExchangeRate; set => base.ZZC_IsReciprocalExchangeRate = value; }

		[List(nameof(Lookups) + "." + nameof(ZZRefCusConfigurationLookups.CustomsValueCodeList))]
		public override ZString ZZC_CustomsValueCodeForExport { get => base.ZZC_CustomsValueCodeForExport; set => base.ZZC_CustomsValueCodeForExport = value; }

		[List(nameof(Lookups) + "." + nameof(ZZRefCusConfigurationLookups.ValuationDateDefaultTypeList))]
		public override ZString ZZC_DefaultImportValuationDate { get => base.ZZC_DefaultImportValuationDate; set => base.ZZC_DefaultImportValuationDate = value; }

		[List(nameof(Lookups) + "." + nameof(ZZRefCusConfigurationLookups.ValuationDateExportDefaultTypeList))]
		public override ZString ZZC_DefaultExportValuationDate { get => base.ZZC_DefaultExportValuationDate; set => base.ZZC_DefaultExportValuationDate = value; }

		public ZString IsReciprocalExchangeRateDescription => Lookups.IsReciprocalExchangeRateList.GetDescriptionFromCode(ZZC_IsReciprocalExchangeRate);
	}
}
