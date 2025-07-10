using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgUserFlagCollection : NonPersistentBusinessObjectCollection<OrgUserFlag>
	{
		public OrgUserFlagCollection(OrgHeader org)
			: base(org.Factory)
		{
			foreach (var userFlagType in OrgUserFlagType.All)
			{
				Add(new OrgUserFlag(org, userFlagType));
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException("Not Supported");
		}
	}
}
