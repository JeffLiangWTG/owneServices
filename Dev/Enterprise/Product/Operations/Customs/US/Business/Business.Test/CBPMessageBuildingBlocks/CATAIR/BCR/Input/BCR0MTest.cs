using System;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class BCR0MTest : BIRDHeaderUpdateTest
	{
		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			BCR0M mb20 = new BCR0M();
			mb20.MasterBillNumber = "MB123";
			mb20.Quantity = 10;
			mb20.Unit = "PK";
			mb20.IssuerOfMasterBillNumber = "AAAA";

			BCR0M hb20WithMB = new BCR0M();
			hb20WithMB.MasterBillNumber = "MB124";
			hb20WithMB.HouseBillNumber = "HB123";
			hb20WithMB.Quantity = 11;
			hb20WithMB.Unit = "PK";
			hb20WithMB.IssuerOfMasterBillNumber = "BBBB";
			hb20WithMB.IssuerCodeOfHouseBillNumber = "CCCC";

			BCR0M hb20WithoutMB = new BCR0M();
			hb20WithoutMB.HouseBillNumber = "HB124";
			hb20WithoutMB.Quantity = 12;
			hb20WithoutMB.Unit = "PK";
			hb20WithoutMB.IssuerCodeOfHouseBillNumber = "DDDD";

			BCR0M shb20 = new BCR0M();
			shb20.MasterBillNumber = "MB125";
			shb20.HouseBillNumber = "HB126";
			shb20.SubHouseBillNumber = "SB123";
			shb20.Quantity = 11;
			shb20.Unit = "PK";
			shb20.IssuerOfMasterBillNumber = "EEEE";
			shb20.IssuerCodeOfHouseBillNumber = "FFFF";

			return new IBIRDHeaderRecord[] { mb20, hb20WithMB, hb20WithoutMB, shb20 };
		}

		protected override EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new BorderCargoReleaseMessageBuilder(declaration.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add);
		}

		protected override Type GetTypeOfMessageBlock()
		{
			return typeof(BCR0M);
		}

		protected override void PrepareDeclaration(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			base.PrepareDeclaration(declaration, headerRecord);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		}
	}
}
