using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderAllocateNumber))]
	sealed class AsycudaManifestHeaderAllocateNumberTest : AllocateNumberTest
	{
		protected override AllocateNumber CreateAllocateNumber(bool allowPart5NumberOutrangeWhenEntered = false)
		{
			return new AsycudaManifestHeaderAllocateNumber(Header, allowPart5NumberOutrangeWhenEntered);
		}

		protected override ImmutableArray<bool> PartNumberReadOnlyArray => ImmutableArray.Create(true, true, true, true, false);

		protected override TWCustomsNumberViewStmNumsWrapper GetTWCustomsNumberViewStmNumsWrapperCore()
		{
			var company = Header.Branch.Company;
			var provider = company.CustomsNumberProvider;
			provider.CustomsNumbers.DeleteAll();
			provider.CustomsNumberWrappers.RemoveAndDeleteAll();
			var stmNums = provider.CustomsNumbers.AddNew();
			var wrapper = (TWCustomsNumberViewStmNumsWrapper)stmNums.Wrapper;
			wrapper.MessageType = "IMP";
			wrapper.RangeType = "D";
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
			AssertEquals("AB  091235555", allocateNumber.Number);
		}

		public void TestSetDefaultPartNumber()
		{
			var allocateNumber = CreateAllocateNumber();
			CombineAssertions(() =>
			{
				AssertEquals("Part2Number", "", allocateNumber.Part2Number);
				AssertEquals("Part4Number", "123", allocateNumber.Part4Number);
			});
		}

		public void TestFormCaption()
		{
			var allocateNumber = CreateAllocateNumber();
			AssertEquals("Modify Entry Number", allocateNumber.FormCaption.Caption);
		}

		public void TestFormDescription()
		{
			var allocateNumber = CreateAllocateNumber();
			AssertEquals("Enter the number and click 'Modify Entry Number' or leave Entry Number blank and the next available Entry Number will be allocated.", allocateNumber.FormDescription.Caption);
		}

		AsycudaManifestHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<AsycudaManifestHeader>();
					header.AMA_Nature = "IMP";
					header.AMA_CustomsOffice = "AB";
					header.AMA_RecipientReference = "123";
				}
				return header;
			}
		}

		AsycudaManifestHeader header;
	}
}
