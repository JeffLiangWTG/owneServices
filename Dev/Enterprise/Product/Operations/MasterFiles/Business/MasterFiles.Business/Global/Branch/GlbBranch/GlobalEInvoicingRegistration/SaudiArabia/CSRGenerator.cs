using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Microsoft;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	public interface ICSRGenerator
	{
		(string privateKey, string csrData) Generate();
	}

	public class CSRGenerator : ICSRGenerator
	{
		public CSRGenerator(GlbBranch branch)
		{
			Argument.NotNull(branch, nameof(branch));
			this.branch = branch;
		}
		readonly GlbBranch branch;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Certificate builder")]
		(string privateKey, string csrData) ICSRGenerator.Generate()
		{
			var orgProxy = branch.OrgProxy
				?? branch.Company.OrgProxy;
			var address = orgProxy.MainAddress;
			var formatter = new AddressFormatter(address.Factory, address, branch.Company, false);
			var registeredAddress = formatter.PostalAddressAsASingleLineWithoutCompanyName();
			var primaryRegistrationCode = orgProxy.PrimaryRegistrationNumber;
			if ((primaryRegistrationCode.CusCode?.OK_CodeType ?? string.Empty) != OrgCusCode.CodeTypes.VATCode || primaryRegistrationCode.NumberForDisplay.IsEmpty)
			{
				throw new ApplicationException($"Unable to get VAT registration for Org Proxy '{orgProxy.OH_Code} - {orgProxy.OH_FullName}'.");
			}
			var primaryRegistrationNumber = primaryRegistrationCode.NumberForDisplay;
			var ou = branch.GB_BranchName;
			if (primaryRegistrationNumber.Length >= 11)
			{
				var checkDigit = primaryRegistrationNumber.Substring(10, 1);
				if (checkDigit == "1")
				{
					var tinCode = orgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(SaudiArabiaOrgCusCodeInfo.OrgCusCodes.TIN, Core.Constants.CountryCodes.SaudiArabia);
					if (tinCode != null)
					{
						ou = tinCode.OK_CustomsRegNo;
					}
				}
			}

			var cw1RgistrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var ecParam = SecObjectIdentifiers.SecP256k1;
			var generator = new ECKeyPairGenerator();
			generator.Init(new ECKeyGenerationParameters(ecParam, new SecureRandom()));
			var ackp = generator.GenerateKeyPair();
			var strBuilder = new StringBuilder();

			//Extract Private key (PEM format)
			var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(ackp.Private);
			var privateKeyPem = Convert.ToBase64String(privateKeyInfo.GetDerEncoded());
			privateKeyPem = Regex.Replace(privateKeyPem, ".{64}", "$0\n");
			strBuilder.Append("-----BEGIN EC PRIVATE KEY-----\n");
			strBuilder.Append(privateKeyPem);
			strBuilder.Append("-----END EC PRIVATE KEY-----");
			privateKeyPem = strBuilder.ToString();

			var subjectAttributes = new Dictionary<DerObjectIdentifier, string>
				{
					{ X509Name.C, "SA" },
					{ X509Name.O, orgProxy.OH_FullName },
					{ X509Name.OU, ou },
					{ X509Name.CN, string.Format("Cargowise {0}{1}{2}", EnvProxy.Instance.CurrentCompany.Code, cw1RgistrationKey.ServerCode, branch.GB_Code) },
				};

			var regAddressOID = new DerObjectIdentifier("2.5.4.26");  //registeredAddress OID, for more info see https://oidref.com/2.5.4.26
			var subjectAlternativeNameAttributes = new Dictionary<DerObjectIdentifier, string>
				{
					{ X509Name.Surname, string.Format("1-Wisetech Global|2-Cargowise|3-{0}{1}{2}-{3}",
									cw1RgistrationKey.EnterpriseCode, EnvProxy.Instance.CurrentCompany.Code, cw1RgistrationKey.ServerCode, branch.GB_Code) },
					{ X509Name.UID,  primaryRegistrationNumber },
					{ X509Name.T, "1000" },
					{ regAddressOID, registeredAddress },
					{ X509Name.BusinessCategory, "Logistics Services" }
				};

			var subjectName = new X509Name(subjectAttributes.Keys.ToList(), subjectAttributes);
			var subjectAltNames = new X509Name(subjectAlternativeNameAttributes.Keys.ToList(), subjectAlternativeNameAttributes);
			var generalNames = new GeneralNames(new[] { new GeneralName(subjectAltNames) });
			var templateName = Env.Instance.IsProductionSystem ? "ZATCA-Code-Signing" : "PREZATCA-Code-Signing";
			var extensionsGenerator = new X509ExtensionsGenerator();
			extensionsGenerator.AddExtension(MicrosoftObjectIdentifiers.MicrosoftCertTemplateV1, false, new DerPrintableString(templateName, true));
			extensionsGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, generalNames);
			var extensions = extensionsGenerator.Generate();
			var signatureFactory = new Asn1SignatureFactory("SHA256WITHECDSA", ackp.Private);
			var attributeX = new AttributeX509(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest, new DerSet(extensions));
			var requestAttributeSet = new DerSet(attributeX);
			var pkcs10CertificationRequest = new Pkcs10CertificationRequest(signatureFactory, subjectName, ackp.Public, requestAttributeSet);

			//Export the CSR (PEM)
			var csr = Convert.ToBase64String(pkcs10CertificationRequest.GetEncoded());
			var csrPem = Regex.Replace(csr, ".{64}", "$0\n");
			strBuilder = new StringBuilder();
			strBuilder.Append("-----BEGIN CERTIFICATE REQUEST-----\n");
			strBuilder.Append($"{csrPem}\n");
			strBuilder.Append("-----END CERTIFICATE REQUEST-----");

			//You can verify the CSR data with openSSL command
			//copy the above csr text string into yourData.csr first,
			//then run: openssl req -text -noout -verify -in yourData.csr
			//or to diagnose the ASN.1 structure use: openssl asn1parse -in yourData.csr
			var csrData = strBuilder.ToString();
			return (privateKeyPem, csrData);
		}
	}
}
