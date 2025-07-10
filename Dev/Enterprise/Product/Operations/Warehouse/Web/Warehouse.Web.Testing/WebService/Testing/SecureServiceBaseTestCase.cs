using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Data;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.Foundation.Cryptography;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public abstract class SecureServiceBaseTestCase<T> : BaseServiceTestCase<T>
			where T : SecureService, new()
	{
		public void TestShouldDisposeConnectionWhenDisposeWebService()
		{
			int c1Hash = 0;
			int c2Hash = 0;
			var thread = new Thread(() => // just to force not use main connection (like web request)
			{
				using (GetNewWebService())
				{
					c1Hash = Db.Connection.GetHashCode();
				}
				using (GetNewWebService())
				{
					c2Hash = Db.Connection.GetHashCode();
				}
			});

			thread.Start();
			thread.Join();

			AssertNotEquals("it should close the first connection and open new connection.", c1Hash, c2Hash);
		}

		#region AssertSuccessfulResponse

		protected static void AssertSuccessfulResponse(WebServiceResponse response, SecureService service)
		{
			AssertNotEquals("", response.SecurityKey);
			var generatedKey = Utilities.GenerateSecurityToken(service.SecurityHeader.UserName, service.SecurityHeader.Password,
					service.SecurityHeader.BranchCode, service.SecurityHeader.DepartmentCode);
			AssertSecurityCodeApproximateEqual(generatedKey, response.SecurityKey);
			AssertEquals("Ensure that this method verifies login credentials", true, service.AllowedToRunServiceHasBeenCalled);
		}

		protected static void AssertSuccessfulResponseWithNoErrors(WebServiceResponse response, SecureService service)
		{
			AssertSuccessfulResponse(response, service);
			AssertEquals(null, response.ErrorMessage);
			AssertEquals(ErrorTypes.None, response.Error);
		}

		static internal void AssertSecurityCodeApproximateEqual(string expected, string actual)
		{
			var decrypter = new AESCryptographicProvider(LoginHelper.CryptographyKey.Value, LoginHelper.CryptographyIv.Value);
			var expectedDecoded = Encoding.Unicode.GetString(decrypter.Decrypt(Convert.FromBase64String(expected)));
			var actualDecoded = Encoding.Unicode.GetString(decrypter.Decrypt(Convert.FromBase64String(actual)));

			const string dateRegex = @"20\d+$";

			var expectedTruncated = Regex.Replace(expectedDecoded, dateRegex, string.Empty, RegexOptions.None);
			var actualTruncated = Regex.Replace(actualDecoded, dateRegex, string.Empty, RegexOptions.None);
			AssertEquals("Security codes should equal, ignoring date-time", expectedTruncated, actualTruncated);
		}

		#endregion

		#region AssertBusinessValidationError

		protected static void AssertBusinessValidationError(SecureService webService, string assertMsg, string expectedExceptionMessage, WebServiceResponse response)
		{
			AssertSuccessfulResponse(response, webService);
			AssertEquals(assertMsg, expectedExceptionMessage, response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		protected static void AssertBusinessValidationError(SecureService webService, string expectedExceptionMessage, WebServiceResponse response)
		{
			AssertSuccessfulResponse(response, webService);
			AssertEquals(expectedExceptionMessage, response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion

		#region PopulateSecurityHeader

		protected void PopulateSecurityHeader(SecureService service)
		{
			service.SecurityHeader = new SecuritySOAPHeader();
			service.SecurityHeader.BranchCode = EnvProxy.Instance.CurrentBranch.Code;
			service.SecurityHeader.DepartmentCode = EnvProxy.Instance.CurrentDepartment.Code;

			service.SecurityHeader.UserName = User.SupportUserName;
			service.SecurityHeader.Password = GetEncryptedText(User.MasterPassword);

			service.SecurityHeader.SecurityKey = "123";
			service.SecurityHeader.WarehouseCode = "";
			service.SecurityHeader.IsAndroidDevice = true;
			service.SecurityHeader.DeviceVersion = new DataService().AndroidWebServiceVersion; // Change to correct version
		}

		#endregion

		#region GetEncryptedText

		protected string GetEncryptedText(string text)
		{
			var encoder = new AESCryptographicProvider(LoginHelper.CryptographyKey.Value, LoginHelper.CryptographyIv.Value);
			return Convert.ToBase64String(encoder.Encrypt(Encoding.Unicode.GetBytes(text)));
		}

		#endregion

		#region RestoreInitialUser

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected void RestoreInitialUser()
		{
			EnvProxy.Instance.SetUserContext(EnvProxy.Instance.CurrentUserContext);
		}

		#endregion

		#region GetNewWebService

		protected override T GetNewWebService()
		{
			var webService = new T();
			PopulateSecurityHeader(webService);

			return webService;
		}

		#endregion
	}
}
