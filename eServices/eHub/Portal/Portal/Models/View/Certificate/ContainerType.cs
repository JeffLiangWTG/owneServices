using System.Collections.Generic;
using System.Linq;

namespace CargoWise.eHub.Portal.Models.View.Certificate
{
	public struct ContainerTypeContent
	{
		public string DisplayName { get; set; }
		public string FileContentType { get; set; }
		public FileType Type { get; set; }
		public string DefaultExtension { get; set; }
		public string ID { get; set; }
	}

	public static class ContainerType
	{
		public static ContainerTypeContent GetContainerType(string containerTypeID)
		{
			var candidateContainerType = List.Where(x => x.ID == containerTypeID);
			return candidateContainerType.Count() > 0 ? candidateContainerType.First() : X509_Binary;
		}

		public static IEnumerable<ContainerTypeContent> List
		{
			get
			{
				return new ContainerTypeContent[] { X509_Text, X509_Binary, PEM_Text, PKCS12_Binary, PKCS7_Text, PKCS7_Binary, OPENSSH_Text };
			}
		}

		public static ContainerTypeContent X509_Text
		{
			get
			{
				return new ContainerTypeContent()
				{
					DisplayName = "X.509 Certificate - x509-text (*.cer,*.crt,*.der)",
					FileContentType = "application/x-x509-ca-cert",
					Type = FileType.Text,
					DefaultExtension = ".cer",
					ID = "x509-text"
				};
			}
		}

		public static ContainerTypeContent X509_Binary
		{
			get
			{
				return new ContainerTypeContent()
				{
					DisplayName = "X.509 Certificate - x509-binary (*.cer,*.crt,*.der)",
					FileContentType = "application/x-x509-ca-cert",
					Type = FileType.Binary,
					DefaultExtension = ".cer",
					ID = "x509-binary"
				};
			}
		}

		public static ContainerTypeContent PEM_Text
		{
			get
			{
				return new ContainerTypeContent()
				{
					DisplayName = "S/MIME: Privacy Enhanced Mail - pem-text (*.pem)",
					FileContentType = "application/x-pem-file",
					Type = FileType.Text,
					DefaultExtension = ".pem",
					ID = "pem-text"
				};
			}
		}

		public static ContainerTypeContent PKCS12_Binary
		{
			get
			{
				return new ContainerTypeContent()
				{
					DisplayName = "Personal Information Exchange - pkcs12-binary (*.pfx,*.p12)",
					FileContentType = "application/x-pkcs12",
					Type = FileType.Binary,
					DefaultExtension = ".pfx",
					ID = "pkcs12-binary"
				};
			}
		}

		public static ContainerTypeContent PKCS7_Binary
		{
			get
			{
				return new ContainerTypeContent()
				{
					DisplayName = "Cryptographic Message Syntax Standard - PKCS #7 Certficates - pkcs7-binary (*.p7b,*.spc)",
					FileContentType = "application/x-pkcs7-certificates",
					Type = FileType.Binary,
					DefaultExtension = ".p7b",
					ID = "pkcs7-binary"
				};
			}
		}

		public static ContainerTypeContent PKCS7_Text
		{
			get
			{
				return new ContainerTypeContent()
				{
					DisplayName = "Cryptographic Message Syntax Standard - PKCS #7 Certficates - pkcs7-text (*.p7b,*.spc)",
					FileContentType = "application/x-pkcs7-certificates",
					Type = FileType.Text,
					DefaultExtension = ".p7b",
					ID = "pkcs7-text"
				};
			}
		}

		public static ContainerTypeContent OPENSSH_Text
		{
			get
			{
				return new ContainerTypeContent()
				{
					DisplayName = "OpenSSH certificates - openssh-text (*.*)",
					FileContentType = "text/text",
					Type = FileType.Text,
					DefaultExtension = ".ssh",
					ID = "openssh-text"
				};
			}
		}
	}

	public enum FileType { Text, Binary }
}
