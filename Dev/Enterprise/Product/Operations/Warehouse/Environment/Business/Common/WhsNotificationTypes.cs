using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business
{
	#region WhsErrorTypes class

	[Serializable]
	public class WhsErrorTypes : ErrorType
	{
		public WhsErrorTypes(string message)
			: base(message)
		{
		}

		public WhsErrorTypes(string name, string message)
			: base(name, message)
		{
		}

		public static ErrorType ZErrorMessageBox => new WhsErrorTypes("ZErrorMessageBox");
		public static ErrorType JobIsFinalised => new WhsErrorTypes((NoResString)"This Job is Finalised."); // Exception message
		public static ErrorType LineIsFinalised => new WhsErrorTypes(Res.GetString("a48c9fff-1f98-4e8e-b0c2-cde49ab433f7", "This line is Finalized."));
		public static ErrorType FinaliseZErrorMessageBox => new WhsErrorTypes("ZErrorMessageBox", (NoResString)"Finalise"); // Exception message
		public static ErrorType NoLinesEntered => new WhsErrorTypes(Res.GetString("c9859967-9e98-4597-819d-0198a5bde68b", "No lines have been entered. A Docket must have lines before it can be finalized."));
		public static ErrorType NoSecurityRights => new WhsErrorTypes(Res.GetString("69206961-1c0e-4666-bc79-58b1d20b1c79", "You do not have the Security Rights to perform this operation."));
		public static ErrorType CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled => new WhsErrorTypes(Res.GetString("fbe1039b-a122-4aac-8bed-f68f162b0e36", "Cannot perform this operation because the job is finalized or canceled."));
		public static ErrorType CannotPerformThisOperationBecauseAllSelectedLinesAreFinalised => new WhsErrorTypes(Res.GetString("16836273-7ec4-4a79-9424-c636d370bea9", "Cannot perform this operation because the selected lines are finalized."));
		public static ErrorType CannotPerformThisOperationBecauseNoWarehouseOrClient => new WhsErrorTypes(Res.GetString("9ec06898-6d7a-4f42-b080-8e64f027c0c3", "Cannot perform this operation because there is no Warehouse or Client entered."));
		public static ErrorType CannotPerformThisOperationBecauseCollectionDoesNotAllowNew => new WhsErrorTypes(Res.GetString("2ba84a46-be53-4166-9971-cd5427423da8", "Cannot perform this operation because the lines cannot be added."));
		public static ErrorType CannotFinaliseChildAdjustment => new WhsErrorTypes(Res.GetString("19d58d6a-3dc6-4715-bec4-6f243442d636", "Error has occurred while Adjusting in for the new client."));
		public static ErrorType CannotFinaliseWithoutReload => new WhsErrorTypes(Res.GetString("56f1afb1-f373-4f60-a45e-18f1cfe89864", "The docket has been updated by another job. Reload this docket before it can be finalized."));
	}

	#endregion

	#region NotificationBufferWithDefaultResponse

	public class NotificationBufferWithDefaultResponse : NotificationBuffer
	{
		#region Constructor

		public NotificationBufferWithDefaultResponse() : this(true) { }
		public NotificationBufferWithDefaultResponse(bool defaultResponse)
			: base()
		{
			fDefaultResponse = defaultResponse;
		}

		#endregion

		#region Property

		bool fDefaultResponse;
		public bool DefaultResponse
		{
			get { return fDefaultResponse; }
			set { fDefaultResponse = value; }
		}

		#endregion

		#region Override

		protected override void QueryUser(IQueryUserEventArgs e)
		{
			if (e is QueryUserMsgBoxEventArgs eventArgs)
			{
				eventArgs.Response = DefaultResponse;
			}
			base.QueryUser(e);
		}

		#endregion
	}

	#endregion
}
