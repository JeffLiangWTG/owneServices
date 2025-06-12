using System;
using System.IO;
using System.Text;
using Hawking.Elk.Common.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hawking.Elk.Common.Tests.Config
{
    [TestClass]
    public class IniFileFixtures
    {
        [TestMethod]
        public void TestLoadIniFile()
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine("key1=1");
            strBuilder.AppendLine("key2=some value");
            strBuilder.AppendLine("key3 =0.123");
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, strBuilder.ToString());

            var iniFile = new IniFile();
            iniFile.Load(filePath);
            Assert.AreEqual(iniFile.KeyValueDictionary["key1"], 1);
            Assert.AreEqual(iniFile.KeyValueDictionary["key2"], "some value");
            Assert.AreEqual(iniFile.KeyValueDictionary["key3"], 0.123);
        }

        [TestMethod]
        public void TestUpdateAndSave()
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine("key1=1");
            strBuilder.AppendLine("key2=some value");
            strBuilder.AppendLine("key3 =0.123");
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, strBuilder.ToString());

            var iniFile = new IniFile();
            iniFile.Load(filePath);
            Assert.AreEqual(iniFile.KeyValueDictionary["key1"], 1);
            Assert.AreEqual(iniFile.KeyValueDictionary["key2"], "some value");
            Assert.AreEqual(iniFile.KeyValueDictionary["key3"], 0.123);

            iniFile.KeyValueDictionary["key3"] = 3.5;
            iniFile.UpdateAndSave();
            iniFile.Load(filePath);
            Assert.AreEqual(iniFile.KeyValueDictionary["key3"], 3.5);
        }

        [TestMethod]
        public void TestUpdateAndSaveEmptyFile()
        {
            var filePath = Path.GetTempFileName();

            var iniFile = new IniFile();
            iniFile.Load(filePath);

            iniFile.KeyValueDictionary["key3"] = 3.5;
            iniFile.UpdateAndSave();
            iniFile.Load(filePath);
            Assert.AreEqual(iniFile.KeyValueDictionary["key3"], 3.5);
        }
    }
}
