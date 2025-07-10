using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocumentLookups : AutoJobRequiredDocumentLookups
	{
		public JobRequiredDocumentLookups(AutoJobRequiredDocument parent)
			: base(parent)
		{
		}

		#region Parent

		public new JobRequiredDocument Parent
		{
			get
			{
				return base.Parent as JobRequiredDocument;
			}
		}

		#endregion

		#region DocumentOwners

		public override OrgHeaderCollection DocumentOwners
		{
			get
			{
				if (Parent != null && Parent.EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport)
				{
					return new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.PK, Environment.Env.CurrentCompany.OrganisationPK));
				}
				else
				{
					return new ConsignorCollection(Factory);
				}
			}
		}

		#endregion

		#region DocUsage_List

		public CodeDescriptionPairList DocUsage_List
		{
			get
			{
				CodeDescriptionPairList fDocUsage_List = new CodeDescriptionPairList();

				string creditorDescription = Res.GetString("73d84040-3cd3-4cd7-828a-6de2059e3216", "Creditor");
				string debtorDescription = Res.GetString("354badbd-5375-4617-9d42-c4ec25270d3f", "Debtor");

				if (Parent != null
					&& (Parent.EQ_DocType == Constants.RefDocTypes.VATExporterExemption
						|| Parent.EQ_DocType == Constants.RefDocTypes.WithholdingTaxExemption
						|| Parent.EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport))
				{
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Creditor, creditorDescription);
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Debtor, debtorDescription);
				}
				else if (Parent != null && Parent.Parent != null && (typeof(OrgHeader).IsAssignableFrom(Parent.Parent.GetType()) || typeof(OrgSupplierPart).IsAssignableFrom(Parent.Parent.GetType()))
					&& Parent.EQ_DocCategory == Constants.ReferenceTypes.ClientSupplierRelationship)
				{
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Broker, Res.GetString("951b2185-af2a-46db-85f9-9f67dfb50c05", "Broker"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Carrier, Res.GetString("587d22ca-e72a-44cc-b41d-8528261abcd1", "Carrier"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Competitor, Res.GetString("621e2692-e000-4efb-8fe1-fe2c34eeeb99", "Competitor"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Creditor, creditorDescription);
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Debtor, debtorDescription);
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.ForwarderAgent, Res.GetString("0c7d6404-52d8-4474-a718-9da71aa1a9f2", "Forwarder / Agent"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.ImporterConsignee, Res.GetString("c22d36e3-b6d2-4395-bce2-0ba1db46b12c", "Importer / Consignee"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Services, Res.GetString("11d97510-50bb-47d5-9dda-356ad7cf50c4", "Services"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.SupplierConsignor, Res.GetString("71659afa-ba24-4291-879b-580d59535390", "Supplier / Consignor"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.TransportClient, Res.GetString("40269d5b-1461-4b35-8e49-d2d6ae3bf6fd", "Transport Client"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Warehouse, Res.GetString("30b4c2d7-b425-46f8-8798-c7fd173059fd", "Warehouse"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.AttorneyForCustomsProcedures, Res.GetString("6EB70642-1B7A-42B4-98F9-DD0C6C243D39", "Attorney for Customs Procedures (ACP)"));
				}
				else
				{
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Export, Res.GetString("15b5a406-3838-468e-930f-97b0e10229be", "Export"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Import, Res.GetString("68133d13-af58-4a7f-9ebd-2cdc31ea7a82", "Import"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Both, Res.GetString("3c9a3ff5-77ba-4302-a57f-2fef52e8930c", "Both Export and Import"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Domestic, Res.GetString("849b7361-b880-4005-9374-ce03797e8b76", "Domestic"));
				}
				return fDocUsage_List;
			}
		}

		#endregion

		#region RefCountry_List

		public RefCountryCollection RefCountry_List
		{
			get
			{
				if (refCountry_List == null)
				{
					refCountry_List = new RefCountryCollection(Factory);
				}
				return refCountry_List;
			}
		}
		RefCountryCollection refCountry_List;

		#endregion

		#region DocPeriod_List

		public CodeDescriptionPairList DocPeriod_List
		{
			get
			{
				CodeDescriptionPairList result;

				result = new CodeDescriptionPairList(OLookUpEditType.JobRequiredDocumentPeriods);
				if (Parent != null)
				{
					if (Parent.EQ_DocType == Constants.RefDocTypes.VATExporterExemption
						|| Parent.EQ_DocType == Constants.RefDocTypes.WithholdingTaxExemption
						|| Parent.EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport)
					{
						result.RemoveCode(Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment);
					}
					else if (Parent.Parent != null && Parent.IsPowerOfAttorney
						&& Parent.EQ_DocCategory == Constants.ReferenceTypes.SupplyChainLogistics || Parent.IsPowerOfAttorney)
					{
						if (typeof(OrgHeader).IsAssignableFrom(Parent.Parent.GetType()))
						{
							result.RemoveCode(Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment);
						}
						else
						{
							result.RemoveCode(Constants.JobRequiredDocuments.DocumentPeriods.Periodic);
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region CategoryType_List

		public CodeDescriptionPairList CategoryType_List
		{
			get
			{
				if (fCategoryType_List == null || (fCategoryType_List != null && fCategoryType_List.Count == 0))
				{
					fCategoryType_List = new CodeDescriptionPairList();

					if (Parent != null && Parent.Parent != null)
					{
						IDocManagerSupport docManager = Parent.Parent.UltimateDocumentParent as IDocManagerSupport;
						if (docManager != null && docManager.DocManagerInfo != null)
						{
							ZString docManagerReferenceType = DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(docManager.DocManagerInfo.DocManagerCode);
							fCategoryType_List.AddPair(docManagerReferenceType, GetReferenceTypeDescriptionFromCode(docManagerReferenceType));
						}

						if (Parent.Parent.AdditionalRefTypes != null)
						{
							foreach (ZString refType in Parent.Parent.AdditionalRefTypes)
							{
								if (fCategoryType_List.ContainsCode(refType))
								{
									throw new NotSupportedException("RefType is already contained in List");
								}
								fCategoryType_List.AddPair(refType, GetReferenceTypeDescriptionFromCode(refType));
							}
						}

						if ((typeof(OrgHeader).IsAssignableFrom(Parent.Parent.GetType()) && AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Any())
							|| (Parent.EQ_DocCategoryInfo.ReadOnly && Parent.EQ_DocCategory.EqualsIgnoringCase(Constants.ReferenceTypes.ComplianceReport)))
						{
							fCategoryType_List.AddPair(Constants.ReferenceTypes.ComplianceReport, GetReferenceTypeDescriptionFromCode(Constants.ReferenceTypes.ComplianceReport));
						}
					}
				}
				return fCategoryType_List;
			}
		}
		CodeDescriptionPairList fCategoryType_List;

		ZString GetReferenceTypeDescriptionFromCode(ZString refType)
		{
			return AllCategoryType_List.GetDescriptionFromCode(refType);
		}

		CodeDescriptionPairList AllCategoryType_List
		{
			get
			{
				if (allCategoryType_List == null)
				{
					allCategoryType_List = new CodeDescriptionPairList();
					allCategoryType_List.AddPair(Constants.ReferenceTypes.All, Constants.ReferenceTypeDescriptions.All);
					allCategoryType_List.AddPair(Constants.ReferenceTypes.Accounting, Constants.ReferenceTypeDescriptions.Accounting);
					allCategoryType_List.AddPair(Constants.ReferenceTypes.BusinessEntityProcessWorkflow, Constants.ReferenceTypeDescriptions.BusinessEntityProcessWorkflow);
					allCategoryType_List.AddPair(Constants.ReferenceTypes.GeneralReferenceTables, Constants.ReferenceTypeDescriptions.GeneralReferenceTables);
					allCategoryType_List.AddPair(Constants.ReferenceTypes.HumanResourcesStaffEmployment, Constants.ReferenceTypeDescriptions.HumanResourcesStaffEmployment);
					allCategoryType_List.AddPair(Constants.ReferenceTypes.ComplianceReport, Constants.ReferenceTypeDescriptions.ComplianceReport);
					allCategoryType_List.AddPair(Constants.ReferenceTypes.Unallocated, Res.GetString("820dd42a-aa36-4abc-9627-5d2c2e3b6801", "Unallocated"));

					if (!DataRegistry.Instance.ProductivityWiseModeEnabled)
					{
						allCategoryType_List.AddPair(Constants.ReferenceTypes.ClientSupplierRelationship, Constants.ReferenceTypeDescriptions.ClientSupplierRelationship);
						allCategoryType_List.AddPair(Constants.ReferenceTypes.SupplyChainLogistics, Constants.ReferenceTypeDescriptions.SupplyChainLogistics);
					}
				}
				return allCategoryType_List;
			}
		}
		CodeDescriptionPairList allCategoryType_List;

#if DEBUG
		public CodeDescriptionPairList AllCategoryType_List_ForTest => AllCategoryType_List;
#endif

		#endregion

		#region DocType_List

		public CodeDescriptionPairList DocType_List
		{
			get
			{
				if (fDocType_List == null || Parent.EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport)
				{
					fDocType_List = new CodeDescriptionPairList();

					if (Parent.EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport)
					{
						if (Parent.EQ_DocTypeInfo.ReadOnly)
						{
							fDocType_List.AddPair(Parent.EQ_DocType);
						}
						else
						{
							var docTypes = GetDocTypesForCategory().Where(x => x.RT_ReferenceType == Parent.EQ_DocCategory);
							foreach (var docType in docTypes)
							{
								fDocType_List.AddPair(docType.RT_DocType, docType.RT_DescMultilingual);
							}
							var docTypeCodes = docTypes.Select(x => x.RT_DocType).ToHashSet();

							fDocType_List.AddRange(AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.
								GetReportTypeList(Parent.EQ_RN_NKRelatedCountry, Res.GetString("4f57d542-8617-49d7-a419-2215deeb681d", "Exempt from") + " {0}").
								OfType<CodeDescriptionPair>().Where(x => !docTypeCodes.Contains(x.Code)).ToList()); // Temporary we do not add Report Type Codes duplicating pre-defined Doc Types (currently only RSB)
						}
					}
					else
					{
						RefDocTypeCollection docTypes = GetDocTypesForCategory();

						foreach (RefDocType docType in docTypes)
						{
							bool isOrgPOA = (Parent != null && Parent.Parent != null && typeof(OrgHeader).IsAssignableFrom(Parent.Parent.GetType())
								&& (docType.RT_DocType == Constants.RefDocTypes.PowerOfAttorney
									|| docType.RT_DocType == Constants.RefDocTypes.PowerOfAttorneyCustoms
									|| docType.RT_DocType == Constants.RefDocTypes.PowerOfAttorneyForwarding)
								&& docType.RT_ReferenceType == Constants.ReferenceTypes.SupplyChainLogistics);

							if (docType.RT_DocType != Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument &&
								docType.RT_DocType != Core.Constants.RefDocTypes.InternallyCreatedPublicDocument &&
								!isOrgPOA)
							{
								fDocType_List.AddPair(docType.RT_DocType, docType.RT_DescMultilingual);
							}
						}
					}
				}

				return fDocType_List;
			}
		}
		CodeDescriptionPairList fDocType_List;

		public void ResetDocAndCategoryTypeList()
		{
			fDocType_List = null;
			fCategoryType_List = null;
		}

		RefDocTypeCollection GetDocTypesForCategory()
		{
			ZQuery docTypeQuery = GetDocTypeCategoryQuery();
			ZQuery visibleQuery = new ZQuery(RefDocTypeSchema.RT_IsActive, ZBool.True);
			docTypeQuery.AddToFilter(visibleQuery, JoinCondition.And);

			RefDocTypeCollection docTypes = new RefDocTypeCollection(Parent.Factory, docTypeQuery);
			docTypes.ApplySort(RefDocType.Schema.RT_DocType, ListSortDirection.Ascending);
			return docTypes;
		}

		DocTypeCategoryQuery GetDocTypeCategoryQuery()
		{
			ZString rT_ReferenceType;

			if (!Parent.EQ_DocCategory.IsEmpty)
			{
				DocTypeCategoryQuery docTypeQuery = new DocTypeCategoryQuery(Factory, Parent.EQ_DocCategory);
				return docTypeQuery;
			}
			else
			{
				if (Parent != null && Parent.Parent != null)
				{
					IDocManagerSupport docManager = Parent.Parent.UltimateDocumentParent as IDocManagerSupport;

					rT_ReferenceType = (docManager != null) ? DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(docManager.DocManagerInfo.DocManagerCode)
						: Constants.ReferenceTypes.All;
				}
				else
				{
					rT_ReferenceType = Constants.ReferenceTypes.All;
				}

				DocTypeCategoryQuery docTypeQuery = new DocTypeCategoryQuery(Factory, rT_ReferenceType);

				if (Parent.Parent != null && Parent.Parent.AdditionalRefTypes != null)
				{
					foreach (ZString refType in Parent.Parent.AdditionalRefTypes)
					{
						docTypeQuery.AddRefType(refType);
					}
				}
				return docTypeQuery;
			}
		}

		#endregion

		#region Form Reference Type

		public String FormReferenceType
		{
			get
			{
				String formReferenceType = "";
				if (Parent != null && Parent.Parent != null)
				{
					IDocManagerSupport docManager = Parent.Parent.UltimateDocumentParent as IDocManagerSupport;

					if (docManager != null && docManager.DocManagerInfo != null)
					{
						formReferenceType = DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(docManager.DocManagerInfo.DocManagerCode);
					}
					else if (Parent.Parent.AdditionalRefTypes.Count > 0)
					{
						formReferenceType = Parent.Parent.AdditionalRefTypes[0].ToString();
					}
				}
				return formReferenceType;
			}
		}

		#endregion
	}
}
