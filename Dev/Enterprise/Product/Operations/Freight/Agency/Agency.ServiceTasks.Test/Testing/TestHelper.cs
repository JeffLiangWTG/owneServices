using System;
using System.IO;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	static internal class TestHelper
	{
		public static HostedServiceAttribute GetAttributeRegistring<ServiceT>()
			where ServiceT : ServiceProviderImpl
		{
			Type serviceType = typeof(ServiceT);
			Assembly serviceAssembly = serviceType.Assembly;
			string serviceAssemblyName = Path.GetFileNameWithoutExtension(serviceAssembly.Location);
			HostedServiceAttribute result = null;
			foreach (HostedServiceAttribute att in Attribute.GetCustomAttributes(serviceAssembly, typeof(HostedServiceAttribute)))
			{
				if (att.TypeAssemblyName == serviceAssemblyName && att.TypeName == serviceType.FullName)
				{
					if (result == null)
					{
						result = att;
					}
					else
					{
						throw new InvalidOperationException(serviceType.Name + " is referenced by more than one attribute");
					}
				}
			}

			return result;
		}

		public static void SetCMMEmailAddresses(BusinessObjectFactory factory, string acknowledgementEmail, string discrepanciesEmail, string errorEmail)
		{
			GlbGroup acknowledgementGroup = acknowledgementEmail == null ? null : NewGroupWithStaffMemberAndEmailAddress(factory, "acks", "ackg", acknowledgementEmail);
			GlbGroup discrepanciesGroup = discrepanciesEmail == null ? null : NewGroupWithStaffMemberAndEmailAddress(factory, "dscs", "dscg", discrepanciesEmail);
			GlbGroup errorGroup = errorEmail == null ? null : NewGroupWithStaffMemberAndEmailAddress(factory, "errs", "errg", errorEmail);
			factory.Save();
			AgencyRegistry.Instance.CMMAcknowledgementEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acknowledgementGroup == null ? Guid.Empty : acknowledgementGroup.PK.ToGuid());
			AgencyRegistry.Instance.CMMDiscrepanciesEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, discrepanciesGroup == null ? Guid.Empty : discrepanciesGroup.PK.ToGuid());
			AgencyRegistry.Instance.CMMErrorEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, acknowledgementGroup == null ? Guid.Empty : errorGroup.PK.ToGuid());
			Env.Registry.SMTPDefaultReturnEmailAddress = "default@wisetechglobal.com";
		}

		#region Implementation
		static GlbGroup NewGroupWithStaffMemberAndEmailAddress(BusinessObjectFactory factory, ZString staffName, ZString groupName, ZString emailAddress)
		{
			GlbStaff staff = factory.New<GlbStaff>();
			staff.GS_Code = new ZString(staffName).Left(staff.GS_CodeInfo.MaxLength);
			staff.GS_FullName = staffName;
			staff.GS_LoginName = staffName;
			staff.GS_EmailAddress = emailAddress;
			GlbGroup group = factory.New<GlbGroup>();
			group.GG_Code = groupName;
			group.GG_Desc = "Group-" + groupName;
			group.Staff.Add(staff);
			return group;
		}
		#endregion
	}
}
