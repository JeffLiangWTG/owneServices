using System;
using System.Text;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class CPCProvider : RequestCodeProvider
	{
		protected override Encoding Encoding => Encoding.GetEncoding("ISO-8859-1");

		public enum CPCType
		{
			EXREG371,
			EXREG372,
			IMREG371,
			IMREG372
		}

		public string RequestCodes(string baseURL, string date, CPCType type)
			=> Sanitize(RequestExportableDocument(baseURL, date, type), type);

		static string Sanitize(string text, CPCType type)
		{
			string RemoveByteOrderMark(string textWithOrderMark) => textWithOrderMark.Trim(new char[] { '\uFEFF' });

			string RemoveLastEmptyLine(string textWithEmptyLine)
			{
				var lineToRemove = type == CPCType.EXREG371 ? Constants.AEAT.LastLineForExportCSV : Constants.AEAT.LastLineForImportCSV;

				return textWithEmptyLine.Replace(lineToRemove, string.Empty).TrimEnd();
			}

			var textWithoutByteOrderMark = RemoveByteOrderMark(text.Replace(Constants.AEAT.FirstLineForCSV, string.Empty));

			var needToRemoveLastLine = type == CPCType.EXREG371 || type == CPCType.IMREG371;

			return needToRemoveLastLine ? RemoveLastEmptyLine(textWithoutByteOrderMark) : textWithoutByteOrderMark;
		}

		protected override Exception GetNewExceptionToThrow(string exceptionMessage) => new CPCException(exceptionMessage);
	}
}
