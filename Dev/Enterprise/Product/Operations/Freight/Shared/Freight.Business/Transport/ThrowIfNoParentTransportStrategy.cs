using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	internal sealed class ThrowIfNoParentTransportStrategy : IValueSetStrategy
	{
		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			if (valueThatHasChanged == null)
			{
				throw new ArgumentNullException(nameof(valueThatHasChanged));
			}

			Transport parent = (Transport)valueThatHasChanged.BizObj;

			if (parent.ParentType == null && parent.JW_ParentType.IsEmpty && (!parent.IsInDatabase || parent.HasChanges))
			{
				parent.ReportParentNullIssue();
				throw new InvalidOperationException("The parent has not been set yet.");
			}
		}
	}
}
