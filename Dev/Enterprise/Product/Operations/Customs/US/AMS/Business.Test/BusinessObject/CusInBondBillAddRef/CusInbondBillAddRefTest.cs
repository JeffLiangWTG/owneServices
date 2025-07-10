using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInbondBillAddRef))]
	sealed class CusInbondBillAddRefTest : Customs.Business.Testing.CusInbondBillAddRefTest<CusInbondBillAddRef>
	{
		public void TestICanDeleteMembers()
		{
			AssertEquals(true, ((ICanDelete)billAdditionalReference).CanDelete);
			var consol = Factory.New<ForwardingConsol>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			billAdditionalReference.BR_Qualifier = BillReferenceList.Codes.OB;
			header.BH_OverrideFreightDefaults = true;
			billAdditionalReference.BR_Qualifier = BillReferenceList.Codes.MB;
			AssertEquals(true, ((ICanDelete)billAdditionalReference).CanDelete);
		}

		public void TestB0_ForeignPortOfUnladingKCodeIsRefreshCorrectly()
		{
			var info = bill.B0_ForeignPortOfUnladingKCodeInfo;
			info.ValueChanged += info_ValueChanged;
			info_ValueChangedCount = 0;
			AssertEquals(true, info.MaxLength < CusInbondBillAddRef.Schema.BR_ReferenceNumMaxLength);
			bill.B0_ForeignPortOfUnladingKCode = "".PadLeft(info.MaxLength, '1');
			var billRef = bill.ShipmentReferenceDetails[BillReferenceList.Codes.CSK];
			AssertEquals(bill.B0_ForeignPortOfUnladingKCode, billRef.BR_ReferenceNum);
			AssertEquals(true, info_ValueChangedCount > 0);
			info_ValueChangedCount = 0;
			billRef.BR_ReferenceNum = "".PadLeft(info.MaxLength + 1, '2');
			AssertEquals(bill.B0_ForeignPortOfUnladingKCode, billRef.BR_ReferenceNum);
			AssertEquals("".PadLeft(info.MaxLength, '2'), billRef.BR_ReferenceNum);
			AssertEquals(true, info_ValueChangedCount > 0);
		}

		public void TestB0_PlaceOfDeliveryIsRefreshCorrectly()
		{
			var info = bill.B0_PlaceOfDeliveryInfo;
			info.ValueChanged += info_ValueChanged;
			info_ValueChangedCount = 0;
			AssertEquals(true, info.MaxLength < CusInbondBillAddRef.Schema.BR_ReferenceNumMaxLength);
			bill.B0_PlaceOfDelivery = "".PadLeft(info.MaxLength, '1');
			var billRef = bill.ShipmentReferenceDetails[BillReferenceList.Codes.ULC];
			AssertEquals(bill.B0_PlaceOfDelivery, billRef.BR_ReferenceNum);
			AssertEquals(true, info_ValueChangedCount > 0);
			info_ValueChangedCount = 0;
			billRef.BR_ReferenceNum = "".PadLeft(info.MaxLength + 1, '2');
			AssertEquals(bill.B0_PlaceOfDelivery, billRef.BR_ReferenceNum);
			AssertEquals("".PadLeft(info.MaxLength, '2'), billRef.BR_ReferenceNum);
			AssertEquals(true, info_ValueChangedCount > 0);
		}

		void info_ValueChanged(object sender, System.EventArgs e)
		{
			info_ValueChangedCount++;
		}

		int info_ValueChangedCount;
		public void TestIShipmentReferenceDetailMembers()
		{
			billAdditionalReference.BR_Qualifier = BillReferenceList.Codes.CN;
			billAdditionalReference.BR_ReferenceNum = "CN2342";
			IShipmentReferenceDetail reference = billAdditionalReference;
			AssertEquals(BillReferenceList.Codes.CN, reference.Qualifier);
			AssertEquals("CN2342", reference.ReferenceIdentifier);
		}

		public void TestBR_QualifierDescription()
		{
			billAdditionalReference.BR_Qualifier = ZString.Empty;
			AssertEquals(ZString.Empty, billAdditionalReference.BR_QualifierDescription);
			foreach (ICodeDescription pair in BillReferenceList.GetCachedValue(Factory))
			{
				billAdditionalReference.BR_Qualifier = pair.Code;
				AssertEquals(pair.Code, pair.Description, billAdditionalReference.BR_QualifierDescription);
			}
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CusInbondBillAddRefLookups), billAdditionalReference.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusInbondBillAddRefValidation), billAdditionalReference.Validation.GetType());
		}

		CusInBondHeader header;
		CusInBondBill bill;
		CusInbondBillAddRef billAdditionalReference;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			bill = header.Bills.AddNew();
			billAdditionalReference = bill.ShipmentReferenceDetails.AddNew();
		}
	}
}
