using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Enterprise.ProcessManagement.Business
{
	[DebuggerDisplay("Jira User {DisplayName}")]
	public class JiraUser : JiraEntity
	{
		internal JiraUser(JToken userToken)
		{
			if (userToken != null)
			{
				AccountID = ParseStringField(userToken, JiraConstants.User.ID);
				Key = ParseStringField(userToken, JiraConstants.User.Key);
				Name = ParseStringField(userToken, JiraConstants.User.Name);
				Email = ParseStringField(userToken, JiraConstants.User.Email);
				DisplayName = ParseStringField(userToken, JiraConstants.User.DisplayName);
				IsActive = ParseBooleanField(userToken, JiraConstants.User.IsActive);
			}
		}

		public string AccountID { get; }
		public string Key { get; }
		public string Name { get; }
		public string Email { get; set; }
		public string DisplayName { get; }
		public bool IsActive { get; }

		public override bool Equals(object obj)
		{
			var otherObject = (JiraUser)obj;

			return otherObject != null
				&& AccountID == otherObject.AccountID
				&& Key == otherObject.Key
				&& Name == otherObject.Name
				&& Email == otherObject.Email
				&& DisplayName == otherObject.DisplayName
				&& IsActive == otherObject.IsActive;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 548396439;
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AccountID);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Email);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisplayName);
				hashCode = hashCode * -1521134295 + IsActive.GetHashCode();
				return hashCode;
			}
		}
	}
}
