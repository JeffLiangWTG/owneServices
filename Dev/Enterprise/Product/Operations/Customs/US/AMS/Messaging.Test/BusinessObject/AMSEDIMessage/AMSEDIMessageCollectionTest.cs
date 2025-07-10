using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(eNettMessageSubTypeList))]
namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	[TestedType(typeof(AMSEDIMessageCollection))]
	class AMSEDIMessageCollectionTest : Enterprise.Messaging.Business.EDIMessageCollectionTest
	{
		public void TestOnlyAMSMessagesAreIncluded()
		{
			var header = Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			var moveHeader = Factory.New<Integration.Customs.US.USAMS.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			var messageUnknown = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			messageUnknown.EM_ApplicationCode = "D!@";
			messageUnknown.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			messageUnknown.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
			messageUnknown.EM_LinkUniqueID = moveHeader.PK;
			foreach (ICodeDescription pair in new ApplicationCodeList())
			{
				var message = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
				message.EM_ApplicationCode = pair.Code;
				message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
				message.EM_LinkTable = CusInBondMoveHeaderSchema.Constants.TableName;
				message.EM_LinkUniqueID = moveHeader.PK;
				message.EM_MessageSubType = pair.Code switch
				{
					ApplicationCodeList.Codes.eNett => eNettMessageSubTypeList.Codes.GetNewPayments,
					ApplicationCodeList.Codes.GlobalElectronicPayment => GEPProviderAPICommandList.Codes.CreateADeal,
					ApplicationCodeList.Codes.GlobalElectronicInvoice => EInvoiceAPICommandList.Codes.GenerateCancellationRequest,
					_ => ZString.Empty
				};
			}
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var moveHeaderInDiffFactory = newFactory.Load<Integration.Customs.US.USAMS.ICusInBondMoveHeader>(moveHeader.PK);
			var collection = new AMSEDIMessageCollection((BusinessObject)moveHeaderInDiffFactory);
			collection.Load();
			var messages = collection.ToArray<Enterprise.Messaging.Business.EDIMessage>();
			AssertEquals(1, messages.Length);
			AssertNotNull("USAMS matched", messages.FirstOrDefault(x => x.EM_ApplicationCode == ApplicationCodeList.Codes.USAMS));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AMSEDIMessageCollection(Factory.New<DummyBusinessObject>());
		}
	}
}
