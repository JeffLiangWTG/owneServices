<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ns0 CodeMapper ContextAccessor DataModelAccessor OCMHelper" version="1.0"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:OCMHelper="http://schemas.microsoft.com/BizTalk/2003/OCMHelper">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:variable name="ServiceProvider" select="OCMHelper:GetServiceProvider($RecipientID)" />

  <xsl:variable name="CarrierName" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'CarrierName', $ServiceProvider)" />
  <xsl:variable name="CarrierMSGID" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'MSGID', $ServiceProvider)" />
  <xsl:variable name="CarrierSubscriptionPrefix" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'SubscriptionPrefix', $ServiceProvider)" />
  <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name',concat('CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.', $CarrierName),'@maxlength','14')" />

  <xsl:variable name="shipment" select="/ns0:UniversalShipment/ns0:Shipment"/>
  <xsl:variable name="consolID" select="$shipment/ns0:DataContext/ns0:DataSourceCollection/ns0:DataSource/ns0:Key/text()"/>
  <xsl:variable name="forwardingType" select="$shipment/ns0:DataContext/ns0:DataSourceCollection/ns0:DataSource/ns0:Type/text()"/>
  <xsl:variable name="shipmentType" select="$shipment/ns0:ShipmentType/ns0:Code/text()"/>
  <xsl:variable name="isCoLoad" select="OCMHelper:IsCoLoad($shipmentType) = 'TRUE'" />

  <xsl:variable name="containerNo" select="$shipment/ns0:ContainerCollection/ns0:Container/ns0:ContainerNumber/text()"/>

  <xsl:variable name="previousConsolReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $CarrierName, '@recipientId', $SenderID , '@ST_ID', $CarrierMSGID, '@value', concat($consolID, '_', $containerNo), '@referenceType', 'JobNumber')" />

  <xsl:variable name="SubscriberConsolReference">
    <xsl:choose>
      <xsl:when test="$previousConsolReference!=''">
        <xsl:value-of select="$previousConsolReference" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="formattedCounter" select='format-number($InterchangeNum, "0000000000")' />
        <xsl:variable name="NewConsolReference" select="concat($CarrierSubscriptionPrefix, $formattedCounter)" />

        <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $NewConsolReference, concat($consolID, '_', $containerNo), 'JobNumber')" />
        <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, concat($consolID, '_', $containerNo), $NewConsolReference, 'JobNumber')" />
        <xsl:value-of select="$NewConsolReference" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="purposeCode" select="$shipment/ns0:DataContext/ns0:DocumentaryOverride/ns0:Purpose/ns0:Code/text()"/>
  <xsl:variable name="subscribeInterchangeNum" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $InterchangeNum, $consolID, 'InterchangeNumber')" />
  <xsl:variable name="subscribePurpose" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, $purposeCode, 'ActionPurpose')" />
  <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, $shipmentType, 'ShipmentType')" />
  <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, 'Verified Gross Container Weight', 'DocumentName')" />
  <xsl:variable name="SubscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, $forwardingType, 'ForwardingType')" />

  <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
  <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $InboxPK, $InterchangeNum)" />

  <xsl:variable name="previousPartyType" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $CarrierName, '@recipientId', $SenderID , '@ST_ID', $CarrierMSGID, '@value', $consolID, '@referenceType', 'PartyType')" />
  <xsl:variable name="SubscribeNewPartyType">
    <xsl:choose>
      <xsl:when test="$previousPartyType=''">
        <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, 'ShippingLine', 'PartyType')" />
      </xsl:when>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="formVersion" select="$shipment/ns0:AddInfoCollection/ns0:AddInfo[ns0:Key/text()='FormVersion']/ns0:Value/text()" />
  <xsl:variable name="SubscribeFormVersion">
    <xsl:if test="$formVersion !=''">
      <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, $formVersion, 'FormVersion')"/>
    </xsl:if>
  </xsl:variable>

  <xsl:variable name="ClientID" select="DataModelAccessor:GetClientRegistrationCode($SenderID, $shipment/ns0:DataContext/ns0:EventBranch/ns0:Code/text(), $CarrierName)" />
  <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat($ClientID,'_',$RecipientID,'_', $InterchangeNum))"/>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="ns0:Shipment">
    <ns0:Shipment>
      <xsl:apply-templates select="@* | *"/>
      <xsl:call-template name="AdditionalReferenceCollection"/>
    </ns0:Shipment>
  </xsl:template>

  <xsl:template match="ns0:DataTargetCollection|ns0:UniversalShipment/ns0:Shipment/ns0:OrganizationAddressCollection|ns0:DataSourceCollection|ns0:BookingConfirmationReference|ns0:WayBillNumber|ns0:CoLoadBookingConfirmationReference|ns0:CoLoadMasterBillNumber|ns0:AdditionalReferenceCollection" />

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:ShipmentType">

    <ns0:BookingConfirmationReference>
      <xsl:call-template name="GetBookingConfirmationRef" />
    </ns0:BookingConfirmationReference>
    <ns0:WayBillNumber>
      <xsl:call-template name="GetWaybillNumber" />
    </ns0:WayBillNumber>

    <xsl:copy-of select ="."/>
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
        <ns0:Code>CAR</ns0:Code>
        <ns0:Description>ShippingLine</ns0:Description>
        <ns0:ServiceCode>VGM</ns0:ServiceCode>
        <ns0:ServiceDescription>Verified Gross Container Weight</ns0:ServiceDescription>
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

  <xsl:template name="AdditionalReferenceCollection">
    <xsl:variable name="hirReference" select="ns0:AdditionalReferenceCollection/ns0:AdditionalReference[ns0:Type/ns0:Code/text() = 'HIR']"/>

    <ns0:AdditionalReferenceCollection>
      <xsl:copy-of select ="ns0:AdditionalReferenceCollection/ns0:AdditionalReference[ns0:Type/ns0:Code/text() != 'HIR']"/>

      <ns0:AdditionalReference>
        <ns0:Type>
          <ns0:Code>HIR</ns0:Code>
          <ns0:Description>eHub Interchange Reference</ns0:Description>
        </ns0:Type>
        <ns0:ReferenceNumber>
          <xsl:choose>
            <xsl:when test="string-length(normalize-space($hirReference)) = 0">
              <xsl:value-of select="$SubscriberConsolReference"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$hirReference"/>
            </xsl:otherwise>
          </xsl:choose>
        </ns0:ReferenceNumber>
      </ns0:AdditionalReference>
    </ns0:AdditionalReferenceCollection>
  </xsl:template>

</xsl:stylesheet>
