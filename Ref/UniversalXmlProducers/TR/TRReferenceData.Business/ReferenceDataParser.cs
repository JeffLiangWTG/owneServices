using System;
using System.Globalization;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	public abstract class ReferenceDataParser : IReferenceDataParser
	{
		public void GenerateUXML(string outputFileName)
		{
			try
			{
				Helper.ExportToXMLFile(DataSource, outputFileName, GetXmlWriterConfiguration(), PublicationDateTime, GetEntities());
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Failed to generate UXML, exception: {ex.GetBaseException().Message}");
			}
		}

		protected abstract string DataSource { get; }

		protected abstract DateTime PublicationDateTime { get; }

		protected abstract RefDataRepoModelEntityType[] GetEntities();

		protected abstract XmlWriterConfiguration GetXmlWriterConfiguration();

		protected StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		public string ErrorMessage => ErrorBuilder.ToString();

		#region IReferenceDataParser Implementation

		string IReferenceDataParser.DataSource => DataSource;

		DateTime IReferenceDataParser.PublicationDateTime => PublicationDateTime;

		RefDataRepoModelEntityType[] IReferenceDataParser.GetEntities() => GetEntities();

		XmlWriterConfiguration IReferenceDataParser.GetXmlWriterConfiguration() => GetXmlWriterConfiguration();

		#endregion
	}
}
