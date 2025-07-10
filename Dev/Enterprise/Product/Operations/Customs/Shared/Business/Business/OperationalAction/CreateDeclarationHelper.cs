using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business
{
	public class CreateDeclarationHelper : Integration.Customs.Shared.ICreateDeclarationHelper
	{
		#region CreateDeclaration

		public BaseJobDeclaration CreateDeclaration(ForwardingShipment shipment, ZGlobalMutex mutex, Func<BaseJobDeclaration> getExistingDeclaration)
		{
			BaseJobDeclaration declaration = null;

			if (!mutex.IsLocked)
			{
				if (AllowCreateDeclaration(shipment))
				{
					if (mutex.Lock())
					{
						declaration = getExistingDeclaration?.Invoke() ?? CreateNewDeclaration(shipment);
						if (declaration != null)
						{
							StartSynchroniser(declaration);
							if (!Globals.IsTest)
							{
								declaration.HasChanges = true;
							}
						}
					}
					else
					{
						OnNotify(shipment, NotifyType.MutexLocked);
					}
				}
				else
				{
					OnNotify(shipment, NotifyType.NotToCreateJob);
				}
			}
			else
			{
				OnNotify(shipment, NotifyType.MutexLocked);
			}

			return declaration;
		}

		bool AllowCreateDeclaration(ForwardingShipment shipment)
		{
			bool result = false;
			if (!Env.Security.CustomsDeclarationEnquiryNew.IsAllowed)
			{
				OnNotify(shipment, NotifyType.SecurityError);
			}
			else
			{
				if (IsInternalBrokerage(shipment))
				{
					result = GetAnswerToCreateDeclaration(shipment);
				}
				else
				{
					result = GetAnswerToCreateDeclarationWarning(shipment);
				}
			}

			return result;
		}

		bool IsInternalBrokerage(ForwardingShipment shipment)
		{
			ZGuid broker = ZGuid.Empty;
			bool result = false;

			if (shipment.IsImport())
			{
				broker = shipment.JS_OH_ImportBroker;
			}
			else if (shipment.IsExport())
			{
				broker = shipment.JS_OH_ExportBroker;
			}

			if (!broker.IsEmpty)
			{
				foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
				{
					if (branch.GB_OH_OrgProxy == broker)
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		BaseJobDeclaration CreateNewDeclaration(ForwardingShipment shipment)
		{
			BaseJobDeclaration internalJobDeclaration = null;
			bool importDecFromOtherCountry = false;
			var importDeclaration = GetNewImportJobDeclaration(shipment);
			importDeclaration.LoadDeclarationForShipment(shipment.JS_UniqueConsignRef);

			if (importDeclaration.DeclarationPK.IsValid)
			{
				importDecFromOtherCountry = GetAnswerToImportDecFromOtherCountry(shipment);

				if (importDecFromOtherCountry)
				{
					internalJobDeclaration = importDeclaration.CreateDeclarationAgainstShipment();
					OnNotify(shipment, NotifyType.DeclarationImportSucceed);
				}
			}

			if (!importDecFromOtherCountry)
			{
				if (HasMoreThanOneTypeOfJobDeclarationPerShipment)
				{
					internalJobDeclaration = (BaseJobDeclaration)shipment.Factory.New(GetTypeForNewJobDeclaration());
				}
				else
				{
					internalJobDeclaration = BaseJobDeclaration.New(shipment.Factory);
				}
				internalJobDeclaration.JE_JS = shipment.PK;
				internalJobDeclaration.ExternalFactoryRefreshEnabled = true;
				internalJobDeclaration.DefaultFreightAmountFromShipmentChargeableAmount();
				internalJobDeclaration.DefaultAdditionalReferenceNumbersFromShipment();
				OnNotify(shipment, NotifyType.DeclarationCreateSucceed);
			}
			if (internalJobDeclaration != null)
			{
				shipment.RaiseJobDeclarationCreated(internalJobDeclaration);
			}

			return internalJobDeclaration;
		}

		#endregion

		#region Automatic Synchronisation

		protected virtual void StartSynchroniser(BaseJobDeclaration declaration)
		{
			if (declaration != null)
			{
				((Integration.Customs.IJobDeclarationWithShipmentSynchonisation)declaration).SynchroniseWithShipmentIfNeeded();
			}
		}

		#endregion

		#region Notify

		public delegate void NotifyDelegate(ForwardingShipment shipment, NotifyType notifyType);
		public NotifyDelegate Notify;

		public void OnNotify(ForwardingShipment shipment, NotifyType notifyType)
		{
			if (Notify != null)
			{
				Notify(shipment, notifyType);
			}
		}

		public enum NotifyType
		{
			MutexLocked,
			NotToCreateJob,
			SecurityError,
			DeclarationCreateSucceed,
			DeclarationImportSucceed,
		}

		public string GetDescriptionForNotifyType(NotifyType notifyType)
		{
			switch (notifyType)
			{
				case NotifyType.MutexLocked:
					return Res.GetString("C6FC5756-6794-455C-91EB-8A99ECEB89C0", "Another Declaration Create is in progress.");
				case NotifyType.NotToCreateJob:
					return Res.GetString("22E70EE6-720F-4337-B471-1F35AC1BCBFB", "A Declaration was not created.");
				case NotifyType.SecurityError:
					return Res.GetString("71339212-A57B-48D5-9A11-298658015177", "'Customs Declaration Enquiry New' permission is required.");
				case NotifyType.DeclarationCreateSucceed:
					return Res.GetString("0A75B8E3-FE82-4C40-B51D-11940D4A7C76", "A Declaration was created.");
				case NotifyType.DeclarationImportSucceed:
					return Res.GetString("EDF282B4-213F-4AC1-AA55-8A9918166AD3", "A Declaration was imported.");
			}

			return notifyType.ToString();
		}

		#endregion

		#region Virtual Methods

		public virtual ImportJobDeclaration GetNewImportJobDeclaration(ForwardingShipment shipment)
		{
			return new ImportJobDeclaration(shipment.Factory);
		}

		protected virtual bool HasMoreThanOneTypeOfJobDeclarationPerShipment
		{
			get { return false; }
		}

		/// <summary>
		/// Useful when type decider for new object depends on a row value which can only be assigned after a new object is created
		/// NZ has ECI Write-Off and FormalEntry JobDeclaration and MessageSubType decides. 
		/// </summary>
		protected virtual Type GetTypeForNewJobDeclaration()
		{
			return typeof(BaseJobDeclaration);
		}

		#endregion

		#region Static Strings

		public static string NotNominatedCustomsBrokerQuestionText
		{
			get { return Res.GetString("7130fbae-5dbe-400f-8f8a-c0a664ebf0ca", "In the case that your company is not the nominated customs broker, do you still want to create the declaration?"); }
		}

		public static string NotNominatedCustomsBrokerMessage
		{
			get { return Res.GetString("38595510-d239-42ff-8b26-0f6926b4a924", "Your company is not the nominated customs broker."); }
		}

		public static string ImportDecFromOtherCountryQuestionText
		{
			get { return Res.GetString("d4233da0-dd4f-497b-9b9c-1308e2e9f2c6", "If for a shipment, there is a declaration that has been created in another country. Should the data be imported?"); }
		}

		public static string ImportDecFromOtherCountryMessage
		{
			get { return Res.GetString("6470cb56-10db-4058-807b-5d3a70359ee5", "There is a Declaration job that has been created for this Shipment in another Country."); }
		}

		#endregion

		#region Questions

		public ICollection<CreateBrokerageQuestion> AllQuestions
		{
			get
			{
				if (fAllQuestions == null)
				{
					fAllQuestions = new List<CreateBrokerageQuestion>();
					fAllQuestions.AddRange(QuestionsToCreateDeclaration);
					fAllQuestions.AddRange(QuestionsToCreateDeclarationWarning);
					fAllQuestions.AddRange(QuestionsToImportDecFromOtherCountry);
				}
				return fAllQuestions;
			}
		}
		List<CreateBrokerageQuestion> fAllQuestions;

		ICollection<CreateBrokerageQuestion> QuestionsToCreateDeclaration
		{
			get
			{
				return fQuestionsToCreateDeclaration ?? (fQuestionsToCreateDeclaration = FreightDataRegistry.Instance.CreateBrokerageJobAutomatically.Value ? new List<CreateBrokerageQuestion>() : GetQuestionsToCreateDeclarationCore());
			}
		}
		ICollection<CreateBrokerageQuestion> fQuestionsToCreateDeclaration;

		bool fConfirmCreateNewDeclaration;
		public bool ConfirmCreateNewDeclaration
		{
			get { return fConfirmCreateNewDeclaration; }
			set
			{
				if (fConfirmCreateNewDeclaration != value)
				{
					fQuestionsToCreateDeclaration = null;
					fAllQuestions = null;
				}
				fConfirmCreateNewDeclaration = value;
			}
		}

		protected virtual ICollection<CreateBrokerageQuestion> GetQuestionsToCreateDeclarationCore()
		{
			var result = new List<CreateBrokerageQuestion>();
			if (ConfirmCreateNewDeclaration)
			{
				result.Add(new CreateBrokerageQuestion()
				{
					ConditionToAsk = delegate(ForwardingShipment shipment) { return true; }
				});
			}
			return result;
		}

		ICollection<CreateBrokerageQuestion> QuestionsToCreateDeclarationWarning
		{
			get
			{
				return fQuestionsToCreateDeclarationWarning ?? (fQuestionsToCreateDeclarationWarning = GetQuestionsToCreateDeclarationWarningCore());
			}
		}
		ICollection<CreateBrokerageQuestion> fQuestionsToCreateDeclarationWarning;

		protected virtual ICollection<CreateBrokerageQuestion> GetQuestionsToCreateDeclarationWarningCore()
		{
			return new List<CreateBrokerageQuestion>()
			{
				new CreateBrokerageQuestion()
				{
					Question = NotNominatedCustomsBrokerQuestionText,
					Message = NotNominatedCustomsBrokerMessage,
					ConditionToAsk = delegate(ForwardingShipment shipment) { return true; }
				}
			};
		}

		ICollection<CreateBrokerageQuestion> QuestionsToImportDecFromOtherCountry
		{
			get
			{
				return fQuestionsToImportDecFromOtherCountry ?? (fQuestionsToImportDecFromOtherCountry = GetQuestionsToImportDecFromOtherCountryCore());
			}
		}
		ICollection<CreateBrokerageQuestion> fQuestionsToImportDecFromOtherCountry;

		protected virtual ICollection<CreateBrokerageQuestion> GetQuestionsToImportDecFromOtherCountryCore()
		{
			return new List<CreateBrokerageQuestion>()
			{
				new CreateBrokerageQuestion()
				{
					Question = ImportDecFromOtherCountryQuestionText,
					Message = ImportDecFromOtherCountryMessage,
					ConditionToAsk = delegate(ForwardingShipment shipment) { return true; }
				}
			};
		}

		bool GetAnswerToCreateDeclaration(ForwardingShipment shipment)
		{
			return GetAnswerCore(shipment, QuestionsToCreateDeclaration, QuestionType.CreateDeclarationQuery);
		}

		bool GetAnswerToCreateDeclarationWarning(ForwardingShipment shipment)
		{
			return GetAnswerCore(shipment, QuestionsToCreateDeclarationWarning, QuestionType.CreateDeclarationWarning);
		}

		bool GetAnswerToImportDecFromOtherCountry(ForwardingShipment shipment)
		{
			return GetAnswerCore(shipment, QuestionsToImportDecFromOtherCountry, QuestionType.ImportDeclarationQuery);
		}

		bool GetAnswerCore(ForwardingShipment shipment, ICollection<CreateBrokerageQuestion> questions, QuestionType type)
		{
			bool result = true;
			if (GetAnswer != null)
			{
				result = GetAnswer(shipment, questions, type);
			}

			return result;
		}

		public delegate bool GetAnswerDelegate(ForwardingShipment shipment, ICollection<CreateBrokerageQuestion> questions, QuestionType type);
		public GetAnswerDelegate GetAnswer;

		public enum QuestionType
		{
			CreateDeclarationQuery,
			CreateDeclarationWarning,
			ImportDeclarationQuery
		}

		#endregion

		#region Implementation

		public void CreateDeclaration(Integration.Forwarding.IForwardingShipment shipment)
		{
			var forwardingShipment = shipment as ForwardingShipment;
			if (forwardingShipment != null && !forwardingShipment.ReadOnly)
			{
				using var mutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(forwardingShipment.PK);
				CreateDeclaration(forwardingShipment, mutex, () => (BaseJobDeclaration)forwardingShipment.GetDeclaration());
			}
		}

		#endregion
	}

	#region CreateBrokerageQuestion

	public class CreateBrokerageQuestion
	{
		public string Question;
		public string Message;
		public bool DefaultAnswer;
		public bool Answer;
		public ConditionToAskDelegate ConditionToAsk;
	}

	public delegate bool ConditionToAskDelegate(ForwardingShipment shipment);

	#endregion
}
