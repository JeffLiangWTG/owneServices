using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AccountingHtmlEmailDefTest : TestCaseWithFactory
	{
		protected abstract Type EmailDefType { get; }

		protected string GetBody(AccountingHtmlEmailDef email)
		{
			MethodInfo info = EmailDefType.GetMethod("GetBody", BindingFlags.NonPublic | BindingFlags.Instance);
			return info.Invoke(email, null) as string;
		}

		protected string GetSubject(AccountingHtmlEmailDef email)
		{
			MethodInfo info = EmailDefType.GetMethod("GetSubject", BindingFlags.NonPublic | BindingFlags.Instance);
			return info.Invoke(email, null) as string;
		}

		public void TestDoesNotCacheBusinessObjects()
		{
			var fields = EmailDefType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
			if (fields.Length == 0)
			{
				Assert(true);
			}
			else
			{
				foreach (var field in fields)
				{
					Assert(string.Format("Field '{0}' can not be a busines object", field.Name), !typeof(IBusiness).IsAssignableFrom(field.FieldType));
				}
			}
		}
	}
}
