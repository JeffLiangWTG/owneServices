using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	class ShippedOnBoard : CodeDescription, IShippedOnBoard
	{
		public ShippedOnBoard(ICodeDescriptionPairList codes)
			: base(codes)
		{
		}

		public ShippedOnBoard(IFindBoxListProvider codes) : base(codes)
		{
		}

		#region Date

		public ZDateTime Date
		{
			get => date;
			set
			{
				if (SetNonPersistentPropertyValue(DateInfo, ref date, value))
				{
				}
			}
		}

		ZDateTime date;

		public ZPropertyInfo DateInfo => GetZPropertyInfo(nameof(Date));

		#endregion
	}
}
