using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveLineItemCollection : ActiveBusinessObjectCollection<CusInBondMoveLineItem>
	{
		public CusInBondMoveLineItemCollection(CusInBondMoveDetail master)
			: base(master)
		{
		}

		internal void DefaultBondedWhsDataFor7512Document()
		{
			if (!defaultBondedWhsDataFor7512DocumentInProgress)
			{
				try
				{
					defaultBondedWhsDataFor7512DocumentInProgress = true;
					var moveHeader = MoveHeader;
					if (moveHeader != null && moveHeader.IsFTZWarehouse)
					{
						AddBlankLineToIndicateFTZMerchandiseifRequired();
					}
					foreach (var container in MoveDetail.Containers.OrderBy(x => GetContainerOrderKey(x)))
					{
						foreach (var commodity in container.Commodities.OrderBy(x => GetCommodityOrderKey(x)))
						{
							DefaultBondedWhsDataFor7512Document(commodity);
						}
					}
				}
				finally
				{
					defaultBondedWhsDataFor7512DocumentInProgress = false;
				}
			}
		}

		void AddBlankLineToIndicateFTZMerchandiseifRequired()
		{
			if (MoveDetail.Containers.Any(x => x.Commodities.Count > 0))
			{
				var line = AddNew_Empty();
				var description = new ZStringBuilder();
				description.Append(" ");
				description.Append(" ");
				description.Append("MERCHANDISE IS FOREIGN TRADE");
				description.Append("MERCHANDISE - SEE BELOW");
				description.Append(" ");
				description.Append(" ");
				line.BI_Description = ((ZString)description.ToStringWithNewLineBetweenAppends()).Left(CusInBondMoveLineItem.Schema.BI_DescriptionMaxLength);
			}
		}

		ZString GetCommodityOrderKey(CusInBondCargoDesc commodity)
		{
			var result = new ZStringBuilder(commodity.BY_PartNumber.PadRight(CusInBondCargoDesc.Schema.BY_PartNumberMaxLength));
			result.Append(commodity.BY_HarmonisedTariff.PadRight(CusInBondCargoDesc.Schema.BY_HarmonisedTariffMaxLength));
			result.Append(commodity.BY_Description.PadRight(CusInBondCargoDesc.Schema.BY_PartNumberMaxLength));
			return result.ToString();
		}

		ZString GetContainerOrderKey(CusInBondContainer container)
		{
			var result = new ZStringBuilder(container.BC_ContainerNum.PadRight(CusInBondContainer.Schema.BC_ContainerNumMaxLength));
			result.Append(container.BC_Seal1.PadRight(CusInBondContainer.Schema.BC_Seal1MaxLength));
			result.Append(container.BC_Seal2.PadRight(CusInBondContainer.Schema.BC_Seal2MaxLength));
			result.Append(container.PK.ToString());
			return result.ToString();
		}

		CusInBondMoveDetail MoveDetail
		{
			get { return (CusInBondMoveDetail)Relationship.Master; }
		}

		CusInBondMoveHeader MoveHeader
		{
			get { return MoveDetail.MoveHeader; }
		}

		protected override void SetDefaultsForNewElementCore(CusInBondMoveLineItem newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (!defaultBondedWhsDataFor7512DocumentInProgress && Count == 0)
			{
				DefaultFromBill(newElement);
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		bool defaultBondedWhsDataFor7512DocumentInProgress;

		void DefaultBondedWhsDataFor7512Document(CusInBondCargoDesc commodity)
		{
			var line = AddNew_Empty();

			if (commodity.BY_MarksAndNumbers != Core.Constants.ContainerMarking.NoMarks)
			{
				line.BI_MarksAndNumbers = commodity.BY_MarksAndNumbers.Left(CusInBondMoveLineItem.Schema.BI_MarksAndNumbersMaxLength);
			}
			var description = new ZStringBuilder();
			if (commodity.BY_PieceCount > ZInt.Zero)
			{
				var quantity = commodity.BY_PieceCount.ToString() + " " + commodity.BY_ManifestUnitCode;
				description.AppendIfNotEmpty(quantity.TrimEnd());
			}
			description.AppendIfNotEmpty(commodity.BY_Description);
			AddToDescriptionTrimStart(line, description.ToStringWithDelimiterBetweenAppends(" "));
			if (commodity.BY_PartNumber.IsEmpty && !commodity.HasChildCommodities)
			{
				DefaultWeightAndMonetaryValueFor7512Document(line, commodity);
			}
			else if (commodity.HasChildCommodities)
			{
				DefaultChildCommoditiesFor7512Document(line, commodity);
			}
			else if (!commodity.BY_PartNumber.IsEmpty)
			{
				DefaultProductWithoutChildCommodiesFor7512Document(line, commodity);
			}
		}

		void DefaultWeightAndMonetaryValueFor7512Document(CusInBondMoveLineItem line, CusInBondCargoDesc commodity)
		{
			UpdateWeight(line, commodity.BY_GrossWeight, commodity.BY_GrossWeightUnit);
			line.BI_MonetaryValue = commodity.BY_MonetaryValue;
		}

		void DefaultChildCommoditiesFor7512Document(CusInBondMoveLineItem line, CusInBondCargoDesc commodity)
		{
			foreach (CusInBondCargoDesc childCommodity in commodity.ChildCommodities)
			{
				line = AddNew_Empty();
				if (!childCommodity.BY_PartNumber.IsEmpty)
				{
					line.BI_Description = GetProductDetails(childCommodity);
					if (childCommodity.HasChildCommodities)
					{
						foreach (CusInBondCargoDesc childChildCommodity in childCommodity.ChildCommodities)
						{
							line = AddNew_Empty();
							line.BI_Description = childChildCommodity.BY_FormattedHarmonisedTariff;
							DefaultWeightAndMonetaryValueFor7512Document(line, childChildCommodity);
						}
					}
					else
					{
						line = AddNew_Empty();
						line.BI_Description = childCommodity.BY_FormattedHarmonisedTariff;
						DefaultWeightAndMonetaryValueFor7512Document(line, childCommodity);
					}
				}
				else
				{
					line.BI_Description = childCommodity.BY_FormattedHarmonisedTariff;
					DefaultWeightAndMonetaryValueFor7512Document(line, childCommodity);
				}
			}
		}

		void AddToDescriptionTrimStart(CusInBondMoveLineItem line, ZString descriptionPrefix, ZStringBuilder descriptionBuilder)
		{
			bool hasZoneStatusCode = false;
			if (!descriptionPrefix.IsEmpty)
			{
				descriptionBuilder.Prepend(descriptionPrefix);
				hasZoneStatusCode = ZoneStatusCodesList.GetAllCodes().Any(x => descriptionPrefix.EndsWith(ZString.Format("\r\n{0}", ZoneStatusCodesList.GetDescriptionFromCode(x)), StringComparison.CurrentCultureIgnoreCase));
			}
			AddToDescriptionTrimStart(line, hasZoneStatusCode ? descriptionBuilder.ToStringWithDelimiterBetweenAppends(" ") : descriptionBuilder.ToStringWithNewLineBetweenAppends());
		}

		ZoneStatusCodeList ZoneStatusCodesList
		{
			get { return Factory.GetCachedValue<ZoneStatusCodeList>(); }
		}

		void AddToDescriptionTrimStart(CusInBondMoveLineItem line, ZString description)
		{
			line.BI_Description = description.TrimStart().Left(CusInBondMoveLineItem.Schema.BI_DescriptionMaxLength);
		}

		CusInBondMoveLineItem AddNew_Empty()
		{
			var result = AddNew();
			result.BI_MarksAndNumbers = ZString.Empty;
			return result;
		}

		void DefaultProductWithoutChildCommodiesFor7512Document(CusInBondMoveLineItem line, CusInBondCargoDesc commodity)
		{
			var bondedWhsDetails = new ZStringBuilder(GetProductDetails(commodity));
			if (!commodity.BY_HarmonisedTariff.IsEmpty)
			{
				AddToDescriptionTrimStart(line, line.BI_Description, bondedWhsDetails);
				line = AddNew_Empty();
				line.BI_MarksAndNumbers = ZString.Empty;
				bondedWhsDetails = new ZStringBuilder(commodity.BY_FormattedHarmonisedTariff);
			}
			AddToDescriptionTrimStart(line, line.BI_Description, bondedWhsDetails);
			DefaultWeightAndMonetaryValueFor7512Document(line, commodity);
		}

		string GetProductDetails(CusInBondCargoDesc commodity)
		{
			return "PRODUCT:" + commodity.BY_PartNumber;
		}

		void DefaultFromBill(CusInBondMoveLineItem moveLine)
		{
			CusInBondBill bill = MoveDetail.Bill;
			IInBondBillDetails inbodDetail = MoveDetail;
			ZInt quantity = MoveDetail.B9_InBoundQty;
			if (quantity.IsEmpty && bill != null)
			{
				quantity = bill.B0_ManifestQty;
			}

			moveLine.BI_Description = quantity.ToString();
			var moveHeader = MoveHeader;
			if (moveHeader != null)
			{
				moveLine.BI_MonetaryValue = moveHeader.BM_MonetaryValue;
			}

			if (bill != null)
			{
				moveLine.BI_Description += " " + bill.B0_ManifestUQ;
				UpdateWeight(moveLine, inbodDetail.WeightInWholeNumber, inbodDetail.WeightUQ);
			}
		}

		void UpdateWeight(CusInBondMoveLineItem moveLine, ZDecimal weightValue, ZString weightUQ)
		{
			var weight = new ZWeight(weightValue, weightUQ);
			if (Core.Constants.Weight.IsImperial(weight.Unit))
			{
				moveLine.BI_Weight = Math.Ceiling(weight.InPoundsSafe);
				moveLine.BI_WeightUnit = WeightUnitList.Codes.Pounds;
			}
			else
			{
				moveLine.BI_Weight = Math.Ceiling(weight.InKilogramsSafe);
				moveLine.BI_WeightUnit = WeightUnitList.Codes.Kilograms;
			}
		}
	}
}
