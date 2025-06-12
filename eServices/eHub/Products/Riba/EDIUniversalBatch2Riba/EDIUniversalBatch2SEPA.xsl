<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var"
				exclude-result-prefixes="msxsl var s0 userCSharp ScriptNS0" version="1.0"
				xmlns:s0="http://www.cargowise.com/Schemas/Universal/2011/11"
				xmlns:ns0="urn:iso:std:iso:20022:tech:xsd:pain.008.001.02"
				xmlns:ScriptNS0="http://schemas.microsoft.com/BizTalk/2003/ScriptNS0"
				xmlns:userCSharp="http://schemas.microsoft.com/BizTalk/2003/userCSharp">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>

	<!--GROUPING USING THE MUENCHIAN METHOD-->
	<!--Group by Transaction Reference which is the order number!-->
	<xsl:key name="transaction-groupby-orderNo" match="/s0:UniversalTransactionBatch/s0:TransactionBatch/s0:TransactionCollection/s0:Transaction" use="s0:TransactionReference" />

	<xsl:variable name="var:SIDRegistrationNumber">
		<xsl:for-each select="s0:UniversalTransactionBatch/s0:TransactionBatch/s0:OrganizationAddressCollection/s0:OrganizationAddress/s0:RegistrationNumberCollection/s0:RegistrationNumber">
			<xsl:variable name="var:regCode" select="string(s0:Type/s0:Code/text())" />
			<xsl:variable name="var:foundSID" select="userCSharp:LogicalEq($var:regCode , &quot;SID&quot;)" />
			<xsl:if test="string($var:foundSID)='true'">
				<xsl:value-of select="string(s0:Value/text())" />
			</xsl:if>
		</xsl:for-each>
	</xsl:variable>

	<xsl:template match="/">
		<Document xmlns="urn:iso:std:iso:20022:tech:xsd:pain.008.001.02" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
			<CstmrDrctDbtInitn>
				<xsl:apply-templates select="/s0:UniversalTransactionBatch" />
				<xsl:apply-templates select="/s0:UniversalTransactionBatch/s0:TransactionBatch/s0:TransactionCollection" />
			</CstmrDrctDbtInitn>
		</Document>
	</xsl:template>

	<xsl:template match="/s0:UniversalTransactionBatch">
		<xsl:variable name="var:batch-id" select="ScriptNS0:SetContextProperty('OverrideFilename',
	  'http://cargowise.com/ehub/processing/2010/06', concat(s0:TransactionBatch/s0:DataContext/s0:Company/s0:Code/text(), concat(s0:TransactionBatch/s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type = 'CollectionBatch']/s0:Key/text(), '.xml')) )" />
		<GrpHdr xmlns="urn:iso:std:iso:20022:tech:xsd:pain.008.001.02" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
			<MsgId>
				<xsl:value-of select="string(s0:TransactionBatch/s0:DataContext/s0:DataSourceCollection/s0:DataSource[s0:Type = 'CollectionBatch']/s0:Key/text())" />
			</MsgId>
			<CreDtTm>
				<xsl:value-of select="substring(string(s0:TransactionBatch/s0:DataContext/s0:TriggerDate/text()),1,19)" />
			</CreDtTm>
			<NbOfTxs>
				<xsl:variable name="totalOrders" select="/s0:UniversalTransactionBatch/s0:TransactionBatch/s0:TransactionCollection/s0:Transaction[generate-id() = generate-id(key('transaction-groupby-orderNo', s0:TransactionReference)[1])]"/>
				<xsl:value-of select="count($totalOrders)"/>
			</NbOfTxs>
			<CtrlSum>
				<xsl:value-of select="format-number(sum(/s0:UniversalTransactionBatch/s0:TransactionBatch/s0:TransactionCollection/s0:Transaction/s0:OutstandingAmount), '0.##')"/>
			</CtrlSum>
			<InitgPty>
				<Nm>
					<xsl:value-of select="substring(s0:TransactionBatch/s0:DataContext/s0:Company/s0:Name/text(), 1, 70)" />
				</Nm>
				<Id>
					<OrgId>
						<Othr>
							<Id>
								<xsl:value-of select="$var:SIDRegistrationNumber" />
							</Id>
						</Othr>
					</OrgId>
				</Id>
			</InitgPty>
		</GrpHdr>
	</xsl:template>

	<xsl:template match="/s0:UniversalTransactionBatch/s0:TransactionBatch/s0:TransactionCollection">
		<!--GROUPING USING THE MUENCHIAN METHOD-->
		<xsl:for-each select="/s0:UniversalTransactionBatch/s0:TransactionBatch/s0:TransactionCollection/s0:Transaction[count(. | key('transaction-groupby-orderNo', s0:TransactionReference)[1]) = 1]">
			<xsl:sort select="s0:TransactionReference" />
			<xsl:variable name="vKeyGroup" select="key('transaction-groupby-orderNo', s0:TransactionReference)" />
			<PmtInf xmlns="urn:iso:std:iso:20022:tech:xsd:pain.008.001.02" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
				<PmtInfId>
					<xsl:value-of select="string(s0:TransactionReference/text())" />
				</PmtInfId>
				<PmtMtd>
					<xsl:text>DD</xsl:text>
				</PmtMtd>
				<NbOfTxs>
					<xsl:text>1</xsl:text>
				</NbOfTxs>
				<CtrlSum>
					<xsl:value-of select="format-number(sum($vKeyGroup/s0:OutstandingAmount), '0.##')" />
				</CtrlSum>
				<PmtTpInf>
					<SvcLvl>
						<Cd>
							<xsl:text>SEPA</xsl:text>
						</Cd>
					</SvcLvl>
					<LclInstrm>
						<Cd>
							<xsl:text>CORE</xsl:text>
						</Cd>
					</LclInstrm>
					<SeqTp>
						<xsl:text>RCUR</xsl:text>
					</SeqTp>
				</PmtTpInf>
				<ReqdColltnDt>
					<xsl:value-of select="substring(s0:OrderCollectionDate/text(), 1, 10)" />
				</ReqdColltnDt>

				<xsl:for-each select="$vKeyGroup[1]/s0:BankAccountCollection/s0:BankAccount">
					<xsl:variable name="var:accountType" select="string(s0:AccountType/text())" />
					<xsl:variable name="var:foundCR" select="userCSharp:LogicalEq($var:accountType , &quot;Credit&quot;)" />
					<xsl:if test="string($var:foundCR)='true'">
						<Cdtr>
							<Nm>
								<xsl:value-of select="string(s0:AccountName/text())" />
							</Nm>
						</Cdtr>
						<CdtrAcct>
							<Id>
								<IBAN>
									<xsl:value-of select="string(s0:IBANNumber/text())" />
								</IBAN>
							</Id>
						</CdtrAcct>
						<CdtrAgt>
							<FinInstnId>
								<xsl:if test="s0:BankSwift/text()">
									<BIC>
										<xsl:value-of select="string(s0:BankSwift/text())" />
									</BIC>
								</xsl:if>
								<xsl:if test="s0:Country/s0:Code">
									<PstlAdr>
										<Ctry>
											<xsl:value-of select="string(s0:Country/s0:Code/text())" />
										</Ctry>
									</PstlAdr>
								</xsl:if>
							</FinInstnId>
						</CdtrAgt>
						<CdtrSchmeId>
							<Id>
								<PrvtId>
									<Othr>
										<Id>
											<xsl:value-of select="$var:SIDRegistrationNumber" />
										</Id>
										<SchmeNm>
											<Prtry>
												<xsl:text>SEPA</xsl:text>
											</Prtry>
										</SchmeNm>
									</Othr>
								</PrvtId>
							</Id>
						</CdtrSchmeId>
					</xsl:if>
				</xsl:for-each>

				<xsl:for-each select="$vKeyGroup[1]/s0:BankAccountCollection/s0:BankAccount">
					<xsl:variable name="var:accountType" select="string(s0:AccountType/text())" />
					<xsl:variable name="var:foundDR" select="userCSharp:LogicalEq($var:accountType , &quot;Debit&quot;)" />
					<xsl:if test="string($var:foundDR)='true'">
						<DrctDbtTxInf>
							<PmtId>
								<EndToEndId>
									<xsl:value-of select="string(../../s0:TransactionReference/text())" />
								</EndToEndId>
							</PmtId>
							<InstdAmt Ccy="EUR">
								<xsl:value-of select="format-number(sum($vKeyGroup/s0:OutstandingAmount), '0.##')" />
							</InstdAmt>
							<DrctDbtTx>
								<MndtRltdInf>
									<MndtId>
										<xsl:value-of select="string(../../s0:SettlementMethod/s0:AuthorizationReference/text())" />
									</MndtId>
									<DtOfSgntr>
										<xsl:value-of select="substring(../../s0:SettlementMethod/s0:AuthorizationExpiryDate/text(), 1, 10)" />
									</DtOfSgntr>
								</MndtRltdInf>
							</DrctDbtTx>
							<DbtrAgt>
								<FinInstnId>
									<xsl:if test="s0:BankSwift/text()">
										<BIC>
											<xsl:value-of select="string(s0:BankSwift/text())" />
										</BIC>
									</xsl:if>
									<xsl:if test="s0:Country/s0:Code">
										<PstlAdr>
											<Ctry>
												<xsl:value-of select="string(s0:Country/s0:Code/text())" />
											</Ctry>
										</PstlAdr>
									</xsl:if>
								</FinInstnId>
							</DbtrAgt>
							<Dbtr>
								<Nm>
									<xsl:choose>
										<xsl:when test="s0:AccountName/text()">
											<xsl:value-of select="string(s0:AccountName/text())" />
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="string(../../s0:OrganizationAddress/s0:CompanyName/text())" />
										</xsl:otherwise>
									</xsl:choose>
								</Nm>
							</Dbtr>
							<DbtrAcct>
								<Id>
									<IBAN>
										<xsl:value-of select="string(s0:IBANNumber/text())" />
									</IBAN>
								</Id>
							</DbtrAcct>
							<xsl:for-each select="key('transaction-groupby-orderNo', ../../s0:TransactionReference)">
								<xsl:sort select="s0:Number" />
								<xsl:value-of select="userCSharp:AddTransactionNumber(string(s0:Number/text()))"/>
							</xsl:for-each>
							<xsl:variable name="var:listNumbers" select="userCSharp:GetTransactionNumbers()"/>
							<RmtInf>
								<Ustrd>
									<xsl:value-of select="substring($var:listNumbers, 1, 140)" />
								</Ustrd>
							</RmtInf>
						</DrctDbtTxInf>
					</xsl:if>
				</xsl:for-each>
			</PmtInf>
		</xsl:for-each>
	</xsl:template>

	<msxsl:script language="C#" implements-prefix="userCSharp">
		<![CDATA[

public string AddTransactionNumber(string transactionNumber)
{
	if(transactionNumbers != string.Empty) { transactionNumbers += ","; }
	transactionNumbers += transactionNumber;
  return string.Empty;
}

public string transactionNumbers = string.Empty;

public string GetTransactionNumbers()
{
	string ret = transactionNumbers;
	transactionNumbers = string.Empty;
  return ret;
}

public System.Collections.ArrayList myCumulativeSumArray = new System.Collections.ArrayList();

public bool LogicalEq(string val1, string val2)
{
	bool ret = false;
	double d1 = 0;
	double d2 = 0;
	if (IsNumeric(val1, ref d1) && IsNumeric(val2, ref d2))
	{
		ret = d1 == d2;
	}
	else
	{
		ret = String.Compare(val1, val2, StringComparison.Ordinal) == 0;
	}
	return ret;
}

public bool IsNumeric(string val)
{
	if (val == null)
	{
		return false;
	}
	double d = 0;
	return Double.TryParse(val, System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d);
}

public bool IsNumeric(string val, ref double d)
{
	if (val == null)
	{
		return false;
	}
	return Double.TryParse(val, System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d);
}


]]>
	</msxsl:script>
</xsl:stylesheet>
