using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartTypeDecider : CountrySpecificTypeDecider
	{
		public static Type GetOrgSupplierPartType(ZString countryCode) => decider.GetTypeForCountryCode(countryCode);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
				new CountrySpecificType(Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Belgium, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.BE.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Brazil, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.China, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Denmark, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.DK.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Finland, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.FI.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.FR.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Germany, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.DE.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Ireland, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.IE.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Israel, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.IL.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.India, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.IN.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Italy, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Japan, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.JP.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Malaysia, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.MY.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Mexico, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.MX.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Netherlands, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.NL.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.NewZealand, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Norway, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.NO.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Poland, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.PL.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Singapore, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.SG.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.ES.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Sweden, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.SE.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Switzerland, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.TR.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.UnitedArabEmirates, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.AE.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.UnitedKingdom, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.GB.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgSupplierPart>(); }),

				#if DEBUG
				new CountrySpecificType(Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs._CustomsTemplate_.IOrgSupplierPart>(); }),
				new CountrySpecificType(Constants.CountryCodes._EUTemplateCountryName_, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs._EUCustomsTemplate_.IOrgSupplierPart>(); })
				#endif
		};

		protected override Type DefaultTypeForUnsupportedCountry => ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IGlobalOrgSupplierPart>();

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgSupplierPart>();

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Enterprise.Integration.Customs.AsycudaCustoms.IOrgSupplierPart>);
			}
		}

		static readonly OrgSupplierPartTypeDecider decider = new OrgSupplierPartTypeDecider();
	}
}
