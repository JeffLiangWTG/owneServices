using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class CreateAndSubmitBrokerageJobRunner
	{
		public CreateAndSubmitBrokerageJobRunner()
		{
		}

		public bool ExecuteSubmit { get; set; }
		public IOperationalActionSectionLog Log { get; set; }

		#region BrokerageSubmitRunner

		protected BrokerageSubmitRunner SubmitRunner
		{
			get
			{
				return fSubmitRunner ?? (fSubmitRunner = GetBrokerageSubmitRunnerCore());
			}
		}
		BrokerageSubmitRunner fSubmitRunner;

#if DEBUG
		protected virtual
#endif
		BrokerageSubmitRunner GetBrokerageSubmitRunnerCore()
		{
			return new BrokerageSubmitRunner();
		}

		#endregion

		#region Execute

		public void Execute(ForwardingShipment shipment)
		{
			Argument.NotNull(shipment, "shipment");

			var factory = new BusinessObjectFactory();
			var shipment1 = factory.Load<ForwardingShipment>(shipment.PK);
			BaseJobDeclaration declaration = null;

			if (shipment1 == null || shipment1.IsDeleted)
			{
				Notify(shipment, OperationalActionLogErrorLevel.Warning, ShipmentNotExistsText);
			}
			else
			{
				declaration = (BaseJobDeclaration)shipment1.GetDeclaration();
				if (declaration != null)
				{
					if (!ExecuteSubmit)
					{
						Notify(shipment, OperationalActionLogErrorLevel.Warning, ShipmentHasDeclarationText);
					}
				}
				else
				{
					var mutex = GetMutex(shipment1);
					declaration = CreateDeclarationHelper.CreateDeclaration(shipment1, mutex, () => (BaseJobDeclaration)shipment1.GetDeclaration());
					if (declaration != null)
					{
						try
						{
							factory.Save();
						}
						catch (ZSaveException e)
						{
							ZExceptionReporting.HandleSaveException(e);
						}
					}
					if (mutex.HasLock)
					{
						mutex.Unlock();
					}
				}

				if (ExecuteSubmit && declaration != null)
				{
					SubmitRunner.Log = Log;
					SubmitRunner.Execute(declaration);
				}
			}
		}

		public CreateDeclarationHelper CreateDeclarationHelper
		{
			get
			{
				if (fCreateDeclarationHelper == null)
				{
					fCreateDeclarationHelper = GetCreateDeclarationHelperCore();
					fCreateDeclarationHelper.ConfirmCreateNewDeclaration = false;
					fCreateDeclarationHelper.Notify = new CreateDeclarationHelper.NotifyDelegate((shipment, type) => { Notify(shipment, type); });
					fCreateDeclarationHelper.GetAnswer = new CreateDeclarationHelper.GetAnswerDelegate((shipment, questions, type) => { return GetAnswer(shipment, questions, type); });
				}
				return fCreateDeclarationHelper;
			}
		}
		CreateDeclarationHelper fCreateDeclarationHelper;

		protected virtual CreateDeclarationHelper GetCreateDeclarationHelperCore()
		{
			var helperProvider = new CreateDeclarationHelperProvider();
			return (CreateDeclarationHelper)helperProvider.NewCreateDeclarationHelper(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		void Notify(ForwardingShipment shipment, CreateDeclarationHelper.NotifyType notifyType)
		{
			switch (notifyType)
			{
				case (CreateDeclarationHelper.NotifyType.MutexLocked):
					Notify(shipment, OperationalActionLogErrorLevel.Warning, MutexLockText);
					break;
				case (CreateDeclarationHelper.NotifyType.DeclarationCreateSucceed):
					Notify(shipment, OperationalActionLogErrorLevel.Informational, DeclarationCreateSucceedText);
					break;
				case (CreateDeclarationHelper.NotifyType.DeclarationImportSucceed):
					Notify(shipment, OperationalActionLogErrorLevel.Informational, DeclarationImportSucceedText);
					break;
			}
		}

		void Notify(ForwardingShipment shipment, OperationalActionLogErrorLevel level, string message)
		{
			if (Log != null)
			{
				Log.NotifyFormat(level, "{0}: {1}", new LogControllerLink(shipment.JS_UniqueConsignRef, ControllerIDs.JobShipment, shipment.PK), message);
			}
		}

		#endregion

		#region Mutex

		ZGlobalMutex GetMutex(ForwardingShipment shipment)
		{
			return DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK);
		}

		#endregion

		#region Static Strings

		public static string MutexLockText
		{
			get { return Res.GetString("3d73a6f5-e00b-46e1-933f-9fc12c395041", "Someone else is already in the process of creating a declaration for shipment.\r\nYou should be able to access the declaration when the person has saved the record. Please try later."); }
		}

		protected static string DeclarationImportSucceedText
		{
			get { return Res.GetString("7cce525f-08b6-494c-944f-89390dff5f24", "A Brokerage Job was imported successfully."); }
		}

		protected static string DeclarationCreateSucceedText
		{
			get { return Res.GetString("fa35c6ae-138b-4903-964b-8d83920ed0bc", "A Brokerage Job was created successfully."); }
		}

		protected static string ShipmentNotExistsText
		{
			get { return Res.GetString("3ebedb9c-b65e-49b4-b196-4d45ed7d93ad", "This Shipment does not exist or has already been deleted."); }
		}

		protected static string ShipmentHasDeclarationText
		{
			get { return Res.GetString("d1378fe0-0600-4c8c-bcd3-9330578c6a4a", "This Shipment already has a Brokerage Job."); }
		}

		protected static string ChosenNotToCreateDeclarationText
		{
			get { return Res.GetString("bda37444-07ec-4284-96f3-8d3ce4164894", "You have chosen not to create a declaration."); }
		}

		#endregion

		#region Questions

		public ICollection<CreateBrokerageQuestion> AllQuestions
		{
			get { return CreateDeclarationHelper.AllQuestions; }
		}

		bool GetAnswer(ForwardingShipment shipment, ICollection<CreateBrokerageQuestion> questions, CreateDeclarationHelper.QuestionType type)
		{
			bool result = true;
			foreach (var question in questions)
			{
				if (question.ConditionToAsk != null && question.ConditionToAsk(shipment) && !question.Answer)
				{
					if (type != CreateDeclarationHelper.QuestionType.ImportDeclarationQuery && !string.IsNullOrEmpty(question.Message))
					{
						Notify(shipment, OperationalActionLogErrorLevel.Warning, question.Message + " " + ChosenNotToCreateDeclarationText);
					}
					result = false;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
