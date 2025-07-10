using CargoWise.Types;
using Enterprise.Customs.NZ.Business;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			SetupManifestZZRequirements();
			Assert(!statusProvider.AllowCancellationMessage(header));
			header.AMA_MessageStatus = "SNT";
			Assert(statusProvider.AllowCancellationMessage(header));
			header.RegistrationNumber = "123456789";
			Assert(statusProvider.AllowCancellationMessage(header));
			header.RegistrationNumber = ZString.Empty;
			header.RegistrationStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			Assert(statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			SetupManifestZZRequirements();
			Assert(!statusProvider.AllowManifestCancellationMessage(header));
			header.AMA_MessageStatus = "SNT";
			Assert(!statusProvider.AllowManifestCancellationMessage(header));
			header.RegistrationNumber = "123456789";
			Assert(!statusProvider.AllowManifestCancellationMessage(header));
			header.RegistrationNumber = ZString.Empty;
			header.RegistrationStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			Assert(!statusProvider.AllowManifestCancellationMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			SetupManifestZZRequirements();
			Assert(!statusProvider.AllowModificationMessage(header));
			header.AMA_MessageStatus = "SNT";
			Assert(!statusProvider.AllowModificationMessage(header));
			header.RegistrationNumber = "123456789";
			Assert(statusProvider.AllowModificationMessage(header));
			header.RegistrationNumber = ZString.Empty;
			header.RegistrationStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			Assert(statusProvider.AllowModificationMessage(header));
			header.RegistrationStatus = ZString.Empty;
			Assert(!statusProvider.AllowModificationMessage(header));
		}

		public override void TestAllowOriginalMessage()
		{
			SetupManifestZZRequirements();
			Assert(statusProvider.AllowOriginalMessage(header));
			header.AMA_MessageStatus = "SNT";
			Assert(!statusProvider.AllowOriginalMessage(header));
			header.RegistrationNumber = "123456789";
			Assert(!statusProvider.AllowOriginalMessage(header));
			header.RegistrationNumber = ZString.Empty;
			header.RegistrationStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			Assert(!statusProvider.AllowOriginalMessage(header));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			SetupManifestZZRequirements();
			Assert(!statusProvider.HasManifestBeenAcceptedByCustoms(header));
			header.RegistrationNumber = "123456789";
			Assert(statusProvider.HasManifestBeenAcceptedByCustoms(header));
			header.RegistrationNumber = ZString.Empty;
			header.RegistrationStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			Assert(statusProvider.HasManifestBeenAcceptedByCustoms(header));
			header.RegistrationStatus = ZString.Empty;
			Assert(!statusProvider.HasManifestBeenAcceptedByCustoms(header));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			SetupManifestZZRequirements();
			Assert(!statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = "SNT";
			Assert(statusProvider.HasManifestBeenSubmittedToCustoms(header));
		}

		public override void TestMessageStatusCanBeReset()
		{
			SetupManifestZZRequirements();
			Assert(!statusProvider.MessageStatusCanBeReset(header));
		}

		protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider() => statusProvider;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;
			statusProvider = header.MessageStatusProvider;
		}

		AsycudaManifestHeader header;
		ASYCUDA.Business.MessageStatusProvider statusProvider;
		void SetupManifestZZRequirements()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var customsStatusCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus;
			helper.CreateCusCodeType(customsStatusCode, "Customs Manifest Status");
			var list = helper.CreateCusCodeList(Core.Constants.CountryCodes.NewZealand, customsStatusCode, LowValueManifestStatusList.Codes.ManifestAccepted, "Accepted", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(list.PK, Universal.RefCusCodeListAttributeTypes.Codes.CustomsCleared, ZString.Empty);
			Factory.Save();
		}
	}
}
