using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(CusRefTariffVersionSchema.Constants.CRT_Version)]
	public class CusRefTariffVersion : AutoCusRefTariffVersion, Integration.Customs.ICusRefTariffVersion
	{
		public CusRefTariffVersion(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.Business.CusRefTariffVersion|CRT_Version", Caption = "Code")]
		[ReadOnlyMember(nameof(CRT_Version_ReadOnly))]
		public override ZString CRT_Version { get => base.CRT_Version; set => base.CRT_Version = value; }

		bool CRT_Version_ReadOnly => IsInDatabase;

		[ResourceStringData("Enterprise.Customs.Business.CusRefTariffVersion|CRT_Description", Caption = "Description")]
		public override ZString CRT_Description { get => base.CRT_Description; set => base.CRT_Description = value; }

		[ResourceStringData("Enterprise.Customs.Business.CusRefTariffVersion|CRT_EffectiveDate", Caption = "Effective Date")]
		public override ZDate CRT_EffectiveDate { get => base.CRT_EffectiveDate; set => base.CRT_EffectiveDate = value; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.Business.CusRefTariffVersion|CRT_RN_NKCountryCode", Caption = "Country/Region")]
		public override ZString CRT_RN_NKCountryCode { get => base.CRT_RN_NKCountryCode; set => base.CRT_RN_NKCountryCode = value; }

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRT_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoCusRefTariffVersion.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public static CusRefTariffVersion Load(BusinessObjectFactory factory, ZString countryCode, ZDateTime effectiveDate)
			{
				CusRefTariffVersion result = null;
				if (!countryCode.IsEmpty && effectiveDate.IsValid)
				{
					var query = new ZQuery();
					query.AddToFilter(CusRefTariffVersionSchema.CRT_RN_NKCountryCode, countryCode);
					query.AddToFilter(CusRefTariffVersionSchema.CRT_EffectiveDate, SQLComparisonOperator.LessThanOrEqualTo, effectiveDate);
					query.OrderBy = Schema.CRT_EffectiveDate + OrderByClause.Descending;
					result = factory.LoadTop1<CusRefTariffVersion>(query);
				}
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CusRefTariffVersion);
		}
	}
}
