using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocCusEntryHeader : DocBaseCusEntryHeader
	{
		DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
		}

		public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
		{
			return new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
		}

		#region Wrapper Fields

		public DocOrganisation Agent
		{
			get { return DocOrganisation.New(CusEntryHeader.EffectiveAgent, Factory); }
		}

		public DocDocAddress WarehouseAddress
		{
			get
			{
				// TODO: ToBeChecked: Logic changed with the removal of PurposeCode in ZA, implementation will be re-visited and changed accordingly #Victor 20160315					
				return Declaration.WarehouseAddress;
			}
		}

		public DocDeclaration Declaration
		{
			get { return DocDeclaration.New(CusEntryHeader.Declaration, Factory); }
		}

		#region Individual Entry Lines

		public DocCusEntryLine FirstEntryLine
		{
			get
			{
				DocCusEntryLine result = null;
				if (EntryLines.Count > 0)
				{
					result = EntryLines[0];
				}

				return result;
			}
		}

		public DocCusEntryLine SecondEntryLine
		{
			get { return EntryLines.Count > 1 ? EntryLines[1] : null; }
		}

		public DocCusEntryLine ThirdEntryLine
		{
			get { return EntryLines.Count > 2 ? EntryLines[2] : null; }
		}

		public DocCusEntryLine FourthEntryLine
		{
			get { return EntryLines.Count > 3 ? EntryLines[3] : null; }
		}

		public DocCusEntryLine FifthEntryLine
		{
			get { return EntryLines.Count > 4 ? EntryLines[4] : null; }
		}

		public DocCusEntryLine SixthEntryLine
		{
			get { return EntryLines.Count > 5 ? EntryLines[5] : null; }
		}

		public DocCusEntryLine SeventhEntryLine
		{
			get { return EntryLines.Count > 6 ? EntryLines[6] : null; }
		}

		#endregion

		#endregion

		#region Collections

		public DocCusEntryLineCollection EntryLines
		{
			get
			{
				if (fEntryLines == null)
				{
					ICusEntryLine[] lines;
					lines = CusEntryHeader.AllMergedLines;
					fEntryLines = new DocCusEntryLineCollection(Factory);
					fEntryLines.Load(lines);
					fEntryLines.Sort("LineNumber", System.ComponentModel.ListSortDirection.Ascending);
				}

				return fEntryLines;
			}
		}
		protected DocCusEntryLineCollection fEntryLines;

		public DocCusEntryLineCollection EntryLinesWithoutFirstLine
		{
			get
			{
				DocCusEntryLineCollection result = new DocCusEntryLineCollection(Factory);

				ZInt count = 0;

				foreach (DocCusEntryLine currentLine in EntryLines)
				{
					if (count > 0)
					{
						result.Add(currentLine);
					}
					else
					{
						count++;
					}
				}

				return result;
			}
		}

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get
			{
				if (invoiceLines == null)
				{
					invoiceLines = new DocJobComInvoiceLineCollection(Factory);

					foreach (var line in CusEntryHeader.InvoiceLines)
					{
						invoiceLines.Add(new DocJobComInvoiceLine((JobComInvoiceLine)line, Factory));
					}
				}
				return invoiceLines;
			}
		}
		protected DocJobComInvoiceLineCollection invoiceLines;

		public DocEntryHeaderCommercialChargesCollection AllCharges
		{
			get
			{
				if (allCharges == null)
				{
					var helper = new DocEntryHeaderCommercialChargeHelper(Factory);
					helper.PrepareDocEntryHeaderCommercialChargeData(CusEntryHeader.InvoiceLines);
					allCharges = helper.GetDocEntryHeaderCommercialChargesCollection();
				}
				return allCharges;
			}
		}
		protected DocEntryHeaderCommercialChargesCollection allCharges;

		public DocEntryHeaderCommercialChargesCollection Charges
		{
			get
			{
				if (charges == null)
				{
					var list = new List<DocEntryHeaderCommercialCharge>();
					AllCharges.CopyToList(list);

					var keys = list.GroupBy(g => new
					{
						InvoiceNo = g.InvoiceNumber,
						Is_Freight = g.IsFreight,
						Is_Insurance = g.IsInsurance,
						Is_Dutiable = g.IsDutiable,
						Is_IncludedInLines = g.IsIncludedInLines
					})
									.Select(g => new
									{
										InvoiceNumber = g.Key.InvoiceNo,
										IsFreight = g.Key.Is_Freight,
										IsInsurance = g.Key.Is_Insurance,
										IsDutiable = g.Key.Is_Dutiable,
										IsIncludedInLines = g.Key.Is_IncludedInLines
									})
									.ToList();

					charges = new DocEntryHeaderCommercialChargesCollection(Factory);
					keys.ForEach(x => charges.AddRange(list.Where(y => y.InvoiceNumber == x.InvoiceNumber
																	&& y.IsFreight == x.IsFreight
																	&& y.IsInsurance == x.IsInsurance
																	&& y.IsDutiable == x.IsDutiable
																	&& y.IsIncludedInLines == x.IsIncludedInLines)));
				}
				return charges;
			}
		}
		protected DocEntryHeaderCommercialChargesCollection charges;

		#endregion

		#region Overrides

		public override ZDecimal CustomsValue
		{
			get { return ZArchitecture.Core.Utilities.Round(base.CustomsValue, 0); }
		}

		#endregion

		#region ZString Fields
		public ZString UniqueConsignmentNumber
		{
			get { return CusEntryHeader.UniqueConsignmentReference; }
		}

		public DocOrganisation Importer => Declaration.Importer;

		public DocOrganisation Supplier
		{
			get { return DocOrganisation.New(CusEntryHeader.Supplier, Factory); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description result")]
		public ZString PageNumber
		{
			get
			{
				ZString result = ZString.Empty;
				DocCusEntryLineCollection entryLines = EntryLinesWithoutFirstLine;

				if (entryLines.Count > 0)
				{
					ZInt numberofPages = (entryLines.Count / 3) + 2;
					result = "Page 1 of " + numberofPages;
				}

				return result;
			}
		}

		public ZString ImportBillOfEntryContainerSection
		{
			get
			{
				ZString result = GetDescriptionOfPackages();
				result += (!result.IsEmpty && !result.EndsWith("\n")) ? "\n" : "";
				result += Declaration.GoodsDescription;
				return result;
			}
		}

		public ZString TodaysDate
		{
			get { return ZDateTime.Now.ToString("yyyy MM dd"); }
		}

		public ZString CustomsProcedureCode => CusEntryHeader.CustomsProcedureCode;

		public ZString LRN => CusEntryHeader.CH_BGMReference;

		public ZString MRN => CusEntryHeader.MovementReferenceNumber;

		public ZString HouseBill => CusEntryHeader.HAWBOverride;

		#endregion

		#region ZInt Fields

		#region Number and Rank of Entry Headers

		public ZString EndorsementWithEntriesList
		{
			get
			{
				return Declaration.EndorsementWithClearedEntriesList();
			}
		}

		#endregion

		#endregion

		#region ZDecimal Fields

		/// <summary> Used in Reports. If you are using this in a document please update this comment </summary>
		public ZDecimal ValueAddedTax
		{
			get { return CusEntryHeader.ValueAddedTax; }
		}

		public ZInt EntryLinesWithoutFirstLineCount
		{
			get { return EntryLinesWithoutFirstLine.Count; }
		}

		public ZDecimal CIFAndC
		{
			get { return CusEntryHeader.CIFInLocalCurrency?.Amount ?? ZDecimal.Zero; }
		}

		public ZDecimal TransactionValue
		{
			get { return CusEntryHeader.TransactionValue; }
		}

		//Customs Full Duty including Sch1 + Fuel Duty + Countervailing Duty + Sch1P2A - Rebate
		public ZDecimal CustomsDuty
		{
			get { return CusEntryHeader.CustomsDuty; }
		}

		public ZDecimal DutySch1P2B
		{
			get { return CusEntryHeader.S1P2BDutyAfter; }
		}

		public ZDecimal TotalVAT
		{
			get { return CusEntryHeader.ValueAddedTax; }
		}

		#endregion

		#region DateTime Fields

		public ZDateTime AssessmentDate
		{
			get { return CusEntryHeader.EntryInstructionAssessmentDate; }
		}

		#endregion

		#region Bool Fields
		public ZBool ShowChargesBreakdown
		{
			get { return Charges.Count > 0; }
		}
		#endregion

		#region Implementation

		CusEntryHeader CusEntryHeader
		{
			get { return (CusEntryHeader)WrappedObject; }
		}

		protected ZString GetDescriptionOfPackages()
		{
			ZString result = ZString.Empty;
			if (Declaration != null)
			{
				if (Declaration.TransportMode == Core.Constants.TransportModes.Air)
				{
					result = Declaration.MarksAndNumbers;
				}
				else
				{
					if (Declaration.ContainerMode != Core.Constants.ContainerModes.NonContainerised && !Declaration.ContainerMode.IsEmpty)
					{
						SetContainerDetailsAndMarks(Declaration);
						result += fContainerNumbers;
						result += (!result.IsEmpty && !result.EndsWith("\n")) ? "\n" : "";
						result += fContainerCount;
						result += (!result.IsEmpty && !result.EndsWith("\n")) ? "\n" : "";
						result += fMarksAndNumbersForLCLContainers;
						result += (!result.IsEmpty && !result.EndsWith("\n")) ? "\n" : "";
						result += GetPackageDetails(Declaration, fMarksAndNumbersForLCLContainers.IsEmpty);
					}
				}
			}
			return result;
		}

		ZString fContainerCount;
		ZString fContainerNumbers;
		ZString fMarksAndNumbersForLCLContainers;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
		protected void SetContainerDetailsAndMarks(DocDeclaration declarationWrapper)
		{
			if (fContainerCount.IsEmpty && fContainerNumbers.IsEmpty && fMarksAndNumbersForLCLContainers.IsEmpty)
			{
				DocCusContainerCollection containers = declarationWrapper.Containers;

				System.Collections.Hashtable containerType = new System.Collections.Hashtable();
				foreach (DocCusContainer currentContainer in containers)
				{
					fContainerNumbers += currentContainer.ContainerNumber + ", ";

					if (currentContainer.Type == "FCL" && currentContainer.Container != null)
					{
						if (containerType.Contains(currentContainer.Container.Code))
						{
							int containerQty = (int)containerType[currentContainer.Container.Code] + 1;
							containerType[currentContainer.Container.Code] = containerQty;
						}
						else
						{
							containerType.Add(currentContainer.Container.Code, 1);
						}
					}
					else if (fMarksAndNumbersForLCLContainers.IsEmpty)
					{
						fMarksAndNumbersForLCLContainers = declarationWrapper.MarksAndNumbers;
					}
				}

				System.Collections.IDictionaryEnumerator enumerator = containerType.GetEnumerator();
				while (enumerator.MoveNext())
				{
					string plural = ((int)enumerator.Value) > 1 ? "S" : string.Empty;
					fContainerCount += enumerator.Value + " X " + enumerator.Key + " FCL CONTAINER" + plural + ", ";
				}

				fContainerNumbers = fContainerNumbers.TrimEndIncludingWhiteSpace(',');
				fContainerCount = fContainerCount.TrimEndIncludingWhiteSpace(',');
			}
		}

		protected ZString GetPackageDetails(DocDeclaration declarationWrapper, ZBool emptyMarksAndNumbers)
		{
			if (Packages > 0)
			{
				string plural = Packages > 1 ? (NoResString)"s" : string.Empty;
				if (emptyMarksAndNumbers)
				{
					return "STC " + Packages + " " + Declaration.PackTypeDescription + plural;
				}
				else
				{
					return Packages + " " + Declaration.PackTypeDescription + plural + " STC";
				}
			}
			return ZString.Empty;
		}

		#endregion
	}
}
