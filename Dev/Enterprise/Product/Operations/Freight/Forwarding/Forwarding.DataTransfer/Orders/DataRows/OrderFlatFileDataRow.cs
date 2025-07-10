using Enterprise.DataTransfer.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public enum RecordTypes
	{
		NONE = 0,
		POH = 1,
		POL = 2,
		PLD = 3,
		INV = 4
	}

	public class OrderFlatFileDataRow : FlatFileDataRow
	{
		public OrderFlatFileDataRow(FlatFileDataRow row)
			: base(row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "The class below is inherited in multiple places, so this class cannot be static. Therefore adding the below suppress message related to static type.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public class Schema
		{
			public const int RecordType = 0;
		}

		#endregion

		#region Fields

		public RecordTypes RecordType
		{
			get
			{
				if (GetField(Schema.RecordType) == "POH")
				{
					return RecordTypes.POH;
				}
				if (GetField(Schema.RecordType) == "POL")
				{
					return RecordTypes.POL;
				}
				if (GetField(Schema.RecordType) == "PLD")
				{
					return RecordTypes.PLD;
				}

				return RecordTypes.INV;
			}
		}

		#endregion
	}
}
