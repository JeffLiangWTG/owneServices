using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ARTerms : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string InvoiceClass = "InvoiceClass";
			public const string InvoiceTerm = "InvoiceTerm";
			public const string InvoiceDays = "InvoiceDays";
			public const string TreatDisbursementsAsStandardValue = "TreatDisbursementsAsStandardValue";
			public const string JobType = "JobType";
			public const string BranchPK = "BranchPK";
			public const string DeptPK = "DeptPK";
			public const string Direction = "Direction";
			public const string TransportMode = "TransportMode";
		}

		#endregion

		public ARTerms()
		{
			this.SetDefaults();
		}

		public ARTerms(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
				: base(fallbackLevel, factory)
		{
			this.SetDefaults();
		}

		void SetDefaults()
		{
			using (base.SuspendSettingHasChanges())
			{
				JobType = JobTypeDirectionAndTransportInfoProvider.All;
				Direction = JobTypeDirectionAndTransportInfoProvider.All;
				TransportMode = JobTypeDirectionAndTransportInfoProvider.All;
				BranchPK = ZGuid.Empty;
				DeptPK = ZGuid.Empty;
				InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
				InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ARTerms(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			ARTerms termsClone = clone as ARTerms;

			if (termsClone != null)
			{
				using (!clone.IsValidationSuspended ? clone.GetValidationSuspender() : null)
				{
					foreach (ARTermsCycle cycle in this.ARTermsCycles)
					{
						var cycleClone = (ARTermsCycle)cycle.Clone(this.CurrentFallbackLevel, this.CurrentFactory);
						termsClone.ARTermsCycles.Add(cycleClone);
					}

					foreach (ARPaymentCycle cycle in this.ARPaymentCycles)
					{
						var cycleClone = (ARPaymentCycle)cycle.Clone(this.CurrentFallbackLevel, this.CurrentFactory);
						termsClone.ARPaymentCycles.Add(cycleClone);
					}
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateJobType();
			ValidateBranchPK();
			ValidateDeptPK();
			ValidateDirection();
			ValidateTransportMode();
			ValidateInvoiceClass();
			ValidateInvoiceTerm();
			ValidateInvoiceDays();

			CheckDuplicateRow();

			RemoveRowError(OrgARTerms.DefaultTermMustExistMessage);
			if (!ParentCollection.OfType<ARTerms>().Any(x => x.InvoiceClass == OrgARTermsLookups.InvoiceTypes.All.Code
						&& x.JobType == JobTypeDirectionAndTransportInfoProvider.All
						&& x.TransportMode == JobTypeDirectionAndTransportInfoProvider.All
						&& x.Direction == JobTypeDirectionAndTransportInfoProvider.All
						&& x.BranchPK == ZGuid.Empty
						&& x.DeptPK == ZGuid.Empty))
			{
				AddRowError(OrgARTerms.DefaultTermMustExistMessage);
			}

			foreach (BusinessObject bizO in ARTermsCycles)
			{
				bizO.RunPreSaveValidation();
			}
			foreach (BusinessObject bizO in ARPaymentCycles)
			{
				bizO.RunPreSaveValidation();
			}
		}

		#region Bound Properties

		#region InvoiceTerm

		[List("InvoiceTermList")]
		public ZString InvoiceTerm
		{
			get { return fInvoiceTerm; }
			set
			{
				bool changed = InvoiceTerm != value;
				SetNonPersistentPropertyValue(InvoiceTermInfo, ref fInvoiceTerm, value);
				if (changed)
				{
					ResetPaymentTermDays(InvoiceTerm, InvoiceDaysInfo);
					SetARTermsCycleReadonly(true);
					ARTermsCycles.MarkAsNeedingValidationIncludingChildren();
					SetARPaymentCycleReadonly(true);
					ARPaymentCycles.MarkAsNeedingValidationIncludingChildren();
				}

				if (!IsValidationSuspended)
				{
					ValidateInvoiceTerm();
				}
			}
		}

		ZString fInvoiceTerm;

		protected bool InvoiceTerm_ReadOnly
		{
			get { return false; }
		}

		public ZPropertyInfo InvoiceTermInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceTerm); }
		}

		public void ValidateInvoiceTerm()
		{
			InvoiceTermInfo.ClearAllNotifications();

			EnglishCharactersValidation.ErrorIfNotWesternEuropean(InvoiceTermInfo);

			MandatoryValidation.CheckEntered(InvoiceTermInfo);
			ListValidation.ErrorIfInvalidCode(InvoiceTermInfo);

			if (InvoiceTerm == Constants.InvoiceTerms.MonthsFromInvoiceCycleDate)
			{
				if (ARTermsCycles.Count == 0)
				{
					InvoiceTermInfo.AddError(Res.GetString("7BC8CD78-4102-470F-A0C7-33CE5266050A", "Invoice Cycle data must be entered."));
				}
			}
			if (InvoiceTerm == Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle)
			{
				if (ARPaymentCycles.Count == 0)
				{
					InvoiceTermInfo.AddError(Res.GetString("B50BF8F6-EC54-4A5D-9B6B-CA4B1D4DE4F9", "Payment Cycle data must be entered."));
				}
			}
			if (InvoiceTerm == Constants.InvoiceTerms.FromDeliveryOrPickupDate && JobType != JobInvoicingConsumerTypes.ShipmentCode && JobType != JobInvoicingConsumerTypes.BrokerageCode && JobType != JobInvoicingConsumerTypes.LocalCartageCode)
			{
				InvoiceTermInfo.AddError(Res.GetString("F35D8D2A-B6BA-11ED-AFA1-0242AC120002", "DLP invoice term is only available for Shipment, Brokerage and Port Transport Job Type."));
			}
		}

		void ResetPaymentTermDays(ZString terms, ZPropertyInfo termDaysInfo)
		{
			if (IsTermWithoutDays(terms))
			{
				termDaysInfo.Value = ZByte.Zero;
			}
		}

		#endregion

		#region InvoiceClass

		[List("InvoiceTypeList")]
		public ZString InvoiceClass
		{
			get { return fInvoiceClass; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceClassInfo, ref fInvoiceClass, value);
				if (!IsValidationSuspended)
				{
					ValidateInvoiceClass();
				}
			}
		}

		ZString fInvoiceClass;

		protected bool InvoiceClass_ReadOnly
		{
			get { return false; }
		}

		public ZPropertyInfo InvoiceClassInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceClass); }
		}

		public void ValidateInvoiceClass()
		{
			InvoiceClassInfo.ClearAllNotifications();

			EnglishCharactersValidation.ErrorIfNotWesternEuropean(InvoiceClassInfo);
			MandatoryValidation.CheckEntered(InvoiceClassInfo);
			ListValidation.ErrorIfInvalidCode(InvoiceClassInfo);
			CheckDuplicateRow();
		}

		#endregion

		#region InvoiceDays

		public ZByte InvoiceDays
		{
			get { return fInvoiceDays; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceDaysInfo, ref fInvoiceDays, value);
				if (!IsValidationSuspended)
				{
					ValidateInvoiceDays();
				}
				ARTermsCycles.MarkAsNeedingValidationIncludingChildren();
			}
		}

		ZByte fInvoiceDays;

		protected bool InvoiceDays_ReadOnly
		{
			get { return IsTermWithoutDays(InvoiceTerm); }
		}

		bool IsTermWithoutDays(ZString term)
		{
			return AccountingMasterFilesUtils.IsTermWithoutDays(term);
		}

		public ZPropertyInfo InvoiceDaysInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceDays); }
		}

		public void ValidateInvoiceDays()
		{
			InvoiceDaysInfo.ClearAllNotifications();
			if (!InvoiceDaysInfo.ReadOnly && InvoiceTerm == Constants.InvoiceTerms.FromDeliveryOrPickupDate && (InvoiceDays < 0 || InvoiceDays > 99))
			{
				InvoiceDaysInfo.AddError(Res.GetString("201A22D8-B6BB-11ED-AFA1-0242AC120002", "Days must be between 0 and 99."));
			}
		}

		#endregion

		#region JobType

		[List("JobTypeList")]
		public ZString JobType
		{
			get { return jobType; }
			set
			{
				using (RowValidationSuspender.GetSuspender())
				{
					SetNonPersistentPropertyValue(JobTypeInfo, ref jobType, value);
					if (!IsValidationSuspended)
					{
						ValidateJobType();
					}
					Direction = Direction_ReadOnly ? new ZString(JobTypeDirectionAndTransportInfoProvider.All) : (Direction.IsEmpty ? new ZString(JobTypeDirectionAndTransportInfoProvider.All) : Direction);
					TransportMode = TransportMode_ReadOnly ? new ZString(JobTypeDirectionAndTransportInfoProvider.All) : (TransportMode.IsEmpty ? new ZString(JobTypeDirectionAndTransportInfoProvider.All) : TransportMode);
				}
			}
		}
		ZString jobType;

		public ZPropertyInfo JobTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JobType); }
		}

		void ValidateJobType()
		{
			JobTypeInfo.ClearAllNotifications();
			InvoiceTermInfo.ClearAllNotifications();

			EnglishCharactersValidation.ErrorIfNotWesternEuropean(JobTypeInfo);
			ValidationHelper.ValidateJobType();
			CheckDuplicateRow();
		}

		#endregion

		#region Branch
		[RelatedBusinessObject("Branch")]
		[List("Branches")]
		public ZGuid BranchPK
		{
			get { return branchPK; }
			set
			{
				SetNonPersistentPropertyValue(BranchPKInfo, ref branchPK, value);
				if (!IsValidationSuspended)
				{
					ValidateBranchPK();
				}
			}
		}
		ZGuid branchPK;

		public GlbBranch Branch
		{
			get { return (GlbBranch)CurrentFactory.Load(typeof(GlbBranch), branchPK); }
		}

		public ZPropertyInfo BranchPKInfo
		{
			get { return GetZPropertyInfo(Schema.BranchPK); }
		}

		public void ValidateBranchPK()
		{
			BranchPKInfo.ClearAllNotifications();

			TypeValidation.CheckValidGuid(BranchPKInfo);
			ListValidation.ErrorIfInvalidPK(BranchPKInfo);

			if (!BranchPKInfo.HasErrors())
			{
				CheckDuplicateRow();
			}
		}
		#endregion

		#region Department

		[RelatedBusinessObject("Department")]
		[List("Departments")]
		public ZGuid DeptPK
		{
			get { return deptPK; }
			set
			{
				SetNonPersistentPropertyValue(DeptPKInfo, ref deptPK, value);
				if (!IsValidationSuspended)
				{
					ValidateDeptPK();
				}
			}
		}
		ZGuid deptPK;

		public GlbDepartment Department
		{
			get { return (GlbDepartment)CurrentFactory.Load(typeof(GlbDepartment), DeptPK); }
		}

		public ZPropertyInfo DeptPKInfo
		{
			get { return GetZPropertyInfo(Schema.DeptPK); }
		}

		public void ValidateDeptPK()
		{
			DeptPKInfo.ClearAllNotifications();

			TypeValidation.CheckValidGuid(DeptPKInfo);
			ListValidation.ErrorIfInvalidPK(DeptPKInfo);
			if (!DeptPKInfo.HasErrors())
			{
				CheckDuplicateRow();
			}
		}

		#endregion

		#region Direction
		[List("DirectionList")]
		public ZString Direction
		{
			get { return direction; }
			set
			{
				SetNonPersistentPropertyValue(DirectionInfo, ref direction, value);
				if (!IsValidationSuspended)
				{
					ValidateDirection();
				}
			}
		}
		ZString direction;

		public ZPropertyInfo DirectionInfo
		{
			get { return GetZPropertyInfo(Schema.Direction); }
		}

		public bool Direction_ReadOnly
		{
			get { return ValidationHelper.IsDirectionReadonly; }
		}

		public void ValidateDirection()
		{
			DirectionInfo.ClearAllNotifications();
			ValidationHelper.ValidateDirection();
			CheckDuplicateRow();
		}

		#endregion

		#region TransportMode
		[List("TransportModeList")]
		public ZString TransportMode
		{
			get { return transportMode; }
			set
			{
				SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value);
				if (!IsValidationSuspended)
				{
					ValidateTransportMode();
				}
			}
		}
		ZString transportMode;

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportMode); }
		}

		public bool TransportMode_ReadOnly
		{
			get { return ValidationHelper.IsTransportModeReadonly; }
		}

		public void ValidateTransportMode()
		{
			TransportModeInfo.ClearAllNotifications();
			ValidationHelper.ValidateTransportMode();
			CheckDuplicateRow();
		}

		#endregion

		#region TreatDisbursementsAsStandardValue

		public ZDecimal TreatDisbursementsAsStandardValue
		{
			get { return fTreatDisbursementsAsStandardValue; }
			set
			{
				var oldValue = TreatDisbursementsAsStandardValue;

				SetNonPersistentPropertyValue(TreatDisbursementsAsStandardValueInfo, ref fTreatDisbursementsAsStandardValue, value);
				if (!IsValidationSuspended)
				{
					ValidateTreatDisbursementsAsStandardValue();
				}
				if (ParentCollection != null && oldValue != value) //could have a check like, if old and new are both ZDecimal.Zero, do nothing, if needed
				{
					foreach (ARTerms terms in ParentCollection)
					{
						terms.TreatDisbursementsAsStandardValue = this.TreatDisbursementsAsStandardValue;
					}
				}
			}
		}

		ZDecimal fTreatDisbursementsAsStandardValue;

		protected bool TreatDisbursementsAsStandardValue_ReadOnly
		{
			get { return false; }
		}

		public ZPropertyInfo TreatDisbursementsAsStandardValueInfo
		{
			get { return GetZPropertyInfo(Schema.TreatDisbursementsAsStandardValue); }
		}

		public void ValidateTreatDisbursementsAsStandardValue()
		{
			TreatDisbursementsAsStandardValueInfo.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(TreatDisbursementsAsStandardValueInfo, 19, 4);
			MandatoryValidation.CheckNotNegative(TreatDisbursementsAsStandardValueInfo);
		}

		#endregion

		#region Collections

		[ChildEditable]
		[ActionFieldFollow(true)]
		public ARTermsCycleCollection ARTermsCycles
		{
			get
			{
				if (fARTermsCycles == null)
				{
					fARTermsCycles = new ARTermsCycleCollection(this.CurrentFallbackLevel, this.CurrentFactory);
					SetARTermsCycleReadonly();
					base.RegisterEditableChildObject(fARTermsCycles);
				}
				return fARTermsCycles;
			}
		}
		ARTermsCycleCollection fARTermsCycles;

		[ChildEditable]
		[ActionFieldFollow(true)]
		public ARPaymentCycleCollection ARPaymentCycles
		{
			get
			{
				if (fARPaymentCycles == null)
				{
					fARPaymentCycles = new ARPaymentCycleCollection(this.CurrentFallbackLevel, this.CurrentFactory);
					SetARPaymentCycleReadonly();
					base.RegisterEditableChildObject(fARPaymentCycles);
				}
				return fARPaymentCycles;
			}
		}
		ARPaymentCycleCollection fARPaymentCycles;

		#endregion

		#endregion

		#region Implementation

		public BusinessObjectCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ARTermsCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}

		void SetARTermsCycleReadonly(bool deleteExisting = false)
		{
			ARTermsCycles.SetReadOnlyIncludingChildren(InvoiceTerm != InvoiceTermsList.MonthsFromInvoiceCycleDate.Code);
			if (ARTermsCycles.ReadOnly)
			{
				if (deleteExisting)
				{
					ARTermsCycles.RemoveAndDeleteAll();
				}
			}
		}

		void SetARPaymentCycleReadonly(bool deleteExisting = false)
		{
			ARPaymentCycles.SetReadOnlyIncludingChildren(InvoiceTerm != InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code);
			if (ARPaymentCycles.ReadOnly)
			{
				if (deleteExisting)
				{
					ARPaymentCycles.RemoveAndDeleteAll();
				}
			}
		}

		#endregion

		#region Lookups

		OrgARTermsLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new OrgARTermsLookups(null);
				}
				return lookups;
			}
		}
		OrgARTermsLookups lookups;

		public CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				CodeDescriptionPairList invoiceTypeList = new CodeDescriptionPairList();
				invoiceTypeList.AddRange(ValidationHelper.InvoiceTypeList);
				AddAdditionalInvoiceTypes(invoiceTypeList);
				return invoiceTypeList;
			}
		}

		void AddAdditionalInvoiceTypes(CodeDescriptionPairList invoiceTypeList)
		{
			invoiceTypeList.Insert(0, Enterprise.MasterFiles.Business.OrgARTermsLookups.InvoiceTypes.All);
			invoiceTypeList.Insert(1, Enterprise.MasterFiles.Business.OrgARTermsLookups.InvoiceTypes.DSB);
		}

		public CodeDescriptionPairList InvoiceTermList
		{
			get
			{
				return Lookups.InvoiceTermListNonCompanySpecific;
			}
		}

		public CodeDescriptionPairList JobTypeList
		{
			get
			{
				return ValidationHelper.JobTypeList;
			}
		}

		public GlbBranchCollection Branches
		{
			get
			{
				return new GlbBranchCollection(LookupFactory);
			}
		}

		public GlbDepartmentCollection Departments
		{
			get
			{
				return new GlbDepartmentCollection(LookupFactory);
			}
		}

		public CodeDescriptionPairList DirectionList
		{
			get
			{
				return ValidationHelper.DirectionList;
			}
		}

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				return ValidationHelper.TransportModeList;
			}
		}

		ReadOnlyBusinessObjectFactory LookupFactory
		{
			get
			{
				return lookupFactory != null && lookupFactory.IsOwnedByCurrentThread
					? lookupFactory
					: (lookupFactory = new ReadOnlyBusinessObjectFactory());
			}
		}
		ReadOnlyBusinessObjectFactory lookupFactory;

		#endregion

		#region Validation
		JobTypeDirectionAndTransportInfoProvider ValidationHelper
		{
			get
			{
				if (validationHelper == null)
				{
					validationHelper = new JobTypeDirectionAndTransportInfoProvider(() => JobTypeInfo, () => DirectionInfo, () => TransportModeInfo);
				}
				return validationHelper;
			}
		}
		JobTypeDirectionAndTransportInfoProvider validationHelper;

		void CheckDuplicateRow()
		{
			if (ParentCollection != null && !RowValidationSuspender.IsSuspended)
			{
				foreach (ARTerms arTerms in ParentCollection)
				{
					arTerms.ClearRowNotifications();
					if (ParentCollection.OfType<ARTerms>().Count(x => x.InvoiceClass == arTerms.InvoiceClass
																			&& x.JobType == arTerms.JobType
																			&& x.TransportMode == arTerms.TransportMode
																			&& x.Direction == arTerms.Direction
																			&& x.BranchPK == arTerms.BranchPK
																			&& x.DeptPK == arTerms.DeptPK) > 1)
					{
						arTerms.AddRowError(Res.GetString("0E0986DC-C11C-4F7F-A889-1A242A32F411", @"Term settings for following already exists -
Job Type: {0}, Direction: {1}, Transport Mode: {2}, Branch: {3}, Dept.: {4}, Invoice type: {5}", arTerms.JobType, (arTerms.Direction.IsEmpty ? (ZString)(NoResString)"<Empty>" : arTerms.Direction), (arTerms.TransportMode.IsEmpty ? (ZString)(NoResString)"<Empty>" : arTerms.TransportMode), (arTerms.BranchPK.IsEmpty ? (ZString)"ALL" : arTerms.Branch.GB_Code), (arTerms.DeptPK.IsEmpty ? (ZString)"ALL" : arTerms.Department.GE_Code), arTerms.InvoiceClass));
					}
				}
			}
		}

		FunctionalitySuspender RowValidationSuspender
		{
			get
			{
				return rowValidationSuspender ?? (rowValidationSuspender = new FunctionalitySuspender(() => CheckDuplicateRow()));
			}
		}
		FunctionalitySuspender rowValidationSuspender;

		#endregion

		#region Xml Serialisation

		ZXmlSerializer TermsCycleSerialiser
		{
			get
			{
				return termsCycleSerialiser ?? (termsCycleSerialiser = ZXmlSerializer.New(typeof(ARTermsCycle)));
			}
		}
		ZXmlSerializer termsCycleSerialiser;

		ZXmlSerializer PaymentCycleSerialiser
		{
			get
			{
				return paymentCycleSerialiser ?? (paymentCycleSerialiser = ZXmlSerializer.New(typeof(ARPaymentCycle)));
			}
		}
		ZXmlSerializer paymentCycleSerialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.JobType, JobType.ToString());
			writer.WriteElementString(Schema.BranchPK, BranchPK.ToString());
			writer.WriteElementString(Schema.DeptPK, DeptPK.ToString());
			writer.WriteElementString(Schema.Direction, Direction.ToString());
			writer.WriteElementString(Schema.TransportMode, TransportMode.ToString());
			writer.WriteElementString(Schema.InvoiceClass, InvoiceClass.ToString());
			writer.WriteElementString(Schema.InvoiceDays, InvoiceDays.ToString());
			writer.WriteElementString(Schema.InvoiceTerm, InvoiceTerm.ToString());
			writer.WriteElementString(Schema.TreatDisbursementsAsStandardValue, TreatDisbursementsAsStandardValue.ToString());

			writer.WriteStartElement("ARTermsCycles");
			foreach (var termsCycle in ARTermsCycles.OfType<ARTermsCycle>())
			{
				TermsCycleSerialiser.Serialize(writer, termsCycle);
			}
			writer.WriteEndElement();

			writer.WriteStartElement("ARPaymentCycles");
			foreach (var paymentCycle in ARPaymentCycles.OfType<ARPaymentCycle>())
			{
				PaymentCycleSerialiser.Serialize(writer, paymentCycle);
			}
			writer.WriteEndElement();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			JobType = new ZString(reader.ReadElementString(Schema.JobType));
			BranchPK = new ZGuid(reader.ReadElementString(Schema.BranchPK));
			DeptPK = new ZGuid(reader.ReadElementString(Schema.DeptPK));
			Direction = new ZString(reader.ReadElementString(Schema.Direction));
			TransportMode = new ZString(reader.ReadElementString(Schema.TransportMode));
			InvoiceClass = new ZString(reader.ReadElementString(Schema.InvoiceClass));
			InvoiceDays = new ZByte(reader.ReadElementString(Schema.InvoiceDays));
			InvoiceTerm = new ZString(reader.ReadElementString(Schema.InvoiceTerm));
			TreatDisbursementsAsStandardValue = new ZDecimal(reader.ReadElementString(Schema.TreatDisbursementsAsStandardValue));

			reader.Reader.ReadStartElement("ARTermsCycles");
			while (reader.Reader.Name == "ARTermsCycle")
			{
				var termsCycle = (ARTermsCycle)TermsCycleSerialiser.Deserialize(reader);
				ARTermsCycles.Add(termsCycle);
			}
			if (ARTermsCycles.Count > 0) //otherwise there was no separate start/end element
			{
				reader.Reader.ReadEndElement();
			}

			reader.Reader.ReadStartElement("ARPaymentCycles");
			while (reader.Reader.Name == "ARPaymentCycle")
			{
				var paymentCycle = (ARPaymentCycle)PaymentCycleSerialiser.Deserialize(reader);
				ARPaymentCycles.Add(paymentCycle);
			}
			if (ARPaymentCycles.Count > 0) //otherwise there was no separate start/end element
			{
				reader.Reader.ReadEndElement();
			}
		}
		#endregion
	}
}
