using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Freight.Common.Business.Testing
{
	public sealed class DummyCartageParent : NonPersistentBusinessObject, ICartageParent, IStmALogParent, IStmNoteParent, IDocManagerSupport
	{
		public DummyCartageParent(BusinessObjectFactory factory) : base(factory)
		{
		}

		public bool CartageTypesReturnsEmptyArray
		{ get; set; }

		#region CartageAddress

		public JobDocAddress CartageAddress
		{
			get
			{
				if (cartageAddress == null)
				{
					cartageAddress = JobDocAddress.GetOrCreateDocAddressFromParent(this, DocAddressType.Carrier);
					cartageAddress.OrganisationPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				}
				return cartageAddress;
			}
		}

		JobDocAddress cartageAddress;

		#endregion

		#region ICartageParent Members

		ZGuid ICartageParent.BranchPK
		{
			get { return ZGuid.Empty; }
		}

		ZGuid ICartageParent.CartageParentID
		{
			get { return PK; }
		}

		ZString ICartageParent.CartageParentTableCode
		{
			get { return "JK"; }
		}

		public void SetCartageType(CartageType cartageType)
		{
			this.cartageTypes = new CartageType[] { cartageType };
		}

		public void SetCartageTypes(params CartageType[] cartageTypes)
		{
			this.cartageTypes = cartageTypes;
		}

		IReadOnlyCollection<CartageType> ICartageParent.CartageTypes
		{
			get
			{
				if (CartageTypesReturnsEmptyArray)
				{
					return Array.Empty<CartageType>();
				}

				if (cartageTypes == null || !cartageTypes.Any())
				{
					SetCartageType(new DummyCartageType(this));
				}

				return cartageTypes.ToArray();
			}
		}
		IEnumerable<CartageType> cartageTypes = Array.Empty<CartageType>();

		CartageType ICartageParent.GetLocalCartageType
		{
			get { return ((ICartageParent)this).CartageTypes.First(); }
		}

		public override string TableName
		{
			get
			{
				return "JobCartage";
			}
		}

		ControllerID ICartageParent.ControllerID
		{
			get { return controllerID; }
		}
		readonly ControllerID controllerID = DummyControllerIDs.Dummy;

		ZString ICartageParent.GoodsDescription
		{
			get { return "Dummy Goods"; }
		}

		ZString ICartageParent.HumanReadableName
		{
			get { return "Dummy Cartage Parent"; }
		}

		ZString ICartageParent.OrderReferenceNumber
		{
			get { return "Dummy Order Ref"; }
		}

		ZString ICartageParent.ServiceLevel
		{
			get { return "DSV"; }
		}

		ZString ICartageParent.WayBillNumber
		{
			get { return "Dummy WayBill"; }
		}

		ZString ICartageParent.UniqueConsignmentID
		{
			get { return "Dum1001"; }
		}

		ZGuid ICartageParent.JobHeaderPK
		{
			get { return ZGuid.Empty; }
		}

		ZGuid ICartageParent.LocalClientAddressPK
		{
			get { return ZGuid.Empty; }
		}

		ZInt ICartageParent.TotalPackages
		{
			get { return 10; }
		}

		ZString ICartageParent.TotalPackType
		{
			get { return Core.Constants.PkgUnit.Bag; }
		}

		ZDecimal ICartageParent.TotalWeight
		{
			get { return 30m; }
		}

		ZString ICartageParent.TotalWeightUnit
		{
			get { return Core.Constants.Weight.Kilograms; }
		}

		ZDecimal ICartageParent.TotalVolume
		{
			get { return 20m; }
		}

		ZString ICartageParent.TotalVolumeUnit
		{
			get { return Core.Constants.Volume.CubicMetres; }
		}

		#endregion

		#region Overrides

		public override bool HasChanges
		{
			get { return hasChanges; }
			set { hasChanges = value; }
		}
		bool hasChanges;

		public override bool IsInDatabase
		{
			get { return true; }
		}

		public override void Delete()
		{
			base.Delete();

			CartageAddress.Delete();
		}

		#endregion

		#region IStmALogParent Members

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get { return Array.Empty<BusinessObject>(); }
		}

		public Logs Logs
		{
			get { return logs ?? (logs = new Logs(this)); }
		}
		Logs logs;

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return Factory; }
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return TableName; }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion

		#region IStmNoteParent Members

		BusinessObject[] IStmNoteParent.BusinessObjectsWithRelatedNotes
		{
			get { return Array.Empty<BusinessObject>(); }
		}

		GetValueDelegate<NoteTypeCollection> IStmNoteParent.CustomNoteTypesDelegate { get; set; }

		StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
		{
			get { return StmNoteContexts.Default; }
		}

		NoteTypeCollection IStmNoteParent.NoteTypes
		{
			get { return new NoteTypeCollection(); }
		}

		public Notes Notes
		{
			get { return notes ?? (notes = new Notes(this)); }
		}
		Notes notes;

		BusinessObjectFactory IStmNoteParent.NotesFactory
		{
			get { return Factory; }
		}

		ZGuid IStmNoteParent.NotesParentPK
		{
			get { return PK; }
		}

		string IStmNoteParent.NotesParentTableName
		{
			get { return TableName; }
		}

		bool IStmNoteParent.SupportsNotes
		{
			get { return true; }
		}

		#endregion

		event EventHandler ICartageParent.CartageTypesChanged
		{
			add { cartageTypesChanged += value; }
			remove { cartageTypesChanged -= value; }
		}
		event EventHandler cartageTypesChanged;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ICartageParent.CartageTypesChanged need to be implemented as required by interface")]
		void OnCartageTypesChanged(EventArgs e)
		{
			if (cartageTypesChanged != null)
			{
				cartageTypesChanged(this, e);
			}
		}

		#region Demurrage

		public TimeSpan Demurrage { get; set; }

		#endregion

		#region ICartageParent Members

		bool ICartageParent.RebuildLocalCartageMenuOnClick
		{
			get { return false; }
		}

		bool ICartageParent.UseJobTotals
		{
			get { return ICartageParentUseJobTotals; }
		}

		public bool ICartageParentUseJobTotals
		{
			get;
			set;
		}

		void ICartageParent.CartageCreatedAndSaved()
		{
			CartageCreatedCalled = true;
		}

		public bool CartageCreatedCalled;

		IStmALogParent ICartageParent.BusinessObjectForRelatedEvents
		{
			get { return this; }
		}

		IDocManagerSupport ICartageParent.BusinessObjectForRelatedEDocs
		{
			get { return this; }
		}

		#endregion

		#region IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return new DocManagerInfo(this, "DUM"); }
		}

		#endregion
	}
}
