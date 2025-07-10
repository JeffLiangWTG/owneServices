using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public class PackingWeightListDocumentWrapper : DocumentWrapper, IDocumentWrapper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		internal const string DocDataPackingListNumber = "Packing List Number";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		internal const string DocDataPackDate = "Pack Date";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need")]
		const string SeeMarks = "Marks and numbers will be printed below...";

		public PackingWeightListDocumentWrapper(JobDeclaration jobDeclaration, BusinessObjectFactory factory) : base(jobDeclaration, factory)
		{
			this.jobDeclaration = jobDeclaration;
		}

		protected virtual void SetPackingLines()
		{
			if (jobDeclaration != null)
			{
				foreach (BasePackage package in jobDeclaration.Packages)
				{
					var packingLine = new PackingLine(package, Factory);
					PackingLines.Add(packingLine);
				}
			}
		}

		#region MarksAndNumbers
		protected virtual ZString MarksAndNumbersCore
		{
			get
			{
				var result = new ZStringBuilder();
				jobDeclaration?.Invoices.Cast<JobComInvoiceHeader>().ForEach(invoice => result.AppendIfNotEmpty(invoice.TW_MarksAndNumbers));
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		List<ZString> fMarksAndNumbersLine;

		const int maxLengthOfMarksAndNumbersLinePart1 = 7;

		ZBool IsMarksOverMaxCount => MarksAndNumbersLine.Count > maxLengthOfMarksAndNumbersLinePart1;

		List<ZString> MarksAndNumbersLine => fMarksAndNumbersLine ?? (fMarksAndNumbersLine = DocumentWrapperHelper.SplitTextByLineBreak(MarksAndNumbers, 35));

		public ZString MarksAndNumbersPart1 => IsMarksOverMaxCount ? new ZString(SeeMarks) : DocumentWrapperHelper.CombineTextLines(MarksAndNumbersLine, 0, maxLengthOfMarksAndNumbersLinePart1);

		public ZString MarksAndNumbersPart2 => DocumentWrapperHelper.CombineTextLines(MarksAndNumbersLine, IsMarksOverMaxCount ? 0 : maxLengthOfMarksAndNumbersLinePart1);
		#endregion

		#region Fields and Properties

		readonly JobDeclaration jobDeclaration;

		public ZString ExportAgentsReference => jobDeclaration?.JE_AgentsReference ?? ZString.Empty;

		public virtual ZString PackingListNumber => jobDeclaration?.DocNote.GetSystemDefinedFieldValue(DocDataPackingListNumber) ?? ZString.Empty;

		public virtual ZString PackDate => jobDeclaration?.DocNote.GetSystemDefinedFieldValue(DocDataPackDate) ?? ZString.Empty;

		public ZString SupplierDetails => string.Format(CultureInfo.InvariantCulture, "{0}\n{1}", SupplierName, SupplierAddressLine); 

		public ZString SupplierName => SupplierAddressData?.CompanyName ?? ZString.Empty;

		ZString SupplierAddressLine => SupplierAddressData?.Address ?? ZString.Empty;

		public ZString ImporterDetails => string.Format(CultureInfo.InvariantCulture, "{0}\n{1}", ImporterName, ImporterAddressLine); 

		ZString ImporterName => ImporterAddressData?.CompanyName ?? ZString.Empty;

		ZString ImporterAddressLine => ImporterAddressData?.Address ?? ZString.Empty;

		public ZString PortOfOrigin => DocumentWrapperHelper.GetOriginProperNameWithCountry(jobDeclaration, " - ");

		public ZString FinalDestination => DocumentWrapperHelper.GetFinalDestinationProperNameWithCountry(jobDeclaration, " - ");

		public ZString TransportMode => jobDeclaration?.TransportMode ?? ZString.Empty;

		public ZString Vessel => jobDeclaration?.JE_VesselName ?? ZString.Empty;

		public ZString MarksAndNumbers => MarksAndNumbersCore;

		public ZString Remarks => RemarksCore;

		protected virtual ZString RemarksCore
		{
			get
			{
				var result = new ZStringBuilder();
				jobDeclaration.Invoices.Where(x => !x.JZ_Remarks.IsEmpty).Select(x => x.JZ_Remarks).Distinct().ForEach(x => result.Append(x));
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public PackingLineCollection PackingLines
		{
			get
			{
				if (fPackingLines == null)
				{
					fPackingLines = new PackingLineCollection(Factory);
					SetPackingLines();
					fPackingLines.SetPackingSummaries();
				}
				return fPackingLines;
			}
		}
		PackingLineCollection fPackingLines;

		public ZString PackTypeSummary => DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag(PackingLines.PackTypeSummary, 13);

		public ZString QuantitySummary => DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag(PackingLines.QuantitySummary, 15);

		public ZString NetWeightSummary => DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag(PackingLines.NetWeightSummary, 15);

		public ZString GrossWeightSummary => DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag(PackingLines.GrossWeightSummary, 15);

		public ZString VolumeSummary => DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag(PackingLines.VolumeSummary, 15);
		#endregion

		DocumentaryAddressDetailsWrapper SupplierAddressData
		{
			get
			{
				if (fSupplierAddressData == null)
				{
					var supplierDocumentaryAddress = jobDeclaration?.SupplierDocumentaryAddress;
					if (supplierDocumentaryAddress != null && supplierDocumentaryAddress.Address != null)
					{
						fSupplierAddressData = new ExportExporterDocumentaryAddressDetailsWrapper(jobDeclaration.Supplier, supplierDocumentaryAddress);
					}
				}
				return fSupplierAddressData;
			}
		}
		DocumentaryAddressDetailsWrapper fSupplierAddressData;

		DocumentaryAddressDetailsWrapper ImporterAddressData
		{
			get
			{
				if (fImporterAddressData == null)
				{
					var importerDocumentaryAddress = jobDeclaration?.ImporterDocumentaryAddress;
					if (importerDocumentaryAddress != null && importerDocumentaryAddress.Address != null)
					{
						fImporterAddressData = new ExportBuyerDocumentaryAddressDetailsWrapper(jobDeclaration.Importer, importerDocumentaryAddress);
					}
				}
				return fImporterAddressData;
			}
		}
		DocumentaryAddressDetailsWrapper fImporterAddressData;
	}
}
