using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusEntryLineFeeTypeDecider : CountrySpecificTypeDecider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public CusEntryLineFeeTypeDecider()
		{
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetEntryLineFeeCountryCode(row, factory));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Belgium, delegate { return ObjectFactory.GetType<Integration.Customs.BE.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, delegate { return ObjectFactory.GetType<Integration.Customs.BR.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.China, delegate { return ObjectFactory.GetType<Integration.Customs.CN.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, delegate { return ObjectFactory.GetType<Integration.Customs.IE.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, delegate { return ObjectFactory.GetType<Integration.Customs.IL.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.India, delegate { return ObjectFactory.GetType<Integration.Customs.IN.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Italy, delegate { return ObjectFactory.GetType<Integration.Customs.IT.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, delegate { return ObjectFactory.GetType<Integration.Customs.JP.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, delegate { return ObjectFactory.GetType<Integration.Customs.MY.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, delegate { return ObjectFactory.GetType<Integration.Customs.MX.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, delegate { return ObjectFactory.GetType<Integration.Customs.NL.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, delegate { return ObjectFactory.GetType<Integration.Customs.NZ.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, delegate { return ObjectFactory.GetType<Integration.Customs.NO.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, delegate { return ObjectFactory.GetType<Integration.Customs.PL.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, delegate { return ObjectFactory.GetType<Integration.Customs.SG.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Integration.Customs.ES.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, delegate { return ObjectFactory.GetType<Integration.Customs.CH.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.TW.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, delegate { return ObjectFactory.GetType<Integration.Customs.AE.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, delegate { return ObjectFactory.GetType<Integration.Customs.GB.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.ICusEntryLineFee>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.ICusEntryLineFee>(); }),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.ICusEntryLineFee>(); }),
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusEntryLineFee>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusEntryLineFee);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryLineFee>);
			}
		}

		ZString GetEntryLineFeeCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var entryLinePK = (row != null) ? new ZGuid(row[CusEntryLineFee.Schema.CF_CL]) : ZGuid.Invalid;
			var entryLine = (entryLinePK.IsValid) ? factory.Load<CusEntryLine>(entryLinePK) : null;
			return (entryLine != null) ? entryLine.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
