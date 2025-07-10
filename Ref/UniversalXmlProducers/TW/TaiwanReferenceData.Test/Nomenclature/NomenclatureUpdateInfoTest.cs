using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class NomenclatureUpdateInfoTest
	{
		public class NomenclatureUpdateInfoForTest : NomenclatureUpdateInfo
		{
			public NomenclatureUpdateInfoForTest(updateInfoBean info, string filename) : base(info, filename)
			{
				this.BeforeExecuteEvent += NomenclatureUpdateInfoForTest_BeforeExecuteEvent;
			}

			private void NomenclatureUpdateInfoForTest_BeforeExecuteEvent()
			{
				SystemContext.Now = () => new DateTime(2018, 07, 20, 14, 34, 32);
			}

			protected override IWebClient GetWebClientWrapper()
			{
				var mock = new Mock<IWebClient>();
				byte[] responseEnnameBytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Nomenclature/enname1.doc"));
				mock.Setup(x => x.DownloadData("http://192.118.118.3/enname1.doc")).Returns(responseEnnameBytes);

				byte[] responseNoteBytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Nomenclature/note_8_C.txt"));
				mock.Setup(x => x.DownloadData("http://192.118.118.4/note_8_C.txt")).Returns(responseNoteBytes);

				byte[] responseNoteEnBytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Nomenclature/note_8_E.txt"));
				mock.Setup(x => x.DownloadData("http://192.118.118.4/note_8_E.txt")).Returns(responseNoteEnBytes);

				return mock.Object;
			}

			public new string GetTens(Dictionary<string, string> hs8Codes, KeyValuePair<string, string> hs)
			{
				return base.GetTens(hs8Codes, hs);
			}
		}

		[Test]
		public void TestGetTens()
		{
			var hs8Codes = new Dictionary<string, string>();
			var hs1 = new KeyValuePair<string, string>("02011010", "特殊品級屠體及半片屠體牛肉，生鮮或冷藏");
			var hs2 = new KeyValuePair<string, string>("02011090", "其他屠體及半片屠體牛肉，生鮮或冷藏");
			var hs3 = new KeyValuePair<string, string>("01012100", "馬，純種繁殖用");
			hs8Codes.Add("02011010", "特殊品級屠體及半片屠體牛肉，生鮮或冷藏");
			hs8Codes.Add("02011090", "其他屠體及半片屠體牛肉，生鮮或冷藏");
			NomenclatureUpdateInfoForTest updateInfo = new NomenclatureUpdateInfoForTest(new updateInfoBean(), "");
			Assert.AreEqual(".10", updateInfo.GetTens(hs8Codes, hs1));
			Assert.AreEqual(".20", updateInfo.GetTens(hs8Codes, hs2));

			TestDelegate testDelegate = () => updateInfo.GetTens(hs8Codes, hs3);
			Assert.That(testDelegate, Throws.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void TestExecute()
		{
			string HsSectionsAndChaptersUrl = "http://192.118.118.3/enname1.doc";
			string HsNomenclatureDescriptionUrl = "http://192.118.118.4/note_8_C.txt";
			string HsNomenclatureDescriptionEnUrl = "http://192.118.118.4/note_8_E.txt";
			string fileName = Path.Combine(Utility.TempDirectory, string.Format("{0}.xml", Guid.NewGuid().ToString()));
			var info = new updateInfoBean();
			info.downloadURL = string.Join(";", new string[] { HsSectionsAndChaptersUrl, HsNomenclatureDescriptionUrl, HsNomenclatureDescriptionEnUrl });
			var updateInfo = new NomenclatureUpdateInfoForTest(info, fileName);
			updateInfo.Run();
			var actualXml = XDocument.Load(fileName);
			string path = Path.Combine(FolderHelper.GetBinFolder(), "doc/Nomenclature/Nomenclature.xml");
			var expectedXml = XDocument.Load(path);
			Assert.AreEqual(expectedXml.ToString(), actualXml.ToString());
			File.Delete(fileName);
		}

		[Test]
		[SetCulture("en-US")]
		public void TestExecuteWhenUS()
		{
			TestExecute();
		}

		[Test]
		[SetCulture("fr-FR")]
		public void TestExecuteWhenFR()
		{
			TestExecute();
		}
	}
}
