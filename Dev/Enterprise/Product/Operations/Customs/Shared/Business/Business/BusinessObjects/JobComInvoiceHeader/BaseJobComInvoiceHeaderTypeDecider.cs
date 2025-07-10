using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class BaseJobComInvoiceHeaderTypeDecider : CountrySpecificTypeDecider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public BaseJobComInvoiceHeaderTypeDecider()
		{
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var declarationPk = new ZGuid(row[BaseJobComInvoiceHeader.Schema.JZ_JE]);
			var declaration = factory.Load<BaseJobDeclaration>(declarationPk);

			if ((declaration as Integration.Customs.EUEMCS.IJobDeclaration) != null)
			{
				return ObjectFactory.GetType<Integration.Customs.EUEMCS.IJobComInvoiceHeader>();
			}
			else
			{
				var countryCode = GetJobComInvoiceHeaderCountryCode(row, declaration, factory);
				return GetTypeForCountryCode(countryCode);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Belgium, delegate { return ObjectFactory.GetType<Integration.Customs.BE.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, delegate { return ObjectFactory.GetType<Integration.Customs.BR.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.China, delegate { return ObjectFactory.GetType<Integration.Customs.CN.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Denmark, delegate { return ObjectFactory.GetType<Integration.Customs.DK.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Finland, delegate { return ObjectFactory.GetType<Integration.Customs.FI.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, delegate { return ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, delegate { return ObjectFactory.GetType<Integration.Customs.IE.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, delegate { return ObjectFactory.GetType<Integration.Customs.IL.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.India, delegate { return ObjectFactory.GetType<Integration.Customs.IN.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Italy, delegate { return ObjectFactory.GetType<Integration.Customs.IT.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, delegate { return ObjectFactory.GetType<Integration.Customs.JP.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, delegate { return ObjectFactory.GetType<Integration.Customs.MY.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, delegate { return ObjectFactory.GetType<Integration.Customs.MX.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, delegate { return ObjectFactory.GetType<Integration.Customs.NL.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, delegate { return ObjectFactory.GetType<Integration.Customs.NZ.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, delegate { return ObjectFactory.GetType<Integration.Customs.NO.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, delegate { return ObjectFactory.GetType<Integration.Customs.PL.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, delegate { return ObjectFactory.GetType<Integration.Customs.SG.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Integration.Customs.ZA.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Integration.Customs.ES.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Sweden, delegate { return ObjectFactory.GetType<Integration.Customs.SE.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, delegate { return ObjectFactory.GetType<Integration.Customs.CH.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.TW.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, delegate { return ObjectFactory.GetType<Integration.Customs.AE.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, delegate { return ObjectFactory.GetType<Integration.Customs.GB.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.IJobComInvoiceHeader>(); }),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IJobComInvoiceHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes._EUTemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._EUCustomsTemplate_.IJobComInvoiceHeader>(); }),
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IJobComInvoiceHeader>();

		protected override Type DefaultTypeForUnsupportedCountry
		{
			get
			{
#if DEBUG
				if (defaultTypeForUnsupportedCountryForTesting != null)
				{
					return defaultTypeForUnsupportedCountryForTesting();
				}
#endif
				return typeof(BaseJobComInvoiceHeader);
			}
		}

#if DEBUG
		public static IDisposable SetupDefaultTypeForUnsupportedCountryForTesting(Type defaultType)
		{
			return new DisposableAction(() =>
			{
				defaultTypeForUnsupportedCountryForTesting = () => defaultType;
			},
			() =>
			{
				defaultTypeForUnsupportedCountryForTesting = null;
			});
		}
		[ThreadStatic]
		static Func<Type> defaultTypeForUnsupportedCountryForTesting;
#endif

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobComInvoiceHeader>);
			}
		}

		ZString GetJobComInvoiceHeaderCountryCode(System.Data.DataRow row, BaseJobDeclaration declaration, BusinessObjectFactory factory)
		{
			var countryCode = ZString.Empty;

			if (declaration != null)
			{
				countryCode = declaration.CountryCode;
			}
			else
			{
				var branchPK = new ZGuid(row[BaseJobComInvoiceHeader.Schema.JZ_GB]);
				var branch = factory.Load<GlbBranch>(branchPK);
				if (branch != null)
				{
					countryCode = branch.Company.GC_RN_NKCountryCode;
				}
			}
			return countryCode;
		}

		public override Type GetTypeForBinding()
		{
			if (suspendCountrySpecificTypesForBinding > 0)
			{
				return null;
			}

			return base.GetTypeForBinding();
		}

		public static IDisposable TemporarilySuspendCountrySpecificTypesForBinding()
		{
			suspendCountrySpecificTypesForBinding++;
			return new DisposableAction(delegate
			{ suspendCountrySpecificTypesForBinding--; });
		}

		[ThreadStatic]
		static int suspendCountrySpecificTypesForBinding;
	}
}
