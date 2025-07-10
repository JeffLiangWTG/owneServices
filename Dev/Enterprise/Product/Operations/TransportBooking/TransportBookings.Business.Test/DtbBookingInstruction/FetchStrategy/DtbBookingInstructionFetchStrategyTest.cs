namespace Enterprise.TransportBookings.Business.Testing
{
	//	public class DtbBookingInstructionFetchStrategyTest : TestCaseWithFactory
	//    {
	//        #region TestFetchForLoad

	//        public void TestFetchForLoad()
	//        {
	//            var freshFactory = new BusinessObjectFactory();
	//            freshFactory.ResetDatabaseLoadCount();

	//            string poke;
	//            var instructions = freshFactory.Load<DtbBookingInstruction>(new ZQuery(DtbBookingInstructionSchema.PK, CreateTransportBookingInstructions()));
	//            AssertNotEquals("Precondition", 0, instructions.Length);

	//            foreach (var instruction in instructions)
	//            {
	//                poke = instruction.Address.Address.OA_Code;
	//                poke = instruction.Confirmations[0].KK_ReceivedBy;
	//                poke = instruction.Confirmations[1].KK_ReceivedBy;
	//            }

	//            // JobDocAddress: 1
	//            // DtbBookingConfirmation: 1
	//            // DtbBookingInstruction: 1
	//            // DtbBookingInstructionPkgDivot: 1
	//            // OrgAddress: 1

	//            // Hits: 5/5

	//            AssertMaxDbHits(5, freshFactory);
	//            AssertEquals("Should hit the db exactly 5 times, why are there less?", 5, freshFactory.DatabaseLoadCount);

	//        }

	//        #endregion

	//        #region CreateTransportBookingInstructions

	//        List<ZGuid> CreateTransportBookingInstructions()
	//        {
	//            var result = new List<ZGuid>();

	//            var importTemplate = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
	//            Helper.AddInstructionToTemplate(importTemplate, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
	//            Helper.AddInstructionToTemplate(importTemplate, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
	//            Helper.AddInstructionToTemplate(importTemplate, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

	//            var exportTemplate = Helper.CreateTransportBookingTemplate("EFCL", "FCL Export", Constants.CartageDirection.Import);
	//            Helper.AddInstructionToTemplate(exportTemplate, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.PickUp);
	//            Helper.AddInstructionToTemplate(exportTemplate, OrganisationTypesList.Codes.CNR, InstructionTypes.Codes.PickUp);
	//            Helper.AddInstructionToTemplate(exportTemplate, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.Delivery);

	//            for (int i = 0; i < 10; i++)
	//            {
	//                var transportCo = Factory.NewWithValidTestData<OrgHeader>();
	//                var cto = Factory.NewWithValidTestData<OrgHeader>();
	//                var cne = Factory.NewWithValidTestData<OrgHeader>();
	//                var cyd = Factory.NewWithValidTestData<OrgHeader>();

	//                var booking = Helper.CreateTransportBookingMovement();
	//                movement.Address.E2_OA_Address = transportCo.MainAddress.PK;
	//                movement.KM_KT_NKBookingTemplate = importTemplate.KT_Code;

	//                var ctoInstruction = booking.Instructions[0];
	//                var cneInstruction = booking.Instructions[1];
	//                var cydInstruction = booking.Instructions[2];

	//                ctoInstruction.Address.E2_OA_Address = cto.MainAddress.PK;
	//                cneInstruction.Address.E2_OA_Address = cne.MainAddress.PK;
	//                cydInstruction.Address.E2_OA_Address = cyd.MainAddress.PK;

	//                ctoInstruction.Confirmations.AddNew();
	//                ctoInstruction.Confirmations.AddNew();
	//                cneInstruction.Confirmations.AddNew();
	//                cneInstruction.Confirmations.AddNew();
	//                cydInstruction.Confirmations.AddNew();
	//                cydInstruction.Confirmations.AddNew();

	//                // Add Packages

	//                result.Add(ctoInstruction.PK);
	//                result.Add(cneInstruction.PK);
	//                result.Add(cydInstruction.PK);
	//            }

	//            Factory.Save();

	//            return result;
	//        }

	//        #endregion

	//        #region Helper

	//        TransportBookingTestHelper Helper
	//        {
	//            get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
	//        }
	//        TransportBookingTestHelper helper;

	//        #endregion
	//    }
}
