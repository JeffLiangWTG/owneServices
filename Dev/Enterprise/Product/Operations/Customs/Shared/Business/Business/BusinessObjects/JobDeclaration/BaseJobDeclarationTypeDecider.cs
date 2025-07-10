using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class BaseJobDeclarationTypeDecider : CountrySpecificTypeDecider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public BaseJobDeclarationTypeDecider()
		{
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var applicationCode = (row != null) ? new ZString(row[BaseJobDeclaration.Schema.JE_ApplicationCode]) : ZString.Empty;
			var declarationCountryCode = GetCountryCodeForDeclaration(row, factory);

			Type type = null;

			if (applicationCode == EMCSApplicationCode)
			{
				var decider = (CountrySpecificTypeDecider)GetTypeDeciderFromType(ObjectFactory.GetType<Integration.Customs.EUEMCS.IJobDeclaration>());
				type = decider.GetTypeForCountryCode(declarationCountryCode);
			}

			return type ?? GetTypeForCountryCode(declarationCountryCode);
		}

		public const string EMCSApplicationCode = "EMC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Belgium, delegate { return ObjectFactory.GetType<Integration.Customs.BE.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, delegate { return ObjectFactory.GetType<Integration.Customs.BR.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.China, delegate { return ObjectFactory.GetType<Integration.Customs.CN.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Denmark, delegate { return ObjectFactory.GetType<Integration.Customs.DK.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Finland, delegate { return ObjectFactory.GetType<Integration.Customs.FI.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, delegate { return ObjectFactory.GetType<Integration.Customs.DE.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, delegate { return ObjectFactory.GetType<Integration.Customs.IE.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, delegate { return ObjectFactory.GetType<Integration.Customs.IL.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.India, delegate { return ObjectFactory.GetType<Integration.Customs.IN.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Italy, delegate { return ObjectFactory.GetType<Integration.Customs.IT.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, delegate { return ObjectFactory.GetType<Integration.Customs.JP.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Malaysia, delegate { return ObjectFactory.GetType<Integration.Customs.MY.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, delegate { return ObjectFactory.GetType<Integration.Customs.MX.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, delegate { return ObjectFactory.GetType<Integration.Customs.NL.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.NewZealand, delegate { return ObjectFactory.GetType<Integration.Customs.NZ.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, delegate { return ObjectFactory.GetType<Integration.Customs.NO.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, delegate { return ObjectFactory.GetType<Integration.Customs.PL.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Singapore, delegate { return ObjectFactory.GetType<Integration.Customs.SG.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Integration.Customs.ZA.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Integration.Customs.ES.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Sweden, delegate { return ObjectFactory.GetType<Integration.Customs.SE.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, delegate { return ObjectFactory.GetType<Integration.Customs.CH.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Integration.Customs.TW.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, delegate { return ObjectFactory.GetType<Integration.Customs.AE.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, delegate { return ObjectFactory.GetType<Integration.Customs.GB.IJobDeclaration>(); }) ,
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>(); }),

				#if DEBUG
				new CountrySpecificType(Core.Constants.CountryCodes._TemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IJobDeclaration>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes._EUTemplateCountryName_, delegate { return ObjectFactory.GetType<Integration.Customs._EUCustomsTemplate_.IJobDeclaration>(); }),
				#endif
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.IJobDeclaration>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseJobDeclaration);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.IJobDeclaration>);
			}
		}

		static ZString GetCountryCodeForDeclaration(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var branchPK = row != null ? new ZGuid(row[BaseJobDeclaration.Schema.JE_GB]) : ZGuid.Invalid;
			var decBranch = branchPK.IsValid ? factory.Load<GlbBranch>(branchPK) : null;
			return decBranch?.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
