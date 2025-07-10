using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocumentValidation : AutoJobRequiredDocumentValidation
	{
		readonly string dateReceivedAndValidToDateMustInTheSameYear = ResString.GetMultilingualString("37daaf13-e95c-485f-870f-c5c599b5f580", "Date Received and Valid To Date must in the same year.");

		public JobRequiredDocumentValidation(AutoJobRequiredDocument parent)
			: base(parent)
		{
			RequiredDoc = (JobRequiredDocument)parent;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDuplicateRows();
		}

		public void ValidateDuplicateRows()
		{
			var parent = RequiredDoc;

			ZQuery GetDuplicateDocumentQuery()
			{
				return new ZDBOnlyQuery(typeof(JobRequiredDocument))
					.AddToFilter(JobRequiredDocumentSchema.EQ_ParentID, parent.EQ_ParentID)
					.AddToFilter(JobRequiredDocumentSchema.PK, SQLComparisonOperator.NotEqual, parent.PK)
					.AddToFilter(JobRequiredDocumentSchema.EQ_OH_DocumentOwner, parent.EQ_OH_DocumentOwner)
					.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, Constants.RefDocTypes.PowerOfAttorney)
					.AddToFilter(JobRequiredDocumentSchema.EQ_RN_NKRelatedCountry, Core.Constants.CountryCodes.Japan)
					.AddToFilter(JobRequiredDocumentSchema.EQ_DocCategory, Constants.ReferenceTypes.ClientSupplierRelationship)
					.AddToFilter(JobRequiredDocumentSchema.EQ_DocUsage, JobRequiredDocument.DocUsage.AttorneyForCustomsProcedures);
			}

			bool HasDuplicateDocumentsInLocal()
			{
				var query = GetDuplicateDocumentQuery();
				return parent.Parent.RequiredDocuments.Find(query).Length > 0;
			}

			bool HasDuplicateDocumentsInDatabase()
			{
				var query = GetDuplicateDocumentQuery();
				return parent.Factory.ExistsInDatabase(JobRequiredDocumentSchema.Constants.TableName, query);
			}

			if (IsUsingClientSupplierRelationship(parent) && (HasDuplicateDocumentsInLocal() || HasDuplicateDocumentsInDatabase()))
			{
				Parent.AddRowError(Res.GetString("76471880-BD68-42EB-8317-2C9B90CEDB9D", "There should be only one CSR + POA + ACP + JP document tracking record of the same document owner under the same organization."));
			}
		}

		protected override void CheckEQ_OH_DocumentOwner()
		{
			var info = Parent.EQ_OH_DocumentOwnerInfo;

			if (IsUsingClientSupplierRelationship(RequiredDoc) && Parent.DocumentOwner == null)
			{
				info.AddError(Res.GetString("D9B2E86B-0AFA-4870-B938-3A969460A59E", "Please select the Attorney for Customs Procedures (ACP) for this Power of Attorney (POA)."));
			}

			if (Parent.EQ_DocUsage == JobRequiredDocument.DocUsage.AttorneyForCustomsProcedures && !(Parent.DocumentOwner?.Country?.Code.Equals(Core.Constants.CountryCodes.Japan) ?? false))
			{
				info.AddMessageError(Res.GetString("327DDD8F-05B7-4BCA-8EA7-DB7E0DC5B8BA", "Attorney for Customs Procedures (ACP) must be from Japan. The entered organization is from '{0}'.", Parent.DocumentOwner?.Country?.Description));
			}
		}

		bool IsUsingClientSupplierRelationship(JobRequiredDocument requiredDocument)
		{
			return requiredDocument.EQ_DocCategory == Constants.ReferenceTypes.ClientSupplierRelationship && requiredDocument.EQ_DocType == Constants.RefDocTypes.PowerOfAttorney
				&& requiredDocument.EQ_DocUsage == JobRequiredDocument.DocUsage.AttorneyForCustomsProcedures && requiredDocument.EQ_RN_NKRelatedCountry == Core.Constants.CountryCodes.Japan;
		}

		protected override void CheckEQ_DocType()
		{
			base.CheckEQ_DocType();

			if (!RequiredDoc.EQ_DocTypeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(RequiredDoc.EQ_DocTypeInfo);
			}

			if (!RequiredDoc.EQ_DocTypeInfo.HasErrors())
			{
				// Error if updating EQ_DocType to an invalid DocType or RequiredDoc does not have an eDocs with the same DocType
				if (IsEQ_DocTypeUpdated || !HasRelatedEDocs())
				{
					ListValidation.ErrorIfInvalidCode(RequiredDoc.EQ_DocTypeInfo);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(RequiredDoc.EQ_DocTypeInfo);
				}
			}

			if (!RequiredDoc.EQ_DocTypeInfo.HasErrors() && RequiredDoc.IsDocTypeDuplicate)
			{
				if (RequiredDoc.DocType.RT_AllowMultiplePeriodicDocs)
				{
					RequiredDoc.EQ_DocTypeInfo.AddError(Res.GetString("741D62EC-49BE-475D-A383-33D4D9E0E54B", "There is already {0} type for this job. If it's not shown - please reload the form.", RequiredDoc.EQ_DocType));
				}
				else
				{
					RequiredDoc.EQ_DocTypeInfo.AddError(Res.GetString("36B4F70B-DC5C-447F-82C5-86A658AAFD80", "The type {0} does not allow multiple periodic documents, but there is already {0} type for this job with the same valid to date. If it's not shown - please reload the form.", RequiredDoc.EQ_DocType));
				}
			}

			AddRowErrorForCostaRicaIfApplicable();
		}

		bool IsEQ_DocTypeUpdated => RequiredDoc.IsInDatabase && !RequiredDoc.EQ_DocTypeInfo.OriginalValue.Equals(RequiredDoc.EQ_DocTypeInfo.Value);

		bool HasRelatedEDocs()
		{
			var docManagerSupport = RequiredDoc.Parent?.UltimateDocumentParent as IDocManagerSupport;
			if (docManagerSupport != null)
			{
				docManagerSupport.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
				return docManagerSupport.DocManagerInfo.EDocsView.GetMostRecentEDoc(RequiredDoc.EQ_DocType) != null;
			}
			return false;
		}

		protected override void CheckEQ_RN_NKRelatedCountry()
		{
			base.CheckEQ_RN_NKRelatedCountry();
			ListValidation.ErrorIfInvalidCode(Parent.EQ_RN_NKRelatedCountryInfo);

			if (Parent.EQ_DocType == Constants.RefDocTypes.VATExporterExemption ||
				Parent.EQ_DocCategory == Constants.ReferenceTypes.ComplianceReport)
			{
				MandatoryValidation.CheckEntered(Parent.EQ_RN_NKRelatedCountryInfo);
			}

			AddRowErrorForCostaRicaIfApplicable();
		}

		protected override void CheckEQ_DocNumber()
		{
			base.CheckEQ_DocNumber();

			if (Parent.EQ_DocType == Constants.RefDocTypes.VATExporterExemption)
			{
				if ((!GlbCompany.CurrentCompany.Country.SupportDeclarationOfIntent || !RequiredDoc.RelatedCountrySupportDeclarationOfIntent) && IsRequiredDocBelongsToCurrentLoginCompany)
				{
					MandatoryValidation.CheckEntered(RequiredDoc.EQ_DocNumberInfo);
				}
			}
			else if (RequiredDoc.EQ_DocType == Core.Constants.RefDocTypes.HeXiaoDan && !RequiredDoc.EQ_DateReceived.IsEmpty)
			{
				MandatoryValidation.CheckEntered(RequiredDoc.EQ_DocNumberInfo);
			}

			if (IsUsingClientSupplierRelationship(RequiredDoc) && Parent.EQ_DocNumber.Length > 10)
			{
				Parent.EQ_DocNumberInfo.AddError(Res.GetString("92BC17E8-C8DF-4188-96BF-DA577D37B4E4", "The maximum length for Power of Attorney for Attorney for Customs Procedures (ACP) is 10 characters."));
			}
		}

		bool IsRequiredDocBelongsToCurrentLoginCompany
		{
			get
			{
				var companyAttrib = RequiredDoc.Attributes[JobRequiredDocAttribTypeList.Codes.CompanyCode];
				return companyAttrib == null || companyAttrib.D0_AttribValue == GlbCompany.CurrentCompany.PK.ToString();
			}
		}

		protected override void CheckEQ_DocUsage()
		{
			base.CheckEQ_DocUsage();
			MandatoryValidation.CheckEntered(RequiredDoc.EQ_DocUsageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.EQ_DocUsageInfo);

			AddRowErrorForCostaRicaIfApplicable();
		}

		protected override void CheckEQ_DocCategory()
		{
			base.CheckEQ_DocCategory();
			MandatoryValidation.CheckEntered(RequiredDoc.EQ_DocCategoryInfo);
			ListValidation.ErrorIfInvalidCode(RequiredDoc.EQ_DocCategoryInfo);
		}

		protected override void CheckEQ_DocPeriod()
		{
			base.CheckEQ_DocPeriod();
			MandatoryValidation.CheckEntered(RequiredDoc.EQ_DocPeriodInfo);
			ListValidation.ErrorIfInvalidCode(RequiredDoc.EQ_DocPeriodInfo);
		}

		protected override void CheckEQ_SntToCustomsBroker()
		{
			base.CheckEQ_SntToCustomsBroker();

			if (RequiredDoc.EQ_DocType == Core.Constants.RefDocTypes.HeXiaoDan &&
				!RequiredDoc.EQ_SntToCustomsBroker.IsEmpty && RequiredDoc.EQ_DateReceived.IsEmpty)
			{
				RequiredDoc.EQ_SntToCustomsBrokerInfo.AddWarning(Res.GetString("28d49fef-c63c-49d7-91aa-be9621a68a65", "Date received from shipper has not yet been set for this document."));
			}
		}

		protected override void CheckEQ_RcvFromCustomsBroker()
		{
			base.CheckEQ_RcvFromCustomsBroker();

			if (RequiredDoc.EQ_DocType == Core.Constants.RefDocTypes.HeXiaoDan &&
				!RequiredDoc.EQ_RcvFromCustomsBroker.IsEmpty && RequiredDoc.EQ_SntToCustomsBroker.IsEmpty)
			{
				RequiredDoc.EQ_RcvFromCustomsBrokerInfo.AddWarning(Res.GetString("09b09d74-13cf-436c-8b09-92a89f5a2b65", "Date sent to customs agent has not yet been set for this document."));
			}
		}

		protected override void CheckEQ_ReturnToShipper()
		{
			base.CheckEQ_ReturnToShipper();

			if (RequiredDoc.Parent != null)
			{
				BusinessObject ultimateParent = RequiredDoc.Parent.UltimateDocumentParent;

				if (ultimateParent != null && RequiredDoc.EQ_DocType == Core.Constants.RefDocTypes.HeXiaoDan
					&& ultimateParent.GetType().IsSubclassOf(ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>()))
				{
					DynamicBusinessObjectCollection outstandingInvoices = RequiredDoc.LocalOutstandingInvoices;

					if (outstandingInvoices.Count != 0)
					{
						ZString warnings = ZString.Empty;
						ZString warning;
						for (int i = 0; i < outstandingInvoices.Count; i++)
						{
							string outstandingAmt = outstandingInvoices[i]["OutstandingAmt"].ToString();
							int decPtIndex = outstandingAmt.IndexOf('.');
							if (outstandingAmt.Length > decPtIndex + 3)
							{
								outstandingAmt = outstandingAmt.Substring(0, decPtIndex + 3);
							}
							warning = Res.GetString("269890b3-75a8-4254-80b6-6d3b3f0bff7e", "The invoice") + " " + outstandingInvoices[i][AccTransactionHeader.Schema.AH_TransactionNum] + " " + Res.GetString("9dceb4cd-3ae4-4671-9c8f-b1f1ea9a5f61", "is outstanding from debtor") + " " + outstandingInvoices[i][OrgHeader.Schema.OH_FullName] + System.Environment.NewLine;
							warning += Res.GetString("56636893-d3a5-452d-8b5c-232ea5d3e42f", "Outstanding amount in") + " " + outstandingInvoices[i][RefCurrency.Schema.RX_Code] + " " + Res.GetString("4cb90bff-a790-4f73-8d92-8e0b892d6822", "is {0}", outstandingAmt) + "\r\n\r\n";
							warnings += warning;
						}

						RequiredDoc.EQ_ReturnToShipperInfo.AddWarning(warnings);
					}
				}
			}

			if (RequiredDoc.EQ_DocType == Core.Constants.RefDocTypes.HeXiaoDan &&
				!RequiredDoc.EQ_ReturnToShipper.IsEmpty && RequiredDoc.EQ_RcvFromCustomsBroker.IsEmpty)
			{
				RequiredDoc.EQ_ReturnToShipperInfo.AddWarning(Res.GetString("d5648010-a129-4597-af6d-1c45f6350801", "Date received from customs agent has not yet been set for this document"));
			}
		}

		protected override void CheckEQ_CreditControlDoc()
		{
			base.CheckEQ_CreditControlDoc();

			if (RequiredDoc.EQ_DocType == Core.Constants.RefDocTypes.HeXiaoDan &&
				RequiredDoc.EQ_CreditControlDoc)
			{
				DynamicBusinessObjectCollection outstandingInvoices = RequiredDoc.LocalOutstandingInvoices;

				if (outstandingInvoices.Count != 0)
				{
					ZString warnings = ZString.Empty;
					ZString warning;
					for (int i = 0; i < outstandingInvoices.Count; i++)
					{
						string outstandingAmt = outstandingInvoices[i]["OutstandingAmt"].ToString();
						int decPtIndex = outstandingAmt.IndexOf('.');
						if (outstandingAmt.Length > decPtIndex + 3)
						{
							outstandingAmt = outstandingAmt.Substring(0, decPtIndex + 3);
						}
						warning = Res.GetString("269890b3-75a8-4254-80b6-6d3b3f0bff7e", "The invoice") + " " + outstandingInvoices[i][AccTransactionHeader.Schema.AH_TransactionNum] + " " + Res.GetString("9dceb4cd-3ae4-4671-9c8f-b1f1ea9a5f61", "is outstanding from debtor") + " " + outstandingInvoices[i][OrgHeader.Schema.OH_FullName] + System.Environment.NewLine;
						warning += Res.GetString("56636893-d3a5-452d-8b5c-232ea5d3e42f", "Outstanding amount in") + " " + outstandingInvoices[i][RefCurrency.Schema.RX_Code] + " " + Res.GetString("4cb90bff-a790-4f73-8d92-8e0b892d6822", "is {0}", outstandingAmt) + "\r\n\r\n";
						warnings += warning;
					}

					RequiredDoc.EQ_CreditControlDocInfo.AddWarning(warnings);
				}
			}
		}

		protected override void CheckEQ_ValidToDate()
		{
			base.CheckEQ_ValidToDate();

			if (RequiredDoc.IsPeriodic && RequiredDoc.EQ_ValidToDate.IsEmpty)
			{
				RequiredDoc.EQ_ValidToDateInfo.AddError(Res.GetString("3afb75f4-91eb-4332-8189-62d65c216d51", "Please enter the expiry date of the document, if the document period is {0}.",
					Core.Constants.JobRequiredDocuments.DocumentPeriodDescriptions.Periodic));
			}
			else if (!RequiredDoc.EQ_ValidToDate.IsEmpty && RequiredDoc.EQ_ValidToDate < ZDateTime.Now.Date)
			{
				RequiredDoc.EQ_ValidToDateInfo.AddWarning(Res.GetString("4a54e29e-2859-4819-848d-173629addeb7", "The document expiry date is entered as a date in the past. This means the document is no longer valid."));
			}
			if (RequiredDoc.IsPowerOfAttorney && RequiredDoc.IsPeriodic && !RequiredDoc.EQ_ValidToDate.IsEmpty)
			{
				if (!RequiredDoc.EQ_ValidToDate.Date.IsEmpty && RequiredDoc.EQ_ValidToDate.Date.IsValid && ZDateTime.Now.Date > RequiredDoc.EQ_ValidToDate.Date.AddDays(Core.Constants.TimeUntilDocumentExpiry.Days))
				{
					RequiredDoc.AddRowWarning(Res.GetString("c537f668-f2c5-42ba-9310-e7cc5811526b", "This document will expire on {0}.", RequiredDoc.EQ_ValidToDate));
				}
				if (RequiredDoc.EQ_ValidToDate < ZDateTime.Now.Date)
				{
					RequiredDoc.AddRowWarning(Res.GetString("4cec9ae3-cacd-4968-99cb-14e3c56aacf0", "This Document has now expired. A new Power of Attorney is required."));
				}
			}
			if (CheckDateReceivedAndValidToDateNeedToBeInTheSameYear())
			{
				RequiredDoc.EQ_ValidToDateInfo.AddError(dateReceivedAndValidToDateMustInTheSameYear);
			}
		}

		bool CheckDateReceivedAndValidToDateNeedToBeInTheSameYear()
		{
			return RequiredDoc.EQ_DocType == Core.Constants.RefDocTypes.VATExporterExemption
				&& RequiredDoc.EQ_ValidToDate.IsValid && RequiredDoc.EQ_DateReceived.IsValid && RequiredDoc.EQ_ValidToDate.Year != RequiredDoc.EQ_DateReceived.Year
				&& RequiredDoc.RelatedCountrySupportDeclarationOfIntent
				&& GlbCompany.CurrentCompany.Country.SupportDeclarationOfIntent;
		}

		protected override void CheckEQ_ValidToDateIsValidZDateTimeRange()
		{
			var typeValidationLimits = new TypeValidationLimits { FutureYearsBeforeError = 10 };
			if (RequiredDoc.IsTaiwanAttorney)
			{
				typeValidationLimits.FutureYearsBeforeWarning = 5;
			}

			TypeValidation.CheckValidZDateTimeRange(Parent.EQ_ValidToDateInfo, typeValidationLimits);
		}

		protected override void CheckEQ_DateReceived()
		{
			base.CheckEQ_DateReceived();

			if (RequiredDoc.IsPeriodic && RequiredDoc.EQ_DateReceived.IsEmpty)
			{
				RequiredDoc.EQ_DateReceivedInfo.AddError(Res.GetString("aa362722-2c39-4003-b40b-64ae2a8eb608", "Received date is required to be entered for periodic documents."));
			}
			else if (!RequiredDoc.EQ_DateReceived.IsEmpty && RequiredDoc.EQ_DateReceived.IsValid && RequiredDoc.EQ_DateReceived.IsValid && RequiredDoc.EQ_DateReceived > ZDateTimeOffset.Now.AddDays(1))
			{
				RequiredDoc.EQ_DateReceivedInfo.AddWarning(Res.GetString("4169a802-6620-49c1-9b93-3018d2ba6366", "The document received date is entered as a future date. Please ensure that this is correct."));
			}
			if (CheckDateReceivedAndValidToDateNeedToBeInTheSameYear())
			{
				RequiredDoc.EQ_DateReceivedInfo.AddError(dateReceivedAndValidToDateMustInTheSameYear);
			}
		}

		protected override void CheckEQ_DocDescription()
		{
			base.CheckEQ_DocDescription();

			if (RequiredDoc.EQ_DocType == Constants.RefDocTypes.MiscellaneousDocument && RequiredDoc.EQ_DocDescription.IsEmpty)
			{
				RequiredDoc.EQ_DocDescriptionInfo.AddError(Res.GetString("d8051523-a697-4b30-80ec-db27b411a58c", "Please enter a description if the document type is {0}.",
					Constants.RefDocTypeDescriptions.MiscellaneousDocument));
			}

			TranslatableDataFieldAttribute.Validate(Parent.EQ_DocDescriptionInfo);
		}

		#region Implementation

		protected JobRequiredDocument RequiredDoc { get; }

		public void AddRowErrorForCostaRicaIfApplicable()
		{
			if (RequiredDoc != null)
			{
				RequiredDoc.ClearRowNotifications();

				if (RequiredDoc.IsCostaRicaExporterExemptionDocumentForDebtor)
				{
					AddRowErrorForMissingAttributeForCostaRica(RequiredDoc, JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType);
					AddRowErrorForMissingAttributeForCostaRica(RequiredDoc, JobRequiredDocAttribTypeList.Codes.IssuingAuthorityName);
				}
			}
		}

		void AddRowErrorForMissingAttributeForCostaRica(JobRequiredDocument reqDoc, string attributeName)
		{
			var attribute = reqDoc.Attributes[attributeName];
			if (attribute == null)
			{
				reqDoc.AddRowError(GetMessageForCostaRicaEXVAttributeMissing(attributeName));
			}
		}

		public static string GetMessageForCostaRicaEXVAttributeMissing(string attributeName)
			=> ResString.GetMultilingualString("B22663CB-8E59-442E-BF59-1FFE612037AB", "Attribute: '{0}' is required for Country/Region: 'Costa Rica', Document Type: 'EXV' and Usage: 'DBT'", attributeName);

		#endregion
	}
}
