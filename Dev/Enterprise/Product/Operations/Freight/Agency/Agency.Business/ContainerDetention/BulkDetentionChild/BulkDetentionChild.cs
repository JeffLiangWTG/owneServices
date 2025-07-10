using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkDetentionChild : AutoBulkDetentionChild
	{
		public BulkDetentionChild(BusinessObjectFactory factory)
			: base(factory)
		{
			MovementPKs = new List<ZGuid>();
		}

		[List("Lookups.Principals")]
		public override ZGuid PrincipalPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PrincipalPK; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PrincipalPK = value; }
		}

		[List("Lookups.DetentionTypes")]
		public override ZString DetentionType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.DetentionType; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.DetentionType = value; }
		}

		[List("Lookups.Clients")]
		public override ZGuid ClientPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.ClientPK; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.ClientPK = value; }
		}

		[List("Lookups.Countries")]
		public override ZString CountryCode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.CountryCode; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.CountryCode = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsLocked")]
		[ReadOnlyMember("IsLocked")]
		public override ZBool IsSelected
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.IsSelected; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.IsSelected = value; }
		}

		public override ZInt ContainerCount
		{
			get { return MovementPKs.Count; }
		}

		public OrgHeader Client
		{
			get { return Factory.Load<OrgHeader>(ClientPK); }
		}

		public OrgHeader Principal
		{
			get { return Factory.Load<OrgHeader>(PrincipalPK); }
		}

		public List<ZGuid> MovementPKs { get; private set; }

		public BulkDetentionChildLookups Lookups
		{
			get { return lookups ?? (lookups = new BulkDetentionChildLookups(this)); }
		}
		BulkDetentionChildLookups lookups;

		public void CreateDetentionInvoice(BusinessObjectFactory createFactory)
		{
			ContainerDetention newDetention = createFactory.New<ContainerDetention>();
			newDetention.NC_OH_Client = ClientPK;
			newDetention.NC_OH_Principal = PrincipalPK;
			newDetention.NC_DetentionType = DetentionType;

			ContainerMovement[] movements = createFactory.Load<ContainerMovement>(new ZQuery(JobContainerMoveSchema.PK, MovementPKs));

			if (movements.Length == 0)
			{
				newDetention.Delete();
				DetentionPK = ZGuid.Empty;
			}
			else
			{
				newDetention.Movements.AddRange(movements);
				DetentionPK = newDetention.PK;

				if (ObjectFactory.Get<IAccounting>().ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(newDetention))
				{
					JobHeader header = new JobHeader.Loader(newDetention).TryCreate();

					if (header.JH_GB.IsEmpty)
					{
						header.JH_GB = GlbBranch.CurrentBranch.PK;
					}

					if (header.JH_GE.IsEmpty)
					{
						header.JH_GE = GlbDepartment.CurrentDepartment.PK;
					}
				}
			}
		}
	}
}


