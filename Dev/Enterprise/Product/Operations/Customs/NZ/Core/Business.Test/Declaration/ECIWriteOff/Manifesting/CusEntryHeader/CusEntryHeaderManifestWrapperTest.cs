using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	[TestedType(typeof(CusEntryHeaderManifestWrapper))]
	public class CusEntryHeaderManifestWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsTSWManifest()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Assert(!entryHeader.ManifestWrapper.IsTSWManifest);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Assert(entryHeader.ManifestWrapper.IsTSWManifest);
			var entryHeader2 = Factory.New<CusEntryHeader>();
			Assert(entryHeader2.ManifestWrapper.IsTSWManifest);
		}

		public void TestMessageTypeDescription()
		{
			AssertEquals(JobMessageTypeList.Descriptions.Export, entryHeader.ManifestWrapper.MessageTypeDescription);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(JobMessageTypeList.Descriptions.Import, entryHeader.ManifestWrapper.MessageTypeDescription);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals(JobMessageTypeList.Descriptions.Excise, entryHeader.ManifestWrapper.MessageTypeDescription);
		}

		public void TestMasterBill()
		{
			AssertEquals(ZString.Empty, entryHeader.ManifestWrapper.MasterBill);
			var testValue = "08111111111";
			declaration.JE_MasterBill = testValue;
			AssertEquals(testValue, entryHeader.ManifestWrapper.MasterBill);
		}

		public void TestFormattedMasterBill()
		{
			AssertEquals(ZString.Empty, entryHeader.ManifestWrapper.FormattedMasterBill);
			declaration.JE_MasterBill = "08111111111";
			AssertEquals("081-11111111", entryHeader.ManifestWrapper.FormattedMasterBill);
		}

		public void TestFlightNo()
		{
			AssertEquals(ZString.Empty, entryHeader.ManifestWrapper.FlightNo);
			var testValue = "QF253";
			declaration.JE_VoyageFlightNo = testValue;
			AssertEquals(testValue, entryHeader.ManifestWrapper.FlightNo);
		}

		public void TestMessageType()
		{
			AssertEquals(JobMessageTypeList.Codes.Export, entryHeader.ManifestWrapper.MessageType);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(JobMessageTypeList.Codes.Import, entryHeader.ManifestWrapper.MessageType);
		}

		public void TestBarrierPort()
		{
			AssertEquals(ZString.Empty, entryHeader.ManifestWrapper.BarrierPort);
			var portOfLoading = "USDNQ";
			var portOfDischarge = "NZCHC";
			declaration.JE_RL_NKPortOfLoading = portOfLoading;
			declaration.JE_RL_NKPortOfArrival = portOfDischarge;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(portOfDischarge, entryHeader.ManifestWrapper.BarrierPort);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(portOfLoading, entryHeader.ManifestWrapper.BarrierPort);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals(ZString.Empty, entryHeader.ManifestWrapper.BarrierPort);
		}

		public void TestBarrierDate()
		{
			AssertEquals(ZDateTime.Empty, entryHeader.ManifestWrapper.BarrierDate);
			var exportDate = new DateTime(2004, 12, 21);
			var arrivalDate = new DateTime(2004, 11, 11);
			declaration.JE_ExportDate = exportDate;
			declaration.JE_DateOfArrival = arrivalDate;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(arrivalDate, entryHeader.ManifestWrapper.BarrierDate);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(exportDate, entryHeader.ManifestWrapper.BarrierDate);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals(ZDateTime.Empty, entryHeader.ManifestWrapper.BarrierDate);
		}

		public void TestMessageMode()
		{
			AssertEquals(JobApplicationCodeList.Codes.TSW, declaration.JE_ApplicationCode);
			AssertEquals(JobApplicationCodeList.Codes.TSW, entryHeader.ManifestWrapper.MessageMode);

			var entryHeader2 = Factory.New<CusEntryHeader>();
			AssertEquals(JobApplicationCodeList.Codes.TSW, entryHeader2.ManifestWrapper.MessageMode);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			entryHeader2.CH_JE = declaration2.PK;
			AssertEquals(JobApplicationCodeList.Codes.CUS, entryHeader2.ManifestWrapper.MessageMode);
		}

		public void TestMessageModeReadOnly()
		{
			Assert(entryHeader.ManifestWrapper.MessageModeInfo.ReadOnly);
		}

		public void TestEntryNo()
		{
			AssertEquals(ZString.Empty, entryHeader.ManifestWrapper.EntryNo);
			var testValue = "80123478";
			entryHeader.EntryNumber = testValue;
			AssertEquals(testValue, entryHeader.ManifestWrapper.EntryNo);
		}

		public void TestStatusDescription()
		{
			AssertEquals(LowValueManifestStatusList.Descriptions.NotSentToCustoms, entryHeader.ManifestWrapper.StatusDescription);
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			AssertEquals(LowValueManifestStatusList.Descriptions.ManifestAccepted, entryHeader.ManifestWrapper.StatusDescription);
		}

		public void TestEDITransmitDate()
		{
			AssertEquals(ZDateTime.Empty, entryHeader.ManifestWrapper.EDITransmitDate);
			var testValue = new DateTime(2005, 4, 5);
			entryHeader.CH_EDITransmitDate = testValue;
			AssertEquals(testValue, entryHeader.ManifestWrapper.EDITransmitDate);
		}

		public void TestAmountPayable()
		{
			AssertEquals(ZDecimal.Zero, entryHeader.ManifestWrapper.AmountPayable);
			var testValue = 343.55m;
			entryHeader.CH_TotalPaid = testValue;
			AssertEquals(testValue, entryHeader.ManifestWrapper.AmountPayable);
		}

		public void TestCarrier()
		{
			AssertEquals(ZGuid.Empty, entryHeader.ManifestWrapper.Carrier);
			var carrier = OrgHeader.New(Factory);
			declaration.JE_OH_ShippingLine = carrier.PK;
			AssertEquals(carrier.PK, entryHeader.ManifestWrapper.Carrier);
		}

		public void TestDeclarationCount()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_DeclarationReference = "M00001001-1";
			declaration1.JE_MessageSubType = "ECI";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "M00001001-2";
			declaration2.JE_MessageSubType = "ECI";
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_DeclarationReference = "M00001001-3";
			declaration3.JE_MessageSubType = "ECI";

			var entryHeaderNoBGMRef = Factory.New<CusEntryHeader>();
			AssertEquals(0, entryHeaderNoBGMRef.ManifestWrapper.DeclarationCount);

			var entryHeaderWithBGMRef = Factory.New<CusEntryHeader>();
			entryHeaderWithBGMRef.CH_BGMReference = "M00001001";
			AssertEquals(3, entryHeaderWithBGMRef.ManifestWrapper.DeclarationCount);
		}

		public void TestArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CusEntryHeaderManifestWrapper(null));
		}

		public void TestDefaultValuesWhenNoDeclaration()
		{
			var wrapper = new CusEntryHeaderManifestWrapper(Factory.New<CusEntryHeader>());
			Assert(wrapper.MasterBill.IsEmpty);
			Assert(wrapper.FlightNo.IsEmpty);
			Assert(wrapper.MessageType.IsEmpty);
			Assert(wrapper.MessageTypeDescription.IsEmpty);
			Assert(wrapper.BarrierPort.IsEmpty);
			Assert(wrapper.BarrierDate.IsEmpty);
			Assert(wrapper.FormattedMasterBill.IsEmpty);
			Assert(wrapper.Carrier.IsEmpty);
			Assert(wrapper.EntryNo.IsEmpty);
			AssertEquals(LowValueManifestStatusList.Descriptions.NotSentToCustoms, wrapper.StatusDescription);
			Assert(wrapper.EDITransmitDate.IsEmpty);
			Assert(wrapper.AmountPayable.IsEmpty);
			Assert(wrapper.DeclarationCount.IsEmpty);
			AssertEquals(JobApplicationCodeList.Codes.TSW, wrapper.MessageMode);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusEntryHeaderManifestWrapper(Factory.New<CusEntryHeader>());

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
		}
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
	}
}
