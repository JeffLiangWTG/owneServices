using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class StatementDeleteAndSendingActionCollection : NonPersistentBusinessObjectCollection<StatementDeleteAndSendingAction>
	{
		public StatementDeleteAndSendingActionCollection(CusStatementHeader statementHeader)
			: base(statementHeader.Factory)
		{
			this.StatementHeader = statementHeader;
			PopulateStatementHeaderElements();

			if (Count == 1)
			{
				this[0].US_SendMessage = true;
			}
		}

		public StatementDeleteAndSendingActionCollection(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			PopulateDeclarationElements(declaration);

			if (Count == 1)
			{
				this[0].US_SendMessage = true;
			}
		}

		public StatementDeleteAndSendingActionCollection(ReconDeclaration reconDeclaration)
			: base(reconDeclaration.Factory)
		{
			IsReconciliationAction = true;
			PopulateReconDeclarationElements(reconDeclaration);
		}

		public readonly CusStatementHeader StatementHeader;
		public readonly bool IsReconciliationAction;

		public bool HasAtLeastOneToSendMessageFor
		{
			get
			{
				foreach (StatementDeleteAndSendingAction action in this)
				{
					if (action.US_SendMessage)
					{
						return true;
					}
				}

				return false;
			}
		}

		public ZBool IsCancelled
		{
			get { return isCancelled; }
			set { isCancelled = value; }
		}
		ZBool isCancelled;

		public bool SendMessage
		{
			get
			{
				foreach (StatementDeleteAndSendingAction action in this)
				{
					if (action.US_SendMessage)
					{
						return true;
					}
				}
				return false;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public bool HasMessageErrors()
		{
			this.RunPreSaveValidation();

			foreach (StatementDeleteAndSendingAction action in this)
			{
				if (action.US_SendMessage && action.HasMessageErrors)
				{
					return true;
				}
			}
			return false;
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Users cannot create a new element in the grid and ImportMessageSendingAction requires CusEntryHeader");
		}

		void PopulateStatementHeaderElements()
		{
			foreach (CusStatementLine line in StatementHeader.StatementLines)
			{
				if (line.IsActive)
				{
					Add(new StatementDeleteAndSendingAction(line));
				}
			}
		}

		void PopulateDeclarationElements(JobDeclaration declaration)
		{
			Add(new StatementDeleteAndSendingAction(declaration));
		}

		void PopulateReconDeclarationElements(ReconDeclaration reconDeclaration)
		{
			Add(new StatementDeleteAndSendingAction(reconDeclaration));
			this[0].US_SendMessage = true;
		}

		#endregion
	}
}
