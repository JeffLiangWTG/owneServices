using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.TW;

namespace Enterprise.Customs.TW.Business
{
	public class TWGlbStaffWrapper : GlbStaffWrapper, ITWGlbStaffWrapper
	{
		protected TWGlbStaffWrapper(GlbStaff staff)
			: base(staff)
		{
		}

		public static TWGlbStaffWrapper Get(GlbStaff staff)
		{
			return staff?.Factory.GetCachedValue(staff.PK.ToString(), () => new TWGlbStaffWrapper(staff));
		}

		#region TWPasswordCollection

		[ChildEditable]
		public GlbExternalPasswordCollection TWPasswordCollection
		{
			get
			{
				if (twGlbExternalPasswordCollection == null)
				{
					twGlbExternalPasswordCollection = new GlbExternalPasswordCollection(Staff);
					twGlbExternalPasswordCollection.Load();
					RegisterEditableChildObject(twGlbExternalPasswordCollection);
				}

				return twGlbExternalPasswordCollection;
			}
		}

		GlbExternalPasswordCollection twGlbExternalPasswordCollection;

		#endregion

		IGlbExternalPasswordCollection_TW ITWGlbStaffWrapper.TWPasswordCollection => TWPasswordCollection;
	}
}
