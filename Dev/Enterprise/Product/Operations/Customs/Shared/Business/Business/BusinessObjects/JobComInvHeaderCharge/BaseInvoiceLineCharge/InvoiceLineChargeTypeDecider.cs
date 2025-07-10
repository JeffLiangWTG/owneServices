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
	public class InvoiceLineChargeTypeDecider : CountrySpecificTypeDecider
	{
		public InvoiceLineChargeTypeDecider()
		{
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetBaseInvoiceLineChargeCountryCode(row, factory));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, () => ObjectFactory.GetType<Integration.Customs.AU.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, () =>  ObjectFactory.GetType<Integration.Customs.BR.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, () =>  ObjectFactory.GetType<Integration.Customs.CA.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.China, () =>  ObjectFactory.GetType<Integration.Customs.CN.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.France, () =>  ObjectFactory.GetType<Integration.Customs.FR.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, () => ObjectFactory.GetType<Integration.Customs.DE.IInvoiceLineCharge>() ),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, () => ObjectFactory.GetType<Integration.Customs.IE.IInvoiceLineCharge>() ),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, () => ObjectFactory.GetType<Integration.Customs.IL.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.India, () => ObjectFactory.GetType<Integration.Customs.IN.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, () =>  ObjectFactory.GetType<Integration.Customs.JP.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, () =>  ObjectFactory.GetType<Integration.Customs.KR.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, () =>  ObjectFactory.GetType<Integration.Customs.MY.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, () =>  ObjectFactory.GetType<Integration.Customs.MX.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, () =>  ObjectFactory.GetType<Integration.Customs.NZ.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, () =>  ObjectFactory.GetType<Integration.Customs.NO.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, () =>  ObjectFactory.GetType<Integration.Customs.SG.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, () =>  ObjectFactory.GetType<Integration.Customs.ZA.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, () =>  ObjectFactory.GetType<Integration.Customs.ES.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, () => ObjectFactory.GetType<Integration.Customs.TR.IInvoiceLineCharge>() ),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, () =>  ObjectFactory.GetType<Integration.Customs.CH.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, () =>  ObjectFactory.GetType<Integration.Customs.GB.IInvoiceLineCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, () =>  ObjectFactory.GetType<Integration.Customs.US.IInvoiceLineCharge>()),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IInvoiceLineCharge>(); })
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IInvoiceLineCharge>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseInvoiceLineCharge);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineCharge>);
			}
		}

		ZString GetBaseInvoiceLineChargeCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var parentPK = (row != null) ? new ZGuid(row[JobComInvHeaderChargeSchema.J7_ParentID.Name]) : ZGuid.Invalid;
			var invoiceLine = (parentPK.IsValid) ? factory.Load<BaseJobComInvoiceLine>(parentPK) : null;
			return (invoiceLine != null && invoiceLine.Declaration != null) ? invoiceLine.Declaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
