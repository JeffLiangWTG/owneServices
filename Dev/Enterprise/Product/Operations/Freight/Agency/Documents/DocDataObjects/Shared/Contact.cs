using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class Contact : DocDataObject, IContact
	{
		#region FullName

		public ZString FullName
		{
			get => fullName;
			set
			{
				if (SetNonPersistentPropertyValue(FullNameInfo, ref fullName, value))
				{
					Validate(FullNameInfo);
				}
			}
		}

		ZString fullName;

		public ZPropertyInfo FullNameInfo => GetZPropertyInfo(nameof(FullName));

		#endregion

		#region Phone

		public ZString Phone
		{
			get => phone;
			set
			{
				if (SetNonPersistentPropertyValue(PhoneInfo, ref phone, value))
				{
					Validate(PhoneInfo);
				}
			}
		}

		ZString phone;

		public ZPropertyInfo PhoneInfo => GetZPropertyInfo(nameof(Phone));

		#endregion

		#region Email

		public ZString Email
		{
			get => email;
			set
			{
				if (SetNonPersistentPropertyValue(EmailInfo, ref email, value))
				{
					Validate(EmailInfo);
				}
			}
		}

		ZString email;

		public ZPropertyInfo EmailInfo => GetZPropertyInfo(nameof(Email));

		#endregion
	}
}
