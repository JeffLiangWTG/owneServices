using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class PSADataParserFixture
	{
		[Test]
		public void Output()
		{
			var rawIMORecords = new List<UNDGSubstance>();

			_webScraperMock.Setup(x => x.ScrapeWithRetriesAsync(It.IsAny<Uri>(), 3)).ReturnsAsync(expectedHtml);
			rawIMORecords.Add(new UNDGSubstance
			{
				DG_UNNO = "0004",
				DG_Variant = "a",
				DG_Class = "1.1D",
				DG_PSN = "AMMONIUM PICRATE",
				DG_PG = ""
			});
			var pSARecords = PSAGroupParser.GetPSARecordsAsync(rawIMORecords, _webScraperMock.Object).GetAwaiter().GetResult();

			var uNDGWithPSARecords = PSAGroupParser.MergePSARecordsToUNDGSubstances(rawIMORecords, pSARecords);
			var writer = PSAXmlWriterConfiguration.GetXmlWriter();
			writer.SetPublicationTime(DateTime.MinValue);
			XmlWriterHelper.ExportToXml(writer, uNDGWithPSARecords, _dumpPath);

			var result = File.ReadAllText(_dumpPath);
			var expected = File.ReadAllText(_expectedXmlFilePath).TrimEnd();

			Assert.That(result, Is.EqualTo(expected));
		}

		/*
		 * The below test cannot be used in DAT due to use of external resources.
		 * Retaining here for future dev convenience
		 * 
		[Test]
		public void SeleniumOutput()
		{
			var url = "https://www.portnet.com/DGWebPublic/com/pn2/dg/web/newdgchemicalpublic/searchChemicalList.do?%7bactionForm.searchBean.unNoFr%7d=0320";

			using (var webDriverHelper = new WebDriverHelper())
			{
				var result = webDriverHelper.GetWebPage(url);
				Assert.That(result.Contains($"<span>0320</span>", StringComparison.OrdinalIgnoreCase));
			}
		}
		*/

		[SetUp]
		public void Setup()
		{
			_binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_dumpPath = Path.Combine(_binPath, @"TestFiles\PSAUNDG Result.xml");
			_expectedXmlFilePath = Path.Combine(_binPath, @"TestFiles\PSAUNDG Expected.xml");
			_webScraperMock = new Mock<IWebScraper>();
		}

		string _dumpPath;
		string _binPath;
		string _expectedXmlFilePath;
		const string expectedHtml = @"
<!--PORTNET II web page template - version 1.6.0 -->
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"">







<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN""
>
<html lang=""en"">
<head>
    <base href=""https://www.portnet.com/DGWebPublic/com/pn2/dg/web/newdgchemicalpublic/restrSearchChemicalList.jsp"">
    <title>PORTNET - DG Substance Search Results</title>    
    <link href=""/DGWebPublic/resources/css/styles.css"" type=""text/css"" rel=""stylesheet""/>
    <script src=""/WLWWeb/common.js""></script></head>
  <body bgcolor=""#fafafa"" onload=""postRender(this);"" style=""margin:0px"">
  
  <iframe id=""rpc"" name=""rpc"" style=""width:0px; height:0px; border: 0px"" src=""""></iframe>

    <table cellspacing=0 cellpadding=5 border=0 width=""100%"" style=""border-bottom:1px solid #cccccc""><tr>
    <td align=""left"" style=""border:0px"" valign=""bottom""><div class=""pagehead"">DG Substance Search Results</div></td>
    <td align=""right"" style=""border:0px"" valign=""bottom"" nowrap><small>Zoom:<input type=text name=""pagezoom"" value=""100"" maxlength=""3"" size=""1"" onchange=""changeDocumentZoom(this.value)"">%&nbsp;
    <a href="""" onclick=""window.focus(); window.print(); return false""><img src=""/DGWebPublic/resources/images/print.gif"" height=16 width=16 border=0>&nbsp;Print</a>
    &nbsp;&nbsp;&nbsp;06-06-2020 14:44:32 SGT
    </small></td></tr></table><br>
    
    
    
    <form name=""chemicalFormBean"" action=""/DGWebPublic/com/pn2/dg/web/newdgchemicalpublic/backToSearchChemical.do;JSESSIONID_DGWebPublic=9QqBpb7Qt1c2JNp3n1cGpCJ2lQKnLz8trnYdT2LPxNd1qrxT1mBq!-51123494!-1494920789"" method=""post"">
        <table border=""0"" width=""700"" align=""center"">
    <tr valign=""top"">
    <td style=""border:0px"">

        <input type=""hidden"" id=""{actionForm.searchBean.unNoFr}"" name=""{actionForm.searchBean.unNoFr}"" value=""0004"">
        <br>
            <table cellSpacing=0 cellPadding=4 width=700 border=0 class=""altrows"">
            
                    <tr>
                        <td class=""tablehead"" width=""7%"">
                        <a class=""tooltip"" href="""" ><b>UN<br>No.</b>
                        <div>UN Number</div>
                        </a>
                        </td>
                        <td class=""tablehead"" width=""7%"">
                        <a class=""tooltip"" href="""" ><b>IMO<br>Class</b>
                        <div>IMO Class</div>
                        </a>
                        </td>
                        <td class=""tablehead"" width=""35%"">
                        <a class=""tooltip"" href="""" ><b>Proper Shipping<br>Name<br></b>
                        <div>Proper Shipping Name</div>
                        </a>
                        </td>
                        <td class=""tablehead"" width=""7%"">
                        <a class=""tooltip"" href="""" ><b>FP<br>from<br></b>
                        <div>Flash Point Lower Limit</div>
                        </a>
                        </td>
                        <td class=""tablehead"" width=""7%"">
                        <a class=""tooltip"" href="""" ><b>FP<br>to<br></b>
                        <div>Flash Point Upper Limit</div>
                        </a>
                        </td>
                        <td class=""tablehead"" width=""7%"">
                        <a class=""tooltip"" href="""" ><b>Pkg<br>Grp</b>
                        <div>Packing Group</div>
                        </a>
                        </td>
                        <td class=""tablehead"" width=""7%"">
                        <a class=""tooltip"" href="""" ><b>PSA<br>Grp</b>
                        <div>PSA Group</div>
                        </a>
                        </td>
                        <td class=""tablehead"" width=""7%"">
                        <a class=""tooltip"" href="""" ><b>Wt<br>Restr</b>
                        <div>Weight Restricted</div>
                        </a>
                        </td>
                        <td class=""tablehead"" width=""16%"">
                        <a class=""tooltip"" href="""" ><b>Special<br>Details</b>
                        <div>Special Details</div>
                        </a>
                        </td>
                    </tr>
                
                    <tr>
                        <td class=""tablebody"" width=""7%"" align=""center""><span>0004</span></td>
                        <td class=""tablebody"" width=""7%"" align=""center""><span>1.1</span><span>D</span></td>
                        <td class=""tablebody"" width=""35%"" align=""left""><span>AMMONIUM PICRATE</span></td>
                        <td class=""tablebody"" width=""7%"" align=""center""><span>&nbsp;</span></td>
                        <td class=""tablebody"" width=""7%"" align=""center""><span>&nbsp;</span></td>
                        <td class=""tablebody"" width=""7%"" align=""center""><span>-</span></td>
                        <td class=""tablebody"" width=""7%"" align=""center""><span>1</span></td>
                        <td class=""tablebody"" width=""7%"" align=""center""><span>Y</span></td>
                        <td class=""tablebody"" width=""16%"" align=""center"">
                            
                            <span></span>
                        </td>
                    </tr>
                
        </table>

                </td></tr></table>
    <br>
    <table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"" align=""center"">
        <tr>
            <td align=""center"" class=""rowhead"">
            <div align=""center"">    
        <input type=""submit"" name=""actionOverride:backToSearchChemical"" value=""<<Back"">
                    </div>
            </td>
        </tr>            
    </table>
    <br>    
        <!--DG related website--> <br>
        <table border=""0"" cellpadding=""3"" cellspacing=""0"" width=700 align=""center"">
            <tr>
                <td class=""sectionhead"" >Legend for PSA Group</td>
            </tr>
            <tr>
                <td valign=""top"" >
                <p><br>
                <table border=""0"">
                    <tr>
                        <td valign=""top"" width=""10%"" nowrap>1S, 2S, 2A, 2B, 2F, 3</td>
                        <td valign=""top"" width=""2%"" nowrap>&nbsp;-&nbsp;</td>
                        <td nowrap>DG is recommended for storage.</td>
                    </tr>
                    <tr>
                        <td valign=""top"" width=""10%"" nowrap>1D, 2</td>
                        <td valign=""top"" width=""2%"" nowrap>&nbsp;-&nbsp;</td>
                        <td nowrap>DG is not recommended for storage.</td>
                    </tr>
                    <tr>
                        <td valign=""top"" colspan=""3"">For Conventional Terminals, all DGs are not recommended for storage.</td>
                    </tr>
                </table>
                </td>
            </tr>
            <tr>
                <td class=""sectionhead"" ><br>Legend for Special Details</td>
            </tr>
            <tr>
                <td valign=""top"" >
                <p><br>
                <table border=""0"">
                    <tr>
                        <td valign=""top"">77</td>
                        <td valign=""top""> -</td>
                        <td nowrap>IF TRANSPORTED IN ISOTANKS, DIRECT HANDLING IS REQUIRED.</td>
                    </tr>
                    <tr>
                        <td valign=""top"">G1</td>
                        <td valign=""top""> -</td>
                        <td nowrap>IF TOTAL WEIGHT OF DG EXCEEDS 250KG, DIRECT HANDLING IS REQUIRED.</td>
                    </tr>
                    <tr>
                        <td valign=""top"">G2</td>
                        <td valign=""top""> -</td>
                        <td nowrap>IF TOTAL WEIGHT OF DG EXCEEDS 500KG, A DG WEIGHT ADMINISTRATION FEE OF SGD100 PER CONTAINER <br>HANDLING IS APPLICABLE.</td>
                    </tr>
                    <tr>
                        <td valign=""top"">G3</td>
                        <td valign=""top""> -</td>
                        <td nowrap>IF TEMPORARY STORAGE IS REQUIRED, PLEASE ARRANGE FOR SPECIAL MONITORING SERVICE WITH CHEMCARE <br>OR APPLY DIRECTLY VIA PORTNET.</td>
                    </tr>
                    <tr>
                        <td valign=""top"">G4</td>
                        <td valign=""top""> -</td>
                        <td nowrap>DG IS NOT ALLOWED FOR TEMPORARY STORAGE. DIRECT HANDLING IS REQUIRED. FOR EXCEPTIONAL <br>HANDLING, PLEASE CONTACT CHEMCARE.</td>
                    </tr>
                    <tr>
                        <td valign=""top"">G5</td>
                        <td valign=""top""> -</td>
                        <td nowrap>THIS PSA GROUP IS FINAL.</td>
                    </tr>
                    <tr>
                        <td valign=""top"">G6</td>
                        <td valign=""top""> -</td>
                        <td nowrap>IF TRANSPORTED IN ISOTANKS, VESSEL IS PROHIBITED AT ALL PSA BERTHS.</td>
                    </tr>
                    <tr>
                        <td valign=""top"">G7</td>
                        <td valign=""top""> -</td>
                        <td nowrap>MAXIMUM ALLOWABLE DG WEIGHT PER VESSEL BERTHING IS 500KG.</td>
                    </tr>
                    <tr>
                        <td valign=""top"">G8</td>
                        <td valign=""top""> -</td>
                        <td nowrap>PLEASE SUBMIT SUPPORTING DOCUMENTS TO AUTHORITIES AND/OR OBTAIN APPROVALS BEFORE ARRIVAL OF <br>THE VESSEL IN PORT OR LOADING.</td>
                    </tr>
                </table>
                <br>
                </p>
                </td>
            </tr>
        </table>
        <!--DG related website-->
    </form>
    
  </body></html>
";
		Mock<IWebScraper> _webScraperMock;
	}
}
