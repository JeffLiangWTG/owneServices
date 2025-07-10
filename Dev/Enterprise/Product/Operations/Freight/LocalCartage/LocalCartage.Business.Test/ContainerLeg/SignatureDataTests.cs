using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class SignatureDataTests : TestCaseWithFactory
	{
		public void TestSignatureData()
		{
			TransportRegistry.Instance.ShowLegSignatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var leg = Factory.New<CommonCartageLeg>();
			var stm = Factory.New<StmData>();
			var data = new SignatureData(leg);
			Factory.Save();
			AssertEquals("Precondition. Should not be valid.", false, data.HasValidSignature);
			AssertEquals("Precondition. Should not display the signature.", false, data.ShowSignature);
			stm.SD_Owner = leg.PK;
			Factory.Save();
			AssertEquals("Should not be valid until signature has data.", false, data.HasValidSignature);
			AssertEquals("Should not display the signature when data is not valid.", false, data.ShowSignature);
			stm.SD_BinaryValue = CargoWise.Types.ZBlob.FromAscii("signatureBytes");
			Factory.Save();
			AssertEquals("Should be valid when signature has data.", true, data.HasValidSignature);
			AssertEquals("Should display the signature when registry value is true.", true, data.ShowSignature);
			TransportRegistry.Instance.ShowLegSignatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Should be valid when signature has data.", true, data.HasValidSignature);
			AssertEquals("Should not display the signature when registry value is false.", false, data.ShowSignature);
		}

		public void TestUpdateSignatureData()
		{
			var leg = Factory.New<CommonCartageLeg>();
			var stm = Factory.New<StmData>();
			var signature = new SignatureData(leg);
			Factory.Save();
			AssertEquals("Precondition. Should not be valid.", false, signature.HasValidSignature);
			AssertEquals("Precondition. Should not display the signature.", false, signature.ShowSignature);
			var departmentGuid = Guid.NewGuid();
			var signatureBytes = ZBlob.FromAscii("signatureBytes");
			signature.SetSignatureData(signatureBytes, departmentGuid);
			Factory.Save();
			var storedSignature = new SignatureData(leg);
			AssertEquals("Should be valid when signature has data.", true, storedSignature.HasValidSignature);
			var storedSignatureData = storedSignature.GetSignatureData();
			AssertEquals("Signature bytes should be the same", signatureBytes, storedSignatureData.SD_BinaryValue);
			AssertEquals("Signature department ID should be the same", departmentGuid, storedSignatureData.SD_DepartmentGuid);
			signature.SetSignatureData(null, Guid.Empty);
			Factory.Save();
			var storedSignature2 = new SignatureData(leg);
			AssertEquals("Should not be valid when signature has no data.", false, storedSignature2.HasValidSignature);
		}

		public void TestCallsSignatureDrawer()
		{
			using (TransportRegistry.Instance.ShowLegSignatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var signatureDrawerMock = new Mock<ISignatureDrawer>();
				signatureDrawerMock.Setup(x => x.DrawSignature(It.IsAny<IEnumerable<Point[]>>(), It.IsAny<Graphics>(), Pens.Black));
				var leg = Factory.New<CommonCartageLeg>();
				var stm = Factory.New<StmData>();
				var signature = new SignatureData(leg, signatureDrawerMock.Object);
				var departmentGuid = Guid.NewGuid();
				var signatureBytes = GetValidSignatureData();
				signature.SetSignatureData(signatureBytes, departmentGuid);
				var bitmap = signature.GetBitmap();
				signatureDrawerMock.VerifyAll();
				Assert("Tests must have an assert and the verify above does not count as an assert. If it fails though it throws triggering a test failure", true);
			}
		}

		public ZBlob GetValidSignatureData()
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(200); // width
				writer.Write(100); // height
				writer.Write(3); // number of lines
				writer.Write(3); // number of points for 1st line
				writer.Write(20); // line 1, point 1, x
				writer.Write(20); // line 1, point 1, y
				writer.Write(20); // line 1, point 2, x
				writer.Write(80); // line 1, point 2, y
				writer.Write(30); // ...
				writer.Write(80);
				writer.Write(2); // number of points for 2nd line
				writer.Write(50);
				writer.Write(20);
				writer.Write(50);
				writer.Write(80);
				writer.Write(1); // number of points for 3rd line
				writer.Write(50);
				writer.Write(20);
				return new ZBlob(stream.ToArray());
			}
		}
	}
}
