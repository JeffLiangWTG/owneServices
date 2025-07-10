<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:cac="urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2" xmlns:cbc="urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2" xmlns:ccts="urn:un:unece:uncefact:documentation:2" xmlns:clm54217="urn:un:unece:uncefact:codelist:specification:54217:2001" xmlns:clm5639="urn:un:unece:uncefact:codelist:specification:5639:1988" xmlns:clm66411="urn:un:unece:uncefact:codelist:specification:66411:2001" xmlns:clmIANAMIMEMediaType="urn:un:unece:uncefact:codelist:specification:IANAMIMEMediaType:2003" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:link="http://www.xbrl.org/2003/linkbase" xmlns:n1="urn:oasis:names:specification:ubl:schema:xsd:Invoice-2" xmlns:qdt="urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2" xmlns:udt="urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2" xmlns:xbrldi="http://xbrl.org/2006/xbrldi" xmlns:xbrli="http://www.xbrl.org/2003/instance" xmlns:xdt="http://www.w3.org/2005/xpath-datatypes" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" exclude-result-prefixes="cac cbc ccts clm54217 clm5639 clm66411 clmIANAMIMEMediaType fn link n1 qdt udt xbrldi xbrli xdt xlink xs xsd xsi">
  <xsl:decimal-format name="european" decimal-separator="," grouping-separator="." NaN="" />
  <xsl:output version="4.0" method="html" indent="no" encoding="UTF-8" doctype-public="-//W3C//DTD HTML 4.01 Transitional//EN" doctype-system="http://www.w3.org/TR/html4/loose.dtd" />
  <xsl:param name="SV_OutputFormat" select="'HTML'" />
  <xsl:variable name="XML" select="/" />
  <xsl:variable name="senaryo" select="translate(//n1:Invoice/cbc:ProfileID,'abcçdefgğhıijklmnoöpqrsştuüvwxyz','ABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ')" />
  <xsl:variable name="hasAnyFalseChangeIndicator">
    <xsl:for-each select="//n1:Invoice/cac:InvoiceLine">
      <xsl:for-each select="(cac:AllowanceCharge[translate(cbc:ChargeIndicator,'abcçdefgğhıijklmnoöpqrsştuüvwxyz','ABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ') = 'TRUE']/cbc:Amount)[string(number())!='NaN']">
        <xsl:text>TRUE</xsl:text>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:variable>
  <xsl:variable name="Dil">
    <xsl:for-each select="//n1:Invoice/cbc:Note">
      <xsl:choose>
        <xsl:when test="contains(., 'Gönderici')">
          <xsl:text>Turkce</xsl:text>
        </xsl:when>
        <xsl:when test="contains(., 'Shipper')">
          <xsl:text>Ingilizce</xsl:text>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
  </xsl:variable>
  <xsl:variable name="FatTur">
    <xsl:for-each select="//n1:Invoice/cbc:Note">
      <xsl:choose>
        <xsl:when test="contains($Dil, 'Turkce') and (contains(.,'Çıkış Limanı'))">
          <xsl:text>DenizTurkce</xsl:text>
        </xsl:when>
        <xsl:when test="contains($Dil, 'Ingilizce') and (contains(.,'Port of origin'))">
          <xsl:text>DenizIngilizce</xsl:text>
        </xsl:when>
        <xsl:when test="contains($Dil, 'Turkce') and (contains(.,'Uçuş No/Tarih'))">
          <xsl:text>HavaTurkce</xsl:text>
        </xsl:when>
        <xsl:when test="contains($Dil, 'Ingilizce') and (contains(.,'Flight No/Date'))">
          <xsl:text>HavaIngilizce</xsl:text>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
  </xsl:variable>
  <xsl:template match="/">
    <html>
      <head>
        <title />
        <style type="text/css">
          body { background-color: #FFFFFF; font-family: 'Tahoma', "Times New Roman", Times, serif; font-size: 11px; color: #666666; -webkit-print-color-adjust: exact; } h1, h2 { padding-bottom: 3px; padding-top: 3px; margin-bottom: 5px; text-transform: uppercase; font-family: Arial, Helvetica, sans-serif; } h1 { font-size: 1.4em; text-transform:none; } h2 { font-size: 1em; color: brown; } h3 { font-size: 1em; color: #333333; text-align: justify; margin: 0; padding: 0; } h4 { font-size: 1.1em; font-style: bold; font-family: Arial, Helvetica, sans-serif; color: #000000; margin: 0; padding: 0; } hr { border: 2px solid #000000; } p, ul, ol { margin-top: 1.5em; } ul, ol { margin-left: 3em; } blockquote { margin-left: 3em; margin-right: 3em; font-style: italic; } a { text-decoration: none; color: #70A300; } a:hover { border: none; color: #70A300; } #budgetContainerTable { border-width: 0px; border-spacing: 0px; border-style: inset; border-color: black; border-collapse: collapse; margin-left:auto; } #despatchTable { border-collapse:collapse; float:right; border-color:black; max-width:270px; } #despatchDocumentReferenceTable{ border-collapse:collapse; } div.divContainer { width:800px; } #ettnTable { border-collapse:collapse; border-color:black; }
          #lineTable { border-width:2px; border-style: inset; border-color: black; border-collapse: collapse; }
          .lineTableTd { border-width: 1px; padding: 1px; border-style: inset; border-color: black; background-color: white; } #lineTableTr { border-width: 1px; padding: 0px; border-style: inset; border-color: black; background-color: white; } #lineTableBudgetTd { border-width: 2px; border-spacing:0px; padding: 1px; border-style: inset; border-color: black; background-color: white; } #notesTable { border-width: 2px; border-style: inset; border-color: black; border-collapse: collapse; } #notesTableTd { border-width: 0px; border-style: inset; border-color: black; border-collapse: collapse; } table { border-spacing:0px; } td { border-color:black; } .fixedTableCss{ table-layout:fixed; overflow-wrap:break-word; word-wrap:break-word; -ms-word-wrap:break-word; } #SGK_Table td{ max-width:400px; white-space: pre-wrap; white-space: -moz-pre-wrap; white-space: -pre-wrap; white-space: -o-pre-wrap; word-wrap: break-word; }
          #notlar2 { border-width:1px; border-style: inset; border-color: black; border-collapse: collapse; }
        </style>
        <title>e-Fatura</title>
      </head>
      <body  style="margin-left=0.6in; margin-right=0.6in; margin-top=0.79in; margin-bottom=0.79in">
        <xsl:for-each select="$XML">
          <table border="0" cellspacing="0px" width="800" cellpadding="0px">
            <tbody>
              <tr valign="top">
                <td width="40%" align="left">
                  <br />
                  <table id="supplierPartyTable" class="fixedTableCss" align="center" border="0" width="100%">
                    <tbody>
                      <hr />
                      <tr align="left">
                        <xsl:for-each select="//n1:Invoice/cac:AccountingSupplierParty/cac:Party">
                          <td align="left">
                            <xsl:if test="cac:PartyName/cbc:Name!=''">
                              <xsl:value-of select="cac:PartyName/cbc:Name" />
                              <br />
                            </xsl:if>
                            <xsl:for-each select="cac:Person">
                              <xsl:if test="cbc:Title!='' or cbc:FirstName!='' or cbc:MiddleName!='' or cbc:FamilyName!='' or cbc:NameSuffix!=''">
                                <xsl:for-each select="cbc:Title">
                                  <xsl:if test=". != ''">
                                    <xsl:apply-templates />
                                    <xsl:text> </xsl:text>
                                  </xsl:if>
                                </xsl:for-each>
                                <xsl:for-each select="cbc:FirstName">
                                  <xsl:if test=". != ''">
                                    <xsl:apply-templates />
                                    <xsl:text> </xsl:text>
                                  </xsl:if>
                                </xsl:for-each>
                                <xsl:for-each select="cbc:MiddleName">
                                  <xsl:if test=". != ''">
                                    <xsl:apply-templates />
                                    <xsl:text> </xsl:text>
                                  </xsl:if>
                                </xsl:for-each>
                                <xsl:for-each select="cbc:FamilyName">
                                  <xsl:if test=". != ''">
                                    <xsl:apply-templates />
                                    <xsl:text> </xsl:text>
                                  </xsl:if>
                                </xsl:for-each>
                                <xsl:for-each select="cbc:NameSuffix">
                                  <xsl:apply-templates />
                                </xsl:for-each>
                              </xsl:if>
                            </xsl:for-each>
                          </td>
                        </xsl:for-each>
                      </tr>
                      <tr align="left">
                        <xsl:for-each select="//n1:Invoice/cac:AccountingSupplierParty/cac:Party">
                          <td align="left">
                            <xsl:for-each select="cac:PostalAddress">
                              <xsl:for-each select="cbc:StreetName">
                                <xsl:if test=". != ''">
                                  <xsl:apply-templates />
                                  <xsl:text> </xsl:text>
                                </xsl:if>
                              </xsl:for-each>
                              <xsl:for-each select="cbc:BuildingName">
                                <xsl:if test=". != ''">
                                  <xsl:apply-templates />
                                  <xsl:text> </xsl:text>
                                </xsl:if>
                              </xsl:for-each>
                              <xsl:if test="cbc:BuildingNumber != ''">
                                <xsl:text>No:</xsl:text>
                                <xsl:for-each select="cbc:BuildingNumber">
                                  <xsl:apply-templates />
                                </xsl:for-each>
                                <xsl:text> </xsl:text>
                              </xsl:if>
                              <xsl:if test="cbc:StreetName !='' or cbc:BuildingName !='' or cbc:BuildingNumber !=''">
                                <br />
                              </xsl:if>
                              <xsl:for-each select="cbc:PostalZone">
                                <xsl:if test=". != ''">
                                  <xsl:apply-templates />
                                  <xsl:text> </xsl:text>
                                </xsl:if>
                              </xsl:for-each>
                              <xsl:for-each select="cbc:District">
                                <xsl:apply-templates />
                                <span>
                                  <xsl:text> </xsl:text>
                                </span>
                              </xsl:for-each>
                              <xsl:for-each select="cbc:CitySubdivisionName">
                                <xsl:apply-templates />
                              </xsl:for-each>
                              <xsl:if test="cbc:CitySubdivisionName!='' and cbc:CityName!=''">
                                <xsl:text>/ </xsl:text>
                              </xsl:if>
                              <xsl:for-each select="cbc:CityName">
                                <xsl:if test=". != ''">
                                  <xsl:apply-templates />
                                  <xsl:text> </xsl:text>
                                </xsl:if>
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </xsl:for-each>
                      </tr>
                      <tr align="left">
                        <xsl:for-each select="//n1:Invoice/cac:AccountingSupplierParty/cac:Party">
                          <xsl:if test="cac:Contact/cbc:Telephone !='' or cac:Contact/cbc:Telefax !=''">
                            <td align="left">
                              <xsl:for-each select="cac:Contact">
                                <xsl:if test="cbc:Telephone != ''">
                                  <xsl:text>Tel: </xsl:text>
                                  <xsl:for-each select="cbc:Telephone">
                                    <xsl:apply-templates />
                                  </xsl:for-each>
                                </xsl:if>
                                <xsl:if test="cbc:Telefax != ''">
                                  <xsl:text> Fax: </xsl:text>
                                  <xsl:for-each select="cbc:Telefax">
                                    <xsl:apply-templates />
                                  </xsl:for-each>
                                </xsl:if>
                                <xsl:text> </xsl:text>
                              </xsl:for-each>
                            </td>
                          </xsl:if>
                        </xsl:for-each>
                      </tr>
                      <xsl:for-each select="//n1:Invoice/cac:AccountingSupplierParty/cac:Party/cbc:WebsiteURI">
                        <xsl:if test=". !=''">
                          <tr align="left">
                            <td>
                              <xsl:text>Web Sitesi: </xsl:text>
                              <xsl:value-of select="." />
                            </td>
                          </tr>
                        </xsl:if>
                      </xsl:for-each>
                      <xsl:for-each select="//n1:Invoice/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail">
                        <xsl:if test=". !=''">
                          <tr align="left">
                            <td>
                              <xsl:text>E-Posta: </xsl:text>
                              <xsl:value-of select="." />
                            </td>
                          </tr>
                        </xsl:if>
                      </xsl:for-each>
                      <tr align="left">
                        <xsl:for-each select="//n1:Invoice/cac:AccountingSupplierParty/cac:Party">
                          <xsl:if test="cac:PartyTaxScheme/cac:TaxScheme/cbc:Name !=''">
                            <td align="left">
                              <xsl:text>Vergi Dairesi: </xsl:text>
                              <xsl:for-each select="cac:PartyTaxScheme">
                                <xsl:for-each select="cac:TaxScheme">
                                  <xsl:for-each select="cbc:Name">
                                    <xsl:apply-templates />
                                  </xsl:for-each>
                                </xsl:for-each>
                                <xsl:text>  </xsl:text>
                              </xsl:for-each>
                            </td>
                          </xsl:if>
                        </xsl:for-each>
                      </tr>
                      <xsl:for-each select="//n1:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification">
                        <xsl:if test="cbc:ID !=''">
                          <tr align="left">
                            <td>
                              <xsl:value-of select="cbc:ID/@schemeID" />
                              <xsl:text>: </xsl:text>
                              <xsl:value-of select="cbc:ID" />
                            </td>
                          </tr>
                        </xsl:if>
                      </xsl:for-each>
                    </tbody>
                  </table>
                  <hr />
                </td>
                <td width="20%" align="center" valign="middle">
                  <br />
                  <br />
                  <img style="width:91px;" align="middle" alt="E-Fatura Logo" src="data:image/jpeg;base64,/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAABkAAD/4QMZaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjYtYzEzMiA3OS4xNTkyODQsIDIwMTYvMDQvMTktMTM6MTM6NDAgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdFJlZj0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlUmVmIyIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOjZDNDJBNEI2QjVCRDExRThCQjM0REIwQkZGMEQxODY0IiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjZDNDJBNEI1QjVCRDExRThCQjM0REIwQkZGMEQxODY0IiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzQgV2luZG93cyI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSIzREVENkU1N0FDREVDNEJBNzkxNUM2M0NCN0RENzM0NyIgc3RSZWY6ZG9jdW1lbnRJRD0iM0RFRDZFNTdBQ0RFQzRCQTc5MTVDNjNDQjdERDczNDciLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7/7gAOQWRvYmUAZMAAAAAB/9sAhAABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB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" />
                  <h1 align="center">
                    <span style="font-weight:bold; ">
                      <xsl:text>e-FATURA</xsl:text>
                    </span>
                    <span style="color:black; font-size:11px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text> e-INVOICE</xsl:text>
                    </span>
                  </h1>
                </td>

                <td width="40%" align="right" valign="middle">
                  <img style="width:270px;" align="middle" alt="Firma Logo"
                        src="data:image/jpeg;base64,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"/>
                </td>
              </tr>
              <tr style="height:118px; " valign="top">
                <td width="40%" align="left" valign="bottom">
                  <table id="customerPartyTable" class="fixedTableCss" align="center" border="0" height="50%" width="100%">
                    <tbody>
                      <hr />
                      <tr>
                        <xsl:for-each select="//n1:Invoice/cac:AccountingCustomerParty/cac:Party">
                          <td align="left">
                            <b>SAYIN</b>
                            <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                              <xsl:text> / DEAR</xsl:text>
                            </span>
                          </td>
                        </xsl:for-each>
                      </tr>
                      <tr>
                        <xsl:choose>
                          <xsl:when test="//n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID='PARTYTYPE' and text()='TAXFREE']">
                            <xsl:for-each select="//n1:Invoice/cac:BuyerCustomerParty/cac:Party">
                              <xsl:call-template name="Party_Title">
                                <xsl:with-param name="PartyType">TAXFREE</xsl:with-param>
                              </xsl:call-template>
                            </xsl:for-each>
                          </xsl:when>
                          <xsl:when test="//n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID='PARTYTYPE' and text()='EXPORT']">
                            <xsl:for-each select="//n1:Invoice/cac:BuyerCustomerParty/cac:Party">
                              <xsl:call-template name="Party_Title">
                                <xsl:with-param name="PartyType">EXPORT</xsl:with-param>
                              </xsl:call-template>
                            </xsl:for-each>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:for-each select="//n1:Invoice/cac:AccountingCustomerParty/cac:Party">
                              <xsl:call-template name="Party_Title">
                                <xsl:with-param name="PartyType">OTHER</xsl:with-param>
                              </xsl:call-template>
                            </xsl:for-each>
                          </xsl:otherwise>
                        </xsl:choose>
                      </tr>
                      <xsl:choose>
                        <xsl:when test="//n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID='PARTYTYPE' and text()='TAXFREE']">
                          <xsl:for-each select="//n1:Invoice/cac:BuyerCustomerParty/cac:Party">
                            <tr>
                              <xsl:call-template name="Party_Adress">
                                <xsl:with-param name="PartyType">TAXFREE</xsl:with-param>
                              </xsl:call-template>
                            </tr>
                            <xsl:call-template name="Party_Other">
                              <xsl:with-param name="PartyType">TAXFREE</xsl:with-param>
                            </xsl:call-template>
                          </xsl:for-each>
                        </xsl:when>
                        <xsl:when test="//n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID='PARTYTYPE' and text()='EXPORT']">
                          <xsl:for-each select="n1:Invoice/cac:BuyerCustomerParty/cac:Party">
                            <tr>
                              <xsl:call-template name="Party_Adress">
                                <xsl:with-param name="PartyType">EXPORT</xsl:with-param>
                              </xsl:call-template>
                            </tr>
                            <xsl:call-template name="Party_Other">
                              <xsl:with-param name="PartyType">EXPORT</xsl:with-param>
                            </xsl:call-template>
                          </xsl:for-each>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:for-each select="//n1:Invoice/cac:AccountingCustomerParty/cac:Party">
                            <tr>
                              <xsl:call-template name="Party_Adress">
                                <xsl:with-param name="PartyType">OTHER</xsl:with-param>
                              </xsl:call-template>
                            </tr>
                            <xsl:call-template name="Party_Other">
                              <xsl:with-param name="PartyType">OTHER</xsl:with-param>
                            </xsl:call-template>
                          </xsl:for-each>
                        </xsl:otherwise>
                      </xsl:choose>
                    </tbody>
                  </table>
                  <hr />
                </td>
                <td width="20%" align="middle" valign="top">
                  <img style="width:130px; padding-left:20px;" align="middle" alt="Firma İmza"
                       src="data:image/jpeg;base64,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" />
                </td>

                <td width="40%" align="right" valign="bottom">
                  <table id="despatchTable" class="fixedTableCss" border="1" height="13" width="300px">
                    <tbody>
                      <tr>
                        <td style="width:130px;" align="left">
                          <span style="font-weight:bold; ">
                            <xsl:text>Özelleştirme No:</xsl:text>
                          </span>
                          <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                            <br/>
                            <xsl:text>(Customization Id)</xsl:text>
                          </span>
                        </td>
                        <td style="width:135px;" align="left">
                          <xsl:for-each select="//n1:Invoice">
                            <xsl:for-each select="cbc:CustomizationID">
                              <xsl:apply-templates />
                            </xsl:for-each>
                          </xsl:for-each>
                        </td>
                      </tr>
                      <tr style="height:13px;">
                        <td align="left">
                          <span style="font-weight:bold;">
                            <xsl:text>Senaryo:</xsl:text>
                          </span>
                          <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                            <br/>
                            <xsl:text>(Profile)</xsl:text>
                          </span>
                        </td>
                        <td align="left">
                          <xsl:for-each select="//n1:Invoice">
                            <xsl:for-each select="cbc:ProfileID">
                              <xsl:apply-templates />
                            </xsl:for-each>
                          </xsl:for-each>
                        </td>
                      </tr>
                      <tr style="height:13px;">
                        <td align="left">
                          <span style="font-weight:bold;">
                            <xsl:text>Fatura Tipi:</xsl:text>
                          </span>
                          <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                            <br/>
                            <xsl:text>(Invoice Type)</xsl:text>
                          </span>
                        </td>
                        <td align="left">
                          <xsl:for-each select="//n1:Invoice">
                            <xsl:for-each select="cbc:InvoiceTypeCode">
                              <xsl:apply-templates />
                            </xsl:for-each>
                          </xsl:for-each>
                        </td>
                      </tr>
                      <tr style="height:13px;">
                        <td align="left">
                          <span style="font-weight:bold;">
                            <xsl:text>Fatura No:</xsl:text>
                          </span>
                          <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                            <br/>
                            <xsl:text>(Invoice No)</xsl:text>
                          </span>
                        </td>
                        <td align="left">
                          <xsl:for-each select="//n1:Invoice">
                            <xsl:for-each select="cbc:ID">
                              <xsl:apply-templates />
                            </xsl:for-each>
                          </xsl:for-each>
                        </td>
                      </tr>
                      <tr style="height:13px;">
                        <td align="left">
                          <span style="font-weight:bold;">
                            <xsl:text>Fatura Tarihi:</xsl:text>
                          </span>
                          <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                            <br/>
                            <xsl:text>(Invoice Date)</xsl:text>
                          </span>
                        </td>
                        <td align="left">
                          <xsl:for-each select="//n1:Invoice">
                            <xsl:for-each select="cbc:IssueDate">
                              <xsl:value-of select="substring(.,9,2)" />-<xsl:value-of select="substring(.,6,2)" />-<xsl:value-of select="substring(.,1,4)" />
                            </xsl:for-each>
                          </xsl:for-each>
                        </td>
                      </tr>
                      <xsl:if test="//n1:Invoice/cbc:IssueDate !=''">
                        <tr style="height:13px; ">
                          <td align="left">
                            <span style="font-weight:bold;">
                              <xsl:text>Düzenleme Tarih/Saat:</xsl:text>
                            </span>
                            <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                              <br/>
                              <xsl:text>(Issuing date/time)</xsl:text>
                            </span>
                          </td>
                          <td align="left">
                            <xsl:for-each select="//n1:Invoice">
                              <xsl:for-each select="cbc:IssueDate">
                                <xsl:value-of select="substring(.,9,2)" />-<xsl:value-of select="substring(.,6,2)" />-<xsl:value-of select="substring(.,1,4)" />
                              </xsl:for-each>
                            </xsl:for-each>
                            <xsl:text> / </xsl:text>
                            <xsl:for-each select="n1:Invoice">
                              <xsl:for-each select="cbc:IssueTime">
                                <xsl:value-of select="substring(.,0,6)" />
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>
                      </xsl:if>
                      <xsl:if test="count(//n1:Invoice/cac:DespatchDocumentReference) = 1">
                        <xsl:if test="//n1:Invoice/cac:DespatchDocumentReference/cbc:ID !=''">
                          <tr style="height:13px; ">
                            <td align="left">
                              <span style="font-weight:bold;">
                                <xsl:text>İrsaliye No:</xsl:text>
                              </span>
                              <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                                <br/>
                                <xsl:text>(Despatch No)</xsl:text>
                              </span>
                            </td>
                            <td align="left">
                              <xsl:value-of select="//n1:Invoice/cac:DespatchDocumentReference/cbc:ID" />
                            </td>
                          </tr>
                        </xsl:if>
                        <xsl:if test="//n1:Invoice/cac:DespatchDocumentReference/cbc:IssueDate !=''">
                          <tr style="height:13px; ">
                            <td align="left">
                              <span style="font-weight:bold;">
                                <xsl:text>İrsaliye Tarihi:</xsl:text>
                              </span>
                              <xsl:if test="$senaryo = 'IHRACAT' or $senaryo = 'İHRACAT'">
                                <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                                  <br/>
                                  <xsl:text>(Despatch Date)</xsl:text>
                                </span>
                              </xsl:if>
                            </td>
                            <td align="left">
                              <xsl:for-each select="//n1:Invoice/cac:DespatchDocumentReference/cbc:IssueDate">
                                <xsl:value-of select="substring(.,9,2)" />-<xsl:value-of select="substring(.,6,2)" />-<xsl:value-of select="substring(.,1,4)" />
                              </xsl:for-each>
                            </td>
                          </tr>
                        </xsl:if>
                      </xsl:if>
                      <xsl:if test="//n1:Invoice/cac:OrderReference/cbc:ID !=''">
                        <tr style="height:13px">
                          <td align="left">
                            <span style="font-weight:bold;">
                              <xsl:text>Sipariş No:</xsl:text>
                            </span>
                            <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                              <br/>
                              <xsl:text>(Order Id)</xsl:text>
                            </span>
                          </td>
                          <td align="left">
                            <xsl:for-each select="//n1:Invoice/cac:OrderReference">
                              <xsl:for-each select="cbc:ID">
                                <xsl:apply-templates />
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>
                      </xsl:if>
                      <xsl:if test="//n1:Invoice/cac:OrderReference/cbc:IssueDate !=''">
                        <tr style="height:13px">
                          <td align="left">
                            <span style="font-weight:bold;">
                              <xsl:text>Sipariş Tarihi:</xsl:text>
                            </span>
                            <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                              <br/>
                              <xsl:text>(Order Date)</xsl:text>
                            </span>
                          </td>
                          <td align="left">
                            <xsl:for-each select="//n1:Invoice/cac:OrderReference">
                              <xsl:for-each select="cbc:IssueDate">
                                <xsl:value-of select="substring(.,9,2)" />-<xsl:value-of select="substring(.,6,2)" />-<xsl:value-of select="substring(.,1,4)" />
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>
                      </xsl:if>
                      <xsl:if test="not(//n1:Invoice/cbc:DocumentCurrencyCode='TRY' or //n1:Invoice/cbc:DocumentCurrencyCode='TRL')">
                        <tr style="height:13px; ">
                          <td align="left">
                            <span style="font-weight:bold;">
                              <xsl:text>Döviz Kuru:</xsl:text>
                            </span>
                            <span style="color:black; font-size:9px; font-weight:bold;" align="middle">
                              <br/>
                              <xsl:text>(Exchange Rate)</xsl:text>
                            </span>
                          </td>
                          <td align="left">
                            <xsl:value-of select="//n1:Invoice/cac:PricingExchangeRate/cbc:CalculationRate" />
                            <xsl:text> TL</xsl:text>
                          </td>
                        </tr>
                      </xsl:if>
                      <xsl:if test="$senaryo = 'YOLCUBERABERFATURA' or $senaryo = 'YOLCUBERABER'">
                        <xsl:for-each select="//n1:Invoice/cac:TaxRepresentativeParty/cac:PartyIdentification/cbc:ID[@schemeID='ARACIKURUMVKN']">
                          <xsl:if test=".!=''">
                            <tr style="height:13px;">
                              <td align="left">
                                <span style="font-weight:bold;">
                                  <xsl:text>Aracı Kurum VKN:</xsl:text>
                                </span>
                              </td>
                              <td align="left">
                                <xsl:value-of select="." />
                              </td>
                            </tr>
                          </xsl:if>
                          <xsl:if test="../../cac:PartyName/cbc:Name != ''">
                            <tr style="height:13px;">
                              <td align="left">
                                <span style="font-weight:bold;">
                                  <xsl:text>Aracı Kurum Unvan:</xsl:text>
                                </span>
                              </td>
                              <td align="left">
                                <xsl:value-of select="../../cac:PartyName/cbc:Name" />
                              </td>
                            </tr>
                          </xsl:if>
                        </xsl:for-each>
                      </xsl:if>
                    </tbody>
                  </table>
                </td>
              </tr>
              <tr align="left">
                <td align="left" colspan="3">
                  <table id="ettnTable" class="fixedTableCss">
                    <tr style="height:13px;">
                      <td align="left" valign="top" width="35px">
                        <span style="font-weight:bold; ">
                          <xsl:text>ETTN:</xsl:text>
                        </span>
                      </td>
                      <td align="left" width="240px">
                        <xsl:for-each select="//n1:Invoice">
                          <xsl:for-each select="cbc:UUID">
                            <xsl:apply-templates />
                          </xsl:for-each>
                        </xsl:for-each>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
              <tr>
                <td>
                  <xsl:if test="//n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID='VKN']='7750409379'">
                    <table id="SGK_Table" border="1" width="100%">
                      <tbody>
                        <tr>
                          <td style="width:40%;" align="left">
                            <span style="font-weight:bold; ">
                              <xsl:text>Fatura Tipi:</xsl:text>
                            </span>
                          </td>
                          <td style="width:60%;" align="left">
                            <xsl:for-each select="n1:Invoice">
                              <xsl:for-each select="cbc:AccountingCost">
                                <xsl:apply-templates />
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>
                        <tr style="height:13px; ">
                          <td style="width:40%;" align="left">
                            <span style="font-weight:bold; ">
                              <xsl:text>Mükellef Kodu:</xsl:text>
                            </span>
                          </td>
                          <td style="width:60%;" align="left">
                            <xsl:for-each select="n1:Invoice">
                              <xsl:for-each select="cac:AdditionalDocumentReference[cbc:DocumentTypeCode='MUKELLEF_KODU']/cbc:DocumentType">
                                <xsl:apply-templates />
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>
                        <tr style="height:13px; ">
                          <td style="width:40%;" align="left">
                            <span style="font-weight:bold; ">
                              <xsl:text>Mükellef Adı:</xsl:text>
                            </span>
                          </td>
                          <td style="width:60%;" align="left">
                            <xsl:for-each select="n1:Invoice">
                              <xsl:for-each select="cac:AdditionalDocumentReference[cbc:DocumentTypeCode='MUKELLEF_ADI']/cbc:DocumentType">
                                <xsl:apply-templates />
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>
                        <tr style="height:13px; ">
                          <td style="width:40%;" align="left">
                            <span style="font-weight:bold; ">
                              <xsl:text>Dosya No:</xsl:text>
                            </span>
                          </td>
                          <td style="width:60%;" align="left">
                            <xsl:for-each select="n1:Invoice">
                              <xsl:for-each select="cac:AdditionalDocumentReference[cbc:DocumentTypeCode='DOSYA_NO']/cbc:DocumentType">
                                <xsl:apply-templates />
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>
                        <tr style="height:13px; ">
                          <td style="width:40%;" align="left">
                            <span style="font-weight:bold; ">
                              <xsl:text>Dönem:</xsl:text>
                            </span>
                          </td>
                          <td style="width:60%;" align="left">
                            <xsl:for-each select="n1:Invoice">
                              <xsl:for-each select="cac:InvoicePeriod/cbc:StartDate">
                                <xsl:apply-templates />
                              </xsl:for-each>
                              <span>
                                <xsl:text> / </xsl:text>
                              </span>
                              <xsl:for-each select="cac:InvoicePeriod/cbc:EndDate">
                                <xsl:apply-templates />
                              </xsl:for-each>
                            </xsl:for-each>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </xsl:if>
                </td>
              </tr>
            </tbody>
          </table>
          <div id="lineTableAligner">
            <span>
              <xsl:text> </xsl:text>
            </span>
          </div>
          <table border="1" id="lineTable" class="fixedTableCss" width="800">
            <tbody>
              <tr id="lineTableTr">
                <td class="lineTableTd" style="width:4%;" align="center">
                  <span style="font-weight:bold;">
                    <xsl:text>Sıra No</xsl:text>
                  </span>
                  <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                    <br/>
                    <xsl:text>(Seq. No)</xsl:text>
                  </span>
                </td>
                <td id="malHizmet" class="lineTableTd" style="width:35%" align="center">
                  <span style="font-weight:bold;">
                    <xsl:text>Mal Hizmet</xsl:text>
                  </span>
                  <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                    <br/>
                    <xsl:text>(Description)</xsl:text>
                  </span>
                </td>
                <td id="miktar" class="lineTableTd" style="width:8%" align="center">
                  <span style="font-weight:bold;">
                    <xsl:text>Miktar</xsl:text>
                  </span>
                  <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                    <br/>
                    <xsl:text>(Quantity)</xsl:text>
                  </span>
                </td>
                <td id="birimFiyat" class="lineTableTd" style="width:10%" align="center">
                  <span style="font-weight:bold; ">
                    <xsl:text>Birim Fiyat</xsl:text>
                  </span>
                  <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                    <br/>
                    <xsl:text>(Unit Price)</xsl:text>
                  </span>
                </td>
                <xsl:if test="not($senaryo = 'IHRACAT' or $senaryo = 'İHRACAT' )">
                  <td class="lineTableTd" style="width:5%" align="center">
                    <span style="font-weight:bold;">
                      <xsl:text>KDV Oranı</xsl:text>
                    </span>
                    <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(VAT Rate)</xsl:text>
                    </span>
                  </td>
                  <td class="lineTableTd" style="width:9%" align="center">
                    <span style="font-weight:bold;">
                      <xsl:text>KDV Tutarı</xsl:text>
                    </span>
                    <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(VAT Amount)</xsl:text>
                    </span>
                  </td>
                </xsl:if>
                <td id="malHizmetTutari" class="lineTableTd" style="width:10%" align="center">
                  <span style="font-weight:bold;">
                    <xsl:text>Mal Hizmet Tutarı</xsl:text>
                  </span>
                  <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                    <br/>
                    <xsl:text>(Amount)</xsl:text>
                  </span>
                </td>
                <xsl:if test="$senaryo = 'IHRACAT' or $senaryo = 'İHRACAT'">
                  <td class="lineTableTd" style="width:6%" align="center">
                    <span style="font-weight:bold;">
                      <xsl:text>Teslim Şartı</xsl:text>
                    </span>
                    <span style="color:black; font-size:6px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(Delivery Conditions)</xsl:text>
                    </span>
                  </td>
                  <td class="lineTableTd" style="width:7%" align="center">
                    <span style="font-weight:bold;">
                      <xsl:text>Eşya Kap Cinsi</xsl:text>
                    </span>
                    <span style="color:black; font-size:7px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(Container Type)</xsl:text>
                    </span>
                  </td>
                  <td class="lineTableTd" style="width:6%" align="center">
                    <span style="font-weight:bold;">
                      <xsl:text>Kap No</xsl:text>
                    </span>
                    <span style="color:black; font-size:7px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(Container ID)</xsl:text>
                    </span>
                  </td>
                  <td class="lineTableTd" style="width:6%" align="center">
                    <span style="font-weight:bold;">
                      <xsl:text>Kap Adet</xsl:text>
                    </span>
                    <span style="color:black; font-size:7px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(Container Unit)</xsl:text>
                    </span>
                  </td>
                  <td class="lineTableTd" style="width:7%" align="center">
                    <span style="font-weight:bold;">
                      <xsl:text>Teslim/ Bedel Ödeme Yeri</xsl:text>
                    </span>
                    <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(Delivery Price/Payment Place)</xsl:text>
                    </span>
                  </td>
                  <td class="lineTableTd" style="width:9%" align="center">
                    <span style="font-weight:bold;">
                      <xsl:text>Gönderilme Şekli</xsl:text>
                    </span>
                    <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(How to Send)</xsl:text>
                    </span>
                  </td>
                  <td class="lineTableTd" style="width:6%" align="center">
                    <span style="font-weight:bold;">
                      <xsl:text>GTİP</xsl:text>
                    </span>
                  </td>
                </xsl:if>
              </tr>
              <xsl:for-each select="//n1:Invoice/cac:InvoiceLine">
                <xsl:choose>
                  <xsl:when test=".">
                    <xsl:apply-templates select="." />
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:apply-templates select="//n1:Invoice" />
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:for-each>
            </tbody>
          </table>
        </xsl:for-each>
        <br />
        <div class="divContainer">
          <table id="budgetContainerTable" class="fixedTableCss" width="295px">
            <tbody>
              <xsl:if test="not($senaryo = 'IHRACAT' or $senaryo = 'İHRACAT')">
                <tr id="budgetContainerTr" align="right">
                  <td id="lineTableBudgetTd" align="right" width="200px" >
                    <span style="font-weight:bold; ">
                      <xsl:text>Mal Hizmet Toplam Tutarı</xsl:text>
                    </span>
                    <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(Total Amount)</xsl:text>
                    </span>
                  </td>
                  <td id="lineTableBudgetTd" align="right" width="85px">
                    <span>
                      <xsl:for-each select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount">
                        <xsl:call-template name="Curr_Type">
                          <xsl:with-param name="valuePath" select="." />
                          <xsl:with-param name="format" select="'###.##0,00'" />
                        </xsl:call-template>
                      </xsl:for-each>
                    </span>
                  </td>
                </tr>
              </xsl:if>
              <xsl:if test="$senaryo = 'IHRACAT' or $senaryo = 'İHRACAT'">
                <tr id="budgetContainerTr" align="right">
                  <td id="lineTableBudgetTd" align="right" width="200px" >
                    <span style="font-weight:bold; ">
                      <xsl:text>Mal Hizmet Toplam Tutarı</xsl:text>
                    </span>
                    <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(Total Amount)</xsl:text>
                    </span>
                  </td>
                  <td id="lineTableBudgetTd" align="right" width="85px">
                    <span>
                      <xsl:value-of select="format-number((//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount ), '###.##0,00', 'european')" />
                      <xsl:text> </xsl:text>
                      <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID ='TRY' or //n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID = 'TRL'">
                        <xsl:text>TL</xsl:text>
                      </xsl:if>
                      <xsl:if test="not(//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID ='TRY' or //n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID = 'TRL')">
                        <xsl:value-of select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID" />
                      </xsl:if>
                    </span>
                  </td>
                </tr>
                <xsl:if test="//n1:Invoice/cac:Delivery/cac:Shipment/cbc:FreeOnBoardValueAmount !='' and number(//n1:Invoice/cac:Delivery/cac:Shipment/cbc:FreeOnBoardValueAmount)  &gt; 0 ">
                  <tr id="budgetContainerTr" align="right">
                    <td id="lineTableBudgetTd" align="right" width="200px" >
                      <span style="font-weight:bold; ">
                        <xsl:text>FOB Tutar</xsl:text>
                      </span>
                    </td>
                    <td id="lineTableBudgetTd" align="right">
                      <span>
                        <xsl:for-each select="//n1:Invoice/cac:Delivery/cac:Shipment/cbc:FreeOnBoardValueAmount">
                          <xsl:call-template name="Curr_Type">
                            <xsl:with-param name="valuePath" select="." />
                            <xsl:with-param name="format" select="'###.##0,00'" />
                          </xsl:call-template>
                        </xsl:for-each>
                      </span>
                    </td>
                  </tr>
                </xsl:if>
                <xsl:if test="//n1:Invoice/cac:Delivery/cac:Shipment/cbc:DeclablackForCarriageValueAmount !='' and number(//n1:Invoice/cac:Delivery/cac:Shipment/cbc:DeclablackForCarriageValueAmount)  &gt; 0 ">
                  <tr id="budgetContainerTr" align="right">
                    <td id="lineTableBudgetTd" align="right" width="200px" >
                      <span style="font-weight:bold; ">
                        <xsl:text>Nakliye Bedeli</xsl:text>
                      </span>
                    </td>
                    <td id="lineTableBudgetTd" align="right">
                      <span>
                        <xsl:for-each select="//n1:Invoice/cac:Delivery/cac:Shipment/cbc:DeclablackForCarriageValueAmount">
                          <xsl:call-template name="Curr_Type">
                            <xsl:with-param name="valuePath" select="." />
                            <xsl:with-param name="format" select="'###.##0,00'" />
                          </xsl:call-template>
                        </xsl:for-each>
                      </span>
                    </td>
                  </tr>
                </xsl:if>
                <xsl:if test="//n1:Invoice/cac:Delivery/cac:Shipment/cbc:InsuranceValueAmount !='' and number(//n1:Invoice/cac:Delivery/cac:Shipment/cbc:InsuranceValueAmount)  &gt; 0 ">
                  <tr id="budgetContainerTr" align="right">
                    <td id="lineTableBudgetTd" align="right" width="200px" >
                      <span style="font-weight:bold; ">
                        <xsl:text>Sigorta Bedeli</xsl:text>
                      </span>
                    </td>
                    <td id="lineTableBudgetTd" align="right">
                      <span>
                        <xsl:for-each select="//n1:Invoice/cac:Delivery/cac:Shipment/cbc:InsuranceValueAmount">
                          <xsl:call-template name="Curr_Type">
                            <xsl:with-param name="valuePath" select="." />
                            <xsl:with-param name="format" select="'###.##0,00'" />
                          </xsl:call-template>
                        </xsl:for-each>
                      </span>
                    </td>
                  </tr>
                </xsl:if>
              </xsl:if>
              <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount !='' and number(//n1:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount) &gt; 0">
                <tr id="budgetContainerTr" align="right">
                  <td id="lineTableBudgetTd" align="right" >
                    <span style="font-weight:bold; ">
                      <xsl:text>Toplam Artırım</xsl:text>
                    </span>
                  </td>
                  <td id="lineTableBudgetTd" align="right">
                    <span>
                      <xsl:for-each select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount">
                        <xsl:call-template name="Curr_Type">
                          <xsl:with-param name="valuePath" select="." />
                          <xsl:with-param name="format" select="'###.##0,00'" />
                        </xsl:call-template>
                      </xsl:for-each>
                    </span>
                  </td>
                </tr>
              </xsl:if>
              <xsl:for-each select="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal">
                <tr id="budgetContainerTr" align="right">
                  <td id="lineTableBudgetTd" align="right" >
                    <span style="font-weight:bold; ">
                      <xsl:text>Hesaplanan </xsl:text>
                      <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name" />
                      <xsl:text>(%</xsl:text>
                      <xsl:value-of select="cbc:Percent" />
                      <xsl:text>)</xsl:text>
                    </span>
                    <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                      <br/>
                      <xsl:text>(VAT)</xsl:text>
                    </span>
                  </td>
                  <td id="lineTableBudgetTd" align="right">
                    <xsl:for-each select="cac:TaxCategory/cac:TaxScheme">
                      <xsl:text> </xsl:text>
                      <xsl:call-template name="Curr_Type">
                        <xsl:with-param name="valuePath" select="../../cbc:TaxAmount" />
                        <xsl:with-param name="format" select="'###.##0,00'" />
                      </xsl:call-template>
                    </xsl:for-each>
                  </td>
                </tr>
              </xsl:for-each>
              <xsl:for-each select="//n1:Invoice/cac:WithholdingTaxTotal/cac:TaxSubtotal">
                <xsl:if test="cbc:TaxAmount != ''">
                  <tr id="budgetContainerTr" align="right">
                    <td id="lineTableBudgetTd" align="right" >
                      <span style="font-weight:bold; ">
                        <xsl:text>KDV Tevkifat-[</xsl:text>
                        <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode" />
                        <xsl:text>]-</xsl:text>
                        <xsl:text>(%</xsl:text>
                        <xsl:value-of select="cbc:Percent" />
                        <xsl:text>)</xsl:text>
                      </span>
                    </td>
                    <td id="lineTableBudgetTd" align="right">
                      <xsl:for-each select="cac:TaxCategory/cac:TaxScheme">
                        <xsl:text> </xsl:text>
                        <xsl:call-template name="Curr_Type">
                          <xsl:with-param name="valuePath" select="../../cbc:TaxAmount" />
                          <xsl:with-param name="format" select="'###.##0,00'" />
                        </xsl:call-template>
                      </xsl:for-each>
                    </td>
                  </tr>
                </xsl:if>
              </xsl:for-each>
              <tr id="budgetContainerTr" align="right">
                <td id="lineTableBudgetTd" align="right" >
                  <span style="font-weight:bold; ">
                    <xsl:text>Vergiler Dahil Toplam Tutar</xsl:text>
                  </span>
                  <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                    <br/>
                    <xsl:text>(Grand Total Included Taxes)</xsl:text>
                  </span>
                </td>
                <td id="lineTableBudgetTd" align="right">
                  <xsl:for-each select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount">
                    <xsl:call-template name="Curr_Type">
                      <xsl:with-param name="valuePath" select="." />
                      <xsl:with-param name="format" select="'###.##0,00'" />
                    </xsl:call-template>
                  </xsl:for-each>
                </td>
              </tr>
              <tr id="budgetContainerTr" align="right">
                <td id="lineTableBudgetTd" align="right" >
                  <span style="font-weight:bold; ">
                    <xsl:text>Ödenecek Tutar</xsl:text>
                  </span>
                  <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                    <br/>
                    <xsl:text>(Amount Payable)</xsl:text>
                  </span>
                </td>
                <td id="lineTableBudgetTd" align="right">
                  <xsl:for-each select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount">
                    <xsl:call-template name="Curr_Type">
                      <xsl:with-param name="valuePath" select="." />
                      <xsl:with-param name="format" select="'###.##0,00'" />
                    </xsl:call-template>
                  </xsl:for-each>
                </td>
              </tr>
              <xsl:if test="not(//n1:Invoice/cbc:DocumentCurrencyCode = 'TRY' or //n1:Invoice/cbc:DocumentCurrencyCode = 'TRL')">
                <xsl:if test="not(//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID ='TRY' or //n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID ='TRL')">
                  <tr id="budgetContainerTr" align="right">
                    <td id="lineTableBudgetTd" align="right" >
                      <span style="font-weight:bold; ">
                        <xsl:text>Vergiler Dahil Toplam Tutar(TL)</xsl:text>
                      </span>
                      <span style="color:black; font-size:8px; font-weight:bold;" align="middle">
                        <br/>
                        <xsl:text>(Grand Total Included Taxes) TL</xsl:text>
                      </span>
                    </td>
                    <td id="lineTableBudgetTd" align="right">
                      <xsl:value-of select="format-number(//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount * //n1:Invoice/cac:PricingExchangeRate/cbc:CalculationRate, '###.##0,00', 'european')" />
                      <xsl:text> TL</xsl:text>
                    </td>
                  </tr>
                </xsl:if>
                <xsl:if test="not(//n1:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID ='TRY' or //n1:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID ='TRL')">
                  <tr align="right">
                    <td id="lineTableBudgetTd" align="middle" colspan="2">
                      <span style="font-weight:bold; ">
                        <xsl:text>İş bu Fatura </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount">
                          <xsl:call-template name="Curr_Type">
                            <xsl:with-param name="valuePath" select="." />
                            <xsl:with-param name="format" select="'###.##0,00'" />
                          </xsl:call-template>
                        </xsl:for-each>
                        <xsl:text>  ödenmelidir.</xsl:text>
                      </span>
                    </td>
                  </tr>
                </xsl:if>
              </xsl:if>
            </tbody>
          </table>
        </div>
        <br />
        <xsl:if test="//n1:Invoice/cac:BillingReference/cac:InvoiceDocumentReference/cbc:DocumentTypeCode[translate(text(),'abcçdefgğhıijklmnoöpqrsştuüvwxyz','ABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ')='İADE' or translate(text(),'abcçdefgğhıijklmnoöpqrsştuüvwxyz','ABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ')='IADE']">
          <table id="lineTable" class="fixedTableCss" width="800">
            <thead>
              <tr id="lineTableTr">
                <td class="lineTableTd" align="center" colspan="2">
                  <span style="font-weight:bold; ">İadeye Konu Olan Faturalar</span>
                </td>
              </tr>
            </thead>
            <tbody>
              <tr id="lineTableTr" align="left">
                <td class="lineTableTd">
                  <span style="font-weight:bold; " align="center">     Fatura No</span>
                </td>
                <td class="lineTableTd">
                  <span style="font-weight:bold; " align="center">     Tarih</span>
                </td>
              </tr>
              <xsl:for-each select="//n1:Invoice/cac:BillingReference/cac:InvoiceDocumentReference/cbc:DocumentTypeCode[translate(text(),'abcçdefgğhıijklmnoöpqrsştuüvwxyz','ABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ')='İADE' or translate(text(),'abcçdefgğhıijklmnoöpqrsştuüvwxyz','ABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ')='IADE']">
                <tr id="lineTableTr" align="left">
                  <td class="lineTableTd">
                          <xsl:value-of select="../cbc:ID" />
                  </td>
                  <td class="lineTableTd">
                          <xsl:for-each select="../cbc:IssueDate">
                      <xsl:apply-templates select="." />
                    </xsl:for-each>
                  </td>
                </tr>
              </xsl:for-each>
            </tbody>
          </table>
          <br />
        </xsl:if>
        <xsl:if test="//n1:Invoice/cac:BillingReference/cac:AdditionalDocumentReference/cbc:DocumentTypeCode='OKCBF'">
          <table id="lineTable" class="fixedTableCss" width="800" border="1">
            <thead>
              <tr>
                <th colspan="6">ÖKC Bilgileri</th>
              </tr>
            </thead>
            <tbody>
              <tr id="okcbfHeadTr" style="font-weight:bold;">
                <td style="width:20%">
                  <xsl:text>Fiş Numarası</xsl:text>
                </td>
                <td style="width:10%" align="center">
                  <xsl:text>Fiş Tarihi</xsl:text>
                </td>
                <td style="width:10%" align="center">
                  <xsl:text>Fiş Saati</xsl:text>
                </td>
                <td style="width:40%" align="center">
                  <xsl:text>Fiş Tipi</xsl:text>
                </td>
                <td style="width:10%" align="center">
                  <xsl:text>Z Rapor No</xsl:text>
                </td>
                <td style="width:10%" align="center">
                  <xsl:text>ÖKC Seri No</xsl:text>
                </td>
              </tr>
            </tbody>
            <xsl:for-each select="//n1:Invoice/cac:BillingReference/cac:AdditionalDocumentReference/cbc:DocumentTypeCode[text()='OKCBF']">
              <tr>
                <td style="width:20%">
                  <xsl:value-of select="../cbc:ID" />
                </td>
                <td style="width:10%" align="center">
                  <xsl:value-of select="../cbc:IssueDate" />
                </td>
                <td style="width:10%" align="center">
                  <xsl:value-of select="substring(../cac:ValidityPeriod/cbc:StartTime,1,5)" />
                </td>
                <td style="width:40%" align="center">
                  <xsl:choose>
                    <xsl:when test="../cbc:DocumentDescription='AVANS'">
                      <xsl:text>Ön Tahsilat(Avans) Bilgi Fişi</xsl:text>
                    </xsl:when>
                    <xsl:when test="../cbc:DocumentDescription='YEMEK_FIS'">
                      <xsl:text>Yemek Fişi/Kartı ile Yapılan Tahsilat Bilgi Fişi</xsl:text>
                    </xsl:when>
                    <xsl:when test="../cbc:DocumentDescription='E-FATURA'">
                      <xsl:text>E-Fatura Bilgi Fişi</xsl:text>
                    </xsl:when>
                    <xsl:when test="../cbc:DocumentDescription='E-FATURA_IRSALIYE'">
                      <xsl:text>İrsaliye Yerine Geçen E-Fatura Bilgi Fişi</xsl:text>
                    </xsl:when>
                    <xsl:when test="../cbc:DocumentDescription='E-ARSIV'">
                      <xsl:text>E-Arşiv Bilgi Fişi</xsl:text>
                    </xsl:when>
                    <xsl:when test="../cbc:DocumentDescription='E-ARSIV_IRSALIYE'">
                      <xsl:text>İrsaliye Yerine Geçen E-Arşiv Bilgi Fişi</xsl:text>
                    </xsl:when>
                    <xsl:when test="../cbc:DocumentDescription='FATURA'">
                      <xsl:text>Faturalı Satış Bilgi Fişi</xsl:text>
                    </xsl:when>
                    <xsl:when test="../cbc:DocumentDescription='OTOPARK'">
                      <xsl:text>Otopark Giriş Bilgi Fişi</xsl:text>
                    </xsl:when>
                    <xsl:when test="../cbc:DocumentDescription='FATURA_TAHSILAT'">
                      <xsl:text>Fatura Tahsilat Bilgi Fişi</xsl:text>
                    </xsl:when>
                    <xsl:when test="../cbc:DocumentDescription='FATURA_TAHSILAT_KOMISYONLU'">
                      <xsl:text>Komisyonlu Fatura Tahsilat Bilgi Fişi</xsl:text>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:text> </xsl:text>
                    </xsl:otherwise>
                  </xsl:choose>
                </td>
                <td style="width:10%" align="center">
                  <xsl:value-of select="../cac:Attachment/cac:ExternalReference/cbc:URI" />
                </td>
                <td style="width:10%" align="center">
                  <xsl:value-of select="../cac:IssuerParty/cbc:EndpointID" />
                </td>
              </tr>
            </xsl:for-each>
          </table>
          <br />
        </xsl:if>
        <xsl:if test="count(//n1:Invoice/cac:DespatchDocumentReference/cbc:ID) &gt; 1">
          <table id="despatchDocumentReferenceTable" class="fixedTableCss" width="800">
            <tr>
              <td align="left" style="padding: 4px 5px; border: 2px solid black;width:145px;">
                <span style="font-weight:bold; ">
                  <xsl:text>İrsaliye No ve Tarihleri :</xsl:text>
                </span>
              </td>
              <td style="padding: 4px 5px; border: 2px solid black;">
                <xsl:for-each select="//n1:Invoice/cac:DespatchDocumentReference">
                  <xsl:if test="cbc:ID !='' and cbc:IssueDate !=''">
                    <xsl:value-of select="cbc:ID" />
                    <xsl:text>  (</xsl:text>
                    <xsl:value-of select="substring(cbc:IssueDate,9,2)" />-<xsl:value-of select="substring(cbc:IssueDate,6,2)" />-<xsl:value-of select="substring(cbc:IssueDate,1,4)" />
                    <xsl:text>)</xsl:text>
                    <xsl:if test="position() != last()">
                      <xsl:text>  |  </xsl:text>
                    </xsl:if>
                  </xsl:if>
                </xsl:for-each>
              </td>
            </tr>
          </table>
          <br />
        </xsl:if>
        <table id="notesTable" class="fixedTableCss" width="800" height="100">
          <tbody>
            <tr align="left">
              <td id="notesTableTd">
                <xsl:if test="not($senaryo = 'IHRACAT' or $senaryo = 'İHRACAT' or $senaryo = 'YOLCUBERABERFATURA' or $senaryo = 'YOLCUBERABER')">
                  <xsl:choose>
                    <xsl:when test="//n1:Invoice/cbc:InvoiceTypeCode = 'IHRACKAYITLI'">
                      <xsl:for-each select="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal">
                        <xsl:if test="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode='0015' and cac:TaxCategory/cbc:TaxExemptionReason != ''">
                          <b>Vergi İstisna Muafiyet Sebebi: </b>
                          <xsl:value-of select="cac:TaxCategory/cbc:TaxExemptionReason" />
                          <br />
                        </xsl:if>
                      </xsl:for-each>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:for-each select="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal">
                        <xsl:if test="cbc:TaxAmount=0 and cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode='0015' and cac:TaxCategory/cbc:TaxExemptionReason != ''">
                          <b>Vergi İstisna Muafiyet Sebebi: </b>
                          <xsl:value-of select="cac:TaxCategory/cbc:TaxExemptionReason" />
                          <br />
                        </xsl:if>
                      </xsl:for-each>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:if>
                <xsl:if test="$senaryo = 'IHRACAT' or $senaryo = 'İHRACAT' or $senaryo = 'YOLCUBERABERFATURA' or $senaryo = 'YOLCUBERABER'">
                  <xsl:for-each select="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal">
                    <xsl:if test="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode='0015' and cac:TaxCategory/cbc:TaxExemptionReason != ''">
                      <b>Vergi İstisna Muafiyet Sebebi: </b>
                      <xsl:value-of select="cac:TaxCategory/cbc:TaxExemptionReason" />
                      <br />
                    </xsl:if>
                  </xsl:for-each>
                </xsl:if>
                <xsl:for-each select="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal">
                  <xsl:if test="starts-with(cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode,'007') and cac:TaxCategory/cbc:TaxExemptionReason != ''">
                    <b>ÖTV İstisna Muafiyet Sebebi: </b>
                    <xsl:value-of select="cac:TaxCategory/cbc:TaxExemptionReasonCode" />
                    <xsl:text>-</xsl:text>
                    <xsl:value-of select="cac:TaxCategory/cbc:TaxExemptionReason" />
                    <br />
                  </xsl:if>
                </xsl:for-each>
                <xsl:for-each select="//n1:Invoice/cac:WithholdingTaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
                  <xsl:if test="cbc:TaxTypeCode != '' or cbc:Name != ''">
                    <b>Tevkifat Sebebi: </b>
                    <xsl:value-of select="cbc:TaxTypeCode" />
                    <xsl:text>-</xsl:text>
                    <xsl:value-of select="cbc:Name" />
                    <br />
                  </xsl:if>
                </xsl:for-each>
                <xsl:for-each select="//n1:Invoice/cbc:Note">
                  <xsl:if test="not(contains(.,'Shipper'))
                               and not(contains(.,'Consignee'))   
                               and not(contains(.,'Reference No'))
                               and not(contains(.,'Port of origin'))
                          and not(contains(.,'Origin'))and not(contains(.,'Destination'))and not(contains(.,'Flight No/Date'))
                               and not(contains(.,'Port of destination'))
                               and not(contains(.,'Mbl No'))
                               and not(contains(.,'Hbl No'))
                               and not(contains(.,'Vessel/Voyage No'))
                               and not(contains(.,'Pcs/Weight/Cbm '))
                               and not(contains(.,'Commodity'))
                               and not(contains(.,'ETD/ETA'))
                               and not(contains(.,'Type Of Container'))
                               and not(contains(.,'Container No'))
                               and not(contains(.,'Gönderici'))   
                               and not(contains(.,'Alıcı'))
                               and not(contains(.,'Referans No'))
                               and not(contains(.,'Çıkış Limanı'))
                               and not(contains(.,'Varış Limanı'))
                               and not(contains(.,'Mbl No  '))
                               and not(contains(.,'Hbl No '))
                               and not(contains(.,'Gemi Adı/Sefer No'))
                               and not(contains(.,'Kap/Kilo/M3'))
                               and not(contains(.,'Mal Cinsi '))
                               and not(contains(.,'Kalkış/Varış'))
                               and not(contains(.,'Konteyner Tipi Ve Adet'))
                               and not(contains(.,'Konteyner No '))
                               and not(contains(.,'Mawb No'))   
                               and not(contains(.,'Hawb No '))
                               and not(contains(.,'Flıght No/Date'))
                               and not(contains(.,'Pcs/G.W/C.W '))
                               and not(contains(.,'Uçuş No/Tarih'))
                               and not(contains(.,'Kap/Brüt Kkilo Ü.Ağırlık'))
                               and not(contains(.,'Varış'))
                               and not(contains(.,'Çıkış'))
                    ">
                    <xsl:value-of select="." />
                    <br />
                  </xsl:if>
                </xsl:for-each>
                <xsl:if test="//n1:Invoice/cac:PaymentMeans/cbc:InstructionNote !='' ">
                  <b>Ödeme Notu: </b>
                  <xsl:value-of select="//n1:Invoice/cac:PaymentMeans/cbc:InstructionNote" />
                  <br />
                </xsl:if>
                <xsl:if test="//n1:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote !='' ">
                  <b>Hesap Açıklaması: </b>
                  <xsl:value-of select="//n1:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote" />
                  <br />
                </xsl:if>
                <xsl:if test="//n1:Invoice/cac:PaymentTerms/cbc:Note !='' ">
                  <b>Ödeme Koşulu: </b>
                  <xsl:value-of select="//n1:Invoice/cac:PaymentTerms/cbc:Note" />
                  <br />
                </xsl:if>
              </td>
            </tr>
            <tr>
              <td>
                <xsl:if test="contains($FatTur,'DenizTurkce')">
                  <table border="0" id="notlar2"  width="795">
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Referans No </xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Referans No')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Gemi Adı/Sefer No</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Gemi Adı/Sefer No')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <span style="font-weight:bold;">
                          <xsl:text>Gönderici</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Gönderici')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Alıcı</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Alıcı')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Çıkış Limanı</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Çıkış Limanı')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Varış Limanı </xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Varış Limanı ')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>HBL No</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Hbl No')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>MBL No</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Mbl No')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Kap/Kilo/M3</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Kap/Kilo/M3')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Mal Cinsi </xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Mal Cinsi ')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Kalkış/Varış </xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Kalkış/Varış ')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Konteyner Tipi Ve Adet</xsl:text>
                        </span>
                      </td>
                      <td >
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Konteyner Tipi Ve Adet')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Konteyner No</xsl:text>
                        </span>
                      </td>
                      <td colspan="3">
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Konteyner No')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                  </table>
                </xsl:if>
                <xsl:if test="contains($FatTur,'DenizIngilizce') and not (contains($FatTur,'HavaIngilizce'))">
                  <table border="0" id="notlar2"  width="795">
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Reference No</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Reference No')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Vessel/Voyage No</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Vessel/Voyage No')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <span style="font-weight:bold;">
                          <xsl:text>Shipper</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Shipper')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Consignee</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Consignee')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Port of origin</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Port of origin')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Port of destination</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Port of destination')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Mbl No</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Mbl No')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Hbl No</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Hbl No')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Pcs/Weight/Cbm</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Commodity')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Commodity</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Commodity')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>ETD/ETA</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'ETD/ETA')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Type Of Container</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Type Of Container')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <td >
                      <span style="font-weight:bold;">
                        <xsl:text>Container No</xsl:text>
                      </span>
                    </td>
                    <td colspan="4">
                      <xsl:text> : </xsl:text>
                      <xsl:for-each select="//n1:Invoice/cbc:Note">
                        <xsl:value-of select="substring-after(.,'Container No')" />
                      </xsl:for-each>
                    </td>
                    <tr>
                    </tr>
                  </table>
                </xsl:if>
                <xsl:if test="contains($FatTur,'HavaTurkce')">
                  <table border="0" id="notlar2"  width="795">
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Referans No </xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Referans No')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Uçuş No/Tarih</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Uçuş No/Tarih')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <span style="font-weight:bold;">
                          <xsl:text>Gönderici</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Gönderici')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Alıcı</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Alıcı')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Çıkış</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Çıkış')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Varış</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Varış')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Mawb No</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Mawb No')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Hawb No</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Hawb No')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Kap/Brüt Kkilo Ü.Ağırlık</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Kap/Brüt Kkilo Ü.Ağırlık')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Mal Cinsi </xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Mal Cinsi ')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                  </table>
                </xsl:if>
                <xsl:if test="contains($FatTur,'HavaIngilizce')">
                  <table border="0" id="notlar2"  width="795">
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Reference No</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Reference No')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Flight No/Date</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Flight No/Date')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <span style="font-weight:bold;">
                          <xsl:text>Shipper</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Shipper')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Consignee</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Consignee')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Origin</xsl:text>
                        </span>

                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Origin')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Destination</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Destination')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Mawb No</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Mawb No')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Hawb No</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Hawb No')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                    <tr>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Pcs/G.W/C.W</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Pcs/G.W/C.W')" />
                        </xsl:for-each>
                      </td>
                      <td >
                        <span style="font-weight:bold;">
                          <xsl:text>Commodity</xsl:text>
                        </span>
                      </td>
                      <td>
                        <xsl:text> : </xsl:text>
                        <xsl:for-each select="//n1:Invoice/cbc:Note">
                          <xsl:value-of select="substring-after(.,'Commodity')" />
                        </xsl:for-each>
                      </td>
                    </tr>
                  </table>
                </xsl:if>

              </td>
            </tr>
          </tbody>
        </table>
        <br/>

        <br/>
        <table id="hesapBilgileri" width="800">
          <tbody>
            <tr>
              <td>
                <fieldset style="border:1px solid black;">
                  <legend align="center" style="background-color:white; border-width:10px;">
                    <b>BANKA HESAP BİLGİLERİMİZ (BANK ACCOUNT DETAILS)</b>
                  </legend>
                  <table width="100%">
                    <tr>
                      <td style="font-weight:bold; font-size:9px;">BANKA (BANK NAME)</td>
                      <td style="font-weight:bold; font-size:9px;">ŞUBE VE KODU (BRANCH NAME-CODE)</td>
                      <td style="font-weight:bold; font-size:9px;">SWIFT NO (SWIFT CODE)</td>
                      <td style="font-weight:bold; font-size:9px;">PARA BİRİMİ (CURRENCY UNIT)</td>
                      <td style="font-weight:bold; font-size:9px;">IBAN NO</td>
                    </tr>
                    <tr>
                      <td >T. GARANTİ BANKASI A.Ş.</td>
                      <td >HAVUZ AVCILAR - 702</td>
                      <td >TGBATRISXXX</td>
                      <td >TL</td>
                      <td >TR53 0006 2000 7020 0006 2947 58</td>
                    </tr>
                    <tr>
                      <td >T. GARANTİ BANKASI A.Ş.</td>
                      <td >HAVUZ AVCILAR - 702</td>
                      <td >TGBATRISXXX</td>
                      <td >USD</td>
                      <td >TR74 0006 2000 7020 0009 0401 59</td>
                    </tr>
                    <tr>
                      <td >T. GARANTİ BANKASI A.Ş.</td>
                      <td >HAVUZ AVCILAR - 702</td>
                      <td >TGBATRISXXX</td>
                      <td >EUR</td>
                      <td >TR47 0006 2000 7020 0009 0401 60</td>
                    </tr>
                  </table>
                </fieldset>
              </td>
            </tr>
          </tbody>
        </table>
      </body>
    </html>
  </xsl:template>
  <xsl:template match="dateFormatter">
    <xsl:value-of select="substring(.,9,2)" />-<xsl:value-of select="substring(.,6,2)" />-<xsl:value-of select="substring(.,1,4)" />
  </xsl:template>
  <xsl:template match="//n1:Invoice/cac:InvoiceLine">
    <tr id="lineTableTr">
      <td class="lineTableTd">
        <xsl:text> </xsl:text>
        <xsl:value-of select="./cbc:ID" />
      </td>
      <td class="lineTableTd">
        <xsl:text> </xsl:text>
        <xsl:value-of select="./cac:Item/cbc:Name" />
      </td>
      <td class="lineTableTd" align="right">
        <xsl:text> </xsl:text>
        <xsl:value-of select="format-number(./cbc:InvoicedQuantity, '###.##0,####', 'european')" />
        <xsl:if test="./cbc:InvoicedQuantity/@unitCode">
          <xsl:for-each select="./cbc:InvoicedQuantity">
            <xsl:text />
            <xsl:choose>
              <xsl:when test="@unitCode  = 'C62'">
                <span>
                  <xsl:text> Adet</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'NIU'">
                <span>
                  <xsl:text> Adet</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'AS'">
                <span>
                  <xsl:text> Asorti</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MON'">
                <span>
                  <xsl:text> Ay</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'FOT'">
                <span>
                  <xsl:text> Ayak</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'D92'">
                <span>
                  <xsl:text> Bant</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'BAR'">
                <span>
                  <xsl:text> Bar</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'BR'">
                <span>
                  <xsl:text> Bar</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'EA'">
                <span>
                  <xsl:text> Beher</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = '2W'">
                <span>
                  <xsl:text> Bidon</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = '4A'">
                <span>
                  <xsl:text> Bobin</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CL'">
                <span>
                  <xsl:text> Bobin</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'GRO'">
                <span>
                  <xsl:text> Brüt</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'E4'">
                <span>
                  <xsl:text> Brüt Kg</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'GT'">
                <span>
                  <xsl:text> Brüt Ton</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'GD'">
                <span>
                  <xsl:text> Brüt Varil</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'AD'">
                <span>
                  <xsl:text> Byte</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CGM'">
                <span>
                  <xsl:text> Cgm</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'PR'">
                <span>
                  <xsl:text> Çift</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'HA'">
                <span>
                  <xsl:text> Çile</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CLT'">
                <span>
                  <xsl:text> Clt.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CMT'">
                <span>
                  <xsl:text> Cm</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CMK'">
                <span>
                  <xsl:text> Cm²</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CMQ'">
                <span>
                  <xsl:text> Cm³</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'RD'">
                <span>
                  <xsl:text> Çubuk</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'SA'">
                <span>
                  <xsl:text> Çuval</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'D79'">
                <span>
                  <xsl:text> Demet</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'A49'">
                <span>
                  <xsl:text> Denye</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DC'">
                <span>
                  <xsl:text> Disk</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'D61'">
                <span>
                  <xsl:text> Dk.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MIN'">
                <span>
                  <xsl:text> Dk.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DLT'">
                <span>
                  <xsl:text> Dlt.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DMK'">
                <span>
                  <xsl:text> Dm²</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DMT'">
                <span>
                  <xsl:text> Dm²</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DPC'">
                <span>
                  <xsl:text> Düzine</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DPR'">
                <span>
                  <xsl:text> Düzine</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DRL'">
                <span>
                  <xsl:text> Düzine</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DZN'">
                <span>
                  <xsl:text> Düzine</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DZP'">
                <span>
                  <xsl:text> Düzine</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = '5H'">
                <span>
                  <xsl:text> Faz</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'Z3'">
                <span>
                  <xsl:text> Fıçı</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'BH'">
                <span>
                  <xsl:text> Fırça</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'A76'">
                <span>
                  <xsl:text> Gal.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'GB'">
                <span>
                  <xsl:text> Galon</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'GLI'">
                <span>
                  <xsl:text> Galon</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'GLL'">
                <span>
                  <xsl:text> Galon</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'GRM'">
                <span>
                  <xsl:text> gr.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'GN'">
                <span>
                  <xsl:text> Gross Galon</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = '10'">
                <span>
                  <xsl:text> Grup</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DAY'">
                <span>
                  <xsl:text> Gün</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'RG'">
                <span>
                  <xsl:text> Halka</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'INH'">
                <span>
                  <xsl:text> İnç</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = '4B'">
                <span>
                  <xsl:text> Kap</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'NCR'">
                <span>
                  <xsl:text> Karat</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'Z2'">
                <span>
                  <xsl:text> Kasa/Sandık</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'D66'">
                <span>
                  <xsl:text> Kaset</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'KGM'">
                <span>
                  <xsl:text> Kg.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = '2P'">
                <span>
                  <xsl:text> Kilobyte</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'IE'">
                <span>
                  <xsl:text> Kişi</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'KJO'">
                <span>
                  <xsl:text> KJO</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'K6'">
                <span>
                  <xsl:text> Klt.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'KTM'">
                <span>
                  <xsl:text> Km</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'KMK'">
                <span>
                  <xsl:text> Km²</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'AB'">
                <span>
                  <xsl:text> Koli</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CT'">
                <span>
                  <xsl:text> Koli</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CH'">
                <span>
                  <xsl:text> Konteyner</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'BJ'">
                <span>
                  <xsl:text> Kova</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'PL'">
                <span>
                  <xsl:text> Kova</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CU'">
                <span>
                  <xsl:text> Kupa</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'BX'">
                <span>
                  <xsl:text> Kutu</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CA'">
                <span>
                  <xsl:text> Kutu</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CS'">
                <span>
                  <xsl:text> Kutu</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'B5'">
                <span>
                  <xsl:text> Kütük</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'B55'">
                <span>
                  <xsl:text> KVM</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'KWH'">
                <span>
                  <xsl:text> Kwh</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'KWT'">
                <span>
                  <xsl:text> Kwt</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'LTR'">
                <span>
                  <xsl:text> Lt.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'RL'">
                <span>
                  <xsl:text> Makara</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'SO'">
                <span>
                  <xsl:text> Makara</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MAW'">
                <span>
                  <xsl:text> Megawatt</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MGM'">
                <span>
                  <xsl:text> Mgm.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = '77'">
                <span>
                  <xsl:text> Miliinç</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MLT'">
                <span>
                  <xsl:text> Mlt</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MMT'">
                <span>
                  <xsl:text> Mm</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MMQ'">
                <span>
                  <xsl:text> Mm³</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MTR'">
                <span>
                  <xsl:text> Mt</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MTK'">
                <span>
                  <xsl:text> Mt²</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MTQ'">
                <span>
                  <xsl:text> Mt³</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'MWH'">
                <span>
                  <xsl:text> Mwh</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'NT'">
                <span>
                  <xsl:text> Net Ton</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'PA'">
                <span>
                  <xsl:text> Paket</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'D97'">
                <span>
                  <xsl:text> Palet</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'PF'">
                <span>
                  <xsl:text> Palet</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'BD'">
                <span>
                  <xsl:text> Pano</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'PG'">
                <span>
                  <xsl:text> Plaka</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'RO'">
                <span>
                  <xsl:text> Rulo</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'HUR'">
                <span>
                  <xsl:text> Saat</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'ST'">
                <span>
                  <xsl:text> Sayfa</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'BK'">
                <span>
                  <xsl:text> Sepet</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'SET'">
                <span>
                  <xsl:text> Set</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'CY'">
                <span>
                  <xsl:text> Silindir</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'BO'">
                <span>
                  <xsl:text> Şişe</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'D62'">
                <span>
                  <xsl:text> Sn.</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'LR'">
                <span>
                  <xsl:text> Tabaka</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'TN'">
                <span>
                  <xsl:text> Teneke</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = '26'">
                <span>
                  <xsl:text> Ton</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'AA'">
                <span>
                  <xsl:text> Top</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'BG'">
                <span>
                  <xsl:text> Torba/Poşet</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'TU'">
                <span>
                  <xsl:text> Tüp</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'BLD'">
                <span>
                  <xsl:text> Varil</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'DR'">
                <span>
                  <xsl:text> Varil</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'ANN'">
                <span>
                  <xsl:text> Yıl</xsl:text>
                </span>
              </xsl:when>
              <xsl:when test="@unitCode  = 'EV'">
                <span>
                  <xsl:text> Zarf</xsl:text>
                </span>
              </xsl:when>
            </xsl:choose>
          </xsl:for-each>
        </xsl:if>
      </td>
      <td class="lineTableTd" align="right">
        <xsl:text> </xsl:text>
        <xsl:call-template name="Curr_Type">
          <xsl:with-param name="valuePath" select="./cac:Price/cbc:PriceAmount" />
          <xsl:with-param name="format" select="'###.##0,00000000'" />
        </xsl:call-template>
      </td>
      <xsl:if test="not($senaryo = 'IHRACAT' or $senaryo = 'İHRACAT')">
        <td class="lineTableTd" align="middle">
          <xsl:text> </xsl:text>
          <xsl:for-each select="./cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
            <xsl:if test="cbc:TaxTypeCode='0015' ">
              <xsl:text />
              <xsl:if test="../../cbc:Percent">
                <xsl:value-of select="format-number(../../cbc:Percent, '###.##0', 'european')" />
              </xsl:if>
            </xsl:if>
          </xsl:for-each>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
          <xsl:for-each select="./cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
            <xsl:if test="cbc:TaxTypeCode='0015' ">
              <xsl:text />
              <xsl:call-template name="Curr_Type">
                <xsl:with-param name="valuePath" select="../../cbc:TaxAmount" />
                <xsl:with-param name="format" select="'###.##0,00'" />
              </xsl:call-template>
            </xsl:if>
          </xsl:for-each>
        </td>
      </xsl:if>
      <td class="lineTableTd" align="right">
        <xsl:text> </xsl:text>
        <xsl:call-template name="Curr_Type">
          <xsl:with-param name="valuePath" select="./cbc:LineExtensionAmount" />
          <xsl:with-param name="format" select="'###.##0,00'" />
        </xsl:call-template>
      </td>
      <xsl:if test="$senaryo = 'IHRACAT' or $senaryo = 'İHRACAT'">
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
          <xsl:for-each select="cac:Delivery/cac:DeliveryTerms/cbc:ID[@schemeID='INCOTERMS']">
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
          </xsl:for-each>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:for-each select="cac:Delivery/cac:Shipment/cac:TransportHandlingUnit/cac:ActualPackage/cbc:PackagingTypeCode">
            <xsl:call-template name="PackagingType">
              <xsl:with-param name="Packaging">
                <xsl:value-of select="." />
              </xsl:with-param>
            </xsl:call-template>
            <xsl:if test="position() != last()">
              <xsl:text>- </xsl:text>
            </xsl:if>
          </xsl:for-each>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:for-each select="cac:Delivery/cac:Shipment/cac:TransportHandlingUnit/cac:ActualPackage/cbc:ID">
            <xsl:value-of select="." />
            <xsl:if test="position() != last()">
              <xsl:text>- </xsl:text>
            </xsl:if>
          </xsl:for-each>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:for-each select="cac:Delivery/cac:Shipment/cac:TransportHandlingUnit/cac:ActualPackage/cbc:Quantity">
            <xsl:value-of select="." />
            <xsl:if test="position() != last()">
              <xsl:text>- </xsl:text>
            </xsl:if>
          </xsl:for-each>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:for-each select="cac:Delivery/cac:DeliveryAddress">
            <xsl:if test="cbc:StreetName !=''">
              <xsl:value-of select="cbc:StreetName" />
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="cbc:BuildingName !=''">
              <xsl:value-of select="cbc:BuildingName" />
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="cbc:BuildingNumber !=''">
              <xsl:value-of select="cbc:BuildingNumber" />
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="cbc:Room !=''">
              <xsl:value-of select="cbc:Room" />
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="cbc:PostalZone !=''">
              <xsl:value-of select="cbc:PostalZone" />
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="cbc:District !=''">
              <xsl:value-of select="cbc:District" />
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="cbc:CitySubdivisionName !=''">
              <xsl:value-of select="cbc:CitySubdivisionName" />
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="cbc:CityName !=''">
              <xsl:value-of select="cbc:CityName" />
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="cbc:Region !=''">
              <xsl:value-of select="cbc:Region" />
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="cac:Country/cbc:Name !=''">
              <xsl:value-of select="cac:Country/cbc:Name" />
              <xsl:text> </xsl:text>
            </xsl:if>
          </xsl:for-each>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:for-each select="cac:Delivery/cac:Shipment/cac:ShipmentStage/cbc:TransportModeCode">
            <xsl:text> </xsl:text>
            <xsl:call-template name="TransportMode">
              <xsl:with-param name="TransportModeType">
                <xsl:value-of select="." />
              </xsl:with-param>
            </xsl:call-template>
          </xsl:for-each>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
          <xsl:value-of select="cac:Delivery/cac:Shipment/cac:GoodsItem/cbc:RequiblackCustomsID" />
        </td>
      </xsl:if>
    </tr>
  </xsl:template>
  <xsl:template match="//n1:Invoice">
    <tr class="lineTableTr">
      <td class="lineTableTd">
        <xsl:text> </xsl:text>
      </td>
      <td class="lineTableTd">
        <xsl:text> </xsl:text>
      </td>
      <td class="lineTableTd" align="right">
        <xsl:text> </xsl:text>
      </td>
      <td class="lineTableTd" align="right">
        <xsl:text> </xsl:text>
      </td>
      <td class="lineTableTd" align="right">
        <xsl:text> </xsl:text>
      </td>
      <td class="lineTableTd" align="right">
        <xsl:text> </xsl:text>
      </td>
      <xsl:if test="contains($hasAnyFalseChangeIndicator,'TRUE')">
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
      </xsl:if>
      <xsl:if test="not($senaryo = 'IHRACAT' or $senaryo = 'İHRACAT')">
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
      </xsl:if>
      <td class="lineTableTd" align="right">
        <xsl:text> </xsl:text>
      </td>
      <xsl:if test="$senaryo = 'IHRACAT' or $senaryo = 'İHRACAT'">
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
        <td class="lineTableTd" align="right">
          <xsl:text> </xsl:text>
        </td>
      </xsl:if>
    </tr>
  </xsl:template>
  <xsl:template name="Party_Title">
    <xsl:param name="PartyType" />
    <td align="left">
      <xsl:if test="cac:PartyName !=''">
        <xsl:value-of select="cac:PartyName/cbc:Name" />
        <br />
      </xsl:if>
      <xsl:for-each select="cac:Person">
        <xsl:for-each select="cbc:Title">
          <xsl:if test=". !=''">
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="cbc:FirstName">
          <xsl:if test=". !=''">
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="cbc:MiddleName">
          <xsl:if test=". !=''">
            <xsl:apply-templates />
            <xsl:text>  </xsl:text>
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="cbc:FamilyName">
          <xsl:if test=". !=''">
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="cbc:NameSuffix">
          <xsl:apply-templates />
        </xsl:for-each>
        <xsl:if test="$PartyType='TAXFREE'">
          <br />
          <xsl:if test="cac:IdentityDocumentReference/cbc:ID != ''">
            <xsl:text>Pasaport No: </xsl:text>
            <xsl:value-of select="cac:IdentityDocumentReference/cbc:ID" />
            <br />
          </xsl:if>
          <xsl:if test="cbc:NationalityID != ''">
            <xsl:text>Ülkesi: </xsl:text>
            <xsl:for-each select="cbc:NationalityID">
              <xsl:call-template name="Country">
                <xsl:with-param name="CountryType">
                  <xsl:value-of select="." />
                </xsl:with-param>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:if>
        </xsl:if>
      </xsl:for-each>
    </td>
  </xsl:template>
  <xsl:template name="Party_Adress">
    <xsl:param name="PartyType" />
    <td align="left">
      <xsl:for-each select="cac:PostalAddress">
        <xsl:for-each select="cbc:StreetName">
          <xsl:if test=". != ''">
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="cbc:BuildingName">
          <xsl:if test=".!= ''">
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="cbc:BuildingNumber">
          <xsl:if test=".!= ''">
            <xsl:text>No:</xsl:text>
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
          </xsl:if>
        </xsl:for-each>
        <xsl:if test="cbc:StreetName !='' or cbc:BuildingName !='' or cbc:BuildingNumber !=''">
          <br />
        </xsl:if>
        <xsl:for-each select="cbc:Room">
          <xsl:if test=".!=''">
            <xsl:text>Kapı No:</xsl:text>
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
            <br />
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="cbc:PostalZone">
          <xsl:if test=". != ''">
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="cbc:District">
          <xsl:if test=". !=''">
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="cbc:CitySubdivisionName">
          <xsl:apply-templates />
        </xsl:for-each>
        <xsl:if test="cbc:CitySubdivisionName and cbc:CityName != ''">
          <xsl:text>/ </xsl:text>
        </xsl:if>
        <xsl:for-each select="cbc:CityName">
          <xsl:if test=". != ''">
            <xsl:apply-templates />
            <xsl:text> </xsl:text>
          </xsl:if>
        </xsl:for-each>
        <xsl:if test="$PartyType!='OTHER' and $PartyType!='TAXFREE'">
          <xsl:if test="cac:Country/cbc:Name != ''">
            <br />
            <xsl:value-of select="cac:Country/cbc:Name" />
          </xsl:if>
        </xsl:if>
      </xsl:for-each>
      <xsl:if test="$PartyType='EXPORT'">
        <xsl:for-each select="cac:PartyLegalEntity">
          <xsl:for-each select="cbc:CompanyID">
            <xsl:if test=". != ''">
              <br />
              <xsl:text>Ülkesindeki VKN: </xsl:text>
              <xsl:value-of select="." />
            </xsl:if>
          </xsl:for-each>
          <xsl:for-each select="cbc:RegistrationName">
            <xsl:if test=". != ''">
              <br />
              <xsl:text>Resmi Unvan: </xsl:text>
              <xsl:value-of select="." />
            </xsl:if>
          </xsl:for-each>
        </xsl:for-each>
      </xsl:if>
    </td>
  </xsl:template>
  <xsl:template name="Party_Other">
    <xsl:param name="PartyType" />
    <xsl:for-each select="cbc:WebsiteURI">
      <xsl:if test=". !=''">
        <tr align="left">
          <td>
            <xsl:text>Web Sitesi: </xsl:text>
            <xsl:value-of select="." />
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="cac:Contact/cbc:ElectronicMail">
      <xsl:if test=". !=''">
        <tr align="left">
          <td>
            <xsl:text>E-Posta: </xsl:text>
            <xsl:value-of select="." />
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="cac:Contact">
      <xsl:if test="cbc:Telephone != '' or cbc:Telefax != ''">
        <tr align="left">
          <td align="left">
            <xsl:for-each select="cbc:Telephone">
              <xsl:if test=". !=''">
                <xsl:text>Tel: </xsl:text>
                <xsl:apply-templates />
              </xsl:if>
            </xsl:for-each>
            <xsl:for-each select="cbc:Telefax">
              <xsl:if test=". !=''">
                <xsl:text> Fax: </xsl:text>
                <xsl:apply-templates />
              </xsl:if>
            </xsl:for-each>
            <xsl:text> </xsl:text>
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
    <xsl:if test="$PartyType!='TAXFREE' and $PartyType!='EXPORT'">
      <xsl:for-each select="cac:PartyTaxScheme/cac:TaxScheme/cbc:Name">
        <xsl:if test=". !=''">
          <tr align="left">
            <td>
              <xsl:text>Vergi Dairesi: </xsl:text>
              <xsl:apply-templates />
            </td>
          </tr>
        </xsl:if>
      </xsl:for-each>
    </xsl:if>
    <xsl:for-each select="cac:PartyIdentification">
      <xsl:if test="cbc:ID != '' and not(contains(cbc:ID/@schemeID,'PARTYTYPE'))">
        <tr align="left">
          <td>
            <xsl:value-of select="cbc:ID/@schemeID" />
            <xsl:text>: </xsl:text>
            <xsl:value-of select="cbc:ID" />
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="TransportMode">
    <xsl:param name="TransportModeType" />
    <xsl:choose>
      <xsl:when test="$TransportModeType=1">Deniz Taşımacılığı</xsl:when>
      <xsl:when test="$TransportModeType=2">Demiryolu Taşımacılığı</xsl:when>
      <xsl:when test="$TransportModeType=3">Karayolu Taşımacılığı</xsl:when>
      <xsl:when test="$TransportModeType=4">Hava Taşımacılığı</xsl:when>
      <xsl:when test="$TransportModeType=5">Posta</xsl:when>
      <xsl:when test="$TransportModeType=6">Kombine Taşımacılık</xsl:when>
      <xsl:when test="$TransportModeType=7">Sabit Nakliyat</xsl:when>
      <xsl:when test="$TransportModeType=8">Ülke İçi Su Taşımacılığı</xsl:when>
      <xsl:when test="$TransportModeType=9">Uygun Olmayan Taşıma Şekli</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$TransportModeType" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="PackagingType">
    <xsl:param name="Packaging" />
    <xsl:choose>
      <xsl:when test="$Packaging='TD'">Açılır Kapanır Tüp / Portatif Tüp</xsl:when>
      <xsl:when test="$Packaging='AE'">Aerosol</xsl:when>
      <xsl:when test="$Packaging='NT'">Ağ</xsl:when>
      <xsl:when test="$Packaging='CK'">Ahşap Varil</xsl:when>
      <xsl:when test="$Packaging='PK'">Ambalaj</xsl:when>
      <xsl:when test="$Packaging='CE'">Balık Sepeti</xsl:when>
      <xsl:when test="$Packaging='SU'">Bavul</xsl:when>
      <xsl:when test="$Packaging='CB'">Bira Kasası</xsl:when>
      <xsl:when test="$Packaging='BB'">Bobin</xsl:when>
      <xsl:when test="$Packaging='BE'">Bohça</xsl:when>
      <xsl:when test="$Packaging='PI'">Boru</xsl:when>
      <xsl:when test="$Packaging='TR'">Büyük Eşya Sandığı</xsl:when>
      <xsl:when test="$Packaging='BU'">Büyük Fıçı</xsl:when>
      <xsl:when test="$Packaging='TO'">Büyük Fıçı (250 Galonluk)</xsl:when>
      <xsl:when test="$Packaging='HG'">Büyük Fıçı (250 Lt'lik)</xsl:when>
      <xsl:when test="$Packaging='VI'">Cam Şişe</xsl:when>
      <xsl:when test="$Packaging='BT'">Civata</xsl:when>
      <xsl:when test="$Packaging='CZ'">Çatır Bezi</xsl:when>
      <xsl:when test="$Packaging='BG'">Çanta</xsl:when>
      <xsl:when test="$Packaging='TC'">Çay Sandığı</xsl:when>
      <xsl:when test="$Packaging='SW'">Çekme - Sarma</xsl:when>
      <xsl:when test="$Packaging='FR'">Çerçeve</xsl:when>
      <xsl:when test="$Packaging='FD'">Çerçeveli Kasa</xsl:when>
      <xsl:when test="$Packaging='MB'">Çok Gözlü Çanta</xsl:when>
      <xsl:when test="$Packaging='PT'">Çömlek</xsl:when>
      <xsl:when test="$Packaging='BR'">Çubuk</xsl:when>
      <xsl:when test="$Packaging='SA'">Çuval</xsl:when>
      <xsl:when test="$Packaging='FL'">Dar Boyunlu Küçük Şişe</xsl:when>
      <xsl:when test="$Packaging='SC'">Dar Kasa</xsl:when>
      <xsl:when test="$Packaging='DR'">Davul</xsl:when>
      <xsl:when test="$Packaging='JC'">Dikdörtgen Bidon (20Lt'lik)</xsl:when>
      <xsl:when test="$Packaging='CA'">Dikdörtgen Madeni Kap</xsl:when>
      <xsl:when test="$Packaging='TK'">Dikdörtgen Tank</xsl:when>
      <xsl:when test="$Packaging='CU'">Fincan</xsl:when>
      <xsl:when test="$Packaging='FP'">Fotoğraf Filmleri Paketi</xsl:when>
      <xsl:when test="$Packaging='GB'">Gaz Şişesi</xsl:when>
      <xsl:when test="$Packaging='GE'">Gemici Sandığı</xsl:when>
      <xsl:when test="$Packaging='SE'">Gemici Sandığı</xsl:when>
      <xsl:when test="$Packaging='TB'">Gerdel</xsl:when>
      <xsl:when test="$Packaging='PL'">Gerdel</xsl:when>
      <xsl:when test="$Packaging='VG'">Hacim, Gaz (1031 M Bar Ve 15C)</xsl:when>
      <xsl:when test="$Packaging='VR'">Hacim, Katı, Granül Parçacıkları ('Taneler')</xsl:when>
      <xsl:when test="$Packaging='VY'">Hacim, Katı, İnce Parçacıkları ('Toz')</xsl:when>
      <xsl:when test="$Packaging='VQ'">Hacim, Sıvı Hale Getirilmiş Gaz (Anormal Isı/Basınç)</xsl:when>
      <xsl:when test="$Packaging='RG'">Halka (Çember)</xsl:when>
      <xsl:when test="$Packaging='MT'">Hasır</xsl:when>
      <xsl:when test="$Packaging='PH'">İbrik</xsl:when>
      <xsl:when test="$Packaging='SD'">İğ</xsl:when>
      <xsl:when test="$Packaging='SK'">İskelet Kasa</xsl:when>
      <xsl:when test="$Packaging='JT'">Jüt (Kenevir) Torba</xsl:when>
      <xsl:when test="$Packaging='CG'">Kafes</xsl:when>
      <xsl:when test="$Packaging='PN'">Kalas</xsl:when>
      <xsl:when test="$Packaging='CL'">Kangal</xsl:when>
      <xsl:when test="$Packaging='BI'">Kap</xsl:when>
      <xsl:when test="$Packaging='HR'">Kapaklı Sepet</xsl:when>
      <xsl:when test="$Packaging='CV'">Kapalı</xsl:when>
      <xsl:when test="$Packaging='CR'">Kasa</xsl:when>
      <xsl:when test="$Packaging='JR'">Kavanoz</xsl:when>
      <xsl:when test="$Packaging='PO'">Kese</xsl:when>
      <xsl:when test="$Packaging='MX'">Kibrit Kutusu</xsl:when>
      <xsl:when test="$Packaging='GI'">Kiriş</xsl:when>
      <xsl:when test="$Packaging='TS'">Kiriş</xsl:when>
      <xsl:when test="$Packaging='SH'">Koku Yastığı</xsl:when>
      <xsl:when test="$Packaging='PC'">Koli</xsl:when>
      <xsl:when test="$Packaging='KN'">Konteyner</xsl:when>
      <xsl:when test="$Packaging='DJ'">Korumalı, Hasır Büyük Şişe</xsl:when>
      <xsl:when test="$Packaging='BV'">Korumalı, Soğan Şeklinde Şişe</xsl:when>
      <xsl:when test="$Packaging='AM'">Korumalı Ampül</xsl:when>
      <xsl:when test="$Packaging='BP'">Korumalı Balon</xsl:when>
      <xsl:when test="$Packaging='CP'">Korumalı Damacana</xsl:when>
      <xsl:when test="$Packaging='BQ'">Korumalı Silindirik Şişe</xsl:when>
      <xsl:when test="$Packaging='DP'">Korumasız, Hasırlı Büyük Şişe</xsl:when>
      <xsl:when test="$Packaging='BS'">Korumasız, Soğan Şeklinde Şişe</xsl:when>
      <xsl:when test="$Packaging='AP'">Korumasız Ampül</xsl:when>
      <xsl:when test="$Packaging='BF'">Korumasız Balon</xsl:when>
      <xsl:when test="$Packaging='CO'">Korumasız Damacana</xsl:when>
      <xsl:when test="$Packaging='BO'">Korumasız Silindirik Şişe</xsl:when>
      <xsl:when test="$Packaging='BJ'">Kova</xsl:when>
      <xsl:when test="$Packaging='BX'">Kutu</xsl:when>
      <xsl:when test="$Packaging='KG'">Küçük Fıçı</xsl:when>
      <xsl:when test="$Packaging='FO'">Küçük Sandık</xsl:when>
      <xsl:when test="$Packaging='LG'">Kütük</xsl:when>
      <xsl:when test="$Packaging='RL'">Makara</xsl:when>
      <xsl:when test="$Packaging='FC'">Meyve Kasası</xsl:when>
      <xsl:when test="$Packaging='CT'">Mukavva Kutu</xsl:when>
      <xsl:when test="$Packaging='PA'">Paket</xsl:when>
      <xsl:when test="$Packaging='NE'">Paketlenmemiş Veya Ambalajlanmamış</xsl:when>
      <xsl:when test="$Packaging='AT'">Püskürgeç</xsl:when>
      <xsl:when test="$Packaging='RO'">Rulo</xsl:when>
      <xsl:when test="$Packaging='SM'">Sac</xsl:when>
      <xsl:when test="$Packaging='CH'">Sandık</xsl:when>
      <xsl:when test="$Packaging='CF'">Sandık</xsl:when>
      <xsl:when test="$Packaging='BK'">Sepet</xsl:when>
      <xsl:when test="$Packaging='WB'">Sepet Şişe</xsl:when>
      <xsl:when test="$Packaging='BN'">Sıkıştırılmamış Balya</xsl:when>
      <xsl:when test="$Packaging='BL'">Sıkıştırılmış Balya</xsl:when>
      <xsl:when test="$Packaging='CY'">Silindirik</xsl:when>
      <xsl:when test="$Packaging='JY'">Silindirik Bidon (20Lt'lik)</xsl:when>
      <xsl:when test="$Packaging='TY'">Silindirik Tank</xsl:when>
      <xsl:when test="$Packaging='CX'">Silindirik Teneke Kutu</xsl:when>
      <xsl:when test="$Packaging='RD'">Sopa</xsl:when>
      <xsl:when test="$Packaging='JG'">Sürahi</xsl:when>
      <xsl:when test="$Packaging='BC'">Şişe Kasası</xsl:when>
      <xsl:when test="$Packaging='ST'">Tabaka</xsl:when>
      <xsl:when test="$Packaging='PG'">Tabla</xsl:when>
      <xsl:when test="$Packaging='PU'">Tabla Paketi / Tabla</xsl:when>
      <xsl:when test="$Packaging='CJ'">Tabut</xsl:when>
      <xsl:when test="$Packaging='BD'">Tahta</xsl:when>
      <xsl:when test="$Packaging='VA'">Tekne</xsl:when>
      <xsl:when test="$Packaging='TN'">Teneke Kutu</xsl:when>
      <xsl:when test="$Packaging='TU'">Küp</xsl:when>
      <xsl:when test="$Packaging='FI'">Ufak Yağ Fıçısı</xsl:when>
      <xsl:when test="$Packaging='VP'">Vakumlu Paket</xsl:when>
      <xsl:when test="$Packaging='BA'">Varil</xsl:when>
      <xsl:when test="$Packaging='CC'">Yayık</xsl:when>
      <xsl:when test="$Packaging='NS'">Yuva</xsl:when>
      <xsl:when test="$Packaging='EN'">Zarf</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$Packaging" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="Country">
    <xsl:param name="CountryType" />
    <xsl:choose>
      <xsl:when test="$CountryType='AF'">Afganistan</xsl:when>
      <xsl:when test="$CountryType='DE'">Almanya</xsl:when>
      <xsl:when test="$CountryType='AD'">Andorra</xsl:when>
      <xsl:when test="$CountryType='AO'">Angola</xsl:when>
      <xsl:when test="$CountryType='AG'">Antigua ve Barbuda</xsl:when>
      <xsl:when test="$CountryType='AR'">Arjantin</xsl:when>
      <xsl:when test="$CountryType='AL'">Arnavutluk</xsl:when>
      <xsl:when test="$CountryType='AW'">Aruba</xsl:when>
      <xsl:when test="$CountryType='AU'">Avustralya</xsl:when>
      <xsl:when test="$CountryType='AT'">Avusturya</xsl:when>
      <xsl:when test="$CountryType='AZ'">Azerbaycan</xsl:when>
      <xsl:when test="$CountryType='BS'">Bahamalar</xsl:when>
      <xsl:when test="$CountryType='BH'">Bahreyn</xsl:when>
      <xsl:when test="$CountryType='BD'">Bangladeş</xsl:when>
      <xsl:when test="$CountryType='BB'">Barbados</xsl:when>
      <xsl:when test="$CountryType='EH'">Batı Sahra (MA)</xsl:when>
      <xsl:when test="$CountryType='BE'">Belçika</xsl:when>
      <xsl:when test="$CountryType='BZ'">Belize</xsl:when>
      <xsl:when test="$CountryType='BJ'">Benin</xsl:when>
      <xsl:when test="$CountryType='BM'">Bermuda</xsl:when>
      <xsl:when test="$CountryType='BY'">Beyaz Rusya</xsl:when>
      <xsl:when test="$CountryType='BT'">Bhutan</xsl:when>
      <xsl:when test="$CountryType='AE'">Birleşik Arap Emirlikleri</xsl:when>
      <xsl:when test="$CountryType='US'">Birleşik Devletler</xsl:when>
      <xsl:when test="$CountryType='GB'">Birleşik Krallık</xsl:when>
      <xsl:when test="$CountryType='BO'">Bolivya</xsl:when>
      <xsl:when test="$CountryType='BA'">Bosna-Hersek</xsl:when>
      <xsl:when test="$CountryType='BW'">Botsvana</xsl:when>
      <xsl:when test="$CountryType='BR'">Brezilya</xsl:when>
      <xsl:when test="$CountryType='BN'">Bruney</xsl:when>
      <xsl:when test="$CountryType='BG'">Bulgaristan</xsl:when>
      <xsl:when test="$CountryType='BF'">Burkina Faso</xsl:when>
      <xsl:when test="$CountryType='BI'">Burundi</xsl:when>
      <xsl:when test="$CountryType='TD'">Çad</xsl:when>
      <xsl:when test="$CountryType='KY'">Cayman Adaları</xsl:when>
      <xsl:when test="$CountryType='GI'">Cebelitarık (GB)</xsl:when>
      <xsl:when test="$CountryType='CZ'">Çek Cumhuriyeti</xsl:when>
      <xsl:when test="$CountryType='DZ'">Cezayir</xsl:when>
      <xsl:when test="$CountryType='DJ'">Cibuti</xsl:when>
      <xsl:when test="$CountryType='CN'">Çin</xsl:when>
      <xsl:when test="$CountryType='DK'">Danimarka</xsl:when>
      <xsl:when test="$CountryType='CD'">Demokratik Kongo Cumhuriyeti</xsl:when>
      <xsl:when test="$CountryType='TL'">Doğu Timor</xsl:when>
      <xsl:when test="$CountryType='DO'">Dominik Cumhuriyeti</xsl:when>
      <xsl:when test="$CountryType='DM'">Dominika</xsl:when>
      <xsl:when test="$CountryType='EC'">Ekvador</xsl:when>
      <xsl:when test="$CountryType='GQ'">Ekvator Ginesi</xsl:when>
      <xsl:when test="$CountryType='SV'">El Salvador</xsl:when>
      <xsl:when test="$CountryType='ID'">Endonezya</xsl:when>
      <xsl:when test="$CountryType='ER'">Eritre</xsl:when>
      <xsl:when test="$CountryType='AM'">Ermenistan</xsl:when>
      <xsl:when test="$CountryType='MF'">Ermiş Martin (FR)</xsl:when>
      <xsl:when test="$CountryType='EE'">Estonya</xsl:when>
      <xsl:when test="$CountryType='ET'">Etiyopya</xsl:when>
      <xsl:when test="$CountryType='FK'">Falkland Adaları</xsl:when>
      <xsl:when test="$CountryType='FO'">Faroe Adaları (DK)</xsl:when>
      <xsl:when test="$CountryType='MA'">Fas</xsl:when>
      <xsl:when test="$CountryType='FJ'">Fiji</xsl:when>
      <xsl:when test="$CountryType='CI'">Fildişi Sahili</xsl:when>
      <xsl:when test="$CountryType='PH'">Filipinler</xsl:when>
      <xsl:when test="$CountryType='FI'">Finlandiya</xsl:when>
      <xsl:when test="$CountryType='FR'">Fransa</xsl:when>
      <xsl:when test="$CountryType='GF'">Fransız Guyanası (FR)</xsl:when>
      <xsl:when test="$CountryType='PF'">Fransız Polinezyası (FR)</xsl:when>
      <xsl:when test="$CountryType='GA'">Gabon</xsl:when>
      <xsl:when test="$CountryType='GM'">Gambiya</xsl:when>
      <xsl:when test="$CountryType='GH'">Gana</xsl:when>
      <xsl:when test="$CountryType='GN'">Gine</xsl:when>
      <xsl:when test="$CountryType='GW'">Gine Bissau</xsl:when>
      <xsl:when test="$CountryType='GD'">Grenada</xsl:when>
      <xsl:when test="$CountryType='GL'">Grönland (DK)</xsl:when>
      <xsl:when test="$CountryType='GP'">Guadeloupe (FR)</xsl:when>
      <xsl:when test="$CountryType='GT'">Guatemala</xsl:when>
      <xsl:when test="$CountryType='GG'">Guernsey (GB)</xsl:when>
      <xsl:when test="$CountryType='ZA'">Güney Afrika</xsl:when>
      <xsl:when test="$CountryType='KR'">Güney Kore</xsl:when>
      <xsl:when test="$CountryType='GE'">Gürcistan</xsl:when>
      <xsl:when test="$CountryType='GY'">Guyana</xsl:when>
      <xsl:when test="$CountryType='HT'">Haiti</xsl:when>
      <xsl:when test="$CountryType='IN'">Hindistan</xsl:when>
      <xsl:when test="$CountryType='HR'">Hırvatistan</xsl:when>
      <xsl:when test="$CountryType='NL'">Hollanda</xsl:when>
      <xsl:when test="$CountryType='HN'">Honduras</xsl:when>
      <xsl:when test="$CountryType='HK'">Hong Kong (CN)</xsl:when>
      <xsl:when test="$CountryType='VG'">İngiliz Virjin Adaları</xsl:when>
      <xsl:when test="$CountryType='IQ'">Irak</xsl:when>
      <xsl:when test="$CountryType='IR'">İran</xsl:when>
      <xsl:when test="$CountryType='IE'">İrlanda</xsl:when>
      <xsl:when test="$CountryType='ES'">İspanya</xsl:when>
      <xsl:when test="$CountryType='IL'">İsrail</xsl:when>
      <xsl:when test="$CountryType='SE'">İsveç</xsl:when>
      <xsl:when test="$CountryType='CH'">İsviçre</xsl:when>
      <xsl:when test="$CountryType='IT'">İtalya</xsl:when>
      <xsl:when test="$CountryType='IS'">İzlanda</xsl:when>
      <xsl:when test="$CountryType='JM'">Jamaika</xsl:when>
      <xsl:when test="$CountryType='JP'">Japonya</xsl:when>
      <xsl:when test="$CountryType='JE'">Jersey (GB)</xsl:when>
      <xsl:when test="$CountryType='KH'">Kamboçya</xsl:when>
      <xsl:when test="$CountryType='CM'">Kamerun</xsl:when>
      <xsl:when test="$CountryType='CA'">Kanada</xsl:when>
      <xsl:when test="$CountryType='ME'">Karadağ</xsl:when>
      <xsl:when test="$CountryType='QA'">Katar</xsl:when>
      <xsl:when test="$CountryType='KZ'">Kazakistan</xsl:when>
      <xsl:when test="$CountryType='KE'">Kenya</xsl:when>
      <xsl:when test="$CountryType='CY'">Kıbrıs</xsl:when>
      <xsl:when test="$CountryType='KG'">Kırgızistan</xsl:when>
      <xsl:when test="$CountryType='KI'">Kiribati</xsl:when>
      <xsl:when test="$CountryType='CO'">Kolombiya</xsl:when>
      <xsl:when test="$CountryType='KM'">Komorlar</xsl:when>
      <xsl:when test="$CountryType='CG'">Kongo Cumhuriyeti</xsl:when>
      <xsl:when test="$CountryType='KV'">Kosova (RS)</xsl:when>
      <xsl:when test="$CountryType='CR'">Kosta Rika</xsl:when>
      <xsl:when test="$CountryType='CU'">Küba</xsl:when>
      <xsl:when test="$CountryType='KW'">Kuveyt</xsl:when>
      <xsl:when test="$CountryType='KP'">Kuzey Kore</xsl:when>
      <xsl:when test="$CountryType='LA'">Laos</xsl:when>
      <xsl:when test="$CountryType='LS'">Lesoto</xsl:when>
      <xsl:when test="$CountryType='LV'">Letonya</xsl:when>
      <xsl:when test="$CountryType='LR'">Liberya</xsl:when>
      <xsl:when test="$CountryType='LY'">Libya</xsl:when>
      <xsl:when test="$CountryType='LI'">Lihtenştayn</xsl:when>
      <xsl:when test="$CountryType='LT'">Litvanya</xsl:when>
      <xsl:when test="$CountryType='LB'">Lübnan</xsl:when>
      <xsl:when test="$CountryType='LU'">Lüksemburg</xsl:when>
      <xsl:when test="$CountryType='HU'">Macaristan</xsl:when>
      <xsl:when test="$CountryType='MG'">Madagaskar</xsl:when>
      <xsl:when test="$CountryType='MO'">Makao (CN)</xsl:when>
      <xsl:when test="$CountryType='MK'">Makedonya</xsl:when>
      <xsl:when test="$CountryType='MW'">Malavi</xsl:when>
      <xsl:when test="$CountryType='MV'">Maldivler</xsl:when>
      <xsl:when test="$CountryType='MY'">Malezya</xsl:when>
      <xsl:when test="$CountryType='ML'">Mali</xsl:when>
      <xsl:when test="$CountryType='MT'">Malta</xsl:when>
      <xsl:when test="$CountryType='IM'">Man Adası (GB)</xsl:when>
      <xsl:when test="$CountryType='MH'">Marshall Adaları</xsl:when>
      <xsl:when test="$CountryType='MQ'">Martinique (FR)</xsl:when>
      <xsl:when test="$CountryType='MU'">Mauritius</xsl:when>
      <xsl:when test="$CountryType='YT'">Mayotte (FR)</xsl:when>
      <xsl:when test="$CountryType='MX'">Meksika</xsl:when>
      <xsl:when test="$CountryType='FM'">Mikronezya</xsl:when>
      <xsl:when test="$CountryType='EG'">Mısır</xsl:when>
      <xsl:when test="$CountryType='MN'">Moğolistan</xsl:when>
      <xsl:when test="$CountryType='MD'">Moldova</xsl:when>
      <xsl:when test="$CountryType='MC'">Monako</xsl:when>
      <xsl:when test="$CountryType='MR'">Moritanya</xsl:when>
      <xsl:when test="$CountryType='MZ'">Mozambik</xsl:when>
      <xsl:when test="$CountryType='MM'">Myanmar</xsl:when>
      <xsl:when test="$CountryType='NA'">Namibya</xsl:when>
      <xsl:when test="$CountryType='NR'">Nauru</xsl:when>
      <xsl:when test="$CountryType='NP'">Nepal</xsl:when>
      <xsl:when test="$CountryType='NE'">Nijer</xsl:when>
      <xsl:when test="$CountryType='NG'">Nijerya</xsl:when>
      <xsl:when test="$CountryType='NI'">Nikaragua</xsl:when>
      <xsl:when test="$CountryType='NO'">Norveç</xsl:when>
      <xsl:when test="$CountryType='CF'">Orta Afrika Cumhuriyeti</xsl:when>
      <xsl:when test="$CountryType='UZ'">Özbekistan</xsl:when>
      <xsl:when test="$CountryType='PK'">Pakistan</xsl:when>
      <xsl:when test="$CountryType='PW'">Palau</xsl:when>
      <xsl:when test="$CountryType='PA'">Panama</xsl:when>
      <xsl:when test="$CountryType='PG'">Papua Yeni Gine</xsl:when>
      <xsl:when test="$CountryType='PY'">Paraguay</xsl:when>
      <xsl:when test="$CountryType='PE'">Peru</xsl:when>
      <xsl:when test="$CountryType='PL'">Polonya</xsl:when>
      <xsl:when test="$CountryType='PT'">Portekiz</xsl:when>
      <xsl:when test="$CountryType='PR'">Porto Riko (US)</xsl:when>
      <xsl:when test="$CountryType='RE'">Réunion (FR)</xsl:when>
      <xsl:when test="$CountryType='RO'">Romanya</xsl:when>
      <xsl:when test="$CountryType='RW'">Ruanda</xsl:when>
      <xsl:when test="$CountryType='RU'">Rusya</xsl:when>
      <xsl:when test="$CountryType='BL'">Saint Barthélemy (FR)</xsl:when>
      <xsl:when test="$CountryType='KN'">Saint Kitts ve Nevis</xsl:when>
      <xsl:when test="$CountryType='LC'">Saint Lucia</xsl:when>
      <xsl:when test="$CountryType='PM'">Saint Pierre ve Miquelon (FR)</xsl:when>
      <xsl:when test="$CountryType='VC'">Saint Vincent ve Grenadinler</xsl:when>
      <xsl:when test="$CountryType='WS'">Samoa</xsl:when>
      <xsl:when test="$CountryType='SM'">San Marino</xsl:when>
      <xsl:when test="$CountryType='ST'">São Tomé ve Príncipe</xsl:when>
      <xsl:when test="$CountryType='SN'">Senegal</xsl:when>
      <xsl:when test="$CountryType='SC'">Seyşeller</xsl:when>
      <xsl:when test="$CountryType='SL'">Sierra Leone</xsl:when>
      <xsl:when test="$CountryType='CL'">Şili</xsl:when>
      <xsl:when test="$CountryType='SG'">Singapur</xsl:when>
      <xsl:when test="$CountryType='RS'">Sırbistan</xsl:when>
      <xsl:when test="$CountryType='SK'">Slovakya Cumhuriyeti</xsl:when>
      <xsl:when test="$CountryType='SI'">Slovenya</xsl:when>
      <xsl:when test="$CountryType='SB'">Solomon Adaları</xsl:when>
      <xsl:when test="$CountryType='SO'">Somali</xsl:when>
      <xsl:when test="$CountryType='SS'">South Sudan</xsl:when>
      <xsl:when test="$CountryType='SJ'">Spitsbergen (NO)</xsl:when>
      <xsl:when test="$CountryType='LK'">Sri Lanka</xsl:when>
      <xsl:when test="$CountryType='SD'">Sudan</xsl:when>
      <xsl:when test="$CountryType='SR'">Surinam</xsl:when>
      <xsl:when test="$CountryType='SY'">Suriye</xsl:when>
      <xsl:when test="$CountryType='SA'">Suudi Arabistan</xsl:when>
      <xsl:when test="$CountryType='SZ'">Svaziland</xsl:when>
      <xsl:when test="$CountryType='TJ'">Tacikistan</xsl:when>
      <xsl:when test="$CountryType='TZ'">Tanzanya</xsl:when>
      <xsl:when test="$CountryType='TH'">Tayland</xsl:when>
      <xsl:when test="$CountryType='TW'">Tayvan</xsl:when>
      <xsl:when test="$CountryType='TG'">Togo</xsl:when>
      <xsl:when test="$CountryType='TO'">Tonga</xsl:when>
      <xsl:when test="$CountryType='TT'">Trinidad ve Tobago</xsl:when>
      <xsl:when test="$CountryType='TN'">Tunus</xsl:when>
      <xsl:when test="$CountryType='TR'">Türkiye</xsl:when>
      <xsl:when test="$CountryType='TM'">Türkmenistan</xsl:when>
      <xsl:when test="$CountryType='TC'">Turks ve Caicos</xsl:when>
      <xsl:when test="$CountryType='TV'">Tuvalu</xsl:when>
      <xsl:when test="$CountryType='UG'">Uganda</xsl:when>
      <xsl:when test="$CountryType='UA'">Ukrayna</xsl:when>
      <xsl:when test="$CountryType='OM'">Umman</xsl:when>
      <xsl:when test="$CountryType='JO'">Ürdün</xsl:when>
      <xsl:when test="$CountryType='UY'">Uruguay</xsl:when>
      <xsl:when test="$CountryType='VU'">Vanuatu</xsl:when>
      <xsl:when test="$CountryType='VA'">Vatikan</xsl:when>
      <xsl:when test="$CountryType='VE'">Venezuela</xsl:when>
      <xsl:when test="$CountryType='VN'">Vietnam</xsl:when>
      <xsl:when test="$CountryType='WF'">Wallis ve Futuna (FR)</xsl:when>
      <xsl:when test="$CountryType='YE'">Yemen</xsl:when>
      <xsl:when test="$CountryType='NC'">Yeni Kaledonya (FR)</xsl:when>
      <xsl:when test="$CountryType='NZ'">Yeni Zelanda</xsl:when>
      <xsl:when test="$CountryType='CV'">Yeşil Burun Adaları</xsl:when>
      <xsl:when test="$CountryType='GR'">Yunanistan</xsl:when>
      <xsl:when test="$CountryType='ZM'">Zambiya</xsl:when>
      <xsl:when test="$CountryType='ZW'">Zimbabve</xsl:when>
      <xsl:when test="$CountryType='AI'">Anguilla</xsl:when>
      <xsl:when test="$CountryType='AQ'">Antartika</xsl:when>
      <xsl:when test="$CountryType='AS'">Amerikan Samoa</xsl:when>
      <xsl:when test="$CountryType='AX'">Aland Adaları</xsl:when>
      <xsl:when test="$CountryType='BV'">Bouvet Adası</xsl:when>
      <xsl:when test="$CountryType='CK'">Cook Adaları</xsl:when>
      <xsl:when test="$CountryType='CX'">Christmas Adası</xsl:when>
      <xsl:when test="$CountryType='CW'">Curaçao</xsl:when>
      <xsl:when test="$CountryType='CC'">Cocos Adaları</xsl:when>
      <xsl:when test="$CountryType='BQ'">Bonaire, Sint Eustatius and Saba</xsl:when>
      <xsl:when test="$CountryType='GS'">Güney Gürcistan ve Güney Sandviç Adaları</xsl:when>
      <xsl:when test="$CountryType='GU'">Guam</xsl:when>
      <xsl:when test="$CountryType='HM'">Heard Adası and McDonald Adaları</xsl:when>
      <xsl:when test="$CountryType='IO'">Britanya Hindistan Okyanus Bölgesi</xsl:when>
      <xsl:when test="$CountryType='MP'">Kuzey Mariana Adaları</xsl:when>
      <xsl:when test="$CountryType='MS'">Montserrat</xsl:when>
      <xsl:when test="$CountryType='NF'">Norfolk Adası</xsl:when>
      <xsl:when test="$CountryType='NU'">Niue</xsl:when>
      <xsl:when test="$CountryType='PN'">Pitcairn</xsl:when>
      <xsl:when test="$CountryType='PS'">Filistin Devleti</xsl:when>
      <xsl:when test="$CountryType='SH'">Saint Helena, Ascension and Tristan Da Cunha</xsl:when>
      <xsl:when test="$CountryType='SX'">Sint Maarten (Dutch Part)</xsl:when>
      <xsl:when test="$CountryType='TF'">French Southern Territories</xsl:when>
      <xsl:when test="$CountryType='TK'">Tokelau</xsl:when>
      <xsl:when test="$CountryType='UM'">United States Minor Outlying Islands</xsl:when>
      <xsl:when test="$CountryType='VI'">Virgin Islands, U.S.</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$CountryType" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="Curr_Type">
    <xsl:param name="format" />
    <xsl:param name="valuePath" />
    <xsl:value-of select="format-number($valuePath, $format, 'european')" />
    <xsl:if test="$valuePath/@currencyID">
      <xsl:text> </xsl:text>
      <xsl:choose>
        <xsl:when test="$valuePath/@currencyID = 'TRL' or $valuePath/@currencyID = 'TRY'">
          <xsl:text>TL</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$valuePath/@currencyID" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>