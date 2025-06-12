<?xml version="1.0" encoding="UTF-16"?>

<xsl:stylesheet xmlns:xsl        = "http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl      = "urn:schemas-microsoft-com:xslt"
                xmlns:var        = "http://schemas.microsoft.com/BizTalk/2003/var"
                exclude-result-prefixes="msxsl var ScriptNS0" version="1.0"
                xmlns:ns0        = "http://www.cargowise.com/Schemas/Universal/2011/11"
                xmlns:ScriptNS0  = "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0" >
  
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  
  <xsl:variable name="Sender"    select="'AGSWORAGS_ACF'" />    <!-- "ACFS" -->
  <xsl:variable name="Recipient" select="'AGSWORAGS'" />        <!-- "AGS World Transport" -->
  <xsl:variable name="TS_Name"   select="'ACFS Container Event XML - Receive as Events'" />
  
  
  <xsl:variable name="EventDates" select="/*[local-name()='ContainerEvents']
                                          /*[local-name()='EventDates'] [1]" />
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template match="/">
    
    <ns0:UniversalInterchange>
      <xsl:call-template name="Map_Header" />
      
      <ns0:Body>
        <ns0:UniversalShipment>
          <ns0:Shipment>
            <xsl:call-template name="Map_DataContext" />
            
            <!-- ContainerCollection: -->
            
            <ns0:ContainerCollection Content="Partial">
              <ns0:Container>
                <xsl:call-template name="Map_ContainerDetails" />
              </ns0:Container>
            </ns0:ContainerCollection>
            
          </ns0:Shipment>
        </ns0:UniversalShipment>
      </ns0:Body>
    </ns0:UniversalInterchange>
    
  </xsl:template>  <!-- End: Main Template -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_Header">
    
    <ns0:Header>
      <ns0:SenderID>
        <xsl:value-of select="$Sender" />
      </ns0:SenderID>
      
      <ns0:RecipientID>
        <xsl:value-of select="$Recipient" />
      </ns0:RecipientID>
    </ns0:Header>
    
  </xsl:template>  <!-- Template: "Map_Header" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_DataContext">
    
    <ns0:DataContext>
      
      <xsl:variable name="DataProvider" select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name, 'Defaults', 'Data Provider' )" />
      <xsl:variable name="CompanyCode"  select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name, 'Defaults', 'Company Code'  )" />
      <xsl:variable name="EnterpriseID" select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name, 'Defaults', 'Enterprise ID' )" />
      <xsl:variable name="ServerID"     select="ScriptNS0:GetRecipientCodeUnkeyed( $Sender, $Recipient, $TS_Name, 'Defaults', 'Server ID'     )" />
      
      
      <xsl:if test="($DataProvider != '')">
        <ns0:DataProvider>
          <xsl:value-of select="$DataProvider" />
        </ns0:DataProvider>
      </xsl:if>
      
      <ns0:DataTargetCollection>
        <ns0:DataTarget>
          
          <ns0:Type>
            <xsl:text>CustomsDeclaration</xsl:text>
          </ns0:Type>
          
          <ns0:Key>
            <xsl:value-of select="$EventDates/*[local-name()='EnterpriseJobNumber']" />
          </ns0:Key>
          
        </ns0:DataTarget>
      </ns0:DataTargetCollection>
      
      <xsl:if test="($CompanyCode != '')">
        <ns0:Company>
          <ns0:Code>
            <xsl:value-of select="$CompanyCode" />
          </ns0:Code>
        </ns0:Company>
      </xsl:if>
      
      <xsl:if test="($EnterpriseID != '')">
        <ns0:EnterpriseID>
          <xsl:value-of select="$EnterpriseID" />
        </ns0:EnterpriseID>
      </xsl:if>
      
      <xsl:if test="($ServerID != '')">
        <ns0:ServerID>
          <xsl:value-of select="$ServerID" />
        </ns0:ServerID>
      </xsl:if>
      
    </ns0:DataContext>
    
  </xsl:template>  <!-- Template: "Map_DataContext" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_ContainerDetails">
    
    <xsl:variable name="ContainerNo"  select="$EventDates/*[local-name()='ContainerNumber']" />
    <xsl:variable name="TheTimestamp" select="$EventDates/*[local-name()='EventDate']" />
    
    <xsl:variable name="TheFieldName" select="ScriptNS0:GetRecipientCode( $Sender, $Recipient, $TS_Name, 
                                                  'Event Types', 'Field Name',
                                                  $EventDates/*[local-name()='EventType'] )" />
    
    <xsl:if test=" ($ContainerNo  != '')
               and ($TheFieldName != '')
               and ($TheTimestamp != '') ">
      
      <!-- ContainerNumber: -->
      
      <xsl:call-template name="Map_String_IfNotEmpty">
        <xsl:with-param name="Node" select="'ns0:ContainerNumber'" />
        <xsl:with-param name="Data" select="$ContainerNo" />
        <xsl:with-param name="Max"  select="20" />
      </xsl:call-template>
      
      
      <!-- The date field: -->
      
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="concat('ns0:', $TheFieldName )" />
        <xsl:with-param name="Data" select="$TheTimestamp" />
      </xsl:call-template>
      
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_ContainerDetails" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="Map_String_IfNotEmpty">
    <xsl:param name="Node" />
    <xsl:param name="Data" />
    <xsl:param name="Max"  />
    
    
    <xsl:variable name="TheData" select="substring( substring( $Data, 
           string-length(
                          substring-before( $Data, 
                                            substring( normalize-space($Data), 1, 1 )
                                           )
                        ) +1
           ), 1, $Max )" />
    
    
    <xsl:if test="($TheData != '')">
      <xsl:call-template name="MapValueIfNotEmpty">
        <xsl:with-param name="Node" select="$Node" />
        <xsl:with-param name="Data" select="$TheData" />
      </xsl:call-template>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "Map_String_IfNotEmpty" -->
  
  
<!-- ======================================================================================================== -->
  
  <xsl:template name="MapValueIfNotEmpty">
    <xsl:param name="Node" />
    <xsl:param name="Data" />
    
    <xsl:if test="($Data != '')">
      <xsl:choose>
        
        <xsl:when test="contains($Node, '/')">
          <xsl:element name="{substring-before($Node, '/')}">
            <xsl:call-template name="MapValueIfNotEmpty">
              <xsl:with-param name="Node" select="substring-after($Node, '/')" />
              <xsl:with-param name="Data" select="$Data" />
            </xsl:call-template>
          </xsl:element>
        </xsl:when>
        
        <xsl:otherwise>
          <xsl:element name="{$Node}">
            <xsl:value-of select="$Data" />
          </xsl:element>
        </xsl:otherwise>
        
      </xsl:choose>
    </xsl:if>
    
  </xsl:template>  <!-- Template: "MapValueIfNotEmpty" -->
  
  
<!-- ======================================================================================================== -->
  
  
</xsl:stylesheet>