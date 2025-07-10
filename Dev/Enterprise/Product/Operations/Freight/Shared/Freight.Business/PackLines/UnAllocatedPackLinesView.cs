using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Collection of Packlines that are not allocated to containers
	/// </summary>
	public abstract class UnAllocatedPackLinesView : BusinessObjectCollectionView<PackLine>
	{
		protected UnAllocatedPackLinesView(PackLinesForSailingsCollection packLines)
			: base(packLines)
		{
			packLines.IsManagedForDataRefresh = true;
		}

		protected UnAllocatedPackLinesView(PackLineNonDependentCollection packLines)
			: base(packLines)
		{
			packLines.IsManagedForDataRefresh = true;
		}

		#region Overrides

		#region AllowNew

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return ShouldIncludeThisPackLine((PackLine)element);
		}

		#endregion

		#endregion

		#region Implementation

		protected abstract bool IsPacked(PackLine line);

		protected virtual CommonShipment GetShipmentFromLine(PackLine line)
		{
			return (line != null) ? line.Shipment : null;
		}

		protected virtual bool ShouldIncludeThisPackLine(PackLine line)
		{
			bool result = line.IsOuterPackType && !IsPacked(line);

			if (result && ShowOnlyReceived)
			{
				CommonShipment shipment = GetShipmentFromLine(line);
				result = (shipment == null || shipment.Sailing == null || shipment.IsReceived);
			}

			return result;
		}

		#endregion

		#region Properties

		#region ShowOnlyReceived

		ZBool fShowOnlyReceived = true;
		public ZBool ShowOnlyReceived
		{
			get { return fShowOnlyReceived; }
			set
			{
				fShowOnlyReceived = value;
				Rebuild();
			}
		}

		#endregion

		#region TotalPacks

		public ZInt TotalPacks
		{
			get
			{
				ZInt result = 0;
				foreach (PackLine line in this)
				{
					result += line.JL_PackageCount;
				}
				return result;
			}
		}

		#endregion

		#endregion
	}
}
