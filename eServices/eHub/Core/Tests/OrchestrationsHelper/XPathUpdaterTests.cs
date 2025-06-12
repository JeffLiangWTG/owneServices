using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

namespace CargoWise.eHub.Core.Orchestrations.Helper.Tests
{
	[TestClass]
	public class XPathUpdaterTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUpdate()
		{
			#region original
			var original = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ns0:EDIFACTEnvelope xmlns:ns0=""http://wisetechglobal.com/eHub/Products/SGCustoms/MHAccess/2017/08/EDIFACTEnvelope"">
	<EDIFACTs>
		<gen0:EFACT_D99A_APERAK xmlns:gen0=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
			<UNH>
				<UNH1>UNH1</UNH1>
				<UNH2>
					<UNH2.1>APERAK</UNH2.1>
					<UNH2.2>D</UNH2.2>
					<UNH2.3>99A</UNH2.3>
					<UNH2.4>UN</UNH2.4>
				</UNH2>
				<UNH4>
					<UNH4.1>13</UNH4.1>
				</UNH4>
				<UNH5>
					<UNH5.1>UNH5.1</UNH5.1>
				</UNH5>
				<UNH6>
					<UNH6.1>UNH6.1</UNH6.1>
				</UNH6>
				<UNH7>
					<UNH7.1>UNH7.1</UNH7.1>
				</UNH7>
			</UNH>
			<gen1:BGM xmlns:gen1=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen2:C002 xmlns:gen2=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C00201>1</C00201>
				</gen2:C002>
				<gen3:C106 xmlns:gen3=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C10601>C10601</C10601>
				</gen3:C106>
			</gen1:BGM>
			<gen4:DTM xmlns:gen4=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen5:C507 xmlns:gen5=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C50701>10</C50701>
				</gen5:C507>
			</gen4:DTM>
			<gen6:FTX xmlns:gen6=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<FTX01>AAA</FTX01>
				<gen7:C107 xmlns:gen7=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C10701>C10701</C10701>
				</gen7:C107>
				<gen8:C108 xmlns:gen8=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C10801>C10801</C10801>
				</gen8:C108>
			</gen6:FTX>
			<gen9:CNT xmlns:gen9=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen10:C270 xmlns:gen10=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C27001>1</C27001>
					<C27002>14</C27002>
				</gen10:C270>
			</gen9:CNT>
			<gen11:DOCLoop1 xmlns:gen11=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen12:DOC xmlns:gen12=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen13:C002_2 xmlns:gen13=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C00201>1</C00201>
					</gen13:C002_2>
					<gen14:C503 xmlns:gen14=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C50301>C50301</C50301>
					</gen14:C503>
				</gen12:DOC>
				<gen15:DTM_2 xmlns:gen15=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen16:C507_2 xmlns:gen16=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C50701>10</C50701>
					</gen16:C507_2>
				</gen15:DTM_2>
			</gen11:DOCLoop1>
			<gen17:RFFLoop1 xmlns:gen17=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen18:RFF xmlns:gen18=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen19:C506 xmlns:gen19=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C50601>AAA</C50601>
					</gen19:C506>
				</gen18:RFF>
				<gen20:DTM_3 xmlns:gen20=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen21:C507_3 xmlns:gen21=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C50701>10</C50701>
					</gen21:C507_3>
				</gen20:DTM_3>
			</gen17:RFFLoop1>
			<gen22:NADLoop1 xmlns:gen22=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen23:NAD xmlns:gen23=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<NAD01>AA</NAD01>
					<gen24:C082 xmlns:gen24=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C08201>C08201</C08201>
					</gen24:C082>
					<gen25:C058 xmlns:gen25=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C05801>C05801</C05801>
					</gen25:C058>
					<gen26:C080 xmlns:gen26=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C08001>C08001</C08001>
					</gen26:C080>
					<gen27:C059 xmlns:gen27=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C05901>C05901</C05901>
					</gen27:C059>
				</gen23:NAD>
				<gen28:CTA xmlns:gen28=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen29:C056 xmlns:gen29=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C05601>C05601</C05601>
					</gen29:C056>
				</gen28:CTA>
				<gen30:COM xmlns:gen30=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen31:C076 xmlns:gen31=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C07601>C07601</C07601>
						<C07602>AA</C07602>
					</gen31:C076>
				</gen30:COM>
			</gen22:NADLoop1>
			<gen32:ERCLoop1 xmlns:gen32=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen33:ERC xmlns:gen33=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen34:C901 xmlns:gen34=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C90101>C90101</C90101>
					</gen34:C901>
				</gen33:ERC>
				<gen35:FTX_2 xmlns:gen35=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<FTX01>AAA</FTX01>
					<gen36:C107_2 xmlns:gen36=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C10701>C10701</C10701>
					</gen36:C107_2>
					<gen37:C108_2 xmlns:gen37=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C10801>C10801</C10801>
					</gen37:C108_2>
				</gen35:FTX_2>
				<gen38:RFFLoop2 xmlns:gen38=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen39:RFF_2 xmlns:gen39=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<gen40:C506_2 xmlns:gen40=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
							<C50601>AAA</C50601>
						</gen40:C506_2>
					</gen39:RFF_2>
					<gen41:FTX_3 xmlns:gen41=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<FTX01>AAA</FTX01>
						<gen42:C107_3 xmlns:gen42=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
							<C10701>C10701</C10701>
						</gen42:C107_3>
						<gen43:C108_3 xmlns:gen43=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
							<C10801>C10801</C10801>
						</gen43:C108_3>
					</gen41:FTX_3>
				</gen38:RFFLoop2>
			</gen32:ERCLoop1>
			<UNT>
				<UNT1>17</UNT1>
				<UNT2>UNH1</UNT2>
			</UNT>
		</gen0:EFACT_D99A_APERAK>
		<gen0:EFACT_D99A_APERAK xmlns:gen0=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
			<UNH>
				<UNH1>UNH1</UNH1>
				<UNH2>
					<UNH2.1>APERAK</UNH2.1>
					<UNH2.2>D</UNH2.2>
					<UNH2.3>99A</UNH2.3>
					<UNH2.4>UN</UNH2.4>
				</UNH2>
				<UNH4>
					<UNH4.1>13</UNH4.1>
				</UNH4>
				<UNH5>
					<UNH5.1>UNH5.1</UNH5.1>
				</UNH5>
				<UNH6>
					<UNH6.1>UNH6.1</UNH6.1>
				</UNH6>
				<UNH7>
					<UNH7.1>UNH7.1</UNH7.1>
				</UNH7>
			</UNH>
			<gen1:BGM xmlns:gen1=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen2:C002 xmlns:gen2=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C00201>1</C00201>
				</gen2:C002>
				<gen3:C106 xmlns:gen3=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C10601>C10601</C10601>
				</gen3:C106>
			</gen1:BGM>
			<gen4:DTM xmlns:gen4=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen5:C507 xmlns:gen5=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C50701>10</C50701>
				</gen5:C507>
			</gen4:DTM>
			<gen6:FTX xmlns:gen6=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<FTX01>AAA</FTX01>
				<gen7:C107 xmlns:gen7=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C10701>C10701</C10701>
				</gen7:C107>
				<gen8:C108 xmlns:gen8=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C10801>C10801</C10801>
				</gen8:C108>
			</gen6:FTX>
			<gen9:CNT xmlns:gen9=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen10:C270 xmlns:gen10=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<C27001>1</C27001>
					<C27002>14</C27002>
				</gen10:C270>
			</gen9:CNT>
			<gen11:DOCLoop1 xmlns:gen11=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen12:DOC xmlns:gen12=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen13:C002_2 xmlns:gen13=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C00201>1</C00201>
					</gen13:C002_2>
					<gen14:C503 xmlns:gen14=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C50301>C50301</C50301>
					</gen14:C503>
				</gen12:DOC>
				<gen15:DTM_2 xmlns:gen15=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen16:C507_2 xmlns:gen16=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C50701>10</C50701>
					</gen16:C507_2>
				</gen15:DTM_2>
			</gen11:DOCLoop1>
			<gen17:RFFLoop1 xmlns:gen17=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen18:RFF xmlns:gen18=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen19:C506 xmlns:gen19=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C50601>AAA</C50601>
					</gen19:C506>
				</gen18:RFF>
				<gen20:DTM_3 xmlns:gen20=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen21:C507_3 xmlns:gen21=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C50701>10</C50701>
					</gen21:C507_3>
				</gen20:DTM_3>
			</gen17:RFFLoop1>
			<gen22:NADLoop1 xmlns:gen22=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen23:NAD xmlns:gen23=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<NAD01>AA</NAD01>
					<gen24:C082 xmlns:gen24=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C08201>C08201</C08201>
					</gen24:C082>
					<gen25:C058 xmlns:gen25=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C05801>C05801</C05801>
					</gen25:C058>
					<gen26:C080 xmlns:gen26=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C08001>C08001</C08001>
					</gen26:C080>
					<gen27:C059 xmlns:gen27=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C05901>C05901</C05901>
					</gen27:C059>
				</gen23:NAD>
				<gen28:CTA xmlns:gen28=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen29:C056 xmlns:gen29=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C05601>C05601</C05601>
					</gen29:C056>
				</gen28:CTA>
				<gen30:COM xmlns:gen30=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen31:C076 xmlns:gen31=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C07601>C07601</C07601>
						<C07602>AA</C07602>
					</gen31:C076>
				</gen30:COM>
			</gen22:NADLoop1>
			<gen32:ERCLoop1 xmlns:gen32=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
				<gen33:ERC xmlns:gen33=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen34:C901 xmlns:gen34=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C90101>C90101</C90101>
					</gen34:C901>
				</gen33:ERC>
				<gen35:FTX_2 xmlns:gen35=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<FTX01>AAA</FTX01>
					<gen36:C107_2 xmlns:gen36=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C10701>C10701</C10701>
					</gen36:C107_2>
					<gen37:C108_2 xmlns:gen37=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<C10801>C10801</C10801>
					</gen37:C108_2>
				</gen35:FTX_2>
				<gen38:RFFLoop2 xmlns:gen38=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
					<gen39:RFF_2 xmlns:gen39=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<gen40:C506_2 xmlns:gen40=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
							<C50601>AAA</C50601>
						</gen40:C506_2>
					</gen39:RFF_2>
					<gen41:FTX_3 xmlns:gen41=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
						<FTX01>AAA</FTX01>
						<gen42:C107_3 xmlns:gen42=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
							<C10701>C10701</C10701>
						</gen42:C107_3>
						<gen43:C108_3 xmlns:gen43=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006"">
							<C10801>C10801</C10801>
						</gen43:C108_3>
					</gen41:FTX_3>
				</gen38:RFFLoop2>
			</gen32:ERCLoop1>
			<UNT>
				<UNT1>17</UNT1>
				<UNT2>UNH1</UNT2>
			</UNT>
		</gen0:EFACT_D99A_APERAK>
	</EDIFACTs>
</ns0:EDIFACTEnvelope>";
			#endregion

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(original);

			var xPathValueUpdater = new XPathValueUpdater(xmlDocument, "/*[local-name()='EDIFACTEnvelope']/*[local-name()='EDIFACTs']/*[local-name()='EFACT_D99A_APERAK']/*[local-name()='BGM']/*[local-name()='C106']/*[local-name()='C10601']", "Batch1");
			var actual = xPathValueUpdater.Update().OuterXml;

			#region expected
			var expected = @"<ns0:EDIFACTEnvelope xmlns:ns0=""http://wisetechglobal.com/eHub/Products/SGCustoms/MHAccess/2017/08/EDIFACTEnvelope""><EDIFACTs><gen0:EFACT_D99A_APERAK xmlns:gen0=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><UNH><UNH1>UNH1</UNH1><UNH2><UNH2.1>APERAK</UNH2.1><UNH2.2>D</UNH2.2><UNH2.3>99A</UNH2.3><UNH2.4>UN</UNH2.4></UNH2><UNH4><UNH4.1>13</UNH4.1></UNH4><UNH5><UNH5.1>UNH5.1</UNH5.1></UNH5><UNH6><UNH6.1>UNH6.1</UNH6.1></UNH6><UNH7><UNH7.1>UNH7.1</UNH7.1></UNH7></UNH><gen1:BGM xmlns:gen1=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen2:C002 xmlns:gen2=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C00201>1</C00201></gen2:C002><gen3:C106 xmlns:gen3=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10601>Batch1</C10601></gen3:C106></gen1:BGM><gen4:DTM xmlns:gen4=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen5:C507 xmlns:gen5=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50701>10</C50701></gen5:C507></gen4:DTM><gen6:FTX xmlns:gen6=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><FTX01>AAA</FTX01><gen7:C107 xmlns:gen7=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10701>C10701</C10701></gen7:C107><gen8:C108 xmlns:gen8=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10801>C10801</C10801></gen8:C108></gen6:FTX><gen9:CNT xmlns:gen9=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen10:C270 xmlns:gen10=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C27001>1</C27001><C27002>14</C27002></gen10:C270></gen9:CNT><gen11:DOCLoop1 xmlns:gen11=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen12:DOC xmlns:gen12=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen13:C002_2 xmlns:gen13=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C00201>1</C00201></gen13:C002_2><gen14:C503 xmlns:gen14=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50301>C50301</C50301></gen14:C503></gen12:DOC><gen15:DTM_2 xmlns:gen15=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen16:C507_2 xmlns:gen16=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50701>10</C50701></gen16:C507_2></gen15:DTM_2></gen11:DOCLoop1><gen17:RFFLoop1 xmlns:gen17=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen18:RFF xmlns:gen18=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen19:C506 xmlns:gen19=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50601>AAA</C50601></gen19:C506></gen18:RFF><gen20:DTM_3 xmlns:gen20=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen21:C507_3 xmlns:gen21=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50701>10</C50701></gen21:C507_3></gen20:DTM_3></gen17:RFFLoop1><gen22:NADLoop1 xmlns:gen22=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen23:NAD xmlns:gen23=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><NAD01>AA</NAD01><gen24:C082 xmlns:gen24=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C08201>C08201</C08201></gen24:C082><gen25:C058 xmlns:gen25=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C05801>C05801</C05801></gen25:C058><gen26:C080 xmlns:gen26=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C08001>C08001</C08001></gen26:C080><gen27:C059 xmlns:gen27=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C05901>C05901</C05901></gen27:C059></gen23:NAD><gen28:CTA xmlns:gen28=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen29:C056 xmlns:gen29=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C05601>C05601</C05601></gen29:C056></gen28:CTA><gen30:COM xmlns:gen30=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen31:C076 xmlns:gen31=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C07601>C07601</C07601><C07602>AA</C07602></gen31:C076></gen30:COM></gen22:NADLoop1><gen32:ERCLoop1 xmlns:gen32=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen33:ERC xmlns:gen33=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen34:C901 xmlns:gen34=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C90101>C90101</C90101></gen34:C901></gen33:ERC><gen35:FTX_2 xmlns:gen35=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><FTX01>AAA</FTX01><gen36:C107_2 xmlns:gen36=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10701>C10701</C10701></gen36:C107_2><gen37:C108_2 xmlns:gen37=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10801>C10801</C10801></gen37:C108_2></gen35:FTX_2><gen38:RFFLoop2 xmlns:gen38=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen39:RFF_2 xmlns:gen39=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen40:C506_2 xmlns:gen40=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50601>AAA</C50601></gen40:C506_2></gen39:RFF_2><gen41:FTX_3 xmlns:gen41=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><FTX01>AAA</FTX01><gen42:C107_3 xmlns:gen42=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10701>C10701</C10701></gen42:C107_3><gen43:C108_3 xmlns:gen43=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10801>C10801</C10801></gen43:C108_3></gen41:FTX_3></gen38:RFFLoop2></gen32:ERCLoop1><UNT><UNT1>17</UNT1><UNT2>UNH1</UNT2></UNT></gen0:EFACT_D99A_APERAK><gen0:EFACT_D99A_APERAK xmlns:gen0=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><UNH><UNH1>UNH1</UNH1><UNH2><UNH2.1>APERAK</UNH2.1><UNH2.2>D</UNH2.2><UNH2.3>99A</UNH2.3><UNH2.4>UN</UNH2.4></UNH2><UNH4><UNH4.1>13</UNH4.1></UNH4><UNH5><UNH5.1>UNH5.1</UNH5.1></UNH5><UNH6><UNH6.1>UNH6.1</UNH6.1></UNH6><UNH7><UNH7.1>UNH7.1</UNH7.1></UNH7></UNH><gen1:BGM xmlns:gen1=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen2:C002 xmlns:gen2=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C00201>1</C00201></gen2:C002><gen3:C106 xmlns:gen3=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10601>Batch1</C10601></gen3:C106></gen1:BGM><gen4:DTM xmlns:gen4=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen5:C507 xmlns:gen5=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50701>10</C50701></gen5:C507></gen4:DTM><gen6:FTX xmlns:gen6=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><FTX01>AAA</FTX01><gen7:C107 xmlns:gen7=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10701>C10701</C10701></gen7:C107><gen8:C108 xmlns:gen8=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10801>C10801</C10801></gen8:C108></gen6:FTX><gen9:CNT xmlns:gen9=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen10:C270 xmlns:gen10=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C27001>1</C27001><C27002>14</C27002></gen10:C270></gen9:CNT><gen11:DOCLoop1 xmlns:gen11=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen12:DOC xmlns:gen12=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen13:C002_2 xmlns:gen13=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C00201>1</C00201></gen13:C002_2><gen14:C503 xmlns:gen14=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50301>C50301</C50301></gen14:C503></gen12:DOC><gen15:DTM_2 xmlns:gen15=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen16:C507_2 xmlns:gen16=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50701>10</C50701></gen16:C507_2></gen15:DTM_2></gen11:DOCLoop1><gen17:RFFLoop1 xmlns:gen17=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen18:RFF xmlns:gen18=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen19:C506 xmlns:gen19=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50601>AAA</C50601></gen19:C506></gen18:RFF><gen20:DTM_3 xmlns:gen20=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen21:C507_3 xmlns:gen21=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50701>10</C50701></gen21:C507_3></gen20:DTM_3></gen17:RFFLoop1><gen22:NADLoop1 xmlns:gen22=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen23:NAD xmlns:gen23=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><NAD01>AA</NAD01><gen24:C082 xmlns:gen24=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C08201>C08201</C08201></gen24:C082><gen25:C058 xmlns:gen25=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C05801>C05801</C05801></gen25:C058><gen26:C080 xmlns:gen26=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C08001>C08001</C08001></gen26:C080><gen27:C059 xmlns:gen27=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C05901>C05901</C05901></gen27:C059></gen23:NAD><gen28:CTA xmlns:gen28=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen29:C056 xmlns:gen29=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C05601>C05601</C05601></gen29:C056></gen28:CTA><gen30:COM xmlns:gen30=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen31:C076 xmlns:gen31=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C07601>C07601</C07601><C07602>AA</C07602></gen31:C076></gen30:COM></gen22:NADLoop1><gen32:ERCLoop1 xmlns:gen32=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen33:ERC xmlns:gen33=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen34:C901 xmlns:gen34=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C90101>C90101</C90101></gen34:C901></gen33:ERC><gen35:FTX_2 xmlns:gen35=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><FTX01>AAA</FTX01><gen36:C107_2 xmlns:gen36=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10701>C10701</C10701></gen36:C107_2><gen37:C108_2 xmlns:gen37=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10801>C10801</C10801></gen37:C108_2></gen35:FTX_2><gen38:RFFLoop2 xmlns:gen38=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen39:RFF_2 xmlns:gen39=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><gen40:C506_2 xmlns:gen40=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C50601>AAA</C50601></gen40:C506_2></gen39:RFF_2><gen41:FTX_3 xmlns:gen41=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><FTX01>AAA</FTX01><gen42:C107_3 xmlns:gen42=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10701>C10701</C10701></gen42:C107_3><gen43:C108_3 xmlns:gen43=""http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006""><C10801>C10801</C10801></gen43:C108_3></gen41:FTX_3></gen38:RFFLoop2></gen32:ERCLoop1><UNT><UNT1>17</UNT1><UNT2>UNH1</UNT2></UNT></gen0:EFACT_D99A_APERAK></EDIFACTs></ns0:EDIFACTEnvelope>";
			#endregion

			Assert.AreEqual(expected, actual);
		}
	}
}
