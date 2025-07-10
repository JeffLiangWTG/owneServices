using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class GlbGroupExtensions
	{
		public static bool IsValidForOutgoingMail(this IGlbGroup group, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			if (group != null && group.Staff != null)
			{
				return group.Staff.Cast<GlbStaff>().FirstOrDefault(staff => EmailAddressValidation.IsEmailAddressValidAndNotEmpty(staff.GS_EmailAddress)) != null;
			}
			return false;
		}
	}
}
