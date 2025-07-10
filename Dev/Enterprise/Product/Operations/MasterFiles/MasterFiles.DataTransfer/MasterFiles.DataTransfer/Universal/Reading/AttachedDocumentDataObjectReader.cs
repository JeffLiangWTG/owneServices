using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class AttachedDocumentDataObjectReader : IAttachedDocumentDataObjectReader
	{
		public bool TryAddAttachedDocument(IAttachedDocument attachedDocument, IXmlImportLogger logger, IDocManagerSupportBase parent, out IeDoc eDoc)
		{
			Argument.NotNull(parent, "IDocManagerSupport parent");
			Argument.NotNull(attachedDocument, "AttachedDocument attachedDocument");
			Argument.NotNull(logger, "IXmlImportLogger logger");

			eDoc = null;

			try
			{
				var docManagerInfo = parent.DocManagerInfoCore();
				if (docManagerInfo is ISimpleLoggerSupporter loggerSupporter)
				{
					loggerSupporter.Logger = logger;
				}

				if (attachedDocument.ImageData != null && attachedDocument.ImageData.Length > SystemDataRegistry.Instance.eDocsMaximumFilesize.Value * 1024 * 1024)
				{
					logger.Log(LogType.Warning, Res.GetString("0bd25bff-0c75-41d7-b811-05f52f69d41c", "The file size of '{0}' exceeds the {1}MB maximum file size allowed for eDocs.", attachedDocument.FileName, SystemDataRegistry.Instance.eDocsMaximumFilesize.Value));
					return false;
				}

				eDoc = AddAttachedDocumentCore(attachedDocument, logger, docManagerInfo);

				if (eDoc != null)
				{
					logger.Log(LogType.Information, Res.GetString("9c716809-28f1-43aa-ad59-500d1feaba37", "Successfully Added eDoc: {0}.", attachedDocument.FileName));
					SetupEDocsFactoryToBeSavedWithMainFactory(docManagerInfo, attachedDocument, (BusinessObject)parent);
				}

				return eDoc != null;
			}
			catch (ImageFormatException)
			{
				logger.Log(LogType.Warning, Res.GetString("0E59105B-6CDA-4643-BC8E-E84F6CDA7511", "Could not link Attached Document(file name: {0}) to {1}. The image data of this file is invalid.", attachedDocument.FileName, (parent as BusinessObject).HumanReadableName));
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static IeDoc AddAttachedDocumentCore(IAttachedDocument attachedDocument, IXmlImportLogger logger, IDocManagerInfoCore docManagerInfo)
		{
			docManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			IeDoc eDoc = null;

			if (!HasValidFileNameAndPath(attachedDocument))
			{
				throw new DataObjectReadFailureException(Res.GetString("C875CAFE-71AF-4AF7-86AC-1BCDAD258ACC", "Invalid file name and path: \"{0}\".", attachedDocument.FileName));
			}

			if (IsDangerousFile(attachedDocument))
			{
				throw new DataObjectReadFailureException(Res.GetString("1675356D-B62E-4156-9F3C-6F00E019FEE6", "File cannot be added because it has a potentially dangerous file type."));
			}

			if (attachedDocument.ImageData == null || attachedDocument.ImageData.Length == 0)
			{
				throw new DataObjectReadFailureException(Res.GetString("CE611B71-AE1A-4D17-B402-DB6CA05C7FC2", "File cannot be empty."));
			}

			if (logger.TopLevelDataContext != null)
			{
				EnterpriseServerAndCompanyID iDs = logger.TopLevelDataContext.GetEnterpriseServerAndCompanyIDs();
				var visibleCompanyPK = Guid.Empty;
				var visiblebranchPK = Guid.Empty;
				var visibleDepartmentPK = Guid.Empty;
				var factory = new BusinessObjectFactory();

				if (!string.IsNullOrEmpty(attachedDocument.VisibleCompanyCode) && attachedDocument.VisibleCompanyCode.HasValue)
				{
					var visibleCompany = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, attachedDocument.VisibleCompanyCode.Value);
					if (visibleCompany != null)
					{
						visibleCompanyPK = visibleCompany.PK.ToGuid();
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("AA17080D-C2BA-4A95-A769-D994E56192AA", "The visible company code '{0}' specified in XML is invalid.", attachedDocument.VisibleCompanyCode));
					}
				}
				if (!string.IsNullOrEmpty(attachedDocument.VisibleBranchCode) && attachedDocument.VisibleBranchCode.HasValue)
				{
					var visibleBranch = factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, attachedDocument.VisibleBranchCode.Value);
					if (visibleBranch != null)
					{
						visiblebranchPK = visibleBranch.PK.ToGuid();
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("401EF696-EB63-4524-A90E-134314A1F0BD", "The visible branch code '{0}' specified in XML is invalid.", attachedDocument.VisibleBranchCode));
					}
				}
				if (!string.IsNullOrEmpty(attachedDocument.VisibleDepartmentCode) && attachedDocument.VisibleDepartmentCode.HasValue)
				{
					var visibleDepartment = factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, attachedDocument.VisibleDepartmentCode.Value);
					if (visibleDepartment != null)
					{
						visibleDepartmentPK = visibleDepartment.PK.ToGuid();
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("4D2247E7-7082-4F3A-8D4D-0E192587A387", "The visible department code '{0}' specified in XML is invalid.", attachedDocument.VisibleDepartmentCode));
					}
				}
				eDoc = (IeDoc)docManagerInfo.AddFileOrDocumentCore(attachedDocument.ImageData, attachedDocument.FileName, attachedDocument.Type.Code, false, visibleCompanyPK, visiblebranchPK, visibleDepartmentPK, documentSource: attachedDocument.Source?.Code);
			}

			if (eDoc == null && logger.Logs.All(log => log.Type != LogType.Error))
			{
				eDoc = (IeDoc)docManagerInfo.AddFileOrDocumentCore(attachedDocument.ImageData, attachedDocument.FileName, attachedDocument.Type.Code);
			}

			if (eDoc != null)
			{
				if (attachedDocument.IsPublished.HasValue)
				{
					eDoc.IsPublished = attachedDocument.IsPublished.Value;
				}

				if (!string.IsNullOrEmpty(attachedDocument.Type.Description))
				{
					eDoc.Description = attachedDocument.Type.Description.Value;
				}

				if (eDoc.Description.IsEmpty)
				{
					eDoc.Description = eDoc.DocType;
				}
			}

			return eDoc;
		}

		static bool HasValidFileNameAndPath(IAttachedDocument document)
		{
			if (!document.FileName.HasValue || document.FileName.Value.IsEmpty)
			{
				return false;
			}

			foreach (var character in Path.GetInvalidFileNameChars())
			{
				if (document.FileName.Value.Contains(character))
				{
					return false;
				}
			}

			return true;
		}

		static bool IsDangerousFile(IAttachedDocument document)
		{
			return new FileTypeValidation().IsDangerousFile(document.FileName);
		}

		void SetupEDocsFactoryToBeSavedWithMainFactory(IDocManagerInfoCore docManagerInfo, IAttachedDocument attachedDocument, BusinessObject parent)
		{
			var mainFactory = parent.Factory;
			var eDocsFactory = (BusinessObjectFactory)docManagerInfo.MasterFactory;
			if (eDocsFactory.ChildFactories.Contains(mainFactory))
			{
				eDocsFactory.ChildFactories.Remove(mainFactory);
			}
			mainFactory.ChildFactories.Add(eDocsFactory);
			mainFactory.SubscribeForDispose(new DisposableAction(() =>
			{
				mainFactory.ChildFactories.Remove(eDocsFactory);
				attachedDocument.IsDisposedByParent = false;
				attachedDocument.Dispose();
			}));
			attachedDocument.IsDisposedByParent = true; //prevent disposal until after save
		}
	}
}
