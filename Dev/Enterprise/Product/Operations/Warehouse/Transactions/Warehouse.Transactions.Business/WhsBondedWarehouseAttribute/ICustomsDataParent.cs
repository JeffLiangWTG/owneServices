namespace Enterprise.Warehouse.Transactions.Business
{
	interface ICustomsDataParent
	{
		bool IsOutwardTypeRequired { get; }
		bool IsCustomsDataReadOnly { get; }
		bool IsCustomsOutwardDataReadOnly { get; }
		bool IsMainCustomsDataPropertiesReadOnly { get; }
	}
}
