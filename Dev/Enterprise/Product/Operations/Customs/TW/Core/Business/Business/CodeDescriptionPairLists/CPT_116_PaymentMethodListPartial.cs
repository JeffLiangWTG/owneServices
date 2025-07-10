using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public static class CPT_116_PaymentMethodList
	{
		public static CodeDescriptionPairList GetPaymentMethodList(BusinessObjectFactory factory, ZString controllingAgency)
		{
			CodeDescriptionPairList codeDescriptionPairList;
			if (controllingAgency == ControllingAgencyList.Codes._20 ||
				controllingAgency == ControllingAgencyList.Codes.CI ||
				controllingAgency == ControllingAgencyList.Codes._2Q)
			{
				codeDescriptionPairList = factory.GetCachedValue<CPT_116_301_PaymentMethodList>();
			}
			else if (controllingAgency == ControllingAgencyList.Codes.DN)
			{
				codeDescriptionPairList = factory.GetCachedValue<CPT_116_301DN_PaymentMethodList>();
			}
			else if (controllingAgency == ControllingAgencyList.Codes.VP)
			{
				codeDescriptionPairList = factory.GetCachedValue<CPT_116_401_PaymentMethodList>();
			}
			else if (controllingAgency == ControllingAgencyList.Codes.CD ||
					controllingAgency == ControllingAgencyList.Codes.IF ||
					controllingAgency == ControllingAgencyList.Codes.DH)
			{
				codeDescriptionPairList = factory.GetCachedValue<CPT_116_601_603_PaymentMethodList>();
			}
			else if (controllingAgency == ControllingAgencyList.Codes.AX)
			{
				codeDescriptionPairList = factory.GetCachedValue<CPT_116_301_AX_PaymentMethodList>();
			}
			else
			{
				codeDescriptionPairList = factory.GetCachedValue<CodeDescriptionPairList>();
			}
			return codeDescriptionPairList;
		}
	}
}
