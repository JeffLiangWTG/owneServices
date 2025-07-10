using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.NZMAFFiles)]
	public class MAFFile : AutoNZMAFFilesAddInfo
	{
		public new class Schema : AutoNZMAFFilesAddInfo.Schema
		{
			public const string ZF_DocumentTypeDescription = "ZF_DocumentTypeDescription";
		}

		public MAFFile(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public MAFMessagingBO MAFMessaging { get; internal set; }

		[List(nameof(Lookups) + "." + nameof(NZMAFFilesAddInfoLookups.AvailableEDocs), "PK", "Code", AllowOnlyTheseValues = true)]
		public override ZGuid ZF_EDocsUniqueID
		{
			get { return base.ZF_EDocsUniqueID; }
			set
			{
				base.ZF_EDocsUniqueID = value;
				DefaultFileNameFromEDoc(value);
			}
		}

		void DefaultFileNameFromEDoc(ZGuid eDocsUniqueID)
		{
			ZString fileName = Lookups.AvailableEDocs.GetFileNameFromPK(eDocsUniqueID);
			ZF_FileName = ((ZString)Path.GetFileNameWithoutExtension(fileName)).Left(ZF_FileNameInfo.MaxLength - 4) + ".PDF";
		}

		[List(nameof(Lookups) + "." + nameof(NZMAFFilesAddInfoLookups.DocumentTypes), AllowOnlyTheseValues = true)]
		public override ZString ZF_DocumentType
		{
			get { return base.ZF_DocumentType; }
			set { base.ZF_DocumentType = value; }
		}

		public ZString ZF_DocumentTypeDescription
		{
			get { return Lookups.DocumentTypes.GetDescriptionFromCode(ZF_DocumentType); }
		}

		public ZPropertyInfo ZF_DocumentTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ZF_DocumentTypeDescription); }
		}
	}
}
