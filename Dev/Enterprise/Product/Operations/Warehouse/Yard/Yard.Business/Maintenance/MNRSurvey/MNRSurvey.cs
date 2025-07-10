using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	[CodeProperty("JobNumber")]
	public class MNRSurvey : AutoMNRSurvey, IStmNoteParent, IDocumentSupportable, IDocManagerSupport, IEDocsProvider
	{
		public MNRSurvey(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		public CYDYardUnitState YardUnitState
		{
			get => Factory.Load<CYDYardUnitState>(MRS_ParentID);
		}

		[RelatedBusinessObject("Facility")]
		public override ZGuid MRS_WW_Facility { get => base.MRS_WW_Facility; set => base.MRS_WW_Facility = value; }

		public WhsWarehouse Facility
		{
			get => Factory.Load<WhsWarehouse>(MRS_WW_Facility);
		}

		public ZString JobNumber { get { return PK.ToString(); } }

		#endregion

		#region IStmNoteParent

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var noteTypes = base.NoteTypesCore;
				noteTypes.Add(PredefinedNoteTypes.Instance.SurveyInstruction);
				noteTypes.Add(PredefinedNoteTypes.Instance.ContainerComment);

				return noteTypes;
			}
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("b2161555-05b0-496a-b83e-b7b413f4da36", "Survey");
			}
		}

		#endregion

		#region Implementation

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			MRS_WW_Facility = Factory.NewWithValidTestData<WhsWarehouse>().PK;
			MRS_ParentTableCode = "YUS";
		}
#endif

		#endregion

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.MNRSurvey);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEdocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new MNRSurveyDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion
	}
}
