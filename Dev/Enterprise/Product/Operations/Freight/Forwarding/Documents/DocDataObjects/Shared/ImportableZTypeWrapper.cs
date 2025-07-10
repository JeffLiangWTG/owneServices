using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class ImportableZTypeWrapper<T> : ImportableWrapper<T> where T : IZType
	{
		public ImportableZTypeWrapper(T value) : base(value)
		{
		}

		public override T ImportableValue
		{
			get => importableValue;
			set
			{
				if (SetNonPersistentPropertyValue(ImportableValueInfo, ref importableValue, value))
				{
					Validate(ImportableValueInfo);
				}
			}
		}
		T importableValue;

		public ZPropertyInfo ImportableValueInfo => GetZPropertyInfo(nameof(ImportableValue));
	}
}
