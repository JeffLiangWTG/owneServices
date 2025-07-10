using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Tracking.Web
{
	public class ReportRequestHandler : DataRequestHandler<ReportRequestHelper>
	{
		protected override BusinessObject[] GetNewBusinessObjects()
		{
			var result = new List<Report>();

			if (PKs.Length > 0 && PKs[0].IsValid)
			{
				var report = HttpContext.Current.Session[PKs[0].ToString()] as Report;

				if (report != null)
				{
					result.Add(report);
				}
			}

			return result.ToArray();
		}

		public override string ContentType
		{
			get
			{
				var defaultContentType = DataContentTypes.Pdf;

				if (BizOAsReport != null)
				{
					switch (BizOAsReport.SelectedFormatType)
					{
						case OrgConstants.AttachmentType.XLS:
						case OrgConstants.AttachmentType.XLSX:
							return DataContentTypes.Excel;

						case OrgConstants.AttachmentType.TIF:
							return DataContentTypes.Tiff;

						case OrgConstants.AttachmentType.PDF:
						case OrgConstants.AttachmentType.PDFA:
							return DataContentTypes.Pdf;

						default:
							return defaultContentType;
					}
				}

				return defaultContentType;
			}
		}

		public override string FileName
		{
			get { return BizOAsReport != null ? string.Format("{0}.{1}", ((IDocument)BizOAsReport).DocumentName, DocumentConverter.GetFileExtensionFromAttachmentFormat(BizOAsReport.SelectedFormatType)) : string.Empty; }
		}

		Report BizOAsReport
		{
			get
			{
#if DEBUG
				if (Globals.IsTest && BusinessObjects[0] == null)
				{
					return null;
				}
#endif
				if (bizOAsReport == null && BusinessObjects.Length > 0)
				{
					bizOAsReport = BusinessObjects[0] as Report;
				}

				return bizOAsReport;
			}
		}
		Report bizOAsReport;

		public override ZBlob GetBinaryData()
		{
			byte[] binaryData = null;
			bool isLocalDocument = false;

			if (BizOAsReport != null)
			{
				lock (BizOAsReport) // Issue 00211803 - WI00061453
				{
#if DEBUG
					StopStopwatchForTesting();
#endif
					using (var outputStream = new MemoryStream())
					{
						var currentUser = HttpContext.Current.Session["SiteUser"] as OrgContactWebUser;
						if (currentUser != null && currentUser.IsLoggedIn)
						{
							GlbBranch branch = GetReportConfigurationLoginBranch(currentUser.LoggedInOrganisation);
							if (branch != null)
							{
								using (var tempBranch = new WebLoginBranch(branch))
								{
									BizOAsReport.Save(new DocDeliveryContact(Factory) { OrgHeaderPK = currentUser.LoggedInOrganisation.PK, Name = currentUser.LoggedInUserName, AttachmentType = BizOAsReport.SelectedFormatType }, null, outputStream);
								}
							}
							else
							{
								BizOAsReport.Save(new DocDeliveryContact(Factory) { OrgHeaderPK = currentUser.LoggedInOrganisation.PK, Name = currentUser.LoggedInUserName, AttachmentType = BizOAsReport.SelectedFormatType }, null, outputStream);
							}
						}
						if (outputStream.Length > 0)
						{
							binaryData = outputStream.ToArray();
						}
					}

					if (BizOAsReport.MenuItem != null)
					{
						isLocalDocument = BizOAsReport.MenuItem.IsLocalDocument;
					}
				}
			}

			if (binaryData == null)
			{
				return binaryData;
			}

			var selectedFormatWithDefault = BizOAsReport != null ? BizOAsReport.SelectedFormatType.ToString() : OrgConstants.AttachmentType.PDF;

			switch (selectedFormatWithDefault)
			{
				case OrgConstants.AttachmentType.XLS:
				case OrgConstants.AttachmentType.XLSX:
					return binaryData;

				case OrgConstants.AttachmentType.TIF:
					return DocumentConverter.ConvertFromExcel(binaryData, OutputFormatType.TIF, ColourDepth.Colour256, isLocalDocument);

				case OrgConstants.AttachmentType.PDFA:
					return DocumentConverter.ConvertFromExcel(binaryData, OutputFormatType.PDFA, ColourDepth.BlackAndWhite, isLocalDocument);

				case OrgConstants.AttachmentType.PDF:
				default:
					return DocumentConverter.ConvertFromExcel(binaryData, OutputFormatType.PDF, ColourDepth.BlackAndWhite, isLocalDocument);
			}
		}

		GlbBranch GetReportConfigurationLoginBranch(OrgHeader webUserOrg)
		{
			GlbBranch result = null;

			string companyCode = GetCompanyCodeFromSelectedConfiguration();

			if (!string.IsNullOrEmpty(companyCode) && companyCode.ToUpper() != GlbCompany.CurrentCompany.GC_Code.ToUpper())
			{
				var company = GlbCompany.CurrentCompany.Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
				if (company != null)
				{
					if (webUserOrg != null)
					{
						GlbBranch controllingBranch = GlbBranch.FindControllingBranchWithFallBackToAnyCompany(webUserOrg);
						if (controllingBranch != null)
						{
							if (controllingBranch.GB_GC == company.PK)
							{
								result = controllingBranch;
							}
						}
					}

					if (result == null)
					{
						foreach (GlbBranch branch in company.Branches)
						{
							if (branch.GB_IsActive)
							{
								result = branch;
								break;
							}
						}
					}
				}
			}

			return result;
		}

		string GetCompanyCodeFromSelectedConfiguration()
		{
			string result = string.Empty;
			ColumnConfigurationManager[] savedConfigurations = null;

			foreach (IFilter filter in BizOAsReport.FilterCollection)
			{
				if (filter is ColumnConfigurationField field)
				{
					savedConfigurations = field.SavedConfigurations.ToArray();
					break;
				}
			}

			if (savedConfigurations != null)
			{
				foreach (ColumnConfigurationManager config in savedConfigurations)
				{
					if (config != null && config.Description == BizOAsReport.SelectedColumnLayout)
					{
						CombinedConfigurationManager configManager = config as CombinedConfigurationManager;
						if (configManager != null)
						{
							result = configManager.CompanyCode;
							break;
						}
					}
				}
			}

			return result;
		}

		public override string NoDataErrorMessage
		{
			get { return Res.GetString("46a45597-e6ec-45ab-9610-47f1f8d912d9", "The report was not found in the current web session."); }
		}
	}
}
