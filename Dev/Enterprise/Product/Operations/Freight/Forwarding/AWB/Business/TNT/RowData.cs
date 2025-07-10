namespace Enterprise.Freight.Forwarding.AWB.TNT
{
	public abstract class RowData
	{
		internal void ReadRow(string rowData)
		{
			new FieldPopulator(this).PopulateFields(rowData);
		}
	}
}
