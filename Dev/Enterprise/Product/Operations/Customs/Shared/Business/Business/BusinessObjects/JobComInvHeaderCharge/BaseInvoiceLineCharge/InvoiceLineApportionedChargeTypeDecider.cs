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
	public class InvoiceLineApportionedChargeTypeDecider : CountrySpecificTypeDecider
	{
		public InvoiceLineApportionedChargeTypeDecider()
		{
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetInvoiceLineApportionedChargeCountryCode(row, factory));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, () => ObjectFactory.GetType<Integration.Customs.AU.IInvoiceLineApportionedCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, () => ObjectFactory.GetType<Integration.Customs.BR.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, () => ObjectFactory.GetType<Integration.Customs.CA.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.China, () => ObjectFactory.GetType<Integration.Customs.CN.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, () => ObjectFactory.GetType<Integration.Customs.DE.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, () => ObjectFactory.GetType<Integration.Customs.IE.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel,  () =>  ObjectFactory.GetType<Integration.Customs.IL.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.India,  () =>  ObjectFactory.GetType<Integration.Customs.IN.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, () => ObjectFactory.GetType<Integration.Customs.TR.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, () => ObjectFactory.GetType<Integration.Customs.JP.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, () => ObjectFactory.GetType<Integration.Customs.MY.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, () => ObjectFactory.GetType<Integration.Customs.MX.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, () => ObjectFactory.GetType<Integration.Customs.NO.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, () => ObjectFactory.GetType<Integration.Customs.SG.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, () => ObjectFactory.GetType<Integration.Customs.ES.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, () => ObjectFactory.GetType<Integration.Customs.CH.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, () => ObjectFactory.GetType<Integration.Customs.GB.IInvoiceLineApportionCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, () => ObjectFactory.GetType<Integration.Customs.US.IInvoiceLineApportionCharge>()),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IInvoiceLineApportionCharge>(); })
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IInvoiceLineApportionCharge>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseInvoiceLineApportionedCharge);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IInvoiceLineApportionCharge>);
			}
		}

		ZString GetInvoiceLineApportionedChargeCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var parentPK = (row != null) ? new ZGuid(row[JobComInvHeaderChargeSchema.J7_ParentID.Name]) : ZGuid.Invalid;
			var invoiceLine = (parentPK.IsValid) ? factory.Load<BaseJobComInvoiceLine>(parentPK) : null;
			return (invoiceLine != null && invoiceLine.Declaration != null) ? invoiceLine.Declaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
