using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Module Controller for AccBankAccount.
	/// </summary>
	public class AccBankAccountController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccBankAccountController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccBankAccount;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccBankAccount; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccBankAccount); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccBankAccountForm((AccBankAccount)businessEntity);
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (!sourceEntity.CanDelete)
			{
				Globals.Message.Show(sourceEntity.ReasonForNotAbleToDelete);
			}
			else
			{
				if (!sourceEntity.ReasonForNotAbleToDelete.IsEmpty)
				{
					Globals.Message.Show(sourceEntity.ReasonForNotAbleToDelete);
				}
				base.ShowDeleteForm(sourceEntity);
			}
			return LastShownForm;
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get
			{
				return Env.Security.BankAccountsModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get
			{
				return Env.Security.BankAccountsModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get
			{
				return Env.Security.BankAccountsModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get
			{
				return Env.Security.BankAccounts;
			}
		}

		public IZForm ShowNewForm(ZString type)
		{
			var bankAccount = (AccBankAccount)GetNewBusinessEntityInLocalFactory();
			bankAccount.AB_AccountType = type;
			return ShowFormForNewEntity(bankAccount);
		}
	}
}
