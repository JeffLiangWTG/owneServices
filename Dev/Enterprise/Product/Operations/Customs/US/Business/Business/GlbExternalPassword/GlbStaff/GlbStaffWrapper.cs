using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class GlbStaffWrapper : MasterFiles.Business.GlbStaffWrapper, IUSGlbStaffWrapper
	{
		protected GlbStaffWrapper(GlbStaff staff)
			: base(staff)
		{
		}

		public static GlbStaffWrapper Get(GlbStaff staff)
		{
			return staff?.Factory.GetCachedValue(staff.PK.ToString(), () => new GlbStaffWrapper(staff));
		}

		#region PasswordCollection

		[ChildEditable]
		public GlbStaffCredentialCollection PasswordCollection
		{
			get
			{
				if (passwordCollection == null)
				{
					passwordCollection = new GlbStaffCredentialCollection(Staff);
					passwordCollection.Load();
					RegisterEditableChildObject(passwordCollection);
				}

				return passwordCollection;
			}
		}

		GlbStaffCredentialCollection passwordCollection;

		#endregion

		IGlbExternalPasswordCollection_US IUSGlbStaffWrapper.PasswordCollection => PasswordCollection;
	}
}
