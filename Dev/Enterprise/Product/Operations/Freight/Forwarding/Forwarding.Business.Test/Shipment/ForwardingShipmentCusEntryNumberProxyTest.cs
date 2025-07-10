using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentCusEntryNumberProxy))]
	sealed class ForwardingShipmentCusEntryNumberProxyTest : ForwardingShipmentCustomsEntryNumberTest
	{
		public override ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			cusEntryNumber.CE_ParentID = shipment.PK;
			cusEntryNumber.CE_ParentTable = ForwardingShipment.Schema.TableName;
			return GetNewShipmentCustomsEntryNumber(shipment, cusEntryNumber);
		}

		ForwardingShipmentCusEntryNumberProxy GetNewShipmentCustomsEntryNumber(ForwardingShipment shipment, CusEntryNumber cusEntryNumber)
		{
			return new ForwardingShipmentCusEntryNumberProxy(shipment, cusEntryNumber);
		}

		public override void TestValidation()
		{
			ForwardingShipmentCusEntryNumberProxy customsEntryNumber = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			AssertNotNull(customsEntryNumber.Validation);
			AssertEquals(typeof(ForwardingShipmentCusEntryNumberProxyValidation), customsEntryNumber.Validation.GetType());
		}

		public void TestCreateInstance()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ForwardingShipmentCusEntryNumberProxy(shipment, null));

			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			AssertNoExceptionThrown(() => new ForwardingShipmentCusEntryNumberProxy(shipment, cusEntryNumber));

			ForwardingShipmentCusEntryNumberProxy cusEntryNumberProxy = new ForwardingShipmentCusEntryNumberProxy(shipment, cusEntryNumber);
			AssertEquals(shipment, cusEntryNumberProxy.Shipment);
			AssertEquals(cusEntryNumber, cusEntryNumberProxy.CusEntryNumber);
		}

		public override void TestGetCusEntryNumber()
		{
			ForwardingShipmentCusEntryNumberProxy cusEntryNumberProxy = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			CusEntryNumber cusEntryNumber = cusEntryNumberProxy.CusEntryNumber;
			AssertEquals(cusEntryNumber, GetCusEntryNumber(cusEntryNumberProxy));
		}

		public override void TestEntryType()
		{
			ForwardingShipmentCusEntryNumberProxy cusEntryNumberProxy = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			CusEntryNumber cusEntryNumber = cusEntryNumberProxy.CusEntryNumber;
			AssertEquals("prerequisite", false, cusEntryNumber.CE_EntryIsSystemGenerated);
			AssertEquals("prerequisite", ZString.Empty, cusEntryNumber.CE_EntryType);

			cusEntryNumber.CE_EntryType = "CAN";
			AssertEquals("CAN", cusEntryNumberProxy.EntryType);
			AssertEquals(false, cusEntryNumberProxy.EntryTypeInfo.ReadOnly);

			cusEntryNumberProxy.EntryType = "COC";
			AssertEquals("COC", cusEntryNumber.CE_EntryType);
			AssertEquals(false, cusEntryNumberProxy.EntryTypeInfo.ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals(true, cusEntryNumberProxy.EntryTypeInfo.ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals(false, cusEntryNumberProxy.EntryTypeInfo.ReadOnly);

			AssertEquals("prerequisite", true, cusEntryNumberProxy.IsExemptionCode("EXML"));
			cusEntryNumberProxy.EntryType = "EXML";
			AssertEquals("XML", cusEntryNumber.CE_EntryType);
			AssertEquals(false, cusEntryNumberProxy.EntryTypeInfo.ReadOnly);

			cusEntryNumberProxy.EntryType = "CAN";
			AssertEquals(false, cusEntryNumberProxy.EntryTypeInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			AssertEquals(false, cusEntryNumberProxy.EntryTypeInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			AssertEquals(true, cusEntryNumberProxy.EntryTypeInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Kazakhstan);
			AssertEquals(false, cusEntryNumberProxy.EntryTypeInfo.ReadOnly);
		}

		public override void TestProxiedEntryNumberIsReadOnly()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationEntryNumber = new CusEntryNumAdditionalReferenceCollection((BusinessObject)declaration).AddNew();

			var shipmentCustomsEntryNumber = new ForwardingShipmentCusEntryNumberProxy(Factory.New<ForwardingShipment>(), declarationEntryNumber);
			Assert(shipmentCustomsEntryNumber.EntryTypeInfo.ReadOnly);
		}

		public override void TestEntryNumber()
		{
			ForwardingShipmentCusEntryNumberProxy cusEntryNumberProxy = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			CusEntryNumber cusEntryNumber = cusEntryNumberProxy.CusEntryNumber;
			AssertEquals("prerequisite", false, cusEntryNumber.CE_EntryIsSystemGenerated);
			AssertEquals("prerequisite", ZString.Empty, cusEntryNumber.CE_EntryNum);

			cusEntryNumber.CE_EntryNum = "11111";
			AssertEquals("11111", cusEntryNumberProxy.EntryNumber);
			AssertEquals(false, cusEntryNumberProxy.EntryNumberInfo.ReadOnly);

			cusEntryNumberProxy.EntryNumber = "22222";
			AssertEquals("22222", cusEntryNumber.CE_EntryNum);
			AssertEquals(false, cusEntryNumberProxy.EntryNumberInfo.ReadOnly);

			AssertEquals("prerequisite", true, cusEntryNumberProxy.IsExemptionCode("EXML"));
			cusEntryNumberProxy.EntryType = "EXML";
			AssertEquals(true, cusEntryNumberProxy.EntryNumberInfo.ReadOnly);

			AssertEquals("prerequisite", false, cusEntryNumberProxy.IsExemptionCode("CAN"));
			cusEntryNumberProxy.EntryType = "CAN";
			AssertEquals(false, cusEntryNumberProxy.EntryNumberInfo.ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals(true, cusEntryNumberProxy.EntryNumberInfo.ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals(false, cusEntryNumberProxy.EntryNumberInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			AssertEquals(false, cusEntryNumberProxy.EntryNumberInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			AssertEquals(true, cusEntryNumberProxy.EntryNumberInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Kazakhstan);
			AssertEquals(false, cusEntryNumberProxy.EntryNumberInfo.ReadOnly);
		}

		public override void TestIssueDate()
		{
			ForwardingShipmentCusEntryNumberProxy cusEntryNumberProxy = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			CusEntryNumber cusEntryNumber = cusEntryNumberProxy.CusEntryNumber;

			cusEntryNumber.CE_IssueDate = new ZDateTime(2011, 1, 1);
			AssertEquals(new ZDateTime(2011, 1, 1), cusEntryNumberProxy.IssueDate);
			AssertEquals(false, cusEntryNumberProxy.IssueDateInfo.ReadOnly);

			cusEntryNumberProxy.IssueDate = new ZDateTime(2012, 2, 2);
			AssertEquals(new ZDateTime(2012, 2, 2), cusEntryNumber.CE_IssueDate);
			AssertEquals(false, cusEntryNumberProxy.IssueDateInfo.ReadOnly);

			AssertEquals("prerequisite", true, cusEntryNumberProxy.IsExemptionCode("EXML"));
			cusEntryNumberProxy.EntryType = "EXML";
			AssertEquals(true, cusEntryNumberProxy.IssueDateInfo.ReadOnly);

			AssertEquals("prerequisite", false, cusEntryNumberProxy.IsExemptionCode("CAN"));
			cusEntryNumberProxy.EntryType = "CAN";
			AssertEquals(false, cusEntryNumberProxy.IssueDateInfo.ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals(true, cusEntryNumberProxy.IssueDateInfo.ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals(false, cusEntryNumberProxy.IssueDateInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			AssertEquals(false, cusEntryNumberProxy.IssueDateInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			AssertEquals(true, cusEntryNumberProxy.IssueDateInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Kazakhstan);
			AssertEquals(false, cusEntryNumberProxy.IssueDateInfo.ReadOnly);
		}

		public override void TestExpiryDate()
		{
			ForwardingShipmentCusEntryNumberProxy cusEntryNumberProxy = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			CusEntryNumber cusEntryNumber = cusEntryNumberProxy.CusEntryNumber;

			cusEntryNumber.CE_ExpiryDate = new ZDateTime(2011, 1, 1);
			AssertEquals(new ZDateTime(2011, 1, 1), cusEntryNumberProxy.ExpiryDate);
			AssertEquals(false, cusEntryNumberProxy.ExpiryDateInfo.ReadOnly);

			cusEntryNumberProxy.ExpiryDate = new ZDateTime(2012, 2, 2);
			AssertEquals(new ZDateTime(2012, 2, 2), cusEntryNumber.CE_ExpiryDate);
			AssertEquals(false, cusEntryNumberProxy.ExpiryDateInfo.ReadOnly);

			AssertEquals("prerequisite", true, cusEntryNumberProxy.IsExemptionCode("EXML"));
			cusEntryNumberProxy.EntryType = "EXML";
			AssertEquals(true, cusEntryNumberProxy.ExpiryDateInfo.ReadOnly);

			AssertEquals("prerequisite", false, cusEntryNumberProxy.IsExemptionCode("CAN"));
			cusEntryNumberProxy.EntryType = "CAN";
			AssertEquals(false, cusEntryNumberProxy.ExpiryDateInfo.ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals(true, cusEntryNumberProxy.ExpiryDateInfo.ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals(false, cusEntryNumberProxy.ExpiryDateInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			AssertEquals(false, cusEntryNumberProxy.ExpiryDateInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			AssertEquals(true, cusEntryNumberProxy.ExpiryDateInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Kazakhstan);
			AssertEquals(false, cusEntryNumberProxy.ExpiryDateInfo.ReadOnly);
		}

		public void TestNonShipmentsCusEntryNumbersReadOnly()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			CusEntryNumber declarationEntryNumber = Factory.New<CusEntryNumber>();
			declarationEntryNumber.CE_EntryIsSystemGenerated = false;
			declarationEntryNumber.CE_ParentID = declaration.PK;
			declarationEntryNumber.CE_ParentTable = "JobDeclaration";

			AssertEquals("prerequisite", declarationEntryNumber, shipment.CusEntryNumbers[0]);

			CusEntryNumber shipmentCusEntryNumber = shipment.CusEntryNumbers.AddNew();
			shipmentCusEntryNumber.CE_EntryIsSystemGenerated = false;
			shipmentCusEntryNumber.CE_ParentID = shipment.PK;
			shipmentCusEntryNumber.CE_ParentTable = ForwardingShipment.Schema.TableName;

			ForwardingShipmentCusEntryNumberProxy declarationCusEntryNumberProxy = new ForwardingShipmentCusEntryNumberProxy(shipment, declarationEntryNumber);
			ForwardingShipmentCusEntryNumberProxy shipmentCusEntryNumberProxy = new ForwardingShipmentCusEntryNumberProxy(shipment, shipmentCusEntryNumber);

			AssertEquals(true, declarationCusEntryNumberProxy.ReadOnly);
			AssertEquals(false, shipmentCusEntryNumberProxy.ReadOnly);
		}

		public void TestCanDeleteShipmentsRelatedCusEntryNumbers()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			CusEntryNumber declarationEntryNumber = Factory.New<CusEntryNumber>();
			declarationEntryNumber.CE_EntryIsSystemGenerated = false;
			declarationEntryNumber.CE_ParentID = declaration.PK;
			declarationEntryNumber.CE_ParentTable = declaration.TableName;

			AssertEquals("prerequisite", declarationEntryNumber, shipment.CusEntryNumbers[0]);

			CusEntryNumber shipmentCusEntryNumber = shipment.CusEntryNumbers.AddNew();
			shipmentCusEntryNumber.CE_EntryIsSystemGenerated = false;
			shipmentCusEntryNumber.CE_ParentID = shipment.PK;
			shipmentCusEntryNumber.CE_ParentTable = ForwardingShipment.Schema.TableName;

			ForwardingShipmentCusEntryNumberProxy declarationCusEntryNumberProxy = new ForwardingShipmentCusEntryNumberProxy(shipment, declarationEntryNumber);
			ForwardingShipmentCusEntryNumberProxy shipmentCusEntryNumberProxy = new ForwardingShipmentCusEntryNumberProxy(shipment, shipmentCusEntryNumber);

			ICanDelete canDeleteCusEntryNumberProxy = declarationCusEntryNumberProxy;
			AssertEquals(false, canDeleteCusEntryNumberProxy.CanDelete);

			canDeleteCusEntryNumberProxy = shipmentCusEntryNumberProxy;
			AssertEquals(true, canDeleteCusEntryNumberProxy.CanDelete);
		}

		protected override void AssertPropertyInfoReadOnly(bool expectedReadOnly, Func<ForwardingShipmentCustomsEntryNumber, ZPropertyInfo> propertyInfoProvider)
		{
			ForwardingShipmentCusEntryNumberProxy customsEntryNumber = (ForwardingShipmentCusEntryNumberProxy)GetNewShipmentCustomsEntryNumber();
			CusEntryNumber cusEntryNumber = customsEntryNumber.CusEntryNumber;

			AssertEquals(String.Format("EntryNumber_ReadOnly for {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode),
				expectedReadOnly, propertyInfoProvider(customsEntryNumber).ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals(String.Format("EntryNumber_ReadOnly for {0}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode),
				true, propertyInfoProvider(customsEntryNumber).ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
		}
		ZString StoredCountry;

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(StoredCountry);
		}
	}
}
