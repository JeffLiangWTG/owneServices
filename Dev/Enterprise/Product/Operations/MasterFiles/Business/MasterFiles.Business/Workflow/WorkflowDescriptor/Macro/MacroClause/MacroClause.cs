using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	internal abstract class MacroClause
	{
		public MacroClause()
		{
		}

		public virtual void Validate(string expression, INotifications notifications, WorkflowMacroValidation validation)
		{
		}

		public abstract string Keyword { get; }

		public bool IsStartWithClause(ZString propertyPath)
		{
			return propertyPath.StartsWith(GetKeywordWithOpenBracket(), StringComparison.OrdinalIgnoreCase);
		}

		public bool IsContainClause(ZString propertyPath)
		{
			return propertyPath.Contains(GetKeywordWithOpenBracket(), StringComparison.OrdinalIgnoreCase);
		}

		internal string GetExpression(ZString propertyPath, out string nextPropertyPath)
		{
			nextPropertyPath = propertyPath;
			if (!propertyPath.StartsWith(GetKeywordWithOpenBracket(), StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			var startIndex = propertyPath.IndexOf("(", 0, StringComparison.OrdinalIgnoreCase);
			var endIndex = propertyPath.IndexOf("\").", startIndex, StringComparison.OrdinalIgnoreCase) + 1;

			if (endIndex < startIndex)
			{
				endIndex = propertyPath.IndexOf(").", startIndex, StringComparison.OrdinalIgnoreCase);
			}

			if (endIndex < startIndex)
			{
				startIndex = propertyPath.IndexOf("(", 0, StringComparison.OrdinalIgnoreCase);
				endIndex = propertyPath.LastIndexOf("\")", StringComparison.OrdinalIgnoreCase) + 1;

				if (endIndex < startIndex)
				{
					endIndex = propertyPath.LastIndexOf(")", StringComparison.OrdinalIgnoreCase);
				}

				nextPropertyPath = string.Empty;
			}
			else
			{
				nextPropertyPath = propertyPath.Substring(endIndex + 2); // +1 for ( and +1 for .
			}

			if (endIndex < startIndex)
			{
				nextPropertyPath = string.Empty;
				return string.Empty;
			}

			return propertyPath.SubstringSafe(startIndex + 1, endIndex - startIndex - 1);
		}

		public virtual bool ShouldReturnDefaultValueIfNull()
		{
			return false;
		}

		public void NotifyWarning(INotifications notifications, ZString message, ZString detailMessage)
		{
			if (notifications != null)
			{
				if (GlbStaff.CurrentUser.GS_IsSystemAccount && !detailMessage.IsEmpty)
				{
					message += string.Format(CultureInfo.CurrentCulture, "\r\n{0}", detailMessage);
				}
				notifications.AddWarning(message);
			}
		}

		public void NotifyError(INotifications notifications, ZString message, ZString detailMessage)
		{
			if (notifications != null)
			{
				if (GlbStaff.CurrentUser.GS_IsSystemAccount && !detailMessage.IsEmpty)
				{
					message += string.Format(CultureInfo.CurrentCulture, "\r\n{0}", detailMessage);
				}
				notifications.AddError(message);
			}
		}

		#region Implementation

		string GetKeywordWithOpenBracket()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0}(", Keyword);
		}

		#endregion
	}
}
