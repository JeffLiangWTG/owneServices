using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.DataModel.Common;
using Common.Logging.Simple;
using Rhino.Mocks;
using CargoWise.eHub.Products.OceanCarrierMessaging.SymmetricalMessaging.Helpers;
using System.Xml.Linq;
using System.Linq;
using Rhino.Mocks.Constraints;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.SymmetricalMessaging.Tests
{
    [TestClass]
    public class OrchestrationHelperTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetTransformationSet_ForOrchestrationInbound()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);

            //Case Party A sends X request to Party B through eHub - eHub Incoming
            var transformation1 = OrchestrationHelper.GetTransformationSet("A", "X_A", 0, new NoOpLogger());
            Assert.AreEqual(transformation1.SenderPK, "cb6a3cff-2176-49d8-adc3-80d64d927391");
            Assert.AreEqual(transformation1.RecipientPK, "a7039a74-a089-4e57-b477-cbdffd3f6d98");
            Assert.AreEqual(transformation1.SourceMessageType, "X_A");
            Assert.AreEqual(transformation1.DestinationMessageType, "I_Rq");
            Assert.AreEqual(transformation1.TransformationSetName, "Request X for Party A to Internal Request");
            Assert.IsTrue(transformation1.Transformations
                .Select(m => m.Item2)
                .SequenceEqual(new List<string>() 
                    {
                        "X_A_2_I_1",
                        "I_1_2_I_2",
                        "I_2_2_I_3",
                        "I_3_2_I_Rq"
                    }
                )
            );
            Assert.AreEqual(transformation1.TransformationsString, "0->X_A_2_I_1; 1->I_1_2_I_2; 2->I_2_2_I_3; 3->I_3_2_I_Rq");

            //Case Party B sends Y response to Party A through eHub - eHub Incoming
            var transformation2 = OrchestrationHelper.GetTransformationSet("B", "Y_B", 0, new NoOpLogger());
            Assert.AreEqual(transformation2.SenderPK, "95a16f4b-9920-442a-bcdb-0f52ba6a2eb6");
            Assert.AreEqual(transformation2.RecipientPK, "a7039a74-a089-4e57-b477-cbdffd3f6d98");
            Assert.AreEqual(transformation2.SourceMessageType, "Y_B");
            Assert.AreEqual(transformation2.DestinationMessageType, "I_Rs");
            Assert.AreEqual(transformation2.TransformationSetName, "Response Y for Party B to Internal Response");
            Assert.IsTrue(transformation2.Transformations
                .Select(m => m.Item2)
                .SequenceEqual(new List<string>() 
                    {
                        "Y_B_2_I_Rs"
                    }
                )
            );
            Assert.AreEqual(transformation2.TransformationsString, "0->Y_B_2_I_Rs");

        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetTransformationSet_ForOrchestrationOutbound()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);

            //Case Party A sends X request to Party B through eHub - eHub Outgoing
            var transformation1 = OrchestrationHelper.GetTransformationSet("B", "I_Rq", 1, new NoOpLogger());
            Assert.AreEqual(transformation1.SenderPK, "a7039a74-a089-4e57-b477-cbdffd3f6d98");
            Assert.AreEqual(transformation1.RecipientPK, "95a16f4b-9920-442a-bcdb-0f52ba6a2eb6");
            Assert.AreEqual(transformation1.SourceMessageType, "I_Rq");
            Assert.AreEqual(transformation1.DestinationMessageType, "X_B");
            Assert.AreEqual(transformation1.TransformationSetName, "Internal Request to Request X for Party B");
            Assert.IsTrue(transformation1.Transformations
                .Select(m => m.Item2)
                .SequenceEqual(new List<string>() 
                    {
                        "I_Rq_2_X_B"
                    }
                )
            );
            Assert.AreEqual(transformation1.TransformationsString, "0->I_Rq_2_X_B");

            //Case Party B sends Y response to Party A through eHub - eHub Outgoing
            var transformation2 = OrchestrationHelper.GetTransformationSet("A", "I_Rs", 1, new NoOpLogger());
            Assert.AreEqual(transformation2.SenderPK, "a7039a74-a089-4e57-b477-cbdffd3f6d98");
            Assert.AreEqual(transformation2.RecipientPK, "cb6a3cff-2176-49d8-adc3-80d64d927391");
            Assert.AreEqual(transformation2.SourceMessageType, "I_Rs");
            Assert.AreEqual(transformation2.DestinationMessageType, "Y_A");
            Assert.AreEqual(transformation2.TransformationSetName, "Internal Response to Response Y for Party A");
            Assert.IsTrue(transformation2.Transformations
                .Select(m => m.Item2)
                .SequenceEqual(new List<string>() 
                    {
                        "I_Rs_2_Y_A"
                    }
                )
            );
            Assert.AreEqual(transformation2.TransformationsString, "0->I_Rs_2_Y_A");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetTransformationSet_ThatDoesNotExist()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);

            //Case Party B sends Y response to Party A through eHub - eHub Outgoing
            var transformation = OrchestrationHelper.GetTransformationSet("X", "X_X", 1, new NoOpLogger());
            Assert.AreEqual(transformation.SenderPK, "");
            Assert.AreEqual(transformation.RecipientPK, "");
            Assert.AreEqual(transformation.SourceMessageType, "");
            Assert.AreEqual(transformation.DestinationMessageType, "");
            Assert.AreEqual(transformation.TransformationSetName, "Not Found");
            Assert.IsNull(transformation.Transformations);
            Assert.AreEqual(transformation.Message, "Unable to find transformation set for the following combination. Client : X; Message Type : X_X; Direction : 1");
            Assert.AreEqual(transformation.TransformationsString, "");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetTransformationSet_GetTransformationType()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            SetupTestData(mockeHubTransactionsContext);

            //Case Party A sends X request to Party B through eHub - eHub Incoming
            var transformation1 = OrchestrationHelper.GetTransformationSet("A", "X_A", 0, new NoOpLogger());
            Assert.AreEqual(transformation1.GetTransformationType(0), "X_A_2_I_1");
            Assert.AreEqual(transformation1.GetTransformationType(1), "I_1_2_I_2");
            Assert.AreEqual(transformation1.GetTransformationType(2), "I_2_2_I_3");
            Assert.AreEqual(transformation1.GetTransformationType(3), "I_3_2_I_Rq");
            Assert.AreEqual(transformation1.GetTransformationType(4), "");

        }

        private static void SetupTestData(eHubTransactionsContext mockeHubTransactionsContext)
        {

            OrchestrationHelper.GetContext = () => mockeHubTransactionsContext;

            var A = new eHubClient() { CC_PK = Guid.Parse("cb6a3cff-2176-49d8-adc3-80d64d927391"), CC_ID = "A", CC_FriendlyName = "Party A" };
            var B = new eHubClient() { CC_PK = Guid.Parse("95a16f4b-9920-442a-bcdb-0f52ba6a2eb6"), CC_ID = "B", CC_FriendlyName = "Party B" };
            var SI = new eHubClient() { CC_PK = Guid.Parse("a7039a74-a089-4e57-b477-cbdffd3f6d98"), CC_ID = "SHIPPING_INSTRUCTION", CC_FriendlyName = "SHIPPING_INSTRUCTION" };

            var DT_X_A = new eHubMessageType() { DT_Code = "X_A" }; //Request X in format A
            var DT_X_B = new eHubMessageType() { DT_Code = "X_B" }; //Request X in format B
            var DT_I_Rq = new eHubMessageType() { DT_Code = "I_Rq" }; //Internal Request
            var DT_I_Rs = new eHubMessageType() { DT_Code = "I_Rs" }; //Internal Response
            var DT_Y_A = new eHubMessageType() { DT_Code = "Y_A" }; //Response Y in format A
            var DT_Y_B = new eHubMessageType() { DT_Code = "Y_B" }; //Response Y in format B

            //To reuse current transformations. 
            //We currently have transformations in multiple steps to and from internal message type
            //and we can reuse them
            //that means we cannot assume the order in transformationSet to show the direction of message
            var DT_I_1 = new eHubMessageType() { DT_Code = "T_1" }; //Intermediary Message 1
            var DT_I_2 = new eHubMessageType() { DT_Code = "T_2" }; //Intermediary Message 2
            var DT_I_3 = new eHubMessageType() { DT_Code = "T_3" }; //Intermediary Message 3

            //Case Party A sends request X in format X_A to Party B in format X_B through eHub in format I_Rq
            var TT_X_A_2_I_1 = new eHubTransformationType() { eHubMessageType_Source = DT_X_A, eHubMessageType_Target = DT_I_1, TT_TransformationType = "X_A_2_I_1" };
            var TT_I_1_2_I_2 = new eHubTransformationType() { eHubMessageType_Source = DT_I_1, eHubMessageType_Target = DT_I_2, TT_TransformationType = "I_1_2_I_2" };
            var TT_I_2_2_I_3 = new eHubTransformationType() { eHubMessageType_Source = DT_I_2, eHubMessageType_Target = DT_I_3, TT_TransformationType = "I_2_2_I_3" };
            var TT_I_3_2_I_Rq = new eHubTransformationType() { eHubMessageType_Source = DT_I_3, eHubMessageType_Target = DT_I_Rq, TT_TransformationType = "I_3_2_I_Rq" };
            var TT_I_Rq_2_X_B = new eHubTransformationType() { eHubMessageType_Source = DT_I_Rq, eHubMessageType_Target = DT_X_B, TT_TransformationType = "I_Rq_2_X_B" };

            //Case eHub rejects the message
            var TT_I_Rq_2_I_Rs = new eHubTransformationType() { eHubMessageType_Source = DT_I_Rq, eHubMessageType_Target = DT_I_Rs, TT_TransformationType = "I_Rq_2_I_Rs" };

            //Case Party B sends response Y in format Y_B to Party A in format Y_A through eHub in format I_Rs
            var TT_Y_B_2_I_Rs = new eHubTransformationType() { eHubMessageType_Source = DT_Y_B, eHubMessageType_Target = DT_I_Rs, TT_TransformationType = "Y_B_2_I_Rs" };
            var TT_I_Rs_2_Y_A = new eHubTransformationType() { eHubMessageType_Source = DT_I_Rs, eHubMessageType_Target = DT_Y_A, TT_TransformationType = "I_Rs_2_Y_A" };

            var TS_X_A_2_I_Rq = new eHubTransformationSet() { TS_Name = "Request X for Party A to Internal Request", eHubClient_Sender = A, eHubClient_Recipient = SI, eHubMessageType = DT_X_A };
            var TM_X_A_2_I_1 = new eHubTransformationMapping() { eHubTransformationSet = TS_X_A_2_I_Rq, eHubTransformationType = TT_X_A_2_I_1, TM_Order = 0 };
            var TM_I_1_2_I_2 = new eHubTransformationMapping() { eHubTransformationSet = TS_X_A_2_I_Rq, eHubTransformationType = TT_I_1_2_I_2, TM_Order = 1 };
            var TM_I_2_2_I_3 = new eHubTransformationMapping() { eHubTransformationSet = TS_X_A_2_I_Rq, eHubTransformationType = TT_I_2_2_I_3, TM_Order = 2 };
            var TM_I_3_2_I_Rq = new eHubTransformationMapping() { eHubTransformationSet = TS_X_A_2_I_Rq, eHubTransformationType = TT_I_3_2_I_Rq, TM_Order = 3 };
            TS_X_A_2_I_Rq.eHubTransformationMappings.Add(TM_X_A_2_I_1);
            TS_X_A_2_I_Rq.eHubTransformationMappings.Add(TM_I_1_2_I_2);
            TS_X_A_2_I_Rq.eHubTransformationMappings.Add(TM_I_2_2_I_3);
            TS_X_A_2_I_Rq.eHubTransformationMappings.Add(TM_I_3_2_I_Rq);

            var TS_I_Rq_2_X_B = new eHubTransformationSet() { TS_Name = "Internal Request to Request X for Party B", eHubClient_Sender = SI, eHubClient_Recipient = B, eHubMessageType = DT_I_Rq };
            var TM_I_Rq_2_X_B = new eHubTransformationMapping() { eHubTransformationSet = TS_I_Rq_2_X_B, eHubTransformationType = TT_I_Rq_2_X_B, TM_Order = 0 };
            TS_I_Rq_2_X_B.eHubTransformationMappings.Add(TM_I_Rq_2_X_B);

            var TS_Y_B_2_I_Rs = new eHubTransformationSet() { TS_Name = "Response Y for Party B to Internal Response", eHubClient_Sender = B, eHubClient_Recipient = SI, eHubMessageType = DT_Y_B };
            var TM_Y_B_2_I_Rs = new eHubTransformationMapping() { eHubTransformationSet = TS_Y_B_2_I_Rs, eHubTransformationType = TT_Y_B_2_I_Rs, TM_Order = 0 };
            TS_Y_B_2_I_Rs.eHubTransformationMappings.Add(TM_Y_B_2_I_Rs);

            var TS_I_Rs_2_Y_A = new eHubTransformationSet() { TS_Name = "Internal Response to Response Y for Party A", eHubClient_Sender = SI, eHubClient_Recipient = A, eHubMessageType = DT_I_Rs };
            var TM_I_Rs_2_Y_A = new eHubTransformationMapping() { eHubTransformationSet = TS_I_Rs_2_Y_A, eHubTransformationType = TT_I_Rs_2_Y_A, TM_Order = 0 };
            TS_I_Rs_2_Y_A.eHubTransformationMappings.Add(TM_I_Rs_2_Y_A);

            var messageTypes = new TestDbSet<eHubMessageType>() 
            { 
                DT_X_A, 
                DT_X_B, 
                DT_I_Rq, 
                DT_I_Rs, 
                DT_Y_A, 
                DT_Y_B 
            };
            var transformationTypes = new TestDbSet<eHubTransformationType>() 
            { 
                TT_X_A_2_I_1, 
                TT_I_1_2_I_2,
                TT_I_2_2_I_3,
                TT_I_3_2_I_Rq,
                TT_I_Rq_2_X_B, 
                TT_I_Rq_2_I_Rs, 
                TT_Y_B_2_I_Rs, 
                TT_I_Rs_2_Y_A 
            };
            var transformationSets = new TestDbSet<eHubTransformationSet>() 
            { 
                TS_X_A_2_I_Rq, 
                TS_I_Rq_2_X_B, 
                TS_Y_B_2_I_Rs, 
                TS_I_Rs_2_Y_A 
            };
            var mappings = new TestDbSet<eHubTransformationMapping>() 
            { 
                TM_X_A_2_I_1, 
                TM_I_1_2_I_2,
                TM_I_2_2_I_3,
                TM_I_3_2_I_Rq,
                TM_I_Rq_2_X_B, 
                TM_Y_B_2_I_Rs, 
                TM_I_Rs_2_Y_A 
            };

            mockeHubTransactionsContext.Stub(x => x.eHubTransformationTypes).Return(transformationTypes);
            mockeHubTransactionsContext.Stub(x => x.eHubTransformationSets).Return(transformationSets);
        }
    }
}
