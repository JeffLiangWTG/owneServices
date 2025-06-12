using Hawking.Xslt.ExtensionObjects.Interfaces;
using Hawking.Unity;
using Unity;
using Xunit;
using Hawking.eHub.Model.eHubTransactions;
using Hawking.eHub.Transform.Helper.Wrapper.Cargowise.eHub.DataAccess.Sql;
using Moq;
using System;

namespace Hawking.eHub.Transform.Helper.Wrapper.Tests.Cargowise.eHub.DataAccess.Sql
{
    public class TransformAccessorFixtures
    {
        IUnityContainer unityContainer;
        public TransformAccessorFixtures()
        {
            unityContainer = DependencyFactory.Container;
            Unity.Config.ApplicationConfig.Initialise();

            unityContainer.RegisterType<ITransformAccessor, TransformAccessor>();
            unityContainer.RegisterType<IeHubStoredProc, eHubStoredProc>();
            unityContainer.RegisterType<IeHubTransactionsContext, eHubTransactionsContext>();

            var unlocoAccessor = new Mock<IUNLOCOAccessor>();
            var localTime = string.Empty;
            unlocoAccessor.Setup(x => x.GetStateFromUNLOCO(It.IsAny<string>())).Returns("TEST_STATE");
            unlocoAccessor.Setup(x => x.GetUNLOCOfromIATA(It.IsAny<string>())).Returns("TEST_LOCO");
            unlocoAccessor.Setup(x => x.ConvertUTCToLocalTimeByUNLOCO(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string utcTime, string loco) => { return utcTime; });

            unityContainer.RegisterInstance<IUNLOCOAccessor>(unlocoAccessor.Object);
        }

        [Fact]
        public void TestCallActionProcedure()
        {
            var transformer = unityContainer.Resolve<ITransformAccessor>();
            var result = transformer.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanTracing.Transforms.COPARN", "@maxlength", "14");
            Assert.True(!string.IsNullOrEmpty(result));
        }

        [Fact]
        public void TestGetStateFromUNLOCO()
        {
            var transformer = unityContainer.Resolve<ITransformAccessor>();

            var lastPort = "AUMEL";
            var result = transformer.CallActionProcedureHelper("GetStateFromUNLOCO", "", "@UNLOCO", lastPort);
            Assert.Equal("VIC", result);
        }

        [Fact]
        public void TestCalculateTimeZoneOffset()
        {
            var transformer = unityContainer.Resolve<ITransformAccessor>();
            var eventLocalTime = new DateTime(2018, 06, 26, 17, 30, 0);
            var result = transformer.CallActionProcedureHelper("CalculateTimeZoneOffset", "@offset", "@UNLOCO", "GBOXF", "@localtime", eventLocalTime.ToLongTimeString());
            Assert.Equal("+00:00", result);
        }

        [Fact] public void TestCallActionProcedureHelper() { }
        [Fact] public void TestConvert() { }
        [Fact] public void TestConvertLocalXmlDateTimeStringToUTC() { }
        [Fact] public void TestConvertToDate() { }
        [Fact] public void TestConvertToDateTimeString() { }
        [Fact] public void TestConvertToXmlDate() { }
        [Fact] public void TestConvertUTCToLocalTimeByUNLOCO() { }
        [Fact] public void TestConvertXmlDateString() { }
        [Fact] public void TestCurrentDateTime() { }
        [Fact] public void TestCurrentDateTimeUTC() { }
        [Fact] public void TestCurrentDateTimeWithTimeZone() { }
        [Fact] public void TestFormatDecimal() { }
        [Fact] public void TestFormatXmlDateTime() { }
        [Fact] public void TestGetClientGroupToken() { }
        [Fact] public void TestGetClientRegistrationCode() { }
        [Fact] public void TestGetClientToken() { }
        [Fact] public void TestGetContextProperty() { }
        [Fact] public void TestGetInstallationPassword() { }
        [Fact] public void TestGetInstallationToken() { }
        [Fact] public void TestGetPassword() { }
        [Fact] public void TestGetRecipientCode() { }
        [Fact] public void TestGetRecipientCodeUnkeyed() { }
        [Fact] public void TestGetSGCustomsAccount() { }
        [Fact] public void TestGetSGCustomsSenderID() { }
        [Fact] public void TestGetUNLOCOfromIATA() { }
        [Fact] public void TestGetUsername() { }
        [Fact] public void TestGetVendorToken() { }
        [Fact] public void TestGetWithOverrides() { }
        [Fact] public void TestGroupByPackingLine() { }
        [Fact] public void TestInitializeSubscription() { }
        [Fact] public void TestInsertSubscriptionValue() { }
        [Fact] public void TestLoadSubscription() { }
        [Fact] public void TestRoundAwayFromZero() { }
        [Fact] public void TestSelectSGIDByRef() { }
        [Fact] public void TestSetContextProperty() { }
        [Fact] public void TestSubscribeHAWB() { }
        [Fact] public void TestSubscribeHistoryOrder() { }
        [Fact] public void TestSubscribeMessageInfo() { }
        [Fact] public void TestSubscribePackLine() { }
        [Fact] public void TestSubscribeShipment() { }
    }
}
