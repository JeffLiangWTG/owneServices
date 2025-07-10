using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Business
{
	public class CryptokiCertificate : NonPersistentBusinessObject
	{
		[ResourceStringData("CryptokiCertificate|Owner", Caption = "Owner")]
		public ZString Owner { get; set; }

		[ResourceStringData("CryptokiCertificate|Issuer", Caption = "Issuer")]
		public ZString Issuer { get; set; }

		[ResourceStringData("CryptokiCertificate|NotBefore", Caption = "Valid From")]
		public ZDateTime NotBefore { get; set; }

		[ResourceStringData("CryptokiCertificate|NotAfter", Caption = "Valid To")]
		public ZDateTime NotAfter { get; set; }

		[ResourceStringData("CryptokiCertificate|Thumbprint", Caption = "Thumbprint")]
		public ZString Thumbprint { get; set; }

		[ResourceStringData("CryptokiCertificate|SerialNumber", Caption = "Serial Number")]
		public ZString SerialNumber { get; set; }

		[ResourceStringData("CryptokiCertificate|TokenManufacturerId", Caption = "Token Manufacturer")]
		public ZString TokenManufacturerId { get; set; }

		[ResourceStringData("CryptokiCertificate|TokenModel", Caption = "Token Model")]
		public ZString TokenModel { get; set; }

		[ResourceStringData("CryptokiCertificate|TokenChipset", Caption = "Token Chipset")]
		public ZString TokenChipset { get; set; }
	}
}
