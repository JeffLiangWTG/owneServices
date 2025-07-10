using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address
{
	public sealed class AddressState : DocDataObject
	{
		#region IsSameAsExporter

		public ZBool IsSameAsExporter
		{
			get => isSameAsExporter;
			set
			{
				if (value)
				{
					IsUnknown = false;
				}
				SetNonPersistentPropertyValue(IsSameAsExporterInfo, ref isSameAsExporter, value);
			}
		}

		ZBool isSameAsExporter;

		public ZPropertyInfo IsSameAsExporterInfo => GetZPropertyInfo(nameof(IsSameAsExporter));

		#endregion

		#region IsUnknown

		public ZBool IsUnknown
		{
			get => isUnknown;
			set
			{
				if (value)
				{
					IsSameAsExporter = false;
				}
				SetNonPersistentPropertyValue(IsUnknownInfo, ref isUnknown, value);
			}
		}

		ZBool isUnknown;

		public ZPropertyInfo IsUnknownInfo => GetZPropertyInfo(nameof(IsUnknown));

		#endregion

		#region ExcludeFromPDF

		public ZBool ExcludeFromPDF
		{
			get => excludeFromPDF;
			set => SetNonPersistentPropertyValue(ExcludeFromPDFInfo, ref excludeFromPDF, value);
		}

		ZBool excludeFromPDF;

		public ZPropertyInfo ExcludeFromPDFInfo => GetZPropertyInfo(nameof(ExcludeFromPDF));

		#endregion
	}
}
