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
	public class BaseGroupInvoiceChargeTypeDecider : CountrySpecificTypeDecider
	{
		public BaseGroupInvoiceChargeTypeDecider()
		{
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetBaseGroupInvoiceChargeCountryCode(row, factory));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, () => ObjectFactory.GetType<Integration.Customs.AU.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, () => ObjectFactory.GetType<Integration.Customs.BR.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, () => ObjectFactory.GetType<Integration.Customs.CA.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.China, () =>  ObjectFactory.GetType<Integration.Customs.CN.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.France, () => ObjectFactory.GetType<Integration.Customs.FR.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, () => ObjectFactory.GetType<Integration.Customs.DE.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, () => ObjectFactory.GetType<Integration.Customs.IE.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, () =>  ObjectFactory.GetType<Integration.Customs.IL.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.India, () => ObjectFactory.GetType<Integration.Customs.IN.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, () =>  ObjectFactory.GetType<Integration.Customs.JP.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, () =>  ObjectFactory.GetType<Integration.Customs.KR.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, () =>  ObjectFactory.GetType<Integration.Customs.MY.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, () =>  ObjectFactory.GetType<Integration.Customs.MX.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, () => ObjectFactory.GetType<Integration.Customs.NZ.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, () =>  ObjectFactory.GetType<Integration.Customs.NO.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, () =>  ObjectFactory.GetType<Integration.Customs.PL.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, () =>  ObjectFactory.GetType<Integration.Customs.SG.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, () =>  ObjectFactory.GetType<Integration.Customs.ES.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, () =>  ObjectFactory.GetType<Integration.Customs.CH.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, () =>  ObjectFactory.GetType<Integration.Customs.TW.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, () =>  ObjectFactory.GetType<Integration.Customs.TR.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, () =>  ObjectFactory.GetType<Integration.Customs.US.IGroupInvoiceCharge>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, () =>  ObjectFactory.GetType<Integration.Customs.GB.IGroupInvoiceCharge>()),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, () => ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IGroupInvoiceCharge>())
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IGroupInvoiceCharge>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseGroupInvoiceCharge);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IGroupInvoiceCharge>);
			}
		}

		ZString GetBaseGroupInvoiceChargeCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var groupHeaderPK = (row != null) ? new ZGuid(row[JobComInvHeaderChargeSchema.J7_ParentID.Name]) : ZGuid.Invalid;
			var groupHeader = (groupHeaderPK.IsValid) ? factory.Load<BaseJobComInvoiceGroupHeader>(groupHeaderPK) : null;
			return (groupHeader != null && groupHeader.JobDeclaration != null) ? groupHeader.JobDeclaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
