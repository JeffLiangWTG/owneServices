using System.Collections.Immutable;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TranshipmentAllocateNumber))]
	sealed class TranshipmentAllocateNumberTest : AllocateNumberTest
	{
		protected override AllocateNumber CreateAllocateNumber(bool allowPart5NumberOutrangeWhenEntered = false)
		{
			return new TranshipmentAllocateNumber(CusInBondHeader, allowPart5NumberOutrangeWhenEntered);
		}

		protected override ImmutableArray<bool> PartNumberReadOnlyArray => ImmutableArray.Create(true, true, true, true, false);

		protected override TWCustomsNumberViewStmNumsWrapper GetTWCustomsNumberViewStmNumsWrapperCore()
		{
			var company = CusInBondHeader.Company;
			var provider = company.CustomsNumberProvider;
			provider.CustomsNumbers.DeleteAll();
			provider.CustomsNumberWrappers.RemoveAndDeleteAll();
			var stmNums = provider.CustomsNumbers.AddNew();
			var wrapper = (TWCustomsNumberViewStmNumsWrapper)stmNums.Wrapper;
			wrapper.MessageType = "TRS";
			wrapper.RangeType = "T";
			wrapper.EndNumber = "E0005";
			wrapper.CurrentValue = "E0005";
			provider.CustomsNumberWrappers.Add(wrapper);
			return wrapper;
		}

		[TestDate(2020, 07, 22)]
		public override void TestNumber()
		{
			var allocateNumber = CreateAllocateNumber();
			allocateNumber.Part5Number = "";
			AssertEquals(ZString.Empty, allocateNumber.Number);

			allocateNumber.Part5Number = "5555";
			AssertEquals("AEAB091235555", allocateNumber.Number);
		}

		public void TestSetDefaultPartNumber()
		{
			var allocateNumber = CreateAllocateNumber();
			CombineAssertions(() =>
			{
				AssertEquals("Part2Number", "AB", allocateNumber.Part2Number);
				AssertEquals("Part4Number", "123", allocateNumber.Part4Number);
			});
		}

		public void TestFormCaption()
		{
			var allocateNumber = CreateAllocateNumber();
			AssertEquals("Allocate Entry Number", allocateNumber.FormCaption.Caption);
		}

		public void TestFormDescription()
		{
			var allocateNumber = CreateAllocateNumber();
			AssertEquals("Enter the number and click 'Allocate Entry Number' or leave Entry Number blank and the next available Entry Number will be allocated.", allocateNumber.FormDescription.Caption);
		}

		CusInBondHeader CusInBondHeader
		{
			get
			{
				if (cusInBondHeader == null)
				{
					cusInBondHeader = Factory.New<CusInBondHeader>();
					cusInBondHeader.UnladingOffice = "AB";
					cusInBondHeader.ReceiptOffice = "AE";
					cusInBondHeader.TW_BoxNumber = "123";
				}
				return cusInBondHeader;
			}
		}

		CusInBondHeader cusInBondHeader;
	}
}
