using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	internal class ForwardersCargoReceiptSummary : ForwardersCargoReceipt
	{
		#region TotalWeightKG

		public ZDecimal TotalWeightKG
		{
			get => totalWeightKG;
			set
			{
				if (SetNonPersistentPropertyValue(TotalWeightKGInfo, ref totalWeightKG, value))
				{
					Validate(TotalWeightKGInfo);
				}
			}
		}

		ZDecimal totalWeightKG;

		public ZPropertyInfo TotalWeightKGInfo => GetZPropertyInfo(nameof(TotalWeightKG));

		#endregion

		#region TotalVolumeM3

		public ZDecimal TotalVolumeM3
		{
			get => totalVolumeM3;
			set
			{
				if (SetNonPersistentPropertyValue(TotalVolumeM3Info, ref totalVolumeM3, value))
				{
					Validate(TotalVolumeM3Info);
				}
			}
		}

		ZDecimal totalVolumeM3;

		public ZPropertyInfo TotalVolumeM3Info => GetZPropertyInfo(nameof(TotalVolumeM3));

		#endregion

		#region TotalPackages

		public ZInt TotalPackages
		{
			get => totalPackages;
			set
			{
				if (SetNonPersistentPropertyValue(TotalPackagesInfo, ref totalPackages, value))
				{
					Validate(TotalPackagesInfo);
				}
			}
		}

		ZInt totalPackages;

		public ZPropertyInfo TotalPackagesInfo => GetZPropertyInfo(nameof(TotalPackages));

		#endregion

		#region TotalPackagesUnit

		public ICodeDescription TotalPackagesUnit
		{
			get => totalPackagesUnit;
			set => totalPackagesUnit = SetChild(totalPackagesUnit, value);
		}

		ICodeDescription totalPackagesUnit;

		#endregion

		#region TotalOrderLines

		public ZInt TotalOrderLines
		{
			get => totalOrderLines;
			set
			{
				if (SetNonPersistentPropertyValue(TotalOrderLinesInfo, ref totalOrderLines, value))
				{
					Validate(TotalOrderLinesInfo);
				}
			}
		}

		ZInt totalOrderLines;

		public ZPropertyInfo TotalOrderLinesInfo => GetZPropertyInfo(nameof(TotalOrderLines));

		#endregion

		#region OrderLineNumbers

		public ZString OrderLineNumbers
		{
			get => orderLineNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(OrderLineNumbersInfo, ref orderLineNumbers, value))
				{
					Validate(OrderLineNumbersInfo);
				}
			}
		}

		ZString orderLineNumbers;

		public ZPropertyInfo OrderLineNumbersInfo => GetZPropertyInfo(nameof(OrderLineNumbers));

		#endregion

		#region ItemNumbers

		public ZString ItemNumbers
		{
			get => itemNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(ItemNumbersInfo, ref itemNumbers, value))
				{
					Validate(ItemNumbersInfo);
				}
			}
		}

		ZString itemNumbers;

		public ZPropertyInfo ItemNumbersInfo => GetZPropertyInfo(nameof(ItemNumbers));

		#endregion

		#region Particular Furnished by Shipper

		public ZString ParticularFurnishedByShipper
		{
			get => particularFurnishedByShipper;
			set
			{
				if (SetNonPersistentPropertyValue(ParticularFurnishedByShipperInfo, ref particularFurnishedByShipper, value))
				{
					Validate(ParticularFurnishedByShipperInfo);
				}
			}
		}

		ZString particularFurnishedByShipper;

		public ZPropertyInfo ParticularFurnishedByShipperInfo => GetZPropertyInfo(nameof(ParticularFurnishedByShipper));

		#endregion
	}
}
