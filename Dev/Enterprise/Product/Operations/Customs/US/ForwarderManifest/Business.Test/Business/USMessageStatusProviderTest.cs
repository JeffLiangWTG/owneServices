using System;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class USMessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public void TestGetMessageStatusList()
		{
			var codeDescriptionPairList = statusProvider.GetMessageStatusList(Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(0, codeDescriptionPairList.Count);
		}

		public void TestGetArrivalStatusList()
		{
			var codeDescriptionPairList = statusProvider.GetArrivalStatusList(Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(0, codeDescriptionPairList.Count);
		}

		public void TestGetRegistrationStatusListCore()
		{
			var codeDescriptionPairList = statusProvider.GetRegistrationStatusListCore(Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(0, codeDescriptionPairList.Count);
		}

		public override void TestAllowCancellationMessage()
		{
			AssertEquals(false, statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			AssertEquals(false, statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			AssertEquals(false, statusProvider.AllowModificationMessage(header));
		}

		public override void TestAllowOriginalMessage()
		{
			AssertEquals(false, statusProvider.AllowOriginalMessage(header));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			AssertEquals(false, statusProvider.HasManifestBeenAcceptedByCustoms(header));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			AssertEquals(false, statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			AssertEquals(true, statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
			AssertEquals(false, statusProvider.HasManifestBeenSubmittedToCustoms(header));
		}

		public override void TestMessageStatusCanBeReset()
		{
			AssertEquals(false, statusProvider.MessageStatusCanBeReset(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			USCustomsDataRegistry.Instance.EnableExportManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			header.AMA_ManifestType = USExportManifestTypes.Codes.EFM;
			statusProvider = header.MessageStatusProvider;
		}

		protected override void TearDown()
		{
			USCustomsDataRegistry.Instance.EnableExportManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			base.TearDown();
		}

		protected override MessageStatusProvider GetMessageStatusProvider()
		{
			return statusProvider;
		}

		AsycudaManifestHeader header;
		MessageStatusProvider statusProvider;
	}
}
