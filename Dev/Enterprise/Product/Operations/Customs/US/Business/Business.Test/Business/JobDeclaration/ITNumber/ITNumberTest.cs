using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ITNumber))]
	public class ITNumberTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIControllerIDProviderMembers()
		{
			var dec = Bill.Declaration;
			IMessageAttacheeInDeclaration msgAttachee = new ITNumber(Bill.ITAndSplitDetails[0]);
			IControllerIDProvider provider = new ITNumber(Bill.ITAndSplitDetails[0]);
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclaration, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);

			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestICargoManifestStatusQueryDataMembers()
		{
			JobDeclaration declaration = Bill.Declaration;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_EntryFilerCode = "ABC";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B89345789";

			ICargoManifestStatusQueryData queryData = new ITNumberCollection(declaration)[0];
			AssertEquals(ZString.Empty, queryData.BillIssuerCode);
			AssertEquals(ZString.Empty, queryData.BillNumber);
			AssertEquals("Reference", "24680", queryData.HumanFriendlyReference);
			AssertEquals("EntryOrInBondNumber", "24680", queryData.EntryOrInBondNumber);
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclaration, queryData.ControllerID);
			AssertEquals(ZString.Empty, queryData.MasterAirWayBillNumber);
			AssertEquals(ZString.Empty, queryData.HouseAirWayBillNumber);
			AssertEquals("JobReferenceNumber", "B89345789", queryData.JobReferenceNumber);
			AssertEquals("BusinessObjectPK", declaration.PK, queryData.BusinessObjectPK);

			EDIMessage message = Factory.New<EDIMessage>();
			queryData.LinkToMessage(message);
			AssertEquals(true, declaration.Messages.Contains(message));
			AssertEquals(declaration, message.EM_LinkedObject);
			AssertEquals(CargoManifestQueryActionType.InBond, queryData.QueryActionType);
			AssertEquals(JobDeclarationSchema.Constants.Prefix, queryData.TableCode);
		}

		public void TestIMessageAttacheeInDeclaration()
		{
			JobDeclaration declaration = Bill.Declaration;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			IMessageAttacheeInDeclaration msgAttachee = new ITNumber(Bill.ITAndSplitDetails[0]);
			AssertEquals("HumanFriendlyReference", Bill.ITNumber, msgAttachee.HumanFriendlyReference);
			AssertEquals(declaration.Messages, msgAttachee.Messages);
			AssertEquals(MessageAttacheeRecordType.ITNumber, msgAttachee.RecordType);
			AssertEquals(MessageAttacheeRecordTypeDescriptions.ITNumber, msgAttachee.RecordTypeDescription);
			AssertEquals("Status", "", msgAttachee.MessageStatus);
			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), msgAttachee.Branch);
			AssertEquals(Core.Constants.TransportModes.Sea, msgAttachee.TransportMode);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ITNumber(Bill.ITAndSplitDetails.AddNew());
		}

		#endregion

		Bill Bill
		{
			get
			{
				if (bill == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					bill = Factory.New<Bill>();
					bill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
					bill.CU_BillNum = "M1";
					bill.ITNumber = "24680";
					declaration.Bills.Add(bill);
				}
				return bill;
			}
		}
		Bill bill;
	}
}
