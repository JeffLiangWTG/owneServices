using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class NMFSLineCollection : DependentCusAddInfoCollection<NMFSLine, BusinessObject>, IPGADataCorrectionCollection
	{
		public NMFSLineCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USNMFSLine)
		{
		}

		internal bool IsNMFSLineDefaultingSuspended
		{
			get { return nmfsLineDefaultingSuspenderIndex > 0; }
		}

		int nmfsLineDefaultingSuspenderIndex;

		public override IDisposable SuspendAdditionallyForImport()
		{
			return new NMFSLineDefaultingSuspender(this);
		}

		class NMFSLineDefaultingSuspender : IDisposable
		{
			public NMFSLineDefaultingSuspender(NMFSLineCollection nmfsLineCollection)
			{
				lineCollection = nmfsLineCollection;
				lineCollection.nmfsLineDefaultingSuspenderIndex++;
			}

			readonly NMFSLineCollection lineCollection;

			public void Dispose()
			{
				lineCollection.nmfsLineDefaultingSuspenderIndex--;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (!IsNMFSLineDefaultingSuspended)
			{
				var newElement = child as NMFSLine;
				var invoiceLine = Master as JobComInvoiceLine;

				if (newElement != null && !newElement.IsExport)
				{
					if (newElement.AddInfoLookups.NMFSPrograms.Count == 1)
					{
						newElement.US_ProgramType = ((CodeDescriptionPair)newElement.AddInfoLookups.NMFSPrograms[0]).Code;
					}

					if (invoiceLine != null && !invoiceLine.IsDeleted && !invoiceLine.IsDataImportInProgress)
					{
						if (Count > 0)
						{
							NMFSLine previousNMFSLine = this[Count - 1];

							if (invoiceLine.CopyLastPGADetailsToNewLine)
							{
								newElement.CopyPersistentValuesFrom(previousNMFSLine);
								newElement.US_DISDocumentID = ZString.Empty;
							}
							else
							{
								newElement.US_IFTPPermitNumber = previousNMFSLine.US_IFTPPermitNumber;
							}

							foreach (NMFSHarvestingDetail harvestingDetail in previousNMFSLine.HarvestingDetails)
							{
								var cloneHarvestingDetail = (NMFSHarvestingDetail)harvestingDetail.Clone();
								if (!invoiceLine.CopyLastPGADetailsToNewLine)
								{
									cloneHarvestingDetail.US_ContainsYellowfinTuna = (ZBool)cloneHarvestingDetail.US_ContainsYellowfinTuna.Default;
								}
								newElement.HarvestingDetails.Add(cloneHarvestingDetail);
							}
						}

						if (newElement.US_NetWeight.IsEmpty)
						{
							var netWeightOnInvoiceLine = new ZWeight(invoiceLine.JI_CustomsQuantity, invoiceLine.JI_CustomsUnitQty);
							if (netWeightOnInvoiceLine.IsValid && netWeightOnInvoiceLine.Amount > 0)
							{
								if (newElement.US_NetWeightUQ.IsEmpty)
								{
									newElement.US_NetWeightUQ = invoiceLine.JI_CustomsUnitQty.Left(3);
								}

								if (Core.Constants.Weight.ContainsCode(newElement.US_NetWeightUQ))
								{
									var sumNetWeight = ZDecimal.Zero;
									foreach (NMFSLine nmfsLine in this)
									{
										var weight = new ZWeight(nmfsLine.US_NetWeight, nmfsLine.US_NetWeightUQ);
										if (weight.IsValid && weight.Amount > 0)
										{
											sumNetWeight += weight.ConvertTo(netWeightOnInvoiceLine.Unit);
										}
									}

									var remainingNetWeight = netWeightOnInvoiceLine.Amount - sumNetWeight;
									if (remainingNetWeight > 0)
									{
										newElement.US_NetWeight = Core.Constants.Weight.ConvertSafe(remainingNetWeight, netWeightOnInvoiceLine.Unit, newElement.US_NetWeightUQ);
									}
								}
							}
						}
					}
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return AllowAddNewPGALines && base.AllowNewCore; }
		}

		public bool AllowAddNewPGALines
		{
			get
			{
				if (!fAllowAddNewPGALines.HasValue)
				{
					var invoiceLine = Master as JobComInvoiceLine;
					fAllowAddNewPGALines = invoiceLine?.AllowAddNewLineToPGACollection() ?? true;
				}
				return fAllowAddNewPGALines.Value;
			}
			set
			{
				fAllowAddNewPGALines = value;
				if (value)
				{
					var invoiceLine = Master as JobComInvoiceLine;
					invoiceLine?.Declaration?.UpdatePGADataReplacementUpdateRequired();
				}
			}
		}
		bool? fAllowAddNewPGALines;

		System.Collections.Generic.IEnumerable<IPGADataCorrection> IPGADataCorrectionCollection.CorrectionItems => this.Cast<IPGADataCorrection>();
	}
}
