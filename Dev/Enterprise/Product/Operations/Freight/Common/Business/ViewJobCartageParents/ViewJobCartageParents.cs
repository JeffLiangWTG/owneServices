using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Integration;

namespace Enterprise.Freight.Common.Business
{
	public class ViewJobCartageParents : AutoViewJobCartageParents, IViewJobCartageParents
	{
		public ViewJobCartageParents(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete => false;

		public override void Delete()
		{
			throw new NotSupportedException("Deletion of the ViewJobCartageParents is not supported.");
		}
	}
}
