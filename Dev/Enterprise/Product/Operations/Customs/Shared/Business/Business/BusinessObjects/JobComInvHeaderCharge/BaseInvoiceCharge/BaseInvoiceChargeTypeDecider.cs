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
	public class BaseInvoiceChargeTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetBaseInvoiceChargeCountryCode(row, factory));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, () => ObjectFactory.GetType<Integration.Customs.AU.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Belgium, () => ObjectFactory.GetType<Integration.Customs.BE.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, () =>  ObjectFactory.GetType<Integration.Customs.BR.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, () =>  ObjectFactory.GetType<Integration.Customs.CA.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.China, () =>  ObjectFactory.GetType<Integration.Customs.CN.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.France, () =>  ObjectFactory.GetType<Integration.Customs.FR.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, () =>  ObjectFactory.GetType<Integration.Customs.DE.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, () =>  ObjectFactory.GetType<Integration.Customs.IE.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, () =>  ObjectFactory.GetType<Integration.Customs.IL.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.India, () => ObjectFactory.GetType<Integration.Customs.IN.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, () =>  ObjectFactory.GetType<Integration.Customs.JP.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, () =>  ObjectFactory.GetType<Integration.Customs.KR.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, () =>  ObjectFactory.GetType<Integration.Customs.MY.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, () =>  ObjectFactory.GetType<Integration.Customs.MX.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, () =>  ObjectFactory.GetType<Integration.Customs.NZ.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, () =>  ObjectFactory.GetType<Integration.Customs.NO.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, () =>  ObjectFactory.GetType<Integration.Customs.PL.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, () =>  ObjectFactory.GetType<Integration.Customs.SG.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, () =>  ObjectFactory.GetType<Integration.Customs.ES.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, () =>  ObjectFactory.GetType<Integration.Customs.ZA.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, () =>  ObjectFactory.GetType<Integration.Customs.CH.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, () =>  ObjectFactory.GetType<Integration.Customs.TW.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, () =>  ObjectFactory.GetType<Integration.Customs.TR.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, () =>  ObjectFactory.GetType<Integration.Customs.AE.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, () =>  ObjectFactory.GetType<Integration.Customs.GB.IInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, () =>  ObjectFactory.GetType<Integration.Customs.US.IInvoiceCharge>()),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IInvoiceCharge>(); })
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IInvoiceCharge>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseInvoiceCharge);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceCharge>);
			}
		}

		ZString GetBaseInvoiceChargeCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var jobComInvoicePK = (row != null) ? new ZGuid(row[JobComInvHeaderChargeSchema.J7_ParentID.Name]) : ZGuid.Invalid;
			var jobComInvHeader = (jobComInvoicePK.IsValid) ? factory.Load<BaseJobComInvoiceHeader>(jobComInvoicePK) : null;
			return (jobComInvHeader != null && jobComInvHeader.JobDeclaration != null) ? jobComInvHeader.JobDeclaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
