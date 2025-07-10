using System;
using CargoWise.Common;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class UpdateAttributesEventArgs : EventArgs
	{
		public UpdateAttributesEventArgs(AttributeParts oldAttributes, PartAttributeNumber partAttribChanged)
		{
			OldAttributes = Argument.NotNull(oldAttributes, "oldAttributes");
			OldKey = OldAttributes.Key;
			PartAttribChanged = partAttribChanged;
		}

		public readonly AttributeParts OldAttributes;
		public readonly PartAttributeNumber PartAttribChanged;
		public readonly string OldKey;
	}
}
