using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocumentDependentCollection : DependentBusinessObjectCollection<JobRequiredDocument, BusinessObject>
	{
		public JobRequiredDocumentDependentCollection(BusinessObject parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			this.Parent = (IHaveRequiredDocuments)parent;
		}

		public JobRequiredDocument AddNew(ZString docType)
		{
			JobRequiredDocument result = AddNew();
			result.EQ_DocType = docType;
			return result;
		}

		public void SetAllDocumentsReceivedEventLogger()
		{
			if (!logEventAlreadyOnParent)
			{
				JobRequiredDocumentAllDocumentsReceivedEventLogger.LogEventOnParentsOnSave(Parent);
				logEventAlreadyOnParent = true;
			}
		}

		bool logEventAlreadyOnParent;

		#region Overrides

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobRequiredDocumentSchema.EQ_ParentID; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			JobRequiredDocument reqDoc = (JobRequiredDocument)child;
			reqDoc.ParentType = Master.GetType();
			base.SetCollectionRelationships(reqDoc);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			JobRequiredDocument reqDoc = (JobRequiredDocument)bizOAdded;
			reqDoc.ParentType = Master.GetType();
			base.OnAdded(reqDoc);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (Count > 0)
			{
				JobRequiredDocument.LogEventOnParentsOnSave(Parent);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var jobRequiredDocument = elementToDelete as JobRequiredDocument;
			jobRequiredDocument?.TryAddDeleteLog();
			base.RemoveAndDelete(elementToDelete);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			JobRequiredDocument reqDoc = (JobRequiredDocument)child;
			reqDoc.EQ_DocCategory = reqDoc.Lookups.FormReferenceType;
		}

		#endregion

		#region Helper methods

		public JobRequiredDocument GetDocByType(ZString docType, string relatedCountry, Predicate<JobRequiredDocument> attributeMatch)
		{
			JobRequiredDocument result = null;
			foreach (JobRequiredDocument doc in GetElementsForTypeSortedByValidDate(docType))
			{
				if (doc.EQ_DocType == docType && (attributeMatch == null || attributeMatch(doc)) && (relatedCountry == null || doc.EQ_RN_NKRelatedCountry.IsEmpty || doc.EQ_RN_NKRelatedCountry == relatedCountry))
				{
					result = doc;
					break;
				}
			}
			return result;
		}

		public JobRequiredDocument GetDocByType(ZString docType, string relatedCountry)
		{
			return GetDocByType(docType, relatedCountry, null);
		}

		public JobRequiredDocument GetDocByType(ZString docType)
		{
			return GetDocByType(docType, null);
		}

		public bool IsDocRequired(ZString docType)
		{
			foreach (JobRequiredDocument doc in this)
			{
				if (doc.EQ_DocType == docType)
				{
					return true;
				}
			}

			return false;
		}

		public bool IsDocReceived(ZString docType)
		{
			foreach (JobRequiredDocument doc in this)
			{
				if (doc.EQ_DocType == docType)
				{
					if (!doc.EQ_DateReceived.IsEmpty)
					{
						return true;
					}
					break;
				}
			}

			return false;
		}

		public void ToggleDocReceived(ZString docType, bool isReceived)
		{
			if (isReceived)
			{
				JobRequiredDocument doc = AddIfNotExists(docType, JobRequiredDocument.DocUsage.Both);
				if (doc.EQ_DateReceived.IsEmpty)
				{
					doc.EQ_DateReceived = ZDateTimeOffset.Now;
				}
			}
			else
			{
				foreach (JobRequiredDocument doc in this)
				{
					if (doc.EQ_DocType == docType)
					{
						doc.EQ_DateReceived = ZDateTimeOffset.Empty;
						break;
					}
				}
			}
		}

		public JobRequiredDocument AddIfNotExists(ZString docType, ZString directionType)
		{
			JobRequiredDocument result = null;

			if (!IsDocRequired(docType))
			{
				result = AddNew();
				result.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
				result.EQ_DocType = docType;
				result.EQ_DocUsage = directionType;
			}
			else
			{
				foreach (JobRequiredDocument doc in this)
				{
					if (doc.EQ_DocType == docType)
					{
						result = doc;
						break;
					}
				}
			}

			return result;
		}

		public void RemoveIfExists(ZString docType)
		{
			for (int i = 0; i < Count; i++)
			{
				JobRequiredDocument doc = this[i];
				if (doc.EQ_DocType == docType)
				{
					RemoveAndDelete(doc);
					break;
				}
			}
		}

		public void CopyToOtherCollection(JobRequiredDocumentDependentCollection other)
		{
			foreach (JobRequiredDocument doc in this)
			{
				if (!other.IsDocRequired(doc.EQ_DocType))
				{
					var newDoc = Factory.New<JobRequiredDocument>();
					other.Add(newDoc);

					// This validation is disabled to fix the WI00775057
					// The root cause is because of the caching of biz obj in "BusinessObject LoadBusinessObjectFromMRU(Type bizOType, ZGuid pk)"
					// The MRU caching for JobDeclaration will be either BaseJobDeclaration or the sub-class, e.g. AU.Declaration.Business.JobDeclaration
					// This causes the JobRequiredDocs list are not synced during Order's attachment to the JobDeclaration
					// Moreover, the JobRequiredDocs have been validated in Order, so it could be disabled here without causing other issues.
					var isValidationSuspended = newDoc.IsValidationSuspended;
					try
					{
						newDoc.SuspendValidation();
						newDoc.CopyPersistentValuesFrom(doc);
						newDoc.EQ_ParentTableCode = other.Parent.TableCode;
						newDoc.EQ_ParentID = other.Parent.PK;
					}
					finally
					{
						if (!isValidationSuspended)
						{
							newDoc.ResumeValidation();
						}
					}
				}
			}
		}

		#endregion

		#region AddCountryRequiredDocuments

		public enum DirectionFilterType
		{
			Domestic = 0,
			Both = 1,
			Import = 2,
			Export = 3
		}

		public void AddCountryRequiredDocuments(SchemaBoolColumn rdTypeColumn, ZString transportMode, ZString packingMode, DirectionFilterType directionType, ZString origin, ZString destination)
		{
			string[] transportModes = new string[] { Constants.TransportModes.All, transportMode, packingMode };
			ZQuery transportModeQuery = new ZQuery(RefCountryRequiredDocumentSchema.RD_TransportMode, transportModes);

			ZQuery directionQuery = new ZQuery(RefCountryRequiredDocumentSchema.RD_DocUsage, GetDirectionValuesForType(directionType));

			string[] originValues = new string[] { origin.SubstringSafe(0, 2), "" };
			ZQuery originQuery = new ZQuery(RefCountryRequiredDocumentSchema.RD_RN_NKOrigin, originValues);

			string[] destinationValues = new string[] { destination.SubstringSafe(0, 2), "" };
			ZQuery destinationQuery = new ZQuery(RefCountryRequiredDocumentSchema.RD_RN_NKDestination, destinationValues);

			ZQuery filter = new ZQuery(rdTypeColumn, true);
			filter.DefaultJoinCondition = JoinCondition.And;
			filter.AddToFilter(originQuery);
			filter.AddToFilter(destinationQuery);
			filter.AddToFilter(transportModeQuery);
			filter.AddToFilter(directionQuery);

			AddCountryRequiredDocuments(filter, directionType);
		}

		string[] GetDirectionValuesForType(DirectionFilterType directionType)
		{
			switch (directionType)
			{
				case DirectionFilterType.Both:
					return new string[] { JobRequiredDocument.DocUsage.All, JobRequiredDocument.DocUsage.Both, JobRequiredDocument.DocUsage.Export, JobRequiredDocument.DocUsage.Import };
				case DirectionFilterType.Export:
					return new string[] { JobRequiredDocument.DocUsage.All, JobRequiredDocument.DocUsage.Both, JobRequiredDocument.DocUsage.Export };
				case DirectionFilterType.Import:
					return new string[] { JobRequiredDocument.DocUsage.All, JobRequiredDocument.DocUsage.Both, JobRequiredDocument.DocUsage.Import };
				case DirectionFilterType.Domestic:
					return new string[] { JobRequiredDocument.DocUsage.All, JobRequiredDocument.DocUsage.Domestic };
				default:
					throw new NotSupportedException("Invalid DirectionFilterType");
			}
		}

		void AddCountryRequiredDocuments(ZQuery filter, DirectionFilterType directionType)
		{
			RefCountryRequiredDocumentCollection refCountryRDs = new RefCountryRequiredDocumentCollection(Factory, filter);
			refCountryRDs.Load();
			foreach (RefCountryRequiredDocument refCountryRD in refCountryRDs)
			{
				JobRequiredDocument jobRD = GetDocByType(refCountryRD.RD_DocType);
				if (jobRD == null)
				{
					jobRD = AddNew();
					using (jobRD.SuspendDocTypeUniquenessCheck())
					{
						jobRD.EQ_DocType = refCountryRD.RD_DocType;
						if (jobRD.EQ_DocType == Constants.RefDocTypes.MiscellaneousDocument)
						{
							jobRD.EQ_DocDescription = Core.Constants.RefDocTypeDescriptions.MiscellaneousDocument.ToString();
						}

						switch (directionType)
						{
							case DirectionFilterType.Domestic:
								jobRD.EQ_DocUsage = JobRequiredDocument.DocUsage.Domestic;
								break;
							case DirectionFilterType.Export:
								jobRD.EQ_DocUsage = JobRequiredDocument.DocUsage.Export;
								break;
							case DirectionFilterType.Import:
								jobRD.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
								break;
							default:
								jobRD.EQ_DocUsage = refCountryRD.RD_DocUsage == JobRequiredDocument.DocUsage.All ? JobRequiredDocument.DocUsage.Both : refCountryRD.RD_DocUsage.ToString();
								break;
						}
					}
				}
				else if (directionType != DirectionFilterType.Domestic && jobRD.EQ_DocUsage != refCountryRD.RD_DocUsage)
				{
					jobRD.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
				}
				jobRD.Origin = JobRequiredDocument.JRDOrigin.FromRequirements;
			}
		}

		#endregion

		#region AddAndAcquitRequiredDocuments

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void AddAndAcquitRequiredDocuments(IHaveRequiredDocuments parent, ZString origin, ZString destination, OrgHeader currentOrg, ZDateTime eTD)
		{
			if (parent == null)
			{
				return;
			}

			var createdJRDs = new List<JobRequiredDocument>();

			foreach (var reqDoc in parent.RequiredDocuments.OfType<JobRequiredDocument>())
			{
				if (reqDoc.EQ_DocCategory == Constants.ReferenceTypes.SupplyChainLogistics)
				{
					if ((origin == "" && destination == "")

						|| (reqDoc.EQ_DocUsage == JobRequiredDocument.DocUsage.Domestic
						&& destination.SubstringSafe(0, 2) == origin.SubstringSafe(0, 2))

						|| (reqDoc.EQ_DocUsage == JobRequiredDocument.DocUsage.Both
						&& (reqDoc.EQ_RN_NKRelatedCountry == destination.SubstringSafe(0, 2)
						|| reqDoc.EQ_RN_NKRelatedCountry == origin.SubstringSafe(0, 2)
						|| reqDoc.EQ_RN_NKRelatedCountry.IsEmpty))

						|| ((reqDoc.EQ_DocUsage == JobRequiredDocument.DocUsage.Import)
						&& ((reqDoc.EQ_RN_NKRelatedCountry == destination.SubstringSafe(0, 2)
						|| reqDoc.EQ_RN_NKRelatedCountry.IsEmpty)
						&& currentOrg.OH_RL_NKClosestPort.SubstringSafe(0, 2) == destination.SubstringSafe(0, 2)))

						|| ((reqDoc.EQ_DocUsage == JobRequiredDocument.DocUsage.Both
						|| reqDoc.EQ_DocUsage == JobRequiredDocument.DocUsage.Export)
						&& ((reqDoc.EQ_RN_NKRelatedCountry == origin.SubstringSafe(0, 2)
						|| reqDoc.EQ_RN_NKRelatedCountry.IsEmpty)
						&& currentOrg.OH_RL_NKClosestPort.SubstringSafe(0, 2) == origin.SubstringSafe(0, 2))))
					{
						AddAndAcquitRequiredDocument(reqDoc, eTD, createdJRDs);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "EQDocDescription checking raw value before assignment")]
		void AddAndAcquitRequiredDocument(JobRequiredDocument doc, ZDateTime eTD, List<JobRequiredDocument> createdJRDs)
		{
			var jobRequiredDoc = GetDocByType(doc.EQ_DocType);
			//apparently being able to make two JRDs for the same EQ_DocType simultaneously is OK (it's unit tested), so allow this case
			//by keeping track of which JRDs we've made this 'AddAndAcquitRequiredDocuments' call and pretending they don't exist
			if (jobRequiredDoc != null && createdJRDs.Contains(jobRequiredDoc))
			{
				jobRequiredDoc = null;
			}

			if (jobRequiredDoc == null &&
				((doc.EQ_DocPeriod == Constants.JobRequiredDocuments.DocumentPeriods.Periodic
					&& doc.EQ_DateReceived.ToZDateTime() <= eTD && doc.EQ_ValidToDate >= eTD)
				|| doc.EQ_DocPeriod != Constants.JobRequiredDocuments.DocumentPeriods.Periodic || !eTD.IsValid))
			{
				jobRequiredDoc = AddNew();
				createdJRDs.Add(jobRequiredDoc);
				jobRequiredDoc.EQ_DocType = doc.EQ_DocType;
				jobRequiredDoc.EQ_DocUsage = doc.EQ_DocUsage;
				jobRequiredDoc.EQ_DocNumber = doc.EQ_DocNumber;
				jobRequiredDoc.EQ_OH_DocumentOwner = doc.EQ_OH_DocumentOwner;
				jobRequiredDoc.EQ_DocPeriod = doc.EQ_DocPeriod;
				jobRequiredDoc.EQ_DocumentNotes = doc.EQ_DocumentNotes;

				if (doc.EQ_DocPeriod == Constants.JobRequiredDocuments.DocumentPeriods.Periodic)
				{
					jobRequiredDoc.EQ_DateReceived = doc.EQ_DateReceived;
					jobRequiredDoc.EQ_ValidToDate = doc.EQ_ValidToDate;
				}
			}

			if (jobRequiredDoc != null && jobRequiredDoc.EQ_DocType == Core.Constants.RefDocTypes.MiscellaneousDocument && jobRequiredDoc.EQ_DocDescription.IsEmpty)
			{
				jobRequiredDoc.EQ_DocDescription = doc.EQ_DocDescription;
			}
			if (jobRequiredDoc != null)
			{
				jobRequiredDoc.EQ_OriginalDocRequired = doc.EQ_OriginalDocRequired;
				jobRequiredDoc.EQ_CreditControlDoc = doc.EQ_CreditControlDoc;
			}

			if (jobRequiredDoc?.IsDocTypeDuplicate ?? false)
			{
				RemoveAndDelete(jobRequiredDoc);
			}
			else if (jobRequiredDoc != null)
			{
				jobRequiredDoc.Origin = JobRequiredDocument.JRDOrigin.FromRequirements;
			}
		}
		#endregion

		#region Implementation

		public IEnumerable<JobRequiredDocument> GetElementsForTypeSortedByValidDate(ZString docType)
		{
			List<JobRequiredDocument> list = new List<JobRequiredDocument>();
			foreach (JobRequiredDocument doc in this)
			{
				if (doc.EQ_DocType == docType)
				{
					list.Add(doc);
				}
			}
			list.Sort((x, y) =>
			{
				int compareValue = y.EQ_ValidToDate.CompareTo(x.EQ_ValidToDate); // in latest order
				if (compareValue == 0)
				{
					compareValue = x.PK.CompareTo(y.PK);
				}

				return compareValue;
			});
			return list.ToArray();
		}

		protected IHaveRequiredDocuments Parent;

		#endregion
	}
}
