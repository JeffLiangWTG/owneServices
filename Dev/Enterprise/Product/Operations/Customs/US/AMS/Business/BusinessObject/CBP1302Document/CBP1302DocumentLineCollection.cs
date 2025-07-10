using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CBP1302DocumentLineCollection : NonPersistentBusinessObjectCollection<CBP1302DocumentLine>
	{
		public CBP1302DocumentLineCollection(CusInBondHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}
		readonly CusInBondHeader header;

		public void Initialise1302()
		{
			RemoveAndDeleteAll();
			Populate1302Properties();
		}

		void Populate1302Properties()
		{
			foreach (var bill in header.Bills)
			{
				var addressCount = 0;
				ZBool firstContainerInBill = true;
				showForeignShipper = false;
				showConsignee = false;
				showNotifyParty = false;

				if (!bill.ForeignShipper.AddressAsASingleLine.IsEmpty)
				{
					showForeignShipper = true;
					addressCount++;
				}
				if (!bill.Consignee.AddressAsASingleLine.IsEmpty)
				{
					showConsignee = true;
					addressCount++;
				}
				if (!bill.NotifyParty1.AddressAsASingleLine.IsEmpty)
				{
					showNotifyParty = true;
					addressCount++;
				}

				var containers = bill.MovementDetail.Containers;
				var containerCount = containers.Count;
				if (containerCount > 0)
				{
					foreach (var container in containers)
					{
						var marksAndNos = ZString.Empty;
						var containersMarksSeals = ZString.Empty;
						var packagesAndDescriptions = ZString.Empty;
						var weightAndUnit = ZString.Empty;

						var commodities = container.Commodities;
						if (commodities.Count > 0)
						{
							foreach (var commodity in commodities)
							{
								marksAndNos += commodity.BY_MarksAndNumbers + System.Environment.NewLine;

								if (!(commodity.BY_PieceCount.IsEmpty && commodity.BY_ManifestUnitCode.IsEmpty &&
									commodity.BY_Description.IsEmpty))
								{
									packagesAndDescriptions += commodity.BY_PieceCount + " " + commodity.BY_ManifestUnitCode + " " +
														commodity.BY_Description + System.Environment.NewLine;
								}
								if (!(commodity.BY_GrossWeight.IsEmpty && commodity.BY_GrossWeightUnit.IsEmpty))
								{
									weightAndUnit += commodity.BY_GrossWeight.ToZInt() + " " + commodity.BY_GrossWeightUnit + System.Environment.NewLine;
								}
							}
						}

						if (!marksAndNos.IsEmpty)
						{
							containersMarksSeals += "MN: " + marksAndNos;
						}

						containersMarksSeals += "CN: " + container.BC_ContainerNum + System.Environment.NewLine;

						if (!container.BC_Seal1.IsEmpty || !container.BC_Seal2.IsEmpty)
						{
							containersMarksSeals += "SN: ";

							if (!container.BC_Seal1.IsEmpty)
							{
								containersMarksSeals += container.BC_Seal1 + System.Environment.NewLine;
							}
							if (!container.BC_Seal2.IsEmpty)
							{
								containersMarksSeals += container.BC_Seal2 + System.Environment.NewLine;
							}
						}

						var undgs = container.UNDGs;
						if (undgs.Count > 0)
						{
							packagesAndDescriptions += "UN: ";
							foreach (var undg in undgs)
							{
								if (!undg.DI_DG.IsEmpty)
								{
									packagesAndDescriptions += undg.UNDGSubstance.DG_Code.ToUpperInvariant() + " ";
								}
							}
							packagesAndDescriptions += System.Environment.NewLine;
						}

						weightAndUnit += System.Environment.NewLine;
						packagesAndDescriptions += System.Environment.NewLine;

						var documentLine = new CBP1302DocumentLine(bill);
						documentLine.ContainersMarksSeals = containersMarksSeals;
						documentLine.PackagesAndDescriptions = packagesAndDescriptions;
						documentLine.WeightAndUnit = weightAndUnit;
						documentLine.FirstContainerInBill = firstContainerInBill;

						ShowAddressesOnDocumentLine(documentLine);
						Add(documentLine);

						if (firstContainerInBill)
						{
							firstContainerInBill = false;
						}
					}
				}
				else
				{
					var documentLine = new CBP1302DocumentLine(bill);
					documentLine.FirstContainerInBill = true;
					ShowAddressesOnDocumentLine(documentLine);
					Add(documentLine);
				}

				if (containerCount < addressCount)
				{
					while (addressCount > 0)
					{
						var documentLine = new CBP1302DocumentLine(bill);
						ShowAddressesOnDocumentLine(documentLine);
						addressCount--;
						Add(documentLine);
					}
				}
			}
		}

		ZBool showForeignShipper;
		ZBool showConsignee;
		ZBool showNotifyParty;

		void ShowAddressesOnDocumentLine(CBP1302DocumentLine documentLine)
		{
			if (showForeignShipper)
			{
				documentLine.ShowForeignShipper = true;
				showForeignShipper = false;
			}
			else if (showConsignee)
			{
				documentLine.ShowConsignee = true;
				showConsignee = false;
			}
			else if (showNotifyParty)
			{
				documentLine.ShowNotifyParty = true;
				showNotifyParty = false;
			}
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
