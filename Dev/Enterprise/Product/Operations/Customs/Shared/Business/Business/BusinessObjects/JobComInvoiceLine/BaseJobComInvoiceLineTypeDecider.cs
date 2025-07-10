using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class BaseJobComInvoiceLineTypeDecider : CountrySpecificTypeDecider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Belgium, delegate { return ObjectFactory.GetType<Integration.Customs.BE.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, delegate { return ObjectFactory.GetType<Integration.Customs.BR.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.China, delegate { return ObjectFactory.GetType<Integration.Customs.CN.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Denmark, delegate { return ObjectFactory.GetType<Integration.Customs.DK.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Finland, delegate { return ObjectFactory.GetType<Integration.Customs.FI.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, delegate { return ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, delegate { return ObjectFactory.GetType<Integration.Customs.IE.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, delegate { return ObjectFactory.GetType<Integration.Customs.IL.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.India, delegate { return ObjectFactory.GetType<Integration.Customs.IN.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Italy, delegate { return ObjectFactory.GetType<Integration.Customs.IT.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, delegate { return ObjectFactory.GetType<Integration.Customs.JP.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, delegate { return ObjectFactory.GetType<Integration.Customs.MY.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, delegate { return ObjectFactory.GetType<Integration.Customs.MX.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, delegate { return ObjectFactory.GetType<Integration.Customs.NL.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, delegate { return ObjectFactory.GetType<Integration.Customs.NZ.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, delegate { return ObjectFactory.GetType<Integration.Customs.NO.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, delegate { return ObjectFactory.GetType<Integration.Customs.PL.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, delegate { return ObjectFactory.GetType<Integration.Customs.SG.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Integration.Customs.ZA.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Integration.Customs.ES.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Sweden, delegate { return ObjectFactory.GetType<Integration.Customs.SE.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, delegate { return ObjectFactory.GetType<Integration.Customs.CH.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.TW.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, delegate { return ObjectFactory.GetType<Integration.Customs.AE.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, delegate { return ObjectFactory.GetType<Integration.Customs.GB.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.IJobComInvoiceLine>(); }),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IJobComInvoiceLine>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes._EUTemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._EUCustomsTemplate_.IJobComInvoiceLine>(); }),
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IJobComInvoiceLine>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseJobComInvoiceLine);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceLine>);
			}
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var invoiceHeader = GetJobComInvoiceHeaderForInvoiceLine(row, factory);
			Type type = null;
			if (invoiceHeader is IInvoiceLineTypeSupporter supporter)
			{
				type = supporter.InvoiceLineType;
			}
			if (type == null)
			{
				var headerCountryCode = GetJobComInvoiceCountryCode(invoiceHeader);
				type = GetTypeForCountryCode(headerCountryCode);
			}
			return type;
		}

		BaseJobComInvoiceHeader GetJobComInvoiceHeaderForInvoiceLine(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var jobComInvoiceHeaderPK = row != null ? new ZGuid(row[BaseJobComInvoiceLine.Schema.JI_JZ]) : ZGuid.Invalid;
			return jobComInvoiceHeaderPK.IsValid ? factory.Load<BaseJobComInvoiceHeader>(jobComInvoiceHeaderPK) : null;
		}

		ZString GetJobComInvoiceCountryCode(BaseJobComInvoiceHeader invoiceHeader)
		{
			var countryCode = invoiceHeader?.CountryCode ?? ZString.Empty;
			return countryCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : countryCode;
		}
	}
}
