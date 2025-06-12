using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.OCMHelperTest
{
  [TestClass]
  public class OCMHelperTest
  {
    [TestMethod]
    public void TestIsMultiPickupDropOff()
    {
      InitialiseCodeMapsTestingContext();

      var helper = new OCMHelper();
      helper.SetTestCodeMapping(new CodeMapperTest(transformAccessor));

      Assert.AreEqual("TRUE", helper.IsMultiPickup("INTTRA", "INTT"));
      Assert.AreEqual("TRUE", helper.IsMultiDropOff("INTTRA", "INTT"));
      Assert.AreEqual("FALSE", helper.IsMultiPickup("xxxx", "xxxx"));
      Assert.AreEqual("FALSE", helper.IsMultiDropOff("ABC", "INTT"));
    }

    [TestMethod]
    public void TestGetServiceProvider()
    {
      var helper = new OCMHelper();
      Assert.AreEqual("", helper.GetServiceProvider(""));
      Assert.AreEqual("INTTRA", helper.GetServiceProvider("INTTRA"));
      Assert.AreEqual("INTTRA", helper.GetServiceProvider("INTTRA_BK1"));
      Assert.AreEqual("HAPAG_LLOYD", helper.GetServiceProvider("HAPAG_LLOYD"));
      Assert.AreEqual("HAPAG_LLOYD", helper.GetServiceProvider("HAPAG_LLOYD_BK1"));
      Assert.AreEqual("HAPAG_LLOYD", helper.GetServiceProvider("HAPAG_LLOYD_SI1_BK1"));
    }

    [TestMethod]
    public void TestIsCoLoad()
    {
      InitialiseCodeMapsTestingContext();

      var helper = new OCMHelper();
      helper.SetTestCodeMapping(new CodeMapperTest(transformAccessor));

      Assert.AreEqual("FALSE", helper.IsCoLoad(""));
      Assert.AreEqual("TRUE", helper.IsCoLoad("CLD"));
      Assert.AreEqual("FALSE", helper.IsCoLoad("AGT"));
      Assert.AreEqual("FALSE", helper.IsCoLoad("XXX"));
    }

    [TestMethod]
    public void TestIsValidContainerNumber()
    {
      var helper = new OCMHelper();

      Assert.IsTrue(helper.IsValidContainerNumber("ABCD1234567"));
      Assert.IsFalse(helper.IsValidContainerNumber("00001234567"));
      Assert.IsFalse(helper.IsValidContainerNumber("ABCDE234567"));
    }

    [TestMethod]
    public void TestGetUEventProcessLog()
    {
      var testData = @"blah blah blah blah blah blah blah blah blah
Populating blah blah blah blah ...
[*Error 001.*]

Populating blah blah blah blah ...
[*Error 002.*]

Populating blah blah blah blah ...

Error: [*Error ****003****.*] blah blah blah

Populating blah blah blah blah ...
[**]

Error: [**] blah blah blah

UniversalShipment Updated.";

      var helper = new OCMHelper();
      var actualResult = helper.GetUEventProcessLog(testData);

      var expectedResult = "Error 001.\nError 002.\nError ****003****.";

      Assert.AreEqual(expectedResult, actualResult);
    }

    [TestMethod]
    public void TestGetAttachmentInBinary()
    {
      var helper = new OCMHelper();

      Assert.AreEqual("", helper.GetAttachmentInBinary(""));
      Assert.AreEqual("", helper.GetAttachmentInBinary("12345"));
    }

    [TestMethod]
    public void TestGetTransportMode()
    {
      InitialiseCodeMapsTestingContext();

      var helper = new OCMHelper();

      Assert.AreEqual(string.Empty, helper.GetTransportMode(string.Empty, string.Empty));
      Assert.AreEqual(TransportModeConstant.Sea, helper.GetTransportMode(TransportModeConstant.Sea, string.Empty));
      Assert.AreEqual(TransportModeConstant.Rail, helper.GetTransportMode(TransportModeConstant.Rail, string.Empty));
      Assert.AreEqual(TransportModeConstant.RailRoad, helper.GetTransportMode(TransportModeConstant.Rail, TransportModeConstant.Road));
      Assert.AreEqual(TransportModeConstant.Road, helper.GetTransportMode(TransportModeConstant.Road, string.Empty));
      Assert.AreEqual(TransportModeConstant.InlandWaterway, helper.GetTransportMode(TransportModeConstant.InlandWaterway, string.Empty));
      Assert.AreEqual(TransportModeConstant.RailWater, helper.GetTransportMode(TransportModeConstant.InlandWaterway, TransportModeConstant.Rail));
      Assert.AreEqual(TransportModeConstant.RoadWater, helper.GetTransportMode(TransportModeConstant.InlandWaterway, TransportModeConstant.Road));
    }

    [TestMethod]
    public void TestGetPayableElseWhere()
    {
      InitialiseCodeMapsTestingContext();

      var helper = new OCMHelper();
      helper.SetTestCodeMapping(new CodeMapperTest(transformAccessor));

      Assert.AreEqual("ELSEWHERE", helper.GetPayableElseWhereOutputCode("CMACGM_xxx"));
      Assert.AreEqual("ELSEWHERE", helper.GetPayableElseWhereDescription("CMACGM_xxx"));

      Assert.AreEqual(string.Empty, helper.GetPayableElseWhereOutputCode("xxx"));
      Assert.AreEqual(string.Empty, helper.GetPayableElseWhereDescription("xxx"));
    }



    [TestMethod]
    public void TestGetAdministrativeRegion()
    {
      InitialiseCodeMapsTestingContext();
      var helper = new OCMHelper();

      Assert.AreEqual(string.Empty, helper.GetAdministrativeRegion(string.Empty));
      Assert.AreEqual(string.Empty, helper.GetAdministrativeRegion("NZAKL"));
      Assert.AreEqual("Northern Ireland", helper.GetAdministrativeRegion("GBLIS"));
    }

    [TestMethod]
    public void TestGetShippingLineSCAC()
    {
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectShippingLineStandardCode", "@stdCode", "@cw1Code", "C1NT")).Return("NPLX").Repeat.Once();

      var helper = new OCMHelper();
      helper.SetTestCodeMapping(mockCodeMapper);

      Assert.AreEqual("NPLX", helper.GetShippingLineSCAC("C1NT"));
      Assert.AreEqual(string.Empty, helper.GetShippingLineSCAC(string.Empty));
      Assert.AreEqual(string.Empty, helper.GetShippingLineSCAC(null));
    }

    void InitialiseCodeMapsTestingContext()
    {
      CodeMapsTestingContext ctx = new CodeMapsTestingContext();

      var eHubClient_SI = new eHubClient { CC_ID = "SHIPPING_INSTRUCTION" };
      ctx.eHubClients.Add(eHubClient_SI);

      var transSet_SI = new eHubTransformationSet { TS_Name = "OCM System Configuration", eHubClient_Recipient = eHubClient_SI };
      ctx.eHubTransformationSets.Add(transSet_SI);

      var codeSet_NVOCC = new eHubCodeSet { CS_Name = "NVOCC", eHubClient_Sender = eHubClient_SI, eHubClient_Recipient = eHubClient_SI, eHubTransformationSet = transSet_SI };
      ctx.eHubCodeSets.Add(codeSet_NVOCC);

      var codeSetResult_NVOCC = new eHubCodeSetResult { eHubCodeSet = codeSet_NVOCC, CR_Order = 1, CR_Name = "Is Co-load" };
      ctx.eHubCodeSetResults.Add(codeSetResult_NVOCC);

      var nvocc_CodeMapKey1 = new eHubCodeMapKey { eHubCodeSet = codeSet_NVOCC, CK_Order = 1, CK_Key1Value = "CLD" };
      var nvocc_CodeMapKey2 = new eHubCodeMapKey { eHubCodeSet = codeSet_NVOCC, CK_Order = 2, CK_Key1Value = "%" };

      var nvocc_codeMapValue1 = new eHubCodeMapValue { eHubCodeMapKey = nvocc_CodeMapKey1, eHubCodeSetResult = codeSetResult_NVOCC, CV_OutputCode = "true" };
      var nvocc_codeMapValue2 = new eHubCodeMapValue { eHubCodeMapKey = nvocc_CodeMapKey2, eHubCodeSetResult = codeSetResult_NVOCC, CV_OutputCode = "false" };

      ctx.eHubCodeMapKeys.Add(nvocc_CodeMapKey1);
      ctx.eHubCodeMapKeys.Add(nvocc_CodeMapKey2);
      ctx.eHubCodeMapValues.Add(nvocc_codeMapValue1);
      ctx.eHubCodeMapValues.Add(nvocc_codeMapValue2);

      var codeSet_TransportBooking = new eHubCodeSet { CS_Name = "Transport Booking", eHubClient_Sender = eHubClient_SI, eHubClient_Recipient = eHubClient_SI, eHubTransformationSet = transSet_SI };
      var codeSetResult1_TransportBooking = new eHubCodeSetResult { eHubCodeSet = codeSet_TransportBooking, CR_Order = 1, CR_Name = "MultiPickup" };
      var codeSetResult2_TransportBooking = new eHubCodeSetResult { eHubCodeSet = codeSet_TransportBooking, CR_Order = 2, CR_Name = "MultiDropOff" };
      var transportBooking_CodeMapKey1 = new eHubCodeMapKey { eHubCodeSet = codeSet_TransportBooking, CK_Order = 1, CK_Key1Value = "INTTRA", CK_Key2Value = "INTT" };
      var transportBooking_CodeMapKey2 = new eHubCodeMapKey { eHubCodeSet = codeSet_TransportBooking, CK_Order = 2, CK_Key1Value = "%", CK_Key2Value = "%" };
      var transportBooking_codeMapValue1 = new eHubCodeMapValue { eHubCodeMapKey = transportBooking_CodeMapKey1, eHubCodeSetResult = codeSetResult1_TransportBooking, CV_OutputCode = "true" };
      var transportBooking_codeMapValue2 = new eHubCodeMapValue { eHubCodeMapKey = transportBooking_CodeMapKey1, eHubCodeSetResult = codeSetResult2_TransportBooking, CV_OutputCode = "true" };
      var transportBooking_codeMapValue3 = new eHubCodeMapValue { eHubCodeMapKey = transportBooking_CodeMapKey2, eHubCodeSetResult = codeSetResult1_TransportBooking, CV_OutputCode = "false" };
      var transportBooking_codeMapValue4 = new eHubCodeMapValue { eHubCodeMapKey = transportBooking_CodeMapKey2, eHubCodeSetResult = codeSetResult2_TransportBooking, CV_OutputCode = "false" };

      ctx.eHubCodeMapKeys.Add(transportBooking_CodeMapKey1);
      ctx.eHubCodeMapKeys.Add(transportBooking_CodeMapKey2);
      ctx.eHubCodeMapValues.Add(transportBooking_codeMapValue1);
      ctx.eHubCodeMapValues.Add(transportBooking_codeMapValue2);
      ctx.eHubCodeMapValues.Add(transportBooking_codeMapValue3);
      ctx.eHubCodeMapValues.Add(transportBooking_codeMapValue4);

      var codeSet_IsPayableElseWhere = new eHubCodeSet { CS_Name = "Payable ElseWhere", eHubClient_Sender = eHubClient_SI, eHubClient_Recipient = eHubClient_SI, eHubTransformationSet = transSet_SI };
      var codeSetResult1_IsPayableElseWhere = new eHubCodeSetResult { eHubCodeSet = codeSet_IsPayableElseWhere, CR_Order = 1, CR_Name = "IsSupported" };
      var codeSetResult2_IsPayableElseWhere = new eHubCodeSetResult { eHubCodeSet = codeSet_IsPayableElseWhere, CR_Order = 2, CR_Name = "OutputCode" };
      var codeSetResult3_IsPayableElseWhere = new eHubCodeSetResult { eHubCodeSet = codeSet_IsPayableElseWhere, CR_Order = 3, CR_Name = "Description" };

      var isPayableElseWhere_CodeMapKey1 = new eHubCodeMapKey { eHubCodeSet = codeSet_IsPayableElseWhere, CK_Order = 1, CK_Key1Value = "CMACGM_%"};
      var isPayableElseWhere_CodeMapKey2 = new eHubCodeMapKey { eHubCodeSet = codeSet_IsPayableElseWhere, CK_Order = 2, CK_Key1Value = "%",};

      var isPayableElseWhere_codeMapValue1 = new eHubCodeMapValue { eHubCodeMapKey = isPayableElseWhere_CodeMapKey1, eHubCodeSetResult = codeSetResult1_IsPayableElseWhere, CV_OutputCode = "true" };
      var isPayableElseWhere_codeMapValue2 = new eHubCodeMapValue { eHubCodeMapKey = isPayableElseWhere_CodeMapKey1, eHubCodeSetResult = codeSetResult2_IsPayableElseWhere, CV_OutputCode = "ELSEWHERE" };
      var isPayableElseWhere_codeMapValue3 = new eHubCodeMapValue { eHubCodeMapKey = isPayableElseWhere_CodeMapKey1, eHubCodeSetResult = codeSetResult3_IsPayableElseWhere, CV_OutputCode = "ELSEWHERE" };

      var isPayableElseWhere_codeMapValue4 = new eHubCodeMapValue { eHubCodeMapKey = isPayableElseWhere_CodeMapKey2, eHubCodeSetResult = codeSetResult1_IsPayableElseWhere, CV_OutputCode = "false" };
      var isPayableElseWhere_codeMapValue5 = new eHubCodeMapValue { eHubCodeMapKey = isPayableElseWhere_CodeMapKey2, eHubCodeSetResult = codeSetResult2_IsPayableElseWhere, CV_OutputCode = string.Empty };
      var isPayableElseWhere_codeMapValue6 = new eHubCodeMapValue { eHubCodeMapKey = isPayableElseWhere_CodeMapKey2, eHubCodeSetResult = codeSetResult3_IsPayableElseWhere, CV_OutputCode = string.Empty };

      ctx.eHubCodeMapKeys.Add(isPayableElseWhere_CodeMapKey1);
      ctx.eHubCodeMapKeys.Add(isPayableElseWhere_CodeMapKey2);
      ctx.eHubCodeMapValues.Add(isPayableElseWhere_codeMapValue1);
      ctx.eHubCodeMapValues.Add(isPayableElseWhere_codeMapValue2);
      ctx.eHubCodeMapValues.Add(isPayableElseWhere_codeMapValue3);
      ctx.eHubCodeMapValues.Add(isPayableElseWhere_codeMapValue4);
      ctx.eHubCodeMapValues.Add(isPayableElseWhere_codeMapValue5);
      ctx.eHubCodeMapValues.Add(isPayableElseWhere_codeMapValue6);

      ctx.eHubCodeSets.Add(codeSet_NVOCC);
      ctx.eHubCodeSets.Add(codeSet_TransportBooking);
      ctx.eHubCodeSets.Add(codeSet_IsPayableElseWhere);

      var codeSet_UNLOCOLookup = new eHubCodeSet { CS_Name = "UNLOCO_Lookup", eHubClient_Sender = eHubClient_SI, eHubClient_Recipient = eHubClient_SI, eHubTransformationSet = transSet_SI };
      var codeSetResult1_UNLOCOLookup = new eHubCodeSetResult { eHubCodeSet = codeSet_UNLOCOLookup, CR_Order = 1, CR_Name = "AdministrativeRegion" };

      var UNLOCOLookup_CodeMapKey1 = new eHubCodeMapKey { eHubCodeSet = codeSet_UNLOCOLookup, CK_Order = 1, CK_Key1Value = "GBLIS" };
      var UNLOCOLookup_CodeMapKey2 = new eHubCodeMapKey { eHubCodeSet = codeSet_UNLOCOLookup, CK_Order = 3, CK_Key1Value = "%", };

      var UNLOCOLookup_codeMapValue1 = new eHubCodeMapValue { eHubCodeMapKey = UNLOCOLookup_CodeMapKey1, eHubCodeSetResult = codeSetResult1_UNLOCOLookup, CV_OutputCode = "Northern Ireland" };
      var UNLOCOLookup_codeMapValue2 = new eHubCodeMapValue { eHubCodeMapKey = UNLOCOLookup_CodeMapKey2, eHubCodeSetResult = codeSetResult1_UNLOCOLookup, CV_OutputCode = string.Empty };

      ctx.eHubCodeMapKeys.Add(UNLOCOLookup_CodeMapKey1);
      ctx.eHubCodeMapKeys.Add(UNLOCOLookup_CodeMapKey2);

      ctx.eHubCodeMapValues.Add(UNLOCOLookup_codeMapValue1);
      ctx.eHubCodeMapValues.Add(UNLOCOLookup_codeMapValue2);

      transformAccessor = new TransformAccessor();
      transformAccessor.SetCodeMapsTestingContext(ctx);
    }

    TransformAccessor transformAccessor;
  }

  public class CodeMapperTest : CodeMapper
  {
    public CodeMapperTest(TransformAccessor ta)
    {
      transformAccessor = ta;
    }
    readonly TransformAccessor transformAccessor;

    public override string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1)
    {
      return transformAccessor.GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, null, null, null, null);
    }
  }
}