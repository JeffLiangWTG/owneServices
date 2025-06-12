<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ns0 CodeMapper ContextAccessor DataModelAccessor OCMHelper userCSharp" version="1.0"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:OCMHelper="http://schemas.microsoft.com/BizTalk/2003/OCMHelper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:variable name="shipment" select="/ns0:UniversalShipment/ns0:Shipment"/>
  <xsl:variable name="consolID" select="$shipment/ns0:DataContext/ns0:DataSourceCollection/ns0:DataSource[1]/ns0:Key/text()"/>
  <xsl:variable name="containerNumber" select="$shipment/ns0:ContainerCollection/ns0:Container[1]/ns0:ContainerNumber/text()"/>
  <xsl:variable name="forwardingType" select="$shipment/ns0:DataContext/ns0:DataSourceCollection/ns0:DataSource[1]/ns0:Type/text()"/>
  <xsl:variable name="shipmentType" select="$shipment/ns0:ShipmentType/ns0:Code/text()"/>

  <xsl:variable name="isCoLoad" select="OCMHelper:IsCoLoad($shipmentType) = 'TRUE'" />

  <xsl:variable name="addressType">
    <xsl:choose>
      <xsl:when test="$isCoLoad">CoLoadWith</xsl:when>
      <xsl:otherwise>ShippingLineAddress</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="eHubClientLookupCode">
    <xsl:call-template name="GetOrgAddressRegNumber">
      <xsl:with-param name="addressType" select="$addressType" />
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="eHubCargowiseClientID" select="DataModelAccessor:GeteHubIDByQualifier($eHubClientLookupCode, 'CARGOWISE')"/>
  <xsl:variable name="eHubPartyTypeFlag">
    <xsl:choose>
      <xsl:when test="$eHubCargowiseClientID != ''">
        <xsl:value-of select="DataModelAccessor:GetClientRegistrationFlag1AsString($eHubCargowiseClientID, $eHubClientLookupCode, 'CARGOWISE')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="userCSharp:ThrowPartyReceiverIDNotFound($eHubClientLookupCode)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="eHubPartyType">
    <xsl:choose>
      <xsl:when test="$eHubPartyTypeFlag='1'">NVOCC</xsl:when>
      <xsl:otherwise>ShippingLine</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="SetRecipientID" select="ContextAccessor:SetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties', $eHubCargowiseClientID)"/>

  <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name','CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE','@maxlength','14')" />
  <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
  <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $InboxPK, $InterchangeNum)" />

  <xsl:variable name="SubscriberVGMReference">
    <xsl:variable name="previousVGMReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $eHubCargowiseClientID, '@recipientId', $SenderID , '@ST_ID', 'CW1MSG', '@value', concat($consolID, '_', $containerNumber), '@referenceType', 'JobNumber')" />
    <xsl:choose>
      <xsl:when test="$previousVGMReference!=''">
        <xsl:value-of select="$previousVGMReference"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="newVGMReference" select="concat('VGM', format-number($InterchangeNum, '0000000000'))"/>
        <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $newVGMReference, concat($consolID, '_', $containerNumber), 'JobNumber')" />
        <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, concat($consolID, '_', $containerNumber), $newVGMReference, 'JobNumber')" />
        <xsl:value-of select="$newVGMReference" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="SubscribeInterchangeNum" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $InterchangeNum, $SubscriberVGMReference, 'InterchangeNumber')" />
  <xsl:variable name="purposeCode" select="$shipment/ns0:DataContext/ns0:DocumentaryOverride/ns0:Purpose/ns0:Code/text()"/>
  <xsl:variable name="SubscribePurpose" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberVGMReference, $purposeCode, 'ActionPurpose')" />
  <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberVGMReference, $shipmentType, 'ShipmentType')" />
  <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberVGMReference, 'Verified Gross Container Weight', 'DocumentName')" />
  <xsl:variable name="SubscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberVGMReference, $forwardingType, 'ForwardingType')" />

  <xsl:variable name="previousPartyType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $eHubCargowiseClientID, '@recipientId', $SenderID , '@ST_ID', 'CW1MSG', '@value', $SubscriberVGMReference, '@referenceType', 'PartyType')" />
  <xsl:variable name="SubscribeNewPartyType">
    <xsl:choose>
      <xsl:when test="$previousPartyType=''">
        <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberVGMReference, $eHubPartyType, 'PartyType')" />
      </xsl:when>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="formVersion" select="$shipment/ns0:AddInfoCollection/ns0:AddInfo[ns0:Key/text()='FormVersion']/ns0:Value/text()" />
  <xsl:variable name="SubscribeFormVersion">
    <xsl:if test="$formVersion !=''">
      <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue('CW1MSG', $eHubCargowiseClientID, $SenderID, $SubscriberVGMReference, $formVersion, 'FormVersion')"/>
    </xsl:if>
  </xsl:variable>

  <xsl:template match="/ns0:UniversalShipment">
    <ns0:UniversalInterchange>
      <ns0:Header>
        <ns0:SenderID>
          <xsl:value-of select="$eHubCargowiseClientID"/>
        </ns0:SenderID>
        <ns0:RecipientID>
          <xsl:value-of select="'SHIPPING_INSTRUCTION'"/>
        </ns0:RecipientID>
        <ns0:Acknowledgement>
          <ns0:Required>OnAll</ns0:Required>
          <ns0:Channel>eHub</ns0:Channel>
          <ns0:RecipientID>CARGOWISE_AC</ns0:RecipientID>
          <ns0:ContextCollection>
            <ns0:Context>
              <ns0:Type>eHub Interchange Reference</ns0:Type>
              <ns0:Value>
                <xsl:value-of select="$SubscriberVGMReference"/>
              </ns0:Value>
            </ns0:Context>
            <ns0:Context>
              <ns0:Type>DocumentName</ns0:Type>
              <ns0:Value>Verified Gross Container Weight</ns0:Value>
            </ns0:Context>
          </ns0:ContextCollection>
        </ns0:Acknowledgement>
      </ns0:Header>
      <ns0:Body>
        <ns0:UniversalShipment>
          <xsl:apply-templates/>
        </ns0:UniversalShipment>
      </ns0:Body>
    </ns0:UniversalInterchange>
  </xsl:template>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="ns0:DataTargetCollection|ns0:UniversalShipment/ns0:Shipment/ns0:OrganizationAddressCollection|ns0:DataSourceCollection|ns0:DocumentaryOverride|ns0:BookingConfirmationReference|ns0:WayBillNumber|ns0:CoLoadBookingConfirmationReference|ns0:CoLoadMasterBillNumber|ns0:AgentsReference" />

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:ShipmentType">

    <xsl:if test="$eHubPartyType='ShippingLine'">
      <ns0:BookingConfirmationReference>
        <xsl:call-template name="GetBookingConfirmationRef" />
      </ns0:BookingConfirmationReference>
      <ns0:WayBillNumber>
        <xsl:call-template name="GetWaybillNumber" />
      </ns0:WayBillNumber>
    </xsl:if>

    <xsl:if test="$eHubPartyType='NVOCC'">
      <ns0:CoLoadBookingConfirmationReference>
        <xsl:call-template name="GetBookingConfirmationRef" />
      </ns0:CoLoadBookingConfirmationReference>
      <ns0:CoLoadMasterBillNumber>
        <xsl:call-template name="GetWaybillNumber" />
      </ns0:CoLoadMasterBillNumber>
    </xsl:if>

  </xsl:template>

  <xsl:template name="GetBookingConfirmationRef">
    <xsl:variable name="bookingConfirmationRef">
      <xsl:choose>
        <xsl:when test="$isCoLoad">
          <xsl:value-of select="$shipment/ns0:CoLoadBookingConfirmationReference/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$shipment/ns0:BookingConfirmationReference/text()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:value-of select="$bookingConfirmationRef"/>
  </xsl:template>

  <xsl:template name="GetWaybillNumber">
    <xsl:variable name="waybillNumber">
      <xsl:choose>
        <xsl:when test="$isCoLoad">
          <xsl:value-of select="$shipment/ns0:CoLoadMasterBillNumber/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$shipment/ns0:WayBillNumber/text()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:value-of select="$waybillNumber"/>
  </xsl:template>

  <xsl:template match="ns0:RecipientRoleCollection">
    <ns0:RecipientRoleCollection>
      <ns0:RecipientRole>
        <ns0:Code>
          <xsl:choose>
            <xsl:when test="$eHubPartyType='NVOCC'">NVO</xsl:when>
            <xsl:otherwise>CAR</xsl:otherwise>
          </xsl:choose>
        </ns0:Code>
        <ns0:Description>
          <xsl:value-of select="$eHubPartyType"/>
        </ns0:Description>
        <ns0:DataVersion>
          <xsl:value-of select="$shipment/ns0:DataContext/ns0:DocumentaryOverride/ns0:DataVersion/text()"/>
        </ns0:DataVersion>
        <ns0:ServiceCode>VGM</ns0:ServiceCode>
        <ns0:ServiceDescription>Verified Gross Container Weight</ns0:ServiceDescription>
        <ns0:Purpose>
          <xsl:value-of select="$purposeCode"/>
        </ns0:Purpose>
        <ns0:SubmissionVersion>
          <xsl:value-of select="$shipment/ns0:DataContext/ns0:DocumentaryOverride/ns0:SubmissionVersion/text()"/>
        </ns0:SubmissionVersion>
      </ns0:RecipientRole>
    </ns0:RecipientRoleCollection>
  </xsl:template>

  <xsl:template match="ns0:ContainerCollection">
    <ns0:ContainerCollection Content="Partial">
      <xsl:for-each select="ns0:Container">
        <ns0:Container>
          <xsl:copy-of select ="ns0:AirVentFlow"/>
          <xsl:copy-of select ="ns0:AirVentFlowRateUnit"/>
          <xsl:copy-of select ="ns0:ContainerNumber"/>
          <xsl:copy-of select ="ns0:ContainerType"/>
          <xsl:copy-of select ="ns0:DeliveryMode"/>
          <xsl:copy-of select ="ns0:DepartureEstimatedPickup"/>
          <xsl:copy-of select ="ns0:FCL_LCL_AIR"/>
          <xsl:copy-of select ="ns0:GrossWeight"/>
          <xsl:copy-of select ="ns0:GrossWeightVerificationDateTime"/>
          <xsl:copy-of select ="ns0:GrossWeightVerificationType"/>
          <xsl:copy-of select ="ns0:HumidityPercent"/>
          <xsl:copy-of select ="ns0:IsControlledAtmosphere"/>
          <xsl:copy-of select ="ns0:IsEmptyContainer"/>
          <xsl:copy-of select ="ns0:IsShipperOwned"/>
          <xsl:copy-of select ="ns0:LengthUnit"/>
          <xsl:copy-of select ="ns0:Link"/>

          <xsl:for-each select="ns0:OrganizationAddressCollection/ns0:OrganizationAddress">
            <xsl:variable name="generateRegistrationNumber">
              <xsl:choose>
                <xsl:when test="ns0:AddressType/text()='GrossWeightVerifiedBy'">N</xsl:when>
                <xsl:otherwise>Y</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <ns0:OrganizationAddressCollection>
              <xsl:call-template name="PopulateOrganisationAddress">
                <xsl:with-param name="org" select="."/>
                <xsl:with-param name="generateRegistrationNumber" select="$generateRegistrationNumber" />
              </xsl:call-template>
            </ns0:OrganizationAddressCollection>
          </xsl:for-each>

          <xsl:copy-of select ="ns0:OverhangBack"/>
          <xsl:copy-of select ="ns0:OverhangFront"/>
          <xsl:copy-of select ="ns0:OverhangHeight"/>
          <xsl:copy-of select ="ns0:OverhangLeft"/>
          <xsl:copy-of select ="ns0:OverhangRight"/>
          <xsl:copy-of select ="ns0:Seal"/>
          <xsl:copy-of select ="ns0:SealPartyType"/>
          <xsl:copy-of select ="ns0:SecondSeal"/>
          <xsl:copy-of select ="ns0:SecondSealPartyType"/>
          <xsl:copy-of select ="ns0:SetPointTemp"/>
          <xsl:copy-of select ="ns0:SetPointTempUnit"/>
          <xsl:copy-of select ="ns0:TareWeight"/>
          <xsl:copy-of select ="ns0:ThirdSeal"/>
          <xsl:copy-of select ="ns0:ThirdSealPartyType"/>
          <xsl:copy-of select ="ns0:WeightUnit"/>

          <ns0:AdditionalReferenceCollection>
            <xsl:for-each select="ns0:AdditionalReference[ns0:Type/ns0:Code/text()!='HIR']">
              <xsl:copy-of select ="."/>
            </xsl:for-each>

            <ns0:AdditionalReference>
              <ns0:Type>
                <ns0:Code>HIR</ns0:Code>
                <ns0:Description>eHub Interchange Reference</ns0:Description>
              </ns0:Type>
              <ns0:ReferenceNumber>
                <xsl:value-of select="$SubscriberVGMReference"/>
              </ns0:ReferenceNumber>
            </ns0:AdditionalReference>
          </ns0:AdditionalReferenceCollection>
        </ns0:Container>
      </xsl:for-each>
    </ns0:ContainerCollection>
  </xsl:template>

  <xsl:template name="PopulateOrganisationAddress">
    <xsl:param name="org"/>
    <xsl:param name="generateRegistrationNumber"/>

    <ns0:OrganizationAddress>
      <xsl:copy-of select ="$org/ns0:AddressType"/>
      <xsl:copy-of select ="$org/ns0:OrganizationCode"/>
      <xsl:copy-of select ="$org/ns0:Address1"/>
      <xsl:copy-of select ="$org/ns0:Address2"/>
      <xsl:copy-of select ="$org/ns0:AddressOverride"/>
      <xsl:copy-of select ="$org/ns0:City"/>
      <xsl:copy-of select ="$org/ns0:CompanyName"/>
      <xsl:copy-of select ="$org/ns0:Contact"/>
      <xsl:copy-of select ="$org/ns0:Country"/>
      <xsl:copy-of select ="$org/ns0:Email"/>
      <xsl:copy-of select ="$org/ns0:Fax"/>
      <xsl:copy-of select ="$org/ns0:Phone"/>
      <xsl:copy-of select ="$org/ns0:Port"/>
      <xsl:copy-of select ="$org/ns0:Postcode"/>
      <xsl:copy-of select ="$org/ns0:State"/>

      <xsl:choose>
        <xsl:when test="$generateRegistrationNumber='Y'">
          <xsl:copy-of select ="$org/ns0:RegistrationNumberCollection"/>
        </xsl:when>
        <xsl:otherwise>
          <ns0:RegistrationNumberCollection />
        </xsl:otherwise>
      </xsl:choose>
    </ns0:OrganizationAddress>
  </xsl:template>


  <xsl:template match="ns0:AdditionalReferenceCollection">
    <ns0:AdditionalReferenceCollection>
      <xsl:for-each select="ns0:AdditionalReference[ns0:Type/ns0:Code/text()!='HIR']">
        <xsl:copy-of select ="."/>
      </xsl:for-each>
    </ns0:AdditionalReferenceCollection>
  </xsl:template>

  <xsl:template name="GetOrgAddressRegNumber">
    <xsl:param name="addressType" />

    <xsl:variable name="orgAddress" select="$shipment/ns0:OrganizationAddressCollection/ns0:OrganizationAddress[ns0:AddressType/text()=$addressType]"/>

    <xsl:choose>
      <xsl:when test="$orgAddress">
        <xsl:variable name="USCCC_Code" select="$orgAddress/ns0:RegistrationNumberCollection/ns0:RegistrationNumber[ns0:Type/ns0:Code/text()='CCC' and ns0:CountryOfIssue/ns0:Code/text()='US']/ns0:Value/text()"/>
        <xsl:choose>
          <xsl:when test="$USCCC_Code!=''">
            <xsl:value-of select="$USCCC_Code"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$orgAddress/ns0:RegistrationNumberCollection/ns0:RegistrationNumber[ns0:Type/ns0:Code/text()='C1C']/ns0:Value/text()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
    public string ThrowPartyReceiverIDNotFound(string SCACCode)
    {
      throw new ArgumentException(string.Format(@"Could not found matching PartyReceiverID in the Client Registration Lookup.(Client Registration: CARGOWISE, Input: [SCAC/C1C:{0}])", SCACCode));
    }
]]>
  </msxsl:script>
</xsl:stylesheet>
