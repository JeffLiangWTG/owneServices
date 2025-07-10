namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.Customs.Business.MultiLineAddInfos;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;
	using Enterprise.DocumentEngine.FlexCelInterface;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;

	partial class MAFMessagingBO : IMAFMessagingRequest
	{
		#region Implementation of IMAFMessagingRequest

		ZString IMAFMessagingRequest.ApplicationName
		{
			get { return Ebacca; }
		}

		ZString IMAFMessagingRequest.ApplicationVersion
		{
			get { return "1.0.0"; }
		}

		ZString IMAFMessagingRequest.DocumentType
		{
			get { return Ebacca; }
		}

		ZString IMAFMessagingRequest.SenderName
		{
			get { return GlbCompany.CurrentCompany.GC_Name; }
		}

		EndPointTypeEndPointType IMAFMessagingRequest.SenderEndPointType
		{
			get { return EndPointTypeEndPointType.Email; }
		}

		ZString IMAFMessagingRequest.SenderAddress
		{
			get { return Env.Registry.MailboxEmailAddress; }
		}

		ZString IMAFMessagingRequest.CallerRefID
		{
			get { return NZMMessage.MessageNumberPlaceHolder; }
		}

		IMAFMessagingMetaData IMAFMessagingRequest.MetaData
		{
			get { return new MetaDataWrapper(this); }
		}

		IEnumerable<IMAFFile> IMAFMessagingRequest.Files
		{
			get
			{
				var eDocs = PlugInSupport.DocManagerInfo.AllEDocs;
				return from CusAddInfo<MAFFile> fileInfo in Files select (IMAFFile)new MAFFileWrapper(fileInfo.Data, eDocs);
			}
		}

		const string Ebacca = "EBACCA";

		#endregion

		#region MetaDataWrapper

		class MetaDataWrapper : IMAFMessagingMetaData
		{
			internal MetaDataWrapper(MAFMessagingBO mafMessaging)
			{
				this.mafMessaging = mafMessaging;
			}

			#region Implementation of IMAFMessagingMetaData

			bool? IMAFMessagingMetaData.IsMAFAuditRequiredByCustoms
			{
				get { return YesNoUnknownList.GetValueForCode(mafMessaging.ZX_IsMAFAuditRequiredByCustoms); }
			}

			bool? IMAFMessagingMetaData.IsCustomsXRayRequired
			{
				get { return YesNoUnknownList.GetValueForCode(mafMessaging.ZX_IsCustomsXRayRequired); }
			}

			bool? IMAFMessagingMetaData.IsCustomsCashClient
			{
				get { return YesNoUnknownList.GetValueForCode(mafMessaging.ZX_IsCustomsCashClient); }
			}

			ZString IMAFMessagingMetaData.MAFConsignmentNumber
			{
				get { return mafMessaging.ZX_ConsignmentNumber; }
			}

			ZString IMAFMessagingMetaData.MAFReceiptNumber
			{
				get { return mafMessaging.ZX_ReceiptNumber; }
			}

			ZString IMAFMessagingMetaData.ClientReferenceNumber
			{
				get { return NZMMessage.SendersReferencePlaceHolder; }
			}

			ZString IMAFMessagingMetaData.AlternativePaymentMethod
			{
				get { return mafMessaging.PaymentTypeToBeSent != MAFPaymentMethodList.Codes.Account ? mafMessaging.PaymentTypeToBeSent : ZString.Empty; }
			}

			ZString IMAFMessagingMetaData.Comments
			{
				get { return mafMessaging.ZX_Comments; }
			}

			IMAFMessagingSource IMAFMessagingMetaData.Source
			{
				get { return mafMessaging.PlugInSupport; }
			}

			#endregion

			#region Implementation of IMAFMessagingFallback

			public IMAFOrganisation Importer
			{
				get
				{
					return MAFOrganisationWrapper.GetMAFOrganisation(
						mafMessaging.PlugInSupport.Importer,
						mafMessaging.EffectiveImporterContactName,
						mafMessaging.EffectiveImporterContactPhone,
						mafMessaging.EffectiveImporterContactFax,
						mafMessaging.EffectiveImporterContactEmail);
				}
			}

			public IMAFOrganisation Exporter
			{
				get
				{
					return MAFOrganisationWrapper.GetMAFOrganisation(
						mafMessaging.PlugInSupport.Exporter,
						mafMessaging.EffectiveExporterContactName,
						mafMessaging.EffectiveExporterContactPhone,
						mafMessaging.EffectiveExporterContactFax,
						mafMessaging.EffectiveExporterContactEmail);
				}
			}

			public ZString ProcessingOffice
			{
				get { return mafMessaging.EffectiveProcessingOffice; }
			}

			public ZString ConsignmentType
			{
				get { return mafMessaging.EffectiveConsignmentType; }
			}

			public ZString CargoType
			{
				get { return mafMessaging.EffectiveCargoType; }
			}

			public ZString MeasurementUQ
			{
				get { return mafMessaging.EffectiveMeasurementUQ; }
			}

			public ZInt MeasurementValue
			{
				get { return mafMessaging.EffectiveMeasurementValue; }
			}

			public IMAFAccountDetails AccountDetails
			{
				get
				{
					return !mafMessaging.IsPaymentMethodOverriden ? mafMessaging.PlugInSupport.AccountDetails
						: MAFAccountDetailsWrapper.GetAccountDetails(mafMessaging.AccountNumberToBeSent, mafMessaging.AccountHolderNameToBeSent);
				}
			}

			#endregion

			readonly MAFMessagingBO mafMessaging;
		}

		#endregion
	}
	#region MAFFileWrapper
	class MAFFileWrapper : IMAFFile
	{
		public MAFFileWrapper(MAFFile file, IStorageDocsBaseCollection eDocs)
		{
			this.file = file;
			this.eDocs = eDocs;
		}

		#region Implementation of IMAFFile

		ZString IMAFFile.FileName
		{
			get { return file.ZF_FileName; }
		}

		ZString IMAFFile.ContentType
		{
			get { return "PDF"; }
		}

		ZString IMAFFile.DocumentType
		{
			get { return file.ZF_DocumentType; }
		}

		MessagingRequestTypeBodyFileDataDataEncoding IMAFFile.DataEncoding
		{
			get { return MessagingRequestTypeBodyFileDataDataEncoding.Base64; }
		}

		#region DataContent

		ZString IMAFFile.DataContent
		{
			get
			{
				var result = string.Empty;
				if (!file.ZF_EDocsUniqueID.IsEmpty && file.ZF_EDocsUniqueID.IsValid)
				{
					var eDoc = eDocs.GetFromUniqueKey(file.ZF_EDocsUniqueID.ToGuid());
					if (eDoc != null)
					{
						if (ImageToPDFConverter.IsPDF(eDoc.ImageData))
						{
							result = EncodeForDataContent(eDoc.ImageData);
						}
						else
						{
							var pdfData = new ImageToPDFConverter().ConvertToPDF(eDoc.ImageData);
							if (pdfData == null)
							{
								ThrowCorruptedFileException();
							}

							result = EncodeForDataContent(pdfData);
						}
					}
				}
				return result;
			}
		}

		static string EncodeForDataContent(byte[] contents)
		{
			return Convert.ToBase64String(contents, Base64FormattingOptions.InsertLineBreaks);
		}

		void ThrowCorruptedFileException()
		{
			throw new MAFMessageException(Res.GetString("ff380886-4fb3-4724-9468-49ca3af6df53", "File [{0}] appears to be corrupted. Could not load as Image to convert to PDF.", ((IMAFFile)this).FileName));
		}

		#endregion

		#endregion

		readonly MAFFile file;
		readonly IStorageDocsBaseCollection eDocs;
	}
	#endregion
}
