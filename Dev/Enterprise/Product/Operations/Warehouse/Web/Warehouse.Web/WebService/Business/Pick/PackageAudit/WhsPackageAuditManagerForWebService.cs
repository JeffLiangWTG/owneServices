namespace Enterprise.Warehouse.Web.WebService
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using Business;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Core;
	using MasterFiles.Business;
	using NumberFountain;
	using Packing.Business;
	using Transactions.Business;
	using ZArchitecture.Business;
	using ZArchitecture.Schema;
	using Constants = Core.Constants;

	public static class WhsPackageAuditManagerForWebServices
	{
		#region VerifyPackageForAuditForReference

		public static void VerifyPackageForAuditForReference(BusinessObjectFactory factory, PackageWebServiceResponse response, ZString packageID)
		{
			var result = LoadPackagesForID(factory, packageID);
			var numberOfPackagesLoaded = result.Count;
			if (numberOfPackagesLoaded == 1)
			{
				CheckAuditRequisitesForPackage(result.First(), response);
			}
			else if (numberOfPackagesLoaded == 0)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("2b292757-d42e-4b0c-8f69-8cb72ccb4dee", "The Package {0} has not been found.", packageID);
			}

			if (!string.IsNullOrEmpty(response.ErrorMessage))
			{
				response.Error = ErrorTypes.BusinessValidationError;
			}
			response.PackageChoices = new PackageChoiceInfoCollection(result.ToArray());
		}

		#endregion

		#region VerifyPackageForAuditForPK

		public static void VerifyPackageForAuditForPK(BusinessObjectFactory factory, PackageWebServiceResponse response, ZGuid pK)
		{
			var result = factory.Load<PkgPackage>(pK);
			if (result != null)
			{
				CheckAuditRequisitesForPackage(result, response);
				if (string.IsNullOrEmpty(response.ErrorMessage))
				{
					response.PackageChoices = new PackageChoiceInfoCollection(new[] { result });
				}
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("e146f27d-bc6b-442d-b1da-b892669ff0cf", "The Package has not been found.");
			}

			if (!string.IsNullOrEmpty(response.ErrorMessage))
			{
				response.Error = ErrorTypes.BusinessValidationError;
			}
		}

		#endregion

		#region PerformPackageContentsAudit

		public static void PerformPackageContentsAudit(BusinessObjectFactory factory, WebServiceResponse response, WhsPackageProductInfo[] auditedPackageContent, Guid packagePK, GlbStaff staff)
		{
			var package = factory.Load<PkgPackage>(packagePK);
			if (package != null && package.PackageJob.KJ_ParentTableCode == WhsDocketSchema.Constants.Prefix)
			{
				var auditHeader = CreateWhsPackageAuditRow(factory, staff, package);
				if (auditHeader != null && !auditHeader.HasErrors)
				{
					var expectedPackageContent = Business.PackageHelper.GetPackageRelatedProductInfos(factory, package);
					var failureLines = GetAuditVariance(expectedPackageContent, auditedPackageContent);

					if (failureLines.Length > 0)
					{
						var success = CreateWhsPackageAuditFailureLines(auditHeader, failureLines);
						if (!success)
						{
							LogResponse(response, ErrorTypes.BusinessValidationError, PackageAuditManagerStrings.Error_FailureLines);
						}
						else if (TrySetPackageHoldStatus(package, PackageHoldEvents.Hold))
						{
							LogResponse(response, ErrorTypes.Information, PackageAuditManagerStrings.Success_Variance);
							LogPackageAuditEvent(auditHeader, AutoEvents.Held);
						}
						else
						{
							LogResponse(response, ErrorTypes.BusinessValidationError, PackageAuditManagerStrings.Error_HoldPackage);
						}
					}
					else if (package.KP_IsHeld && PackageWasHeldByAFailedAudit(auditHeader.Order))
					{
						if (TrySetPackageHoldStatus(package, PackageHoldEvents.ClearHold))
						{
							LogPackageAuditEvent(auditHeader, AutoEvents.ClearedHold);
						}
						else
						{
							LogResponse(response, ErrorTypes.BusinessValidationError, PackageAuditManagerStrings.Error_HoldPackage);
						}
					}
					else
					{
						LogPackageAuditEvent(auditHeader, AutoEvents.RecordAudited);
					}
				}
				else
				{
					LogResponse(response, ErrorTypes.BusinessValidationError, PackageAuditManagerStrings.Error_AuditHeader);
				}
			}
			else
			{
				LogResponse(response, ErrorTypes.BusinessValidationError, PackageAuditManagerStrings.Error_PackageOrderNotFound);
			}

			if (response.Error == ErrorTypes.None || response.Error == ErrorTypes.Information)
			{
				factory.Save();
			}
		}

		#endregion

		#region GetAuditVariance

		static WhsPackageProductInfo[] GetAuditVariance(WhsPackageProductInfo[] expectedPackageContent, WhsPackageProductInfo[] auditedPackageContent)
		{
			var all = new List<WhsPackageProductInfo>(auditedPackageContent);
			foreach (var expectedPackage in expectedPackageContent)
			{
				var package = all.FirstOrDefault(l => l.ProductPK == expectedPackage.ProductPK);
				if (package == null)
				{
					package = expectedPackage;
					all.Add(package);
				}
				else
				{
					package.ExpectedQty = expectedPackage.ExpectedQty;
				}
			}

			return all.Where(l => l.Quantity != l.ExpectedQty).ToArray();
		}

		#endregion

		#region CreateWhsPackageAudit

		static WhsPackageAudit CreateWhsPackageAuditRow(BusinessObjectFactory factory, GlbStaff staff, PkgPackage package)
		{
			var newWhsPackageAudit = factory.New<WhsPackageAudit>();
			newWhsPackageAudit.WPA_WD_Order = package.PackageJob.KJ_ParentID;
			newWhsPackageAudit.WPA_GS_NKAuditor = staff.GS_Code;
			newWhsPackageAudit.WPA_PackageID = package.KP_PackageID;
			newWhsPackageAudit.WPA_AuditCompleteTime = ZDateTimeOffset.Now;

			newWhsPackageAudit.RunPreSaveValidation();
			return newWhsPackageAudit;
		}

		static bool CreateWhsPackageAuditFailureLines(WhsPackageAudit auditHeader, WhsPackageProductInfo[] failureLines)
		{
			foreach (var failureLine in failureLines)
			{
				var newAuditLine = auditHeader.PackageAuditFailureLines.AddNew();
				newAuditLine.WPF_OP = failureLine.ProductPK;
				newAuditLine.WPF_ExpectedQty = failureLine.ExpectedQty;
				newAuditLine.WPF_AuditedQty = failureLine.Quantity;

				newAuditLine.RunPreSaveValidation();
				if (newAuditLine.HasErrors)
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region VerifyPackageForPreviousAudit

		public static void VerifyPackageForPreviousAudit(BusinessObjectFactory factory, WebServiceResponse response, Guid packagePK)
		{
			var package = factory.Load<PkgPackage>(packagePK);
			var packageJob = package?.PackageJob;

			if (packageJob != null && packageJob.KJ_ParentTableCode == WhsDocketSchema.Constants.Prefix)
			{
				var audit = LoadMostRecentAudit(factory, package);
				if (audit != null)
				{
					response.Error = ErrorTypes.YesNoEnquiry;

					var failureLinesQuery = new ZQuery(WhsPackageAuditLineFailureSchema.WPF_WPA_WhsPackageAudit, audit.PK);
					if (factory.LoadTop1<WhsPackageAuditLineFailure>(failureLinesQuery) != null)
					{
						response.ErrorMessage = PackageAuditManagerStrings.YesNoEnquiry_AuditedFailed;
					}
					else
					{
						response.ErrorMessage = PackageAuditManagerStrings.YesNoEnquiry_AuditedPassed;
					}
				}
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = PackageAuditManagerStrings.Error_PackageOrderNotFound;
			}
		}

		#endregion

		#region TrySetPackageHoldStatus

		static bool TrySetPackageHoldStatus(PkgPackage package, PackageHoldEvents holdEvent)
		{
			var newHoldStatus = (holdEvent == PackageHoldEvents.Hold);
			if (!package.PackageJob.KJ_IsFinalized && package.KP_IsHeld != newHoldStatus)
			{
				package.KP_IsHeld = newHoldStatus;
				package.RunPreSaveValidation();
			}
			return !package.HasErrors && !package.PackageJob.KJ_IsFinalized;
		}

		#endregion

		#region LogPackageAuditEvent

		static void LogPackageAuditEvent(WhsPackageAudit audit, Event newEvent)
		{
			var isHeld = newEvent == AutoEvents.Held;
			var eventReason = isHeld ? Constants.EventReferenceParameterReasons.AuditFailed : Constants.EventReferenceParameterReasons.AuditPassed;
			var freeTextReference = GetFreeTextReferenceForEvent(isHeld, audit);

			audit.Order.Logs.AddNew(newEvent, freeTextReference, new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, Constants.Departments.Warehouse),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, eventReason),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, audit.Order.Warehouse.WW_WarehouseNameMultilingual)
			});
		}

		#endregion

		#region GetFreeTextReferenceForEvent

		static string GetFreeTextReferenceForEvent(bool isHeld, WhsPackageAudit audit)
		{
			var textReference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Package: {0}", audit.WPA_PackageID); // This is log information.
			if (isHeld)
			{
				if (audit.PackageAuditFailureLines.Count == 1)
				{
					textReference += string.Format(CultureInfo.InvariantCulture, (NoResString)", Product: {0}", audit.PackageAuditFailureLines.Single().SupplierPart?.OP_PartNum); // This is log information.
				}
				else
				{
					textReference += (NoResString)", Product: MULTIPLE"; // This is log information.
				}
			}
			return textReference;
		}

		#endregion

		#region PackageWasHeldByAFailedAudit

		static bool PackageWasHeldByAFailedAudit(WhsDocket order)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.Held.Code);
			query.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, AutoEvents.ClearedHold.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, order.PK);
			query.OrderBy = StmALogSchema.Constants.SL_EventTime + OrderByClause.Descending;

			var reasonCode = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason;
			var eventLog = order.Factory.LoadTop1<StmALog>(query);
			return (eventLog != null && eventLog.Parameters.ContainsKey(reasonCode) && eventLog.Parameters[reasonCode] == Constants.EventReferenceParameterReasons.AuditFailed);
		}

		#endregion

		#region LoadMostRecentAudit

		static WhsPackageAudit LoadMostRecentAudit(BusinessObjectFactory factory, PkgPackage package)
		{
			var packageJob = package?.PackageJob;
			WhsPackageAudit audit = null;

			if (packageJob != null && packageJob.KJ_ParentTableCode == WhsDocketSchema.Constants.Prefix)
			{
				var order = package.PackageJob.ParentJob;
				var auditHeaderQuery = new ZQuery(WhsPackageAuditSchema.WPA_WD_Order, order.PK);
				auditHeaderQuery.AddToFilter(WhsPackageAuditSchema.WPA_PackageID, package.KP_PackageID);
				auditHeaderQuery.OrderBy = WhsPackageAuditSchema.Constants.WPA_AuditCompleteTime + OrderByClause.Descending;
				audit = factory.LoadTop1<WhsPackageAudit>(auditHeaderQuery);
			}
			return audit;
		}

		#endregion

		#region LoadPackagesForID

		static ICollection<PkgPackage> LoadPackagesForID(BusinessObjectFactory factory, ZString packageID)
		{
			var barcodeToSearchFor = packageID;
			var ssccBarCode = SSCCBarCodeChecker.GetSSCCFromRawBarcode(barcodeToSearchFor);
			if (!string.IsNullOrEmpty(ssccBarCode))
			{
				barcodeToSearchFor = ssccBarCode;
			}

			// Package Id
			var packageIDSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			packageIDSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, barcodeToSearchFor);

			var query = new ZDBOnlyQuery(typeof(PkgPackage));
			query.AddSubQuery(packageIDSubQuery, JoinCondition.And);

			return factory.Load<PkgPackage>(query);
		}

		#endregion

		#region CheckAuditRequisitesForPackage

		static void CheckAuditRequisitesForPackage(PkgPackage package, PackageWebServiceResponse response)
		{
			if (package.IsOuter || package.ParentPackage.IsContainer)
			{
				if (package.PackageJob?.ParentJob as WhsOrder != null)
				{
					var packedItems = package.PackedItemDivots.Select(d => d.PackedItem);

					foreach (var pickLine in packedItems.Cast<WhsPickLine>())
					{
						if (pickLine == null || !pickLine.IsPickedFromPutawayLocation)
						{
							response.Error = ErrorTypes.BusinessValidationError;
							response.ErrorMessage = Res.GetString("93c4dcc5-0501-4a8b-8e3e-f5e351c3e837", "The Package {0} has not been completely picked.", package.KP_PackageID);
						}
					}
				}
				else
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("9b4ffa3f-26a4-44c3-b262-1f6ce5670d31", "The Package {0} does not belong to an order.", package.KP_PackageID);
				}
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("6c12ff27-0907-43bc-831f-1963c2a787f7", "The Package {0} is not an outer or a first level Package so it can't be audited.", package.KP_PackageID);
			}
		}

		#endregion

		#region LogErrors

		static void LogResponse(WebServiceResponse response, ErrorTypes errorType, string errorMessage)
		{
			if (errorType != ErrorTypes.None && !string.IsNullOrEmpty(errorMessage))
			{
				response.Error = errorType;
				response.ErrorMessage = errorMessage;
			}
		}

		#endregion

		#region PackageAuditManagerStrings

		public static class PackageAuditManagerStrings
		{
			public static string Error_PackageOrderNotFound
			{
				get { return Res.GetString("0f7c2239-e36a-4e40-83c9-2d4a386180ec", "Package/Order were not found."); }
			}

			public static string Error_AuditHeader
			{
				get { return Res.GetString("eca81097-cab7-42b8-9bb3-26123c80a9f9", "An error occurred while attempting to create the audit header."); }
			}

			public static string Error_FailureLines
			{
				get { return Res.GetString("0ed6dc89-1c29-43b0-8af9-df8553a2e07e", "Error trying to add audit failure information."); }
			}

			public static string Error_HoldPackage
			{
				get { return Res.GetString("32e05d4e-df88-4627-a4de-0ea847019e67", "Error when trying to update the hold on package."); }
			}

			public static string Success_Variance
			{
				get { return Res.GetString("1dd262aa-d10b-48ea-9776-0c51fa713651", "Audit completed. Variance exists."); }
			}

			public static string YesNoEnquiry_AuditedFailed
			{
				get { return Res.GetString("fa00ef14-b040-4e6d-8385-e0d5245db68c", "This package has already been audited and failed. Do you want to re-audit?"); }
			}

			public static string YesNoEnquiry_AuditedPassed
			{
				get { return Res.GetString("aca2311b-eeb0-4c00-8857-aa003e3c3353", "This package has already been audited and passed. Do you want to re-audit?"); }
			}
		}

		#endregion

		#region Enums

		public enum PackageHoldEvents
		{
			Hold,
			ClearHold,
			Successful
		}

		#endregion
	}
}
