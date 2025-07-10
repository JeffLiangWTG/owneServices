using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyContainer : CFSContainer, IDocumentSupportable, IOutturnLinkable, IOutturnProvider
	{
		#region Schema

		public new class Schema : CFSContainer.Schema
		{
			public const string TotalShipments = "TotalShipments";
			public const string ReceiptDate = "ReceiptDate";

			public const string CanadaCCNNumber = "CanadaCCNNumber";
			public const string CanadaPCNNumber = "CanadaPCNNumber";
		}

		#endregion

		public TallyContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		#region PackLines

		[ChildEditable(true)]
		public new TallyPackLineManyToManyCollection PackLines
		{
			get { return (TallyPackLineManyToManyCollection)base.PackLines; }
		}

		protected override PackLineManyToManyCollection GetNewPackLineCollection()
		{
			return new TallyPackLineManyToManyCollection(this);
		}

		#endregion

		#region Consol

		protected override CommonConsol LoadParentConsol()
		{
			return Factory.Load<PackUnpackLoadListConsol>(JC_JK);
		}

		#endregion

		#region Services

		[ChildEditable(true)]
		public new TallyServiceDependentCollection Services
		{
			get { return (TallyServiceDependentCollection)base.Services; }
		}

		protected override JobServiceDependentCollection GetNewServiceCollection()
		{
			return new TallyServiceDependentCollection(this, Factory);
		}

		#endregion

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JC_LCLUnpack = Env.Time.CurrentLocalDateTime;
		}

		public override void OnSaving()
		{
			EventManager unPackEventManager = new EventManager();
			unPackEventManager.AddAuditLogEvent(AutoEvents.ContainerTallied.Code, TableName, PK.ToGuid(), Env.Time.CurrentLocalDateTime, JC_ContainerJobID);

			base.OnSaving();
		}

		#endregion

		#region Property Overrides

		#region PackUnpackShipments

		[ChildEditable(true)]
		public new PackUnpackShipmentDependentCollection PackUnpackShipments
		{
			get { return (PackUnpackShipmentDependentCollection)base.PackUnpackShipments; }
		}

		protected override CFSShipmentDependentCollection GetNewPackUnpackShipmentDependentCollection()
		{
			PackUnpackShipmentDependentCollection collection = new PackUnpackShipmentDependentCollection(Factory, this);
			collection.ValidateTotalsAgainstPackLines = true;
			collection.AllowSurplusPacks = true;

			if (ChildEditableService.GetState(Factory) == ChildEditableServiceStates.TallyContainer)
			{
				RegisterEditableChildObject(collection);
			}

			return collection;
		}

		#endregion

		[ReadOnly(true)]
		public override ZGuid JC_OH_CFSClient
		{
			get { return base.JC_OH_CFSClient; }
			set { base.JC_OH_CFSClient = value; }
		}

		public override ZString JC_SealNum
		{
			get
			{
				return base.JC_SealNum;
			}
			set
			{
				base.JC_SealNum = value;
				UpdateLink();
			}
		}

		public override ZBool JC_IsSealOk
		{
			get
			{
				return base.JC_IsSealOk;
			}
			set
			{
				base.JC_IsSealOk = value;
				UpdateLink();
			}
		}

		#region JC_LCLUnpack

		public override ZDateTime JC_LCLUnpack
		{
			get { return base.JC_LCLUnpack; }
			set
			{
				base.JC_LCLUnpack = value;

				foreach (PackUnpackShipment shipment in PackUnpackShipments)
				{
					shipment.UnpackDate = value;
				}
			}
		}

		#endregion

		#endregion

		#region Calculated Properties

		#region TotalShipments

		public ZInt TotalShipments
		{
			get { return PackUnpackShipments.Count; }
		}

		public ZPropertyInfo TotalShipmentsInfo
		{
			get { return GetZPropertyInfo(Schema.TotalShipments); }
		}

		#endregion

		#region Receipt Date

		[BusinessObjectTestExclude()]
		public ZDateTime ReceiptDate
		{
			get
			{
				return link != null ? link.ReceiptDate : ZDateTime.Empty;
			}
			set
			{
				if (link != null)
				{
					link.ReceiptDate = value;
				}
				ReceiptDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ReceiptDateInfo
		{
			get { return GetZPropertyInfo(Schema.ReceiptDate); }
		}

		protected bool ReceiptDate_ReadOnly
		{
			get { return (link == null); }
		}

		#endregion

		#region Canada Specific

		public ZString CanadaCCNNumber
		{
			get
			{
				return this.Consol == null ? ZString.Empty : this.Consol.CanadaCCNNumber;
			}
		}

		public ZString CanadaPCNNumber
		{
			get
			{
				return this.Consol == null ? ZString.Empty : this.Consol.CanadaPCNNumber;
			}
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		protected override DocManagerInfo NewDocManagerInfo()
		{
			return new DocManagerInfo(this, Core.Constants.DocManagerCodes.TallyContainer);
		}

		#endregion

		#region IDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter
		{
			get { return new TallyContainerDocumentSupporter(this); }
		}

		#endregion

		#region IOutturnProvider Members

		IOutturn IOutturnProvider.GetOutturnFor(PackUnpackShipment shipment)
		{
			IOutturn result = null;
			if (link != null)
			{
				result = link.GetOutturnFor(shipment);
			}
			return result;
		}

		#endregion

		#region IOutturnLinkable Members

		void IOutturnLinkable.SetOutturnLink(IOutturnLink inputLink)
		{
			this.link = inputLink;
			UpdateLink();
			ReceiptDateInfo.RefreshBinding();
			foreach (PackUnpackShipment shipment in PackUnpackShipments)
			{
				shipment.NotifyCustomsListener();
			}
		}

		void UpdateLink()
		{
			if (link != null)
			{
				link.SetSealNumber(JC_SealNum);
				link.SetSealIntact(JC_IsSealOk);
			}
		}

		protected internal IOutturnLink link;

		#endregion

		#region Customs Outturn Link Test
#if DEBUG

		public bool IsLinkedToCustomsForTest
		{
			get { return link != null; }
		}
#endif
		#endregion

		#region Nil Outturn

		public void NilOutturn()
		{
			JC_IsSealOk = true;
			JC_LCLUnpack = ZDateTime.Now;
			ReceiptDate = ZDateTime.Now;

			foreach (PackUnpackShipment shipment in PackUnpackShipments)
			{
				foreach (PackLine line in shipment.OuterPackLines)
				{
					line.JL_Outturn = line.JL_PackageCount;
					line.JL_Damaged = 0;
					line.JL_Pillaged = 0;
				}
			}
		}

		#endregion
	}
}
