using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class BaseJobComInvoiceGroupHeaderTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var declarationPk = new ZGuid(row[BaseJobComInvoiceHeader.Schema.JZ_JE]);
			var declaration = factory.Load<BaseJobDeclaration>(declarationPk) as Integration.Customs.EUEMCS.IJobDeclaration;

			if (declaration != null)
			{
				return ObjectFactory.GetType<Integration.Customs.EUEMCS.IJobComInvoiceGroupHeader>();
			}
			else
			{
				var countryCodeForBizO = GetJobComInvoiceGroupCountryCode(row, factory);
				return GetTypeForCountryCode(countryCodeForBizO);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Belgium, delegate { return ObjectFactory.GetType<Integration.Customs.BE.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, delegate { return ObjectFactory.GetType<Integration.Customs.BR.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.China, delegate { return ObjectFactory.GetType<Integration.Customs.CN.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Denmark, delegate { return ObjectFactory.GetType<Integration.Customs.DK.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Finland, delegate { return ObjectFactory.GetType<Integration.Customs.FI.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, delegate { return ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, delegate { return ObjectFactory.GetType<Integration.Customs.IE.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, delegate { return ObjectFactory.GetType<Integration.Customs.IL.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.India, delegate { return ObjectFactory.GetType<Integration.Customs.IN.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Italy, delegate { return ObjectFactory.GetType<Integration.Customs.IT.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, delegate { return ObjectFactory.GetType<Integration.Customs.JP.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, delegate { return ObjectFactory.GetType<Integration.Customs.MY.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, delegate { return ObjectFactory.GetType<Integration.Customs.MX.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, delegate { return ObjectFactory.GetType<Integration.Customs.NL.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, delegate { return ObjectFactory.GetType<Integration.Customs.NZ.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, delegate { return ObjectFactory.GetType<Integration.Customs.NO.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, delegate { return ObjectFactory.GetType<Integration.Customs.PL.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, delegate { return ObjectFactory.GetType<Integration.Customs.SG.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Integration.Customs.ES.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Integration.Customs.ZA.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Sweden, delegate { return ObjectFactory.GetType<Integration.Customs.SE.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, delegate { return ObjectFactory.GetType<Integration.Customs.CH.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.TW.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, delegate { return ObjectFactory.GetType<Integration.Customs.AE.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, delegate { return ObjectFactory.GetType<Integration.Customs.GB.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.IJobComInvoiceGroupHeader>(); }),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IJobComInvoiceGroupHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes._EUTemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._EUCustomsTemplate_.IJobComInvoiceGroupHeader>(); }),
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IJobComInvoiceGroupHeader>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseJobComInvoiceGroupHeader);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceGroupHeader>);
			}
		}

		ZString GetJobComInvoiceGroupCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var declarationPK = row != null ? new ZGuid(row[BaseJobComInvoiceGroupHeader.Schema.JZ_JE]) : ZGuid.Invalid;
			var jobDeclaration = declarationPK.IsValid ? factory.Load<BaseJobDeclaration>(declarationPK) : null;
			return jobDeclaration != null ? jobDeclaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
