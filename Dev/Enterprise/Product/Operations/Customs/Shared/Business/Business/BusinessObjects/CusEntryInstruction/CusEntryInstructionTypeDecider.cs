using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusEntryInstructionTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.China, ObjectFactory.GetType<Integration.Customs.CN.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Taiwan, ObjectFactory.GetType<Integration.Customs.TW.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Japan, ObjectFactory.GetType<Integration.Customs.JP.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Sweden, ObjectFactory.GetType<Integration.Customs.SE.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Norway, ObjectFactory.GetType<Integration.Customs.NO.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Mexico, ObjectFactory.GetType<Integration.Customs.MX.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.Israel, ObjectFactory.GetType<Integration.Customs.IL.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.India, ObjectFactory.GetType<Integration.Customs.IN.ICusEntryInstruction>),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.ICusEntryInstruction>),
			};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusEntryInstruction>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusEntryInstruction);

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var declarationCountryCode = GetDeclarationCountryCode(row, factory);
			Type type = null;
			if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(declarationCountryCode))
			{
				var decider = (CusEntryInstructionTypeDecider)TypeDecider.GetTypeDeciderFromType(ObjectFactory.GetType<Integration.Customs.EU.ICusEntryInstruction>());
				type = decider.GetTypeForCountryCode(declarationCountryCode);
			}
			return type ?? GetTypeForCountryCode(declarationCountryCode);
		}

		protected ZString GetDeclarationCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var declarationPK = (row != null) ? new ZGuid(row[CusEntryInstruction.Schema.CEI_JE]) : ZGuid.Invalid;
			var declaration = declarationPK.IsValid ? factory.Load<BaseJobDeclaration>(declarationPK) : null;
			return (declaration != null) ? declaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusEntryInstruction>);
			}
		}
	}
}
