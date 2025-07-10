using System.Collections;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public class GuaranteeCountrySpecificInstruction : SharedCusPermitCountrySpecificInstruction, IGuaranteeCountrySpecificInstruction
	{
		public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static GuaranteeCountrySpecificInstruction GetByCountryCode(BusinessObjectFactory factory, string countryCode)
		{
			return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "GuaranteeCountrySpecificInstruction_{0}", countryCode), () =>
			{
				GuaranteeCountrySpecificInstruction result = null;
				var types = ObjectFactory.Get<Hashtable>("GuaranteeCountrySpecificInstructions");
				if (!string.IsNullOrEmpty(countryCode))
				{
					countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
					var objectHandle = (ObjectHandle)types[countryCode];
					result = (GuaranteeCountrySpecificInstruction)objectHandle?.GetObject(factory);
				}

				if (result == null && ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryCode))
				{
					var objectHandle = (ObjectHandle)types[Core.Constants.CountryCodes.EuropeanUnion];
					result = (GuaranteeCountrySpecificInstruction)objectHandle.GetObject(factory);
				}

				if (result == null)
				{
					var objectHandle = (ObjectHandle)types["Shared"];
					result = (GuaranteeCountrySpecificInstruction)objectHandle?.GetObject(factory);
				}

				return result ?? new GuaranteeCountrySpecificInstruction(factory);
			});
		}

		public new CodeDescriptionPairList GetTypeList()
		{
			return new CodeDescriptionPairList();
		}

		public virtual RefCurrencyCollection Currencies => new RefCurrencyCollection(Factory);

		public virtual CodeDescriptionPairList GetTypeList(ZString countryCode)
		{
			var result = new CodeDescriptionPairList(GetTypeCodeDescriptionPairList());
			result.AddRangeOverwriteIfExists(GetTypeListFromDB(countryCode));
			return result;
		}

		public bool SupportsAdditionalCustomsReferences(ZString guaranteeType) => SupportsAdditionalCustomsReferencesCore(guaranteeType);

		public CodeDescriptionPairList AdditionalCustomsReferenceTypes => AdditionalCustomsReferenceTypesCore;

		public override PermitTransactionTypeList GetTransactionTypeList(ZString permitType, ZString permitSubType)
		{
			return Factory.GetCachedValue(ZString.Format("GetTransactionTypeList_{0}_{1}", permitType, permitSubType), () =>
			{
				var result = new PermitTransactionTypeList();
				result.AddPair(GuaranteeTransactionTypeList.Codes.OBA, GuaranteeTransactionTypeList.Descriptions.OBA);
				return result;
			});
		}

		protected virtual CodeDescriptionPairList GetTypeCodeDescriptionPairList() => new CodeDescriptionPairList();

		protected virtual CodeDescriptionPairList GetTypeListFromDB(ZString countryCode) => RefCusCodeListTypes.GetCachedList(Factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, ZDateTime.Now);

		protected virtual bool SupportsAdditionalCustomsReferencesCore(ZString guaranteeType) => false;

		protected virtual CodeDescriptionPairList AdditionalCustomsReferenceTypesCore => new CodeDescriptionPairList();
	}
}
