namespace Enterprise.Customs.UY.Manifest.Business
{
	public static class UYMessageConstants
	{
		public const string MessageServiceTaskCategory = "UYC";

		public const string InterchangeToTest = "UYCustomsTEST";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Path name")]
		public const string Xpath = "/*[local-name()]/*[local-name()='Signature']";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response Message for Successful Cancellations")]
		public const string CancellationMessage = "Baja de Conocimiento exitoso.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response Message for Successful New Manifests")]
		public const string NewMessage = "El alta del conocimiento";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response Message for Successful Amend Manifest")]
		public const string AmendMessage = "Modificacion de Conocimiento exitosa.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response Message for Successful Amend Pack")]
		public const string AmendPack = "Modificacion de la linea exitosa.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response Message for Successful Amend Pack Adding a New One")]
		public const string NewPack = "Alta linea exitosa.";

		public const string IdBill = "C";

		public const string IdPack = "L";

		public const string ResponseType = "RespuestaTipo"; // Customs Response

		public const string HBLNumber = "ConocimientoOriginalNumero"; // Customs Response

		public const string HBLSequenceNumber = "ConocimientoNumeroSecuencial"; // Customs Response

		public const string DNANumber = "ConocimientoNumeroDNA"; // Customs Response

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
		public const string BillNotFound = "Can't find the Bill related";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
		public const string Processing = "Processing Log";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error Message")]
		public const string WrongXML = "Can not deserialize the object";

		public const string ManifestNumber = "ManifiestoNumero"; // Customs Response

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
		public const string Result = "Result      : ";

		public const string NewLine = "\r\n"; // Customs Response

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
		public const string Description = "Description : ";

		public const string NewTwoLines = "\r\n\r\n"; // Customs Response

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
		public const string References = "References";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
		public const string DNANumberForView = "DNANumber   : ";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
		public const string Sequence = "Sequence    : ";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
		public const string HBLNumberForView = "HBLNumber   : ";

		public const string AcceptanceCode = "1450"; // Customs Response

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message Error")]
		public const string Error = "Error";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message Detail")]
		public const string Detail = "Detail";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
		public const string NotificationTime = "Notification Time : ";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Customs Response")]
		public const string NotificationType = "Notification Type : ";
	}
}
