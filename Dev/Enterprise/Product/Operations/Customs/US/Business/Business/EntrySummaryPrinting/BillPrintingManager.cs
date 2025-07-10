using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class BillPrintingManager
	{
		public BillPrintingManager(CusEntryHeader entryHeader, EDIMessage bluMessage, BusinessObjectFactory factory, EntrySummary7501BillCollection bills)
		{
			this.bluMessage = bluMessage;
			this.factory = factory;
			this.entryHeader = entryHeader;
			billObjectCollection = bills;
		}
		readonly EDIMessage bluMessage;
		readonly BusinessObjectFactory factory;
		readonly CusEntryHeader entryHeader;
		readonly EntrySummary7501BillCollection billObjectCollection;

		[WTG.StaticAnalysis.Annotation.CodeAlive("Used in this code 4 times in ParseBlock23")]
		enum ManifestTypeCode { I, M, H, S }
		ZDateTime itDate = ZDateTime.Empty;
		internal ZString BLUFirmsCode;
		internal ZString BLUVoyageFlight;

		internal ZString SCACAndMBillNumber
		{
			get
			{
				if (!sCACAndMBillNumberCached.HasValue)
				{
					var result = ZString.Empty;
					foreach (EntrySummary7501Bill billObject in billObjectCollection)
					{
						if (result.IsEmpty && !billObject.MasterBill.IsEmpty)
						{
							result = billObject.EffectiveMasterBillIssuerSCAC + billObject.MasterBill;
						}
					}
					sCACAndMBillNumberCached = result;
				}

				return sCACAndMBillNumberCached.Value;
			}
		}
		ZString? sCACAndMBillNumberCached;

		internal ZDateTime FirstBillITDate
		{
			get { return FirstBillWithITNumber != null ? FirstBillWithITNumber.ITDate : ZDateTime.Empty; }
		}

		internal ZString FirstBillITNO
		{
			get { return FirstBillWithITNumber != null ? FirstBillWithITNumber.ITNO : ZString.Empty; }
		}

		EntrySummary7501Bill FirstBillWithITNumber
		{
			get
			{
				if (firstBillWithITNumber == null)
				{
					foreach (EntrySummary7501Bill billObject in billObjectCollection)
					{
						if (!billObject.ITNO.IsEmpty)
						{
							firstBillWithITNumber = billObject;
							break;
						}
					}
				}
				return firstBillWithITNumber;
			}
		}
		EntrySummary7501Bill firstBillWithITNumber;

		internal void PopulateElements(IEnumerable<MessageBlock> blocksFrom7501)
		{
			var billDetailsList = new List<BillDetails>();
			BillDetails billDetails = null;

			foreach (var block in blocksFrom7501)
			{
				switch (block.MandatoryCharacters)
				{
					case "20":
						var aens20 = block as AENS20;
						if (aens20 != null)
						{
							itDate = aens20.InBondInTransitDate;
						}
						break;
					case "22":
						billDetails = ParseBlock22((IBlock22BillDetails)block);
						billDetailsList.Add(billDetails);
						break;
					case "23":
						ParseBlock23((AENS23)block, billDetails);
						break;
				}
			}

			PopulateBillObjectCollectionFromBLU(billDetailsList);
		}

		BillDetails ParseBlock22(IBlock22BillDetails block)
		{
			var billDetails = new BillDetails();
			billDetails.Quantity = block.Quantity;
			billDetails.Unit = block.Unit;
			billDetails.ITNo = block.ITNo;
			billDetails.MasterBillNumber = block.MasterBillNumber;
			billDetails.IssuerCodeOfMasterBillNumber = block.IssuerCodeOfMasterBillNumber;
			billDetails.HouseBillNumber = block.HouseBillNumber;
			billDetails.IssuerCodeOfHouseBillNumber = block.IssuerCodeOfHouseBillNumber;
			billDetails.SubHouseBillNumber = block.SubHouseBillNumber;
			if (!block.ITDate.IsEmpty)
			{
				itDate = block.ITDate;
			}

			if (bluMessage == null)
			{
				var billObject = new EntryMessageENS7501Bill(entryHeader.PK, factory, itDate.Date, billDetails);
				billObjectCollection.Add(billObject);
			}
			return billDetails;
		}

		void ParseBlock23(AENS23 block, BillDetails billDetails)
		{
			if (billDetails != null)
			{
				if (block.ManifestComponentTypeCode == nameof(ManifestTypeCode.I))
				{
					billDetails.ITNo = block.ManifestComponentIdentifier;
				}
				else if (block.ManifestComponentTypeCode == nameof(ManifestTypeCode.M))
				{
					billDetails.MasterBillNumber = block.ManifestComponentIdentifier;
					billDetails.IssuerCodeOfMasterBillNumber = block.ManifestComponentIssuerCode;
				}
				else if (block.ManifestComponentTypeCode == nameof(ManifestTypeCode.H))
				{
					billDetails.HouseBillNumber = block.ManifestComponentIdentifier;
					billDetails.IssuerCodeOfHouseBillNumber = block.ManifestComponentIssuerCode;
				}
				else if (block.ManifestComponentTypeCode == nameof(ManifestTypeCode.S))
				{
					billDetails.SubHouseBillNumber = block.ManifestComponentIdentifier;
					billDetails.IssuerCodeOfSubHouseBillNumber = block.ManifestComponentIssuerCode;
				}
			}
		}

		void PopulateBillObjectCollectionFromBLU(List<BillDetails> billDetailsList)
		{
			if (bluMessage != null)
			{
				if (bluMessage.EM_MessageType == ApplicationIdentifierCodeList.Codes.BillofLadingUpdate)
				{
					ProcessLegacyBLUMessage(billDetailsList);
				}
				else
				{
					ProcessACEBLUMessage();
				}
			}
		}

		#region Process Legacy BLU Message

		void ProcessLegacyBLUMessage(List<BillDetails> billDetailsList)
		{
			List<BOLL3> boll3s = new List<BOLL3>();
			foreach (MessageBlock block in bluMessage.MessageBlock.MessageBlocks)
			{
				var boll1 = block as BOLL1;
				if (boll1 != null)
				{
					BLUFirmsCode = boll1.LocationOfGoods;
					BLUVoyageFlight = boll1.VoyageFlightNumber;
				}
				else
				{
					var boll3 = block as BOLL3;
					if (boll3 != null)
					{
						boll3s.Add(boll3);
					}
				}
			}

			AddOrUpdateBollDetails(boll3s, billDetailsList);
			boll3s.ForEach(boll3 => billObjectCollection.Add(new EntryMessageENS7501Bill(entryHeader.PK, factory, itDate.Date, boll3)));
		}

		void AddOrUpdateBollDetails(List<BOLL3> boll3s, List<BillDetails> billDetailsList)
		{
			var newBoll3List = new List<BOLL3>(boll3s);
			newBoll3List.ForEach(boll3 => billDetailsList.ForEach(billDetails =>
			{
				if ((boll3.MasterBillNumber.IsEmpty || boll3.MasterBillNumber == billDetails.MasterBillNumber) &&
					(boll3.HouseBillNumber.IsEmpty || boll3.HouseBillNumber == billDetails.HouseBillNumber) &&
					(boll3.SubHouseBillNumber.IsEmpty || boll3.SubHouseBillNumber == billDetails.SubHouseBillNumber))
				{
					BOLL3 missingBoll3 = boll3;

					bool billDetailITNumberSentInBLU = billDetails.ITNo.StartsWith("V") || USCustomsDataRegistry.Instance.SendAllITNumbersInBOLMessage.Value;
					if (!billDetails.ITNo.IsEmpty && !billDetailITNumberSentInBLU)
					{
						missingBoll3 = new BOLL3();
						boll3s.Add(missingBoll3);

						if (boll3.InBondNumber.IsEmpty)
						{
							boll3s.Remove(boll3);
						}

						missingBoll3.MasterBillNumber = billDetails.MasterBillNumber;
						missingBoll3.IssuerCodeOfMasterBillNumber = billDetails.IssuerCodeOfMasterBillNumber;
						missingBoll3.HouseBillNumber = billDetails.HouseBillNumber;
						missingBoll3.IssuerCodeOfHouseBillNumber = billDetails.IssuerCodeOfHouseBillNumber;
						missingBoll3.SubHouseBillNumber = billDetails.SubHouseBillNumber;
						missingBoll3.IssuerCodeOfSubHouseBillNumber = billDetails.IssuerCodeOfSubHouseBillNumber;
						missingBoll3.ManifestQuantity = billDetails.Quantity;
						missingBoll3.Unit = billDetails.Unit;
					}

					if (missingBoll3.InBondNumber.IsEmpty)
					{
						missingBoll3.InBondNumber = billDetails.ITNo;
					}
				}
			}));
		}

		#endregion

		#region Process ACE BLU Message

		void ProcessACEBLUMessage()
		{
			BillDetails billDetails = null;
			var quantityFromSE16s = ZInt.Zero;
			var unitFromFirstSE16 = ZString.Empty;
			var declarationPackUQ = Declaration != null ? Declaration.JE_TotalNoOfPacksPackType : ZString.Empty;
			var previousBillType = ZString.Empty;

			foreach (MessageBlock block in bluMessage.MessageBlock.MessageBlocks)
			{
				var se15 = block as ASESE15;
				if (se15 != null)
				{
					switch (se15.BillTypeIndicator)
					{
						case SEBillTypesList.Codes.InBond:
							if (billDetails != null)
							{
								AddToBillCollection(billDetails);
								UpdateQuantity(billDetails, quantityFromSE16s, unitFromFirstSE16);
								quantityFromSE16s = 0;
								unitFromFirstSE16 = ZString.Empty;
							}

							billDetails = new BillDetails();
							billDetails.ITNo = se15.BillOfLadingNumber;
							UpdateQuantity(billDetails, se15.Quantity, declarationPackUQ);
							break;

						case SEBillTypesList.Codes.RegularBill:
						case SEBillTypesList.Codes.MasterBill:
							if (billDetails != null && previousBillType != SEBillTypesList.Codes.InBond)
							{
								AddToBillCollection(billDetails);
								UpdateQuantity(billDetails, quantityFromSE16s, unitFromFirstSE16);
								quantityFromSE16s = 0;
								unitFromFirstSE16 = ZString.Empty;
							}

							if (billDetails == null || previousBillType != SEBillTypesList.Codes.InBond)
							{
								billDetails = new BillDetails();
							}

							UpdateMasterBill(billDetails, se15.BillOfLadingNumber, se15.IssuerCodeOfBillOfLadingNumber);
							UpdateQuantity(billDetails, se15.Quantity, declarationPackUQ);
							break;
						case SEBillTypesList.Codes.HouseBill:
							if (billDetails == null)
							{
								billDetails = new BillDetails();
							}

							UpdateHouseBill(billDetails, se15.BillOfLadingNumber, se15.IssuerCodeOfBillOfLadingNumber);
							UpdateQuantity(billDetails, se15.Quantity, declarationPackUQ);
							break;
						case SEBillTypesList.Codes.SubHouseBill:
							if (billDetails == null)
							{
								billDetails = new BillDetails();
							}

							UpdateSubHouseBill(billDetails, se15.BillOfLadingNumber, se15.IssuerCodeOfBillOfLadingNumber);
							UpdateQuantity(billDetails, se15.Quantity, declarationPackUQ);
							break;
					}

					previousBillType = se15.BillTypeIndicator;
				}
				else
				{
					var se16 = block as ASESE16;
					if (se16 != null)
					{
						quantityFromSE16s += se16.Quantity;
						if (unitFromFirstSE16.IsEmpty)
						{
							unitFromFirstSE16 = se16.UnitOfMeasure;
						}
					}
				}
			}

			if (billDetails != null)
			{
				AddToBillCollection(billDetails);
				UpdateQuantity(billDetails, quantityFromSE16s, unitFromFirstSE16);
			}
		}

		void AddToBillCollection(BillDetails billDetails)
		{
			billObjectCollection.Add(new EntryMessageENS7501Bill(entryHeader.PK, factory, itDate.Date, billDetails));
		}

		void UpdateMasterBill(BillDetails billDetails, ZString masterBill, ZString masterBillIssuerCode)
		{
			if (!masterBill.IsEmpty)
			{
				billDetails.MasterBillNumber = masterBill;
				billDetails.IssuerCodeOfMasterBillNumber = masterBillIssuerCode;
			}
		}

		void UpdateHouseBill(BillDetails billDetails, ZString bill, ZString billIssuerCode)
		{
			if (!bill.IsEmpty)
			{
				billDetails.HouseBillNumber = bill;
				billDetails.IssuerCodeOfHouseBillNumber = billIssuerCode;
			}
		}

		void UpdateSubHouseBill(BillDetails billDetails, ZString bill, ZString billIssuerCode)
		{
			if (!bill.IsEmpty)
			{
				billDetails.SubHouseBillNumber = bill;
				billDetails.IssuerCodeOfSubHouseBillNumber = billIssuerCode;
			}
		}

		void UpdateQuantity(BillDetails billDetails, ZInt quantity, ZString uq)
		{
			if (!quantity.IsEmpty)
			{
				billDetails.Quantity = quantity;
				billDetails.Unit = uq;
			}
		}

		#endregion

		#region New Properties

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = entryHeader.Declaration); }
		}
		JobDeclaration declaration;

		#endregion
	}

	public class BillDetails : IBlock22BillDetails
	{
		public ZString MasterBillNumber
		{
			get;
			set;
		}

		public ZString IssuerCodeOfMasterBillNumber
		{
			get;
			set;
		}

		public ZString HouseBillNumber
		{
			get;
			set;
		}

		public ZString IssuerCodeOfHouseBillNumber
		{
			get;
			set;
		}

		public ZString SubHouseBillNumber
		{
			get;
			set;
		}

		public ZString IssuerCodeOfSubHouseBillNumber
		{
			get;
			set;
		}

		public ZString ITNo
		{
			get;
			set;
		}

		public ZInt Quantity
		{
			get;
			set;
		}

		public ZString Unit
		{
			get;
			set;
		}

		public ZDate ITDate
		{
			get;
			set;
		}
	}
}
