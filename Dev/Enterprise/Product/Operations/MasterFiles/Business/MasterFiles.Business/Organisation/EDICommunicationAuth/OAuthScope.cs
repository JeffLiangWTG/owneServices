using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public sealed class OAuth2Scope : NonPersistentBusinessObject, IOAuth2Scope, IObsoleteValidation
	{
		#region ScopeName

		public ZString ScopeName
		{
			get { return scopeName; }

			set
			{
				if (scopeName != value)
				{
					SetNonPersistentPropertyValue(ScopeNameInfo, ref scopeName, value);

					if (!IsValidationSuspended)
					{
						ValidateScopeName();
					}
				}
			}
		}

		ZString scopeName;

		public ZPropertyInfo ScopeNameInfo
		{
			get { return GetZPropertyInfo(nameof(ScopeName)); }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateScopeName();
		}

		public void ValidateScopeName()
		{
			ScopeNameInfo.ClearAllNotifications();
			if (scopeName.ToString().Any(char.IsWhiteSpace))
			{
				ScopeNameInfo.AddError(Res.GetString("a966be56-78b7-41a1-a7bc-4697a92d35b3", "Scope Value cannot contain whitespace : {0}", scopeName));
			}
		}
	}
}
