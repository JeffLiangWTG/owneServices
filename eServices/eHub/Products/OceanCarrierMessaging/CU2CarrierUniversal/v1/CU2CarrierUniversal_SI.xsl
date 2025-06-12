<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl ns0 userCSharp CodeMapper DataModelAccessor ContextAccessor OCMHelper SubscriptionHelper" version="1.0"
                xmlns:ns0="http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:CodeMapper="http://schemas.microsoft.com/BizTalk/2003/CodeMapper"
                xmlns:ContextAccessor="http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"
                xmlns:DataModelAccessor="http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor"
                xmlns:OCMHelper="http://schemas.microsoft.com/BizTalk/2003/OCMHelper"
                xmlns:SubscriptionHelper="http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:variable name="shipment" select="/ns0:UniversalShipment/ns0:Shipment"/>

  <xsl:variable name="SenderID" select="ContextAccessor:GetContextProperty('SourceParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>
  <xsl:variable name="RecipientID" select="ContextAccessor:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

  <xsl:variable name="ServiceProvider" select="OCMHelper:GetServiceProvider($RecipientID)" />

  <xsl:variable name="CarrierName" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'CarrierName', $ServiceProvider)" />
  <xsl:variable name="CarrierMSGID" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'MSGID', $ServiceProvider)" />
  <xsl:variable name="CarrierSubscriptionPrefix" select="CodeMapper:GetRecipientCode('SHIPPING_INSTRUCTION', 'SHIPPING_INSTRUCTION', 'OCM System Configuration', 'CarrierSettings', 'SubscriptionPrefix', $ServiceProvider)" />
  <xsl:variable name="InterchangeNum" select="CodeMapper:CallActionProcedureHelper('GetCounterValue','@value','@name',concat('CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.', $CarrierName),'@maxlength','14')" />
  <xsl:variable name="consolID" select="$shipment/ns0:DataContext/ns0:DataSourceCollection/ns0:DataSource/ns0:Key[1]" />
  <xsl:variable name="forwardingType" select="$shipment/ns0:DataContext/ns0:DataSourceCollection/ns0:DataSource/ns0:Type/text()"/>
  <xsl:variable name="shipmentType" select="$shipment/ns0:ShipmentType/ns0:Code/text()"/>
  <xsl:variable name="isCoLoad" select="OCMHelper:IsCoLoad($shipmentType) = 'TRUE'" />
  <xsl:variable name="groupingMethod" select="$shipment/ns0:AddInfoCollection/ns0:AddInfo[ns0:Key/text()='GroupingMethod']/ns0:Value/text()" />

  <xsl:variable name="previousConsolReference" select="CodeMapper:CallActionProcedureHelper('SelectSubscribedReference', '@reference', '@senderId', $CarrierName, '@recipientId', $SenderID , '@ST_ID', $CarrierMSGID, '@value', $consolID, '@referenceType', 'JobNumber')" />
  <xsl:variable name="SubscriberConsolReference">
    <xsl:choose>
      <xsl:when test="$previousConsolReference!=''">
        <xsl:value-of select="$previousConsolReference" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="formattedCounter" select='format-number($InterchangeNum, "0000000000")' />
        <xsl:variable name="NewConsolReference" select="concat($CarrierSubscriptionPrefix, $formattedCounter)" />
        <xsl:variable name="SubScriberValue1" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $NewConsolReference, $consolID, 'JobNumber')" />
        <xsl:variable name="SubScriberValue2" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, $NewConsolReference, 'JobNumber')" />
        <xsl:value-of select="$NewConsolReference" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="purposeCode" select="$shipment/ns0:DataContext/ns0:DocumentaryOverride/ns0:Purpose/ns0:Code/text()"/>
  <xsl:variable name="subscribePurpose" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, $purposeCode, 'ActionPurpose')" />
  <xsl:variable name="subscribeShipmentType" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, $shipmentType, 'ShipmentType')" />
  <xsl:variable name="SubscribeDocumentName" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, 'Shipping Instruction', 'DocumentName')" />
  <xsl:variable name="SubscribeForwardingType" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, $forwardingType, 'ForwardingType')" />

  <xsl:variable name="InboxPK" select="ContextAccessor:GetContextProperty('InternalTrackingID', 'http://cargowise.com/ehub/tracking/2010/06')"/>
  <xsl:variable name="SubscribeInboxPK" select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $InboxPK, $InterchangeNum)" />

  <xsl:variable name="portOfLoadingCountryCode" select="substring($shipment/ns0:PortOfLoading/ns0:Code/text(), 1, 2)"/>
  <xsl:variable name="portOfDischargeCountryCode" select="substring($shipment/ns0:PortOfDischarge/ns0:Code/text(), 1, 2)"/>
  <xsl:variable name="portOfDestinationCountryCode" select="substring($shipment/ns0:PortOfDestination/ns0:Code/text(), 1, 2)"/>

  <xsl:variable name="ClientID" select="DataModelAccessor:GetClientRegistrationCode($SenderID, $shipment/ns0:DataContext/ns0:EventBranch/ns0:Code/text(), $CarrierName)" />
  <xsl:variable name="FileName" select="ContextAccessor:SetContextProperty('OverrideFilename', 'http://cargowise.com/ehub/processing/2010/06', concat($ClientID,'_',$RecipientID,'_', $InterchangeNum))"/>

  <xsl:variable name="formVersion" select="$shipment/ns0:AddInfoCollection/ns0:AddInfo[ns0:Key/text()='FormVersion']/ns0:Value/text()" />
  <xsl:variable name="SubscribeFormVersion">
    <xsl:if test="$formVersion !=''">
      <xsl:value-of select="DataModelAccessor:InsertSubscriptionValue($CarrierMSGID, $CarrierName, $SenderID, $consolID, $formVersion, 'FormVersion')"/>
    </xsl:if>
  </xsl:variable>

  <xsl:variable name="InsertBoleroSubscription" select="SubscriptionHelper:InsertBoleroSubscription($SenderID, $ClientID, $ServiceProvider, $SubscriberConsolReference, $consolID)" />

  <xsl:template match="ns0:Shipment">
    <ns0:Shipment>
      <xsl:apply-templates select="@* | *"/>
      <xsl:call-template name="AdditionalReferenceCollection"/>
    </ns0:Shipment>
  </xsl:template>

  <xsl:template match="ns0:Shipment/ns0:OrganizationAddressCollection/ns0:OrganizationAddress">
    <xsl:call-template name="PopulateOrganisationAddressCore">
      <xsl:with-param name="org" select="." />
      <xsl:with-param name="elementName" select="'ns0:OrganizationAddress'" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ns0:Carrier">
    <xsl:call-template name="PopulateOrganisationAddressCore">
      <xsl:with-param name="org" select="." />
      <xsl:with-param name="elementName" select="'ns0:Carrier'" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ns0:Shipment/ns0:PackingLineCollection">
    <ns0:PackingLineCollection>
      <xsl:for-each select="ns0:PackingLine">
        <xsl:choose>
          <xsl:when test="$groupingMethod!=''">
            <xsl:copy-of select ="ns0:PackingLineCollection/ns0:PackingLine"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select ="."/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </ns0:PackingLineCollection>
  </xsl:template>

  <xsl:template match="ns0:SubShipment/ns0:OrganizationAddressCollection/ns0:OrganizationAddress">
    <xsl:call-template name="PopulateOrganisationAddressCore">
      <xsl:with-param name="org" select="." />
      <xsl:with-param name="elementName" select="'ns0:OrganizationAddress'" />
      <xsl:with-param name="generateRegistrationNumber" select="'N'" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ns0:Shipment/ns0:WayBillNumber|ns0:Shipment/ns0:BookingConfirmationReference" />

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:CoLoadBookingConfirmationReference">
    <ns0:CoLoadBookingConfirmationReference>
      <xsl:choose>
        <xsl:when test="$isCoLoad">
          <xsl:value-of select="$shipment/ns0:CoLoadBookingConfirmationReference/text()" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$shipment/ns0:BookingConfirmationReference/text()" />
        </xsl:otherwise>
      </xsl:choose>
    </ns0:CoLoadBookingConfirmationReference>
  </xsl:template>

  <xsl:template match="/ns0:UniversalShipment/ns0:Shipment/ns0:CoLoadMasterBillNumber">
    <ns0:CoLoadMasterBillNumber>
      <xsl:choose>
        <xsl:when test="$isCoLoad">
          <xsl:value-of select="$shipment/ns0:CoLoadMasterBillNumber/text()" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$shipment/ns0:WayBillNumber/text()" />
        </xsl:otherwise>
      </xsl:choose>
    </ns0:CoLoadMasterBillNumber>
  </xsl:template>

  <xsl:template name="PopulateOrganisationAddressCore">
    <xsl:param name="org"/>
    <xsl:param name="elementName"/>
    <xsl:param name="generateRegistrationNumber" select="'Y'"/>

    <xsl:element name="{$elementName}">
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
          <xsl:variable name="addressType" select="$org/ns0:AddressType/text()"/>
          <xsl:variable name="countryCode" select="$org/ns0:Country/ns0:Code/text()"/>


          <xsl:element name ="ns0:RegistrationNumberCollection">

            <xsl:for-each select="$org/ns0:RegistrationNumberCollection/ns0:RegistrationNumber">
              <xsl:variable name="regNumberTypeCode" select="ns0:Type/ns0:Code/text()" />
              <xsl:variable name="CountryOfIssue" select="ns0:CountryOfIssue/ns0:Code/text()" />

              <xsl:variable name="needToGenerateRegNumber">
                <xsl:choose>
                  <xsl:when test="$addressType='ConsigneeDocumentaryAddress' and not(starts-with(userCSharp:ToUpper($org/ns0:CompanyName/text()), 'TO ORDER')) and not(starts-with(userCSharp:ToUpper($org/ns0:CompanyName/text()), 'TO THE ORDER'))">Y</xsl:when>
                  <xsl:when test="$addressType = 'ConsignorDocumentaryAddress' or $addressType = 'NotifyParty'">Y</xsl:when>
                  <xsl:when test="$portOfDestinationCountryCode='IN' and ($addressType = 'ConsigneeDocumentaryAddress' or $addressType = 'NotifyParty') and $regNumberTypeCode='IEC'">Y</xsl:when>
                  <xsl:when test="$addressType='ShippingLineAddress' and (($CountryOfIssue='US' and $regNumberTypeCode='CCC') or $regNumberTypeCode='C1C')">Y</xsl:when>
                  <xsl:when test="$addressType='CoLoadWith' and (($CountryOfIssue='US' and $regNumberTypeCode='CCC') or $regNumberTypeCode='C1C')">Y</xsl:when>
                  <xsl:when test="$addressType='Carrier' and (($CountryOfIssue='US' and $regNumberTypeCode='CCC') or $regNumberTypeCode='C1C')">Y</xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:variable>

              <xsl:if test="$needToGenerateRegNumber='Y'">
                <ns0:RegistrationNumber>
                  <xsl:copy-of select ="*[local-name()='CountryOfIssue']"/>
                  <xsl:copy-of select ="*[local-name()='Type']"/>
                  <xsl:copy-of select ="*[local-name()='Value']"/>
                  <xsl:copy-of select ="*[local-name()='RegulatingCountry']"/>
                </ns0:RegistrationNumber>
              </xsl:if>
            </xsl:for-each>
          </xsl:element>
        </xsl:when>
        <xsl:otherwise>
          <ns0:RegistrationNumberCollection />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <xsl:template match="ns0:AdditionalReferenceCollection"/>
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

  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[

public string ToUpper(string input)
{
  string result = "";
  if (input != null)
  {
    result = input;
  }
  return result.ToUpper();
}

]]>

  </msxsl:script>
</xsl:stylesheet>
