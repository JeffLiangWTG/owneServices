using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentCustomsEntryNumber))]
	class ForwardingShipmentCustomsEntryNumberTest : CommonShipmentCustomsEntryNumberTest
	{
		public override ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			return new ForwardingShipmentCustomsEntryNumber(Factory.New<ForwardingShipment>());
		}

		public override void TestValidation()
		{
			ForwardingShipmentCustomsEntryNumber customsEntryNumber = (ForwardingShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			AssertNotNull(customsEntryNumber.Validation);
			AssertEquals(typeof(ForwardingShipmentCustomsEntryNumberValidation), customsEntryNumber.Validation.GetType());
		}

		public void TestEntryNumber_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertPropertyInfoReadOnly(false, (customsEntryNumber) => customsEntryNumber.EntryNumberInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				AssertPropertyInfoReadOnly(false, (customsEntryNumber) => customsEntryNumber.EntryNumberInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Iceland))
			{
				AssertPropertyInfoReadOnly(true, (customsEntryNumber) => customsEntryNumber.EntryNumberInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Kazakhstan))
			{
				AssertPropertyInfoReadOnly(false, (customsEntryNumber) => customsEntryNumber.EntryNumberInfo);
			}
		}

		public void TestEntryType_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertPropertyInfoReadOnly(false, (customsEntryNumber) => customsEntryNumber.EntryTypeInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				AssertPropertyInfoReadOnly(false, (customsEntryNumber) => customsEntryNumber.EntryTypeInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Iceland))
			{
				AssertPropertyInfoReadOnly(true, (customsEntryNumber) => customsEntryNumber.EntryTypeInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Kazakhstan))
			{
				AssertPropertyInfoReadOnly(false, (customsEntryNumber) => customsEntryNumber.EntryTypeInfo);
			}
		}

		protected virtual void AssertPropertyInfoReadOnly(bool expectedReadOnly, Func<ForwardingShipmentCustomsEntryNumber, ZPropertyInfo> propertyInfoProvider)
		{
			ForwardingShipmentCustomsEntryNumber customsEntryNumber = (ForwardingShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			ForwardingShipment shipment = customsEntryNumber.Shipment;
			AssertEquals(String.Format("EntryNumber_ReadOnly for {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode),
				expectedReadOnly, propertyInfoProvider(customsEntryNumber).ReadOnly);

			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;

			AssertEquals(String.Format("EntryNumber_ReadOnly for {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode),
				expectedReadOnly, propertyInfoProvider(customsEntryNumber).ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals(String.Format("EntryNumber_ReadOnly for {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode),
				true, propertyInfoProvider(customsEntryNumber).ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals(String.Format("EntryNumber_ReadOnly for {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode),
				expectedReadOnly, propertyInfoProvider(customsEntryNumber).ReadOnly);

			shipment.CusEntryNumbers.AddNew();
			AssertEquals(String.Format("EntryNumber_ReadOnly for {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode),
				true, propertyInfoProvider(customsEntryNumber).ReadOnly);
		}

		public void TestMRNPortMessagingSyncronisation()
		{
			var portMessagingHelper = new Mock<Integration.Forwarding.IPortMessagingForwardingHelper>(MockBehavior.Strict);

			using (ObjectFactory.Substitute(portMessagingHelper.Object))
			{
				var customsEntryNumber = (ForwardingShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
				customsEntryNumber.EntryType = "FOO";
				customsEntryNumber.EntryNumber = "11";

				portMessagingHelper.Setup(m => m.SyncroniseMRN(Factory, customsEntryNumber.Shipment.PK, "11", "22"));
				customsEntryNumber.EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				customsEntryNumber.EntryNumber = "22";

				Assert("Asserting via mocks", true);
			}
		}
	}
}
