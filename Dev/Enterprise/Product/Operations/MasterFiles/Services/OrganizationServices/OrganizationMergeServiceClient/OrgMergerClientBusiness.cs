using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CargoWise.Common;
using OrgMergeService;

namespace Enterprise.MasterFiles.OrganizationMergeServiceClientApp
{
	public class OrgMergerClientBusiness : IDisposable
	{
		public OrganizationMergeClient Client
		{
			get
			{
				if (client == null)
				{
					client = new OrganizationMergeClient();
				}

				return client;
			}
		}
		OrganizationMergeClient client;

		#region Implementation

		internal string Merge(string loginName, string loginPassword, string xml)
		{
			Argument.NotNull(xml, nameof(xml));
			Argument.NotNull(loginPassword, nameof(loginPassword)); // Suggested By ReviewBot 
			Argument.NotNull(loginName, nameof(loginName));

			var errors = new List<string>();
			ValidateMergeFromXmlParameters(errors, xml);
			PrepareLoginCredentials(errors, ref loginName, ref loginPassword);

			if (errors.Any())
			{
				return FormatErrors(errors);
			}

			return Client.MergeFromXmlAsUser(loginName, loginPassword, xml.Trim());
		}

		internal string Merge(string loginName, string loginPassword, string newOrgCode, string[] oldOrgCodes)
		{
			Argument.NotNull(oldOrgCodes, nameof(oldOrgCodes)); // Suggested By ReviewBot 
			Argument.NotNull(loginPassword, nameof(loginPassword)); // Suggested By ReviewBot
			Argument.NotNull(loginName, nameof(loginName));

			var errors = new List<string>();
			ValidateMergeParameters(errors, newOrgCode, oldOrgCodes);
			PrepareLoginCredentials(errors, ref loginName, ref loginPassword);

			if (errors.Any())
			{
				return FormatErrors(errors);
			}

			return Client.MergeAsUser(loginName, loginPassword, oldOrgCodes, newOrgCode);
		}

		void PrepareLoginCredentials(ICollection<string> errors, ref string loginName, ref string loginPassword)
		{
			Argument.NotNull(loginName, nameof(loginName));
			Argument.NotNull(errors, nameof(errors));
			Argument.NotNull(loginPassword, nameof(loginPassword)); // Suggested By ReviewBot 

			ValidateLoginParameters(errors, loginName, loginPassword);

			loginName = EncryptData(loginName);
			loginPassword = EncryptData(loginPassword);
		}

		#endregion

		#region Validation

		string FormatErrors(ICollection<string> errors)
		{
			Argument.NotNull(errors, nameof(errors)); // Suggested By ReviewBot 

			return string.Join(Environment.NewLine, errors);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		void ValidateLoginParameters(ICollection<string> errors, string username, string password)
		{
			Argument.NotNull(errors, nameof(errors)); // Suggested By ReviewBot 

			if (string.IsNullOrWhiteSpace(username))
			{
				errors.Add("Please enter a Login.");
			}

			if (string.IsNullOrEmpty(password))
			{
				errors.Add("Please enter a Password.");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		void ValidateMergeParameters(ICollection<string> errors, string newOrgCode, string[] oldOrgCodes)
		{
			Argument.NotNull(oldOrgCodes, nameof(oldOrgCodes)); // Suggested By ReviewBot 
			Argument.NotNull(errors, nameof(errors)); // Suggested By ReviewBot

			if (string.IsNullOrEmpty(newOrgCode))
			{
				errors.Add("Please enter a New Organization Code.");
			}

			if (oldOrgCodes.Length == 0)
			{
				errors.Add("Please enter at least one Old Organization code.");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		void ValidateMergeFromXmlParameters(ICollection<string> errors, string xml)
		{
			if (string.IsNullOrWhiteSpace(xml))
			{
				errors.Add("Please enter a value");
			}
		}

		#endregion

		#region Cryptography

		RSACryptoServiceProvider rsaProvider;
		public RSACryptoServiceProvider RsaProvider
		{
			get
			{
				return rsaProvider ?? (rsaProvider = new RSACryptoServiceProvider());
			}
		}

		string EncryptData(string data)
		{
			Argument.NotNull(data, nameof(data)); // Suggested By ReviewBot 

			using (StreamReader reader = new StreamReader("publickey.xml"))
			{
				string publicOnlyKeyXML = reader.ReadToEnd();
				RsaProvider.FromXmlString(publicOnlyKeyXML);

				byte[] plainbytes = System.Text.Encoding.UTF8.GetBytes(data);
				byte[] cipherbytes = RsaProvider.Encrypt(plainbytes, false);
				return Convert.ToBase64String(cipherbytes);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (client != null)
				{
					((IDisposable)client).Dispose();
				}
			}
		}
		#endregion
	}
}
