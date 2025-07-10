using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff
{
	public class CusEntryHeader : Declaration.CusEntryHeader, Integration.Customs.NZ.IECIWriteOffCusEntryHeader
	{
		public static new readonly CusEntryHeaderTypeDecider TypeDecider = new CusEntryHeaderTypeDecider();

		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region SetDefaultValues
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CH_MessageType = EntryHeaderTypes.NZ.ECIWriteOff;
		}
		#endregion

		#region IsManifestEntry
		public bool IsManifestEntry => CH_MessageType == EntryHeaderTypes.NZ.ECIWriteOffManifest;
		#endregion

		#region Messages

		[ChildEditable(true)]
		public new NZCMessageCollection Messages
		{
			get { return base.Messages; }
		}

		protected override EDIMessageCollection GetNewMessageCollection()
		{
			return new NZCMessageCollection(this);
		}
		#endregion

		#region Declaration
		public new JobDeclaration Declaration
		{
			get { return base.Declaration; }
		}
		#endregion

		#region EntryNumberType

		protected override ZString EntryNumberType
		{
			get { return CusEntryNumberTypeList.Codes.ECIWriteOff; }
		}

		#endregion

		#region Packages
		public override ZInt PackagesCount
		{
			get
			{
				return Declaration.JE_TotalNoOfPacks;
			}
		}
		#endregion

		#region TotalAmountPayable
		public override ZDecimal TotalAmountPayable
		{
			get { return CH_TotalPaid; }
		}
		#endregion

		#region IsECIWriteOff
		public override bool IsECIWriteOff
		{
			get { return true; }
		}
		#endregion

		#region SetDeclarationStatusesWhenSetToCurrent
		public override void SetDeclarationStatusesWhenSetToCurrent(JobDeclaration declaration)
		{
			var isImportManifest = declaration.IsTSWICRWriteOff && declaration.IsECIManifestDeclarationReference;
			if (isImportManifest)
			{
				declaration.JE_EntryStatus = declaration.CalculateCombinedJobStatus(declaration.JE_ManifestBioStatus, declaration.JE_ManifestNZCSStatus);
			}
			else
			{
				if (declaration.JE_ECI_LastResponseStatus.IsEmpty)
				{
					declaration.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;
				}

				if (declaration.JE_ECI_LastResponseStatus == LowValueConsignmentStatusList.Codes.NoStatusReported)
				{
					switch (CH_EntryStatus)
					{
						case LowValueManifestStatusList.Codes.InspectionsAuditRequirements:
							declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentHeld;
							break;
						case LowValueManifestStatusList.Codes.ManifestAccepted:
							declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
							break;
						case LowValueManifestStatusList.Codes.ManifestCancelled:
							declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentCancelled;
							break;
						case LowValueManifestStatusList.Codes.ManifestInError:
							declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
							break;
						case LowValueManifestStatusList.Codes.ManifestRejected:
							declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
							break;
						case LowValueManifestStatusList.Codes.NotSentToCustoms:
							declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;
							break;
						case LowValueManifestStatusList.Codes.SentToCustoms:
							declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
							break;
					}
				}
				else
				{
					declaration.JE_EntryStatus = declaration.JE_ECI_LastResponseStatus;
				}
			}
		}
		#endregion

		protected override void SetMessagingStatusToNotSentInternal()
		{
			CH_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
		}

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);
	}
}
