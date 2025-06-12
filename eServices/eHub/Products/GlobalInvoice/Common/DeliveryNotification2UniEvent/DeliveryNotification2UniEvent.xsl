<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="ns0 ScriptNS0 ScriptNS1 ScriptNS2 userCSharp" version="1.0"
                xmlns:ns0="http://cargowise.com/ehub/core/2018/06"
                xmlns:ns1="http://www.cargowise.com/Schemas/Universal/2012/11"
                xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
                xmlns:ScriptNS1="http://schemas.microsoft.com/BizTalk/2003/ScriptNS1"
                xmlns:ScriptNS2="http://schemas.microsoft.com/BizTalk/2003/ScriptNS2"
                xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/ns0:DeliveryNotificationMessage" />
  </xsl:template>
  <xsl:template match="ns0:DeliveryNotificationMessage">
    <xsl:apply-templates select="//*[local-name()='GlobalElectronicInvoicing']" />
  </xsl:template>

  <xsl:template match="//*[local-name()='GlobalElectronicInvoicing']">

    <xsl:variable name="notificationType" select="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='NotificationType']/text()"/>
    <xsl:variable name="electronicInvoiceBatchRequest" select="//*[local-name()='GlobalElectronicInvoicing']/*[local-name()='Header']/*[local-name()='ElectronicInvoiceBatchRequest']"/>
    <xsl:variable name="messagingSystem" select="$electronicInvoiceBatchRequest/*[local-name()='MessagingSystem']"/>
    <xsl:variable name="RecipientID" select="ScriptNS1:GetContextProperty('DestinationParty', 'http://schemas.microsoft.com/BizTalk/2003/system-properties')"/>

    <ns1:UniversalInterchangeInclude>
      <ns1:Body>
        <ns1:UniversalEvent>
          <ns1:Event>
            <ns1:EventTime>
              <xsl:value-of select="ScriptNS0:CurrentDateTime('s')" />
            </ns1:EventTime>
            <ns1:EventType>
              <xsl:value-of select="ScriptNS2:GetRecipientCode('GLB_ELEC_INVOICING' , 'GLB_ELEC_INVOICING' , 'GEI DeliveryNotification to UniEvent' , 'Notification Type' , 'Event Type', $notificationType, $messagingSystem)"/>
            </ns1:EventType>
            <ns1:DataContext>
              <ns1:DataTargetCollection>
                <ns1:DataTarget>
                  <ns1:Type>AccEInvoicingBatch</ns1:Type>
                  <ns1:Key>
                    <xsl:value-of select="$electronicInvoiceBatchRequest/*[local-name()='BatchNumber']"/>
                  </ns1:Key>
                </ns1:DataTarget>
              </ns1:DataTargetCollection>
            </ns1:DataContext>
            <ns1:EventParameters>
              <ns1:MessageType>
                <xsl:value-of select="ScriptNS2:GetRecipientCode('GLB_ELEC_INVOICING' , 'GLB_ELEC_INVOICING' , 'GEI DeliveryNotification to UniEvent' , 'Messaging System' , 'Message Type', $messagingSystem)"/>
              </ns1:MessageType>
              <ns1:MessageSubType>
                <xsl:value-of select="ScriptNS2:GetRecipientCode('GLB_ELEC_INVOICING' , 'GLB_ELEC_INVOICING' , 'GEI DeliveryNotification to UniEvent' , 'Messaging System' , 'Message SubType', $messagingSystem)"/>
              </ns1:MessageSubType>
              <xsl:variable name="errorDescription" select="/*[local-name()='DeliveryNotificationMessage']/*[local-name()='ErrorDescription']/text()" />
              <xsl:if test="$errorDescription != '' and  $notificationType!= 'ACK'">
                <ns1:Reason>
                  <xsl:value-of select="userCSharp:GetErrorDescription($errorDescription)"/>
                </ns1:Reason>
              </xsl:if>
            </ns1:EventParameters>
            <ns1:ContextCollection>
              <ns1:Context>
                <ns1:Type>CompanyCode</ns1:Type>
                <ns1:Value>
                  <xsl:value-of select="substring($RecipientID, 4 ,3)"/>
                </ns1:Value>
              </ns1:Context>
            </ns1:ContextCollection>
          </ns1:Event>
        </ns1:UniversalEvent>
      </ns1:Body>
    </ns1:UniversalInterchangeInclude>

  </xsl:template>
  <msxsl:script language="C#" implements-prefix="userCSharp">
    <![CDATA[
     public string GetErrorDescription(string errorSt)
     {
         if (errorSt.Contains("--->"))
         {
             var index1 = errorSt.IndexOf("--->", StringComparison.Ordinal);
             var string1 = errorSt.Substring(index1 + 1);
             var stringArray = string1.Split(new[] { ": " , "at "}, StringSplitOptions.None);
             return stringArray[1].TrimEnd();
         }
         return errorSt;
     }
    ]]>
  </msxsl:script>
</xsl:stylesheet>
