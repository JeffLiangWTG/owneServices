using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterData.GUI
{
	public class PersonFilterDataSource : NonPersistentBusinessObject
	{
		#region Filter Active

		public ZString PersonFilterActive
		{
			get
			{
				return personFilterActive;
			}
			set
			{
				if (personFilterActive != value)
				{
					SetNonPersistentPropertyValue(PersonFilterActiveInfo, ref personFilterActive, value);
				}
			}
		}
		ZString personFilterActive = PersonFilterActivesList.Descriptions.All.GetUnresolvedString();

		public ZPropertyInfo PersonFilterActiveInfo => GetZPropertyInfo(nameof(PersonFilterActive));

		[List(nameof(PersonFilterActives))]
		public ZString PersonFilterActiveDescription
		{
			get
			{
				return PersonFilterActives.GetDescriptionFromCode(PersonFilterActive);
			}
			set
			{
				var code = PersonFilterActives.GetCodeFromDescription(value);
				if (!string.IsNullOrEmpty(code))
				{
					PersonFilterActive = code;
				}

				PersonFilterActiveInfo.RefreshBinding();
			}
		}

		public PersonFilterActivesList PersonFilterActives => personFilterActives ?? (personFilterActives = new PersonFilterActivesList());
		PersonFilterActivesList personFilterActives;

		#endregion

		#region Filter Email

		public ZString PersonFilterEmail
		{
			get
			{
				return personFilterEmail;
			}
			set
			{
				if (personFilterEmail != value)
				{
					SetNonPersistentPropertyValue(PersonFilterEmailInfo, ref personFilterEmail, value);
				}
			}
		}
		ZString personFilterEmail = PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString();

		public ZPropertyInfo PersonFilterEmailInfo => GetZPropertyInfo(nameof(PersonFilterEmail));

		[List(nameof(PersonFilterOptions))]
		public ZString PersonFilterEmailDescription
		{
			get
			{
				return PersonFilterOptions.GetDescriptionFromCode(PersonFilterEmail);
			}
			set
			{
				var code = PersonFilterOptions.GetCodeFromDescription(value);
				if (!string.IsNullOrEmpty(code))
				{
					PersonFilterEmail = code;
				}

				PersonFilterEmailInfo.RefreshBinding();
			}
		}

		public ZString PersonFilterEmailKeyword
		{
			get => personFilterEmailKeyword;
			set
			{
				if (personFilterEmailKeyword != value)
				{
					SetNonPersistentPropertyValue(PersonFilterEmailKeywordInfo, ref personFilterEmailKeyword, value);
				}
			}
		}
		ZString personFilterEmailKeyword;
		public ZPropertyInfo PersonFilterEmailKeywordInfo => GetZPropertyInfo(nameof(PersonFilterEmailKeyword));

		#endregion

		#region Filter Phone

		public ZString PersonFilterPhone
		{
			get
			{
				return personFilterPhone;
			}
			set
			{
				if (personFilterPhone != value)
				{
					SetNonPersistentPropertyValue(PersonFilterPhoneInfo, ref personFilterPhone, value);
				}
			}
		}
		ZString personFilterPhone = PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString();

		public ZPropertyInfo PersonFilterPhoneInfo => GetZPropertyInfo(nameof(PersonFilterPhone));

		[List(nameof(PersonFilterOptions))]
		public ZString PersonFilterPhoneDescription
		{
			get
			{
				return PersonFilterOptions.GetDescriptionFromCode(PersonFilterPhone);
			}
			set
			{
				var code = PersonFilterOptions.GetCodeFromDescription(value);
				if (!string.IsNullOrEmpty(code))
				{
					PersonFilterPhone = code;
				}

				PersonFilterPhoneInfo.RefreshBinding();
			}
		}

		public ZString PersonFilterPhoneKeyword
		{
			get => personFilterPhoneKeyword;
			set
			{
				if (personFilterPhoneKeyword != value)
				{
					SetNonPersistentPropertyValue(PersonFilterPhoneKeywordInfo, ref personFilterPhoneKeyword, value);
				}
			}
		}
		ZString personFilterPhoneKeyword;

		public ZPropertyInfo PersonFilterPhoneKeywordInfo => GetZPropertyInfo(nameof(PersonFilterPhoneKeyword));

		#endregion

		#region Filter Name

		public ZString PersonFilterName
		{
			get
			{
				return personFilterName;
			}
			set
			{
				if (personFilterName != value)
				{
					SetNonPersistentPropertyValue(PersonFilterNameInfo, ref personFilterName, value);
				}
			}
		}
		ZString personFilterName = PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString();

		public ZPropertyInfo PersonFilterNameInfo => GetZPropertyInfo(nameof(PersonFilterName));

		[List(nameof(PersonFilterOptions))]
		public ZString PersonFilterNameDescription
		{
			get
			{
				return PersonFilterOptions.GetDescriptionFromCode(PersonFilterName);
			}
			set
			{
				var code = PersonFilterOptions.GetCodeFromDescription(value);
				if (!string.IsNullOrEmpty(code))
				{
					PersonFilterName = code;
				}

				PersonFilterNameInfo.RefreshBinding();
			}
		}

		public ZString PersonFilterNameKeyword
		{
			get => personFilterNameKeyword;
			set
			{
				if (personFilterNameKeyword != value)
				{
					SetNonPersistentPropertyValue(PersonFilterNameKeywordInfo, ref personFilterNameKeyword, value);
				}
			}
		}
		ZString personFilterNameKeyword;

		public ZPropertyInfo PersonFilterNameKeywordInfo => GetZPropertyInfo(nameof(PersonFilterNameKeyword));

		#endregion

		public PersonFilterOptionsList PersonFilterOptions => personFilterOptions ?? (personFilterOptions = new PersonFilterOptionsList());
		PersonFilterOptionsList personFilterOptions;
	}
}
