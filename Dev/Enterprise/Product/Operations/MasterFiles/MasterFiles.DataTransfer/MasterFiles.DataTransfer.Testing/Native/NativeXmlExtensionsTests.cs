using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Native;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Testing.Native
{
	class NativeXmlExtensionsTests : TestCaseWithFactory
	{
		public void TestGetNativeMessageSubTypeFromObjectType()
		{
			foreach (var tuple in BizOTypes)
			{
				AssertEquals($"The BizO type {tuple.Item1} does not have the correct subtype code.", tuple.Item2, tuple.Item1.GetNativeMessageSubTypeFromObjectType());
				AssertNotNull("The subtype is not a member of the CodeDescriptionPair list.", new EDIMessageSubTypeList()[tuple.Item2]);
				AssertNoExceptionThrown(() => BizOTypes.Single(i => i.Item1 == tuple.Item1));
			}
		}

		static IEnumerable<Tuple<Type, string>> BizOTypes => new[]
		{
			Tuple.Create(typeof(OrgHeader), EDIMessageSubTypeList.Codes.XmlNativeOrganization),
			Tuple.Create(typeof(IRatingHeader), EDIMessageSubTypeList.Codes.XmlNativeRate),
			Tuple.Create(typeof(ICommonShipment), EDIMessageSubTypeList.Codes.XmlNativeShipment),
			Tuple.Create(typeof(RefUNLOCO), EDIMessageSubTypeList.Codes.XmlNativeUNLOCO),
			Tuple.Create(typeof(IRefAirline), EDIMessageSubTypeList.Codes.XmlNativeAirline),
			Tuple.Create(typeof(IRefCommodityCode), EDIMessageSubTypeList.Codes.XmlNativeCommodityCode),
			Tuple.Create(typeof(GlbCompany), EDIMessageSubTypeList.Codes.XmlNativeCompany),
			Tuple.Create(typeof(IRefContainer), EDIMessageSubTypeList.Codes.XmlNativeContainer),
			Tuple.Create(typeof(IRefCountry), EDIMessageSubTypeList.Codes.XmlNativeCountry),
			Tuple.Create(typeof(IRefExchangeRate), EDIMessageSubTypeList.Codes.XmlNativeCurrencyExchangeRate),
			Tuple.Create(typeof(Customs.Shared.IBaseCusStatementHeader), EDIMessageSubTypeList.Codes.XmlNativeCountry),
			Tuple.Create(typeof(Forwarding.IOrder), EDIMessageSubTypeList.Codes.XmlNativeOrder),
			Tuple.Create(typeof(OrgSupplierPart), EDIMessageSubTypeList.Codes.XmlNativeProduct),
			Tuple.Create(typeof(IRefServiceLevel), EDIMessageSubTypeList.Codes.XmlNativeRate),
			Tuple.Create(typeof(GlbStaff), EDIMessageSubTypeList.Codes.XmlNativeStaff),
			Tuple.Create(typeof(IRefVessel), EDIMessageSubTypeList.Codes.XmlNativeVessel),
			Tuple.Create(typeof(Customs.IJobDeclarationWithShipmentSynchonisation), EDIMessageSubTypeList.Codes.XmlNativeDeclaration)
		};
	}
}
