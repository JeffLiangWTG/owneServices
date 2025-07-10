#if NETFRAMEWORK
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
#elif NET
using Enterprise.Services.ServiceHost.NetCore;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.Charges
{
	[GlowTicketAuthentication]
	public class JobHeaderAndChargesController : ControllerBase
	{
#if NET
		readonly IAccountingBillingService service;
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string")]
		public const string MaxErrorError = "Error";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string")]
		public const string MaxErrorWarning = "Warning";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string")]
		public const string MaxErrorNone = "None";

#if NET
		public JobHeaderAndChargesController(IAccountingBillingService service)
		{
			this.service = service;
		}
#endif

		[Route("api/jobHeaderAndCharges/updateJobHeaderAndCharges")]
		[HttpPost]
		public IActionResult UpdateJobHeaderAndCharges([FromBody] JobHeaderModel jobHeader)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (jobHeader == null || !Enum.TryParse(jobHeader.EntityState, out ValidEntityState entityState) || entityState == ValidEntityState.Deleted || entityState == ValidEntityState.Unchanged && (jobHeader.JobCharges == null || jobHeader.JobCharges.Count == 0))
				{
					return BadRequest();
				}
				else
				{
					var adapter = ObjectFactory.Get<IJobCostingAdapter>();
					var logger = new XmlSessionTracker(new SimpleLogger());
					var factory = new BusinessObjectFactory();

					if (entityState != ValidEntityState.Unchanged)
					{
						UpdateJobHeader(jobHeader, factory, logger);
					}
					if (jobHeader.JobCharges != null && jobHeader.JobCharges.Count != 0)
					{
						UpdateCharges(jobHeader, adapter, logger, factory);
					}

					if (!logger.HasErrors)
					{
						try
						{
							factory.Save();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							logger.Log(Integration.LogType.Error, ex.Message);
						}
					}

					return ConstructResponse(logger);
				}
			}
		}

		public void UpdateJobHeader(JobHeaderModel jobHeaderModel, BusinessObjectFactory factory, XmlSessionTracker logger)
		{
			if (Enum.TryParse(jobHeaderModel.EntityState, out ValidEntityState entityState))
			{
				switch (entityState)
				{
					case ValidEntityState.Added:
						var parent = factory.Load(jobHeaderModel.ParentTableCode, jobHeaderModel.JH_ParentID) as IJobHeaderParent;
						if (parent != null)
						{
#if NETFRAMEWORK
							var billingController = new AccountingBillingController();
#elif NET
							var billingController = new AccountingBillingController(service);
#endif
							if (jobHeaderModel.ParentTableCode != null && jobHeaderModel.JH_OA_LocalChargesAddr != null)
							{
								billingController.CreateJobHeader(jobHeaderModel.JH_ParentID, jobHeaderModel.ParentTableCode, GlbStaff.CurrentUser.PK.ToGuid(), jobHeaderModel.JH_GB, jobHeaderModel.JH_GE, (Guid)jobHeaderModel.JH_OA_LocalChargesAddr);
							}
							else
							{
								logger.Log(Integration.LogType.Error, Res.GetString("45fe5bd4-0bfa-4a75-823f-1832d781571a", "Failed to create job."));
							}
						}
						break;

					case ValidEntityState.Modified:
						var job = factory.Load<Job>(jobHeaderModel.JH_PK);
						if (job != null)
						{
							SetJobHeaderProperties(job, jobHeaderModel);
						}
						else
						{
							logger.Log(Integration.LogType.Error, Res.GetString("570cbb3a-e29d-47e8-aa5c-27d5590ff688", "Failed to load Job."));
						}
						break;

					case ValidEntityState.Deleted:
						throw new NotImplementedException();

					default:
						break;
				}
			}
		}

		public void SetJobHeaderProperties(Job job, JobHeaderModel model)
		{
			if (model.JH_A_JCL != null)
			{
				job.JH_A_JCL = (ZDateTime)model.JH_A_JCL;
			}
			if (model.JH_A_JOP != null)
			{
				job.JH_A_JOP = (ZDateTime)model.JH_A_JOP;
			}
			job.JH_AgentChargesCFX = model.JH_AgentChargesCFX;
			job.JH_ARInvoiceReference = model.JH_ARInvoiceReference;
			job.JH_Description = model.JH_Description;
			job.JH_Direction = model.JH_Direction;
			job.JH_ExcludeFromPeriodicRating = model.JH_ExcludeFromPeriodicRating;
			job.JH_GB = model.JH_GB;
			job.JH_GC = model.JH_GC;
			job.JH_GE = model.JH_GE;
			job.JH_GS_NKRepOps = model.JH_GS_NKRepOps;

			job.JH_HeaderType = model.JH_HeaderType;
			job.JH_HoldReason = model.JH_HoldReason;
			job.JH_IsProfitSharePosted = model.JH_IsProfitSharePosted;
			if (model.JH_IsActive && !job.JH_IsActive)
			{
				job.ActivateJob();
			}
			else if (!model.JH_IsActive && job.JH_IsActive)
			{
				job.MarkAsInactive();
			}
			if (model.JH_JH_ParentJob != null)
			{
				job.JH_JH_ParentJob = (ZGuid)model.JH_JH_ParentJob;
			}
			job.JH_JobBufferPercentOverride = model.JH_JobBufferPercentOverride;
			job.JH_JobLocalReference = model.JH_JobLocalReference;

			if (model.JH_JobPlannedStartDate != null)
			{
				job.JH_JobPlannedStartDate = (ZDateTime)model.JH_JobPlannedStartDate;
			}
			job.JH_LocalChargesCFX = model.JH_LocalChargesCFX;
			job.JH_LocalClientInvoicingStyle = model.JH_LocalClientInvoicingStyle;
			job.JH_Name = model.JH_Name;
			if (model.JH_OA_AgentCollectAddr != null)
			{
				job.JH_OA_AgentCollectAddr = (ZGuid)model.JH_OA_AgentCollectAddr;
			}
			if (model.JH_OA_LocalChargesAddr != null)
			{
				job.JH_OA_LocalChargesAddr = (ZGuid)model.JH_OA_LocalChargesAddr;
			}
			if (model.JH_OC_LocalBillingContact != null)
			{
				job.JH_OC_LocalBillingContact = (ZGuid)model.JH_OC_LocalBillingContact;
			}
			job.JH_ParentID = model.JH_ParentID;
			job.JH_PaymentCollectionStatus = model.JH_PaymentCollectionStatus;
			job.JH_ProfitLossReasonCode = model.JH_ProfitLossReasonCode;
			if (model.JH_ProfitShareInvoice != null)
			{
				job.JH_ProfitShareInvoice = (ZGuid)model.JH_ProfitShareInvoice;
			}
			job.JH_RatingHasBeenRun = model.JH_RatingHasBeenRun;
			if (model.JH_RevenueRecognizedDate != null)
			{
				job.JH_RevenueRecognizedDate = (ZDateTime)model.JH_RevenueRecognizedDate;
			}
			job.JH_SingleAgentsInvoicePerConsol = model.JH_SingleAgentsInvoicePerConsol;
			job.JH_Status = model.JH_Status;
			if (model.JH_SystemCreateTimeUtc != null)
			{
				job.JH_SystemCreateTimeUtc = (ZDateTime)model.JH_SystemCreateTimeUtc;
			}
			job.JH_SystemCreateUser = model.JH_SystemCreateUser;
			if (model.JH_SystemLastEditTimeUtc != null)
			{
				job.JH_SystemLastEditTimeUtc = (ZDateTime)model.JH_SystemLastEditTimeUtc;
			}
			job.JH_SystemLastEditUser = model.JH_SystemLastEditUser;
			job.JH_TH_NKQuoteNumber = model.JH_TH_NKQuoteNumber;
			job.JH_UniqueJobInvoiceNumber = model.JH_UniqueJobInvoiceNumber;
			job.JH_GS_NKRepSales = model.JH_GS_NKRepSales;
		}

		public XmlSessionTracker UpdateCharges(JobHeaderModel jobHeader, IJobCostingAdapter adapter, XmlSessionTracker logger, BusinessObjectFactory factory)
		{
			var shipment = CreateShipment(jobHeader, factory, DefaultDataObjectWriterStrategy.Instance);
			shipment.JobCosting.SetChargeLineCollection(() => ConvertCharges(jobHeader.JobCharges, factory, DefaultDataObjectWriterStrategy.Instance));

			try
			{
				adapter.ImportCharges(factory, logger, shipment, jobHeader.JH_ParentID, jobHeader.ParentTableCode);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!logger.HasErrors)
				{
					throw;
				}
			}

			return logger;
		}

		IActionResult ConstructResponse(XmlSessionTracker logger)
		{
			var maxError = MaxErrorNone;

			if (logger.HasErrors)
			{
				maxError = MaxErrorError;
			}
			else if (logger.HasWarnings)
			{
				maxError = MaxErrorWarning;
			}

			var message = new ZStringBuilder();
			foreach (var log in ((IXmlImportLogger)logger).Logs)
			{
				message.AppendLine(log.Message);
			}

			var response = new UpdateChargesJsonResponse(maxError, message.ToString());

#if NETFRAMEWORK
			return new NegotiatedContentResult<string>(HttpStatusCode.OK, Newtonsoft.Json.JsonConvert.SerializeObject(response), this);
#elif NET
			return new JsonResult(Newtonsoft.Json.JsonConvert.SerializeObject(response));
#endif
		}

		public class UpdateChargesJsonResponse
		{
			public UpdateChargesJsonResponse(ZString maxError, ZString message)
			{
				MaxError = maxError;
				Message = message;
			}
			public UpdateChargesJsonResponse()
			{
			}
			public string MaxError { get; set; }
			public string Message { get; set; }
		}

		enum ValidEntityState
		{
			Added = 0,
			Modified = 1,
			Deleted = 2,
			Unchanged = 3
		}

		InstructionType GetInstructionTypeFromEntityState(string entityStateString)
		{
			InstructionType instructionType;

			if (Enum.TryParse(entityStateString, out ValidEntityState entityState))
			{
				switch (entityState)
				{
					case ValidEntityState.Added:
						instructionType = InstructionType.Insert;
						break;
					case ValidEntityState.Deleted:
						instructionType = InstructionType.Delete;
						break;
					case ValidEntityState.Modified:
						instructionType = InstructionType.Update;
						break;
					default:
						instructionType = InstructionType.UpdateAndInsertIfNotFound;
						break;
				}

				return instructionType;
			}
			else
			{
				throw new DataObjectReadFailureException(Res.GetString("3ea8f067-4e72-4fd5-b409-c260f1d9652e", "Invalid entity state."));
			}
		}

		Shipment CreateShipment(JobHeaderModel jobHeader, BusinessObjectFactory factory, IDataObjectWriterStrategy writerStrategy)
		{
			var shipment = new Shipment(writerStrategy);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipment.JobCosting = new JobCosting(writerStrategy);

			var branch = factory.Load<GlbBranch>(jobHeader.JH_GB);
			var department = factory.Load<GlbDepartment>(jobHeader.JH_GE);

			shipment.JobCosting.Branch = new Branch()
			{
				Code = branch?.GB_Code,
				Name = branch?.GB_BranchName
			};

			shipment.JobCosting.Department = new Department()
			{
				Code = department?.GE_Code,
				Name = department?.GE_Desc
			};

			return shipment;
		}

		List<ChargeLine> ConvertCharges(ICollection<JobChargeModel> jobCharges, BusinessObjectFactory factory, IDataObjectWriterStrategy writerStrategy)
		{
			var chargeLines = new List<ChargeLine>();

			foreach (var jobCharge in jobCharges)
			{
				OrgHeader creditorOrg = null;
				OrgHeader debtorOrg = null;
				AccTaxRate costGSTRate = null;
				AccTaxRate sellGSTRate = null;
				var chargeCode = factory.Load<AccChargeCode>(jobCharge.JR_AC);
				var branch = factory.Load<GlbBranch>(jobCharge.JR_GB);
				var department = factory.Load<GlbDepartment>(jobCharge.JR_GE);

				if (jobCharge.JR_OH_CostAccount != null)
				{
					creditorOrg = factory.Load<OrgHeader>((ZGuid)jobCharge.JR_OH_CostAccount);
				}
				if (jobCharge.JR_OH_SellAccount != null)
				{
					debtorOrg = factory.Load<OrgHeader>((ZGuid)jobCharge.JR_OH_SellAccount);
				}
				if (jobCharge.JR_AT_CostGSTRate != null)
				{
					costGSTRate = factory.Load<AccTaxRate>((ZGuid)jobCharge.JR_AT_CostGSTRate);
				}
				if (jobCharge.JR_AT_SellGSTRate != null)
				{
					sellGSTRate = factory.Load<AccTaxRate>((ZGuid)jobCharge.JR_AT_SellGSTRate);
				}

				var chargeLine = new ChargeLine(writerStrategy)
				{
					Branch = new Branch()
					{
						Code = branch?.GB_Code,
						Name = branch?.GB_BranchName
					},

					Department = new Department()
					{
						Code = department?.GE_Code,
						Name = department?.GE_Desc
					},

					ChargeCode = new ChargeCode()
					{
						Code = chargeCode?.AC_Code,
						Description = chargeCode?.AC_DescMultilingual
					},

					CostAPInvoiceNumber = jobCharge.JR_APInvoiceNum,
					CostDueDate = jobCharge.JR_PaymentDate,

					ImportMetaData = new ImportMetaData(writerStrategy)
					{
						Instruction = GetInstructionTypeFromEntityState(jobCharge.EntityState)
					},

					CostGSTVATID = new TaxID()
					{
						TaxCode = costGSTRate?.AT_Code,
						Description = costGSTRate?.AT_Description
					},

					CostInvoiceDate = jobCharge.JR_APInvoiceDate,
					CostLocalAmount = jobCharge.JR_LocalCostAmt,
					CostOSAmount = jobCharge.JR_OSCostAmt,

					CostOSCurrency = new Currency()
					{
						Code = jobCharge.JR_RX_NKCostCurrency,
					},

					CostOSGSTVATAmount = jobCharge.JR_OSCostGSTAmt,

					Creditor = new OrganizationReference()
					{
						Key = creditorOrg?.OH_Code,
						Type = nameof(DataContextType.Organization)
					},

					Debtor = new OrganizationReference()
					{
						Key = debtorOrg?.OH_Code,
						Type = nameof(DataContextType.Organization)
					},

					Description = jobCharge.JR_Desc,
					DisplaySequence = jobCharge.JR_DisplaySequence,

					SellGSTVATID = new TaxID()
					{
						TaxCode = sellGSTRate?.AT_Code,
						Description = sellGSTRate?.AT_Description
					},

					SellInvoiceType = jobCharge.JR_InvoiceType,
					SellLocalAmount = jobCharge.JR_LocalSellAmt,
					SellOSAmount = jobCharge.JR_OSSellAmt,

					SellOSCurrency = new Currency()
					{
						Code = jobCharge.JR_RX_NKSellCurrency,
					},

					SupplierReference = jobCharge.JR_CostReference,
					SellReference = jobCharge.JR_SellReference,
					GovernmentReportingSellChargeCode = jobCharge.JR_SellGovtChargeCode,
					GovernmentReportingCostChargeCode = jobCharge.JR_CostGovtChargeCode,
					CostExchangeRate = jobCharge.JR_OSCostExRate,
					SellExchangeRate = jobCharge.JR_OSSellExRate,
					CostRatingBehaviour = new CodeDescriptionPair
					{
						Code = jobCharge.CostRatingBehaviour,
						Description = JobChargeLookups.GetDescriptionForRatingBehaviourCode(jobCharge.CostRatingBehaviour)
					},

					SellRatingBehaviour = new CodeDescriptionPair
					{
						Code = jobCharge.SellRatingBehaviour,
						Description = JobChargeLookups.GetDescriptionForRatingBehaviourCode(jobCharge.SellRatingBehaviour)
					}
				};

				if (jobCharge.EntityState == nameof(ValidEntityState.Modified) || jobCharge.EntityState == nameof(ValidEntityState.Deleted))
				{
					chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>
					{
						new MatchingCriteria()
						{
							FieldName = nameof(ZDataTable.PrimaryKey),
							Value = jobCharge.JR_PK.ToString()
						}
					});
				}
				else
				{
					chargeLine.ImportMetaData.SetMatchingCriteriaCollection(() => new List<MatchingCriteria>());
				}

				chargeLines.Add(chargeLine);
			}

			return chargeLines;
		}

		public class JobHeaderModel : IJobHeader
		{
			Guid IJobHeader.JH_PK
			{
				get
				{
					return JH_PK;
				}
			}
			public Guid JH_PK { get; set; }

			public DateTime? JH_A_JCL { get; set; }
			public DateTime? JH_A_JOP { get; set; }
			public decimal JH_AgentChargesCFX { get; set; }
			public string JH_ARInvoiceReference { get; set; }
			public string JH_ClientContractNumber { get; set; }
			public string JH_Description { get; set; }
			public string JH_Direction { get; set; }
			public bool JH_ExcludeFromPeriodicRating { get; set; }
			public Guid JH_GB { get; set; }
			public Guid JH_GC { get; set; }
			public Guid JH_GE { get; set; }
			public string JH_GS_NKRepOps { get; set; }
			public string JH_GS_NKRepSales { get; set; }
			public string JH_HeaderType { get; set; }
			public string JH_HoldReason { get; set; }
			public bool JH_IsProfitSharePosted { get; set; }
			public bool JH_IsValid { get; set; }
			public bool JH_IsActive { get; set; }
			public Guid? JH_JH_ParentJob { get; set; }
			public byte JH_JobBufferPercentOverride { get; set; }
			public string JH_JobLocalReference { get; set; }
			public string JH_JobNum { get; set; }
			public DateTime? JH_JobPlannedStartDate { get; set; }
			public decimal JH_LocalChargesCFX { get; set; }
			public string JH_LocalClientInvoicingStyle { get; set; }
			public string JH_Name { get; set; }
			public Guid? JH_OA_AgentCollectAddr { get; set; }
			public Guid? JH_OA_LocalChargesAddr { get; set; }
			public Guid? JH_OC_LocalBillingContact { get; set; }
			public Guid JH_ParentID { get; set; }
			public string JH_PaymentCollectionStatus { get; set; }
			public string JH_ProfitLossReasonCode { get; set; }
			public Guid? JH_ProfitShareInvoice { get; set; }
			public bool JH_RatingHasBeenRun { get; set; }
			public DateTime? JH_RevenueRecognizedDate { get; set; }
			public bool JH_SingleAgentsInvoicePerConsol { get; set; }
			public string JH_Status { get; set; }
			public bool JH_IsDisbursement { get; set; }
			public DateTime? JH_SystemCreateTimeUtc { get; set; }
			public string JH_SystemCreateUser { get; set; }
			public DateTime? JH_SystemLastEditTimeUtc { get; set; }
			public string JH_SystemLastEditUser { get; set; }
			public string JH_TH_NKQuoteNumber { get; set; }
			public short JH_UniqueJobInvoiceNumber { get; set; }
			public Guid? JH_GB_TaxBranch { get; set; }
			public string ParentTableCode { get; set; }
			public IGlbStaffInfo RepOps { get; set; }
			public IGlbStaffInfo RepSales { get; set; }
			public IGlbStaffInfo CreatedByStaff { get; set; }
			public IGlbStaffInfo LastEditedByStaff { get; set; }
			public IGlbBranchInfo GlbBranch { get; set; }
			public IGlbCompanyInfo GlbCompany { get; set; }
			public IGlbDepartmentInfo GlbDepartment { get; set; }
			public IJobHeader ParentJob { get; set; }
			public IOrgAddressInfo AgentCollectAddr { get; set; }
			public IOrgAddressInfo LocalChargesAddr { get; set; }
			public IOrgContactInfo LocalBillingContact { get; set; }

			public ICollection<JobChargeModel> JobCharges { get; set; }

			ICollection<IJobCharge> IJobHeader.JobCharges
			{
				get
				{
					return JobCharges.ToList<IJobCharge>();
				}
			}

			public ICollection<IJobExRate> JobExRates { get; }

			public ICollection<ILog<IJobHeader>> Logs { get; }

			public ICollection<IProcessTask<IJobHeader>> ProcessTasks { get; }

			public ICollection<IWorkflowAuditLog<IJobHeader>> WorkflowAuditLogs { get; }

			public ICollection<IWorkflowEvent<IJobHeader>> WorkflowEvents { get; }

			public ICollection<IWorkflowException<IJobHeader>> WorkflowExceptions { get; }

			public ICollection<IAcknowledgement<IJobHeader>> Acknowledgements { get; }

			ICollection<INote<IJobHeader>> INoteProvider<IJobHeader>.Notes => null;

			public string EntityState { get; set; }

			public IGlbBranchInfo TaxBranch { get; set; }
		}

		public class JobChargeModel : IJobCharge
		{
			public Guid JR_PK { get; set; }

			public Guid? JR_A9_CostVATClass { get; set; }
			public Guid? JR_A9_SellVATClass { get; set; }
			public Guid? JR_AB { get; set; }
			public Guid JR_AC { get; set; }
			public decimal JR_AgentDeclaredCostAmt { get; set; }
			public decimal JR_AgentDeclaredSellAmt { get; set; }
			public Guid? JR_AK { get; set; }
			public Guid? JR_AL_APLine { get; set; }
			public Guid? JR_AL_ARLine { get; set; }
			public Guid? JR_AL_CFXLine { get; set; }
			public DateTime? JR_APInvoiceDate { get; set; }
			public string JR_APInvoiceNum { get; set; }
			public string JR_APLinePostingStatus { get; set; }
			public byte JR_APNumberOfSupportingDocuments { get; set; }
			public string JR_ARLinePostingStatus { get; set; }
			public byte JR_ARNumberOfSupportingDocuments { get; set; }
			public Guid? JR_AT_CostGSTRate { get; set; }
			public Guid? JR_AT_SellGSTRate { get; set; }
			public Guid? JR_AW_CostWHTRate { get; set; }
			public Guid? JR_AW_SellWHTRate { get; set; }
			public Guid? JR_CAL_APLine { get; set; }
			public Guid? JR_CAL_ARLine { get; set; }
			public string JR_ChargeType { get; set; }
			public string JR_ChequeNo { get; set; }
			public string JR_CostGovtChargeCode { get; set; }
			public bool JR_CostRated { get; set; }
			public bool JR_CostRatingOverride { get; set; }
			public string JR_CostRatingOverrideComment { get; set; }
			public string JR_CostReference { get; set; }
			public DateTime? JR_CostTaxDate { get; set; }
			public decimal JR_DeclaredOSCostAmt { get; set; }
			public string JR_Desc { get; set; }
			public short JR_DisplaySequence { get; set; }
			public Guid? JR_E6 { get; set; }
			public Guid? JR_E6_GatewaySellHeader { get; set; }
			public decimal JR_EstimatedCost { get; set; }
			public decimal JR_EstimatedRevenue { get; set; }
			public Guid JR_GB { get; set; }
			public Guid? JR_GB_InternalBranch { get; set; }
			public Guid JR_GC { get; set; }
			public Guid JR_GE { get; set; }
			public Guid? JR_GE_InternalDept { get; set; }
			public string JR_InvoiceType { get; set; }
			public bool JR_IsCostTaxAmountOverridden { get; set; }
			public bool JR_IsIncludedInProfitShare { get; set; }
			public bool JR_IsValid { get; set; }
			public bool? JR_IsAPCashAdvance { get; set; }
			public bool? JR_IsARCashAdvance { get; set; }
			public bool? JR_IsSpotCost { get; set; }
			public Guid JR_JH { get; set; }
			public Guid? JR_JH_InternalJob { get; set; }
			public Guid? JR_JR_RevenueLine { get; set; }
			public decimal JR_LineCFX { get; set; }
			public string JR_LineType { get; set; }
			public decimal JR_LocalCostAmt { get; set; }
			public decimal JR_LocalSellAmt { get; set; }
			public decimal JR_MarginPercentage { get; set; }
			public Guid? JR_OA_SellInvoiceAddress { get; set; }
			public Guid? JR_OC_SellInvoiceContact { get; set; }
			public Guid? JR_OH_CostAccount { get; set; }
			public Guid? JR_OH_SellAccount { get; set; }
			public Guid? JR_OP_Product { get; set; }
			public string JR_OrderReference { get; set; }
			public decimal JR_OSCostAmt { get; set; }
			public decimal JR_OSCostExRate { get; set; }
			public decimal JR_OSCostGSTAmt { get; set; }
			public decimal JR_OSCostWHTAmt { get; set; }
			public decimal JR_OSSellAmt { get; set; }
			public decimal JR_OSSellExRate { get; set; }
			public decimal JR_OSSellWHTAmt { get; set; }
			public DateTime? JR_PaymentDate { get; set; }
			public string JR_PaymentType { get; set; }
			public bool JR_PreventInvoicePrintGrouping { get; set; }
			public decimal JR_ProductQuantity { get; set; }
			public bool JR_ProFormaCost { get; set; }
			public bool JR_ProFormaRevenue { get; set; }
			public string JR_RX_NKCostCurrency { get; set; }
			public string JR_RX_NKSellCurrency { get; set; }
			public string JR_RX_NKSellInvoiceCurrency { get; set; }
			public string JR_SellGovtChargeCode { get; set; }
			public bool JR_SellRated { get; set; }
			public bool JR_SellRatingOverride { get; set; }
			public string JR_SellRatingOverrideComment { get; set; }
			public string JR_SellReference { get; set; }
			public DateTime? JR_SellTaxDate { get; set; }
			public string JR_CostPlaceOfSupply { get; set; }
			public string JR_CostPlaceOfSupplyType { get; set; }
			public string JR_SellPlaceOfSupply { get; set; }
			public string JR_SellPlaceOfSupplyType { get; set; }
			public DateTime? JR_SystemCreateTimeUtc { get; set; }
			public string JR_SystemCreateUser { get; set; }
			public DateTime? JR_SystemLastEditTimeUtc { get; set; }
			public string JR_SystemLastEditUser { get; set; }
			public string CostRatingBehaviour { get; set; }
			public string SellRatingBehaviour { get; set; }
			public DateTime? JR_APDocumentReceivedDate { get; set; }
			public IRefCurrencyInfo CostCurrency { get; set; }
			public IRefCurrencyInfo SellCurrency { get; set; }
			public IRefCurrencyInfo SellInvoiceCurrency { get; set; }
			public IAccInvMsgInfo CostVATClass { get; set; }
			public IAccInvMsgInfo SellVATClass { get; set; }
			public IAccBankAccountInfo AccBankAccount { get; set; }
			public IAccChargeCodeInfo AccChargeCode { get; set; }
			public IAccChequeBookInfo AccChequeBook { get; set; }
			public IAccTransactionLineInfo APLine { get; set; }
			public IAccTransactionLineInfo ARLine { get; set; }
			public IAccTransactionLineInfo CFXLine { get; set; }
			public IAccTaxRateInfo CostGSTRate { get; set; }
			public IAccTaxRateInfo SellGSTRate { get; set; }
			public IJobConsolCost JobConsolCost { get; set; }
			public IJobConsolCost GatewaySellHeader { get; set; }
			public IGlbBranchInfo GlbBranch { get; set; }
			public IGlbBranchInfo InternalBranch { get; set; }
			public IGlbCompanyInfo GlbCompany { get; set; }
			public IGlbDepartmentInfo GlbDepartment { get; set; }
			public IGlbDepartmentInfo InternalDept { get; set; }
			public IGlbStaffInfo CreatedByStaff { get; set; }
			public IGlbStaffInfo LastEditedByStaff { get; set; }
			public IJobHeader JobHeader { get; set; }
			public IJobHeader InternalJob { get; set; }
			public IJobCharge RevenueLine { get; set; }
			public IOrgAddressInfo SellInvoiceAddress { get; set; }
			public IOrgContactInfo SellInvoiceContact { get; set; }
			public IOrgHeaderInfo CostAccount { get; set; }
			public IOrgHeaderInfo SellAccount { get; set; }
			public IOrgSupplierPartInfo Product { get; set; }
			public IGlbBranchInfo CostTaxBranch { get; set; }
			public IGlbBranchInfo SellTaxBranch { get; set; }

			public ICollection<IAcknowledgement<IJobCharge>> Acknowledgements { get; }
			public string EntityState { get; set; }
			public string JR_InvoiceType_Old { get; set; }
			public string JR_CostReference_Old { get; set; }
			public string JR_SellReference_Old { get; set; }
			public string JR_SellGovtChargeCode_Old { get; set; }
			public string JR_CostGovtChargeCode_Old { get; set; }
			public string JR_Desc_Old { get; set; }
			public string JR_ApInvoiceNum_Old { get; set; }
			public string JR_CostSupplyType { get; set; }
			public string JR_SellSupplyType { get; set; }
			public Guid? JR_GB_CostTaxBranch { get; set; }
			public Guid? JR_GB_SellTaxBranch { get; set; }
		}
	}
}
