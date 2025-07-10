using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class User : IUser
	{
		public User(GlbStaff staff)
		{
			this.staff = staff;
		}

		readonly GlbStaff staff;

		public ZString Code => staff?.GS_Code ?? ZString.Empty;

		public ZString Name => staff?.GS_FullName ?? ZString.Empty;

		public ZString Phone => staff?.GS_WorkPhone ?? ZString.Empty;

		public ZString Email => staff?.GS_EmailAddress ?? ZString.Empty;

		public ZString Fax => staff?.GS_FaxNum ?? ZString.Empty;

		public ZBool IsDeveloper => staff?.GS_IsDeveloper ?? false;

		public object Signature => staff?.SignatureImage;

		public IReadOnlyCollection<ICertificate> Certificates => certificates ?? (certificates = staff?.Certificates?.Select(c => new Certificate(c)).ToArray() ?? System.Array.Empty<ICertificate>());
		public IReadOnlyCollection<ICertificate> certificates;

		public IReadOnlyCollection<IUserGroup> Groups => groups ?? (groups = staff?.Groups.Select(g => new UserGroup(g)).ToArray() ?? System.Array.Empty<IUserGroup>());
		IReadOnlyCollection<IUserGroup> groups;
	}
}
