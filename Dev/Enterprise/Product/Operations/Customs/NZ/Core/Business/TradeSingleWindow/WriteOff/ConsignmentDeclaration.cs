using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	sealed class ConsignmentDeclaration : Consignment, IWriteOffStatus
	{
		internal ConsignmentDeclaration(WriteOffResponseDeclaration wofResponse, string id, string status)
			: base(wofResponse, id, status, ZString.Empty)
		{
			declaration = wofResponse.EntryHeader.Declaration.JE_HouseBill == id || wofResponse.EntryHeader.Declaration.JE_HouseBill.IsEmpty
				? wofResponse.EntryHeader.Declaration
				: null;

			entryHeader = wofResponse.EntryHeader;
		}

		internal ConsignmentDeclaration(WriteOffResponseManifesting wofResponse, string id, string status)
			: base(wofResponse, id, status, ZString.Empty)
		{
			declaration = wofResponse.EntryHeader.Declarations
											  .Cast<JobDeclaration>()
											  .FirstOrDefault(d => d.JE_HouseBill == id);
			entryHeader = wofResponse.EntryHeader;
		}

		protected override ZString CustomsStatusCore
		{
			get { return declaration != null ? declaration.JE_EntryStatus : ZString.Empty; }
			set
			{
				if (declaration != null)
				{
					bool isCustomsOrBioResponse = false;
					if (Response != null && IsImportManifest)
					{
						declaration.JE_EntryStatus = value;
						declaration.AgencyMessageBeingProcessed = Response.ResponsibleGovernmentAgency;
						if (declaration.AgencyMessageBeingProcessed == TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO)
						{
							declaration.JE_ManifestBioStatus = value;
							isCustomsOrBioResponse = true;
						}
						else if (declaration.AgencyMessageBeingProcessed == TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms)
						{
							declaration.JE_ManifestNZCSStatus = value;
							isCustomsOrBioResponse = true;
						}
					}

					if (!ImportManifestConsignmentHasChangedToFormalEntry)
					{
						declaration.JE_EntryStatus = IsImportManifest && isCustomsOrBioResponse ? declaration.CalculateCombinedJobStatus(declaration.JE_ManifestBioStatus, declaration.JE_ManifestNZCSStatus) : value;
						declaration.JE_ECI_LastResponseStatus = value;
						if (Response != null)
						{
							declaration.AgencyMessageBeingProcessed = Response.ResponsibleGovernmentAgency;
							declaration.JE_TSWCombinedStatus = declaration.IsExport ? GetExportConsignmentStatus(value) : TSWStatus.GetCombinedWriteOffStatus;
						}
					}
				}
			}
		}

		bool IsImportManifest => declaration != null && declaration.IsTSWICRWriteOff && declaration.IsECIManifestDeclarationReference;

		bool ImportManifestConsignmentHasChangedToFormalEntry
		{
			get
			{
				bool result = false;
				if (Response != null && Response.GetType() == typeof(WriteOffResponseManifesting))
				{
					if (declaration.IsImport)
					{
						result = !IsImportManifest;
					}
				}

				return result;
			}
		}

		protected override ZString MessageStatusCore
		{
			get { return declaration != null ? declaration.JE_MessageStatus : ZString.Empty; }
			set
			{
				if (declaration != null)
				{
					declaration.JE_MessageStatus = value;
				}
			}
		}

		ZString GetExportConsignmentStatus(string value)
		{
			var result = value;
			var combinedStatus = declaration.JE_TSWCombinedStatus;

			if (value == LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined || combinedStatus == LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined)
			{
				declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentHeld;
				if (value != LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved)
				{
					// do not override "Effective Hold" status of Transhipment Declined with other Customs statuses, unless Transhipment Approved status is subsequently received from MPI Biosecurity
					result = LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined;
				}
			}

			if (((combinedStatus == LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff ||
				  combinedStatus == LowValueConsignmentStatusList.Codes.CI ||
				  combinedStatus == LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined)
				  && value == LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved) ||
				((combinedStatus == LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved ||
				  combinedStatus == LowValueConsignmentStatusList.Codes.CI)
				  && value == LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff))
			{
				declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
				result = LowValueConsignmentStatusList.Codes.CI;
			}

			return result;
		}

		TSWStatus TSWStatus
		{
			get { return new TSWStatus(this); }
		}

		#region IWriteOffStatus Members

		Declaration.ECIWriteOff.CusEntryHeader IWriteOffStatus.EntryHeader => entryHeader;

		ZString IWriteOffStatus.CustomsStatus => declaration.JE_EntryStatus;

		ZString IWriteOffStatus.GoodsClearanceStatus => Response.GoodsClearanceStatus;

		ZString IWriteOffStatus.CombinedStatus => declaration.JE_TSWCombinedStatus;

		JobDeclaration ITSWStatus.Declaration => declaration;

		ZString ITSWStatus.ResponseStatus => ZString.Empty;

		ZString ITSWStatus.EnterpriseStatus => Status;

		ZString ITSWStatus.Agency => Response.ResponsibleGovernmentAgency;

		#endregion

		protected override ZString HouseBillCore
		{
			get { return declaration != null ? declaration.JE_HouseBill : ZString.Empty; }
		}

		protected override string JobNumberCore
		{
			get { return declaration != null ? declaration.JE_DeclarationReference.ToString() : base.JobNumberCore; }
		}

		protected override void LogCustomsImpediment()
		{
			if (declaration != null)
			{
				declaration.LogCustomsImpediment();
			}
		}

		readonly JobDeclaration declaration;
		readonly Declaration.ECIWriteOff.CusEntryHeader entryHeader;
	}
}

// Tested in
// - CREMessageProcessorDeclarationTest
// - CREMessageProcessorManifestingTest
// - ICRMessageProcessorManifestingTest
// - ICRMessageProcessorDeclarationTest
