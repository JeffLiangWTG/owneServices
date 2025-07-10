using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class DocumentTrackingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DocumentTracking; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return string.Empty; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewStandardMenuItems());
			result.Add(new ZMenuItem(ResString.GetMultilingualString("MenuItem.BulkUpdate", "Bulk Update"), OnBulkUpdate_Click));
			return result.ToArray();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.DocumentTracking);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DocumentTrackingFilterControl((JobRequiredDocumentCollection)GridCollection, (DocumentTrackingFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			JobRequiredDocumentCollection result = new JobRequiredDocumentCollection(Factory);
			result.ParentType = typeof(ForwardingDocsAndCartage);
			return result;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DocumentTrackingFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Forwarder; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ForwardingDocumentTracking; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		protected override FilteredGridLoader CreateSearchManager()
			=> new DocumentTrackingGridLoader(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

		class DocumentTrackingGridLoader : FilteredGridLoader
		{
			public DocumentTrackingGridLoader(FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
				: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
			{
			}

			public override BusinessObjectReader CreateReader(ZQuery query) => new JobRequiredDocumentReader(query, typeof(ForwardingDocsAndCartage));
		}

		#region Implementation

		void OnBulkUpdate_Click(object sender, EventArgs e)
		{
			DocumentTrackingBulkUpdateBusinessObject bO = Factory.New<DocumentTrackingBulkUpdateBusinessObject>();
			new DocumentTrackingBulkUpdateForm(bO).Show();
		}

		#endregion

		#region JobRequiredDocumentReader

		internal class JobRequiredDocumentReader : BusinessObjectListReader
		{
			public JobRequiredDocumentReader(ZQuery filter, Type parentType)
				: base(filter, typeof(JobRequiredDocument))
			{
				this.parentType = parentType;
			}
			readonly Type parentType;

			protected override BusinessObject[] LoadNextBatchCore(BusinessObject lastBusinessObjectRead)
			{
				var result = (JobRequiredDocument[])base.LoadNextBatchCore(lastBusinessObjectRead);
				if (result != null)
				{
					foreach (JobRequiredDocument doc in result)
					{
						doc.ParentType = parentType;
					}
				}
				return result;
			}
		}

		#endregion
	}
}
