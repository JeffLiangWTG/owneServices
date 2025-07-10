using System;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class CRLHATest : BIRDHeaderUpdateTest
	{
		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			CRLHA mb20 = new CRLHA();
			mb20.InbondNumber = "12345670";
			mb20.MasterBillNumber = "MB123";
			mb20.Quantity = 10;
			mb20.Unit = "PK";
			mb20.ImmediateTransportationITDate = new ZDate(2009, 6, 1);
			mb20.IssuerCodeOfMasterBillNumber = "AAAA";

			CRLHA hb20WithMB = new CRLHA();
			hb20WithMB.InbondNumber = "12345671";
			hb20WithMB.MasterBillNumber = "MB124";
			hb20WithMB.HouseBillNumber = "HB123";
			hb20WithMB.Quantity = 11;
			hb20WithMB.Unit = "PK";
			hb20WithMB.ImmediateTransportationITDate = new ZDate(2009, 6, 2);
			hb20WithMB.IssuerCodeOfMasterBillNumber = "BBBB";
			hb20WithMB.IssuerCodeOfHouseBillNumber = "CCCC";

			CRLHA hb20WithoutMB = new CRLHA();
			hb20WithoutMB.InbondNumber = "12345672";
			hb20WithoutMB.HouseBillNumber = "HB124";
			hb20WithoutMB.Quantity = 12;
			hb20WithoutMB.Unit = "PK";
			hb20WithoutMB.ImmediateTransportationITDate = new ZDate(2009, 6, 3);
			hb20WithoutMB.IssuerCodeOfHouseBillNumber = "DDDD";

			CRLHA shb20 = new CRLHA();
			shb20.InbondNumber = "12345673";
			shb20.MasterBillNumber = "MB125";
			shb20.HouseBillNumber = "HB126";
			shb20.SubHouseBillNumber = "SB123";
			shb20.Quantity = 11;
			shb20.Unit = "PK";
			shb20.ImmediateTransportationITDate = new ZDate(2009, 6, 2);
			shb20.IssuerCodeOfMasterBillNumber = "EEEE";
			shb20.IssuerCodeOfHouseBillNumber = "FFFF";

			return new IBIRDHeaderRecord[] { mb20, hb20WithMB, hb20WithoutMB, shb20 };
		}

		protected override void PrepareDeclaration(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			base.PrepareDeclaration(declaration, headerRecord);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		}

		protected override EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new CargoReleaseMessageBuilder(declaration.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add, true);
		}

		protected override Type GetTypeOfMessageBlock()
		{
			return typeof(CRLHA);
		}
	}
}
