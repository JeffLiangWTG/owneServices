using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class ImportableWrapper<T> : DocDataObject
	{
		public ImportableWrapper(T value)
		{
			ImportableValue = value;
		}

		public virtual T ImportableValue { get; set; }

		#region ShouldImportObject

		public ZBool ShouldImportObject
		{
			get => shouldImportObject;
			set
			{
				if (SetNonPersistentPropertyValue(ShouldImportObjectInfo, ref shouldImportObject, value))
				{
					Validate(ShouldImportObjectInfo);
				}
			}
		}
		ZBool shouldImportObject;

		public ZPropertyInfo ShouldImportObjectInfo => GetZPropertyInfo(nameof(ShouldImportObject));

		#endregion
	}
}
