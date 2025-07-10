using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Enterprise.Edifact.D09B.Elements;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor.Test
{
	[TestFixture]
	public class EDIFACTMessageProcessorFixture : BaseMessageProcessorFixture
	{
		[Test]
		public void ProcessCHSCODE_CPDCODE()
		{
			AssertProcess("SGCLASET Test CHSCODE_CPDCODE.txt", AssertCHSCODE_CPDCODE);
		}

		void AssertCHSCODE_CPDCODE(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(2, noOfUpdates);
			AssertSameDataAsDirectory("CHSCODE_CPDCODE", xmlFiles);
		}

		[Test]
		public void ProcessCPCCODE()
		{
			AssertProcess("SGCLASET Test CPCCODE.txt", AssertCPCCODE);
		}

		void AssertCPCCODE(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(44, noOfUpdates);
			AssertSameDataAsDirectory("CPCCODE", xmlFiles);
		}

		[Test]
		public void ProcessCPDCODE()
		{
			AssertProcess("SGCLASET Test CPDCODE.txt", AssertCPDCODE);
		}

		void AssertCPDCODE(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(3, noOfUpdates);
			AssertSameDataAsDirectory("CPDCODE", xmlFiles);
		}

		[Test]
		public void ProcessCTYCODE_CPCCODE()
		{
			AssertProcess("SGCLASET Test CTYCODE_CPCCODE.txt", AssertCTYCODE_CPCCODE);
		}

		void AssertCTYCODE_CPCCODE(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(4, noOfUpdates);
			AssertSameDataAsDirectory("CTYCODE_CPCCODE", xmlFiles);
		}

		[Test]
		public void ProcessCTYCODE_PRTCODE()
		{
			AssertProcess("SGCLASET Test CTYCODE_PRTCODE.txt", AssertCTYCODE_PRTCODE);
		}

		void AssertCTYCODE_PRTCODE(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(1, noOfUpdates);
			AssertSameDataAsDirectory("CTYCODE_PRTCODE", xmlFiles);
		}

		[Test]
		public void ProcessCTYCODE_PRTCODE2()
		{
			AssertProcess("SGCLASET Test CTYCODE_PRTCODE 2.txt", AssertCTYCODE_PRTCODE2);
		}

		void AssertCTYCODE_PRTCODE2(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(1, noOfUpdates);
			AssertSameDataAsDirectory("CTYCODE_PRTCODE 2", xmlFiles);
		}

		[Test]
		public void ProcessLOCCODE_CPDCODE()
		{
			AssertProcess("SGCLASET Test LOCCODE_CPDCODE.txt", AssertLOCCODE_CPDCODE);
		}

		void AssertLOCCODE_CPDCODE(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(6, noOfUpdates);
			AssertSameDataAsDirectory("LOCCODE_CPDCODE", xmlFiles);
		}

		[Test]
		public void ProcessLOCCODE_CPCCODE()
		{
			AssertProcess("SGCLASET Test LOCCODE_CPCCODE.txt", AssertLOCCODE_CPCCODE);
		}

		void AssertLOCCODE_CPCCODE(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(3, noOfUpdates);
			AssertSameDataAsDirectory("LOCCODE_CPCCODE", xmlFiles);
		}

		[Test]
		public void ProcessDataDoesNotStartWithUNH()
		{
			AssertProcessData("UNA:+.? 'UNB+UNOA:4+DCST.DCST401:ZZ+V13T.V13T001:ZZ+20190109:2000+1++CLASET'" + GetCommodityTxt(false), AssertProcessDataDoesNotStartWithUNH);
		}

		string GetCommodityTxt(bool isTerminated, bool addReleaseDate = true)
		{
			return string.Format(CultureInfo.InvariantCulture, @"UNH+1|00001+CLASET:D:09B:UN:041+CUSCOM'
BGM+273++9'
RFF+MS:DCST.DCST401'
{1}RFF+ACW:00015'
DTM+771:20181228:102'
VLI+CPDCODE:AK'
SCD+2+PC201901091104057901'
STS++{0}'
DTM+7:20190109000000SST:304'
DTM+36:99991231235959SST:304'
ATT+ZZZ++SCDAMC2621L2'
ATT+ZZZ++29319090'
IDE+1'
ATT+ZZZ'
CAV+::SC'
FTX+ADE+++CD:SCD'
FTX+ADE+++DES:SINGAPORE CIVIL DEFENCE FORCE'
FTX+ADE+++CTL:IM'
FTX+ADE+++UOM:KG'
UNT+{2}+1|00001'", isTerminated ? StatusDescriptionCodeList.Terminated : StatusDescriptionCodeList.Added, addReleaseDate ? @"RFF+RE:00001'
DTM+771:20190102:102'
" : "", addReleaseDate ? "22" : "19");
		}

		void AssertProcessDataDoesNotStartWithUNH(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(2, noOfUpdates);
			AssertSameDataAsDirectory("ProcessCommodity", xmlFiles);
		}

		[Test]
		public void ProcessTerminatedCommodity()
		{
			AssertProcessData(GetCommodityTxt(true), AssertProcessCommodityTerminated);
		}

		void AssertProcessCommodityTerminated(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(2, noOfUpdates);
			AssertSameDataAsDirectory("ProcessCommodityTerminated", xmlFiles);
		}

		[Test]
		public void ProcessTerminatedCommodityWithoutReleaseDate()
		{
			AssertProcessData(GetCommodityTxt(true, false), AssertProcessTerminatedCommodityWithoutReleaseDate);
		}

		void AssertProcessTerminatedCommodityWithoutReleaseDate(FileInfo[] xmlFiles, int noOfUpdates)
		{
			Assert.AreEqual(2, noOfUpdates);
			AssertSameDataAsDirectory("ProcessCommodityTerminatedWithoutReleaseDate", xmlFiles);
		}

		protected override string ContentType => DataSourceConstants.ContentType.Edifact;
	}
}
