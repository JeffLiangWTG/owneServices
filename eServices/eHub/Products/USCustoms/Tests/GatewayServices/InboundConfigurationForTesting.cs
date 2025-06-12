
//namespace CargoWise.eServices.USCustoms.Tests
//{
//    public class InboundConfigurationForTesting1 : IMQInboundConfiguration
//    {
//        public InboundConfigurationForTesting1(string clientId, string applicationCode, string interchangeType)
//        {
//            this.clientId = clientId;
//            this.applicationCode = applicationCode;
//            this.interchangeType = interchangeType;
//        }

//        #region IConfiguration Members

//        string Services.IMQConfiguration.Hostname
//        {
//            get { return "ord-vcmq-1"; }
//        }

//        int Services.IMQConfiguration.Port
//        {
//            get { return 1416; }
//        }

//        string Services.IMQConfiguration.Channel
//        {
//            get { return "USChannel"; }
//        }

//        string Services.IMQConfiguration.QueueManager
//        {
//            get { return "DummyUSQueue"; }
//        }

//        string Services.IMQConfiguration.QueueName
//        {
//            get { return "USLocal"; }
//        }

//        #endregion

//        #region IMQInboundConfiguration Members

//        string IMQInboundConfiguration.GetDestinationId(string data)
//        {
//            return clientId;
//        }
//        readonly string clientId;

//        string IMQInboundConfiguration.GetApplicationCode(string data)
//        {
//            return applicationCode;
//        }
//        readonly string applicationCode;

//        string IMQInboundConfiguration.GetInterchangeType(string data)
//        {
//            return interchangeType;
//        }
//        readonly string interchangeType;

//        #endregion
//    }

//    public class InboundConfigurationForTesting2 : InboundConfigurationForTesting1, IMQInboundConfiguration
//    {
//        public InboundConfigurationForTesting2(string clientId, string applicationCode, string interchangeType)
//            : base(clientId, applicationCode, interchangeType)
//        {
//        }

//        #region IConfiguration Members

//        int Services.IMQConfiguration.Port
//        {
//            get { return 1417; }
//        }

//        string Services.IMQConfiguration.Channel
//        {
//            get { return "USChannel1"; }
//        }

//        string Services.IMQConfiguration.QueueManager
//        {
//            get { return "DummyUSQueue1"; }
//        }

//        string Services.IMQConfiguration.QueueName
//        {
//            get { return "USLocal1"; }
//        }

//        #endregion
//    }

//    public class InboundConfigurationForTesting3 : InboundConfigurationForTesting2, IMQInboundConfiguration
//    {
//        public InboundConfigurationForTesting3(string clientId, string applicationCode, string interchangeType)
//            : base(clientId, applicationCode, interchangeType)
//        {
//        }

//        #region IConfiguration Members

//        string Services.IMQConfiguration.QueueName
//        {
//            get { return "USLocal2"; }
//        }

//        #endregion
//    }

//    public class InboundConfigurationForTestingSYD_VHMY_1 : IMQInboundConfiguration
//    {
//        public InboundConfigurationForTestingSYD_VHMY_1(string clientId, string applicationCode, string interchangeType)
//        {
//            this.clientId = clientId;
//            this.applicationCode = applicationCode;
//            this.interchangeType = interchangeType;
//        }

//        #region IConfiguration Members

//        string Services.IMQConfiguration.Hostname
//        {
//            get { return "SYD-VHMY-1"; }
//        }

//        int Services.IMQConfiguration.Port
//        {
//            get { return 1421; }
//        }

//        string Services.IMQConfiguration.Channel
//        {
//            get { return "QM_TEST.SVRCONN"; }
//        }

//        string Services.IMQConfiguration.QueueManager
//        {
//            get { return "QM_TEST"; }
//        }

//        string Services.IMQConfiguration.QueueName
//        {
//            get { return "QM_TEST.LOCAL.ONE"; }
//        }

//        #endregion

//        #region IMQInboundConfiguration Members

//        string IMQInboundConfiguration.GetDestinationId(string data)
//        {
//            return clientId;
//        }
//        readonly string clientId;

//        string IMQInboundConfiguration.GetApplicationCode(string data)
//        {
//            return applicationCode;
//        }
//        readonly string applicationCode;

//        string IMQInboundConfiguration.GetInterchangeType(string data)
//        {
//            return interchangeType;
//        }
//        readonly string interchangeType;

//        #endregion
//    }
//}
