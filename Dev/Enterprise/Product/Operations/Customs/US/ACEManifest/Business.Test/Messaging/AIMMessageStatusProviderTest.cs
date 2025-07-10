using System.Linq;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	sealed class AIMMessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			Assert("Should always allow.", Provider.AllowCancellationMessage(Bill));
		}

		public override void TestAllowModificationMessage()
		{
			Assert("Should always allow.", Provider.AllowModificationMessage(Bill));
		}

		public override void TestAllowOriginalMessage()
		{
			Assert("Should always allow.", Provider.AllowOriginalMessage(Bill));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			Assert("Should be false as it's bill level message.", !Provider.AllowManifestCancellationMessage(Bill));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			Assert("Should be false as it's bill level message.", !Provider.HasManifestBeenAcceptedByCustoms(Bill));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			Assert("Should be false as it's bill level message.", !Provider.HasManifestBeenSubmittedToCustoms(Bill));
		}

		public override void TestMessageStatusCanBeReset()
		{
			Assert("Should be false as it's Air AMS Message.", !Provider.MessageStatusCanBeReset(Bill));
		}

		public override void TestGetMessageFunctionSubTypeForSend()
		{
			AssertEquals("When header is null or IsExpressCourier is false, return FRI.", AIMMessageSubTypes.FRI, Provider.GetMessageFunctionSubTypeForSend(Header));
			AssertEquals("When header is null or IsExpressCourier is false, return FRI.", AIMMessageSubTypes.FRI, Provider.GetMessageFunctionSubTypeForSend(null));

			Header.IsExpressCourier = true;
			AssertEquals("When header is not null and IsExpressCourier is true, return FXI.", AIMMessageSubTypes.FXI, Provider.GetMessageFunctionSubTypeForSend(Header));
		}

		public override void TestGetMessageFunctionSubTypeForAmend()
		{
			AssertEquals("When header is null or IsExpressCourier is false, return FRC.", AIMMessageSubTypes.FRC, Provider.GetMessageFunctionSubTypeForAmend(Header));
			AssertEquals("When header is null or IsExpressCourier is false, return FRC.", AIMMessageSubTypes.FRC, Provider.GetMessageFunctionSubTypeForAmend(null));

			Header.IsExpressCourier = true;
			AssertEquals("When header is not null and IsExpressCourier is true, return FXC.", AIMMessageSubTypes.FXC, Provider.GetMessageFunctionSubTypeForAmend(Header));
		}

		public override void TestGetMessageFunctionSubTypeForCancel()
		{
			AssertEquals("When header is null or IsExpressCourier is false, return FRX.", AIMMessageSubTypes.FRX, Provider.GetMessageFunctionSubTypeForCancel(Header));
			AssertEquals("When header is null or IsExpressCourier is false, return FRX.", AIMMessageSubTypes.FRX, Provider.GetMessageFunctionSubTypeForCancel(null));

			Header.IsExpressCourier = true;
			AssertEquals("When header is not null and IsExpressCourier is true, return FXX.", AIMMessageSubTypes.FXX, Provider.GetMessageFunctionSubTypeForCancel(Header));
		}

		protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider()
		{
			return new AIMMessageStatusProvider();
		}

		#region Implement
		AIMMessageStatusProvider Provider => provider ?? (provider = (AIMMessageStatusProvider)GetMessageStatusProvider());
		AIMMessageStatusProvider provider;
		AsycudaBill Bill => Header.Bills.Cast<AsycudaBill>().FirstOrDefault() ?? Header.Bills.AddNew();
		AsycudaManifestHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
				}

				return header;
			}
		}

		AsycudaManifestHeader header;
		#endregion
	}
}
