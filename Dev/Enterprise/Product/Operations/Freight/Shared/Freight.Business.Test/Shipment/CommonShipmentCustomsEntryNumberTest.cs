using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonShipmentCustomsEntryNumber))]
	public class CommonShipmentCustomsEntryNumberTest : ShipmentCustomsEntryNumberTest
	{
		public override ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber()
		{
			return new CommonShipmentCustomsEntryNumber(Factory.New<CommonShipment>());
		}

		public override void TestGetCusEntryNumber()
		{
			CommonShipmentCustomsEntryNumber customsEntryNumber = (CommonShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			CommonShipment shipment = customsEntryNumber.Shipment;

			CusEntryNumber cusEntryNumber = GetCusEntryNumber(customsEntryNumber);
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertNotNull(cusEntryNumber);
			AssertEquals("new cusEntryNumber created", 1, shipment.CusEntryNumbers.Count);
			AssertEquals("JobShipment", cusEntryNumber.CE_ParentTable);
			AssertEquals(false, cusEntryNumber.CE_EntryIsSystemGenerated);
			AssertEquals(shipment.PK, cusEntryNumber.CE_ParentID);
			AssertEquals("AU", cusEntryNumber.CE_RN_NKCountryCode);

			shipment.CusEntryNumbers.AddNew();
			AssertEquals("prerequisite", 2, shipment.CusEntryNumbers.Count);
			AssertExceptionThrown(typeof(InvalidOperationException), () => GetCusEntryNumber(customsEntryNumber));
		}

		public override void TestEntryType()
		{
			CommonShipmentCustomsEntryNumber customsEntryNumber = (CommonShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			CommonShipment shipment = customsEntryNumber.Shipment;
			AssertEquals(0, shipment.CusEntryNumbers.Count);
			AssertEquals("CAN", customsEntryNumber.EntryType);
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);

			CusEntryNumber cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals("prerequisite", ZString.Empty, cusEntryNumber.CE_EntryType);
			AssertEquals("CAN", customsEntryNumber.EntryType);
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);

			cusEntryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CTN;
			Assert(customsEntryNumber.EntryTypeInfo.ReadOnly);

			cusEntryNumber.CE_EntryType = "COC";
			AssertEquals("COC", customsEntryNumber.EntryType);
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			AssertEquals(true, customsEntryNumber.EntryTypeInfo.ReadOnly);

			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			AssertEquals(false, customsEntryNumber.EntryTypeInfo.ReadOnly);

			customsEntryNumber.Reset();

			shipment.CusEntryNumbers.AddNew();
			AssertEquals(2, shipment.CusEntryNumbers.Count);
			AssertEquals(ZString.Empty, customsEntryNumber.EntryType);
			AssertEquals(true, customsEntryNumber.EntryTypeInfo.ReadOnly);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Belgium))
			{
				shipment.CusEntryNumbers.RemoveAndDeleteAll();
				shipment.CustomsEntryNumber = "911";
				shipment.CustomsEntryNumberType = ZString.Empty;
				AssertEquals("Precondition", "PMT", shipment.CustomsEntryNumberType);
				AssertEquals("A CusEntryNum should be created because CE_EntryType is default to PMT", 1, shipment.CusEntryNumbers.Count);
				AssertEquals("PMT", shipment.CusEntryNumbers[0].CE_EntryType);
			}
		}

		public override void TestProxiedEntryNumberIsReadOnly()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var declarationEntryNumber = new CusEntryNumAdditionalReferenceCollection((BusinessObject)declaration).AddNew();

			var shipmentCustomsEntryNumber = (CommonShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			shipmentCustomsEntryNumber.Shipment.CusEntryNumbers.Add(declarationEntryNumber);

			Assert(shipmentCustomsEntryNumber.EntryTypeInfo.ReadOnly);
		}

		public override void TestEntryNumber()
		{
			CommonShipmentCustomsEntryNumber customsEntryNumber = (CommonShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			CommonShipment shipment = customsEntryNumber.Shipment;
			AssertEquals(0, shipment.CusEntryNumbers.Count);
			AssertEquals(ZString.Empty, customsEntryNumber.EntryNumber);
			AssertEquals(false, customsEntryNumber.EntryNumberInfo.ReadOnly);

			CusEntryNumber cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryIsSystemGenerated = false;
			cusEntryNumber1.CE_EntryNum = "11111";
			AssertEquals("11111", customsEntryNumber.EntryNumber);
			AssertEquals(false, customsEntryNumber.EntryNumberInfo.ReadOnly);

			CusEntryNumber cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryIsSystemGenerated = false;
			cusEntryNumber2.CE_EntryNum = "22222";
			AssertEquals("11111, 22222", customsEntryNumber.EntryNumber);
			AssertEquals(true, customsEntryNumber.EntryNumberInfo.ReadOnly);

			CusEntryNumber cusEntryNumber3 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber3.CE_EntryIsSystemGenerated = false;
			cusEntryNumber3.CE_EntryNum = "33333";
			AssertEquals("11111, 22222, 33333", customsEntryNumber.EntryNumber);
			AssertEquals(true, customsEntryNumber.EntryNumberInfo.ReadOnly);
		}

		public override void TestIssueDate()
		{
			CommonShipmentCustomsEntryNumber customsEntryNumber = (CommonShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			CommonShipment shipment = customsEntryNumber.Shipment;
			AssertEquals(0, shipment.CusEntryNumbers.Count);
			AssertEquals(ZDateTime.Empty, customsEntryNumber.IssueDate);
			AssertEquals(true, customsEntryNumber.IssueDateInfo.ReadOnly);

			CusEntryNumber cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryIsSystemGenerated = false;
			cusEntryNumber1.CE_IssueDate = new ZDateTime(2011, 7, 25);
			AssertEquals(new ZDateTime(2011, 7, 25), customsEntryNumber.IssueDate);
			AssertEquals(true, customsEntryNumber.IssueDateInfo.ReadOnly);
			cusEntryNumber1.CE_EntryNum = "11111";
			AssertEquals(false, customsEntryNumber.IssueDateInfo.ReadOnly);

			CusEntryNumber cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "22222";
			cusEntryNumber2.CE_EntryIsSystemGenerated = false;
			cusEntryNumber2.CE_IssueDate = new ZDateTime(2011, 7, 25);
			AssertEquals(ZDateTime.Empty, customsEntryNumber.IssueDate);
			AssertEquals(true, customsEntryNumber.IssueDateInfo.ReadOnly);
		}

		public override void TestExpiryDate()
		{
			CommonShipmentCustomsEntryNumber customsEntryNumber = (CommonShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			CommonShipment shipment = customsEntryNumber.Shipment;
			AssertEquals(0, shipment.CusEntryNumbers.Count);
			AssertEquals(ZDateTime.Empty, customsEntryNumber.ExpiryDate);
			AssertEquals(true, customsEntryNumber.ExpiryDateInfo.ReadOnly);

			CusEntryNumber cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryIsSystemGenerated = false;
			cusEntryNumber1.CE_ExpiryDate = new ZDateTime(2011, 7, 25);
			AssertEquals(new ZDateTime(2011, 7, 25), customsEntryNumber.ExpiryDate);
			AssertEquals(true, customsEntryNumber.ExpiryDateInfo.ReadOnly);
			cusEntryNumber1.CE_EntryNum = "11111";
			AssertEquals(false, customsEntryNumber.ExpiryDateInfo.ReadOnly);

			CusEntryNumber cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "22222";
			cusEntryNumber2.CE_EntryIsSystemGenerated = false;
			cusEntryNumber2.CE_ExpiryDate = new ZDateTime(2011, 7, 25);
			AssertEquals(ZDateTime.Empty, customsEntryNumber.ExpiryDate);
			AssertEquals(true, customsEntryNumber.ExpiryDateInfo.ReadOnly);
		}

		public override void TestValidation()
		{
			CommonShipmentCustomsEntryNumber customsEntryNumber = (CommonShipmentCustomsEntryNumber)GetNewShipmentCustomsEntryNumber();
			AssertNotNull(customsEntryNumber.Validation);
			AssertEquals(typeof(ShipmentCustomsEntryNumberValidation), customsEntryNumber.Validation.GetType());
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			var bizObj = info.BizObj as CommonShipmentCustomsEntryNumber;

			if (info.Name == ShipmentCustomsEntryNumber.Schema.EntryNumber)
			{
				bizObj.EntryType = "CCN";
			}
			else if (info.Name == ShipmentCustomsEntryNumber.Schema.IssueDate || info.Name == ShipmentCustomsEntryNumber.Schema.ExpiryDate)
			{
				bizObj.EntryType = "CCN";
				bizObj.EntryNumber = "AAAAAA";
			}

			base.TestBizObjectField(info);
		}

		#region Implementation

		protected CusEntryNumber GetCusEntryNumber(ShipmentCustomsEntryNumber customsEntryNumber)
		{
			return customsEntryNumber.GetType().GetMethod("GetCusEntryNumber", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(customsEntryNumber, null) as CusEntryNumber;
		}

		#endregion
	}
}
