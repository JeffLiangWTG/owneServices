using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor.MessageProcessors;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business.Testing.UCMP;

sealed class CUSRESMessageProcessorTest : TestCaseWithFactory
{
	public void TestLinkedBusinessObjectMetaData_CUSRES_WithLinkedCusEntryHeader()
	{
		var (incomingMessage, cusEntryHeader, declaration) = TestMessageFactory.Get_CUSRES_WithLinkedCusEntryHeader(Factory);
		var expectedResult = ProcessingResult.New(new LinkedBusinessObjectMetaData(cusEntryHeader.TableName, cusEntryHeader.PK, declaration.JE_GB, ZString.Empty));
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}

	public void TestBranch_CUSRES_WithLinkedCusEntryHeader()
	{
		var (incomingMessage, cusEntryHeader, declaration) = TestMessageFactory.Get_CUSRES_WithLinkedCusEntryHeader(Factory);
		var expectedResult = ProcessingResult.New(declaration.JE_GB);
		MessageProcessorTestHelper.AssertBranch(incomingMessage, expectedResult);
	}

	public void TestSerializationKeysResult_CUSRES_WithLinkedCusEntryHeader()
	{
		var (incomingMessage, cusEntryHeader, declaration) = TestMessageFactory.Get_CUSRES_WithLinkedCusEntryHeader(Factory);
		var expectedResult = ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>()
		{
			declaration.JE_DeclarationReference.ToString(),
			cusEntryHeader.CH_BGMReference.ToString(),
		}));
		MessageProcessorTestHelper.AssertSerializationKeysResult(incomingMessage, expectedResult);
	}

	public void TestLinkedBusinessObjectMetaData_CUSRES_WithLinkedAsycudaManifest()
	{
		var manifestHeader = Factory.New<AsycudaManifestHeader>();
		manifestHeader.AMA_MasterBill = "MB01234567";
		var incomingMessage = TestMessageFactory.Get_CUSRES_WithLinkedAsycudaManifestHeader(manifestHeader);
		var expectedResult = ProcessingResult.New(new LinkedBusinessObjectMetaData(manifestHeader.TableName, manifestHeader.PK, manifestHeader.AMA_GB, ZString.Empty));
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}
	
	public void TestBranch_CUSRES_WithLinkedAsycudaManifest()
	{
		var manifestHeader = Factory.New<AsycudaManifestHeader>();
		manifestHeader.AMA_MasterBill = "MB01234567";
		var incomingMessage = TestMessageFactory.Get_CUSRES_WithLinkedAsycudaManifestHeader(manifestHeader);
		var expectedResult = ProcessingResult.New(manifestHeader.AMA_GB);
		MessageProcessorTestHelper.AssertBranch(incomingMessage, expectedResult);
	}

	public void TestSerializationKeysResult_CUSRES_WithLinkedAsycudaManifest()
	{
		var manifestHeader = Factory.New<AsycudaManifestHeader>();
		manifestHeader.AMA_MasterBill = "MB01234567";
		var incomingMessage = TestMessageFactory.Get_CUSRES_WithLinkedAsycudaManifestHeader(manifestHeader);
		var expectedResult = ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
		{
			manifestHeader.AMA_JobReference.ToString()
		}));
		MessageProcessorTestHelper.AssertSerializationKeysResult(incomingMessage, expectedResult);
	}

	public void TestLinkedBusinessObjectMetaData_CUSRES_WithLinkedManifestHeader()
	{
		var manifestHeader = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
		var incomingMessage = TestMessageFactory.Get_CUSRES_WithLinkedAsycudaManifestHeader(manifestHeader);
		var expectedResult = ProcessingResult.New(new LinkedBusinessObjectMetaData(manifestHeader.TableName, manifestHeader.PK, manifestHeader.AMA_GB, ZString.Empty));
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}

	public void TestBranch_CUSRES_WithLinkedManifestHeader()
	{
		var manifestHeader = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
		var incomingMessage = TestMessageFactory.Get_CUSRES_WithLinkedAsycudaManifestHeader(manifestHeader);
		var expectedResult = ProcessingResult.New(manifestHeader.AMA_GB);
		MessageProcessorTestHelper.AssertBranch(incomingMessage, expectedResult);
	}

	public void TestSerializationKeysResult_CUSRES_WithLinkedManifestHeader()
	{
		var manifestHeader = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
		var incomingMessage = TestMessageFactory.Get_CUSRES_WithLinkedAsycudaManifestHeader(manifestHeader);
		var expectedResult = ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
		{
			manifestHeader.AMA_JobReference.ToString()
		}));

		MessageProcessorTestHelper.AssertSerializationKeysResult(incomingMessage, expectedResult);
	}

	public void TestLinkedBusinessObjectMetaData_CUSRES_WithLinkedManifestBill()
	{
		var manifestHeader = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
		var bill = manifestHeader.Bills.AddNew();
		bill.ABL_BillNumber = "HB-MB01234567";

		TestMessageFactory.GetOutgoingCUSRESEDIMessage(bill);
		var incomingMessage = TestMessageFactory.GetIncomingCUSRESEDIMessage(Factory);
		Factory.Save();

		var expectedResult = ProcessingResult.New(new LinkedBusinessObjectMetaData(bill.TableName, bill.PK, manifestHeader.AMA_GB, ZString.Empty));

		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}

	public void TestBranch_CUSRES_WithLinkedManifestBill()
	{
		var manifestHeader = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
		var bill = manifestHeader.Bills.AddNew();
		bill.ABL_BillNumber = "HB-MB01234567";

		TestMessageFactory.GetOutgoingCUSRESEDIMessage(bill);
		var incomingMessage = TestMessageFactory.GetIncomingCUSRESEDIMessage(Factory);
		Factory.Save();

		var expectedResult = ProcessingResult.New(manifestHeader.AMA_GB);

		MessageProcessorTestHelper.AssertBranch(incomingMessage, expectedResult);
	}

	public void TestSerializationKeysResult_CUSRES_WithLinkedManifestBill()
	{
		var manifestHeader = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
		var bill = manifestHeader.Bills.AddNew();
		bill.ABL_BillNumber = "HB-MB01234567";

		TestMessageFactory.GetOutgoingCUSRESEDIMessage(bill);
		var incomingMessage = TestMessageFactory.GetIncomingCUSRESEDIMessage(Factory);
		Factory.Save();

		var expectedResult = ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
		{
			manifestHeader.AMA_JobReference.ToString(),
			"HB-MB01234567"
		}));

		MessageProcessorTestHelper.AssertSerializationKeysResult(incomingMessage, expectedResult);
	}

	public void TestLinkedBusinessObjectMetaData_CUSRES_WithNoLinkedObject()
	{
		var incomingMessage = TestMessageFactory.GetIncomingCUSRESEDIMessage(Factory, "TODO");
		var expectedResult = ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, ZACApplicationTypeMessageProcessor.GetMessageProcessorCannotProcessMessage("CUSRES", incomingMessage));
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}

	public void TestLinkedBusinessObjectMetaData_CUSRES_REQDOC()
	{
		var jobReference = "01020304JSA20160708000064";
		var (incomingMessage, cusEntryHeader, declaration) = TestMessageFactory.Get_CUSRES_REQDOC(Factory, jobReference);
		var expectedResult = ProcessingResult.New(new LinkedBusinessObjectMetaData(cusEntryHeader.TableName, cusEntryHeader.PK, incomingMessage.EM_GB, ZString.Empty));
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}

	public void TestBranch_CUSRES_REQDOC()
	{
		var jobReference = "01020304JSA20160708000064";
		var (incomingMessage, cusEntryHeader, declaration) = TestMessageFactory.Get_CUSRES_REQDOC(Factory, jobReference);
		var expectedResult = ProcessingResult.New(incomingMessage.EM_GB);
		MessageProcessorTestHelper.AssertBranch(incomingMessage, expectedResult);
	}

	public void TestSerializationKeysResult_CUSRES_REQDOC()
	{
		var jobReference = "01020304JSA20160708000064";
		var (incomingMessage, cusEntryHeader, declaration) = TestMessageFactory.Get_CUSRES_REQDOC(Factory, jobReference);
		var expectedResult = ProcessingResult.New(new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
		{
			declaration.JE_DeclarationReference.ToString(),
			jobReference
		}));
		MessageProcessorTestHelper.AssertSerializationKeysResult(incomingMessage, expectedResult);
	}

	public void TestLinkedBusinessObjectMetaData_CUSRES_REQDOC_WithNoLinkedObject()
	{
		var incomingMessage = TestMessageFactory.GetIncomingMessage<CUSRES_REQDOCEDIMessage>(Factory, "N/A");
		var expectedResult = ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, ZACApplicationTypeMessageProcessor.GetUnableToFindTheLinkedJobMessage("CUSRES-REQDOC", incomingMessage));
		MessageProcessorTestHelper.AssertLinkedBusinessObjectMetaData(incomingMessage, expectedResult);
	}
}
