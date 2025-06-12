//using CargoWise.eServices.USCustoms.Services;

//namespace CargoWise.eServices.USCustoms.Tests
//{
//    public class OutboundConfigurationForTesting1 : IMQOutboundConfiguration
//    {
//        #region IConfiguration Members

//        public string Hostname
//        {
//            get { return "ord-vcmq-1"; }
//        }

//        public int Port
//        {
//            get { return 1416; }
//        }

//        public string Channel
//        {
//            get { return "USChannel"; }
//        }

//        public string QueueManager
//        {
//            get { return "DummyUSQueue"; }
//        }

//        public string QueueName
//        {
//            get { return "USLocal"; }
//        }

//        public string GetFooterData(string footer)
//        {
//            return "Z" + OurABISettings + footer.Substring(OurABISettings.Length + 1);
//        }

//        public string GetHeaderData(string header)
//        {
//            return "A" + OurABISettings + header.Substring(OurABISettings.Length + 1);
//        }

//        const string OurABISettings = "8888DKDPASSSD";

//        #endregion
//    }

//    public class OutboundConfigurationForTesting2 : IMQOutboundConfiguration
//    {
//        #region IConfiguration Members

//        public string Hostname
//        {
//            get { return "ord-vcmq-1"; }
//        }

//        public int Port
//        {
//            get { return 1417; }
//        }

//        public string Channel
//        {
//            get { return "USChannel1"; }
//        }

//        public string QueueManager
//        {
//            get { return "DummyUSQueue1"; }
//        }

//        public string QueueName
//        {
//            get { return "USLocal1"; }
//        }

//        public string GetFooterData(string footer)
//        {
//            return "Z" + OurABISettings + footer.Substring(OurABISettings.Length + 1);
//        }

//        public string GetHeaderData(string header)
//        {
//            return "A" + OurABISettings + header.Substring(OurABISettings.Length + 1);
//        }

//        const string OurABISettings = "7777DEEDGWSOE";

//        #endregion
//    }

//    public class OutboundConfigurationForTesting3 : IMQOutboundConfiguration
//    {
//        #region IConfiguration Members

//        public string Hostname
//        {
//            get { return "ord-vcmq-1"; }
//        }

//        public int Port
//        {
//            get { return 1417; }
//        }

//        public string Channel
//        {
//            get { return "USChannel1"; }
//        }

//        public string QueueManager
//        {
//            get { return "DummyUSQueue1"; }
//        }

//        public string QueueName
//        {
//            get { return "USLocal2"; }
//        }

//        public string GetFooterData(string footer)
//        {
//            return "Z" + OurABISettings + footer.Substring(OurABISettings.Length + 1);
//        }

//        public string GetHeaderData(string header)
//        {
//            return "A" + OurABISettings + header.Substring(OurABISettings.Length + 1);
//        }

//        const string OurABISettings = "6666ABCDEFGHI";

//        #endregion
//    }

//    public class OutboundConfigurationForTestingSYD_VHMY_1 : IMQOutboundConfiguration
//    {
//        #region IConfiguration Members

//        string Services.IMQConfiguration.Hostname
//        {
//            get { return "SYD-VHMY-1"; }
//        }

//        int IMQConfiguration.Port
//        {
//            get { return 1421; }
//        }

//        string IMQConfiguration.Channel
//        {
//            get { return "QM_TEST.SVRCONN"; }
//        }

//        string IMQConfiguration.QueueManager
//        {
//            get { return "QM_TEST"; }
//        }

//        string IMQConfiguration.QueueName
//        {
//            get { return "QM_TEST.LOCAL.ONE"; }
//        }

//        public string GetFooterData(string footer)
//        {
//            return "Z" + OurABISettings + footer.Substring(OurABISettings.Length + 1);
//        }

//        public string GetHeaderData(string header)
//        {
//            return "A" + OurABISettings + header.Substring(OurABISettings.Length + 1);
//        }

//        const string OurABISettings = "1234ABCDEFGHI";

//        #endregion
//    }

//}
