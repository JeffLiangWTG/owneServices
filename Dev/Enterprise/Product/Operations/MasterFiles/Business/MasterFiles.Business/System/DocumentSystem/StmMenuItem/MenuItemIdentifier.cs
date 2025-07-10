using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public struct MenuItemIdentifier : IEquatable<MenuItemIdentifier>
	{
		BusinessContext businessContext;
		string name;

		public MenuItemIdentifier(BusinessContext businessContext, string name)
		{
			this.businessContext = businessContext;
			this.name = name;
		}

		public BusinessContext BusinessContext
		{
			get { return businessContext; }
			set { businessContext = value; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public override bool Equals(object obj)
		{
			return (obj is MenuItemIdentifier) && Equals((MenuItemIdentifier)obj);
		}

		public bool Equals(MenuItemIdentifier other)
		{
			return (other.BusinessContext == BusinessContext) && (other.Name == Name);
		}

		public override int GetHashCode()
		{
			int result = BusinessContext.GetHashCode();
			if (Name != null)
			{
				result = result ^ Name.GetHashCode();
			}
			return result;
		}

		public DocumentZQuery GetQuery()
		{
			DocumentZQuery result = new DocumentZQuery(BusinessContext, Name);
			result.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, ZBool.True);
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer-directed string")]
		public T LoadMenuItem<T>(BusinessObjectFactory factory) where T : StmMenuItem
		{
			T result = null;

			T[] documents = factory.Load<T>(GetQuery());
			if (documents.Length == 1)
			{
				result = documents[0];
			}
			else
			{
				string caption;
				string message;
				if (documents.Length == 0)
				{
					caption = (NoResString)"No Menu Item Found";
					message = (NoResString)"No menu item was found with the business context \"{0}\" and the name \"{1}\".";
				}
				else
				{
					caption = "Multiple Menu Items Found";
					message = "More than one menu item was found with the business context \"{0}\" and the name \"{1}\".";
				}
				Globals.Message.ShowDeveloperErrorOnce("MenuItemIdentifierError", string.Format(message, BusinessContext, Name), caption);
			}

			return result;
		}
	}
}
