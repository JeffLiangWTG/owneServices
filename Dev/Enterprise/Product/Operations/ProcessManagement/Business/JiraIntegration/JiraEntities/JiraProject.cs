using System.Collections.Generic;
using System.Diagnostics;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;

namespace Enterprise.ProcessManagement.Business
{
	[DebuggerDisplay("Jira Project - {Name}|{Code}: {Description}")]
	public class JiraProject : JiraEntity, IExternalEntityLinkable
	{
		internal JiraProject(JToken projectToken)
		{
			if (projectToken != null)
			{
				ID = ParseStringField(projectToken, JiraConstants.Project.ID);
				Code = ParseStringField(projectToken, JiraConstants.Project.Code);
				Description = ParseStringField(projectToken, JiraConstants.Project.Description);

				var lead = projectToken[JiraConstants.Project.Lead];

				if (IsJTokenUseable(lead))
				{
					ProjectLeadID = ParseStringField(lead, JiraConstants.User.ID);
				}

				Name = ParseStringField(projectToken, JiraConstants.Project.Name);

				var projectCategory = projectToken[JiraConstants.Project.ProjectCategory];

				if (IsJTokenUseable(projectCategory))
				{
					CategoryName = ParseStringField(projectCategory, JiraConstants.ProjectCategory.Name);
				}
			}
		}

		public string Name { get; }
		public string Code { get; }
		public string ID { get; }
		public string Description { get; }
		public string ProjectLeadID { get; }
		public string ProjectLeadEmail { get; set; }
		public string CategoryName { get; }

		public override bool Equals(object obj)
		{
			var otherObject = (JiraProject)obj;

			return otherObject != null
				&& Name == otherObject.Name
				&& Code == otherObject.Code
				&& ID == otherObject.ID
				&& Description == otherObject.Description
				&& ProjectLeadID == otherObject.ProjectLeadID
				&& ProjectLeadEmail == otherObject.ProjectLeadEmail
				&& CategoryName == otherObject.CategoryName;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = -1265253360;
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Code);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ID);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProjectLeadID);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProjectLeadEmail);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CategoryName);
				return hashCode;
			}
		}

		#region IExternalEntityLinkable Members

		string IExternalEntityLinkable.ParentTableCode => WorkProjectSchema.Constants.Prefix;

		#endregion
	}
}
