using System.Xml.XPath;
using Hawking.Xslt.ExtensionObjects.Interfaces;
using Unity;

namespace Hawking.eHub.Transform.Helper.Wrapper
{
    public class eHubTransformHelperFacade : IeHubTransformHelperFacade
    {
        #region Member Variables

        ICodeMapper CodeMapper;
        IMathHelper MathHelper;
        IStringMapper StringMapper;
        IUnitConverter UnitConverter;
        IXmlHelper XmlHelper;
        IDateMapper DateMapper;

        ITransformAccessor TransformAccessor;
        ICACustomsGroupingHelper CACustomsGroupingHelper;
        IConfigurationAccessor ConfigurationAccessor;
        IContextAccessor ContextAccessor;
        IDataModelAccessor DataModelAccessor;
        IJPCustomsDataModelAccessor JPCustomsDataModelAccessor;
        INEXDOCDataModelAccessor NEXDOCDataModelAccessor;
        ISGCustomsDataModelAccessor SGCustomsDataModelAccessor;
        ISGCustomsSubscriptionHelper SGCustomsSubscriptionHelper;

        #endregion

        public eHubTransformHelperFacade(IUnityContainer unitContainer)
        {
            CodeMapper = unitContainer.Resolve<ICodeMapper>();
            MathHelper = unitContainer.Resolve<IMathHelper>();
            StringMapper = unitContainer.Resolve<IStringMapper>();
            UnitConverter = unitContainer.Resolve<IUnitConverter>();
            XmlHelper = unitContainer.Resolve<IXmlHelper>();
            DateMapper = unitContainer.Resolve<IDateMapper>();
            TransformAccessor = unitContainer.Resolve<ITransformAccessor>();
            CACustomsGroupingHelper = unitContainer.Resolve<ICACustomsGroupingHelper>();
            ConfigurationAccessor = unitContainer.Resolve<IConfigurationAccessor>();
            ContextAccessor = unitContainer.Resolve<IContextAccessor>();
            DataModelAccessor = unitContainer.Resolve<IDataModelAccessor>();
            JPCustomsDataModelAccessor = unitContainer.Resolve<IJPCustomsDataModelAccessor>();
            NEXDOCDataModelAccessor = unitContainer.Resolve<INEXDOCDataModelAccessor>();
            SGCustomsDataModelAccessor = unitContainer.Resolve<ISGCustomsDataModelAccessor>();
            SGCustomsSubscriptionHelper = unitContainer.Resolve<ISGCustomsSubscriptionHelper>();
        }

        public eHubTransformHelperFacade(
            ICodeMapper CodeMapper,
            IMathHelper MathHelper,
            IStringMapper StringMapper,
            IUnitConverter UnitConverter,
            IXmlHelper XmlHelper,
            IDateMapper DateMapper,
            ITransformAccessor TransformAccessor,
            ICACustomsGroupingHelper CACustomsGroupingHelper,
            IConfigurationAccessor ConfigurationAccessor,
            IContextAccessor ContextAccessor,
            IDataModelAccessor DataModelAccessor,
            IJPCustomsDataModelAccessor JPCustomsDataModelAccessor,
            INEXDOCDataModelAccessor NEXDOCDataModelAccessor,
            ISGCustomsDataModelAccessor SGCustomsDataModelAccessor,
            ISGCustomsSubscriptionHelper SGCustomsSubscriptionHelper)
        {
            this.CodeMapper = CodeMapper;
            this.MathHelper = MathHelper;
            this.StringMapper = StringMapper;
            this.UnitConverter = UnitConverter;
            this.XmlHelper = XmlHelper;
            this.DateMapper = DateMapper;
            this.TransformAccessor = TransformAccessor;
            this.CACustomsGroupingHelper = CACustomsGroupingHelper;
            this.ConfigurationAccessor = ConfigurationAccessor;
            this.ContextAccessor = ContextAccessor;
            this.DataModelAccessor = DataModelAccessor;
            this.JPCustomsDataModelAccessor = JPCustomsDataModelAccessor;
            this.NEXDOCDataModelAccessor = NEXDOCDataModelAccessor;
            this.SGCustomsDataModelAccessor = SGCustomsDataModelAccessor;
            this.SGCustomsSubscriptionHelper = SGCustomsSubscriptionHelper;
        }

        #region Wrappers

        public string CallActionProcedure(string procedure, string outputParm, params string[] inputParms)
        {
            return CodeMapper.CallActionProcedure(procedure, outputParm, inputParms);
        }

        public string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1)
        {
            return CodeMapper.CallActionProcedureHelper(procedure, outputParm, inputParmN1, inputParmV1);
        }

        public string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2)
        {
            return CodeMapper.CallActionProcedureHelper(procedure, outputParm, inputParmN1, inputParmV1, inputParmN2, inputParmV2);
        }

        public string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3)
        {
            return CodeMapper.CallActionProcedureHelper(procedure, outputParm, inputParmN1, inputParmV1, inputParmN2, inputParmV2, inputParmN3, inputParmV3);
        }

        public string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3, string inputParmN4, string inputParmV4)
        {
            return CodeMapper.CallActionProcedureHelper(procedure, outputParm, inputParmN1, inputParmV1, inputParmN2, inputParmV2, inputParmN3, inputParmV3, inputParmN4, inputParmV4);
        }

        public string CallActionProcedureHelper(string procedure, string outputParm, string inputParmN1, string inputParmV1, string inputParmN2, string inputParmV2, string inputParmN3, string inputParmV3, string inputParmN4, string inputParmV4, string inputParmN5, string inputParmV5)
        {
            return CodeMapper.CallActionProcedureHelper(procedure, outputParm, inputParmN1, inputParmV1, inputParmN2, inputParmV2, inputParmN3, inputParmV3, inputParmN4, inputParmV4, inputParmN5, inputParmV5);
        }

        public string Convert(string value, string fromUnit, string toUnit)
        {
            return UnitConverter.Convert(value, fromUnit, toUnit);
        }

        public string ConvertLocalXmlDateTimeStringToUTC(string localXmlDateTimeString, string toFormatString)
        {
            return DateMapper.ConvertLocalXmlDateTimeStringToUTC(localXmlDateTimeString, toFormatString);
        }

        public string ConvertLocalXmlDateTimeStringToUTC(string localXmlDateTimeString, string toFormatString, string defaultDateString)
        {
            return DateMapper.ConvertLocalXmlDateTimeStringToUTC(localXmlDateTimeString, toFormatString, defaultDateString);
        }

        public string ConvertLocalXmlDateTimeStringToUTC(string localXmlDateTimeString, string defaultTimeZoneOffset, string toFormatString, string defaultDateString)
        {
            return DateMapper.ConvertLocalXmlDateTimeStringToUTC(localXmlDateTimeString, defaultTimeZoneOffset, toFormatString, defaultDateString);
        }

        public string ConvertToCsv(string input)
        {
            return StringMapper.ConvertToCsv(input);
        }

        public string ConvertToCsv(string input, bool simplyReplaceSpecialCharactersWithSpaces)
        {
            return StringMapper.ConvertToCsv(input, simplyReplaceSpecialCharactersWithSpaces);
        }

        public string ConvertToDate(string origDateTime, string formatString)
        {
            return DateMapper.ConvertToDate(origDateTime, formatString);
        }

        public string ConvertToDateTimeString(string dateVal)
        {
            return DateMapper.ConvertToDateTimeString(dateVal);
        }

        public string ConvertToDateTimeString(string dateVal, string dateValFmt)
        {
            return DateMapper.ConvertToDateTimeString(dateVal, dateValFmt);
        }

        public string ConvertToDateTimeString(string dateVal, string dateValFmt, string outFmt)
        {
            return DateMapper.ConvertToDateTimeString(dateVal, dateValFmt, outFmt);
        }

        public string ConvertToDateTimeString(string dateVal, string dateValFmt, string timeVal, string timeValFmt)
        {
            return DateMapper.ConvertToDateTimeString(dateVal, dateValFmt, timeVal, timeValFmt);
        }

        public string ConvertToDateTimeString(string dateVal, string dateValFmt, string timeVal, string timeValFmt, string outFmt)
        {
            return DateMapper.ConvertToDateTimeString(dateVal, dateValFmt, timeVal, timeValFmt, outFmt);
        }

        public decimal ConvertToDecimal(string stringNumber)
        {
            return StringMapper.ConvertToDecimal(stringNumber);
        }

        public string ConvertToUTCTime(string sourceXmlDate)
        {
            return DateMapper.ConvertToUTCTime(sourceXmlDate);
        }

        public string ConvertToXmlDate(string dateString, string formatString)
        {
            return DateMapper.ConvertToXmlDate(dateString, formatString);
        }

        public string ConvertToXmlDatePartOnly(string dateString, string formatString)
        {
            return DateMapper.ConvertToXmlDatePartOnly(dateString, formatString);
        }

        public string ConvertUTCToLocalTimeByUNLOCO(string utcTime, string unloco)
        {
            return DateMapper.ConvertUTCToLocalTimeByUNLOCO(utcTime, unloco);
        }

        public string ConvertXmlDateString(string sourceXmlDate, string toFormatString)
        {
            return DateMapper.ConvertXmlDateString(sourceXmlDate, toFormatString);
        }

        public string ConvertXmlDateString(string sourceXmlDate, string toFormatString, string defaultDateString)
        {
            return DateMapper.ConvertXmlDateString(sourceXmlDate, toFormatString, defaultDateString);
        }

        public string CurrentDateTime(string toFormat)
        {
            return DateMapper.CurrentDateTime(toFormat);
        }

        public string CurrentDateTimeUTC(string toFormat)
        {
            return DateMapper.CurrentDateTimeUTC(toFormat);
        }

        public string CurrentDateTimeWithTimeZone()
        {
            return DateMapper.CurrentDateTimeWithTimeZone();
        }

        public string CurrentDateWithTimeZone()
        {
            return DateMapper.CurrentDateWithTimeZone();
        }

        public string Format(string inputValue, string format)
        {
            return StringMapper.Format(inputValue, format);
        }

        public string FormatDecimal(string input, string format, bool treatInvalidInputAsZero = false)
        {
            return StringMapper.FormatDecimal(input, format, treatInvalidInputAsZero);
        }

        public string FormatXmlDateTime(string xmlDateTime, string outputFormat)
        {
            return DateMapper.FormatXmlDateTime(xmlDateTime, outputFormat);
        }

        public string GetClientGroupToken(string clientId)
        {
            return ConfigurationAccessor.GetClientGroupToken(clientId);
        }

        public string GetClientRegistrationCode(string clientID, string qualifier, string registrationTypeID)
        {
            return DataModelAccessor.GetClientRegistrationCode(clientID, qualifier, registrationTypeID);
        }

        public string GetClientToken(string systemId, string staffCode)
        {
            return ConfigurationAccessor.GetClientToken(systemId, staffCode);
        }

        public string GetContextProperty(string contextItemName, string contextItemNamespace)
        {
            return ContextAccessor.GetContextProperty(contextItemName, contextItemNamespace);
        }

        public string GetContextProperty(string contextItemName, string contextItemNamespace, string defaultValue)
        {
            return ContextAccessor.GetContextProperty(contextItemName, contextItemNamespace, defaultValue);
        }

        public string GetInstallationPassword(string recipientID)
        {
            return NEXDOCDataModelAccessor.GetInstallationPassword(recipientID);
        }

        public string GetInstallationToken(string recipientID)
        {
            return NEXDOCDataModelAccessor.GetInstallationToken(recipientID);
        }

        public string GetPassword(string senderID)
        {
            return JPCustomsDataModelAccessor.GetPassword(senderID);
        }

        public string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string code)
        {
            return CodeMapper.GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, code);
        }

        public string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1)
        {
            return CodeMapper.GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1);
        }

        public string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2)
        {
            return CodeMapper.GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2);
        }

        public string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3)
        {
            return CodeMapper.GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, key3);
        }

        public string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4)
        {
            return CodeMapper.GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, key3, key4);
        }

        public string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4, string key5)
        {
            return CodeMapper.GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, key3, key4, key5);
        }

        public string GetRecipientCodeUnkeyed(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField)
        {
            return CodeMapper.GetRecipientCodeUnkeyed(senderClientCode, recipientClientCode, transformationName, codeSet, resultField);
        }

        public string GetSGCustomsAccount(string brokerID, string eHubClientID)
        {
            return SGCustomsDataModelAccessor.GetSGCustomsAccount(brokerID, eHubClientID);
        }

        public string GetSGCustomsSenderID(string accountName)
        {
            return SGCustomsDataModelAccessor.GetSGCustomsSenderID(accountName);
        }

        public string GetStateFromUNLOCO(string UNLOCO)
        {
            return CodeMapper.GetStateFromUNLOCO(UNLOCO);
        }

        public string GetUNLOCOfromIATA(string IATACode)
        {
            return CodeMapper.GetUNLOCOfromIATA(IATACode);
        }

        public string GetUNLOCOFromIATA(string IATACode)
        {
            return TransformAccessor.GetUNLOCOFromIATA(IATACode);
        }

        public string GetUsername(string senderID)
        {
            return JPCustomsDataModelAccessor.GetUsername(senderID);
        }

        public string GetValueOrEmpty(string inputString)
        {
            return StringMapper.GetValueOrEmpty(inputString);
        }

        public string GetVendorToken(string recipientID)
        {
            return NEXDOCDataModelAccessor.GetVendorToken(recipientID);
        }

        public XPathNodeIterator GetWithOverrides(XPathNodeIterator nodes)
        {
            return XmlHelper.GetWithOverrides(nodes);
        }

        public XPathNodeIterator GetWithOverrides(XPathNodeIterator nodes, string nodeName)
        {
            return XmlHelper.GetWithOverrides(nodes, nodeName);
        }

        public XPathNavigator GroupByPackingLine(XPathNodeIterator commercialInvoiceLineCollection, XPathNodeIterator packingLineCollection)
        {
            return CACustomsGroupingHelper.GroupByPackingLine(commercialInvoiceLineCollection, packingLineCollection);
        }

        public XPathNavigator InitializeSubscription()
        {
            return SGCustomsSubscriptionHelper.InitializeSubscription();
        }

        public void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value)
        {
            DataModelAccessor.InsertSubscriptionValue(subscriptionType, providerID, subscriberID, value);
        }

        public void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value, string reference)
        {
            DataModelAccessor.InsertSubscriptionValue(subscriptionType, providerID, subscriberID, value, reference);
        }

        public void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value, string reference, string referenceType)
        {
            DataModelAccessor.InsertSubscriptionValue(subscriptionType, providerID, subscriberID, value, reference, referenceType);
        }

        public bool IsValidDate(string dateString, string formatString)
        {
            return DateMapper.IsValidDate(dateString, formatString);
        }

        public string Join(XPathNodeIterator Values, string Delimeter)
        {
            return SGCustomsSubscriptionHelper.Join(Values, Delimeter);
        }

        public XPathNavigator LoadSubscription(XPathNavigator SubscriptionNavigator, string Content)
        {
            return SGCustomsSubscriptionHelper.LoadSubscription(SubscriptionNavigator, Content);
        }

        public string PadLeft(string inputString, int length, string paddingChar)
        {
            return StringMapper.PadLeft(inputString, length, paddingChar);
        }

        public string PadRight(string inputString, int length, string paddingChar)
        {
            return StringMapper.PadRight(inputString, length, paddingChar);
        }

        public string RemoveNewLines(string input)
        {
            return StringMapper.RemoveNewLines(input);
        }

        public string RemoveNewLines(string input, int lineLen, int outLen)
        {
            return StringMapper.RemoveNewLines(input, lineLen, outLen);
        }

        public string RemoveNewLines(string input, int lineLen)
        {
            return StringMapper.RemoveNewLines(input, lineLen);
        }

        public string Replace(string inputString, string oldValue, string newValue)
        {
            return StringMapper.Replace(inputString, oldValue, newValue);
        }

        public string RoundAwayFromZero(string inputValue)
        {
            return MathHelper.RoundAwayFromZero(inputValue);
        }

        public string RoundAwayFromZero(string inputValue, string decimalPlaces)
        {
            return MathHelper.RoundAwayFromZero(inputValue, decimalPlaces);
        }

        public string RoundToEven(string inputValue)
        {
            return MathHelper.RoundToEven(inputValue);
        }

        public string RoundToEven(string inputValue, string decimalPlaces)
        {
            return MathHelper.RoundToEven(inputValue, decimalPlaces);
        }

        public XPathNavigator SelectHousesByIDT(XPathNavigator SubscriptionNavigator, string IDT, string Purpose)
        {
            return SGCustomsSubscriptionHelper.SelectHousesByIDT(SubscriptionNavigator, IDT, Purpose);
        }

        public XPathNavigator SelectHousesByIDT(XPathNavigator SubscriptionNavigator, string IDT)
        {
            return SGCustomsSubscriptionHelper.SelectHousesByIDT(SubscriptionNavigator, IDT);
        }

        public XPathNavigator SelectInfoByIDT(XPathNavigator Subscription, string IDT, string Purpose, string InfoKey)
        {
            return SGCustomsSubscriptionHelper.SelectInfoByIDT(Subscription, IDT, Purpose, InfoKey);
        }

        public XPathNavigator SelectInfoByIDT(XPathNavigator Subscription, string IDT, string InfoKey)
        {
            return SGCustomsSubscriptionHelper.SelectInfoByIDT(Subscription, IDT, InfoKey);
        }

        public XPathNavigator SelectLatestIDTWhichDeclaredHAWB(XPathNavigator Subscription, string HAWB)
        {
            return SGCustomsSubscriptionHelper.SelectLatestIDTWhichDeclaredHAWB(Subscription, HAWB);
        }

        public XPathNavigator SelectPackLineByRef(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef)
        {
            return SGCustomsSubscriptionHelper.SelectPackLineByRef(SubscriptionNavigator, HAWB, ConsignmentRef);
        }

        public XPathNavigator SelectPackLineBySGID(XPathNavigator SubscriptionNavigator, string HAWB, string SGID)
        {
            return SGCustomsSubscriptionHelper.SelectPackLineBySGID(SubscriptionNavigator, HAWB, SGID);
        }

        public XPathNavigator SelectPackLineCurrentStatus(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef)
        {
            return SGCustomsSubscriptionHelper.SelectPackLineCurrentStatus(SubscriptionNavigator, HAWB, ConsignmentRef);
        }

        public XPathNavigator SelectPackLinesByIDT(XPathNavigator SubscriptionNavigator, string HAWB, string IDT, string Purpose)
        {
            return SGCustomsSubscriptionHelper.SelectPackLinesByIDT(SubscriptionNavigator, HAWB, IDT, Purpose);
        }

        public XPathNavigator SelectPackLinesByIDT(XPathNavigator SubscriptionNavigator, string HAWB, string IDT)
        {
            return SGCustomsSubscriptionHelper.SelectPackLinesByIDT(SubscriptionNavigator, HAWB, IDT);
        }

        public string SelectSGIDByRef(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef)
        {
            return SGCustomsSubscriptionHelper.SelectSGIDByRef(SubscriptionNavigator, HAWB, ConsignmentRef);
        }

        public void SetContextProperty(string contextItemName, string contextItemNamespace, string replacementValue)
        {
            ContextAccessor.SetContextProperty(contextItemName, contextItemNamespace, replacementValue);
        }

        public XPathNavigator SubscribeEvent(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef, string Status, string IDT, string Purpose)
        {
            return SGCustomsSubscriptionHelper.SubscribeEvent(SubscriptionNavigator, HAWB, ConsignmentRef, Status, IDT, Purpose);
        }

        public XPathNavigator SubscribeHAWB(XPathNavigator SubscriptionNavigator, string HAWB)
        {
            return SGCustomsSubscriptionHelper.SubscribeHAWB(SubscriptionNavigator, HAWB);
        }

        public XPathNavigator SubscribeHistoryOrder(XPathNavigator SubscriptionNavigator, string Purpose, string DateTime, string IDT1, string IDT2, string IDT3)
        {
            return SGCustomsSubscriptionHelper.SubscribeHistoryOrder(SubscriptionNavigator, Purpose, DateTime, IDT1, IDT2, IDT3);
        }

        public XPathNavigator SubscribeInfo(XPathNavigator SubscriptionNavigator, string HAWB, string ConsignmentRef, string IDT, string Purpose, string InfoName, string InfoValue)
        {
            return SGCustomsSubscriptionHelper.SubscribeInfo(SubscriptionNavigator, HAWB, ConsignmentRef, IDT, Purpose, InfoName, InfoValue);
        }

        public XPathNavigator SubscribeMessageInfo(XPathNavigator SubscriptionNavigator, string IDT, string Purpose, string InfoName, string InfoValue)
        {
            return SGCustomsSubscriptionHelper.SubscribeMessageInfo(SubscriptionNavigator, IDT, Purpose, InfoName, InfoValue);
        }

        public XPathNavigator SubscribePackLine(XPathNavigator SubscriptionNavigator, string HAWB, string SGID, string ConsignmentRef, string Status, string IDT, string Purpose)
        {
            return SGCustomsSubscriptionHelper.SubscribePackLine(SubscriptionNavigator, HAWB, SGID, ConsignmentRef, Status, IDT, Purpose);
        }

        public XPathNavigator SubscribeShipment(XPathNavigator SubscriptionNavigator, string ManifestNumber, string MAWB, string Purpose)
        {
            return SGCustomsSubscriptionHelper.SubscribeShipment(SubscriptionNavigator, ManifestNumber, MAWB, Purpose);
        }

        public string ToString(XPathNavigator SubscriptionNavigator)
        {
            return SGCustomsSubscriptionHelper.ToString(SubscriptionNavigator);
        }

        public string ValueMappingWithReturnValue(string condition, string value)
        {
            return StringMapper.ValueMappingWithReturnValue(condition, value);
        }

        public string ValueMappingWithReturnValue(string condition, string trueValue, string falseValue)
        {
            return StringMapper.ValueMappingWithReturnValue(condition, trueValue, falseValue);
        }

        #endregion
    }
}
