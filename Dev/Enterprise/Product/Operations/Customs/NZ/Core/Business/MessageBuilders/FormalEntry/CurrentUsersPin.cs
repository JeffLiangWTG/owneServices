using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry
{
	public class CurrentUsersPin
	{
		public CurrentUsersPin(BusinessObjectFactory factory, bool isInTestMode)
		{
			this.factory = factory;
			this.isInTestMode = isInTestMode;
		}

		public CurrentUsersPin(GlbStaff staff, bool isInTestMode)
			: this(staff.Factory, isInTestMode)
		{
			fStaff = staff;
		}
		readonly BusinessObjectFactory factory;
		readonly bool isInTestMode;

		#region Staff
		GlbStaff Staff
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = factory.Load<GlbStaff>(Env.CurrentUser.PK);
				}
				return fStaff;
			}
		}
		GlbStaff fStaff;
		#endregion

		Enterprise.MasterFiles.Integration.Customs.NZ.INZGlbStaffWrapper StaffWrapper => staffWrapper ?? (staffWrapper = Staff.GetNZWrapper());
		Enterprise.MasterFiles.Integration.Customs.NZ.INZGlbStaffWrapper staffWrapper;

		#region BrokerID
		public ZString BrokerID
		{
			get { return IsTestWithNoBrokerSetUp ? new ZString(TestSystemBrokerID) : StaffWrapper.NZBPassword.GP_UserID; }
		}
		#endregion

		#region DecryptedPinCode
		public ZString DecryptedPinCode
		{
			get { return IsTestWithNoBrokerSetUp ? (ZString)TestSystemPinCode : StaffWrapper.NZBPassword.CurrentDecryptedPassword.ToUpper(); }
			set { StaffWrapper.NZBPassword.CurrentDecryptedPassword = value; }
		}
		#endregion

		public const string TestSystemBrokerID = "65432198B";
		public const string TestSystemPinCode = "MYTEST";

		bool IsTestWithNoBrokerSetUp
		{
			get { return isInTestMode && StaffWrapper.NZBPassword.GP_UserID.IsEmpty; }
		}
	}
}
