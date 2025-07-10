using System;
using CargoWise.Common;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class PickLinesToPickedPackTypeInfo : DataObjectInfo
	{
		public PickLinesToPickedPackTypeInfo()
			: this(Array.Empty<Guid>(), Array.Empty<PickedPackTypeInfo>())
		{
		}

		public PickLinesToPickedPackTypeInfo(Guid[] pickLinePKs, PickedPackTypeInfo[] pickedPackTypes)
		{
			PickLinePKs = Argument.NotNull(pickLinePKs, nameof(pickLinePKs));
			PickedPackTypes = Argument.NotNull(pickedPackTypes, nameof(pickedPackTypes));
		}

		public Guid[] PickLinePKs { get; set; }
		public PickedPackTypeInfo[] PickedPackTypes { get; set; }
	}
}
