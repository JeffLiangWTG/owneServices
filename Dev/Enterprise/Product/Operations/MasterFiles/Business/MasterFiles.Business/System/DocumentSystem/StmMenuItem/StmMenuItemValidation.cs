using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmMenuItemValidation : AutoStmMenuItemValidation
	{
		public StmMenuItemValidation(AutoStmMenuItem parent)
			: base(parent)
		{
		}

		protected override void CheckSU_EmailSenderOverride()
		{
			base.CheckSU_EmailSenderOverride();

			if (!string.IsNullOrEmpty(Parent.SU_EmailSenderOverride))
			{
				Parent.SU_EmailSenderOverrideInfo.AddWarning(Res.GetString("BD316AC1-8222-42D9-B59B-EDCD58316143", "In order to utilize this function please ensure that SMTP relay is enabled for this address/domain on your SMTP server"));

				if (!Env.Registry.AllowEmailsToBeSentFromUsersAddress)
				{
					var warningMessage = Res.GetString("594626AC-2D23-457E-AD65-EBCE9245B9F3", "The address override will not work because the registry setting '{0}' is off", Env.Registry.RawRegistry.AllowEmailsToBeSentFromUsersAddress.Caption);

					Parent.SU_EmailSenderOverrideInfo.AddWarning(warningMessage);
				}

				if (!EmailAddressValidation.IsEmailAddressValid(Parent.SU_EmailSenderOverride))
				{
					Parent.SU_EmailSenderOverrideInfo.AddError(Res.GetString("4B2052A2-5487-4723-B95B-EF1F26F930C4", "That is not a valid email address"));
				}
			}
		}

		protected override void CheckSU_MenuName()
		{
			base.CheckSU_MenuName();
			if (Parent.SU_MenuName.Contains(':'))
			{
				Parent.SU_MenuNameInfo.AddError(Res.GetString("191e6f49-a011-486c-807c-4e64b0359dbe", "The Menu Name cannot contain a colon (':') character."));
			}
			TranslatableDataFieldAttribute.Validate(Parent.SU_MenuNameInfo);
		}

		protected override void CheckSU_BusinessContext()
		{
			base.CheckSU_BusinessContext();
			if (Parent.SU_BusinessContext.Contains(':'))
			{
				Parent.SU_BusinessContextInfo.AddError(Res.GetString("d3b91d19-57e3-450b-b023-dd613334613f", "The Business Context cannot contain a colon (':') character."));
			}
		}

		protected override void CheckSU_MenuPath()
		{
			base.CheckSU_MenuPath();
			ZString menuPath = Parent.SU_MenuPath.Trim();

			if (Parent.SU_MenuPath.Contains(':'))
			{
				Parent.SU_MenuPathInfo.AddError(Res.GetString("1d621ef5-85a9-431e-9926-b0dc8fd86061", "The Menu Path cannot contain a colon (':') character."));
			}

			if (Parent.SU_MenuType != Core.Constants.StmMenuItemTypes.OperationalActions && (menuPath.StartsWith("/") || menuPath.EndsWith("/")))
			{
				Parent.SU_MenuPathInfo.AddError(Res.GetString("64bd8692-c067-403e-9034-45e5bc8bd441", "Please remove all leading and trailing slashes from menu paths."));
			}
		}

		protected override void CheckSU_MenuShortcut()
		{
			base.CheckSU_MenuShortcut();
			if (Parent.SU_MenuShortcut == "CtrlShift0")
			{
				Parent.SU_MenuShortcutInfo.AddWarning(Res.GetString("6a23ec66-b661-4192-abce-c29548b727a9", "The CTRL+SHIFT+0 shortcut is known to not work on Windows Vista. It is better to use another shortcut."));
			}
		}

		protected override void CheckSU_PrimaryDocPackItemId()
		{
			base.CheckSU_PrimaryDocPackItemId();
			var primaryDocPackItem = Parent.SU_PrimaryDocPackItemId;
			var pk = Parent.PK;

			var templatePivotFound = new Lazy<bool>(() =>
			{
				var query = new ZQuery(StmMenuTemplatePivotSchema.PK, primaryDocPackItem);
				query.AddToFilter(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, pk));
				return Parent.Factory.LoadTop1<StmMenuTemplatePivot>(query) != null;
			});
			var menuPivotFound = new Lazy<bool>(() =>
			{
				var query = new ZQuery(StmMenuMenuPivotSchema.PK, primaryDocPackItem);
				query.AddToFilter(new ZQuery(StmMenuMenuPivotSchema.SF_SU_Inward, pk));
				return Parent.Factory.LoadTop1<StmMenuMenuPivot>(query) != null;
			});

			if (primaryDocPackItem.IsValid && !templatePivotFound.Value && !menuPivotFound.Value)
			{
				Parent.SU_PrimaryDocPackItemIdInfo.AddError(SU_PrimaryDocPackItemError);
			}
		}

		protected override void CheckSU_DeliveryRestrictionMacro()
		{
			if (Parent.SU_DeliveryRestrictionType == nameof(DeliveryRestrictionType.UDF) && string.IsNullOrEmpty(Parent.SU_DeliveryRestrictionMacro))
			{
				Parent.SU_DeliveryRestrictionMacroInfo.AddError(Res.GetString("5ed87451-1713-430e-bbfa-360d3fee0c66", "User defined delivery restriction macro cannot be empty."));
			}
		}

		internal static string SU_PrimaryDocPackItemError
		{
			get { return Res.GetString("cc00e869-0675-4c9b-84be-56944fc32b1b", "The Primary Document was not found within the document pack"); }
		}
	}
}
