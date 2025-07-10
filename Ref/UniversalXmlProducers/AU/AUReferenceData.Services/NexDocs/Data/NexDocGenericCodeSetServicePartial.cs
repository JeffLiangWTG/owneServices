namespace CargoWise.RefDbRepo.AUReferenceData.Services.NexDocGenericCodeSetService
{
	sealed partial class ListCodeSet : IListCodeSet
	{
		int IListCodeSet.Position => listPosition;

		IItemCodeSet[] IListCodeSet.Items => listItemCodeSet;
	}

	sealed partial class ItemCodeSet : IItemCodeSet
	{
		string IItemCodeSet.Key => key.ToUpperInvariant();

		string IItemCodeSet.Value => value;

		CodeSetValueType IItemCodeSet.ValueType
		{
			get
			{
				switch (type)
				{
					case CodeValueType.boolean:
						return CodeSetValueType.boolean;
					case CodeValueType.dateTime:
						return CodeSetValueType.dateTime;
					case CodeValueType.@decimal:
						return CodeSetValueType.@decimal;
					case CodeValueType.@double:
						return CodeSetValueType.@double;
					case CodeValueType.@float:
						return CodeSetValueType.@float;
					case CodeValueType.integer:
						return CodeSetValueType.integer;
					default:
						return CodeSetValueType.@string;
				}
			}
		}
	}
}
