using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public sealed class EIDOMessagingConfigurationAttribute : TestSetupAttribute
	{
		public EIDOMessagingConfigurationAttribute()
		{
			Enabled = true;
			Testing = true;
			Principals = new string[] { "Principal" };
			Email = "bob@freadnet.org";
			Password = "password";
			SenderId = "SenderID";
			RecipientId = "RecipientId";
			AcknowledgementEmail = "ack@freadnet.org";
			ErrorEmail = "err@freadnet.org";
		}

		public override void SetUp(TestCase testCase)
		{
			EIDOMessagingHeader detail = new EIDOMessagingHeader();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			if (Enabled)
			{
				detail.Testing = Testing;
				detail.Email = Email;
				int i = 1;
				string suffix = "";
				foreach (string principal in Principals)
				{
					EIDOMessagingIdentity identity = detail.Identities.AddNew();
					identity.PrincipalPK = CreatePrincipal(factory, principal);
					identity.Password = Password + suffix;
					identity.SenderID = SenderId + suffix;
					identity.RecipientID = RecipientId + suffix;
					suffix = (i++).ToString();
				}
			}
			else
			{
				detail.Testing = Testing;
				detail.Email = "";
			}

			SetupEmail(factory, AgencyRegistry.Instance.EIDOAcknowledgementEmailGroup, "acks", "ackg", AcknowledgementEmail);
			SetupEmail(factory, AgencyRegistry.Instance.EIDOErrorEmailGroup, "errs", "errg", ErrorEmail);
			AgencyRegistry.Instance.EIDOMessagingDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, detail);
		}

		public override void TearDown(TestCase testCase)
		{
		}

		public bool Enabled { get; set; }

		public bool Testing { get; set; }

		public string[] Principals { get; set; }

		public string Password { get; set; }

		public string Email { get; set; }

		public string SenderId { get; set; }

		public string RecipientId { get; set; }

		public string AcknowledgementEmail { get; set; }

		public string ErrorEmail { get; set; }

		static ZGuid CreatePrincipal(BusinessObjectFactory factory, string principalCode)
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, principalCode);
			OrgHeader principal = factory.LoadTop1<OrgHeader>(filter);
			if (principal == null)
			{
				principal = factory.NewWithValidTestData<OrgHeader>();
				principal.OH_Code = principalCode;
				principal.OH_IsShippingProvider = true;
				principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
				factory.Save();
			}

			return principal.PK;
		}

		static void SetupEmail(BusinessObjectFactory factory, GuidRegistryItem registry, string staffName, string groupName, string emailAddress)
		{
			if (string.IsNullOrEmpty(emailAddress))
			{
				registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			else
			{
				GlbStaff staff = factory.New<GlbStaff>();
				staff.GS_Code = Left(staffName, staff.GS_CodeInfo.MaxLength);
				staff.GS_FullName = staffName;
				staff.GS_LoginName = staffName;
				staff.GS_EmailAddress = emailAddress;
				GlbGroup group = factory.New<GlbGroup>();
				group.GG_Code = groupName;
				group.GG_Desc = "Group-" + groupName;
				group.Staff.Add(staff);
				factory.Save();
				registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			}
		}

		static string Left(string value, int maxLength)
		{
			if (value == null)
			{
				return null;
			}
			else if (value.Length > maxLength)
			{
				return value.Substring(0, maxLength);
			}
			else
			{
				return value;
			}
		}
	}
}
