using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Organisation
{
	public static class OrganisationGuiState
	{
		public static void SaveInteger(Control control, ZString name, ZInt value)
		{
			SaveInteger(FullName(control, name), value);
		}

		public static void SaveInteger(ZString name, ZInt value)
		{
			var reg = OrganisationRegistry.Instance.GuiStateIntegers;
			reg.Name = name;
			reg.SetValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty, value);
			if (value == 0)
			{
				DeleteInteger(name);
			}
		}

		public static ZInt LoadInteger(Control control, ZString name)
		{
			return LoadInteger(FullName(control, name));
		}

		public static ZInt LoadInteger(ZString name)
		{
			var reg = OrganisationRegistry.Instance.GuiStateIntegers;
			reg.Name = name;
			return reg.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty);
		}

		public static void DeleteInteger(Control control, ZString name)
		{
			DeleteInteger(FullName(control, name));
		}

		public static void DeleteInteger(ZString name)
		{
			var reg = OrganisationRegistry.Instance.GuiStateIntegers;
			reg.Name = name;
			Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "delete from dbo.stmdata where SD_Name = '{0}'", reg.Name));
		}

		static ZString FullName(Control control, ZString name)
		{
			Argument.NotNullOrEmpty(name, "name");
			var fullname = new ZStringBuilder(name);

			for (Control current = control; current != null; current = current.Parent)
			{
				fullname.Prepend(current.Name + ".");
			}

			// The name is used as a registry item key, and the key of a registry item can't have more than 300 characters
			if (fullname.Length > 300)
			{
				return string.Concat("...", fullname.ToString().Substring(fullname.Length - 297));
			}
			else
			{
				return fullname.ToString();
			}
		}
	}
}
