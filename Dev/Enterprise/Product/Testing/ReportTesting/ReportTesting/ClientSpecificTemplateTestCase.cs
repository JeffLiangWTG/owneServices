using System;
using System.IO;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DbUpgrader.Data;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting
{
	public abstract class ClientSpecificTemplateTestCase : TemplateTestCase
	{
		public override DocumentEngine.Business.StmTemplateBaseCollection CandidateTemplates
		{
			get
			{
				if (base.CandidateTemplates.Count != 1 && !clientSpecificDocsLoaded)
				{
					LoadClientSpecificDocuments();
					ResetCandidateTemplates();
				}
				return base.CandidateTemplates;
			}
		}

		protected override ZQuery AdditionalTemplateFilters
		{
			get
			{
				return new ZQuery(StmTemplateSchema.SO_IsClientSpecific, true);
			}
		}

		protected abstract Clients ClientCode { get; }

		public override string TemplateFolder
		{
			get
			{
				return base.TemplateFolder + ClientCode.ToString() + @"\";
			}
		}

		internal void LoadClientSpecificDocuments()
		{
			if (!clientSpecificDocsLoaded)
			{
				using var locator = new ClientSpecificDocumentsEmbeddedResourceLocator();
				var documentsXmlFile = locator.ExtractClientDocumentXmlEmbeddedResourceToTempFile(ClientCode.ToString());
				AssertEquals(documentsXmlFile + " should exist", true, File.Exists(documentsXmlFile));
				ClientDocumentsUpgradeTask task = new ClientDocumentsUpgradeTask(documentsXmlFile);
				task.Run();
				clientSpecificDocsLoaded = true;
			}
		}
		bool clientSpecificDocsLoaded;

		protected override void OnBeforeBaseTestCaseRunBare()
		{
			overridenClientAssembly = ClientHookLoader.Instance.OverrideClientAssemblyForTest(ClientCode);
			base.OnBeforeBaseTestCaseRunBare();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (overridenClientAssembly != null)
			{
				var toDispose = overridenClientAssembly;
				overridenClientAssembly = null;
				toDispose.Dispose();
			}
		}

		IDisposable overridenClientAssembly;
	}
}
