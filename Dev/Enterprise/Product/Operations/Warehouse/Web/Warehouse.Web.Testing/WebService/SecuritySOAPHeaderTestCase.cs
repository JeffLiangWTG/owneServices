using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class SecuritySOAPHeaderTestCase : TestCase
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var securityHeader = new SecuritySOAPHeader();
			AssertNotNull(securityHeader);
			AssertNotEquals(securityHeader, new SecuritySOAPHeader());
			AssertEquals("", securityHeader.BranchCode);
			AssertEquals("", securityHeader.DepartmentCode);
			AssertEquals("", securityHeader.Password);
			AssertEquals("", securityHeader.SecurityKey);
			AssertEquals("", securityHeader.UserName);
			AssertEquals("", securityHeader.WarehouseCode);
			AssertEquals(0, securityHeader.ProcessID);
			AssertEquals("", securityHeader.DeviceID);
			AssertEquals(null, securityHeader.DeviceModelDetails);
			AssertEquals("", securityHeader.DeviceVersion);
			AssertEquals(false, securityHeader.IsAndroidDevice);
		}

		public void TestCopyConstructor()
		{
			var securityHeader = new SecuritySOAPHeader();
			securityHeader.Actor = "S1";
			securityHeader.BranchCode = "A";
			securityHeader.DepartmentCode = "B";
			securityHeader.DeviceID = "ABC";
			securityHeader.DeviceModelDetails = "DEF";
			securityHeader.DeviceVersion = "A1";
			securityHeader.DidUnderstand = true;
			securityHeader.EncodedMustUnderstand = "0";
			securityHeader.EncodedMustUnderstand12 = "1";
			securityHeader.EncodedRelay = "1";
			securityHeader.MustUnderstand = false;
			securityHeader.Password = "123";
			securityHeader.ProcessID = 10;
			securityHeader.Relay = true;
			securityHeader.Role = "S1";
			securityHeader.SecurityKey = "S2";
			securityHeader.UserName = "ABC1";
			securityHeader.WarehouseCode = "WHS1";
			securityHeader.IsAndroidDevice = true;

			var copySecurityHeader = new SecuritySOAPHeader(securityHeader);
			AssertEquals("S1", copySecurityHeader.Actor);
			AssertEquals("A", copySecurityHeader.BranchCode);
			AssertEquals("B", copySecurityHeader.DepartmentCode);
			AssertEquals("ABC", copySecurityHeader.DeviceID);
			AssertEquals("DEF", copySecurityHeader.DeviceModelDetails);
			AssertEquals("A1", copySecurityHeader.DeviceVersion);
			AssertEquals(true, copySecurityHeader.DidUnderstand);
			AssertEquals("0", copySecurityHeader.EncodedMustUnderstand);
			AssertEquals("0", copySecurityHeader.EncodedMustUnderstand12);
			AssertEquals("1", copySecurityHeader.EncodedRelay);
			AssertEquals(false, copySecurityHeader.MustUnderstand);
			AssertEquals("123", copySecurityHeader.Password);
			AssertEquals(10, copySecurityHeader.ProcessID);
			AssertEquals(true, copySecurityHeader.Relay);
			AssertEquals("S1", copySecurityHeader.Role);
			AssertEquals("S2", copySecurityHeader.SecurityKey);
			AssertEquals("ABC1", copySecurityHeader.UserName);
			AssertEquals("WHS1", copySecurityHeader.WarehouseCode);
			Assert(copySecurityHeader.IsAndroidDevice);
		}

		#endregion

		#region TestProperties

		public void TestProperties()
		{
			var securityHeader = new SecuritySOAPHeader();
			AssertEquals("", securityHeader.BranchCode);
			AssertEquals("", securityHeader.DepartmentCode);
			AssertEquals("", securityHeader.Password);
			AssertEquals("", securityHeader.SecurityKey);
			AssertEquals("", securityHeader.UserName);
			AssertEquals("", securityHeader.WarehouseCode);
			AssertEquals(0, securityHeader.ProcessID);
			AssertEquals("", securityHeader.DeviceID);
			AssertEquals(null, securityHeader.DeviceModelDetails);
			AssertEquals("", securityHeader.DeviceVersion);
			AssertEquals(false, securityHeader.IsAndroidDevice);

			var heartbeatID = Guid.NewGuid();
			securityHeader.BranchCode = "BRN";
			securityHeader.DepartmentCode = "DPR";
			securityHeader.Password = "PASSWORD";
			securityHeader.SecurityKey = "SECURITYKEY";
			securityHeader.UserName = "AAA";
			securityHeader.WarehouseCode = "WHS";
			securityHeader.ProcessID = 123;
			securityHeader.DeviceID = "TESTDEVICE";
			securityHeader.DeviceModelDetails = "MAN:Zebra|MDL:TC77|OS:AND";
			securityHeader.DeviceVersion = "11.2.1.1";
			securityHeader.IsAndroidDevice = true;
			AssertEquals("BRN", securityHeader.BranchCode);
			AssertEquals("DPR", securityHeader.DepartmentCode);
			AssertEquals("PASSWORD", securityHeader.Password);
			AssertEquals("SECURITYKEY", securityHeader.SecurityKey);
			AssertEquals("AAA", securityHeader.UserName);
			AssertEquals("WHS", securityHeader.WarehouseCode);
			AssertEquals(123, securityHeader.ProcessID);
			AssertEquals("TESTDEVICE", securityHeader.DeviceID);
			AssertEquals("MAN:Zebra|MDL:TC77|OS:AND", securityHeader.DeviceModelDetails);
			AssertEquals("11.2.1.1", securityHeader.DeviceVersion);
			Assert(securityHeader.IsAndroidDevice);
		}

		#endregion
	}
}
