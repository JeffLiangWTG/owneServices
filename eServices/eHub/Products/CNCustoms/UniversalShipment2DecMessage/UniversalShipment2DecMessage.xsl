<xsl:stylesheet exclude-result-prefixes="msxsl var s0" version="1.0" xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:ScriptNS3="http://schemas.microsoft.com/BizTalk/2003/ScriptNS3"
                xmlns:ScriptNS4="http://schemas.microsoft.com/BizTalk/2003/ScriptNS4"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp"
                xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ns0="http://www.chinaport.gov.cn/dec"
                xmlns:gmi="http://cargowise.com/ehub/core/genericmessagedelivery"
                xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output indent="no" method="xml" omit-xml-declaration="yes" version="1.0"/>
  <xsl:variable name="UpperCases" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
  <xsl:variable name="LowerCases" select="'abcdefghijklmnopqrstuvwxyz'"/>
  <xsl:variable name="MessageSubType" select="/s0:UniversalShipment/s0:Shipment/s0:MessageSubType/s0:Code/text()"/>
  <xsl:variable name="EntryHeader" select="/s0:UniversalShipment/s0:Shipment/s0:EntryHeaderCollection/s0:EntryHeader[1]"/>
  <xsl:variable name="IsEnteringEntry" select="userCSharp:IsEnteringEntry(/s0:UniversalShipment/s0:Shipment/s0:MessageType/s0:Code/text(),/s0:UniversalShipment/s0:Shipment/s0:MessageSubType/s0:Code/text(), $EntryHeader/s0:Type/s0:Code/text())"/>
  <xsl:variable name="IsCustomsEntry" select="boolean($EntryHeader/s0:Type/s0:Code/text()='CUS')"/>
  <xsl:variable name="IsCrossBorder" select="userCSharp:IsCrossBorder(/s0:UniversalShipment/s0:Shipment/s0:PortOfLoading/s0:Code/text(),/s0:UniversalShipment/s0:Shipment/s0:PortOfDischarge/s0:Code/text())"/>
  <xsl:variable name="CreateDateTime" select="ScriptNS1:ConvertToDateTimeString(/s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:TriggerDate/text(),'','','','yyyyMMddHHmmss')"/>
  <xsl:key match="/s0:UniversalShipment/s0:Shipment/s0:AddInfoCollection/s0:AddInfo" name="ShipmentAddInfo" use="s0:Key"/>
  <xsl:key match="/s0:UniversalShipment/s0:Shipment/s0:OrganizationAddressCollection/s0:OrganizationAddress" name="ShipmentOrgAddress" use="s0:AddressType"/>
  <xsl:key match="/s0:UniversalShipment/s0:Shipment/s0:EntryHeaderCollection/s0:EntryHeader[1]/s0:AddInfoCollection/s0:AddInfo" name="EntryHeaderAddInfo" use="s0:Key"/>
  <xsl:variable name="InstructionLink" select="$EntryHeader/s0:EntryInstructionLink/text()"/>
  <xsl:variable name="LinkedEntryInstruction" select="/s0:UniversalShipment/s0:Shipment/s0:EntryInstructionCollection/s0:EntryInstruction[s0:Link=$InstructionLink]"/>
  <xsl:variable name="NeedToSumbitCIQData" select="($MessageSubType != 'BTH' or $IsCustomsEntry) and $LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQRequires' and s0:Value/text()='Y']"/>
  <xsl:variable name="NeedToSumbitSpecialDataForCIQ" select="boolean(0)"/>
  <xsl:variable name="InvoiceLinesLinkedToEntry" select="/s0:UniversalShipment/s0:Shipment/s0:CommercialInfo/s0:CommercialInvoiceCollection/s0:CommercialInvoice/s0:CommercialInvoiceLineCollection/s0:CommercialInvoiceLine[s0:EntryInstructionLink=$InstructionLink]"/>

  <xsl:variable name="TimeStamp" select="ScriptNS1:ConvertXmlDateString(userCSharp:GetCurrentDateTime('yyyy-MM-ddTHH:mm:ss.fff'), 'yyyyMMddHHmmssfff')"/>
  <xsl:variable name="filename" select="ScriptNS4:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat($EntryHeader/s0:Reference, '_', $TimeStamp))"/>
  <xsl:variable name="var:source-party" select="ScriptNS4:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="var:recipient">
    <xsl:variable name="DestinationParty">
      <xsl:value-of select="key('ShipmentAddInfo', 'DestinationParty')/s0:Value"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$DestinationParty = ''">
        <xsl:value-of select="concat($var:source-party, '_CSW')" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$DestinationParty"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="var:new-destination">
    <xsl:value-of select="ScriptNS4:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $var:recipient)"/>
  </xsl:variable>

  <xsl:variable name="ReqDocuments" select="$LinkedEntryInstruction/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code='RQD']"/>

  <xsl:template match="/">
    <xsl:element name="gmi:GenericMessageInterchange">
      <xsl:element name="gmi:Header">
        <xsl:element name="gmi:SenderID">
          <xsl:value-of select="$var:source-party"/>
        </xsl:element>
        <xsl:element name="gmi:RecipientID">
          <xsl:value-of select="$var:recipient"/>
        </xsl:element>
        <xsl:element name="gmi:InterchangeType">CSW</xsl:element>
        <xsl:element name="gmi:InterchangeNumber">
          <xsl:value-of select="substring-after(/s0:UniversalShipment/s0:Shipment/s0:DataContext/s0:EventReference/text(), 'RFN=')"/>
        </xsl:element>
      </xsl:element>
      <xsl:element name="gmi:Body">
        <xsl:for-each select="/s0:UniversalShipment/s0:Shipment/s0:EntryHeaderCollection/s0:EntryHeader">
          <xsl:call-template name="EntryHeader2DecMessage"/>
        </xsl:for-each>
        <xsl:element name="AttachedDocumentCollection">
          <xsl:for-each select="/s0:UniversalShipment/s0:Shipment/s0:AttachedDocumentCollection/s0:AttachedDocument">
            <xsl:element name="AttachedDocument">
              <xsl:element name="FileName">
                <xsl:value-of select="s0:FileName"/>
              </xsl:element>
              <xsl:element name="ImageData">
                <xsl:value-of select="s0:ImageData"/>
              </xsl:element>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="EntryHeader2DecMessage">
    <xsl:element name="ns0:DecMessage">
      <xsl:call-template name="HeaderItems"/>
      <xsl:element name="ns0:DecLists">
        <xsl:for-each select="./s0:EntryLineCollection/s0:EntryLine">
          <xsl:call-template name="EntryLine2DecList"/>
        </xsl:for-each>
      </xsl:element>

      <xsl:element name="ns0:DecContainers">
        <xsl:for-each select="/s0:UniversalShipment/s0:Shipment/s0:ContainerCollection/s0:Container">
          <xsl:call-template name="Container2DecContainer"/>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="ns0:DecLicenseDocus">
        <xsl:call-template name="LicenseDocus"/>
      </xsl:element>

      <xsl:call-template name="RequestCerts"/>

      <xsl:call-template name="OtherPacks"/>

      <xsl:call-template name="CopLimits"/>

      <xsl:call-template name="Users"/>

      <xsl:element name="ns0:DecFreeTxt">
        <xsl:call-template name="FreeText"/>
      </xsl:element>

      <xsl:element name="ns0:DecSign">
        <xsl:call-template name="Sign"/>
      </xsl:element>

      <xsl:call-template name="CopPromises"/>
      <xsl:call-template name="TpAccess"/>

      <xsl:call-template name="TEdocRealation"/>
      <xsl:call-template name="EcoRelation"/>
    </xsl:element>
  </xsl:template>

  <xsl:variable name="invoiceLineWithCO" select="$InvoiceLinesLinkedToEntry[s0:CustomsSupportingInformationCollection/s0:CustomsSupportingInformation/s0:Type/s0:Code/text()='1Y' and not(s0:CustomsSupportingInformationCollection/s0:CustomsSupportingInformation/s0:ReferenceNumber/text()='')][1]"/>
  <xsl:variable name="FirstLinkingInvoiceLine" select="$InvoiceLinesLinkedToEntry[1]"/>
  <xsl:variable name="FirstLinkingInvoiceHeader" select="$FirstLinkingInvoiceLine/../.."/>
  <xsl:variable name="CNTransportMode" select="key('ShipmentAddInfo', 'CNTransportMode')/s0:Value/text()"/>
  <xsl:variable name="TransportMode" select="/s0:UniversalShipment/s0:Shipment/s0:TransportMode/s0:Code/text()"/>
  <xsl:variable name="IEDate">
    <xsl:variable name="IEDateType">
      <xsl:choose>
        <xsl:when test="$IsEnteringEntry">DischargeDate</xsl:when>
        <xsl:otherwise>LoadingDate</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="ieDateText" select="/s0:UniversalShipment/s0:Shipment/s0:DateCollection/s0:Date[s0:Type=$IEDateType]/s0:Value/text()"/>
    <xsl:variable name="ieDateToShow">
      <xsl:choose>
        <xsl:when test="$ieDateText">
          <xsl:value-of select="ScriptNS1:ConvertToDateTimeString($ieDateText,'','','','yyyyMMdd')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="$IsEnteringEntry">
            <xsl:value-of select="userCSharp:GetCurrentDateTime('yyyyMMdd')"/>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="$ieDateToShow"/>
  </xsl:variable>
  <xsl:variable name="DeclarantOrg" select="key('ShipmentOrgAddress', 'Declarant')"/>
  <xsl:variable name="AgentCode" select="$DeclarantOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='CCD' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
  <xsl:variable name="TradeOrgAddressType" >
    <xsl:choose>
      <xsl:when test="$IsEnteringEntry">ImporterDocumentaryAddress</xsl:when>
      <xsl:otherwise>SupplierDocumentaryAddress</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="TradeOrg" select="key('ShipmentOrgAddress', $TradeOrgAddressType)"/>
  <xsl:variable name="TradeOrgCnName">
    <xsl:call-template name="GetOrgCnNameFallbackToCompanyName">
      <xsl:with-param name="OrgAddress" select="$TradeOrg" />
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="OwnerOrgAddressType" >
    <xsl:choose>
      <xsl:when test="$IsEnteringEntry">BuyerDocumentaryAddress</xsl:when>
      <xsl:otherwise>Manufacturer</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="OwnerOrgNode">
    <xsl:variable name="OwnerOrgAddrss" select="key('ShipmentOrgAddress', $OwnerOrgAddressType)"/>
    <xsl:choose>
      <xsl:when test="$OwnerOrgAddrss">
        <xsl:copy-of select="$OwnerOrgAddrss"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="$TradeOrg"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="OwnerOrg" select="msxsl:node-set($OwnerOrgNode)/*"/>
  <xsl:variable name="OwnerCode" select="$OwnerOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='CCD' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
  <xsl:variable name="OwnerName">
    <xsl:call-template name="GetOrgCnNameFallbackToCompanyName">
      <xsl:with-param name="OrgAddress" select="$OwnerOrg" />
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="OverseasOrgAddressType" >
    <xsl:choose>
      <xsl:when test="$IsEnteringEntry">SupplierDocumentaryAddress</xsl:when>
      <xsl:otherwise>ImporterDocumentaryAddress</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="OverseasOrgNode">
    <xsl:variable name="OverseasOrgAddress" select="key('ShipmentOrgAddress', $OverseasOrgAddressType)"/>
    <xsl:if test="$OverseasOrgAddress/s0:Country/s0:Code/text() != 'CN'">
      <xsl:copy-of select="$OverseasOrgAddress" />
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="OverseasOrg" select="msxsl:node-set($OverseasOrgNode)/*" />

  <!-- 进口/出口报关单表头 DecHead -->
  <xsl:template name="HeaderItems">
    <!-- seperator of Main Body -->
    <xsl:element name="ns0:DecHead">
      <xsl:element name="ns0:SeqNo">
        <xsl:value-of select="$EntryHeader/s0:EntryNumberCollection/s0:EntryNumber[s0:Type/s0:Code='UNI']/s0:Number"/>
      </xsl:element>
      <!-- 数据中心统一编号-->
      <xsl:element name="ns0:IEFlag">
        <xsl:choose>
          <xsl:when test="$IsEnteringEntry">
            <xsl:value-of select="'I'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'E'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 进出口标志 -->
      <xsl:element name="ns0:Type">
        <xsl:variable name="IsPTF" select="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='OPM' and s0:SubType/s0:Code='PTF']" />
        <xsl:variable name="IsATF" select="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='OPM' and s0:SubType/s0:Code='ATF']" />
        <xsl:variable name="IsCDC" select="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='OPM' and s0:SubType/s0:Code='CDC']" />
        <xsl:choose>
          <!--<xsl:when test="$IsATF">ZB</xsl:when>
          <xsl:when test="$IsPTF">SW</xsl:when>-->
          <xsl:when test="$IsCustomsEntry">
            <xsl:choose>
              <xsl:when test="$IsCDC">CL</xsl:when>
              <xsl:otherwise xml:space="preserve">  </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="not($IsCustomsEntry)">ML</xsl:when>
        </xsl:choose>
      </xsl:element>
      <!-- 单据类型 -->
      <!-- TODO: Check -->
      <xsl:element name="ns0:AgentCode">
        <xsl:value-of select="$AgentCode"/>
      </xsl:element>
      <!-- 申报单位代码 -->
      <xsl:variable name="AgentName">
        <xsl:call-template name="GetOrgCnNameFallbackToCompanyName">
          <xsl:with-param name="OrgAddress" select="$DeclarantOrg" />
        </xsl:call-template>
      </xsl:variable>
      <xsl:element name="ns0:AgentName">
        <xsl:value-of select="$AgentName"/>
      </xsl:element>
      <!-- 申报单位名称 -->
      <xsl:element name="ns0:ApprNo"/>
      <!-- Keep Empty 批准文号 -->
      <xsl:element name="ns0:BillNo">
        <xsl:choose>
          <xsl:when test="$EntryHeader/s0:AddInfoCollection/s0:AddInfo[s0:Key='BillOfLading']">
            <xsl:value-of select="$EntryHeader/s0:AddInfoCollection/s0:AddInfo[s0:Key='BillOfLading']/s0:Value"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='BillOfLading']/s0:Value"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 提单号 -->
      <xsl:element name="ns0:ContrNo">
        <xsl:value-of select="ScriptNS3:DeDuplicateValuesAndConcatenate($InvoiceLinesLinkedToEntry/../../s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='CTR']/s0:Reference, ',', 32, '等', boolean(0))"/>
      </xsl:element>
      <!-- 合同号 -->
      <xsl:element name="ns0:CustomMaster">
        <xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:CustomsOffice/s0:Code"/>
      </xsl:element>
      <!-- 主管海关（申报地海关） -->
      <xsl:element name="ns0:CutMode">
        <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='LevyType']/s0:Value"/>
      </xsl:element>
      <!-- 征免性质 -->
      <xsl:element name="ns0:DistinatePort">
        <xsl:choose>
          <xsl:when test="$IsEnteringEntry">
            <xsl:value-of select="key('ShipmentAddInfo', 'CNLastPortBeforeEntry')/s0:Value"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="key('ShipmentAddInfo', 'CNPortOfDestination')/s0:Value"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 经停港/指运港 -->
      <xsl:element name="ns0:FeeCurr">
        <xsl:value-of select="key('EntryHeaderAddInfo', 'FreightFeeCurrencyCode')/s0:Value"/>
      </xsl:element>
      <!-- 运费币制 AddInfo on EntryHeader -->
      <xsl:element name="ns0:FeeMark">
        <xsl:value-of select="key('EntryHeaderAddInfo', 'FreightFeeMarkCode')/s0:Value"/>
      </xsl:element>
      <!-- 运费标记 AddInfo on EntryHeader -->
      <xsl:element name="ns0:FeeRate">
        <xsl:value-of select="key('EntryHeaderAddInfo', 'FreightFeeAmount')/s0:Value"/>
      </xsl:element>
      <!-- 运费／率 AddInfo on EntryHeader -->
      <xsl:variable name="weightsOnInvoiceLines">
        <xsl:for-each select="$InvoiceLinesLinkedToEntry">
          <xsl:element name="GrossWeightInKG">
            <xsl:value-of select="ScriptNS0:Convert(./s0:Weight/text(), ./s0:WeightUnit/s0:Code/text(), 'KG')"/>
          </xsl:element>
          <xsl:element name="NetWeightInKG">
            <xsl:value-of select="ScriptNS0:Convert(./s0:NetWeight/text(), ./s0:NetWeightUnit/s0:Code/text(), 'KG')"/>
          </xsl:element>
        </xsl:for-each>
      </xsl:variable>
      <xsl:element name="ns0:GrossWet">
        <xsl:value-of select="ScriptNS3:Sum(msxsl:node-set($weightsOnInvoiceLines)/GrossWeightInKG, 2, 0.01)"/>
      </xsl:element>
      <!-- 毛重 -->
      <xsl:element name="ns0:IEDate">
        <xsl:if test="$IsEnteringEntry">
          <xsl:value-of select="$IEDate"/>
        </xsl:if>
      </xsl:element>
      <!-- 进出口日期 -->
      <xsl:element name="ns0:IEPort">
        <xsl:value-of select="key('ShipmentAddInfo', 'OfficeOfEntryExit')/s0:Value"/>
      </xsl:element>
      <!-- 进出口岸 -->
      <xsl:element name="ns0:InsurCurr">
        <xsl:value-of select="key('EntryHeaderAddInfo', 'InsuranceFeeCurrencyCode')/s0:Value"/>
      </xsl:element>
      <!-- 保险费币制 AddInfo on EntryHeader -->
      <xsl:element name="ns0:InsurMark">
        <xsl:value-of select="key('EntryHeaderAddInfo', 'InsuranceFeeMarkCode')/s0:Value"/>
      </xsl:element>
      <!-- 保险费标记 AddInfo on EntryHeader -->
      <xsl:element name="ns0:InsurRate">
        <xsl:value-of select="key('EntryHeaderAddInfo', 'InsuranceFeeAmount')/s0:Value"/>
      </xsl:element>
      <!-- 保险费／率 AddInfo on EntryHeader -->
      <xsl:element name="ns0:LicenseNo">
        <xsl:choose>
          <xsl:when test ="not($IsCustomsEntry) and $MessageSubType = 'BTH'"></xsl:when>
          <xsl:when test="$IsEnteringEntry">
            <xsl:value-of select="$InvoiceLinesLinkedToEntry/s0:CustomsSupportingInformationCollection/s0:CustomsSupportingInformation[s0:Category/s0:Code='SUP' and (s0:Type/s0:Code='01' or s0:Type/s0:Code='02')]/s0:ReferenceNumber"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$InvoiceLinesLinkedToEntry/s0:CustomsSupportingInformationCollection/s0:CustomsSupportingInformation[s0:Category/s0:Code='SUP' and (s0:Type/s0:Code='03' or s0:Type/s0:Code='04' or s0:Type/s0:Code='05' or s0:Type/s0:Code='0x' or s0:Type/s0:Code='0y' or s0:Type/s0:Code='1G')]/s0:ReferenceNumber"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 许可证编号 -->
      <xsl:element name="ns0:ManualNo">
        <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='ManualNo']/s0:Value"/>
      </xsl:element>
      <!-- 备案号 -->
      <xsl:element name="ns0:NetWt">
				<xsl:value-of select="ScriptNS3:Sum(msxsl:node-set($weightsOnInvoiceLines)/NetWeightInKG, 2, 0.01)"/>
      </xsl:element>
      <!-- 净重 -->
      <xsl:element name="ns0:NoteS">
        <xsl:choose>
          <xsl:when test="key('EntryHeaderAddInfo', 'Remarks')">
            <xsl:value-of select="key('EntryHeaderAddInfo', 'Remarks')/s0:Value"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='CustomsMessageRemarks']/s0:Value"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 备注 -->
      <xsl:element name="ns0:OtherCurr">
        <xsl:value-of select="key('EntryHeaderAddInfo', 'OtherFeeCurrencyCode')/s0:Value"/>
      </xsl:element>
      <!-- Keep Empty 杂费币制 -->
      <xsl:element name="ns0:OtherMark">
        <xsl:value-of select="key('EntryHeaderAddInfo', 'OtherFeeMarkCode')/s0:Value"/>
      </xsl:element>
      <!-- Keep Empty 杂费标志 -->
      <xsl:element name="ns0:OtherRate">
        <xsl:value-of select="key('EntryHeaderAddInfo', 'OtherFeeAmount')/s0:Value"/>
      </xsl:element>
      <!-- Keep Empty 杂费／率 -->
      <xsl:element name="ns0:OwnerCode">
        <xsl:value-of select="$OwnerCode"/>
      </xsl:element>
      <!-- 消费使用/生产销售单位代码 -->
      <xsl:element name="ns0:OwnerName">
        <xsl:value-of select="$OwnerName"/>
      </xsl:element>
      <!-- 消费使用/生产销售单位名称 -->
      <xsl:element name="ns0:PackNo">
        <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='Packages']/s0:Value"/>
      </xsl:element>
      <!-- 件数 -->
      <xsl:element name="ns0:TradeCode">
        <xsl:value-of select="$TradeOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='CCD' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
      </xsl:element>
      <!-- 境内收发货人编号 -->
      <xsl:element name="ns0:TradeCountry">
        <xsl:variable name="TradeCountry">
          <xsl:choose>
            <xsl:when test="$IsEnteringEntry">
              <xsl:value-of select="substring(/s0:UniversalShipment/s0:Shipment/s0:PortOfLoading/s0:Code/text(), 1, 2)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="substring(/s0:UniversalShipment/s0:Shipment/s0:PortOfDischarge/s0:Code/text(), 1, 2)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="GetIsoAlpha3CountryCode">
          <xsl:with-param name="CountryCode" select="$TradeCountry"/>
        </xsl:call-template>
      </xsl:element>
      <!-- 启运国/运抵国 -->
      <xsl:element name="ns0:TradeMode">
        <xsl:value-of select="$LinkedEntryInstruction/s0:Style"/>
      </xsl:element>
      <!-- 监管方式 -->
      <xsl:element name="ns0:TradeName">
        <xsl:value-of select="$TradeOrgCnName"/>
      </xsl:element>
      <!-- 境内收发货人名称 -->
      <xsl:element name="ns0:TrafMode">
        <xsl:choose>
          <xsl:when test="$IsCrossBorder">
            <xsl:choose>
              <xsl:when test="$TransportMode='SEA'">2</xsl:when>
              <xsl:when test="$TransportMode='RAI'">3</xsl:when>
              <xsl:when test="$TransportMode='ROA'">4</xsl:when>
              <xsl:when test="$TransportMode='AIR'">5</xsl:when>
              <xsl:when test="$TransportMode='MAI'">6</xsl:when>
              <xsl:when test="$TransportMode='PHC'">L</xsl:when>
              <xsl:when test="$TransportMode='FIX'">G</xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="$IsCustomsEntry">
                <xsl:value-of select="$CNTransportMode"/>
              </xsl:when>
              <xsl:otherwise>9</xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 运输方式代码 -->
      <xsl:element name="ns0:TrafName">
        <xsl:choose>
          <xsl:when test="$EntryHeader/s0:AddInfoCollection/s0:AddInfo[s0:Key='VesselName']">
            <xsl:value-of select="$EntryHeader/s0:AddInfoCollection/s0:AddInfo[s0:Key='VesselName']/s0:Value"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="$IsCrossBorder">
                <xsl:choose>
                  <xsl:when test="$TransportMode='SEA' or $TransportMode='RAI'">
                    <xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:VesselName"/>
                  </xsl:when>
                  <xsl:when test="$TransportMode='MAI'">
                    <xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:AdditionalBillCollection/s0:AdditionalBill[s0:BillType/s0:Code='MWB']/s0:BillNumber"/>
                  </xsl:when>
                  <xsl:when test="$TransportMode='PHC'">旅客携带</xsl:when>
                  <xsl:when test="$TransportMode='FIX'">管道</xsl:when>
                </xsl:choose>
              </xsl:when>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>

      </xsl:element>
      <!-- 运输工具代码及名称 -->
      <xsl:element name="ns0:TransMode">
        <xsl:variable name="IncoTerm" select="$FirstLinkingInvoiceHeader/s0:IncoTerm/s0:Code/text()"/>
        <xsl:choose>
          <xsl:when test="$IsCrossBorder or $IsCustomsEntry">
            <xsl:choose>
              <xsl:when test="$IncoTerm='CIF'">1</xsl:when>
              <xsl:when test="$IncoTerm='CAF'">2</xsl:when>
              <xsl:when test="$IncoTerm='FOB'">3</xsl:when>
              <xsl:when test="$IncoTerm='CAI'">4</xsl:when>
              <xsl:when test="$IncoTerm='EXW'">7</xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="$IsEnteringEntry">1</xsl:when>
              <xsl:otherwise>3</xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 成交方式 -->
      <xsl:element name="ns0:WrapType">
        <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='PackageUQ']/s0:Value"/>
      </xsl:element>
      <!-- 包装种类 -->
      <xsl:element name="ns0:EntryId"/>
      <xsl:element name="ns0:PreEntryId"/>
      <!-- TODO: Check -->
      <xsl:element name="ns0:EdiId">1</xsl:element>
      <!-- 报关标志 -->
      <xsl:element name="ns0:Risk"/>
      <!-- Keep Empty -->
      <xsl:element name="ns0:CopName">
        <xsl:value-of select="$AgentName"/>
      </xsl:element>
      <xsl:element name="ns0:CopCode">
        <xsl:value-of select="$AgentCode"/>
      </xsl:element>
      <!-- Keep Empty -->
      <!-- TODO: Check -->
      <xsl:element name="ns0:EntryType">
        <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='DocumentSubmissionType']/s0:Value"/>
      </xsl:element>
      <!-- 报关单类型 -->
      <xsl:element name="ns0:PDate">
        <xsl:value-of select="userCSharp:GetCurrentDateTime('yyyyMMdd')"/>
      </xsl:element>
      <!-- 打印日期 -->
      <xsl:element name="ns0:TypistNo">
        <xsl:value-of select="key('ShipmentAddInfo', 'OperatorCardID')/s0:Value"/>
      </xsl:element>
      <!-- 录入员IC卡号 Keep Empty -->
      <xsl:element name="ns0:InputerName">
        <xsl:value-of select="key('ShipmentAddInfo', 'OperatorName')/s0:Value"/>
      </xsl:element>
      <!-- 录入员名称 Keep Empty -->
      <xsl:element name="ns0:PartenerID">
        <xsl:value-of select="key('ShipmentAddInfo', 'BrokerName')/s0:Value"/>
      </xsl:element>
      <!-- 申报人标识 Keep Empty -->
      <!-- TODO: Check -->
      <xsl:element name="ns0:TgdNo"/>
      <!-- 宁波通关申请单号 Keep Empty -->
      <!-- TODO: Check -->
      <xsl:element name="ns0:DataSource">
        <xsl:variable name="DecType">
          <xsl:value-of select="$LinkedEntryInstruction/s0:SubStyle/s0:Code"/>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="$DecType != ''">
            <xsl:value-of select="concat('    ', $DecType)" />
          </xsl:when>
        </xsl:choose>
      </xsl:element>
      <!-- 扩展字段 -->
      <xsl:element name="ns0:DeclTrnRel">0</xsl:element>
      <!-- 报关/转关关系标志 -->
      <xsl:element name="ns0:ChkSurety">
        <xsl:choose>
          <xsl:when test="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='OPM' and s0:SubType/s0:Code='AIC']">1</xsl:when>
          <xsl:otherwise>0</xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 担保验放标志 -->
      <xsl:element name="ns0:BillType">
        <xsl:if test="not($IsCustomsEntry)">1</xsl:if>
      </xsl:element>
      <!-- 备案清单类型 -->
      <!-- TODO: Check -->
      <xsl:variable name="CopCodeScc" select="$DeclarantOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='USC' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
      <xsl:element name="ns0:CopCodeScc">
        <xsl:value-of select="$CopCodeScc"/>
      </xsl:element>
      <!-- 录入单位统一编码 Keep Empty -->
      <!-- TODO: Check -->
      <xsl:element name="ns0:OwnerCodeScc">
        <xsl:value-of select="$OwnerOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='USC' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
      </xsl:element>
      <!-- 消费使用/生产销售单位单位统一编码 -->
      <xsl:element name="ns0:AgentCodeScc">
        <xsl:value-of select="$CopCodeScc"/>
      </xsl:element>
      <!-- 申报代码统一编码 -->
      <xsl:element name="ns0:TradeCoScc">
        <xsl:value-of select="$TradeOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='USC' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
      </xsl:element>
      <!-- 收发货人统一编码 -->
      <xsl:element name="ns0:PromiseItmes">
        <xsl:value-of select="userCSharp:GetPromiseItmes($FirstLinkingInvoiceHeader/s0:AddInfoCollection/s0:AddInfo[s0:Key='SpecialRelationshipConfirm']/s0:Value/text(), $FirstLinkingInvoiceHeader/s0:AddInfoCollection/s0:AddInfo[s0:Key='PriceAffectConfirm']/s0:Value/text(), $FirstLinkingInvoiceHeader/s0:AddInfoCollection/s0:AddInfo[s0:Key='PaymentOfRoyaltyConfirm']/s0:Value/text(), $FirstLinkingInvoiceHeader/s0:ValuationCode/s0:Code/text())"/>
      </xsl:element>
      <!-- 承诺事项 -->
      <xsl:element name="ns0:TradeAreaCode">
        <xsl:call-template name="GetIsoAlpha3CountryCode">
          <xsl:with-param name="CountryCode" select="key('ShipmentAddInfo', 'RN_NKCountryOfTrade')/s0:Value"/>
        </xsl:call-template>
      </xsl:element>
      <!-- 贸易国别 -->
      <xsl:element name="ns0:CheckFlow">0</xsl:element>
      <!-- 查验分流 Keep Empty -->
      <!-- TODO: Check: Pending CSY what is this -->
      <xsl:element name="ns0:TaxAaminMark">
      </xsl:element>
      <!-- 税收征管标记 Keep Empty -->
      <!-- TODO: Check: Pending CSY what is this -->
      <xsl:element name="ns0:MarkNo">
        <xsl:variable name="markNo" select="key('EntryHeaderAddInfo', 'MarksAndNumbers')/s0:Value"/>
        <xsl:choose>
          <xsl:when test="$markNo">
            <xsl:value-of select="$markNo"/>
          </xsl:when>
          <xsl:otherwise>N/M</xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 标记唛码 -->
      <xsl:element name="ns0:DespPortCode">
        <xsl:if test="$IsEnteringEntry">
          <xsl:value-of select="key('ShipmentAddInfo', 'CNPortOfOrigin')/s0:Value"/>
        </xsl:if>
      </xsl:element>
      <!-- 启运港代码 -->
      <xsl:element name="ns0:EntyPortCode">
        <xsl:value-of select="key('ShipmentAddInfo', 'CIQOfficeOfEntryExit')/s0:Value"/>
      </xsl:element>
      <!-- 入/离境口岸代码 -->
      <xsl:element name="ns0:GoodsPlace">
        <xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:LocationAtClearance/s0:Code"/>
      </xsl:element>
      <!-- 存放地点 -->
      <xsl:element name="ns0:BLNo">
        <xsl:if test="$NeedToSumbitCIQData and $IsEnteringEntry and $TransportMode='SEA'">
          <xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:AdditionalBillCollection/s0:AdditionalBill[s0:BillType/s0:Code='MWB']/s0:BillNumber"/>
        </xsl:if>
      </xsl:element>
      <!-- B/L号 -->
      <xsl:element name="ns0:InspOrgCode"></xsl:element>
      <!-- 口岸检验检疫机关（字段已废弃，不再使用） -->
      <xsl:element name="ns0:SpecDeclFlag">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:choose>
            <xsl:when test="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='SBI' and s0:SubType/s0:Code='B01']">
              <xsl:value-of select="'1'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'0'"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:choose>
            <xsl:when test="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='SBI' and s0:SubType/s0:Code='B02']">
              <xsl:value-of select="'1'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'0'"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:choose>
            <xsl:when test="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='SBI' and s0:SubType/s0:Code='B03']">
              <xsl:value-of select="'1'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'0'"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:choose>
            <xsl:when test="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='SBI' and s0:SubType/s0:Code='B04']">
              <xsl:value-of select="'1'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'0'"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:choose>
            <xsl:when test="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='SBI' and s0:SubType/s0:Code='C01']">
              <xsl:value-of select="'1'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'0'"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:choose>
            <xsl:when test="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='SBI' and s0:SubType/s0:Code='C02']">
              <xsl:value-of select="'1'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'0'"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:choose>
            <xsl:when test="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='SBI' and s0:SubType/s0:Code='C03']">
              <xsl:value-of select="'1'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'0'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:element>
      <!-- 特种业务标识 -->
      <xsl:element name="ns0:PurpOrgCode">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='COF' and s0:SubType/s0:Code='DES']/s0:Reference"/>
        </xsl:if>
      </xsl:element>
      <!-- 目的地海关 -->
      <xsl:element name="ns0:DespDate">
        <xsl:if test="$IsEnteringEntry">
          <xsl:value-of select="ScriptNS1:ConvertToDateTimeString(/s0:UniversalShipment/s0:Shipment/s0:DateCollection/s0:Date[s0:Type='LoadingDate']/s0:Value,'','','','yyyyMMdd')"/>
        </xsl:if>
      </xsl:element>
      <!-- 启运日期 -->
      <xsl:element name="ns0:CmplDschrgDt">
        <xsl:if test="$NeedToSumbitCIQData and $IsEnteringEntry and $ReqDocuments">
          <xsl:value-of select="ScriptNS1:ConvertToDateTimeString(key('ShipmentAddInfo', 'DateOfUnloadComplete')/s0:Value,'','','','yyyyMMdd')"/>
        </xsl:if>
      </xsl:element>
      <!-- 卸毕日期 -->
      <xsl:element name="ns0:CorrelationReasonFlag">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQRelatedReason']/s0:Value"/>
        </xsl:if>
      </xsl:element>
      <!-- 关联理由 -->
      <xsl:element name="ns0:VsaOrgCode"></xsl:element>
      <!-- 领证机关（字段已废弃，不再使用） -->
      <xsl:element name="ns0:OrigBoxFlag">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:choose>
            <xsl:when test="$InvoiceLinesLinkedToEntry/s0:AddInfoCollection/s0:AddInfo[s0:Key='OrigContainerFlag' and (s0:Value='Y' or s0:Value='1')]">1</xsl:when>
            <xsl:when test="$InvoiceLinesLinkedToEntry/s0:AddInfoCollection/s0:AddInfo[s0:Key='OrigContainerFlag' and (s0:Value='N' or s0:Value='0')]">0</xsl:when>
          </xsl:choose>
        </xsl:if>
      </xsl:element>
      <!-- 原集装箱标识 -->
      <xsl:element name="ns0:DeclareName">
        <xsl:value-of select="key('ShipmentAddInfo', 'BrokerName')/s0:Value"/>
      </xsl:element>
      <!-- 申报人员姓名 -->
      <xsl:element name="ns0:NoOtherPack">
        <xsl:choose>
          <xsl:when test="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='PKG']">0</xsl:when>
          <xsl:otherwise>1</xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 无其他包装 -->
      <xsl:element name="ns0:OrgCode"></xsl:element>
      <!-- 检验检疫受理机关（字段已废弃，不再使用） -->
      <xsl:element name="ns0:OverseasConsignorCode">
        <xsl:if test="$IsEnteringEntry">
          <xsl:variable name="OverseasOrgCode">
            <xsl:value-of select="key('EntryHeaderAddInfo', 'OverseasPartyCode')/s0:Value"/>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="$OverseasOrgCode = ''">NO</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$OverseasOrgCode"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:element>
      <!-- 境外发货人代码 -->
      <xsl:element name="ns0:OverseasConsignorCname">
        <xsl:if test="$NeedToSumbitCIQData and $NeedToSumbitSpecialDataForCIQ">
          <xsl:call-template name="GetOrgCnName">
            <xsl:with-param name="OrgAddress" select="$OverseasOrg" />
          </xsl:call-template>
        </xsl:if>
      </xsl:element>
      <!-- 境外收发货人名称（中文） -->
      <xsl:element name="ns0:OverseasConsignorEname">
        <xsl:if test="$IsEnteringEntry">
          <xsl:variable name="OverseasOrgEnName">
            <xsl:call-template name="GetOrgEnNameFallbackToCompanyName">
              <xsl:with-param name="OrgAddress" select="$OverseasOrg" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="$OverseasOrgEnName = ''">NO</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$OverseasOrgEnName"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:element>
      <!-- 境外发货人名称（外文） -->
      <xsl:element name="ns0:OverseasConsignorAddr">
        <xsl:if test="$NeedToSumbitCIQData and $IsEnteringEntry and $NeedToSumbitSpecialDataForCIQ">
          <xsl:value-of select="$OverseasOrg/s0:Address1"/>
        </xsl:if>
      </xsl:element>
      <!-- 境外收发货人地址 -->
      <xsl:element name="ns0:OverseasConsigneeCode">
        <xsl:if test="not($IsEnteringEntry)">
          <xsl:variable name="OverseasOrgCode">
            <xsl:value-of select="key('EntryHeaderAddInfo', 'OverseasPartyCode')/s0:Value"/>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="$OverseasOrgCode = ''">NO</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$OverseasOrgCode"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:element>
      <!-- 境外收货人编码 -->
      <xsl:element name="ns0:OverseasConsigneeEname">
        <xsl:if test="not($IsEnteringEntry)">
          <xsl:variable name="OverseasOrgEnName">
            <xsl:call-template name="GetOrgEnNameFallbackToCompanyName">
              <xsl:with-param name="OrgAddress" select="$OverseasOrg" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="$OverseasOrgEnName = ''">NO</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$OverseasOrgEnName"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:element>
      <!-- 境外收货人名称(外文) -->
      <xsl:element name="ns0:DomesticConsigneeEname">
        <xsl:if test="$NeedToSumbitCIQData and $NeedToSumbitSpecialDataForCIQ">
          <xsl:variable name="TradeOrgEnName">
            <xsl:call-template name="GetOrgEnNameFallbackToCompanyName">
              <xsl:with-param name="OrgAddress" select="$TradeOrg" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:if test ="$TradeOrgCnName != $TradeOrgEnName">
            <xsl:value-of select="$TradeOrgEnName" />
          </xsl:if>
        </xsl:if>
      </xsl:element>
      <!-- 境内收发货人名称（外文） -->
      <xsl:element name="ns0:CorrelationNo">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQRelatedNum']/s0:Value"/>
        </xsl:if>
      </xsl:element>
      <!-- 关联号码 -->
      <xsl:element name="ns0:EdiRemark2"></xsl:element>
      <!-- EDI申报备注2 Keep Empty -->
      <xsl:element name="ns0:EdiRemark"></xsl:element>
      <!-- EDI申报备注 Keep Empty -->
      <xsl:element name="ns0:TradeCiqCode">
        <xsl:value-of select="$TradeOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='CIQ' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
      </xsl:element>
      <!-- 境内收发货人检验检疫编码 -->
      <xsl:element name="ns0:OwnerCiqCode">
        <xsl:value-of select="$OwnerOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='CIQ' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
      </xsl:element>
      <!-- 消费使用/生产销售单位检验检疫编码 -->
      <xsl:element name="ns0:DeclCiqCode">
        <xsl:value-of select="$DeclarantOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='CIQ' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
      </xsl:element>
      <!-- 申报单位检验检疫编码 -->
    </xsl:element>
  </xsl:template>

  <!-- 进口/出口报关单表体 DecList -->
  <xsl:template name="EntryLine2DecList">
    <xsl:variable name="EntryLineNumber" select="./s0:LineNumber/text()"/>
    <xsl:variable name="LinkedInvoiceLinesToEntryLine" select="$InvoiceLinesLinkedToEntry[s0:EntryLineNumber=$EntryLineNumber]"/>
    <xsl:variable name="FirstLinkedInvoiceLine" select="$LinkedInvoiceLinesToEntryLine[1]"/>
    <xsl:variable name="quantitiesOnInvoiceLines">
      <xsl:for-each select="$LinkedInvoiceLinesToEntryLine">
        <xsl:element name="TradeQuantity">
          <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TradeQuantity']/s0:Value"/>
        </xsl:element>
        <xsl:element name="CustomsQuantity">
          <xsl:value-of select="./s0:CustomsQuantity/text()"/>
        </xsl:element>
        <xsl:element name="CustomsSecondQuantity">
          <xsl:value-of select="./s0:CustomsSecondQuantity/text()"/>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="CountryOfOriginAlp3">
      <xsl:call-template name="GetIsoAlpha3CountryCode">
        <xsl:with-param name="CountryCode" select="$FirstLinkedInvoiceLine/s0:CountryOfOrigin/s0:Code/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="CountryOfExportAlp3">
      <xsl:call-template name="GetIsoAlpha3CountryCode">
        <xsl:with-param name="CountryCode" select="$FirstLinkedInvoiceLine/s0:CountryOfExport/s0:Code/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="Manufacturer" select="$FirstLinkedInvoiceLine/s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType='Manufacturer']"></xsl:variable>
    <xsl:variable name="ManufacturerIsInChina" select="$Manufacturer/s0:Country/s0:Code/text() = 'CN'"></xsl:variable>

    <xsl:element name="ns0:DecList">
      <xsl:element name="ns0:ClassMark"></xsl:element>
      <!-- 归类标志 Fixed Empty -->
      <xsl:element name="ns0:CodeTS">
        <xsl:value-of select="./s0:HarmonisedCode"/>
      </xsl:element>
      <!-- 商品编号 -->
      <xsl:element name="ns0:ContrItem">
        <xsl:choose>
          <xsl:when test="$MessageSubType='BTH' and not($IsEnteringEntry)">
            <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='ProductManualNo2']/s0:Value"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='ProductManualNo']/s0:Value"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 备案序号 -->
      <xsl:element name="ns0:DeclPrice">
        <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='UnitPrice']/s0:Value"/>
      </xsl:element>
      <!-- 申报单价 -->
      <xsl:element name="ns0:DutyMode">
        <xsl:choose>
          <xsl:when test="$MessageSubType='BTH' and not($IsCustomsEntry)">3</xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='DutyMode']/s0:Value"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 征减免税方式 -->
      <xsl:element name="ns0:Factor">
        <!-- TODO: Check: Empty for Now, Pending CSY -->
      </xsl:element>
      <!-- 申报计量单位与法定单位比例因子 -->
      <xsl:element name="ns0:GModel">
        <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='GoodsSpecModel']/s0:Value"/>
      </xsl:element>
      <!-- 商品规格、型号 -->
      <xsl:element name="ns0:GName">
        <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='NameOfGoods']/s0:Value"/>
      </xsl:element>
      <!-- 商品名称 -->
      <xsl:element name="ns0:GNo">
        <xsl:value-of select="$EntryLineNumber"/>
      </xsl:element>
      <!-- 商品序号 -->
      <xsl:element name="ns0:OriginCountry">
        <xsl:choose>
          <xsl:when test="$IsEnteringEntry">
            <xsl:value-of select="$CountryOfOriginAlp3"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$CountryOfExportAlp3"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 原产国 -->
      <xsl:element name="ns0:TradeCurr">
        <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='CurrencyCode']/s0:Value"/>
      </xsl:element>
      <!-- 成交币制 -->
      <xsl:element name="ns0:DeclTotal">
        <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='TotalPrice']/s0:Value"/>
      </xsl:element>
      <!-- 申报总价 -->
      <xsl:element name="ns0:GQty">
        <xsl:value-of select="floor(sum(msxsl:node-set($quantitiesOnInvoiceLines)/TradeQuantity/text()) * 100000) div 100000"/>
      </xsl:element>
      <!-- 成交数量 -->
      <xsl:element name="ns0:FirstQty">
        <xsl:value-of select="floor(sum(msxsl:node-set($quantitiesOnInvoiceLines)/CustomsQuantity/text()) * 100000) div 100000"/>
      </xsl:element>
      <!-- 第一法定数量 -->
      <xsl:variable name="SecondQtyUnit" select="$FirstLinkedInvoiceLine/s0:CustomsSecondQuantityUnit/s0:Code"></xsl:variable>
      <xsl:element name="ns0:SecondQty">
        <xsl:choose>
          <xsl:when test="$SecondQtyUnit != ''">
            <xsl:value-of select="floor(sum(msxsl:node-set($quantitiesOnInvoiceLines)/CustomsSecondQuantity/text()) * 100000) div 100000"/>
          </xsl:when>
          <xsl:otherwise>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 第二法定数量 -->
      <xsl:element name="ns0:GUnit">
        <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TradeUnitQty']/s0:Value"/>
      </xsl:element>
      <!-- 成交计量单位 -->
      <xsl:element name="ns0:FirstUnit">
        <xsl:value-of select="$FirstLinkedInvoiceLine/s0:CustomsQuantityUnit/s0:Code"/>
      </xsl:element>
      <!-- 第一计量单位（法定单位） -->
      <xsl:element name="ns0:SecondUnit">
        <xsl:value-of select="$SecondQtyUnit"/>
      </xsl:element>
      <!--第二计量单位 -->
      <xsl:element name="ns0:UseTo">
        <!-- TODO: Check: fixed as empty-->
      </xsl:element>
      <!--用途代码 -->
      <xsl:element name="ns0:WorkUsd">
        <!-- TODO: Check: fixed as empty-->
      </xsl:element>
      <!--工缴费 -->
      <xsl:element name="ns0:ExgNo">
        <!-- <xsl:value-of select="$FirstLinkedInvoiceLine/s0:PartNo"/>-->
      </xsl:element>
      <!-- 货号 -->
      <xsl:element name="ns0:ExgVersion">
        <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='ProductVersion']/s0:Value"/>
      </xsl:element>
      <!-- 版本号 -->
      <xsl:element name="ns0:DestinationCountry">
        <xsl:choose>
          <xsl:when test="$IsEnteringEntry">
            <xsl:value-of select="$CountryOfExportAlp3"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$CountryOfOriginAlp3"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 最终目的国（地区） -->
      <xsl:element name="ns0:CiqCode">
        <xsl:value-of select="substring($FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQTariff']/s0:Value, 11, 3)"/>
      </xsl:element>
      <!-- 检验检疫编码 -->
      <xsl:element name="ns0:DeclGoodsEname">
        <xsl:if test="$NeedToSumbitCIQData and $NeedToSumbitSpecialDataForCIQ">
          <xsl:value-of select="./s0:Description"/>
        </xsl:if>
      </xsl:element>
      <!-- TODO: 商品英文名称: check with CSY if we need it  -->
      <xsl:element name="ns0:OrigPlaceCode">
        <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQOriginState']/s0:Value"/>
      </xsl:element>
      <!-- 原产地区代码 -->
      <xsl:element name="ns0:Purpose">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQEndUse']/s0:Value"/>
        </xsl:if>
      </xsl:element>
      <!-- 用途代码 -->
      <xsl:element name="ns0:ProdValidDt">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:variable name="ExpiryDates">
            <xsl:for-each select="$LinkedInvoiceLinesToEntryLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQExpiryDate']/s0:Value">
              <xsl:element name="Value">
                <xsl:value-of select="ScriptNS1:ConvertToDateTimeString(.,'','','','yyyyMMdd')" />
              </xsl:element>
            </xsl:for-each>
          </xsl:variable>
          <xsl:value-of select="ScriptNS3:GetSmallestValue(msxsl:node-set($ExpiryDates)/*)"/>
        </xsl:if>
      </xsl:element>
      <!-- 产品有效期 -->
      <xsl:element name="ns0:ProdQgp">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQQualityGuaranteePeriod']/s0:Value"/>
        </xsl:if>
      </xsl:element>
      <!-- 产品保质期 -->
      <xsl:variable name="CargoAttributes" select="$LinkedInvoiceLinesToEntryLine/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='CAT']"/>
      <xsl:element name="ns0:GoodsAttr">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:value-of select="ScriptNS3:DeDuplicateValuesAndConcatenate($CargoAttributes/s0:SubType/s0:Code, ',', 20, '', boolean(0))"/>
        </xsl:if>
      </xsl:element>
      <!-- 货物属性代码 -->
      <xsl:element name="ns0:Stuff">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:value-of select="ScriptNS3:DeDuplicateValuesAndConcatenate($LinkedInvoiceLinesToEntryLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQIngredient']/s0:Value, ';', 400, '', boolean(0))"/>
        </xsl:if>
      </xsl:element>
      <!-- 成份/原料/组份 -->
      <xsl:variable name="UNDG" select="$FirstLinkedInvoiceLine/s0:HazardousMaterial/s0:UNDGCollection/s0:UNDG[1]"/>
      <xsl:variable name="Uncode" select="$UNDG/s0:UNDGCode/text()" />
      <xsl:variable name="UNDGRequired" select="$NeedToSumbitCIQData and (boolean($CargoAttributes/s0:SubType[s0:Code='31' or s0:Code='32' or s0:Code='33']) or $Uncode!='')" />
      <xsl:if test="$UNDGRequired">
        <xsl:element name="ns0:Uncode">
          <xsl:value-of select="substring($Uncode, 1, 4)"/>
        </xsl:element>
        <!-- UN编码 -->
        <xsl:element name="ns0:DangName">
          <xsl:if test="$Uncode != ''">
            <xsl:value-of select="userCSharp:GetDangName($UNDG/s0:IMOClass/text(), $UNDG/s0:SubLabel1/text(), $UNDG/s0:SubLabel2/text())"/>
          </xsl:if>
        </xsl:element>
        <!-- 危险类别 -->
        <xsl:element name="ns0:DangPackType">
          <xsl:if test="$Uncode != ''">
            <xsl:variable name="undgpg" select="$UNDG/s0:PackingGroup/text()"/>
            <xsl:choose>
              <xsl:when test="$undgpg='I'">
                <xsl:value-of select="'1'"/>
              </xsl:when>
              <xsl:when test="$undgpg='II'">
                <xsl:value-of select="'2'"/>
              </xsl:when>
              <xsl:when test="$undgpg='III'">
                <xsl:value-of select="'3'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'4'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </xsl:element>
        <!-- 包装类别 -->
        <xsl:element name="ns0:DangPackSpec">
          <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='PackageTypeOfUNDG']/s0:Value"/>
        </xsl:element>
        <!-- 包装UN标记 -->
      </xsl:if>
      <xsl:if test="$UNDGRequired">
        <xsl:element name="ns0:NoDangFlag">
          <xsl:choose>
            <xsl:when test="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='NonDangerousChemicalFlag']/s0:Value = 'N'">0</xsl:when>
            <xsl:otherwise>1</xsl:otherwise>
          </xsl:choose>
        </xsl:element>
      </xsl:if>
      <!-- 非危险化学品 -->
      <xsl:element name="ns0:EngManEntCnm">
        <xsl:if test="$NeedToSumbitCIQData and $IsEnteringEntry and not($ManufacturerIsInChina)">
          <xsl:call-template name="GetOrgCnNameFallbackToCompanyName">
            <xsl:with-param name="OrgAddress" select="$Manufacturer"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:element>
      <!-- 境外生产企业名称 -->
      <xsl:element name="ns0:DestCode">
        <xsl:variable name="districtAddInfoKey">
          <xsl:choose>
            <xsl:when test="$IsEnteringEntry">DestinationRegion</xsl:when>
            <xsl:otherwise>OriginRegion</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key=$districtAddInfoKey]/s0:Value"/>
      </xsl:element>
      <!-- 目的地/货源地代码 -->
      <xsl:element name="ns0:GoodsSpec">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:variable name="AddInfoCIQSpec" select="ScriptNS3:DeDuplicateValuesAndConcatenate($LinkedInvoiceLinesToEntryLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQSpec']/s0:Value, ';', 2000, '', boolean(0))"/>
          <xsl:choose>
            <xsl:when test="$AddInfoCIQSpec">
              <xsl:value-of select="$AddInfoCIQSpec"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="ScriptNS3:DeDuplicateValuesAndConcatenate($LinkedInvoiceLinesToEntryLine/s0:LocalDescription, ';', 2000, '', boolean(0))"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:element>
      <!-- 检验检疫货物规格 -->
      <xsl:element name="ns0:GoodsModel">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:variable name="AddInfoCIQModel" select="ScriptNS3:DeDuplicateValuesAndConcatenate($LinkedInvoiceLinesToEntryLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQModel']/s0:Value, ';', 2000, '', boolean(0))"/>
          <xsl:choose>
            <xsl:when test="$AddInfoCIQModel">
              <xsl:value-of select="$AddInfoCIQModel"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="ScriptNS3:DeDuplicateValuesAndConcatenate($LinkedInvoiceLinesToEntryLine/s0:Model, ';', 2000, '', boolean(0))"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:element>
      <!-- 货物型号 -->
      <xsl:element name="ns0:GoodsBrand">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:variable name="AddInfoCIQBrand" select="ScriptNS3:DeDuplicateValuesAndConcatenate($LinkedInvoiceLinesToEntryLine/s0:AddInfoCollection/s0:AddInfo[s0:Key='CIQBrand']/s0:Value, ';', 2000, '', boolean(0))" />
          <xsl:choose>
            <xsl:when test="$AddInfoCIQBrand">
              <xsl:value-of select="$AddInfoCIQBrand"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="ScriptNS3:DeDuplicateValuesAndConcatenate($LinkedInvoiceLinesToEntryLine/s0:BrandName, ';', 2000, '', boolean(0))"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:element>
      <!-- 货物品牌 -->
      <xsl:element name="ns0:ProduceDate">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:variable name="produceDates">
            <xsl:for-each select="$LinkedInvoiceLinesToEntryLine/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='CIQ' and s0:SubType/s0:Code='BN']/s0:DateCollection/s0:Date/s0:Value">
              <xsl:element name="Value">
                <xsl:value-of select="ScriptNS1:ConvertToDateTimeString(.,'','','','yyyyMMdd')" />
              </xsl:element>
            </xsl:for-each>
          </xsl:variable>
          <xsl:value-of select="ScriptNS3:DeDuplicateValuesAndConcatenate(msxsl:node-set($produceDates)/*, ';', 2000, '', boolean(0))"/>
        </xsl:if>
      </xsl:element>
      <!-- 生产日期 -->
      <xsl:element name="ns0:ProdBatchNo">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:value-of select="ScriptNS3:DeDuplicateValuesAndConcatenate($LinkedInvoiceLinesToEntryLine/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='CIQ' and s0:SubType/s0:Code='BN']/s0:Reference, ';', 2000, '', boolean(0))"/>
        </xsl:if>
      </xsl:element>
      <!-- 生产批号 -->
      <xsl:element name="ns0:DistrictCode">
        <xsl:variable name="districtAddInfoKey">
          <xsl:choose>
            <xsl:when test="$IsEnteringEntry">DestinationDistrict</xsl:when>
            <xsl:otherwise>OriginDistrict</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:value-of select="$FirstLinkedInvoiceLine/s0:AddInfoCollection/s0:AddInfo[s0:Key=$districtAddInfoKey]/s0:Value"/>
      </xsl:element>
      <!-- 境内目的地/境内货源地 -->
      <xsl:element name="ns0:CiqName">
        <xsl:if test="$NeedToSumbitCIQData">
        </xsl:if>
      </xsl:element>
      <!-- 检验检疫名称（字段已废弃，不再使用） -->
      <xsl:if test="$NeedToSumbitCIQData">
        <xsl:variable name ="ProductQualifications" select="ScriptNS3:DeDuplicateCustomsSupportingInformation($LinkedInvoiceLinesToEntryLine/s0:CustomsSupportingInformationCollection/s0:CustomsSupportingInformation[s0:Category/s0:Code='PQD'], boolean(0))/s0:CustomsSupportingInformation"/>
        <xsl:if test="$ProductQualifications">
          <xsl:element name="ns0:DecGoodsLimits">
            <xsl:for-each select="$ProductQualifications">
              <xsl:call-template name="GoodsLimit">
                <xsl:with-param name="EntryLineNumber" select="$EntryLineNumber" />
                <xsl:with-param name="LinkedInvoiceLines" select="$LinkedInvoiceLinesToEntryLine" />
              </xsl:call-template>
            </xsl:for-each>
          </xsl:element>
        </xsl:if>
      </xsl:if>
      <!-- 许可证信息表 -->
      <xsl:element name="ns0:MnufctrRegNo">
        <xsl:if test="$NeedToSumbitCIQData">
          <xsl:if test="not($IsEnteringEntry) and $ManufacturerIsInChina">
            <xsl:value-of select="$Manufacturer/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='CIQ' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
          </xsl:if>
        </xsl:if>
      </xsl:element>
      <!-- 生产单位注册号 -->
      <xsl:element name="ns0:MnufctrRegName">
        <xsl:if test="$NeedToSumbitCIQData and not($IsEnteringEntry) and $ManufacturerIsInChina">
          <xsl:call-template name="GetOrgCnNameFallbackToCompanyName">
            <xsl:with-param name="OrgAddress" select="$Manufacturer"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:element>
      <!-- 生产单位名称 -->
      <xsl:element name="ns0:RcepOrigPlaceCode">
        <xsl:variable name="CertificateOfOrigin" select="$LinkedInvoiceLinesToEntryLine/s0:CustomsSupportingInformationCollection/s0:CustomsSupportingInformation[s0:Category/s0:Code='SUP' and s0:Type/s0:Code='1Y'][1]"/>
        <xsl:variable name="CooReferenceNumber" select="$CertificateOfOrigin/s0:ReferenceNumber/text()"/>
        <xsl:variable name="CooCountryCode" select="$CertificateOfOrigin/s0:Country/s0:Code/text()"/>
        <xsl:if test="$CooReferenceNumber != ''">
          <xsl:choose>
            <xsl:when test="$CooCountryCode != ''">
              <xsl:call-template name="GetIsoAlpha3CountryCode">
                <xsl:with-param name="CountryCode" select="$CooCountryCode"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$CountryOfOriginAlp3"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:element>
      <!-- 优惠贸易协定项下原产地 -->
    </xsl:element>
  </xsl:template>

  <!-- 报关单集装箱 DecContainer -->
  <xsl:template name="Container2DecContainer">
    <xsl:variable name="ContainerNum" select="s0:ContainerNumber" />
    <xsl:element name="ns0:Container">
      <xsl:element name="ns0:ContainerId">
        <xsl:value-of select="$ContainerNum"/>
      </xsl:element>
      <!-- 集装箱号 -->
      <xsl:element name="ns0:ContainerMd">
        <xsl:value-of select="s0:CustomsContainerSize/s0:Code"/>
      </xsl:element>
      <!-- 集装箱规格 -->
      <xsl:element name="ns0:GoodsNo">
        <xsl:variable name="linkedEntryNumbers">
          <xsl:for-each select="/s0:UniversalShipment/s0:Shipment/s0:PackingLineCollection/s0:PackingLine[s0:ContainerNumber=$ContainerNum]/s0:PackedItemCollection/s0:PackedItem">
            <xsl:variable name="invoiceLineLink" select="s0:CommercialInvoiceLineLink"/>
            <xsl:element name="Value">
              <xsl:value-of select="$InvoiceLinesLinkedToEntry[s0:Link=$invoiceLineLink]/s0:EntryLineNumber" />
            </xsl:element>
          </xsl:for-each>
        </xsl:variable>
        <xsl:value-of select="ScriptNS3:DeDuplicateValuesAndConcatenate(msxsl:node-set($linkedEntryNumbers)/*, ',', 255, '', boolean(1))"/>
      </xsl:element>
      <!-- 商品项号 -->
      <xsl:element name="ns0:LclFlag">
        <xsl:choose>
          <xsl:when test="s0:FCL_LCL_AIR/s0:Code='LCL'">
            <xsl:value-of select="'1'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'0'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:element>
      <!-- 拼箱标识 -->
      <xsl:variable name="GoodsWeight" select="ScriptNS0:Convert(./s0:GoodsWeight/text(), ./s0:WeightUnit/s0:Code/text(), 'KG')" />
      <xsl:if test="$GoodsWeight">
        <xsl:element name="ns0:GoodsContaWt">
          <xsl:value-of select="$GoodsWeight"/>
        </xsl:element>
      </xsl:if>
      <!-- 箱货重量 -->
      <xsl:element name="ns0:ContainerWt">
        <xsl:value-of select="ScriptNS0:Convert(./s0:TareWeight/text(), ./s0:WeightUnit/s0:Code/text(), 'KG')"/>
      </xsl:element>
      <!-- 自重 -->
    </xsl:element>
  </xsl:template>

  <!-- 随附单证 DecLicenseDocus -->
  <xsl:template name="LicenseDocus">
    <xsl:if test ="$IsCustomsEntry or $MessageSubType != 'BTH'">
      <xsl:for-each select="ScriptNS3:DeDuplicateCustomsSupportingInformation($InvoiceLinesLinkedToEntry/s0:CustomsSupportingInformationCollection/s0:CustomsSupportingInformation[s0:Category/s0:Code='SUP'], boolean(1))/s0:CustomsSupportingInformation">
        <xsl:element name="ns0:LicenseDocu">
          <xsl:variable name="typeCode" select="./s0:Type/s0:Code"/>
          <xsl:element name="ns0:DocuCode">
            <xsl:value-of select ="substring($typeCode, 2, 1)"/>
          </xsl:element>
          <xsl:variable name="subTypeCode" select="./s0:SubType/s0:Code"/>
          <xsl:variable name="docNumber">
            <xsl:choose>
              <xsl:when test="$typeCode='1Y' and $subTypeCode='X'">JE00000</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="./s0:ReferenceNumber/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <!-- 单证代码 -->
          <xsl:element name="ns0:CertCode">
            <xsl:choose>
              <xsl:when test="$typeCode='1Y'">
                <xsl:variable name="tradeAgreementCode">
                  <xsl:choose>
                    <xsl:when test="$invoiceLineWithCO/s0:PrimaryPreference/text()='LDC'">13</xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$invoiceLineWithCO/s0:SecondaryPreference"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:value-of select="concat('&lt;', $tradeAgreementCode, '&gt;', $subTypeCode, $docNumber)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select ="$docNumber"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>
        </xsl:element>
        <!-- 单证编号 -->
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <!-- 申请单证信息表 DecRequestCert -->
  <xsl:template name="RequestCerts">
    <xsl:if test="$NeedToSumbitCIQData">
      <xsl:if test="$ReqDocuments">
        <xsl:element name="ns0:DecRequestCerts">
          <xsl:for-each select="$ReqDocuments">
            <xsl:element name="ns0:DecRequestCert">
              <xsl:element name="ns0:AppCertCode">
                <xsl:value-of select ="./s0:AddInfoCollection/s0:AddInfo[s0:Key='DocumentType']/s0:Value"/>
              </xsl:element>
              <!-- 申请单证代码 -->
              <xsl:element name="ns0:ApplOri">
                <xsl:value-of select ="./s0:AddInfoCollection/s0:AddInfo[s0:Key='NumberOfOriginals']/s0:Value"/>
              </xsl:element>
              <!--申请单证正本数 -->
              <xsl:element name="ns0:ApplCopyQuan">
                <xsl:value-of select ="./s0:AddInfoCollection/s0:AddInfo[s0:Key='NumberOfCopies']/s0:Value"/>
              </xsl:element>
            </xsl:element>
            <!--申请单证副本数 -->
          </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!-- 其他包装信息表 DecOtherPack -->
  <xsl:template name="OtherPacks">
    <xsl:variable name="OtherPackages" select="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='PKG']"/>
    <xsl:if test="$OtherPackages">
      <xsl:element name ="ns0:DecOtherPacks">
        <xsl:for-each select="$OtherPackages">
          <xsl:element name="ns0:DecOtherPack">
            <xsl:element name="ns0:PackQty" />
            <!--包装件数 -->
            <xsl:element name="ns0:PackType">
              <xsl:value-of select ="./s0:SubType/s0:Code"/>
            </xsl:element>
            <!--包装材料种类 -->
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <!-- 企业资质信息表 DecCopLimit -->
  <xsl:template name="CopLimits">
    <xsl:if test="$NeedToSumbitCIQData">
      <xsl:variable name="Qualifications" select="$LinkedEntryInstruction/s0:CustomsReferenceCollection/s0:CustomsReference[s0:Type/s0:Code='EPQ']"/>
      <xsl:if test="$Qualifications">
        <xsl:element name ="ns0:DecCopLimits">
          <xsl:for-each select="$Qualifications">
            <xsl:element name="ns0:DecCopLimit">
              <xsl:element name="ns0:EntQualifNo">
                <xsl:value-of select ="./s0:Reference"/>
              </xsl:element>
              <!--企业资质编号 -->
              <xsl:element name="ns0:EntQualifTypeCode">
                <xsl:value-of select ="./s0:SubType/s0:Code"/>
              </xsl:element>
              <!--企业资质类别代码 -->
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!-- 使用人信息表 DecUser -->
  <xsl:template name="Users">
    <xsl:if test="$NeedToSumbitCIQData and $IsEnteringEntry">
      <xsl:variable name="BuyerContact" select="$OwnerOrg/s0:Contact/text()" />
      <xsl:variable name="Contact">
        <xsl:choose>
          <xsl:when test="$BuyerContact">
            <xsl:value-of select="$BuyerContact"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$TradeOrg/s0:Contact/text()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="Phone">
        <xsl:choose>
          <xsl:when test="$BuyerContact">
            <xsl:value-of select="$OwnerOrg/s0:Phone/text()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$TradeOrg/s0:Phone/text()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:if test="$Contact">
        <xsl:element name="ns0:DecUsers">
          <xsl:element name="ns0:DecUser">
            <xsl:element name="ns0:UseOrgPersonCode">
              <xsl:value-of select="$Contact"/>
            </xsl:element>
            <xsl:element name="ns0:UseOrgPersonTel">
              <xsl:value-of select="$Phone"/>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!-- 报关单自由文本信息 DecFreeTxt -->
  <xsl:template name="FreeText">
    <xsl:element name="ns0:RelId">
      <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='RelatedMRN']/s0:Value"/>
    </xsl:element>
    <!-- 关联报关单号 -->
    <xsl:element name="ns0:RelManNo">
      <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='RelatedManualNo']/s0:Value"/>
    </xsl:element>
    <!-- 关联备案号 -->
    <xsl:element name="ns0:BonNo">
      <xsl:variable name="WarehouseOrg" select="key('ShipmentOrgAddress', 'CustomsWarehouseAddress')"/>
      <xsl:value-of select="$WarehouseOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='CPW' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
    </xsl:element>
    <!-- 监管仓号 -->
    <xsl:element name="ns0:VoyNo">
      <xsl:choose>
        <xsl:when test="$EntryHeader/s0:AddInfoCollection/s0:AddInfo[s0:Key='Voyage']">
          <xsl:value-of select="$EntryHeader/s0:AddInfoCollection/s0:AddInfo[s0:Key='Voyage']/s0:Value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test="$IsCrossBorder">
              <xsl:choose>
                <xsl:when test="$TransportMode='SEA'">
                  <xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:VoyageFlightNo"/>
                </xsl:when>
                <xsl:when test="$TransportMode='MAI' or $TransportMode='RAI'">
                  <xsl:value-of select="$IEDate"/>
                </xsl:when>
                <xsl:when test="$TransportMode='ROA'">
                  <xsl:value-of select="/s0:UniversalShipment/s0:Shipment/s0:AdditionalBillCollection/s0:AdditionalBill[s0:BillType/s0:Code='MWB']/s0:BillNumber"/>
                </xsl:when>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
    <!-- 航次号 -->
    <xsl:element name="ns0:DecBpNo"></xsl:element>
    <!-- TODO: 报关员联系方式 -->
    <xsl:element name="ns0:CusFie">
      <xsl:variable name="DepotOrg" select="key('ShipmentOrgAddress', 'CustomsDepotAddress')"/>
      <xsl:value-of select="$DepotOrg/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/s0:Code='CPD' and s0:CountryOfIssue/s0:Code='CN']/s0:Value"/>
    </xsl:element>
    <!-- 货场代码 -->
    <xsl:element name="ns0:DecNo">
      <xsl:value-of select="key('ShipmentAddInfo', 'BrokerNumber')/s0:Value"/>
    </xsl:element>
    <!-- 申报人员证号 -->
  </xsl:template>

  <!-- 报关单签名 DecSign -->
  <xsl:template name="Sign">
    <xsl:element name="ns0:OperType">G</xsl:element>
    <!-- 操作类型 -->
    <xsl:element name="ns0:ICCode"/>
    <!-- TODO: 操作员IC卡号 -->
    <xsl:element name="ns0:CopCode"/>
    <!-- TODO: 操作企业组织机构代码 -->
    <xsl:element name="ns0:OperName"/>
    <!-- TODO: 操作员姓名 -->
    <xsl:element name="ns0:ClientSeqNo">
      <xsl:value-of select="$EntryHeader/s0:Reference" />
    </xsl:element>
    <!-- 客户端报关单编号 -->
    <xsl:element name="ns0:Sign"/>
    <!-- TODO: 数字签名信息 -->
    <xsl:element name="ns0:SignDate"/>
    <!-- TODO: 签名日期 -->
    <xsl:element name="ns0:Certificate"/>
    <!-- TODO: 操作员卡的证书号 -->
    <xsl:element name="ns0:HostId"/>
    <!-- TODO: 客户端邮箱的HostId -->
    <xsl:element name="ns0:BillSeqNo"/>
    <!-- TODO: 对应清单统一编号 -->
    <xsl:element name="ns0:DomainId"/>
    <!-- TODO: 签名人分类 -->
    <xsl:element name="ns0:Note"/>
    <!-- TODO: 备注 -->
  </xsl:template>

  <xsl:template name="CopPromises">
    <xsl:if test="$NeedToSumbitCIQData and $LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='EnterprisePromised' and s0:Value='Y']">
      <xsl:element name="ns0:DecCopPromises">
        <xsl:element name="ns0:DecCopPromise">
          <xsl:element name="ns0:DeclaratioMaterialCode">
            <xsl:choose>
              <xsl:when test="$IsEnteringEntry">101040</xsl:when>
              <xsl:otherwise>102053</xsl:otherwise>
            </xsl:choose>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <!-- 两段准入申请 -->
  <xsl:template name="TpAccess">
    <xsl:if test="$NeedToSumbitCIQData and $IsEnteringEntry">
      <xsl:element name="ns0:DecTpAccess">
        <xsl:element name="ns0:TransitionApply">
          <xsl:choose>
            <xsl:when test="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='ApplyForTransition']/s0:Value = 'Y'">1</xsl:when>
            <xsl:otherwise>0</xsl:otherwise>
          </xsl:choose>
        </xsl:element>
        <xsl:element name="ns0:TransitionSite">
          <xsl:value-of select="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='TransitionSite']/s0:Value" />
        </xsl:element>
        <xsl:element name="ns0:ConditionalLiftoffApply">
          <xsl:choose>
            <xsl:when test="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='ApplyForConditionalPickup']/s0:Value = 'Y'">1</xsl:when>
            <xsl:otherwise>0</xsl:otherwise>
          </xsl:choose>
        </xsl:element>
        <xsl:element name="ns0:PortDestMergeCheckApply">
          <xsl:choose>
            <xsl:when test="$LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='ApplyForCombinedInspections']/s0:Value = 'Y'">1</xsl:when>
            <xsl:otherwise>0</xsl:otherwise>
          </xsl:choose>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <!-- 随附单据 EdocRealation -->
  <xsl:template name="TEdocRealation">
    <xsl:for-each select="$LinkedEntryInstruction/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code='EIA']">
      <xsl:element name="ns0:EdocRealation">
        <xsl:variable name="fileName" select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='FileName']/s0:Value"/>
        <xsl:variable name="attachmentType" select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='AttachmentType']/s0:Value"/>
        <xsl:variable name="attachmentNumber" select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='AttachmentNumber']/s0:Value"/>
        <xsl:variable name="entryLineLinks" select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='EntryLineLinks']/s0:Value"/>
        <xsl:element name="ns0:EdocID">
          <xsl:choose>
            <xsl:when test="$attachmentNumber != '' ">
              <xsl:value-of select="$attachmentNumber" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$attachmentType"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:element>
        <xsl:element name="ns0:EdocCode">
          <xsl:value-of select="$attachmentType" />
        </xsl:element>
        <xsl:element name="ns0:EdocFomatType">US</xsl:element>
        <xsl:element name="ns0:OpNote"/>
        <xsl:element name="ns0:EdocCopId">
          <xsl:value-of select="$fileName" />
        </xsl:element>
        <xsl:element name="ns0:EdocOwnerCode">
          <xsl:value-of select="$OwnerCode"/>
        </xsl:element>
        <xsl:element name="ns0:SignUnit">
          <xsl:value-of select="$AgentCode"/>
        </xsl:element>
        <xsl:element name="ns0:SignTime"/>
        <xsl:element name="ns0:EdocOwnerName">
          <xsl:value-of select="$OwnerName"/>
        </xsl:element>
        <xsl:element name="ns0:EdocSize"/>
        <xsl:element name="ns0:GNoStr">
          <xsl:value-of select="$entryLineLinks"/>
        </xsl:element>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <!-- 随附单证对应关系 EcoRelation -->
  <xsl:template name="EcoRelation">
    <xsl:variable name="EcoDocuments">
      <xsl:for-each select="$InvoiceLinesLinkedToEntry/s0:CustomsSupportingInformationCollection/s0:CustomsSupportingInformation[s0:Category/s0:Code='SUP' and (s0:Type/s0:Code='1Y' or s0:Type/s0:Code='0t' or s0:Type/s0:Code='0e' or s0:Type/s0:Code='0q')]">
        <xsl:element name="CustomsSupportingInformation">
          <xsl:element name="EntryLineNumber">
            <xsl:value-of select="../../s0:EntryLineNumber" />
          </xsl:element>
          <xsl:element name="Type">
            <xsl:element name="Code">
              <xsl:value-of select="./s0:Type/s0:Code" />
            </xsl:element>
          </xsl:element>
          <xsl:element name="SubType">
            <xsl:element name="Code">
              <xsl:value-of select="./s0:SubType/s0:Code" />
            </xsl:element>
          </xsl:element>
          <xsl:element name="ReferenceNumber">
            <xsl:value-of select="./s0:ReferenceNumber" />
          </xsl:element>
          <xsl:element name="LineNo">
            <xsl:value-of select="./s0:LineNo" />
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="EcoRelationNodes">
      <xsl:for-each select="ScriptNS3:DeDuplicateCustomsSupportingInformation(msxsl:node-set($EcoDocuments)/*, boolean(1))/s0:CustomsSupportingInformation">
        <xsl:element name="EcoRelationNode">
          <xsl:variable name="typeCode" select="./s0:Type/s0:Code/text()"/>
          <xsl:variable name="subTypeCode" select="./s0:SubType/s0:Code/text()" />
          <xsl:variable name="docNumber">
            <xsl:choose>
              <xsl:when test="$typeCode='1Y' and $subTypeCode='X'">JE00000</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="./s0:ReferenceNumber/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:element name="CertType">
            <xsl:value-of select="substring($typeCode, 2, 1)"/>
          </xsl:element>
          <xsl:element name="EcoCertNo">
            <xsl:choose>
              <xsl:when test="$typeCode='1Y'">
                <xsl:variable name="tradeAgreementCode">
                  <xsl:choose>
                    <xsl:when test="$invoiceLineWithCO/s0:PrimaryPreference/text()='LDC'">13</xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$invoiceLineWithCO/s0:SecondaryPreference"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:value-of select="concat('&lt;', $tradeAgreementCode, '&gt;', $subTypeCode, $docNumber)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select ="$docNumber"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>
          <xsl:element name="DecGNo">
            <xsl:value-of select="./s0:EntryLineNumber" />
          </xsl:element>
          <xsl:element name="EcoGNo">
            <xsl:choose>
              <xsl:when test="$typeCode='1Y' and $subTypeCode='X'">
                <xsl:value-of select="./s0:EntryLineNumber" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select ="./s0:LineNo"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:for-each select="ScriptNS3:ReorderEcoRelationNodes(msxsl:node-set($EcoRelationNodes)/*)/s0:EcoRelationNode">
      <xsl:element name="ns0:EcoRelation">
        <xsl:element name="ns0:CertType">
          <xsl:value-of select ="./s0:CertType"/>
        </xsl:element>
        <!--单证代码 Y14?-->
        <xsl:element name="ns0:EcoCertNo">
          <xsl:value-of select ="./s0:EcoCertNo"/>
        </xsl:element>
        <!--单据证书号-->
        <xsl:element name="ns0:DecGNo">
          <xsl:value-of select="./s0:DecGNo" />
        </xsl:element>
        <!--报关单商品项号-->
        <xsl:element name="ns0:EcoGNo">
          <xsl:value-of select ="./s0:EcoGNo"/>
        </xsl:element>
        <!--原产地证书单证项号-->
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <!-- 许可证信息表  DecGoodsLimit -->
  <xsl:template name="GoodsLimit">
    <xsl:param name="EntryLineNumber" />
    <xsl:param name="LinkedInvoiceLines" />
    <xsl:variable name ="docCode" select ="./s0:Type/s0:Code"/>
    <xsl:variable name ="docNum" select ="./s0:ReferenceNumber"/>
    <xsl:variable name="firstInvoiceUQ" select="$LinkedInvoiceLines[1]/s0:InvoiceQuantityUnit/s0:Code/text()" />
    <xsl:variable name="quantitiesOnInvoiceLines">
      <xsl:for-each select="$LinkedInvoiceLines">
        <xsl:variable name="invoiceUQ" select="./s0:InvoiceQuantityUnit/s0:Code/text()" />
        <xsl:variable name="invoiceQty" select="./s0:InvoiceQuantity/text()" />
        <xsl:if test="$invoiceQty &gt; 0 and $invoiceUQ != '' and $firstInvoiceUQ = $invoiceUQ">
          <xsl:element name="InvoiceQuantity">
            <xsl:value-of select="./s0:InvoiceQuantity/text()"/>
          </xsl:element>
        </xsl:if>
        <xsl:element name="TradeQuantity">
          <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key/text()='TradeQuantity']/s0:Value"/>
        </xsl:element>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="useInvoiceQty" select="count(msxsl:node-set($quantitiesOnInvoiceLines)/InvoiceQuantity/text()) = count($LinkedInvoiceLines)" />
    <xsl:element name="ns0:DecGoodsLimit">
      <xsl:element name="ns0:GoodsNo">
        <xsl:value-of select="$EntryLineNumber"/>
      </xsl:element>
      <!-- 商品序号 -->
      <xsl:element name="ns0:LicTypeCode">
        <xsl:value-of select="$docCode"/>
      </xsl:element>
      <!-- 许可证类别代码 -->
      <xsl:element name="ns0:LicenceNo">
        <xsl:value-of select="$docNum"/>
      </xsl:element>
      <!-- 许可证编号 -->
      <xsl:element name="ns0:LicWrtofDetailNo">
        <xsl:value-of select="./s0:LineNo"/>
      </xsl:element>
      <!-- 许可证核销明细序号 -->
      <xsl:element name="ns0:LicWrtofQty">
        <xsl:value-of select="./s0:Quantity"/>
      </xsl:element>
      <!-- 许可证核销数量 -->
      <xsl:if test="$docCode = '408' or $docCode = '409'">
        <xsl:variable name="BLDate" select="ScriptNS1:ConvertToDateTimeString($LinkedEntryInstruction/s0:AddInfoCollection/s0:AddInfo[s0:Key='BillOfLadingDate']/s0:Value,'','','','yyyyMMdd')"/>
        <xsl:for-each select="$LinkedInvoiceLines/s0:AddInfoGroupCollection/s0:AddInfoGroup[s0:Type/s0:Code='VID']">
          <xsl:element name="ns0:DecGoodsLimitVin">
            <xsl:element name="ns0:LicenceNo">
              <xsl:value-of select="$docNum"/>
            </xsl:element>
            <!-- 许可证编号 -->
            <xsl:element name="ns0:LicTypeCode">
              <xsl:value-of select="$docCode"/>
            </xsl:element>
            <!-- 许可证类别代码 -->
            <xsl:element name="ns0:VinNo">
              <xsl:value-of select="position()"/>
            </xsl:element>
            <!-- VIN序号 -->
            <xsl:element name="ns0:BillLadDate">
              <xsl:value-of select="$BLDate"/>
            </xsl:element>
            <!-- 提/运单日期 -->
            <xsl:element name="ns0:QualityQgp">
              <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='QGP']/s0:Value"/>
            </xsl:element>
            <!-- 质量保质期 -->
            <xsl:element name="ns0:MotorNo">
              <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='EngineNo']/s0:Value"/>
            </xsl:element>
            <!-- 发动机号或电机号 -->
            <xsl:element name="ns0:VinCode">
              <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='VIN']/s0:Value"/>
            </xsl:element>
            <!-- 车辆识别代码（VIN） -->
            <xsl:element name="ns0:ChassisNo">
              <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='ChassisNo']/s0:Value"/>
            </xsl:element>
            <!-- 底盘(车架)号 -->
            <xsl:element name="ns0:InvoiceNum">
              <xsl:choose>
                <xsl:when test="$useInvoiceQty">
                  <xsl:value-of select="floor(sum(msxsl:node-set($quantitiesOnInvoiceLines)/InvoiceQuantity/text()) * 100000) div 100000"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="floor(sum(msxsl:node-set($quantitiesOnInvoiceLines)/TradeQuantity/text()) * 100000) div 100000"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:element>
            <!-- 发票所列数量 -->
            <xsl:element name="ns0:ProdCnnm">
              <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='ProductNameCN']/s0:Value"/>
            </xsl:element>
            <!-- 品名（中文名称） -->
            <xsl:element name="ns0:ProdEnnm">
              <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='ProductNameEN']/s0:Value"/>
            </xsl:element>
            <!-- 品名（英文名称） -->
            <xsl:element name="ns0:ModelEn">
              <xsl:value-of select="./s0:AddInfoCollection/s0:AddInfo[s0:Key='ModelEN']/s0:Value"/>
            </xsl:element>
            <!-- 型号（英文） -->
            <xsl:element name="ns0:PricePerUnit">
              <xsl:variable name="tradeQty" select="../../s0:AddInfoCollection/s0:AddInfo[s0:Key='TradeQuantity']/s0:Value"/>
              <xsl:variable name="linePrice" select="../../s0:LinePrice"></xsl:variable>
              <xsl:choose>
                <xsl:when test="$useInvoiceQty">
                  <xsl:value-of select="../../s0:UnitPrice"/>
                </xsl:when>
                <xsl:when test ="$tradeQty != 0">
                  <xsl:value-of select="round($linePrice div $tradeQty * 10000) div 10000"/>
                </xsl:when>
                <xsl:otherwise>0</xsl:otherwise>
              </xsl:choose>
            </xsl:element>
            <!-- 单价 -->
            <xsl:element name="ns0:InvoiceNo">
              <xsl:value-of select="../../../../s0:InvoiceNumber"/>
            </xsl:element>
            <!-- 发票号 -->
          </xsl:element>
        </xsl:for-each>
      </xsl:if>
      <!-- 许可证Vin -->
      <xsl:element name="ns0:LicWrtofQtyUnit">
        <xsl:value-of select="./s0:UnitOfQuantity/s0:Code"/>
      </xsl:element>
      <!-- 许可证核销数量单位 -->
    </xsl:element>
  </xsl:template>

  <xsl:template name="GetOrgCnName">
    <xsl:param name="OrgAddress" />
    <xsl:value-of select="$OrgAddress/s0:LocalAddressCollection/s0:LocalAddress[translate(s0:Language/s0:Code,$UpperCases,$LowerCases)='zh-cn']/s0:CompanyName/text()"/>
  </xsl:template>

  <xsl:template name="GetOrgCnNameFallbackToCompanyName">
    <xsl:param name="OrgAddress" />
    <xsl:variable name="CnLocalAddressName">
      <xsl:call-template name="GetOrgCnName">
        <xsl:with-param name="OrgAddress" select="$OrgAddress" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$CnLocalAddressName != ''">
        <xsl:value-of select="$CnLocalAddressName"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$OrgAddress/s0:CompanyName"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetOrgEnName">
    <xsl:param name="OrgAddress" />
    <xsl:value-of select="$OrgAddress/s0:LocalAddressCollection/s0:LocalAddress[substring(translate(s0:Language/s0:Code,$UpperCases,$LowerCases),1,2)='en']/s0:CompanyName/text()"/>
  </xsl:template>

  <xsl:template name="GetOrgEnNameFallbackToCompanyName">
    <xsl:param name="OrgAddress" />
    <xsl:variable name="EnLocalAddressName">
      <xsl:call-template name="GetOrgEnName">
        <xsl:with-param name="OrgAddress" select="$OrgAddress" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$EnLocalAddressName != ''">
        <xsl:value-of select="$EnLocalAddressName"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$OrgAddress/s0:CompanyName"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetIsoAlpha3CountryCode">
    <xsl:param name="CountryCode" />
    <xsl:variable name ="IsoAlpha3Code">
      <xsl:choose>
        <xsl:when test="$CountryCode=''">ZZZ</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="ScriptNS2:CallActionProcedureHelper('GetColumnValue', '@result', '@tableName', 'RefCountry', '@inputColumn', 'RN_Code', '@outputColumn', 'RN_IsoAlpha3Code', '@inputValue', $CountryCode)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$IsoAlpha3Code=''">ZZZ</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$IsoAlpha3Code"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <msxsl:script implements-prefix="userCSharp" language="C#">
    <![CDATA[

    public bool IsEnteringEntry(string shipmentType, string decType, string messageType)
    {
      if (decType == "BTH")
      {
        return messageType == "CUS" && shipmentType == "IMP" || messageType == "REC" && shipmentType == "EXP";
      }
      else
      {
        return shipmentType == "IMP";
      }
    }

    public bool IsCrossBorder(string portOfLoading, string portOfArrival)
    {
      return !portOfLoading.StartsWith("CN") || !portOfArrival.StartsWith("CN");
    }

    public string ThrowPartyReceiverIDNotFound(string destinationPartyCSW)
    {
      throw new ArgumentException(string.Format(@"Could not found matching DestinationParty.ID:{0}", destinationPartyCSW));
    }

    public string GetCurrentDateTime(string format)
    {
      return System.DateTime.UtcNow.AddHours(8).ToString(format);
    }

    public string GetDangName(string imoClass, string subLable1, string subLabel2)
    {
      string imoClassCleared = GeDangNamePart(imoClass);
      string subLable1Cleared = GeDangNamePart(subLable1);
      string subLabel2Cleared = GeDangNamePart(subLabel2);
      return imoClassCleared
        + (string.IsNullOrWhiteSpace(subLable1Cleared) ? "" : "+" + subLable1Cleared)
        + (string.IsNullOrWhiteSpace(subLabel2Cleared) ? "" : "+" + subLabel2Cleared);
    }

    string GeDangNamePart(string input)
    {
      return System.Text.RegularExpressions.Regex.Replace(input, @"[^0-9\.]", "");
    }

    public string GetPromiseItmes(string specialRelationshipConfirm, string priceAffectConfirm, string paymentOfRoyaltyConfirm, string valuationCode)
    {
      string c1 = string.IsNullOrWhiteSpace(specialRelationshipConfirm) ? "9" : specialRelationshipConfirm;
      string c2 = string.IsNullOrWhiteSpace(priceAffectConfirm) ? "9" : priceAffectConfirm;
      string c3 = string.IsNullOrWhiteSpace(paymentOfRoyaltyConfirm) ? "9" : paymentOfRoyaltyConfirm;
      string c45 = string.IsNullOrWhiteSpace(valuationCode) ? "99" : valuationCode.Length == 1 ? valuationCode + "9" : valuationCode.Replace(" ", "9");
      return c1 + c2 + c3 + c45;
    }

  ]]>
  </msxsl:script>

</xsl:stylesheet>
