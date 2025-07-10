using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	[CodeProperty("JobNumber")]
	public class MNRWorkOrderLine : AutoMNRWorkOrderLine,
		IEDocsProvider,
		IDocumentSupportable
	{
		public MNRWorkOrderLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("WorkOrderHeader")]
		public override ZGuid MWL_MWO_MNRWorkOrderHeader { get => base.MWL_MWO_MNRWorkOrderHeader; set => base.MWL_MWO_MNRWorkOrderHeader = value; }

		public MNRWorkOrderHeader WorkOrderHeader
		{
			get => Factory.Load<MNRWorkOrderHeader>(MWL_MWO_MNRWorkOrderHeader);
		}

		public ZString JobNumber { get { return PK.ToString(); } }

		public virtual ZBool ChargingByLength => MWL_Width == 0;

		public virtual ZDecimal Area => MWL_Length * MWL_Width;

		public virtual ZDecimal Perimeter => (MWL_Length + MWL_Width) * 2;

		public virtual ZDecimal LinearLength => MWL_Length;

		public virtual ZBool HasLaborHours => MWL_LaborHours > 0;

		#endregion

		#region Implementation

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			MWL_ResponsibleParty = "OWN";
			MWL_Description = "Test";
		}
#endif

		#endregion

		#region IEDocsProvider

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.MNRWorkOrderLine);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new MNRWorkOrderLineDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion
	}
}
