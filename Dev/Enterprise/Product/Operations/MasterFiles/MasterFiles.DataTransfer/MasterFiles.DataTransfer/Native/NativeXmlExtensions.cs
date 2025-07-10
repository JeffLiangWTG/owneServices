using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.DataTransfer.Native
{
	public static class NativeXmlExtensions
	{
		public static ZString GetNativeMessageSubTypeFromObjectType(this Type type)
		{
			var mappedType = BizOTypeCodePairs.FirstOrDefault(pair => pair.Key.IsAssignableFrom(type)).Value;
			if (string.IsNullOrEmpty(mappedType))
			{
				ErrorReporter.ReportOnce("Enterprise.MasterFiles.DataTransfer.Native.NativeXmlExtensions.GetNativeMessageSubTypeFromObjectType", FormattableString.Invariant($"BizO type {type} could not be mapped."));
			}
			return mappedType ?? ZString.Empty;
		}

		static Dictionary<Type, string> BizOTypeCodePairs => new Dictionary<Type, string>
		{
			{ typeof(OrgHeader), EDIMessageSubTypeList.Codes.XmlNativeOrganization },
			{ typeof(IRatingHeader), EDIMessageSubTypeList.Codes.XmlNativeRate },
			{ typeof(ICommonShipment), EDIMessageSubTypeList.Codes.XmlNativeShipment },
			{ typeof(RefUNLOCO), EDIMessageSubTypeList.Codes.XmlNativeUNLOCO },
			{ typeof(IRefAirline), EDIMessageSubTypeList.Codes.XmlNativeAirline },
			{ typeof(IRefCommodityCode), EDIMessageSubTypeList.Codes.XmlNativeCommodityCode },
			{ typeof(GlbCompany), EDIMessageSubTypeList.Codes.XmlNativeCompany },
			{ typeof(IRefContainer), EDIMessageSubTypeList.Codes.XmlNativeContainer },
			{ typeof(IRefCountry), EDIMessageSubTypeList.Codes.XmlNativeCountry },
			{ typeof(IRefExchangeRate), EDIMessageSubTypeList.Codes.XmlNativeCurrencyExchangeRate },
			{ typeof(Shared.IBaseCusStatementHeader), EDIMessageSubTypeList.Codes.XmlNativeCountry },
			{ typeof(Forwarding.IOrder), EDIMessageSubTypeList.Codes.XmlNativeOrder },
			{ typeof(OrgSupplierPart), EDIMessageSubTypeList.Codes.XmlNativeProduct },
			{ typeof(IRefServiceLevel), EDIMessageSubTypeList.Codes.XmlNativeRate },
			{ typeof(GlbStaff), EDIMessageSubTypeList.Codes.XmlNativeStaff },
			{ typeof(IRefVessel), EDIMessageSubTypeList.Codes.XmlNativeVessel },
			{ typeof(IJobDeclarationWithShipmentSynchonisation), EDIMessageSubTypeList.Codes.XmlNativeDeclaration },
			{ typeof(OrgSalesCall), EDIMessageSubTypeList.Codes.XmlNativeCommunication },
			{ typeof(OrgOpportunity), EDIMessageSubTypeList.Codes.XmlNativeOpportunity }
		};
	}
}
