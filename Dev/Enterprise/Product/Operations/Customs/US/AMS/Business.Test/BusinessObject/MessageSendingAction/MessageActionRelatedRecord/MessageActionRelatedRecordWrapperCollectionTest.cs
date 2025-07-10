using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(MessageActionRelatedRecordWrapperCollection))]
	sealed class MessageActionRelatedRecordWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageActionRelatedRecordWrapperCollection>
	{
		public void TestSynchroniseWithRelatedRecordsCountChanged()
		{
			var company = Factory.Load<MasterFiles.Business.GlbCompany>(MasterFiles.Business.GlbCompany.CurrentCompany.PK);
			company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(MasterFiles.Business.OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Related Message Records", 0, Header.MessageAttacheesRelatedRecords.Count);

			var bill = Header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var sendingObject = new MessageSendingObject(moveDetail, ActionCode.Creating);
			var builder = new ACEAMSMessageBuilder(sendingObject, ActionCode.Creating);
			var message1 = builder.PopulateMessage();
			Factory.Save();

			AssertEquals("Related Message Records", 1, Header.MessageAttacheesRelatedRecords.Count);
			AssertNotNull(Header.MessageAttacheesRelatedRecords.GetElementWrapping(AMSMovementHeader));
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, WrapperCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, WrapperCollection.AllowRemove);
		}

		public void TestReadonly()
		{
			AssertEquals(true, WrapperCollection.ReadOnly);
		}

		public void TestSorting()
		{
			AssertEquals(3, WrapperCollection.Count);
			AssertEquals(SubApplicationCodeList.Descriptions.AMS, WrapperCollection[0].relatedRecord.RecordTypeDescription);
			AssertEquals(SubApplicationCodeList.Descriptions.PermitToTransfer, WrapperCollection[1].relatedRecord.RecordTypeDescription);
			AssertEquals(SubApplicationCodeList.Descriptions.MasterInBond, WrapperCollection[2].relatedRecord.RecordTypeDescription);
		}

		protected override MessageActionRelatedRecordWrapperCollection GetCollectionToTest()
		{
			return WrapperCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageActionRelatedRecordWrapper(AMSMovementHeader);
		}

		MessageActionRelatedRecordWrapperCollection WrapperCollection
		{
			get
			{
				if (wrapperCollection == null)
				{
					AssertNotNull(AMSMovementHeader);
					AssertNotNull(PTTMovementHeader);
					AssertNotNull(INBMovementHeader);
					wrapperCollection = new MessageActionRelatedRecordWrapperCollection(Header, delegate
					{ return ((IMessageActionHeader)Header).MessageAttachees; });
				}
				return wrapperCollection;
			}
		}
		MessageActionRelatedRecordWrapperCollection wrapperCollection;

		CusInBondMoveHeader AMSMovementHeader
		{
			get { return movementHeader ?? (movementHeader = Header.MovementHeader); }
		}
		CusInBondMoveHeader movementHeader;

		CusInBondMoveHeader PTTMovementHeader
		{
			get { return pttMovementHeader ?? (pttMovementHeader = Header.PTTMovements.AddNew()); }
		}
		CusInBondMoveHeader pttMovementHeader;

		CusInBondMoveHeader INBMovementHeader
		{
			get { return inbMovementHeader ?? (inbMovementHeader = Header.InBondMovementHeaders.AddNew()); }
		}
		CusInBondMoveHeader inbMovementHeader;

		CusInBondHeader Header
		{
			get
			{
				if (header == null)
				{
					var consol = Factory.New<ForwardingConsol>();
					header = Factory.New<CusInBondHeader>();
					header.BH_ParentID = consol.PK;
					header.BH_ParentTableCode = consol.TablePrefix;
				}
				return header;
			}
		}
		CusInBondHeader header;
	}
}
