using NUnit.Framework;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Util.Testing
{
	sealed class ServerParametersTest : TestCase
	{
		public void TestAll()
		{
			const string sampleData = @"
			Crimson have no idea what valid XML looks like
			some <invalidRootXML/> <another>
			<whatever>
			<FSUSERID>v13t002</FSUSERID><FSUSERHOME>/fshome/sftphome/</FSUSERHOME><FSUPLOAD>/mhxupload/</FSUPLOAD><FSDOWNLOAD>/mhxdownload/</FSDOWNLOAD><FSHOSTNAME>txtrialsftp.tradexchange.gov.sg</FSHOSTNAME><FSHOSTPORT>22</FSHOSTPORT><RSAKEY>temp</RSAKEY><SALT>34GQLuw39OUM6iOnkWNO/g==</SALT><KEYLENGTH>256</KEYLENGTH><WSUUID>UNHDLRgVNzF/B6suNswlZErL/Ycpoyif56hlYrR7cqXDAcel74P4saZavIIpE7tW</WSUUID>
			</whatever>";
			const string soapDownloadDirectory = "/fshome/sftphome/<<LOGIN>>/mhxdownload/";
			var sp = new ServerParameters(sampleData);
			CombineAssertions(() =>
			{
				AssertEquals("v13t002", sp.FsUserId);
				AssertEquals("/fshome/sftphome/", sp.FsUserHome);
				AssertEquals("/mhxdownload/", sp.FsDownload);
				AssertEquals("34GQLuw39OUM6iOnkWNO/g==", sp.Salt);
				AssertEquals("UNHDLRgVNzF/B6suNswlZErL/Ycpoyif56hlYrR7cqXDAcel74P4saZavIIpE7tW", sp.WsUuid);
				AssertEquals("/fshome/sftphome/v13t002/mhxdownload/", sp.FullDownloadPathCalculated("Daniel", soapDownloadDirectory));
				sp = new ServerParameters("bunch of crap");
				AssertEquals("/fshome/sftphome/Daniel/mhxdownload/", sp.FullDownloadPathCalculated("Daniel", soapDownloadDirectory));
			});
		}
	}
}
