using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[CodeProperty(JobShipmentPreplanning.Schema.EF_PreshipID)]
	[DescriptionProperty(JobShipmentPreplanning.Schema.EF_PreshipID)]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	[UniversalCopyAssociateElement("JobShipment", "Shipment")]
	public class JobShipmentPreplanning :
		AutoJobShipmentPreplanning,
		Integration.Forwarding.IJobShipmentPreplanning,
		IJobNumber,
		IRoutingSupport,
		IWorkflowProvider,
		ITransportParent,
		IDocManagerSupport,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn
	{
		public JobShipmentPreplanning(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EF_UnitOfWeight = Env.Registry.FreightWeightUnit;
			EF_UnitOfVolume = Env.Registry.FreightVolumeUnit;
			EF_F3_NKPackType = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (Orders.Count > 0 || EF_JE.IsValid || EF_JS.IsValid)
			{
				throw new CannotDeleteException("You cannot delete a Shipment Pre Advice if it has attached orders or shipment/declaration information.");
			}
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Related Business Objects

		#region Orders

		[ChildEditable(true)]
		public OrderCollection Orders
		{
			get
			{
				if (orders == null)
				{
					orders = new OrderCollection(this, EF_IsCancelled ? new ZQuery() { IgnoreActiveFilter = true } : null);
					RegisterEditableChildObject(orders);
				}

				return orders;
			}
		}
		OrderCollection orders;

		public void AppendContainersFromOrder(Order order)
		{
			AppendContainersFromOrderCore(order);
		}

		protected virtual void AppendContainersFromOrderCore(Order order)
		{
			foreach (OrderContainer containerToCopy in order.PlannedContainers)
			{
				if (containerToCopy.J1_ContainerNumber.IsEmpty
					|| !Containers.Cast<OrderContainer>().Any(x => x.J1_ContainerNumber == containerToCopy.J1_ContainerNumber))
				{
					OrderContainer currentContainer = Containers.AddNew();
					currentContainer.J1_ContainerNumber = containerToCopy.J1_ContainerNumber;
					currentContainer.J1_ContainerCount = containerToCopy.J1_ContainerCount;
					currentContainer.J1_RC = containerToCopy.J1_RC;
					currentContainer.J1_SealNum = containerToCopy.J1_SealNum;
					currentContainer.J1_AdditionalSealNum = containerToCopy.J1_AdditionalSealNum;
					currentContainer.J1_Additional2SealNum = containerToCopy.J1_Additional2SealNum;
				}
			}
		}

		#endregion

		#region Order Lines

		[ChildEditable(true)]
		public OrderDeliveryLineCollection OrderLines
		{
			get
			{
				if (fOrderLines == null)
				{
					fOrderLines = new OrderDeliveryLineCollection(this);
					fOrderLines.Load();
					RegisterEditableChildObject(fOrderLines);
				}
				return fOrderLines;
			}
		}

		OrderDeliveryLineCollection fOrderLines;

		#endregion

		#region Order Numbers

		public ZBoolDescriptionPairList OrderNumberList
		{
			get
			{
				if (fOrderNumberList == null)
				{
					fOrderNumberList = new ZBoolDescriptionPairList();
					fOrderNumberList.OnPairChanged += new ZBoolDescriptionPairChangedEventHandler(OrderNumberList_OnPairChanged);
					Orders.CountChanged += delegate
					{ RefreshOrderNumbersList(); };
					RefreshOrderNumbersList();
				}
				return fOrderNumberList;
			}
		}

		void RefreshOrderNumbersList()
		{
			fOrderNumberList.Clear();
			foreach (Order order in Orders)
			{
				fOrderNumberList.Add(new ZBoolDescriptionPair(order.PK, order.JD_OrderNumberAndSplit, false));
			}
		}

		ZBoolDescriptionPairList fOrderNumberList;

		void OrderNumberList_OnPairChanged(ZBoolDescriptionPairChangedEventArgs e)
		{
			OrderLines.RemoveAll();
			OrderLines.Load();
			OrderLines.SortByOrderNumberAndLineNumber();
		}

		#endregion

		#region Containers

		[ChildEditable(true)]
		public PreAdviceOrderContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new PreAdviceOrderContainerCollection(this);
					fContainers.Load();
					RegisterEditableChildObject(fContainers);
				}

				return fContainers;
			}
		}

		PreAdviceOrderContainerCollection fContainers;

		#endregion

		#region Shipments

		public ForwardingShipment Shipment
		{
			get { return Factory.Load<ForwardingShipment>(EF_JS); }
		}

		IEnumerable<ForwardingShipment> GetOrderShipments()
		{
			return Orders.Where(order => order.Shipment != null).Select(order => order.Shipment).Distinct();
		}

		#endregion

		#region Declaration

		public BusinessObject Declaration
		{
			get { return (BusinessObject)Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(EF_JE); }
		}

		IEnumerable<BusinessObject> GetOrderDeclarations()
		{
			return Orders.Where(order => order.Declaration != null).Select(order => order.Declaration).Distinct();
		}

		#endregion

		#region Transports

		[ChildEditable(false)]
		public PreAdviceTransportCollection PreAdviceTransports
		{
			get
			{
				if (fTransports == null)
				{
					using (SuspendSettingHasChanges())
					using (GetValidationSuspender())
					{
						fTransports = new PreAdviceTransportCollection(this);
						fTransports.Load();

						if (fTransports.Count == 0)
						{
							Transport firstTransport = fTransports.AddNew();
							using (firstTransport.SuspendSettingHasChanges())
							using (firstTransport.GetValidationSuspender())
							{
								firstTransport.JW_IsLinked = true;
							}
						}

						RegisterEditableChildObject(fTransports);
					}
				}

				return fTransports;
			}
		}

		PreAdviceTransportCollection fTransports;

		#endregion

		#endregion

		#region Saving

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			SetPreshipIDIfNeeded();
			if (Shipment != null)
			{
				CopyDEXEventToShipment();
			}
		}

		public ZString LogReference(bool checkIsInDatabase)
		{
			return checkIsInDatabase && !IsInDatabase ? (ZString)PK.ToString() : EF_PreshipID;
		}

		public void SetPreshipIDIfNeeded()
		{
			if (EF_PreshipID.IsEmpty)
			{
				EF_PreshipID = Env.NumberFountains.ShipmentPreplanningNumbers.GetNextFormatted(Factory);
			}
		}

		void CopyDEXEventToShipment()
		{
			StmALog log = Shipment.Logs.MostRecentLogByEventTime(Events.DataExport);
			if (log == null)
			{
				log = Logs.MostRecentLogByEventTime(Events.DataExport);
				if (log != null)
				{
					Shipment.Logs.AddNew(Events.DataExport, log.SL_Reference, log.SL_EventTimeOffset);
				}
			}
		}

		protected override void OnFactorySaving()
		{
			foreach (Order order in Orders)
			{
				SetValuesOnOrder(order);
			}
			base.OnFactorySaving();
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IImportExport Members

		public Directions JobDirection
		{
			get { return ImportExportHelper.GetJobDirection(EF_RL_NKPortLoad, EF_RL_NKPortDisch); }
		}

		#endregion

		#region Properties

		[List("Lookups.Carriers")]
		public override ZGuid EF_OH_Carrier
		{
			get { return base.EF_OH_Carrier; }
			set { base.EF_OH_Carrier = value; }
		}

		[List("Lookups.Buyers")]
		public override ZGuid EF_OA_BuyerAddress
		{
			get { return base.EF_OA_BuyerAddress; }
			set { base.EF_OA_BuyerAddress = value; }
		}

		[List("Lookups.Shipments")]
		public override ZGuid EF_JS
		{
			get { return base.EF_JS; }
			set
			{
				if (!value.IsEmpty && !EF_JE.IsEmpty && !IsCopying)
				{
					EF_JE = ZGuid.Empty;
				}
				base.EF_JS = value;
			}
		}

		protected bool EF_JS_ReadOnly
		{
			get { return ShipmentDeclarationReadOnly; }
		}

		[List("Lookups.Declarations")]
		public override ZGuid EF_JE
		{
			get { return base.EF_JE; }
			set
			{
				if (!value.IsEmpty && !EF_JS.IsEmpty)
				{
					EF_JS = ZGuid.Empty;
				}

				var declaration = (BusinessObject)Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(value);
				((IBusinessObjectInternals)this).IsCopying = true;
				try
				{
					EF_JS = (declaration == null) ? ZGuid.Empty : (ZGuid)declaration[JobDeclarationSchema.JE_JS.Name];
				}
				finally
				{
					((IBusinessObjectInternals)this).IsCopying = false;
				}

				base.EF_JE = value;
			}
		}

		protected bool EF_JE_ReadOnly
		{
			get { return ShipmentDeclarationReadOnly; }
		}

		bool ShipmentDeclarationReadOnly
		{
			get { return GetOrderShipments().Any() || GetOrderDeclarations().Any(); }
		}

		[MeasureUnit(Schema.EF_UnitOfVolume, MeasureUnitType.Volume)]
		public override ZDecimal EF_ActualVolume
		{
			get { return base.EF_ActualVolume; }
			set { base.EF_ActualVolume = this.GetRoundedValue(JobShipmentPreplanningSchema.EF_ActualVolume, EF_ActualVolumeInfo, value); }
		}

		[MeasureUnit(Schema.EF_UnitOfWeight, MeasureUnitType.Weight)]
		public override ZDecimal EF_ActualWeight
		{
			get { return base.EF_ActualWeight; }
			set { base.EF_ActualWeight = this.GetRoundedValue(JobShipmentPreplanningSchema.EF_ActualWeight, EF_ActualWeightInfo, value); }
		}

		[List("Lookups.UnitWeightList")]
		public override ZString EF_UnitOfWeight
		{
			get { return base.EF_UnitOfWeight; }
			set
			{
				base.EF_UnitOfWeight = value;
				this.SetRoundedValue(JobShipmentPreplanningSchema.EF_ActualWeight, EF_ActualWeightInfo);
			}
		}

		[List("Lookups.UnitVolumeList")]
		public override ZString EF_UnitOfVolume
		{
			get { return base.EF_UnitOfVolume; }
			set
			{
				base.EF_UnitOfVolume = value;
				this.SetRoundedValue(JobShipmentPreplanningSchema.EF_ActualVolume, EF_ActualVolumeInfo);
			}
		}

		[List("Lookups.UnitPackList")]
		public override ZString EF_F3_NKPackType
		{
			get { return base.EF_F3_NKPackType; }
			set { base.EF_F3_NKPackType = value; }
		}

		public ZString PreAdviceLockedMessage
		{
			get
			{
				ZString result = "";
				if (IsExistingOperationsJobLinked)
				{
					string messageParameter;
					if (Shipment != null)
					{
						messageParameter = Shipment.HumanReadableName;
					}
					else if (Declaration != null)
					{
						messageParameter = Declaration.HumanReadableName;
					}
					else
					{
						messageParameter = Res.GetString("422dfaa2-d801-45f3-8ca0-cbe1938b7a17", "the shipments attached to the respective orders");
					}

					result = Res.GetString("054f2235-d441-4849-a2fe-c3272f6da739", "This Shipment Pre Advice is linked to an operations job. Up to date data and modifications should occur on {0}.", messageParameter);
				}

				return result;
			}
		}

		bool IsExistingOperationsJobLinked
		{
			get { return (EF_JS.IsValid || EF_JE.IsValid || GetOrderShipments().Any() || GetOrderDeclarations().Any()) && !HasChanges; }
		}

		public ZPropertyInfo PreAdviceLockedMessageInfo
		{
			get { return GetZPropertyInfo(nameof(PreAdviceLockedMessage)); }
		}

		public ZBool IncludeInExportToForwardingJob
		{
			get { return fIncludeInExportToForwardingJob; }
			set { SetNonPersistentPropertyValue(IncludeInExportToForwardingJobInfo, ref fIncludeInExportToForwardingJob, value); }
		}

		public ZPropertyInfo IncludeInExportToForwardingJobInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInExportToForwardingJob)); }
		}

		ZBool fIncludeInExportToForwardingJob = true;

		[List("Lookups.PortLoads")]
		public override ZString EF_RL_NKPortLoad
		{
			get { return base.EF_RL_NKPortLoad; }
			set
			{
				base.EF_RL_NKPortLoad = value;
				foreach (Transport routing in PreAdviceTransports)
				{
					if (routing.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel || routing.JW_TransportMode.IsEmpty)
					{
						routing.JW_RL_NKLoadPort = EF_RL_NKPortLoad;
					}
				}
			}
		}

		[List("Lookups.PortDisches")]
		public override ZString EF_RL_NKPortDisch
		{
			get { return base.EF_RL_NKPortDisch; }
			set
			{
				base.EF_RL_NKPortDisch = value;
				foreach (Transport routing in PreAdviceTransports)
				{
					if (routing.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel || routing.JW_TransportMode.IsEmpty)
					{
						routing.JW_RL_NKDiscPort = EF_RL_NKPortDisch;
					}
				}
			}
		}

		#region TransportMode

		public ZString MainTransportMode => MainTransport != null ? MainTransport.JW_TransportMode : ZString.Empty;

		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("974ed169-4b4a-4d25-b938-7cc01f66c52a", "Shipment Pre Advice {0}", EF_PreshipID); }
		}

		#endregion

		#region EF_PreshipID

		[NotDefaultingPropertyValue]
		[ReadOnly(true)]
		public override ZString EF_PreshipID
		{
			get { return base.EF_PreshipID; }
			set { base.EF_PreshipID = value; }
		}

		#endregion

		#region EF_IsCancelled

		public override ZBool EF_IsCancelled
		{
			get { return base.EF_IsCancelled; }
			set
			{
				if (value)
				{
					foreach (Order order in Orders.ToArray())
					{
						order.JD_IsCancelled = true;
					}
				}
				orders = null;
				base.EF_IsCancelled = value;
			}
		}

		#endregion

		#region BuyerZAddressWithContact

		public ZAddressWithContact BuyerZAddressWithContact => buyerZAddressWithContact ?? (buyerZAddressWithContact = GetBuyerZAddressWithContact());

		ZAddressWithContact buyerZAddressWithContact;

		ZAddressWithContact GetBuyerZAddressWithContact()
		{
			var result = new ZAddressWithContact(EF_OC_BuyerContactInfo, EF_OA_BuyerAddressInfo);
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = GetDefaultBuyerAddress;
			return result;
		}

		static ZGuid GetDefaultBuyerAddress(IOrgHeader org)
		{
			OrgAddress result = null;
			var header = org as OrgHeader;
			if (header != null && header.AddressesActive.Any())
			{
				var finder = new OrgAddressDefaultFinder(IBusinessObjectCollectionExtensions.ToArray<OrgAddress>(header.AddressesActive), true, null);
				result = finder.DefaultAddressOfType(OrgAddressType.Office);
			}

			return result == null ? ZGuid.Empty : result.PK;
		}

		#endregion

		#region BuyerPK

		[List("Lookups.Buyers")]
		public virtual ZGuid BuyerPK
		{
			get { return !IsDeleted && BuyerAddress != null ? BuyerAddress.OA_OH : ZGuid.Empty; }
			set
			{
				var org = Factory.Load<OrgHeader>(value);
				EF_OA_BuyerAddress = GetDefaultBuyerAddress(org);
				BuyerPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BuyerPKInfo
		{
			get { return GetZPropertyInfo(nameof(BuyerPK)); }
		}

		public OrgHeader Buyer
		{
			get { return Factory.Load<OrgHeader>(BuyerPK); }
		}

		#endregion

		#endregion

		#region Create Forwarding Jobs

		#region Matching Pre Advices

		public JobShipmentPreplanningCollection MatchingPreAdvices
		{
			get
			{
				JobShipmentPreplanningCollection lMatchingPreAdvices;
				if (fMatchingPreAdvices == null)
				{
					lMatchingPreAdvices = new JobShipmentPreplanningCollection(Factory);
				}
				else
				{
					lMatchingPreAdvices = fMatchingPreAdvices;
				}

				ZQuery query = new ZQuery(JobShipmentPreplanningSchema.PK, PK);
				query.AddToFilter(UnLinkedTransportQuery, JoinCondition.Or);
				query.AddToFilter(LinkedTransportQuery, JoinCondition.Or);
				lMatchingPreAdvices.Load(query);
				fMatchingPreAdvices = lMatchingPreAdvices;
				return fMatchingPreAdvices;
			}
		}

		JobShipmentPreplanningCollection fMatchingPreAdvices;

		ZQuery LinkedTransportQuery
		{
			get
			{
				ZDBOnlyQuery linkedQuery = new ZDBOnlyQuery(typeof(JobShipmentPreplanning));
				if (!EF_MasterBill.IsEmpty)
				{
					linkedQuery.AddToFilter(JobShipmentPreplanningSchema.EF_MasterBill, EF_MasterBill);

					if (MainTransport != null)
					{
						ZDBOnlySubQuery transportLinkedQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
						ZDBOnlySubQuery sailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);
						ZDBOnlySubQuery jobVoyOriginQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
						ZDBOnlySubQuery voyageQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);

						voyageQuery.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, MainTransport.JW_Vessel);
						voyageQuery.AddToFilter(JobVoyageSchema.JV_VoyageFlight, MainTransport.JW_VoyageFlight);
						jobVoyOriginQuery.AddSubQuery(voyageQuery, JoinCondition.And);
						sailingQuery.AddSubQuery(jobVoyOriginQuery, JoinCondition.And);
						transportLinkedQuery.AddSubQuery(sailingQuery, JoinCondition.And);

						linkedQuery.AddSubQuery(transportLinkedQuery, JoinCondition.And);
					}
				}
				else
				{
					linkedQuery.IsNoResultQuery = true;
				}

				return linkedQuery;
			}
		}

		ZQuery UnLinkedTransportQuery
		{
			get
			{
				ZDBOnlyQuery unLinkedQuery = new ZDBOnlyQuery(typeof(JobShipmentPreplanning));
				if (!EF_MasterBill.IsEmpty)
				{
					unLinkedQuery.AddToFilter(JobShipmentPreplanningSchema.EF_MasterBill, EF_MasterBill);

					if (MainTransport != null)
					{
						ZDBOnlySubQuery transportUnLinkedQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
						transportUnLinkedQuery.AddToFilter(JobConsolTransportSchema.JW_Vessel, MainTransport.JW_Vessel);
						transportUnLinkedQuery.AddToFilter(JobConsolTransportSchema.JW_VoyageFlight, MainTransport.JW_VoyageFlight);
						unLinkedQuery.AddSubQuery(transportUnLinkedQuery, JoinCondition.And);
					}
				}
				else
				{
					unLinkedQuery.IsNoResultQuery = true;
				}

				return unLinkedQuery;
			}
		}

		#endregion

		#region Create Shipment

		public ForwardingConsol CreateConsolAndShipment(OrderShipmentCreationMode creationMode, INotifications notification,
			Func<ForwardingConsol, bool> tryFixConsolErrorsAndSave = null)
		{
			return CreateShipmentAndAttachToConsol(creationMode, notification, null, tryFixConsolErrorsAndSave);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public ForwardingConsol CreateShipmentAndAttachToConsol(OrderShipmentCreationMode creationMode, INotifications notification,
			ForwardingConsol consol, Func<ForwardingConsol, bool> tryFixConsolErrorsAndSave = null)
		{
			if (!CheckPreAdvice(notification))
			{
				return null;
			}

			var continueProcessing = QueryUserIfMultipleJobsExist(notification, consol);
			if (continueProcessing)
			{
				var error = CheckValidContainersAndPreEstimateContainersGrossWeights();
				if (!string.IsNullOrEmpty(error))
				{
					notification.Notify(new InfoNotification(error));
					return null;
				}

				var consolCreated = false;
				if (consol == null)
				{
					var factoryForConsol = new BusinessObjectFactory();
					consol = CreateConsolFromPreAdvice(factoryForConsol);
					consolCreated = true;
				}

				PopulateConsol(consol, creationMode, notification);

				consol.RunPreSaveValidation();

				string GetSuccessMessage()
				{
					return consolCreated
						? Res.GetString("cb5a7144-fbd8-4b24-a601-b87b1b2871dc", "{0} and shipment(s) have been successfully created from this pre-advice.", consol.HumanReadableName)
						: Res.GetString("2f591060-a3c7-42e5-a9a5-ef3cd42a20d3", "Shipment(s) have been successfully created from this pre-advice and attached to {0}", consol.HumanReadableName);
				}

				if (consol.HasErrors)
				{
					if (tryFixConsolErrorsAndSave != null)
					{
						if (tryFixConsolErrorsAndSave.Invoke(consol))
						{
							notification.Notify(new InfoNotification(GetSuccessMessage()));
						}
						else
						{
							notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("89213914-9f5a-47d9-8e4d-1b122580ebf6", "The consol was not saved by the user.")));
						}
					}
					else
					{
						notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("4ffc0511-cf4e-43f7-96d0-d19ee5f192e8", "Consol could not be saved because of validation errors.")));
					}
				}
				else
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(consol.Factory.Save, null, true);
					notification.Notify(new InfoNotification(GetSuccessMessage()));
				}

				RefreshBinding();
			}

			return consol;
		}

		bool CheckPreAdvice(INotifications notification)
		{
			if (Orders.Count == 0)
			{
				notification.Notify(new InfoNotification(Res.GetString("9fa58640-1059-4861-8766-b40651bac400", "This Pre Advice has no orders attached.")));
				return false;
			}

			if (!IsInDatabase || HasChanges)
			{
				notification.Notify(new InfoNotification(Res.GetString("6629cf01-8156-47e7-aefb-0e4d6d05ba5e", "Please save your changes before performing any Actions on this Pre Advice.")));
				return false;
			}

			if (IsExistingOperationsJobLinked)
			{
				notification.Notify(new InfoNotification(Res.GetString("15456635-0d1f-40ca-99ef-6ba841a7960b", "This Pre Advice has already been linked to an operations job and cannot be linked again.")));
				return false;
			}

			return true;
		}

		void PopulateConsol(ForwardingConsol consol, OrderShipmentCreationMode creationMode,
			INotifications notification)
		{
			AppendContainersToConsol(consol);
			AppendTransportsToConsol(consol);
			AppendShipmentsToConsol(consol, creationMode, notification);
			AppendEvents(consol);
		}

		ZString CheckValidContainersAndPreEstimateContainersGrossWeights()
		{
			var containers = Containers.Cast<OrderContainer>()
				.Distinct(new LambdaComparer<OrderContainer>((a, b) => a.J1_ContainerNumber == b.J1_ContainerNumber, a => a.J1_ContainerNumber.GetHashCode())).ToArray();
			var orderLines = Orders.SelectMany(x => x.OrderLines).ToArray();

			var invalidOrderLines = GetOrderLinesErrorMesseage(orderLines, containers);
			if (!invalidOrderLines.IsEmpty)
			{
				return invalidOrderLines;
			}

			var preAdviceContainers = containers.ToDictionary(x => x.J1_ContainerNumber, x => x.Container != null ? x.Container.RC_TareWeight : 0);

			if (!preAdviceContainers.ContainsKey(ZString.Empty))
			{
				preAdviceContainers.Add(ZString.Empty, 0);
			}

			return GetContainerWeightErrorMessage(orderLines, preAdviceContainers);
		}

		ZString GetOrderLinesErrorMesseage(OrderLine[] orderLines, OrderContainer[] containers)
		{
			var errors = new ZStringBuilder();
			var invalidOrderLines = orderLines.Where(ol => !ol.JO_ContainerNumber.IsEmpty && containers.All(c => c.J1_ContainerNumber != ol.JO_ContainerNumber)).ToArray();
			if (invalidOrderLines.Any())
			{
				errors.Append(Res.GetString("A64BE429-C5FB-4B7C-9715-24B26B3CE590", "Invalid containers found in the following order lines:"));
				foreach (var orderLine in invalidOrderLines)
				{
					errors.Append(Res.GetString("332DEF58-1145-436D-B006-709CCE2BE08C", "container {0} in order line {1}", orderLine.JO_ContainerNumber, orderLine.JO_LineNo));
				}
				return errors.ToStringWithNewLineBetweenAppends();
			}
			return ZString.Empty;
		}

		ZString GetContainerWeightErrorMessage(OrderLine[] orderLines, Dictionary<ZString, ZDecimal> preAdviceContainers)
		{
			var errors = new ZStringBuilder();
			var containerWeights = orderLines.GroupBy(x => x.JO_ContainerNumber)
				.Select(x => new
				{
					ContainersNumber = x.Key,
					TareWeight = preAdviceContainers[x.Key],
					GrossWeight = (ZDecimal)(preAdviceContainers[x.Key] + x.Sum(i => GetLineWeightInKGSafe(i)))
				});

			foreach (var containerWeight in containerWeights)
			{
				if (!containerWeight.GrossWeight.IsWithinSqlPrecisionAndScale(9, 3))
				{
					ZDecimal maxAvailableValue = 999999m - containerWeight.TareWeight;
					if (maxAvailableValue > 0)
					{
						errors.Append(Res.GetString("67212316-3dd8-4456-9154-3b9b71d2683e",
							"Total actual weight of container {0} is too large. Maximum actual weight for this container is {1} KG.",
							containerWeight.ContainersNumber, maxAvailableValue));
					}
				}
			}

			return errors.ToStringWithNewLineBetweenAppends();
		}

		ZDecimal GetLineWeightInKGSafe(OrderLine line)
		{
			return (line.JO_UnitOfWeight.IsEmpty || Core.Constants.Weight.ContainsCode(line.JO_UnitOfWeight))
				? Core.Constants.Weight.Convert(line.JO_ActualWeight, line.JO_UnitOfWeight, Core.Constants.Weight.Kilograms, false)
				: 0.0m;
		}

		#endregion

		#region Create Shipment Implementation

		public event EventHandler<EnterShipmentNumbersEventArgs> OnAfterShipmentsCreated;
		public event EventHandler<OrderLineToPackLineConversionEventArgs> OnOrderLineToPackLineConversion;

		void AppendEvents(BusinessObject bo)
		{
			foreach (StmALog log in Logs.GetAllLogs())
			{
				if (log.SL_SE_NKEvent == Events.AllImportDocumentsReceived.Code)
				{
					bo.GetLogs().AddNew(Events.AllImportDocumentsReceived, log.SL_EventTimeOffset);
				}
			}
		}

		bool QueryUserIfMultipleJobsExist(INotifications notification, ForwardingConsol consol = null)
		{
			bool @continue = true;

			if (!EF_MasterBill.IsEmpty)
			{
				var query = new ZQuery(JobConsolSchema.JK_MasterBillNum, EF_MasterBill);

				if (consol != null)
				{
					query.AddToFilter(JobConsolSchema.JK_UniqueConsignRef, SQLComparisonOperator.NotEqual, consol.JK_UniqueConsignRef);
				}

				if (Factory.ExistsInDatabase(JobConsolSchema.Constants.TableName, query))
				{
					var args = consol == null
						? new QueryUserMsgBoxEventArgs(Res.GetString("5c0a99a2-469f-4694-bc20-f4fa48ac53af", "A consol already exists for this Master Bill. If you continue, a new consol and shipment(s) will be created.\r\n\r\nDo you want to continue?"), false)
						: new QueryUserMsgBoxEventArgs(Res.GetString("d400eb65-1fc4-407f-9db5-147ac6b677cd", "A different consol already exists for this Master Bill. If you continue, the shipment(s) will be created and attached to the selected consol - {0}.\r\n\r\nDo you want to continue?", consol.JK_UniqueConsignRef), false);
					notification.QueryUser(args);

					if (!args.Response)
					{
						@continue = false;
					}
				}
			}

			return @continue;
		}

		ForwardingConsol CreateConsolFromPreAdvice(BusinessObjectFactory factoryForConsol)
		{
			var consol = factoryForConsol.New<ForwardingConsol>();

			if (Orders.Count > 0 && Orders[0].JD_TransportMode == Core.Constants.TransportModes.Courier)
			{
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_AgentType = Core.Constants.AgentType.Courier;
			}
			else if (MainTransport != null)
			{
				consol.JK_TransportMode = MainTransport.JW_TransportMode;
				consol.JK_ConsolMode = GetConsolMode(consol);
			}

			consol.SetDefaultShippingLineAddress(EF_OH_Carrier);
			consol.JK_MasterBillNum = EF_MasterBill;
			consol.JK_RL_NKLoadPort = EF_RL_NKPortLoad;
			consol.JK_RL_NKDischargePort = EF_RL_NKPortDisch;
			consol.SetDefaultSendingForwarderAddress(EF_OH_SendingAgent);
			consol.SetDefaultReceivingForwarderAddress(EF_OH_ReceivingAgent);

			return consol;
		}

		ZString GetConsolMode(ForwardingConsol consol)
		{
			ZString result;

			if (Orders.Count == 1 && !Orders[0].JD_ContainerMode.IsEmpty)
			{
				result = GetConsolContainerMode(consol.JK_ConsolMode_List, Orders[0].JD_ContainerMode);
			}
			else if (Containers.Count > 0)
			{
				result = consol.IsSea ? Core.Constants.ContainerModes.FCL : Core.Constants.ContainerModes.ULD;
			}
			else if (Orders.Count > 0 && !Orders[0].JD_ContainerMode.IsEmpty)
			{
				result = GetConsolContainerMode(consol.JK_ConsolMode_List, Orders[0].JD_ContainerMode);
			}
			else
			{
				result = consol.IsSea ? Core.Constants.ContainerModes.LCL : Core.Constants.ContainerModes.Loose;
			}

			return result;
		}

		ZString GetConsolContainerMode(CodeDescriptionPairList availableContainerModes, ZString orderContainerMode)
		{
			ZString result = orderContainerMode != Core.Constants.ContainerModes.AgentConsol ? orderContainerMode : new ZString(Core.Constants.ContainerModes.Groupage);

			if (!availableContainerModes.ContainsCode(result))
			{
				result = Core.Constants.ContainerModes.Other;
			}

			return result;
		}

		void AppendContainersToConsol(ForwardingConsol consol)
		{
			if (Containers.Count > 0)
			{
				foreach (var orderContainer in Containers.OfType<OrderContainer>())
				{
					OrderContainerHelper.PopulateForwardingContainerFromOrderContainer(orderContainer, consol.Containers.AddNew());
				}
			}
		}

		void AppendTransportsToConsol(ForwardingConsol consol)
		{
			Transport existingTransport = consol.Transports[0];
			foreach (Transport transport in PreAdviceTransports)
			{
				Transport consolTransport = consol.Factory.New<Transport>();
				consolTransport.ParentType = typeof(ForwardingConsol);
				consolTransport.CopyPersistentValuesFrom(transport);
				if (transport.JW_TransportMode == Core.Constants.TransportModes.Courier)
				{
					consolTransport.JW_TransportMode = Core.Constants.TransportModes.Air;
				}
				consol.Transports.Add(consolTransport);
			}
			existingTransport.Delete();
		}

		void AddPackLinesFromOrderLines(ForwardingShipment shipment, IEnumerable<Order> orderIEnum)
		{
			var helper = new OrderLineToPackLineConversionHelper(Factory, shipment, orderIEnum);

			var args = new OrderLineToPackLineConversionEventArgs(helper, true);

			if (helper.OrderLines.Count > 1)
			{
				OnOrderLineToPackLineConversion?.Invoke(this, args);
			}

			if (args.ShouldCreatePacklines)
			{
				helper.CreatePackLines();
			}
		}

		public enum OrderShipmentCreationMode
		{
			/// <summary>
			/// Default - a single shipment is created per buyer/supplier pair
			/// </summary>
			ShipmentPerBuyerSupplier,

			/// <summary>
			/// A single shipment is created for all orders
			/// </summary>
			SingleShipment,

			/// <summary>
			/// A single shipment is created per order
			/// </summary>
			ShipmentPerOrder
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void AppendShipmentsToConsol(ForwardingConsol consol, OrderShipmentCreationMode creationMode, INotifications notification)
		{
			var uniqueOrders = GetUniqueOrders(consol, creationMode);

			var isBCN = uniqueOrders.Count > 1;
			if (isBCN)
			{
				consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			}

			foreach (var pair in uniqueOrders)
			{
				var shipment = consol.Shipments.AddNew();
				if (MainTransport != null)
				{
					shipment.JS_TransportMode = MainTransport.JW_TransportMode;
					if (shipment.Lookups.JS_PackingMode_List.ContainsCode(consol.JK_ConsolMode))
					{
						shipment.JS_PackingMode = consol.JK_ConsolMode;
					}
					else if (!shipment.Lookups.JS_PackingMode_List.ContainsCode(shipment.JS_PackingMode) && shipment.Lookups.JS_PackingMode_List.Count > 0)
					{
						shipment.JS_PackingMode = shipment.Lookups.JS_PackingMode_List[0].Code;
					}
				}

				shipment.JS_HouseBill = GetHouseBill(notification, !EF_HouseBill.IsEmpty ? EF_HouseBill : pair.Value[0].JD_Waybill, JobShipmentSchema.JS_HouseBill.MaxLength);

				var firstOrder = pair.Value[0];

				using (shipment.BuyerSupplierLinksHelper?.SuspendPortRestoration())
				{
					using (shipment.SuspendSettingDestFromConsigneeLocation())
					{
						shipment.ConsigneeDocumentaryAddress.OrganisationPK = firstOrder.BuyerPK;
						shipment.ConsigneeDocumentaryAddress.E2_OA_Address = firstOrder.JD_OA_BuyerAddress;
					}

					using (shipment.SuspendSettingOriginFromConsignorLocation())
					{
						shipment.ConsignorDocumentaryAddress.OrganisationPK = firstOrder.SupplierPK;
						shipment.ConsignorDocumentaryAddress.E2_OA_Address = firstOrder.JD_OA_SupplierAddress;
					}
				}

				if (firstOrder.NotifyPartyDocAddress != null && !firstOrder.NotifyPartyDocAddress.IsEmpty)
				{
					shipment.NotifyPartyDocumentaryAddress.CopyPersistentValuesFrom(firstOrder.NotifyPartyDocAddress);
				}

				if (firstOrder.NotifyParty2DocAddress != null && !firstOrder.NotifyParty2DocAddress.IsEmpty)
				{
					shipment.NotifyParty2DocumentaryAddress.CopyPersistentValuesFrom(firstOrder.NotifyParty2DocAddress);
				}

				if (firstOrder.NotifyParty3DocAddress != null && !firstOrder.NotifyParty3DocAddress.IsEmpty)
				{
					shipment.NotifyParty3DocumentaryAddress.CopyPersistentValuesFrom(firstOrder.NotifyParty3DocAddress);
				}

				if (firstOrder.BuyerContact != null)
				{
					shipment.ConsigneeDocumentaryAddress.E2_Contact = firstOrder.BuyerContact.OC_ContactName;
				}

				if (firstOrder.SupplierContact != null)
				{
					shipment.ConsignorDocumentaryAddress.E2_Contact = firstOrder.SupplierContact.OC_ContactName;
				}

				if (FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.Value)
				{
					shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
				}
				else if (shipment.Consignor != null)
				{
					if (!shipment.BuyerSupplierLinksHelper?.TryRestoreOriginPort() ?? true)
					{
						shipment.JS_RL_NKOrigin = shipment.Consignor.OH_RL_NKClosestPort;
					}
				}

				if (FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.Value)
				{
					shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
				}
				else if (shipment.Consignee != null)
				{
					if (!shipment.BuyerSupplierLinksHelper?.TryRestoreDestinationPort() ?? true)
					{
						shipment.JS_RL_NKDestination = shipment.Consignee.OH_RL_NKClosestPort;
					}
				}

				var pickupDate = GetPickupDate(pair.Value);
				if (!pickupDate.IsEmpty)
				{
					shipment.DocsAndCartage.JP_PickupRequiredBy = pickupDate;
				}

				var cloneArgs = new BusinessObjectCloneArgs(new[] { nameof(JobDocAddressSchema.E2_AddressType) });

				var deliveryPoint = GetDeliverPoint(pair.Value);
				if (deliveryPoint != null)
				{
					if (deliveryPoint is JobDocAddress deliveryPointAsJobDocAddress && !deliveryPointAsJobDocAddress.IsEmpty)
					{
						shipment.ConsigneeDeliveryAddress.CopyPersistentValuesFrom(deliveryPointAsJobDocAddress, cloneArgs);
					}
					else
					{
						shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryPoint.E2_OA_Address;
					}
				}

				var pickupPoint = GetPickupPoint(pair.Value);
				if (pickupPoint != null && !pickupPoint.IsEmpty)
				{
					shipment.ConsignorPickupAddress.CopyPersistentValuesFrom(pickupPoint, cloneArgs);
				}

				var deliveryDate = GetDeliveryDate(pair.Value);
				if (!deliveryDate.IsEmpty)
				{
					shipment.DocsAndCartage.JP_DeliveryRequiredBy = deliveryDate;
				}

				AddPackLinesFromOrderLines(shipment, pair.Value);

				if (!EF_ActualWeight.IsEmpty || !EF_ActualVolume.IsEmpty || !EF_Packs.IsEmpty)
				{
					shipment.JS_ActualWeight = EF_ActualWeight;
					shipment.JS_UnitOfWeight = EF_UnitOfWeight;
					shipment.JS_ActualVolume = EF_ActualVolume;
					shipment.JS_UnitOfVolume = EF_UnitOfVolume;
					shipment.JS_OuterPacks = EF_Packs;
					shipment.JS_F3_NKPackType = EF_F3_NKPackType;
				}
				else
				{
					shipment.JS_ActualWeight = pair.Value[0].JD_ActualWeight;
					shipment.JS_UnitOfWeight = pair.Value[0].JD_UnitOfWeight;
					shipment.JS_ActualVolume = pair.Value[0].JD_ActualVolume;
					shipment.JS_UnitOfVolume = pair.Value[0].JD_UnitOfVolume;
					shipment.JS_OuterPacks = pair.Value[0].JD_Packs;
					shipment.JS_F3_NKPackType = pair.Value[0].JD_F3_NKPackType;
				}

				shipment.JS_TotalPackageCount = pair.Value[0].JD_Calc_InnerPacks;
				shipment.JS_F3_NKTotalCountPackType = pair.Value[0].JD_Calc_InnerPackType;

				shipment.Validation.ValidateJS_ActualChargeable();
				shipment.SuppressShipmentNumberValidation = true;
				if (shipment.JS_ActualChargeableInfo.HasErrors())
				{
					shipment.JS_ActualChargeable = -1m;
				}

				ZDecimal goodsValue = 0;
				foreach (var order in pair.Value)
				{
					order.JD_JS = shipment.PK;
					foreach (var line in order.OrderLines)
					{
						goodsValue += line.JO_LinePrice;
					}
				}
				shipment.JS_GoodsValue = goodsValue;
				shipment.JS_INCO = pair.Value[0].JD_IncoTerm;
				shipment.JS_GoodsDescription = pair.Value[0].JD_OrderGoodsDescription;
				shipment.JS_RS_NKServiceLevel = pair.Value[0].JD_RS_NKServiceLevel_NI;

				BusinessObject dec = null;
				if (IsBuyersBrokerThisOrg(consol.JK_ConsolMode))
				{
					dec = CreateBrokerageWithShipment(shipment, pair.Value);
				}

				if (pair.Value[0].ControllingAgentDocAddress.IsValidAddress)
				{
					shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ControllingAgent).E2_OA_Address = pair.Value[0].ControllingAgentDocAddress.E2_OA_Address;
				}

				if (pair.Value[0].ControllingCustomerDocAddress.IsValidAddress)
				{
					shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ControllingCustomer).E2_OA_Address = pair.Value[0].ControllingCustomerDocAddress.E2_OA_Address;
				}

				if (uniqueOrders.Count == 1)
				{
					var preplanning = consol.Factory.Load<JobShipmentPreplanning>(PK);
					preplanning.EF_JS = shipment.PK;

					if (dec != null)
					{
						preplanning.EF_JE = dec.PK;
					}
				}

				AppendEvents(shipment);
			}

			if (consol.Shipments.Count > 0)
			{
				var nameShipmentsEventArg = new EnterShipmentNumbersEventArgs(consol);
				OnAfterShipmentsCreated?.Invoke(this, nameShipmentsEventArg);
			}
		}

		Dictionary<string, List<Order>> GetUniqueOrders(ForwardingConsol consol, OrderShipmentCreationMode creationMode)
		{
			var uniqueOrders = new Dictionary<string, List<Order>>();
			foreach (JobShipmentPreplanning preAdvice in MatchingPreAdvices)
			{
				if (preAdvice.IncludeInExportToForwardingJob)
				{
					foreach (var order in preAdvice.Orders)
					{
						List<Order> result;
						var key = GetUniqueOrderKey(creationMode, order);
						if (!uniqueOrders.TryGetValue(key, out result))
						{
							result = new List<Order>();
							uniqueOrders.Add(key, result);
						}

						result.Add(consol.Factory.Load<Order>(order.PK));
					}
				}
			}

			return uniqueOrders;
		}

		ZString GetHouseBill(INotifications notification, ZString houseBill, ZInt maxLengthAllowed)
		{
			if (houseBill.Length > maxLengthAllowed)
			{
				notification.Notify(new InfoNotification(Res.GetString("e7367d28-a418-4919-a92c-a80fe8e4371e", "House bill number has been truncated to fit shipment limit of {0} characters.", maxLengthAllowed)));
				return houseBill.Left(maxLengthAllowed);
			}
			return houseBill;
		}

		string GetUniqueOrderKey(OrderShipmentCreationMode creationMode, Order order)
		{
			string key;
			if (creationMode == OrderShipmentCreationMode.SingleShipment)
			{
				key = "OneHouseBillOnly";
			}
			else if (creationMode == OrderShipmentCreationMode.ShipmentPerOrder)
			{
				key = ZGuid.NewZGuid().ToStringKey();
			}
			else
			{
				key = order.BuyerPK.ToStringKey() + order.SupplierPK.ToStringKey();
			}

			return key;
		}

		ZDateTime GetPickupDate(IEnumerable<Order> shipmentOrders)
		{
			ZDateTime result = ZDateTime.Empty;
			foreach (Order order in shipmentOrders)
			{
				if (!order.JD_ExWorksRequiredBy.IsEmpty)
				{
					if (result.IsEmpty)
					{
						result = order.JD_ExWorksRequiredBy;
					}
					else if (result > order.JD_ExWorksRequiredBy)
					{
						result = order.JD_ExWorksRequiredBy;
					}
				}
			}
			return result;
		}

		JobDocAddress GetPickupPoint(IEnumerable<Order> shipmentOrders)
		{
			JobDocAddress result = null;
			foreach (Order order in shipmentOrders)
			{
				if (!order.GoodsAvailableAtAddress.IsEmpty)
				{
					if (result == null)
					{
						result = order.GoodsAvailableAtAddress;
					}
					else if (result.AddressAsASingleLine != order.GoodsAvailableAtAddress.AddressAsASingleLine)
					{
						return null;
					}
				}
			}

			return result;
		}

		ZDateTime GetDeliveryDate(IEnumerable<Order> shipmentOrders)
		{
			ZDateTime result = ZDateTime.Empty;
			foreach (Order order in shipmentOrders)
			{
				if (!order.JD_DeliveryRequiredBy.IsEmpty)
				{
					if (result.IsEmpty)
					{
						result = order.JD_DeliveryRequiredBy;
					}
					else if (result > order.JD_DeliveryRequiredBy)
					{
						result = order.JD_DeliveryRequiredBy;
					}
				}
			}
			return result;
		}

		IDocAddress GetDeliverPoint(IEnumerable<Order> shipmentOrders)
		{
			var deliveryPoint = GetDeliverPointFromOrderLineDeliveries(shipmentOrders);
			if (deliveryPoint == null)
			{
				return GetDeliverPointFromOrders(shipmentOrders);
			}

			return deliveryPoint;
		}

		OrgAddress GetDeliverPointFromOrderLineDeliveries(IEnumerable<Order> shipmentOrders)
		{
			OrgAddress deliveryPoint = null;
			foreach (Order order in shipmentOrders)
			{
				foreach (OrderLine orderLine in order.OrderLines)
				{
					foreach (OrderLineDelivery delivery in orderLine.Deliveries)
					{
						if (delivery.DeliveryPoint != null)
						{
							if (deliveryPoint == null)
							{
								deliveryPoint = delivery.DeliveryPoint;
							}
							else if (deliveryPoint.PK != delivery.DeliveryPoint.PK)
							{
								return null;
							}
						}
					}
				}
			}

			return deliveryPoint;
		}

		JobDocAddress GetDeliverPointFromOrders(IEnumerable<Order> shipmentOrders)
		{
			JobDocAddress deliveryPoint = null;

			foreach (Order order in shipmentOrders)
			{
				if (!order.GoodsDeliveredToAddress.IsEmpty)
				{
					if (deliveryPoint == null)
					{
						deliveryPoint = order.GoodsDeliveredToAddress;
					}
					else if (deliveryPoint.AddressAsASingleLine != order.GoodsDeliveredToAddress.AddressAsASingleLine)
					{
						return null;
					}
				}
			}

			return deliveryPoint;
		}

		JobDocAddress GetNotifyPoint(IEnumerable<Order> shipmentOrders)
		{
			JobDocAddress result = null;
			foreach (Order order in shipmentOrders)
			{
				if (!order.NotifyPartyDocAddress.IsEmpty)
				{
					result = order.NotifyPartyDocAddress;
					break;
				}
			}

			return result;
		}

		JobDocAddress GetNotify2Point(IEnumerable<Order> shipmentOrders)
		{
			JobDocAddress result = null;
			foreach (Order order in shipmentOrders)
			{
				if (!order.NotifyParty2DocAddress.IsEmpty)
				{
					result = order.NotifyParty2DocAddress;
					break;
				}
			}

			return result;
		}

		JobDocAddress GetNotify3Point(IEnumerable<Order> shipmentOrders)
		{
			JobDocAddress result = null;
			foreach (Order order in shipmentOrders)
			{
				if (!order.NotifyParty3DocAddress.IsEmpty)
				{
					result = order.NotifyParty3DocAddress;
					break;
				}
			}

			return result;
		}

#if DEBUG
		internal
#endif
		bool IsBuyersBrokerThisOrg(ZString containerMode)
		{
			if (Buyer != null && GlbCompany.CurrentCompany.OrgProxy != null)
			{
				OrgHeader broker = null;
				if (MainTransport != null)
				{
					broker = Buyer.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, MainTransport.JW_TransportMode, containerMode, EF_RL_NKPortDisch);
				}

				return broker != null
					&& (broker.PK == GlbCompany.CurrentCompany.OrgProxy.PK
						|| (GlbBranch.CurrentBranch.OrgProxy != null && broker.PK == GlbBranch.CurrentBranch.OrgProxy.PK));
			}

			return false;
		}

		#endregion

		#endregion

		#region Create Declaration

		#region Create Dec with Shipment

		BusinessObject CreateBrokerageWithShipment(ForwardingShipment shipment, List<Order> ordersList)
		{
			BusinessObject dec = (BusinessObject)shipment.Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			dec[JobDeclarationSchema.JE_JS] = shipment.PK;
			AppendInvoiceLineToDeclaration(dec, ordersList);

			foreach (Order order in ordersList)
			{
				order.JD_JE = dec.PK;
			}

			return dec;
		}

		#endregion

		#region Create Stand Alone Dec

		public BusinessObject CreateStandAloneBrokerage(INotifications notification, Order linkedOrder = null)
		{
			BusinessObject dec = null;

			if (!IsInDatabase || HasChanges)
			{
				notification.Notify(new InfoNotification(Res.GetString("51473b2f-b52d-4fbd-a672-4c12129d0b36", "Please save your changes before performing any Actions on this Pre Advice.")));
				return null;
			}

			if (IsExistingOperationsJobLinked)
			{
				notification.Notify(new InfoNotification(Res.GetString("d90ef74e-d392-4bce-a6a0-1af1febf97a7", "This Pre Advice has already been linked to an operations job and cannot be linked again.")));
				return null;
			}

			bool @continue = QueryUserIfExistingDeclarationExists(notification);
			if (@continue)
			{
				dec = CreateDeclarationFromPreAdvice(notification, linkedOrder);
				var containers = AppendContainersToDeclaration(dec);
				AppendTransportsToDeclaration(dec);
				AppendContainerModeToDeclaration(dec, linkedOrder, containers);
				AppendInvoiceLineToDeclaration(dec, Orders);
				AppendEvents(dec);
				ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true);
				notification.Notify(new InfoNotification(Res.GetString("a199b05f-c13b-40b8-8122-07d708d755ca", "{0} and commercial invoice line(s) have been successfully created from this pre-advice.", dec.HumanReadableName)));
				RefreshBinding();
			}

			return dec;
		}

		#endregion

		#region Customs Invoices

		public BusinessObject AppendInvoiceLineToCustomsDeclaration(INotifications notification)
		{
			if (!IsInDatabase || HasChanges)
			{
				notification.Notify(new InfoNotification(Res.GetString("291f7096-78a6-4881-8bdb-d8a432e17c70", "Please save your changes before performing any Actions on this Pre Advice.")));
				return null;
			}

			if (IsExistingOperationsJobLinked)
			{
				notification.Notify(new InfoNotification(Res.GetString("6827aead-a5a4-4024-826e-9ab7584a27ea", "This Pre Advice has already been linked to an operations job and cannot be linked again.")));
				return null;
			}

			BusinessObject dec = QueryUserForCustomsDeclaration(notification);

			if (dec != null)
			{
				ZString warning = AppendInvoiceLineToDeclaration(dec, Orders);

				ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true);

				if (!warning.IsEmpty)
				{
					notification.Notify(new InfoNotification(warning));
				}
				else
				{
					notification.Notify(new InfoNotification(Res.GetString("dd3d9c2e-258c-4ec6-8ae6-992ab21cd5fe", "Commercial invoice lines were successfully appended to {0}.", dec.HumanReadableName)));
				}

				return dec;
			}

			return null;
		}

		public ForwardingConsol GetConsolidationToAttachShipmentTo(INotifications notification)
		{
			if (!IsInDatabase || HasChanges)
			{
				notification.Notify(new InfoNotification(Res.GetString("291f7096-78a6-4881-8bdb-d8a432e17c70", "Please save your changes before performing any Actions on this Pre Advice.")));
				return null;
			}

			var consolidation = QueryUserForConsolidation(notification);
			return consolidation;
		}

		public string GetConsolMatchingErrors(ForwardingConsol consol)
		{
			var errors = new ZStringBuilder();
			if (!IsMatchingConsolPort(consol.JK_RL_NKLoadPort, EF_RL_NKPortLoad))
			{
				errors.AppendLine(Res.GetString("176e9e89-9b0c-43b2-9609-4bea4a7ba2c4", "The Load Port of the selected Consol does not match the Pre Advice."));
			}

			if (!IsMatchingConsolPort(consol.JK_RL_NKDischargePort, EF_RL_NKPortDisch))
			{
				errors.AppendLine(Res.GetString("5458e46e-6d95-4540-ae13-8371bbbb8df9", "The Discharge Port of the selected Consol does not match the Pre Advice."));
			}

			if (!errors.IsEmpty)
			{
				errors.AppendLine();
				errors.AppendLine(Res.GetString("51685b9c-42fe-4c50-914f-e1deb897fc1f", "Please select a valid Consol"));
			}

			return errors.ToString();
		}

		bool IsMatchingConsolPort(ZString consolPort, ZString jobShipmentPreplanningPort)
		{
			return consolPort.IsEmpty || jobShipmentPreplanningPort.IsEmpty || consolPort == jobShipmentPreplanningPort;
		}

		BusinessObject[] GetInvoiceHeadersForDeclaration(BusinessObject declaration)
		{
			ZQuery query = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
			query.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);
			return (BusinessObject[])declaration.Factory.Load<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>(query);
		}

		string AppendInvoiceLineToDeclaration(BusinessObject declaration, IEnumerable<Order> shipmentOrders)
		{
			Dictionary<string, BusinessObject> invoiceHeadersPerInvoiceNum = new Dictionary<string, BusinessObject>();
			BusinessObject[] existingInvoiceHeaders = GetInvoiceHeadersForDeclaration(declaration);
			foreach (BusinessObject existingInvoiceHeader in existingInvoiceHeaders)
			{
				if (!invoiceHeadersPerInvoiceNum.ContainsKey((ZString)existingInvoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceNumber]))
				{
					invoiceHeadersPerInvoiceNum.Add((ZString)existingInvoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceNumber], existingInvoiceHeader);
				}
			}

			bool updatedSome = false;
			bool updatedAll = true;

			foreach (Order order in shipmentOrders)
			{
				foreach (OrderLine orderLine in order.OrderLines)
				{
					if (orderLine.JO_QtyReceived > 0)
					{
						updatedSome = true;
						BusinessObject invoiceHeader;
						invoiceHeadersPerInvoiceNum.TryGetValue(orderLine.JO_CommercialInvoiceNo, out invoiceHeader);
						if (invoiceHeader == null)
						{
							invoiceHeader = (BusinessObject)declaration.Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
							invoiceHeadersPerInvoiceNum.Add(orderLine.JO_CommercialInvoiceNo, invoiceHeader);

							invoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceNumber] = orderLine.JO_CommercialInvoiceNo;
							invoiceHeader[JobComInvoiceHeaderSchema.JZ_OH_Supplier] = order.SupplierPK;
							invoiceHeader[JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency] = (order.OrderCurrency != null) ? order.JD_RX_NKOrderCurrency : ZString.Empty;
							invoiceHeader[JobComInvoiceHeaderSchema.JZ_RN_NKDefaultOrigin] = order.JD_RN_NKCountryOfSupply;
							invoiceHeader[JobComInvoiceHeaderSchema.JZ_IncoTerm] = order.JD_IncoTerm;

							if (invoiceHeader is IAttachOrders parent)
							{
								parent.AttachedOrders.Add(order);
							}

							((Integration.Forwarding.ICommercialInvoiceProvider)declaration).Invoices.Add(invoiceHeader);
						}

						var iBaseJobComInvoiceLine = declaration.Factory.New<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>();
						var iBaseJobComInvoiceLineBO = (BusinessObject)iBaseJobComInvoiceLine;
						iBaseJobComInvoiceLineBO[JobComInvoiceLineSchema.JI_JZ] = invoiceHeader.PK;
						iBaseJobComInvoiceLine.SynchroniseFromForwardingOrderLine(orderLine, false);
						invoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceAmount] = (ZDecimal)invoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceAmount] + (ZDecimal)(iBaseJobComInvoiceLineBO[JobComInvoiceLineSchema.JI_LinePrice]);
					}
					else
					{
						updatedAll = false;
					}
				}
			}

			string warning = "";

			if (!updatedSome)
			{
				return Res.GetString("cfb4d80f-b23c-4d3b-af3e-f25d4a8abe77", "No Commercial Lines were appended to {0}. Please check Quantity Received has been entered on all Order Lines.", declaration.HumanReadableName);
			}
			else if (!updatedAll)
			{
				return Res.GetString("fd397b20-684e-4673-ad7d-84d94deff2cb", "Not all Commercial Lines were appended to {0}. Please check Quantity Received has been entered on all Order Lines.", declaration.HumanReadableName);
			}

			return warning;
		}

		#endregion

		#region Create Declaration Implementation

		bool QueryUserIfExistingDeclarationExists(INotifications notification)
		{
			bool @continue = true;
			if (GetExistingCustomsDeclaration() != null)
			{
				QueryUserMsgBoxEventArgs args = new QueryUserMsgBoxEventArgs(Res.GetString("a0cb9c80-2688-431d-a9b2-504e1b23c44a", "A declaration already exists for this Master Bill, House Bill and Buyer. If you continue, a new declaration will be created.\r\n\r\nDo you want to continue?"), false);
				notification.QueryUser(args);

				if (!args.Response)
				{
					@continue = false;
				}
			}

			return @continue;
		}

		BusinessObject CreateDeclarationFromPreAdvice(INotifications notification, Order linkedOrder)
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			declaration[JobDeclarationSchema.JE_OH_ShippingLine] = EF_OH_Carrier;
			declaration[JobDeclarationSchema.JE_OH_Forwarder] = EF_OH_ReceivingAgent;
			declaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = EF_RL_NKPortLoad;
			declaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = EF_RL_NKPortDisch;
			declaration[JobDeclarationSchema.JE_MasterBill] = EF_MasterBill;
			declaration[JobDeclarationSchema.JE_HouseBill] = GetHouseBill(notification, !EF_HouseBill.IsEmpty || linkedOrder == null ? EF_HouseBill : linkedOrder.JD_Waybill, JobDeclarationSchema.JE_HouseBill.MaxLength);
			declaration[JobDeclarationSchema.JE_OH_Importer] = BuyerPK;
			declaration[JobDeclarationSchema.JE_TotalWeight] = EF_ActualWeight;
			declaration[JobDeclarationSchema.JE_TotalWeightUnit] = EF_UnitOfWeight;
			declaration[JobDeclarationSchema.JE_TotalVolume] = EF_ActualVolume;
			declaration[JobDeclarationSchema.JE_TotalVolumeUnit] = EF_UnitOfVolume;
			declaration[JobDeclarationSchema.JE_TotalNoOfPacks] = EF_Packs;
			declaration[JobDeclarationSchema.JE_TotalNoOfPacksPackType] = EF_F3_NKPackType;

			var cloneArgs = new BusinessObjectCloneArgs(new[] { nameof(JobDocAddressSchema.E2_AddressType) });

			var iDocDeclaration = (IDocAddresses)declaration;
			var deliveryPoint = GetDeliverPoint(Orders);
			if (deliveryPoint != null)
			{
				var deliveryDocAddress = iDocDeclaration.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ImporterPickupDeliveryAddress);
				if (deliveryPoint is JobDocAddress deliveryPointAsJobDocAddress && !deliveryPointAsJobDocAddress.IsEmpty)
				{
					deliveryDocAddress.CopyPersistentValuesFrom(deliveryPointAsJobDocAddress, cloneArgs);
				}
				else
				{
					deliveryDocAddress.E2_OA_Address = deliveryPoint.E2_OA_Address;
				}
			}

			var pickupPoint = GetPickupPoint(Orders);
			if (pickupPoint != null && !pickupPoint.IsEmpty)
			{
				iDocDeclaration.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupplierPickupDeliveryAddress).CopyPersistentValuesFrom(pickupPoint, cloneArgs);
			}

			var notifyPoint = GetNotifyPoint(Orders);
			if (notifyPoint != null)
			{
				iDocDeclaration.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty).CopyPersistentValuesFrom(notifyPoint, cloneArgs);
			}

			var notifyPoint2 = GetNotify2Point(Orders);
			if (notifyPoint2 != null)
			{
				iDocDeclaration.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty2).CopyPersistentValuesFrom(notifyPoint2, cloneArgs);
			}

			var notifyPoint3 = GetNotify3Point(Orders);
			if (notifyPoint3 != null)
			{
				iDocDeclaration.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty3).CopyPersistentValuesFrom(notifyPoint3, cloneArgs);
			}

			if (GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode == EF_RL_NKPortLoad.Left(2))
			{
				declaration[JobDeclarationSchema.JE_MessageType] = Core.Constants.Sales.Mode.Export;
			}
			else
			{
				declaration[JobDeclarationSchema.JE_MessageType] = Core.Constants.Sales.Mode.Import;
			}

			if (Orders.Count > 0)
			{
				declaration[JobDeclarationSchema.JE_OH_Supplier] = Orders[0].SupplierPK;
				declaration[JobDeclarationSchema.JE_RL_NKOrigin] = Orders[0].JD_RL_NKGoodsAvailableAt;
				declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = Orders[0].JD_RL_NKGoodsDeliveredTo;
				declaration[JobDeclarationSchema.JE_TransportMode] = Orders[0].JD_TransportMode;
			}

			foreach (Order order in Orders)
			{
				order.JD_JE = declaration.PK;
			}
			EF_JE = declaration.PK;

			return declaration;
		}

		void AppendTransportsToDeclaration(BusinessObject dec)
		{
			foreach (Transport routing in PreAdviceTransports)
			{
				if (routing.JW_RL_NKLoadPort == (ZString)dec[JobDeclarationSchema.JE_RL_NKPortOfLoading])
				{
					dec[JobDeclarationSchema.JE_ExportDate] = routing.JW_ATD;
				}

				if (routing.JW_RL_NKLoadPort == (ZString)dec[JobDeclarationSchema.JE_RL_NKOrigin])
				{
					dec[JobDeclarationSchema.JE_DateAtOrigin] = routing.JW_ETD;
				}

				if (routing.JW_RL_NKDiscPort == (ZString)dec[JobDeclarationSchema.JE_RL_NKPortOfArrival])
				{
					dec[JobDeclarationSchema.JE_TransportMode] = routing.JW_TransportMode;
					dec[JobDeclarationSchema.JE_VesselName] = routing.JW_Vessel;
					dec[JobDeclarationSchema.JE_VoyageFlightNo] = routing.JW_VoyageFlight;
					dec[JobDeclarationSchema.JE_DateOfArrival] = routing.JW_ATA;
				}

				if (routing.JW_RL_NKDiscPort == (ZString)dec[JobDeclarationSchema.JE_RL_NKFinalDestination])
				{
					dec[JobDeclarationSchema.JE_DateAtFinalDestination] = routing.JW_ETA;
				}
			}
		}

		List<Enterprise.Integration.Customs.Shared.IBaseCusContainer> AppendContainersToDeclaration(BusinessObject dec)
		{
			var containersInDeclaration = new List<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();

			if (Containers.Count > 0)
			{
				foreach (OrderContainer orderContainer in Containers)
				{
					var containerInDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
					containersInDeclaration.Add((Enterprise.Integration.Customs.Shared.IBaseCusContainer)containerInDeclaration);

					containerInDeclaration[CusContainerSchema.CO_JE] = dec.PK;
					containerInDeclaration[CusContainerSchema.CO_ContainerNumber] = orderContainer.J1_ContainerNumber;
					containerInDeclaration[CusContainerSchema.CO_RC] = orderContainer.J1_RC;
					containerInDeclaration[CusContainerSchema.CO_Seal] = orderContainer.J1_SealNum;
					containerInDeclaration[CusContainerSchema.CO_SecondSeal] = orderContainer.J1_AdditionalSealNum;
				}
			}

			return containersInDeclaration;
		}

		void AppendContainerModeToDeclaration(BusinessObject declaration, Order linkedOrder, List<Enterprise.Integration.Customs.Shared.IBaseCusContainer> containersInDeclaration)
		{
			var orderToBeUsedInContainerMode = linkedOrder ?? (Orders.Count > 0 ? Orders[0] : null);
			if (orderToBeUsedInContainerMode != null)
			{
				var transportMode = orderToBeUsedInContainerMode.JD_TransportMode;
				var packingMode = orderToBeUsedInContainerMode.JD_ContainerMode;
				if (!packingMode.IsEmpty)
				{
					var containerMode = ((Enterprise.Integration.Customs.IBaseJobDeclaration)declaration).GetContainerMode(transportMode, packingMode);
					if (!containerMode.IsEmpty)
					{
						declaration[JobDeclarationSchema.JE_ContainerMode] = containerMode;
					}

					foreach (var container in containersInDeclaration)
					{
						container.CO_FCL_LCL_AIR = container.GetContainerModeFromFreight(packingMode);
					}
				}
			}
		}

		BusinessObject GetExistingCustomsDeclaration()
		{
			ZQuery query = new ZQuery(JobDeclarationSchema.JE_MasterBill, EF_MasterBill);
			query.AddToFilter(JobDeclarationSchema.JE_HouseBill, EF_HouseBill);
			query.AddToFilter(JobDeclarationSchema.JE_OH_Importer, BuyerPK);

			return (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Customs.IBaseJobDeclaration>(query);
		}

		BusinessObject QueryUserForCustomsDeclaration(INotifications notification)
		{
			QueryUserFindboxEventArgs args = new QueryUserFindboxEventArgs(ModuleIDs.Customs.JobDeclaration, Lookups.Declarations);
			notification.QueryUser(args);
			return (BusinessObject)Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(args.SelectedItemPK);
		}

		ForwardingConsol QueryUserForConsolidation(INotifications notification)
		{
			var args = new QueryUserFindboxEventArgs(ModuleIDs.JobConsol, Lookups.Consolidations);
			notification.QueryUser(args);

			return Factory.Load<ForwardingConsol>(args.SelectedItemPK);
		}

		#endregion

		#endregion

		#region EnterpriseBusinessObject

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				if (!IsDeleted)
				{
					foreach (Transport transport in PreAdviceTransports)
					{
						result.Add(transport);

						if (transport.Voyage != null)
						{
							result.Add(transport.Voyage);
						}
					}
				}

				return result.ToArray();
			}
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return EF_PreshipID; }
		}

		#endregion

		#region IRoutingSupport Members

		RoutingCollection IRoutingSupport.TransportsIncludingRelated
		{
			get
			{
				if (transportsIncludingRelated == null)
				{
					transportsIncludingRelated = new RoutingCollection(this);
				}
				return transportsIncludingRelated;
			}
		}

		RoutingCollection transportsIncludingRelated;

		TransportCollection IRoutingSupport.Transports
		{
			get { return PreAdviceTransports; }
		}

		Transport MainTransport
		{
			get { return PreAdviceTransports.FindTransportByDischargePort(EF_RL_NKPortDisch); }
		}

		ZString IRoutingSupport.TransportMode
		{
			get { return MainTransport == null ? ZString.Empty : MainTransport.JW_TransportMode; }
		}

		string IRoutingSupport.AdditionalETAUpdateMsg
		{
			get { return null; }
		}

		string IRoutingSupport.AdditionalETDUpdateMsg
		{
			get { return null; }
		}

		#endregion

		#region ITransportParent Members

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return new JobShipmentPreplanningTransportSupporter(this); }
		}

		ZString ITransportParentCommon.TypeCode
		{
			get { return Core.Constants.TransportParentTypes.ShipmentPreAdvice; }
		}

		TransportCollection ITransportParent.Transports
		{
			get { return PreAdviceTransports; }
		}

		#endregion

		#region ITransportChangeNotifier Members

		void ITransportChangeNotifier.NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue)
		{
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.JobShipmentPreplanningWorkflowDescriptorCode; }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (fWorkflowItems == null)
				{
					fWorkflowItems = this.GetOrCreateProcessTaskCollection(() => new JobShipmentPrePlanningProcessTaskCollection(this));
					RegisterEditableChildObject(fWorkflowItems);
				}
				return fWorkflowItems;
			}
		}
		JobShipmentPrePlanningProcessTaskCollection fWorkflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, BuyerPK, ZGuid.Empty);
			return result;
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return new DocManagerInfo(this, Core.Constants.DocManagerCodes.JobShipmentPrePlanning); }
		}

		#endregion

		#region Set Order Values

		JobShipmentPreplanningOrderHelper OrderHelper
		{
			get { return orderHelper ?? (orderHelper = new JobShipmentPreplanningOrderHelper(this)); }
		}
		JobShipmentPreplanningOrderHelper orderHelper;

		public void SetValuesOnOrder(Order order)
		{
			OrderHelper.SetValuesOnOrder(order);
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return ZString.Empty; }
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.EF_ActualVolume:
					unitOfMeasure = EF_UnitOfVolume;
					break;

				case Schema.EF_ActualWeight:
					unitOfMeasure = EF_UnitOfWeight;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public ZDecimal GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			// not required as Shipment Pre Advice does not have its own transport mode and will use defaults
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobShipmentPreplanningFetchStrategy(this);
		}

		#endregion
	}
}
