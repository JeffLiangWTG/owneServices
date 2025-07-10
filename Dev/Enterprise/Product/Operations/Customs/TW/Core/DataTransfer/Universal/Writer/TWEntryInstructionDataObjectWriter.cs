using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class TWEntryInstructionDataObjectWriter : CustomsEntryInstructionDataObjectWriter
	{
		public TWEntryInstructionDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, IEnumerable<ZGuid> caHeaderPKsToPopulate = null) : base(manager, helper)
		{
			cAHeaderPKsToPopulate = caHeaderPKsToPopulate;
			writerHelper = helper as TWDataObjectWriterHelper;
		}
		readonly IEnumerable<ZGuid> cAHeaderPKsToPopulate;
		readonly TWDataObjectWriterHelper writerHelper;

		protected override List<AddInfoGroup> GetEntryInstructionAddInfoGroupCollection(Customs.Business.CusEntryInstruction instructionBO)
		{
			var result = base.GetEntryInstructionAddInfoGroupCollection(instructionBO) ?? new List<AddInfoGroup>();
			if (instructionBO is CusEntryInstruction instructionBoTW)
			{
				PopulateControllingMessageHeader(result, instructionBoTW, new CodeDescriptionPair
				{
					Code = Constants.EntryInstruction.Codes.CM,
					Description = Constants.EntryInstruction.Descriptions.CM
				});
			}
			return result;
		}

		IEnumerable<CusTWControllingMessageHeader> GetControllingMessageHeadersToPopulate(CusEntryInstruction instructionBoTW)
		{
			return cAHeaderPKsToPopulate == null ? instructionBoTW.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>()
				: instructionBoTW.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().Where(x => cAHeaderPKsToPopulate.Contains(x.PK));
		}

		ZString GetKeyByAddInfoName(ZString name)
		{
			var prefix = new ZString("TW1_");
			return name.Replace(prefix, ZString.Empty);
		}

		void PopulateControllingMessageHeader(List<AddInfoGroup> result, CusEntryInstruction instructionBoTW, CodeDescriptionPair groupType)
		{
			foreach (var controllingMessageHeader in GetControllingMessageHeadersToPopulate(instructionBoTW))
			{
				var infoGroup = new AddInfoGroup(writeManager.WriterStrategy)
				{
					Type = groupType,
					AddInfoCollection = new List<AddInfo>()
				};

				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.MessageNumber, controllingMessageHeader.TW1_FunctionalReferenceId);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.PermitNumber, controllingMessageHeader.PermitNumber);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ControllingAgency, controllingMessageHeader.TW1_ControllingAgency);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.BusinessType, controllingMessageHeader.TW1_BusinessType);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ProcessingUnit, controllingMessageHeader.TW1_ProcessingUnit);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.PaymentMethod, controllingMessageHeader.TW1_PaymentMethod);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ProofOfPaper, controllingMessageHeader.TW1_ProofOfPaper);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ElectronicReceipt, controllingMessageHeader.TW1_ElectronicReceipt);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.AppointmentDate, controllingMessageHeader.TW1_AppointmentDate);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.AppointmentPeriod, controllingMessageHeader.TW1_AppointmentPeriod);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.Request, controllingMessageHeader.TW1_RequestDescription);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ControllingMessageType, controllingMessageHeader.TW1_ControllingMessageType);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.Purpose, controllingMessageHeader.TW1_Purpose);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.PreviousPermitNumber, controllingMessageHeader.TW1_PrePermitNumber);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.InspectionRegistrationNumber, controllingMessageHeader.TW1_InspectionRegistrationNumber);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.PreviousWineInspectionStatus, controllingMessageHeader.TW1_PreWineInspectionStatus);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ApplyForSampleReturn, controllingMessageHeader.TW1_ApplyForSampleReturn);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.SamplingReductionReason, controllingMessageHeader.TW1_SamplingReductionReason);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.SampleReturnAddress, controllingMessageHeader.TW1_SampleReturnAddress);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.CertificateType, controllingMessageHeader.TW1_CertificateType);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.Link, writerHelper?.AllocateControllingMessageHeaderLink(controllingMessageHeader.PK));
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_IsEstimatedLoadingDate)), controllingMessageHeader.TW1_IsEstimatedLoadingDate);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_IsSpecialApplication)), controllingMessageHeader.TW1_IsSpecialApplication);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_SpecialApplicationId)), controllingMessageHeader.TW1_SpecialApplicationId);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_CopyQuantity)), controllingMessageHeader.TW1_CopyQuantity);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_OriginalQuantity)), controllingMessageHeader.TW1_OriginalQuantity);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_EUSteelProductNo)), controllingMessageHeader.TW1_EUSteelProductNo);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_EUSteelProductPhase)), controllingMessageHeader.TW1_EUSteelProductPhase);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_ManufacturerPrintingCode)), controllingMessageHeader.TW1_ManufacturerPrintingCode);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_ReturnPreviousCOO)), controllingMessageHeader.TW1_ReturnPreviousCOO);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_PrintingCode)), controllingMessageHeader.TW1_PrintingCode);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_IsTriangularTrade)), controllingMessageHeader.TW1_IsTriangularTrade);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_Remarks)), controllingMessageHeader.TW1_Remarks);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, GetKeyByAddInfoName(nameof(controllingMessageHeader.TW1_Observations)), controllingMessageHeader.TW1_Observations);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.NX101_Notes, controllingMessageHeader.TW_Notes);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, PredefinedNoteTypes.Instance.ECFAPrintedRemarks.Description, controllingMessageHeader.TW1_ECFAPrintedRemarks);
				if (controllingMessageHeader.ProductLabelRanges.Any()
					|| controllingMessageHeader.EthanolPermitNumbers.Any()
					|| controllingMessageHeader.PreviousDocumentNumbers.Any()
					|| controllingMessageHeader.CertificateOfOrigins.Any())
				{
					infoGroup.AddInfoGroupCollection = new List<AddInfoGroup>();

					PopulateControllingMessageHeaderLabels(infoGroup.AddInfoGroupCollection, controllingMessageHeader, new CodeDescriptionPair() { Code = Constants.EntryInstruction.Codes.LB, Description = Constants.EntryInstruction.Descriptions.LB });
					PopulateControllingMessageHeaderEthanolPermitNumbers(infoGroup.AddInfoGroupCollection, controllingMessageHeader, new CodeDescriptionPair { Code = Constants.EntryInstruction.Codes.EPN, Description = Constants.EntryInstruction.Descriptions.EPN });
					PopulatePreviousDocumentNumbers(result, controllingMessageHeader, new CodeDescriptionPair
					{
						Code = CusSupportingInfoTypeList.Codes.PreviousDocumentNumber,
						Description = CusSupportingInfoTypeList.Descriptions.PreviousDocumentNumber
					});

					PopulateCertificateOfOriginNumbers(result, controllingMessageHeader, new CodeDescriptionPair
					{
						Code = CusSupportingInfoTypeList.Codes.CmCertificateOfOriginNumber,
						Description = CusSupportingInfoTypeList.Descriptions.CmCertificateOfOriginNumber
					});
				}

				PopulateControllingMessageHeaderAddresses(infoGroup, controllingMessageHeader);

				result.Add(infoGroup);
			}
		}

		void PopulatePreviousDocumentNumbers(List<AddInfoGroup> result, CusTWControllingMessageHeader controllingMessageHeader, CodeDescriptionPair groupType)
		{
			if (controllingMessageHeader.PreviousDocumentNumbers.Any())
			{
				var pdnInfoGroup = new AddInfoGroup
				{
					Type = groupType,
					AddInfoCollection = new List<AddInfo>()
				};
				foreach (var pdn in controllingMessageHeader.PreviousDocumentNumbers.Cast<PreviousDocumentNumberCusSupporting>())
				{
					pdnInfoGroup.AddInfoCollection.Add(new AddInfo()
					{
						Value = pdn.CSI_ReferenceNumber
					});
				}
				result.Add(pdnInfoGroup);
			}
		}

		void PopulateCertificateOfOriginNumbers(List<AddInfoGroup> result, CusTWControllingMessageHeader controllingMessageHeader, CodeDescriptionPair groupType)
		{
			if (controllingMessageHeader.CertificateOfOrigins.Any())
			{
				var conInfoGroup = new AddInfoGroup
				{
					Type = groupType,
					AddInfoCollection = new List<AddInfo>()
				};
				foreach (var con in controllingMessageHeader.CertificateOfOrigins.Cast<CMCertificateOfOriginCusSupporting>())
				{
					conInfoGroup.AddInfoCollection.Add(new AddInfo()
					{
						Value = con.CSI_ReferenceNumber
					});
				}
				result.Add(conInfoGroup);
			}
		}

		void PopulateControllingMessageHeaderLabels(List<AddInfoGroup> result, CusTWControllingMessageHeader controllingMessageHeader, CodeDescriptionPair groupType)
		{
			foreach (var label in controllingMessageHeader.ProductLabelRanges.Cast<CusTWProductLabelRange>())
			{
				var lableInfoGroup = new AddInfoGroup
				{
					Type = groupType,
					AddInfoCollection = new List<AddInfo>()
				};
				UpdateAddInfoCollection(lableInfoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ProductLabelRages.Status, label.TW0_Status);
				UpdateAddInfoCollection(lableInfoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ProductLabelRages.EndNumber, label.TW0_EndNumber);
				UpdateAddInfoCollection(lableInfoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ProductLabelRages.StartNumber, label.TW0_StartNumber);
				UpdateAddInfoCollection(lableInfoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ProductLabelRages.RunNumber, label.TW0_RunNumber);
				UpdateAddInfoCollection(lableInfoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.ProductLabelRages.Year, label.TW0_Year);

				result.Add(lableInfoGroup);
			}
		}

		void PopulateControllingMessageHeaderEthanolPermitNumbers(List<AddInfoGroup> result, CusTWControllingMessageHeader controllingMessageHeader, CodeDescriptionPair groupType)
		{
			foreach (var epn in controllingMessageHeader.EthanolPermitNumbers.Cast<EthanolPermitNumberCusSupporting>())
			{
				var epnInfoGroup = new AddInfoGroup
				{
					Type = groupType,
					AddInfoCollection = new List<AddInfo>()
				};
				UpdateAddInfoCollection(epnInfoGroup.AddInfoCollection, Constants.AddInfoKeys.ControllingMessage.EthanolPermitNumbers.EthanolPermitNumber, epn.CSI_ReferenceNumber);

				result.Add(epnInfoGroup);
			}
		}

		void PopulateControllingMessageHeaderAddresses(AddInfoGroup result, CusTWControllingMessageHeader controllingMessageHeader)
		{
			var docAddressTypes = GetControllingMessageHeaderDocAddressTypes(controllingMessageHeader).ToHashSet();
			if (docAddressTypes.Count > 0)
			{
				var docAddresses = controllingMessageHeader.DocAddresses.Cast<JobDocAddress>().Where(c => docAddressTypes.Contains(c.DocAddressType));
				result.SetOrganizationAddressCollection(() => result.OrganizationAddressCollection.MergeCollection(ProcessCollection(docAddresses, new JobDocAddressDataObjectWriter(writeManager)), false, UniversalCommonHelper.IsOrganizationAddressTypeMatched));
			}
		}

		IEnumerable<DocAddressType> GetControllingMessageHeaderDocAddressTypes(CusTWControllingMessageHeader controllingMessageHeader)
		{
			if (!controllingMessageHeader.LocalProcessorAddress.IsEmpty)
			{
				yield return DocAddressType.LocalProcessorAddress;
				yield return DocAddressType.LocalProcessorTranslatedDocAddress;
			}

			if (!controllingMessageHeader.ApplicantDocumentaryAddress.IsEmpty)
			{
				yield return DocAddressType.Applicant;
				yield return DocAddressType.ApplicantTranslatedDocumentaryAddress;
			}

			if (!controllingMessageHeader.SupplierDocumentaryAddress.IsEmpty)
			{
				yield return DocAddressType.SupplierDocumentaryAddress;
				yield return DocAddressType.SupplierTranslatedDocumentaryAddress;
			}

			if (!controllingMessageHeader.ImporterDocumentaryAddress.IsEmpty)
			{
				yield return DocAddressType.ImporterDocumentaryAddress;
				yield return DocAddressType.ImporterTranslatedDocumentaryAddress;
			}
		}

		void UpdateAddInfoCollection(List<AddInfo> addInfoList, ZString key, IZType value)
		{
			if (!value.IsEmpty && !value.IsDefault)
			{
				helper.Update(addInfoList, key, value);
			}
		}

		protected override List<AddInfo> GetEntryInstructionAddInfoCollection(Customs.Business.CusEntryInstruction instructionBO)
		{
			var addInfoList = base.GetEntryInstructionAddInfoCollection(instructionBO) ?? new List<AddInfo>();

			if (instructionBO is CusEntryInstruction instructionBoTW)
			{
				addInfoList.Add(new AddInfo()
				{
					Key = PredefinedNoteTypes.Instance.TWTradersRemarks.Description,
					Value = instructionBoTW.TW_TradersRemarks
				});

				addInfoList.Add(new AddInfo()
				{
					Key = Constants.AddInfoKeys.Declaration.BrokerLicense,
					Value = instructionBoTW.JobDeclaration?.CusAgentCertificateNumber ?? ZString.Empty
				});
			}

			return addInfoList;
		}
	}
}
