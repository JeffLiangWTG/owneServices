using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.eTail.Integration
{
	public interface ILabelPrintingService
	{
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		bool TryPrintLabel(byte[] binaryData, string fileType, Guid printerPK, out string errorMessage);
	}
}
