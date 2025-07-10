
using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLDescriptorPivotCollection : ActiveBusinessObjectCollection<AccGLDescriptorPivot>
	{
		public AccGLDescriptorPivotCollection(BusinessObjectFactory factory, AccGLAccountDescriptor parent)
			: base(factory, parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent), "The Descriptor Pivot Collection requires a non-null parent Descriptor.");
			}
		}

		public void MapToGLHeader(ZGuid gLHeaderPK)
		{
			if (gLHeaderPK.IsEmpty)
			{
				DeleteAll();
			}
			else
			{
				AccGLDescriptorPivot pivot = GetAccGLDescriptorPivot() ?? AddNew();
				pivot.YJ_AG = gLHeaderPK;
			}
		}

		public AccGLDescriptorPivot GetAccGLDescriptorPivot()
		{
			RefreshAll(Factory);
			return Count > 0 ? this[0] : null;
		}
	}
}
