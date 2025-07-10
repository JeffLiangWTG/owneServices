using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public sealed class FlattenEntryLineAndBillForImmediateDeliveryCollection : NonPersistentBusinessObjectCollection<FlattenEntryLineAndBillForImmediateDelivery>
	{
		public FlattenEntryLineAndBillForImmediateDeliveryCollection(CusEntryHeader entryHeader)
			: base(entryHeader.Factory)
		{
			this.entryHeader = entryHeader;
			shouldPrintManifestQtyFor06 = ShouldPrintManifestQuantityFor06();
			PopulateMergedLines();
		}

		readonly bool shouldPrintManifestQtyFor06;

		public ZInt ITBLAWBToPrint
		{
			get { return rowNumber; }
		}

		public ZInt UniqueTariffLines
		{
			get { return uniqueTariffsCount; }
		}

		JobDeclaration Declaration
		{
			get { return entryHeader.Declaration; }
		}

		ZString MailReferenceNumber
		{
			get { return Declaration.JE_MasterBill; }
		}

		#region Implementation

		void PopulateMergedLines()
		{
			this.RemoveAll();

			var isFTZConsumption = Declaration.IsConsumptionFTZ;

			OutputTariffDetails(isFTZConsumption);

			if (isFTZConsumption)
			{
				rowNumber = uniqueTariffsCount + 1;
			}
			else
			{
				rowNumber = OutputBillNumbers();
			}

			if (entryHeader.MergedLines.Count > 0)
			{
				if (Declaration.JE_TransportMode == Core.Constants.TransportModes.Mail)
				{
					AddLineIfNecessary(rowNumber);
					this[rowNumber].ItBlAwbCode = "P";
					this[rowNumber].ItBlAwbNumber = MailReferenceNumber;
				}
			}
		}

		#region Output Detail Rows

		bool ShouldPrintManifestQuantityFor06()
		{
			var result = false;

			if (Declaration.IsConsumptionFTZ)
			{
				var firstInvoice = Declaration.Invoices.Cast<JobComInvoiceHeader>().OrderBy(x => x.JZ_InvoiceNumber).FirstOrDefault();
				var firstInvoiceLine = firstInvoice == null ? null : firstInvoice.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.JI_LineNo).FirstOrDefault();
				result = firstInvoiceLine != null && firstInvoiceLine.US_ManifestQty > 0;
			}

			return result;
		}

		void OutputTariffDetails(bool outputFTZInformation)
		{
			var linesAdded = new Dictionary<ZString, FlattenEntryLineAndBillForImmediateDelivery>();

			foreach (CusEntryLine cusLine in entryHeader.MergedLines)
			{
				ZString tariff = cusLine.CL_AdValoremTariff.ExcludeChars(" ");
				ZString countryOfOrigin = cusLine.RandomLine != null ? cusLine.RandomLine.US_UC_NKCountryOfOrigin : ZString.Empty;
				ZString manufacturerID = cusLine.RandomLine != null ? cusLine.RandomLine.ManufacturerFallBackToSupplierNumber : ZString.Empty;
				ZString zoneStatus = outputFTZInformation && cusLine.RandomLine != null ? cusLine.RandomLine.US_ZoneStatus : ZString.Empty;

				ZString key = tariff + countryOfOrigin + manufacturerID + zoneStatus;
				if (!key.IsEmpty)
				{
					FlattenEntryLineAndBillForImmediateDelivery line;
					if (!linesAdded.TryGetValue(key, out line))
					{
						line = new FlattenEntryLineAndBillForImmediateDelivery(tariff, countryOfOrigin, manufacturerID);
						if (outputFTZInformation)
						{
							line.ItBlAwbNumber = Factory.GetCachedValue<ZoneStatusList>().GetDescriptionFromCode(cusLine.RandomLine.US_ZoneStatus);
							if (!cusLine.US_SupLine)
							{
								if (shouldPrintManifestQtyFor06)
								{
									line.ManifestQuantityUQ = cusLine.RandomLine.US_ManifestUQ;
								}
								else
								{
									line.ManifestQuantityUQ = cusLine.RandomLine.JI_InvoiceUQ;
								}
							}
						}
						linesAdded.Add(key, line);
						Add(line);
					}
					if (outputFTZInformation && !cusLine.US_SupLine)
					{
						if (shouldPrintManifestQtyFor06)
						{
							line.ManifestQuantity += cusLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.US_ManifestQty).DefaultIfEmpty(ZInt.Zero).Sum(x => x);
						}
						else
						{
							line.ManifestQuantity += cusLine.InvoiceLines.Select(x => x.JI_InvoiceQuantity).DefaultIfEmpty(ZDecimal.Zero).Sum(x => x);
						}
					}
				}
			}

			uniqueTariffsCount = linesAdded.Count;
		}

		int OutputBillNumbers()
		{
			int rowNumberForOutput = 0;
			ZString itNumber = "";
			ZString masterBillNumber = "";
			ZString houseBillNumber = "";
			ZString houseBillMaster = "";

			foreach (Bill bill in entryHeader.LowestBillDetails)
			{
				ZString manifestUQ = "";
				ZInt manifestQuantity = 0;

				if (bill.ITNumber != "" && itNumber != bill.ITNumber)
				{
					foreach (ITAndSplitDetails itNumberDetails in bill.ITAndSplitDetails)
					{
						manifestUQ = "";
						manifestQuantity = 0;

						itNumber = itNumberDetails.US_ITNumber;
						OutputBillNumber(rowNumberForOutput++, "I", "", itNumberDetails.US_ITNumber, 0, "");

						IBillDetails iBillDetailsFromBill = itNumberDetails;

						if (iBillDetailsFromBill.MasterBillNumber != "")
						{
							masterBillNumber = iBillDetailsFromBill.MasterBillNumber;

							if (iBillDetailsFromBill.HouseBillNumber == "")
							{
								manifestQuantity = iBillDetailsFromBill.PackageQuantity;
								manifestUQ = iBillDetailsFromBill.PackageType;
							}

							OutputBillNumber(rowNumberForOutput++, "M", iBillDetailsFromBill.IssuerCodeOfMasterBillNumber, masterBillNumber, manifestQuantity, manifestUQ);
						}

						if (iBillDetailsFromBill.HouseBillNumber != "")
						{
							houseBillNumber = iBillDetailsFromBill.HouseBillNumber;
							houseBillMaster = iBillDetailsFromBill.MasterBillNumber;

							if (iBillDetailsFromBill.SubHouseBillNumber == "")
							{
								manifestQuantity = iBillDetailsFromBill.PackageQuantity;
								manifestUQ = iBillDetailsFromBill.PackageType;
							}

							OutputBillNumber(rowNumberForOutput++, "H", iBillDetailsFromBill.IssuerCodeOfHouseBillNumber, houseBillNumber, manifestQuantity, manifestUQ);
						}

						if (iBillDetailsFromBill.SubHouseBillNumber != "")
						{
							OutputBillNumber(rowNumberForOutput++, "S", iBillDetailsFromBill.IssuerCodeOfSubHouseBillNumber, iBillDetailsFromBill.SubHouseBillNumber, iBillDetailsFromBill.PackageQuantity, iBillDetailsFromBill.PackageType);
						}
					}
				}
				else
				{
					IBillDetails iBillDetailsFromBill = bill;

					if (iBillDetailsFromBill.MasterBillNumber != "" && masterBillNumber != iBillDetailsFromBill.MasterBillNumber && Declaration.JE_TransportMode != Core.Constants.TransportModes.Mail)
					{
						masterBillNumber = iBillDetailsFromBill.MasterBillNumber;

						if (iBillDetailsFromBill.HouseBillNumber == "")
						{
							manifestQuantity = iBillDetailsFromBill.PackageQuantity;
							manifestUQ = iBillDetailsFromBill.PackageType;
						}

						OutputBillNumber(rowNumberForOutput++, "M", iBillDetailsFromBill.IssuerCodeOfMasterBillNumber, masterBillNumber, manifestQuantity, manifestUQ);
					}

					if (iBillDetailsFromBill.HouseBillNumber != "" && ((houseBillNumber != iBillDetailsFromBill.HouseBillNumber) || (houseBillMaster != iBillDetailsFromBill.MasterBillNumber)))
					{
						houseBillNumber = iBillDetailsFromBill.HouseBillNumber;
						houseBillMaster = iBillDetailsFromBill.MasterBillNumber;

						if (iBillDetailsFromBill.SubHouseBillNumber == "")
						{
							manifestQuantity = iBillDetailsFromBill.PackageQuantity;
							manifestUQ = iBillDetailsFromBill.PackageType;
						}

						OutputBillNumber(rowNumberForOutput++, "H", iBillDetailsFromBill.IssuerCodeOfHouseBillNumber, houseBillNumber, manifestQuantity, manifestUQ);
					}

					if (iBillDetailsFromBill.SubHouseBillNumber != "")
					{
						OutputBillNumber(rowNumberForOutput++, "S", iBillDetailsFromBill.IssuerCodeOfSubHouseBillNumber, iBillDetailsFromBill.SubHouseBillNumber, iBillDetailsFromBill.PackageQuantity, iBillDetailsFromBill.PackageType);
					}

					if (iBillDetailsFromBill.MasterBillNumber.IsEmpty && iBillDetailsFromBill.HouseBillNumber.IsEmpty && iBillDetailsFromBill.SubHouseBillNumber.IsEmpty && !iBillDetailsFromBill.PackageQuantity.IsEmpty)
					{
						OutputBillNumber(rowNumberForOutput++, ZString.Empty, ZString.Empty, ZString.Empty, iBillDetailsFromBill.PackageQuantity, iBillDetailsFromBill.PackageType);
					}
				}
			}

			return rowNumberForOutput;
		}

		void OutputBillNumber(int rowNumber, ZString billCode, ZString issuerCode, ZString billNumber, ZInt manifestQty, ZString manifestUQ)
		{
			AddLineIfNecessary(rowNumber);

			this[rowNumber].ItBlAwbCode = billCode;

			if (Declaration.JE_TransportMode == Core.Constants.TransportModes.Sea ||
				Declaration.JE_TransportMode == Core.Constants.TransportModes.Rail ||
				Declaration.JE_TransportMode == TransportTypeList.Codes.Truck)
			{
				if (billCode == "M" && !Declaration.US_SchDLoading.IsEmpty)
				{
					this[rowNumber].PortOfLading = "(" + Declaration.US_SchDLoading + ")";
				}

				if (!issuerCode.IsEmpty)
				{
					this[rowNumber].ItBlAwbNumber = issuerCode;
				}
			}

			this[rowNumber].ItBlAwbNumber += billNumber;
			this[rowNumber].ManifestQuantity = (ZDecimal)manifestQty;
			this[rowNumber].ManifestQuantityUQ = manifestUQ;
		}

		#endregion

		void AddLineIfNecessary(ZInt number)
		{
			if (number >= this.Count)
			{
				Add(new FlattenEntryLineAndBillForImmediateDelivery(ZString.Empty, ZString.Empty, ZString.Empty));
			}
		}

		readonly CusEntryHeader entryHeader;

		ZInt rowNumber;
		ZInt uniqueTariffsCount;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FlattenEntryLineAndBillForImmediateDelivery(ZString.Empty, ZString.Empty, ZString.Empty);
		}

		#endregion
	}
}
