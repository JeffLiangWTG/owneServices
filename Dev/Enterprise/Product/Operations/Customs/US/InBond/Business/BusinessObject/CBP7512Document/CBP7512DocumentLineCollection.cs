using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CBP7512DocumentLineCollection : NonPersistentBusinessObjectCollection<CBP7512DocumentLine>
	{
		public CBP7512DocumentLineCollection(CusInBondMoveHeader moveHeader)
			: base(moveHeader.Factory)
		{
			this.moveHeader = moveHeader;
			PopulateLines();
		}
		readonly CusInBondMoveHeader moveHeader;

		void PopulateLines()
		{
			List<CusInBondMoveDetail> bills = new List<CusInBondMoveDetail>(moveHeader.MovementDetails);
			bills.Sort(new CusInBondMoveDetailComparer());
			List<string> containers = new List<string>();
			List<string> hazardousMaterials = new List<string>();
			var isFTZMove = false;
			var isAir = false;
			var header = moveHeader.Header;
			if (header != null)
			{
				isFTZMove = header.BH_FTZMove;
				isAir = header.IsAir;
			}
			foreach (var moveDetail in bills)
			{
				var bill = moveDetail.Bill;
				foreach (var lineItem in moveDetail.CBP7512Lines.OrderBy(x => x.BI_PrintingSequenceNo))
				{
					Add(lineItem);
				}

				if (isFTZMove && !isAir)
				{
					if (!moveHeader.IsFTZWarehouse)
					{
						foreach (WarehouseDetail warehouseDetail in moveDetail.WarehouseDetails)
						{
							if (!warehouseDetail.US_WarehouseNumber.IsEmpty)
							{
								Add(new CBP7512DocumentLine() { DescriptionAndQtyOfMerchandise = GetEntryDetails(warehouseDetail) });
							}
						}
					}
				}
				else
				{
					ZString billNumber = GetBillNumber(bill, isAir);
					if (!billNumber.IsEmpty)
					{
						Add(new CBP7512DocumentLine() { DescriptionAndQtyOfMerchandise = "BOL: " + billNumber });
					}
				}

				ZStringBuilder additionalReferences = new ZStringBuilder();
				foreach (CusInbondBillAddRef billAddRef in bill.AdditionalReferences)
				{
					if (billAddRef.BR_Qualifier != ReferenceQualifierList.Codes.FEN)
					{
						additionalReferences.Append(billAddRef.BR_Qualifier + " - " + billAddRef.BR_QualifierDescription + ": " + billAddRef.BR_ReferenceNum);
					}
				}

				if (!additionalReferences.IsEmpty)
				{
					additionalReferences.Prepend("ADDITIONAL REFERENCE(S):");
					Add(new CBP7512DocumentLine() { DescriptionAndQtyOfMerchandise = additionalReferences.ToStringWithNewLineBetweenAppends() });
				}

				foreach (var container in moveDetail.Containers)
				{
					if (!container.IsNonContainerized)
					{
						string containerData = "CTNR # " + container.BC_ContainerNum + (container.BC_Seal1.IsEmpty ? "" : "   SEAL # " + container.BC_Seal1);
						if (!containers.Contains(containerData))
						{
							containers.Add(containerData);
						}
					}

					foreach (var undg in container.UNDGs)
					{
						var hazardous = new HazardousMaterial(undg);
						ZStringBuilder hazardousBuilder = new ZStringBuilder();
						hazardousBuilder.Append("UN" + hazardous.HazMatCode);
						hazardousBuilder.AppendIfNotEmpty(hazardous.HazMatDesc);
						if (!hazardous.HazMatClass.IsEmpty)
						{
							ZString classDetail = "class " + hazardous.HazMatClass;
							if (!hazardous.HazMatQualifier.IsEmpty)
							{
								classDetail += " " + hazardous.HazMatQualifier;
							}
							hazardousBuilder.Append(classDetail);
						}

						if (hazardous.IsFlashPointTempRelevant)
						{
							hazardousBuilder.Append("(" + hazardous.FlashPointTemp.ToString() + "C c.c.)");
						}

						hazardousBuilder.AppendIfNotEmpty(hazardous.HazMatClassificationDesc);
						if (!hazardousBuilder.IsEmpty)
						{
							ZString hazardousMaterial = hazardousBuilder.ToStringWithDelimiterBetweenAppends(", ");
							if (!hazardousMaterials.Contains(hazardousMaterial))
							{
								hazardousMaterials.Add(hazardousMaterial);
							}
						}
					}
				}
			}

			ZStringBuilder builder = new ZStringBuilder(containers);
			foreach (string hazardousMaterial in hazardousMaterials)
			{
				builder.Append(hazardousMaterial);
			}

			if (!moveHeader.BM_AdditionalText.IsEmpty)
			{
				if (!builder.IsEmpty)
				{
					builder.Append("");
				}
				builder.Append(moveHeader.BM_AdditionalText);
			}

			var parent = moveHeader.Header.Parent;
			if (parent != null)
			{
				if (!parent.HouseBill.IsEmpty)
				{
					if (!builder.IsEmpty)
					{
						builder.Append("");
					}

					var houseBillBuilder = new ZStringBuilder();
					houseBillBuilder.Append(isAir ? "HAWB:" : "HBOL:");
					if (!isAir)
					{
						var validHouseBillIssuerCodes = new List<ZString>();
						if (parent is ForwardingShipment shipment)
						{
							validHouseBillIssuerCodes.AddRange(shipment.GetValidHouseBillIssuerCodes());
						}
						else if (parent is JobDeclaration declaration && !declaration.JE_HouseBillIssuerSCAC.IsEmpty)
						{
							validHouseBillIssuerCodes.Add(declaration.JE_HouseBillIssuerSCAC);
						}

						var houseBillIssuerCode = ZString.Empty;
						if (!parent.HouseBill.ShouldTrimSCACFromBills(validHouseBillIssuerCodes))
						{
							houseBillIssuerCode = validHouseBillIssuerCodes.FirstOrDefault();
						}

						houseBillBuilder.AppendIfNotEmpty(houseBillIssuerCode);
					}

					houseBillBuilder.Append(parent.HouseBill);
					builder.Append(houseBillBuilder.ToStringWithDelimiterBetweenAppends(" "));
				}
			}

			if (!builder.IsEmpty)
			{
				builder.Prepend("");
				Add(new CBP7512DocumentLine() { DescriptionAndQtyOfMerchandise = builder.ToStringWithNewLineBetweenAppends() });
			}
		}

		string GetEntryDetails(WarehouseDetail warehouseDetail)
		{
			var result = new ZStringBuilder(ZString.Format("ENTRY: {0}", warehouseDetail.US_WarehouseNumber));
			result.Append(ZString.Format("BONDED:{0};WITHDRAW:{1};BAL:{2}", warehouseDetail.US_WarehouseBondedQuantity.ToString(0), warehouseDetail.US_WarehouseWithdrawQuantity.ToString(0), warehouseDetail.WarehouseBondedQuantityBalance.ToString(0)));
			result.Append(DottedLine);
			return result.ToStringWithNewLineBetweenAppends();
		}

		internal const string DottedLine = "---------------------------------------------------------------------------------";

		ZString GetBillNumber(CusInBondBill bill, bool isAir)
		{
			var result = new ZStringBuilder();
			result.AppendIfNotEmpty(bill.IssuerCodeAndMasterBillNumber);
			if (isAir)
			{
				result.Append(" (");
				result.Append(bill.B0_HouseBillNumber);
				result.Append(")");
			}
			return result.ToString();
		}

		void Add(CusInBondMoveLineItem lineItem)
		{
			CBP7512DocumentLine line = new CBP7512DocumentLine();
			line.UpdateFrom(lineItem);
			Add(line);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
