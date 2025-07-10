using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	sealed class AIMMessageChooserTest : TestCaseWithFactory
	{
		public void TestReason()
		{
			var chooser = GetChooser();
			AssertEquals(2, chooser.ReasonInfo.MaxLength);
		}

		public void TestRequestCode()
		{
			var chooser = GetChooser();
			AssertEquals(2, chooser.ReasonInfo.MaxLength);
		}

		public void TestIsChangeOrCancellation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var items = header.Bills.Cast<ISelectionItem>();
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FSQ);
			Assert(!chooser.IsChangeOrCancellation);
			var chooser2 = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRC);
			Assert(chooser2.IsChangeOrCancellation);
			var chooser3 = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRX);
			Assert(chooser3.IsChangeOrCancellation);
			var chooser4 = new AIMMessageChooser(header, items, AIMMessageSubTypes.FXC);
			Assert(chooser4.IsChangeOrCancellation);
			var chooser5 = new AIMMessageChooser(header, items, AIMMessageSubTypes.FXX);
			Assert(chooser5.IsChangeOrCancellation);
		}

		public void TestIsFreightStatusQuery()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var items = header.Bills.Cast<ISelectionItem>();
			var chooser = new AIMMessageChooser(header, items, AIMMessageSubTypes.FRX);
			Assert(!chooser.IsFreightStatusQuery);
			var chooser2 = new AIMMessageChooser(header, items, AIMMessageSubTypes.FSQ);
			Assert(chooser2.IsFreightStatusQuery);
		}

		public void TestIsManifestMessage()
		{
			var chooser = GetChooser();
			Assert(!chooser.IsManifestMessage);
			chooser.IsManifestMessage = true;
			Assert(chooser.IsManifestMessage);
		}

		public void TestLookups()
		{
			var chooser = GetChooser();
			AssertType<AIMMessageChooserLookups>(chooser.Lookups);
		}

		public void TestValidation()
		{
			var chooser = GetChooser();
			AssertType<AIMMessageChooserValidation>(chooser.Validation);
		}

		AIMMessageChooser GetChooser()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var items = header.Bills.Cast<ISelectionItem>();
			return new AIMMessageChooser(header, items, string.Empty);
		}
	}
}
