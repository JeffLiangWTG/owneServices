using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class BaseCusStatementHeader : AutoCusStatementHeader, Integration.Customs.Shared.IBaseCusStatementHeader, IWorkflowProvider
	{
		protected BaseCusStatementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly BaseCusStatementHeaderTypeDecider TypeDecider = new BaseCusStatementHeaderTypeDecider();

		#endregion

		#region IWorkflowProvider

		ZString IWorkflowProviderCore.WorkflowType => GetWorkflowTypeCore();

		protected virtual ZString GetWorkflowTypeCore()
		{
			return StatementProcessTask.StatementWorkflow.Code;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return GetTemplateSelectionCriteriaCore();
		}

		protected virtual IColumnValueRanker GetTemplateSelectionCriteriaCore()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder().ToArray());
			return result;
		}

		protected virtual IEnumerable<IZType> GetClientsInTemplateSelectionOrder()
		{
			if (Importer != null)
			{
				yield return B2_OH_Importer;
			}
			yield return ZGuid.Empty;
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
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

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollectionWithKey(WorkflowItemCollectionKey, GetWorkflowItemsCore);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual string WorkflowItemCollectionKey => nameof(BaseCusStatementHeader);

		protected virtual ProcessTaskCollection GetWorkflowItemsCore()
		{
			return new StatementProcessTaskCollection(this);
		}

		protected void ReloadWorkflowItems()
		{
			workflowItems = null;
		}

		public Type ProcessTaskType => WorkflowItems.TypeOfElements;

		#endregion

		public ZString CountryCode
		{
			get
			{
				var company = Company;
				return (company != null) ? company.GC_RN_NKCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
		}

		#region Override

		public override void Delete()
		{
			if (!IsDeleted)
			{
				WorkflowItems.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B2_GC = GlbCompany.CurrentCompany.PK;
			B2_StatementType = "U";
		}

		#endregion
	}

	public class BaseCusStatementHeaderTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusStatementHeaderCountryCode(row, factory));

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Integration.Customs.US.ICusStatementHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Integration.Customs.CA.ICusStatementHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.KoreaSouth, delegate { return ObjectFactory.GetType<Integration.Customs.KR.ICusStatementHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.France, delegate { return ObjectFactory.GetType<Integration.Customs.FR.ICusStatementHeader>(); }),
				new CountrySpecificType(Core.Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Integration.Customs.TR.ICusStatementHeader>(); })
			};

		protected override Type DefaultTypeForUnsupportedCountry => ObjectFactory.GetType<Integration.Customs.US.ICusStatementHeader>();

		protected ZString GetCusStatementHeaderCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var companyPK = (row != null) ? new ZGuid(row[CusStatementHeaderSchema.B2_GC.Name]) : ZGuid.Invalid;
			var company = (companyPK.IsValid) ? factory.Load<GlbCompany>(companyPK) : null;
			return (company != null) ? company.GC_RN_NKCountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
