using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Org.BouncyCastle.OpenSsl;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia.Testing
{
	public class CSRGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateCSR()
		{
			var csrGenerator = new CSRGenerator(branch) as ICSRGenerator;
			(var privateKey, var csrData) = csrGenerator.Generate();
			Assert(privateKey.StartsWith("-----BEGIN EC PRIVATE KEY-----"));
			Assert(csrData.StartsWith("-----BEGIN CERTIFICATE REQUEST-----"));
			using (TextReader textReader = new StringReader(csrData))
			{
				var reader = new PemReader(textReader);
				var req = reader.ReadObject() as Org.BouncyCastle.Pkcs.Pkcs10CertificationRequest;
				var info = req.GetCertificationRequestInfo();

				var cw1RgistrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				var serverCode = cw1RgistrationKey.ServerCode;
				AssertEquals(string.Format("C=SA,O=My Test Proxy Org,OU=Test Branch,CN=Cargowise EDI{0}GBX", serverCode), info.Subject.ToString());
				var requestExtension = info.Attributes.First().ToAsn1Object().ToString();

				var templateNameHex = "5052455a415443412d436f64652d5369676e696e67";
				AssertEquals("PREZATCA-Code-Signing", ConvertHex(templateNameHex));
				var snHex = "312d576973657465636820476c6f62616c7c322d436172676f776973657c332d4544494544494441542d47425831";
				AssertEquals("1-Wisetech Global|2-Cargowise|3-EDIEDIDAT-GBX1", ConvertHex(snHex));
				var categoryHex = "4c6f67697374696373205365727669636573";
				AssertEquals("Logistics Services", ConvertHex(categoryHex));
				var addressHex = "41444452455353204c494e4520312041444452455353204c494e452032205445535420434954592053415544492041524142494131";
				AssertEquals("ADDRESS LINE 1 ADDRESS LINE 2 TEST CITY SAUDI ARABIA1", ConvertHex(addressHex));
				var regNumHex = "3132332031323320313233203132";
				AssertEquals("123 123 123 12", ConvertHex(regNumHex));

				//OID 1.2.840.113549.1.9.14 => pkcs.pkcs-9.extensionRequest
				//OID 1.3.6.1.4.1.311.20.2 => microsoft.20.2
				//OID 2.5.29.17 => ds.certificateExtension.subjectAltName
				AssertEquals(requestExtension, $"[1.2.840.113549.1.9.14, [[[1.3.6.1.4.1.311.20.2, #1315{templateNameHex}], [2.5.29.17, #3081c9a481c63081c33136303406035504040c2d{snHex}1e301c060a0992268993f22c6401010c0e{regNumHex}310d300b060355040c0c0431303030313d303b060355041a0c34{addressHex}1b3019060355040f0c12{categoryHex}]]]]");
			}
			Assert(csrData.EndsWith("-----END CERTIFICATE REQUEST-----"));
		}

		string ConvertHex(string hexString)
		{
			string ascii = string.Empty;
			for (int i = 0; i < hexString.Length; i += 2)
			{
				string hs = string.Empty;

				hs = hexString.Substring(i, 2);
				uint decval = System.Convert.ToUInt32(hs, 16);
				char character = System.Convert.ToChar(decval);
				ascii += character;
			}
			return ascii;
		}

		protected override void SetUp()
		{
			base.SetUp();
			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "GBX";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_FullName = "My Test Proxy Org";
			orgProxy.MainAddress.OA_Address1 = "Address Line 1";
			orgProxy.MainAddress.OA_Address2 = "Address Line 2";
			orgProxy.MainAddress.OA_City = "Test City";
			orgProxy.MainAddress.OA_State = "02";
			orgProxy.MainAddress.OA_RN_NKCountryCode = "SA";
			orgProxy.OH_RL_NKClosestPort = "SAABT";
			orgProxy.PrimaryRegistrationNumber.NumberTypeForDisplay = "SA:VAT";
			orgProxy.PrimaryRegistrationNumber.Number = "123 123 123 12";
			branch.GB_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
		}
		GlbBranch branch;
	}
}
