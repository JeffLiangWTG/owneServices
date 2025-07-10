using System.Collections;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public class PermitCountrySpecificInstruction : SharedCusPermitCountrySpecificInstruction, IPermitCountrySpecificInstruction
	{
		public PermitCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static PermitCountrySpecificInstruction GetByCountryCode(BusinessObjectFactory factory, string countryCode)
		{
			return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "PermitCountrySpecificInstruction_{0}", countryCode), () =>
			{
				PermitCountrySpecificInstruction result = null;
				var types = ObjectFactory.Get<Hashtable>("PermitCountrySpecificInstructions");
				if (!string.IsNullOrEmpty(countryCode))
				{
					countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
					var objectHandle = (ObjectHandle)types[countryCode];
					result = (PermitCountrySpecificInstruction)objectHandle?.GetObject(factory);
				}

				if (result == null && ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryCode))
				{
					var objectHandle = (ObjectHandle)types[Core.Constants.CountryCodes.EuropeanUnion];
					result = (PermitCountrySpecificInstruction)objectHandle.GetObject(factory);
				}

				if (result == null)
				{
					var objectHandle = (ObjectHandle)types["Shared"];
					result = (PermitCountrySpecificInstruction)objectHandle?.GetObject(factory);
				}

				return result ?? new PermitCountrySpecificInstruction(factory);
			});
		}

		#region IPermitCountrySpecificInstruction

		ICodeDescriptionPairList IPermitCountrySpecificInstruction.GetTypeList() => GetTypeList();

		ICodeDescriptionPairList IPermitCountrySpecificInstruction.GetSubTypeList(ZString typeCode) => GetSubTypeList(typeCode);

		#endregion

		public virtual bool IsRuleExceptionApplicable(ZString ruleCode)
		{
			return GetMatchingType(ruleCode) != PermitMatchingType.SingleValue;
		}
		public virtual string GetCustomLabelForPermitNumber(ZString permitType) => string.Empty;
	}

	public enum PermitMatchingType
	{
		Range,
		SingleValue
	}
}
