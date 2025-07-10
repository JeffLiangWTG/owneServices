using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public record struct GeneratedID(ZString FormattedID, int IDNumber)
	{
		public bool HasID => !string.IsNullOrEmpty(FormattedID);

		public static implicit operator ZString(GeneratedID generatedID) => generatedID.FormattedID;
	}
}
