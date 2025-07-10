using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.OceanCarrier.Business
{
	[UniversalDataContext(DataContextType.CarrierShipment)]
	[CodeProperty(Schema.CSH_CarrierShipmentReference), DescriptionProperty(Schema.CSH_HouseBill)]
	public sealed class CarrierShipmentHeader : AutoCarrierShipmentHeader, IEDocsProvider, IJobInvoicingPlugIn, IRatingSupporter, IWorkflowProvider, IDocAddresses
	{
		public CarrierShipmentHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[ResourceStringData("9E8C07B7-DF8A-429D-A1DF-7A1CC3FEBA74", Caption = "Carrier Reference")]
		public override ZString CSH_CarrierShipmentReference
		{
			get => base.CSH_CarrierShipmentReference;
			set => base.CSH_CarrierShipmentReference = value;
		}

		[ResourceStringData("05857F09-612F-4BCE-BF1E-5336137F6731", Caption = "House Bill")]
		public override ZString CSH_HouseBill
		{
			get => base.CSH_HouseBill;
			set => base.CSH_HouseBill = value;
		}

		public string JobNumber => CSH_CarrierShipmentReference;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("91728208-D41C-4A5D-B0D0-55E788629814", "Carrier Shipment");

				if (!CSH_CarrierShipmentReference.IsEmpty)
				{
					result += " " + CSH_CarrierShipmentReference;
				}
				if (!CSH_HouseBill.IsEmpty)
				{
					result += " " + Res.GetString("a3d2fdf2-1d20-41d6-816d-c6285a8aa188", "(House Bill='{0}')", CSH_HouseBill);
				}

				return result;
			}
		}

		#region Cargoes

		[ChildEditable]
		public CarrierShipmentCargoCollection Cargoes
		{
			get
			{
				if (cargoes != null)
				{
					return cargoes;
				}

				cargoes = new CarrierShipmentCargoCollection(this);
				RegisterEditableChildObject(cargoes);
				return cargoes;
			}
		}

		CarrierShipmentCargoCollection cargoes;

		#endregion

		#region Dates

		// this is a temp solution for testing purposes, we'll be able to change this once we have operational routing done
		public ZDateTime ExpectedDepartureDate => ZDateTime.Today.AddDays(1);
		public ZDateTime ExpectedArrivalDate => ZDateTime.Today.AddDays(30);

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void Delete()
		{
			if (JobHeader is JobHeader jobHeader
				&& !jobHeader.IsInDatabase)
			{
				jobHeader.Delete();
			}

			WorkflowItems.RemoveAndDeleteAll();

			base.Delete();
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				if (IsDeleted)
				{
					return base.BusinessObjectsWithRelatedEventsCore;
				}

				var businessObjects = new List<BusinessObject>();
				businessObjects.AddRange(Cargoes);

				var jobHeader = new JobHeader.Loader(this).Load();
				if (jobHeader != null)
				{
					businessObjects.Add(jobHeader);
				}

				return businessObjects.ToArray();
			}
		}

		#region IEDocsManagerInfo

		public DocManagerInfo DocManagerInfo => docManagerInfo ??= new DocManagerInfo(this, Constants.DocManagerCodes.CarrierShipmentHeader);

		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IJobInvoicingPlugIn members

		public CarrierShipmentHeaderJobInvoicingPlugIn InvoicingPlugIn => invoicingPlugIn ??= new CarrierShipmentHeaderJobInvoicingPlugIn(this);
		CarrierShipmentHeaderJobInvoicingPlugIn invoicingPlugIn;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => InvoicingPlugIn.InvoicingSupporter;

		#endregion

		#region JobHeader

		public JobHeader JobHeader => new JobHeader.Loader(this).Load();

		#endregion

		#region IJobHeaderParent members

		bool IJobHeaderParent.AllowInvoiceDeletion => InvoicingPlugIn.AllowInvoiceDeletion;

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		#endregion

		#region IRatingSupporter

		public RatingAdaptersProvider AdaptersProvider => new CarrierShipmentRatingAdaptersProvider(this);

		#endregion

		#region Workflow

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_GB, GlbBranch.CurrentBranch.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GE, GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, CSH_ShipmentType, ZString.Empty);

			return result;
		}

		public ZString WorkflowType => WorkflowDescriptors.CarrierShipmentHeaderWorkflowDescriptorCode;

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

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

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CarrierShipmentHeaderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		CarrierShipmentHeaderProcessTaskCollection workflowItems;

		#endregion

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				if (noteTypes == null)
				{
					noteTypes = base.NoteTypesCore;
					noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
				}
				return noteTypes;
			}
		}
		NoteTypeCollection noteTypes;

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ??= new CarrierShipmentHeaderDocumentSupporter(this); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		public void PopulateCSH_CarrierShipmentReferenceIfNeeded()
		{
			if (!IsDeleted)
			{
				PopulateNumberPropertyIfRequired(CSH_CarrierShipmentReferenceInfo, GenerateCarrierShipmentReference);
			}
		}

		public ZString GenerateCarrierShipmentReference(BusinessObjectFactory factory)
		{
			if (CSH_ShipmentType == "SHP")
			{
				var generatorTarget = new CarrierShipmentGeneratorTarget();
				var generator = new NumberGenerator
				{
					Factory = Factory,
					Context = new NumberGeneratorContext(),
					BaseFountain = Env.NumberFountains.CarrierShipmentReference,
					FountainGetter = Env.NumberFountains.GetCarrierShipmentReferenceGeneratorFountain,
					PrimaryTarget = generatorTarget,
					TargetBO = this
				};
				generator.ValueProviders.AddRange(new StandardValueSource());
				generator.Generate();
				generator.EnforceMaxLengths();

				return generatorTarget.Value.ToUpper();
			}

			return ZString.Empty;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (CSH_CarrierShipmentReference.IsEmpty)
			{
				PopulateCSH_CarrierShipmentReferenceIfNeeded();
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			CSH_ShipmentType = "SHP";
		}

#endif

		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		public SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		public JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			return new JobDocAddressRequirement(addressType);
		}

		public void DocAddressChanged(JobDocAddress docAddress)
		{
		}

		public void OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		public void OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		public void AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		public void OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		public bool CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		public OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType)
		{
			return new OrgHeaderCollection(Factory);
		}

		[ChildEditable]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}

		JobDocAddressDependentCollection docAddresses;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				if (CSH_ShipmentType == "SHP")
				{
					var result = new List<DocAddressType>
					{
						DocAddressType.BookingPartyDocumentaryAddress,
						DocAddressType.ConsigneeDocumentaryAddress,
						DocAddressType.ConsignorDocumentaryAddress,
						DocAddressType.NotifyParty
					};

					return result.ToArray();
				}

				return Array.Empty<DocAddressType>();
			}
		}
	}
}
