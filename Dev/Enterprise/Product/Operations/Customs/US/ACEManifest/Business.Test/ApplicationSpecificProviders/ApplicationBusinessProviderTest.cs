using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.ACEManifest.Business.UniversalDataTransfer;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : ASYCUDA.Business.Testing.ApplicationBusinessProviderAbstractTest<ApplicationBusinessProvider, AsycudaManifestHeader>
	{
		public void TestManifestDescription()
		{
			var manifest = CreateNewManifest();
			AssertManifestDescription(manifest, "United States - Air AMS (Import)", (x) => true);
		}

		public void TestSupportsAutoSendGlobalManifest()
		{
			CombineAssertions(() =>
			{
				var header = CreateNewManifest();
				var provider = header.ApplicationBusinessProvider;
				var messageStatusProvider = new AIMMessageStatusProvider();
				AssertEquals("Supports IAM", true, provider.SupportsAutoSendGlobalManifest(new ACEManifestTypes().IAM.Code));
				AssertEquals("Supports no other types", false, provider.SupportsAutoSendGlobalManifest("ASY"));
				var billOriginal = header.Bills.AddNew();
				var billAmendment = header.Bills.AddNew();
				billAmendment.ABL_MessageStatus = MessageStatusCodeList.Codes.Sent;
				var billOther = header.Bills.AddNew();
				billOther.ABL_MessageStatus = MessageStatusCodeList.Codes.Accepted;
				Factory.Save();
				var processor = provider.GetSendGlobalManifestProcessor(header);
				AssertNotNull("SGM processor generated", processor);
				processor.Process(new LoggingInformation());
				Factory.ReloadAll<AsycudaBill>();
				AssertEquals("Message status for original", MessageStatusCodeList.Codes.Sent, billOriginal.ABL_MessageStatus);
				AssertEquals("One message added for original", 1, billOriginal.Messages.Count);
				AssertEquals("Message subtype for original", messageStatusProvider.GetMessageFunctionSubTypeForSend(header), billOriginal.Messages[0].EM_MessageSubType);
				AssertEquals("One message added for amendment", 1, billAmendment.Messages.Count);
				AssertEquals("Message subtype for amendment", messageStatusProvider.GetMessageFunctionSubTypeForAmend(header), billAmendment.Messages[0].EM_MessageSubType);
				AssertEquals("Message status is sent for original", MessageStatusCodeList.Codes.Sent, billOriginal.ABL_MessageStatus);
				AssertEquals("Message status unchanged otherwise", MessageStatusCodeList.Codes.Accepted, billOther.ABL_MessageStatus);
				AssertEquals("No message added otherwise", 0, billOther.Messages.Count);
			});
		}

		public void AssertManifestDescription(AsycudaManifestHeader manifest, string expectedDescription, Func<IManifestType, bool> filter)
		{
			var applicationProvider = manifest.ApplicationBusinessProvider;
			var manifestCountryCodes = new[] { (ZString)Core.Constants.CountryCodes.UnitedStates };
			ResetManifestTypes(applicationProvider);
			AssertEquals((new ZString("US"), new ZString(expectedDescription)), applicationProvider.GetManifestDescriptions(Factory, manifestCountryCodes, filter).FirstOrDefault());
		}

		public override void TestManifestTypes()
		{
			expectedManifestTypes = new ACEManifestTypes().All;
			base.TestManifestTypes();
		}

		protected override IEnumerable<IManifestType> ExpectedManifestTypes => expectedManifestTypes;
		IEnumerable<IManifestType> expectedManifestTypes;
		protected override Type ExpectedMessagingProviderType => typeof(MessagingProvider);
		protected override Type ExpectedFeatureProviderType => typeof(FeatureProvider);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterType => typeof(ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>);
		protected override Type ExpectedGetCustomsDeclarationDataObjectWriterType => typeof(ASYCUDA.Business.UniversalDataTransfer.AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>);
		protected override Type ExpectedGetAsycudaManifestHeaderDataObjectWriterHelperType => typeof(ACEAsycudaManifestHeaderDataObjectWriterHelper);
		protected override Type ExpectedGetAsycudaManifestDataObjectReaderHelperType => typeof(ACEAsycudaManifestDataObjectReaderHelper);
	}
}
