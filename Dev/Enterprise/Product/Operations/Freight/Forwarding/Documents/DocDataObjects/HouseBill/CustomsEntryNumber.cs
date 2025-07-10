using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class CustomsEntryNumber : DocDataObject
	{
		public ICodeDescription Type { get; set; }
		public ZString Value
		{
			get => _value;
			set
			{
				if (SetNonPersistentPropertyValue(ValueInfo, ref _value, value))
				{
				}
			}
		}

		ZString _value;

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));
	}
}
