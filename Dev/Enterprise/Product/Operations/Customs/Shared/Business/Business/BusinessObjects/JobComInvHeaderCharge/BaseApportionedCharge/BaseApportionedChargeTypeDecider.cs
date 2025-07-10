using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BaseApportionedChargeTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetBaseApportionedChargeCountryCode(row, factory));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, () => ObjectFactory.GetType<Integration.Customs.AU.IInvoiceApportionedCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, () =>  ObjectFactory.GetType<Integration.Customs.BR.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, () =>  ObjectFactory.GetType<Integration.Customs.CA.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.China, () =>  ObjectFactory.GetType<Integration.Customs.CN.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, () => ObjectFactory.GetType<Integration.Customs.DE.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, () => ObjectFactory.GetType<Integration.Customs.IE.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, () => ObjectFactory.GetType<Integration.Customs.IL.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.India, () => ObjectFactory.GetType<Integration.Customs.IN.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, () =>  ObjectFactory.GetType<Integration.Customs.JP.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, () =>  ObjectFactory.GetType<Integration.Customs.MY.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, () =>  ObjectFactory.GetType<Integration.Customs.MX.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, () =>  ObjectFactory.GetType<Integration.Customs.NO.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, () =>  ObjectFactory.GetType<Integration.Customs.SG.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, () =>  ObjectFactory.GetType<Integration.Customs.ZA.IApportionedCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, () =>  ObjectFactory.GetType<Integration.Customs.ES.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, () =>  ObjectFactory.GetType<Integration.Customs.CH.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, () =>  ObjectFactory.GetType<Integration.Customs.TW.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, () =>  ObjectFactory.GetType<Integration.Customs.AE.IApportionedCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, () =>  ObjectFactory.GetType<Integration.Customs.GB.IInvoiceApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, () =>  ObjectFactory.GetType<Integration.Customs.US.IInvoiceApportionCharge>()),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IInvoiceApportionCharge>(); })
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IInvoiceApportionCharge>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseApportionedCharge);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceApportionCharge>);
			}
		}

		ZString GetBaseApportionedChargeCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var jobComInvoicePK = (row != null) ? new ZGuid(row[JobComInvHeaderChargeSchema.J7_ParentID.Name]) : ZGuid.Invalid;
			var jobComInvHeader = (jobComInvoicePK.IsValid) ? factory.Load<BaseJobComInvoiceHeader>(jobComInvoicePK) : null;
			return (jobComInvHeader != null && jobComInvHeader.JobDeclaration != null) ? jobComInvHeader.JobDeclaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
