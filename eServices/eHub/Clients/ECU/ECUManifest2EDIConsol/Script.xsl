<xsl:element name ="ns0:TotalOuterPacksQty">
<xsl:attribute name ="ns0:DimensionType">
<xsl:value-of select=".//*[local-name()='CargoDetail']/*[local-name()='Package']/Description"/>
</xsl:attribute>
<xsl:value-of select="sum(.//*[local-name()='CargoDetail']/*[local-name()='Package']/Amount)"/>
</xsl:element>

<xsl:element name ="ns0:Weight">
    <xsl:attribute name ="ns0:DimensionType">
        <xsl:variable name="UQ">
            <xsl:value-of select=".//*[local-name()='CargoDetail']/*[local-name()='Weight']/Unit"/>
        </xsl:variable>
        <xsl:choose>
            <xsl:when test="$UQ ='KGS'">KG</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$UQ"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:attribute>
    <xsl:value-of select="sum(.//*[local-name()='CargoDetail']/*[local-name()='Weight']/Value)"/>
</xsl:element>

<xsl:element name ="ns0:Volume">
    <xsl:attribute name ="ns0:DimensionType">
        <xsl:variable name="UQ">
            <xsl:value-of select=".//*[local-name()='CargoDetail']/*[local-name()='Volume']/Unit"/>
        </xsl:variable>
        <xsl:choose>
            <xsl:when test="$UQ ='CBM'">M3</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$UQ"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:attribute>
    <xsl:value-of select="sum(.//*[local-name()='CargoDetail']/*[local-name()='Volume']/Value)"/>
</xsl:element>

<xsl:variable name="SpecialInstr">
    <xsl:value-of select=".//*[local-name()='SpecialInstructions']"/>
</xsl:variable>
<xsl:element name ="ns0:Notes">
  <xsl:if test="string-length($SpecialInstr) > 0">
	<xsl:element name ="ns0:Note">
	  <xsl:element name ="ns0:NoteType">SpecialInstructions</xsl:element>
	  <xsl:element name ="ns0:NoteData">
		<xsl:value-of select="$SpecialInstr"/>
	  </xsl:element>
	</xsl:element>
  </xsl:if>
  <xsl:element name ="ns0:Note">
	<xsl:element name ="ns0:NoteType">DetailedGoodsDescription</xsl:element>
	<xsl:element name ="ns0:NoteData">
	  <xsl:for-each select="./*[local-name()='DETAIL']/*[local-name()='CargoDetail']">
		<xsl:value-of select="./*[local-name()='GoodDescription']"/>
		<xsl:value-of select="'&#10;'"/>
	  </xsl:for-each>
	</xsl:element>
  </xsl:element>
  <xsl:element name ="ns0:Note">
	<xsl:element name ="ns0:NoteType">MarksAndNumbers</xsl:element>
	<xsl:element name ="ns0:NoteData">
	  <xsl:for-each select="./*[local-name()='DETAIL']/*[local-name()='CargoDetail']">
		<xsl:value-of select="./*[local-name()='MarksNo']"/>
		<xsl:value-of select="'&#10;'"/>
	  </xsl:for-each>
	</xsl:element>
  </xsl:element>
</xsl:element>



<xsl:template name="ExtractAddressInfo">
    <xsl:param name="AddressStr"/>
    <xsl:param name="StartIndex"/>
    <xsl:param name="NumOfCharToGet"/>
    <xsl:param name="ElementName"/>
    <xsl:variable name="LenOfAddressStr">
        <xsl:value-of select="string-length($AddressStr)"/>
    </xsl:variable>
    <xsl:variable name="ActualStrLength">
        <xsl:value-of select="number($LenOfAddressStr)-number($StartIndex)+1"/>
    </xsl:variable>
    <xsl:if test="$ActualStrLength> 0">
        <xsl:element name="{$ElementName}">
            <xsl:choose>
                <xsl:when test="$ActualStrLength > $NumOfCharToGet">
                    <xsl:value-of select="substring($AddressStr, $StartIndex,$NumOfCharToGet)"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="substring($AddressStr, $StartIndex,$ActualStrLength)"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:element>
    </xsl:if>
</xsl:template>


<xsl:call-template name="ExtractAddressInfo">
    <xsl:with-param name="AddressStr" select="'01234567890123456789'"/>
    <xsl:with-param name="StartIndex" select="1"/>
    <xsl:with-param name="NumOfCharToGet" select="100"/>
    <xsl:with-param name="ElementName" select="'DocAddress'"/>
</xsl:call-template>



<xsl:template name="GenerateDocAddress">
    <xsl:param name="OrganisationType"/>
    <xsl:param name="DocAddressType"/>
    <xsl:element name ="ns0:DocAddress">
        <xsl:attribute name="AddressType">
            <xsl:value-of select="$DocAddressType"></xsl:value-of>
        </xsl:attribute>
        <xsl:variable name="ShipperAddr">
            <xsl:value-of select="./*[local-name()=$OrganisationType]/NAD"/>
        </xsl:variable>
        <xsl:variable name="ShipperAddrLength">
            <xsl:value-of select="string-length($ShipperAddr)"/>
        </xsl:variable>
        <xsl:variable name="OwnerCodeOrName">
            <xsl:choose>
                <xsl:when test="$ShipperAddrLength >=50">
                    <xsl:value-of select="substring($ShipperAddr, 1, 50)"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="substring($ShipperAddr, 1, $ShipperAddrLength)"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:element name ="ns0:AddressReference">
            <xsl:element name ="ns0:AddressSequenceRef">1</xsl:element>
            <xsl:element name ="ns0:Organisation">
                <xsl:attribute name="OwnerCode">
                    <xsl:value-of select="$OwnerCodeOrName"/>
                </xsl:attribute>
                <xsl:element name ="ns0:OrganisationDetails">
                    <xsl:element name ="ns0:Name">
                        <xsl:value-of select="$OwnerCodeOrName"/>
                    </xsl:element>
                    <xsl:if test="$ShipperAddrLength > 50">
                        <xsl:element name ="ns0:Addresses">
                            <xsl:element name ="ns0:Address">
                                <xsl:element name ="ns0:AddressLine1">
                                    <xsl:call-template name="ExtractAddressStr">
                                        <xsl:with-param name="AddressStr" select="$ShipperAddr"/>
                                        <xsl:with-param name="StartIndex" select="50"/>
                                        <xsl:with-param name="NumOfCharToGet" select="50"/>
                                    </xsl:call-template>
                                </xsl:element>
                                <xsl:if test="$ShipperAddrLength > 100">
                                    <xsl:element name ="ns0:AddressLine2">
                                        <xsl:call-template name="ExtractAddressStr">
                                            <xsl:with-param name="AddressStr" select="$ShipperAddr"/>
                                            <xsl:with-param name="StartIndex" select="100"/>
                                            <xsl:with-param name="NumOfCharToGet" select="50"/>
                                        </xsl:call-template>
                                    </xsl:element>
                                </xsl:if>
                                <xsl:if test="$ShipperAddrLength > 150">
                                    <xsl:element name ="ns0:CityOrSuburb">
                                        <xsl:call-template name="ExtractAddressStr">
                                            <xsl:with-param name="AddressStr" select="$ShipperAddr"/>
                                            <xsl:with-param name="StartIndex" select="150"/>
                                            <xsl:with-param name="NumOfCharToGet" select="25"/>
                                        </xsl:call-template>
                                    </xsl:element>
                                </xsl:if>
                                <xsl:if test="$ShipperAddrLength > 175">
                                    <xsl:element name ="ns0:StateOrProvince">
                                        <xsl:call-template name="ExtractAddressStr">
                                            <xsl:with-param name="AddressStr" select="$ShipperAddr"/>
                                            <xsl:with-param name="StartIndex" select="175"/>
                                            <xsl:with-param name="NumOfCharToGet" select="25"/>
                                        </xsl:call-template>
                                    </xsl:element>
                                </xsl:if>
                                <xsl:if test="$ShipperAddrLength > 200">
                                    <xsl:element name ="ns0:PostCode">
                                        <xsl:call-template name="ExtractAddressStr">
                                            <xsl:with-param name="AddressStr" select="$ShipperAddr"/>
                                            <xsl:with-param name="StartIndex" select="200"/>
                                            <xsl:with-param name="NumOfCharToGet" select="10"/>
                                        </xsl:call-template>
                                    </xsl:element>
                                </xsl:if>
                                <xsl:if test="$OrganisationType='Consignee'">
                                    TEST                                    
                                </xsl:if>
                            </xsl:element>
                        </xsl:element>
                    </xsl:if>
                </xsl:element>
            </xsl:element>
        </xsl:element>
    </xsl:element>
</xsl:template>





<xsl:element name ="ns0:DocAddress">
    <xsl:attribute name="AddressType">CRD</xsl:attribute>
    <xsl:variable name="ShipperAddr">
        <xsl:value-of select="./*[local-name()='Shipper']/NAD"/>
    </xsl:variable>
    <xsl:variable name="ShipperAddrLength">
        <xsl:value-of select="string-length($ShipperAddr)"/>
    </xsl:variable>
    <xsl:variable name="OwnerCodeOrName">
        <xsl:choose>
            <xsl:when test="$ShipperAddrLength >=50">
                <xsl:value-of select="substring($ShipperAddr, 1, 50)"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="substring($ShipperAddr, 1, $ShipperAddrLength)"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:variable>
    <xsl:element name ="ns0:AddressReference">
        <xsl:element name ="ns0:Organisation">
            <xsl:attribute name="OwnerCode">
                <xsl:value-of select="$OwnerCodeOrName"/>
            </xsl:attribute>
            <xsl:element name ="ns0:OrganisationDetails">
                <xsl:element name ="ns0:Name">
                    <xsl:value-of select="$OwnerCodeOrName"/>
                </xsl:element>
                <xsl:if test="$ShipperAddrLength > 50">
                    <xsl:element name ="ns0:Addresses">
                        <xsl:element name ="ns0:Address">
                            <xsl:element name ="ns0:AddressLine1">
                                <xsl:call-template name="ExtractAddressStr">
                                    <xsl:with-param name="AddressStr" select="$ShipperAddr"/>
                                    <xsl:with-param name="StartIndex" select="50"/>
                                    <xsl:with-param name="NumOfCharToGet" select="50"/>
                                </xsl:call-template>
                            </xsl:element>
                            <xsl:if test="$ShipperAddrLength > 100">
                                <xsl:element name ="ns0:AddressLine2">
                                    <xsl:call-template name="ExtractAddressStr">
                                        <xsl:with-param name="AddressStr" select="$ShipperAddr"/>
                                        <xsl:with-param name="StartIndex" select="100"/>
                                        <xsl:with-param name="NumOfCharToGet" select="50"/>
                                    </xsl:call-template>
                                </xsl:element>
                            </xsl:if>
                            <xsl:if test="$ShipperAddrLength > 150">
                                <xsl:element name ="ns0:CityOrSuburb">
                                    <xsl:call-template name="ExtractAddressStr">
                                        <xsl:with-param name="AddressStr" select="$ShipperAddr"/>
                                        <xsl:with-param name="StartIndex" select="150"/>
                                        <xsl:with-param name="NumOfCharToGet" select="25"/>
                                    </xsl:call-template>
                                </xsl:element>
                            </xsl:if>
                            <xsl:if test="$ShipperAddrLength > 175">
                                <xsl:element name ="ns0:StateOrProvince">
                                    <xsl:call-template name="ExtractAddressStr">
                                        <xsl:with-param name="AddressStr" select="$ShipperAddr"/>
                                        <xsl:with-param name="StartIndex" select="175"/>
                                        <xsl:with-param name="NumOfCharToGet" select="25"/>
                                    </xsl:call-template>
                                </xsl:element>
                            </xsl:if>
                            <xsl:if test="$ShipperAddrLength > 200">
                                <xsl:element name ="ns0:PostCode">
                                    <xsl:call-template name="ExtractAddressStr">
                                        <xsl:with-param name="AddressStr" select="$ShipperAddr"/>
                                        <xsl:with-param name="StartIndex" select="200"/>
                                        <xsl:with-param name="NumOfCharToGet" select="10"/>
                                    </xsl:call-template>
                                </xsl:element>
                            </xsl:if>
                        </xsl:element>
                    </xsl:element>
                </xsl:if>
            </xsl:element>
        </xsl:element>
    </xsl:element>
</xsl:element>
    
    


<xsl:element name ="ns0:DocAddress">
    <xsl:variable name="ShipperAddr">
        <xsl:value-of select="./*[local-name()='Shipper']/NAD"/>
    </xsl:variable>
    <xsl:variable name="ShipperAddrLength">
        <xsl:value-of select="string-length($ShipperAddr)"/>
    </xsl:variable>
    <xsl:variable name="OwnerCodeOrName">
        <xsl:choose>
            <xsl:when test="$ShipperAddrLength >=50">
                <xsl:value-of select="substring($ShipperAddr, 1, 50)"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="substring($ShipperAddr, 1, $ShipperAddrLength)"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:variable>
    <xsl:element name ="ns0:AddressReference">
        <xsl:element name ="ns0:Organisation">
            <xsl:attribute name="OwnerCode">
                <xsl:value-of select="$OwnerCodeOrName"/>
            </xsl:attribute>
            <xsl:element name ="ns0:OrganisationDetails">
                <xsl:element name ="ns0:Name">
                    <xsl:value-of select="$OwnerCodeOrName"/>
                </xsl:element>
            </xsl:element>
        </xsl:element>
    </xsl:element>
</xsl:element>




<xsl:variable name="ShipperAddrLength">
        <xsl:value-of select="string-length($ShipperAddr)"/>
    </xsl:variable>
    <xsl:attribute name="AddressType">CRD</xsl:attribute>
    <xsl:variable name="OwnerCodeOrName">
        <xsl:choose>
            <xsl:when test="$ShipperAddrLength >=50">
                <xsl:value-of select="substring($ShipperAddr, 1, 50)"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="substring($ShipperAddr, 1, $ShipperAddrLength)"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:variable>
    <xsl:element name ="ns0:AddressReference">
        <xsl:element name ="ns0:Organisation">
            <xsl:attribute name="OwnerCode">
                <xsl:value-of select="$OwnerCodeOrName"/>
            </xsl:attribute>
            <xsl:element name ="ns0:OrganisationDetails">
                <xsl:element name ="ns0:Name">
                    <xsl:value-of select="$OwnerCodeOrName"/>
                </xsl:element>
            </xsl:element>
        </xsl:element>
    </xsl:element>
</xsl:element>


<xsl:element name ="ns0:ContainerMode">
	  <xsl:value-of select="local-name()"/>
</xsl:element>


<xsl:element name ="ns0:ContainerMode">
  <xsl:value-of select="../*[local-name()='ContainerInfo']"/>
</xsl:element>


<xsl:element name ="ns0:ContainerMode">
  <xsl:value-of select="../../*[local-name()='ContainerInfo']/*[local-name()='Container']/*[local-name()='CntrInfo']/*[local-name()='ShipType']"/>
</xsl:element>

<xsl:value-of select=".//*[local-name()='CargoDetail']/*[local-name()='Weight']/Unit"/>


<xsl:element name ="ns0:ContainerNumber">
  <xsl:variable name="TempContainerId">
	<xsl:value-of select="./@CntrId"/>
  </xsl:variable>
  <xsl:value-of select="userCSharp:TrimContainerNumber(string(../../../../*[local-name()='ContainerInfo']/*[local-name()='Container'][@CntrID=$TempContainerId]/*[local-name()='CntrInfo']/*[local-name()='CntrName']))"/> 
</xsl:element>

<xsl:element name ="ns0:GoodsDescription">
  <xsl:value-of select="./*[local-name()='DETAIL']/*[local-name()='CargoDetail']/*[local-name()='GoodDescription']"/>
</xsl:element>

<xsl:element name ="ns0:MarksAndNumbers">
  <xsl:value-of select="./*[local-name()='DETAIL']/*[local-name()='CargoDetail']/*[local-name()='MarksNo']"/>
</xsl:element>

