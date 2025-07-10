using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class DGNContainer : DocDataObject
	{
		#region Ctor

		public DGNContainer(object identifier = default)
			: base(identifier)
		{
		}

		#endregion

		#region Number

		public ZString Number
		{
			get => number;
			set
			{
				if (SetNonPersistentPropertyValue(NumberInfo, ref number, value))
				{
					Validate(NumberInfo);
				}
			}
		}

		ZString number;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region PackCount

		public ZInt PackCount
		{
			get => packCount;
			set
			{
				if (SetNonPersistentPropertyValue(PackCountInfo, ref packCount, value))
				{
					Validate(PackCountInfo);
				}
			}
		}

		ZInt packCount;

		public ZPropertyInfo PackCountInfo => GetZPropertyInfo(nameof(PackCount));

		#endregion

		#region PackingLines

		public IReadOnlyCollection<DGNPackingLine> PackingLines
		{
			get => packingLines;
			set => packingLines = SetChildCollection(packingLines, value);
		}

		IReadOnlyCollection<DGNPackingLine> packingLines;

		#endregion

		#region HasDangerousGoods
		public ZBool HasDangerousGoods
		{
			get => hasDangerousGoods;
			set
			{
				if (SetNonPersistentPropertyValue(HasDangerousGoodsInfo, ref hasDangerousGoods, value))
				{
					Validate(HasDangerousGoodsInfo);
				}
			}
		}

		ZBool hasDangerousGoods;

		public ZPropertyInfo HasDangerousGoodsInfo => GetZPropertyInfo(nameof(HasDangerousGoods));

		#endregion HasDangerousGoods

		#region IsNonOperativeReefer

		public ZBool IsNonOperativeReefer
		{
			get => isNonOperativeReefer;
			set
			{
				if (SetNonPersistentPropertyValue(IsNonOperativeReeferInfo, ref isNonOperativeReefer, value))
				{
					Validate(IsNonOperativeReeferInfo);
				}
			}
		}

		ZBool isNonOperativeReefer;

		public ZPropertyInfo IsNonOperativeReeferInfo => GetZPropertyInfo(nameof(IsNonOperativeReefer));

		#endregion
	}
}
