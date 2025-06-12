using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using CargoWise.eHub.DataModel.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.DataModel.Tests.Common
{
	[TestClass]
	public class SerializationTests
	{
		[TestMethod]
		public void eHubCertificate_Serializable()
		{
			var cert = new eHubCertificate();
			cert.CE_BinaryContainer = new byte[]{ 0x1, 0x2, 0x3, 0x4, 0x5 };
			cert.CE_Password = "Password";
			using (var stream = new MemoryStream())
			{
				var binaryF = new BinaryFormatter();
				binaryF.Serialize(stream, cert);
				stream.Position = 0;
				var deserializedObject = binaryF.Deserialize(stream) as eHubCertificate;
				Assert.AreEqual(deserializedObject.CE_PK, cert.CE_PK);
				Assert.IsTrue(deserializedObject.CE_BinaryContainer
					.SequenceEqual(cert.CE_BinaryContainer));
				Assert.AreEqual(deserializedObject.CE_Password, cert.CE_Password);
			}

		}
	}
}
