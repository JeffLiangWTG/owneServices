using System;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using IMessageParent = Enterprise.Customs.ASYCUDA.Business.IMessageParent;
using MessageStatusCodeList = Enterprise.Customs.ASYCUDA.Business.MessageStatusCodeList;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			AssertActionForCustomsAcceptance("Cancel", statusProvider.AllowCancellationMessage);
		}

		public override void TestAllowManifestCancellationMessage()
		{
			Assert("ZAManifest does not currently support manifest level cancellation", !statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			AssertActionForCustomsAcceptance("Modify", statusProvider.AllowModificationMessage);
		}

		void AssertActionForCustomsAcceptance(string action, Func<IMessageParent, bool> func)
		{
			AssertForCustomsAcceptance(func, $"Cannot {action} without customs acceptance", $"{action} is allowed when we have a CARN", $"{action} is allowed when we have a CustomsCleared status", $"{action} is allowed when we have a RegistrationNumber");
		}

		void AssertForCustomsAcceptance(Func<IMessageParent, bool> func, string notAccepted, string carnAccepted, string statusAccepted, string numberAccepted)
		{
			Assert(notAccepted, !func(header));
			header.CARN = "1234";
			Assert(carnAccepted, func(header));
			header.CARN = ZString.Empty;
			header.RegistrationStatus = "8";
			Assert(statusAccepted, func(header));
			header.RegistrationStatus = ZString.Empty;
			header.RegistrationNumber = "REGNO1234";
			Assert(numberAccepted, func(header));
		}

		public override void TestAllowOriginalMessage()
		{
			bool Func(IMessageParent parent) => !statusProvider.AllowOriginalMessage(header);
			AssertForCustomsAcceptance(Func, $"Original allowed while we do not have a customs acceptance", $"Original is allowed when we have a CARN", $"Original is allowed when we have a CustomsCleared status", $"Original is allowed when we have a RegistrationNumber");
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			AssertForCustomsAcceptance(statusProvider.HasManifestBeenAcceptedByCustoms, $"Not Accepted", $"Accepted when we have a CARN", $"Accepted when we have a CustomsCleared status", $"Accepted when we have a RegistrationNumber");
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			Assert("Not Submitted since we do not have a RegistrationNumber or MessageStatus", !statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			Assert("Manifest has been sent when we have a 'Sent' MessageStatus", statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
			Assert("Manifest must no longer be seent when we have an 'Error' MessageStatus", !statusProvider.HasManifestBeenSubmittedToCustoms(header));
			AssertForCustomsAcceptance(statusProvider.HasManifestBeenSubmittedToCustoms, $"Not Submitted", $"Submitted when we have a CARN", $"Submitted when we have a CustomsCleared status", $"Submitted when we have a RegistrationNumber");
		}

		public override void TestMessageStatusCanBeReset()
		{
			Assert("ZA Manifests do not currently support message status reset", !statusProvider.MessageStatusCanBeReset(header));
		}

		public void TestAllowModificationMessageAndAllowCancellationMessage()
		{
			foreach (var mode in new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road })
			{
				header = Factory.New<AsycudaManifestHeader>();
				header.AMA_TransportMode = mode;
				var messageStatusProvider = new MessageStatusProvider();
				AssertEquals(false, messageStatusProvider.AllowModificationMessage(header));
				AssertEquals(false, messageStatusProvider.AllowCancellationMessage(header));
				header.CARN = "SmallHands";
				AssertEquals(true, messageStatusProvider.AllowModificationMessage(header));
				AssertEquals(true, messageStatusProvider.AllowCancellationMessage(header));
				header.CARN = "";
				header.RegistrationNumber = "LargeFeet";
				AssertEquals(true, messageStatusProvider.AllowModificationMessage(header));
				AssertEquals(true, messageStatusProvider.AllowCancellationMessage(header));
				header.RegistrationNumber = "";
				header.RegistrationStatus = "8";
				AssertEquals(true, messageStatusProvider.AllowModificationMessage(header));
				AssertEquals(true, messageStatusProvider.AllowCancellationMessage(header));
			}
		}

		public void TestGetMessageStatusList()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			statusProvider = header.MessageStatusProvider;
			AssertSame(Factory.GetCachedValue<ZAMessageStatusList>(), statusProvider.GetMessageStatusList(Factory, "ZA"));
			AssertSame(Factory.GetCachedValue<ZAMessageStatusList>(), statusProvider.GetMessageStatusList(Factory, "US"));
			AssertSame(Factory.GetCachedValue<ZAMessageStatusList>(), statusProvider.GetMessageStatusList(Factory, ""));
		}

		public void TestGetRegistrationStatusList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "CustomsStatus");
			var za8 = helper.CreateNewOrGetExistingCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border (SACU clearances)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IAllowCancel", "true");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "INotify", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "ISendEntryDocs", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IUpdateCustomsStatus", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IUpdateEntryNumber", "");
			var za9 = helper.CreateNewOrGetExistingCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(za9.PK, "INotify", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za9.PK, "IUpdateEntryNumber", "");
			Factory.Save();
			var list1 = header.Lookups.RegistrationStatusList;
			var list2 = header.Lookups.RegistrationStatusList;
			Assert(ReferenceEquals(list1, list2));
			Assert(list1.ContainsCode("8"));
			Assert(list1.ContainsCode("9"));
		}

		AsycudaManifestHeader header;
		ASYCUDA.Business.MessageStatusProvider statusProvider;
		protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider()
		{
			return statusProvider;
		}

		protected override void SetUp()
		{
			base.SetUp();
			ZaAsycudaToCuscarTests.SetupZZ(Factory, Core.Constants.CountryCodes.SouthAfrica);
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			statusProvider = header.MessageStatusProvider;
		}
	}
}
