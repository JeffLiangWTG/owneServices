using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public sealed class RegistrySetup : IDisposable
	{
		public RegistrySetup(TargetInRegistry setupOptions)
		{
			switch (setupOptions)
			{
				case TargetInRegistry.All:
					{
						AddSecurityRight(Env.Security.AllowMessageErrors.Code);
						AddSecurityRight(Env.Security.MergeByDefault.Code);
						AddSecurityRight(Env.Security.USFTAReconDefault.Code);
						AddSecurityRight(Env.Security.USPAYERAccountNumberDefault.Code);
						AddSecurityRight(Env.Security.USPaymentTypeDefault.Code);
						AddSecurityRight(Env.Security.USReconIssueDefault.Code);
						AddSecurityRight(Env.Security.USAllRequestedIndicator.Code);
					}
					break;

				case TargetInRegistry.AllowMessageErrors:
					AddSecurityRight(Env.Security.AllowMessageErrors.Code);
					break;

				case TargetInRegistry.MergeBy:
					AddSecurityRight(Env.Security.MergeByDefault.Code);
					break;

				case TargetInRegistry.FTARecon:
					AddSecurityRight(Env.Security.AllowMessageErrors.Code);
					AddSecurityRight(Env.Security.USFTAReconDefault.Code);
					break;

				case TargetInRegistry.PayerAccountNumber:
					AddSecurityRight(Env.Security.AllowMessageErrors.Code);
					AddSecurityRight(Env.Security.USPAYERAccountNumberDefault.Code);
					break;

				case TargetInRegistry.PaymentType:
					AddSecurityRight(Env.Security.AllowMessageErrors.Code);
					AddSecurityRight(Env.Security.USPaymentTypeDefault.Code);
					break;

				case TargetInRegistry.ReconIssue:
					AddSecurityRight(Env.Security.AllowMessageErrors.Code);
					AddSecurityRight(Env.Security.USReconIssueDefault.Code);
					break;
				case TargetInRegistry.AIIRequestedIndicator:
					AddSecurityRight(Env.Security.AllowMessageErrors.Code);
					AddSecurityRight(Env.Security.USAllRequestedIndicator.Code);
					break;

				default:
					break;
			}
		}

		void AddSecurityRight(ZString context)
		{
			var se = Factory.New<GlbSecurity>();
			se.GU_SecurityRight = context;
			se.GU_SecurityItemIsAllowed = true;
			se.GU_GS = User.PK;
			se.GU_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#region Group

		public GlbGroup Group
		{
			get
			{
				if (group == null)
				{
					group = Factory.New<GlbGroup>();
					group.GG_Code = "ABC";
					Factory.Save();
				}
				return group;
			}
		}
		GlbGroup group;

		#endregion

		#region User

		public GlbStaff User
		{
			get
			{
				if (user == null)
				{
					user = Factory.New<GlbStaff>();
					user.FillWithValidTestData();
					user.GS_Code = "AMA";
					user.GS_LoginName = "anton";
					user.StaffPlainTextPassword = "password";
					user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;

					Group.Staff.Add(user);
					Factory.Save();
				}

				return user;
			}
		}
		GlbStaff user;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (group != null)
			{
				group.Delete();
			}

			if (user != null)
			{
				user.Delete();
			}

			Factory.Save();
		}

		#endregion
	}
}
