using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class eDocForTesting : IeDoc
	{
		public eDocForTesting(ZString docType, ZString fileName, ZString description, ZDateTime dateAdded, BusinessObject bizO, BusinessObjectFactory bizOFactory, ZString type)
		{
			UniqueKey = ZGuid.NewZGuid();
			DocType = docType;
			FileName = fileName;
			Description = description;
			DateAdded = dateAdded;
			storageMain = (BusinessObject)bizOFactory.New<IStorageMain>();
			//storageMain[StorageMainSchema.SM_ParentFK] = bizO.PK;
			//storageMain[StorageMainSchema.SM_CD1] = 0;
			//storageMain[StorageMainSchema.SM_CD2] = 0;
			//storageMain[StorageMainSchema.SM_DB] = 1;
			storageMain[StorageMainSchema.SM_Type] = type;
			//storageMain[StorageMainSchema.SM_PhysicalLocation] = string.Empty;
		}

		public ZDateTime DateAdded
		{
			get;
			set;
		}

		public ZString Description
		{
			get;
			set;
		}

		public ZString DocType
		{
			get;
			set;
		}

		public ZString DocSourceDescription
		{
			get;
			set;
		}

		public ZString DocSource
		{
			get;
			set;
		}

		public CodeDescriptionPairList DocType_List
		{
			get { return new CodeDescriptionPairList(); }
		}

		public ZString FileName
		{
			get;
			private set;
		}

		public void Delete()
		{
			throw new NotImplementedException();
		}

		public ZBlob ImageData
		{
			get;
			set;
		}

		public ZBool IsDeleted
		{
			get;
			set;
		}

		public ZBool IsPublished
		{
			get;
			set;
		}

		public ZString VisibleCompanyCode
		{
			get { return string.Empty; }
		}

		public ZString VisibleBranchCode
		{
			get { return string.Empty; }
		}

		public ZString VisibleDepartmentCode
		{
			get { return string.Empty; }
		}

		public ZBool IsSystemGenerated
		{
			get { return false; }
		}

		public ZDateTime LastEdited
		{
			get { return DateAdded.AddDays(1); }
		}

		public ZString LastEditedUser
		{
			get { return GlbStaff.CurrentUser.GS_Code; }
		}

		public void NotifyReadByUser()
		{
		}

		public ZBool IsCustomisableDocTypes
		{
			get { return false; }
		}

		public ZGuid UniqueKey
		{
			get;
			private set;
		}

		public BusinessObject ParentMain
		{
			get
			{
				return storageMain;
			}
		}
		readonly BusinessObject storageMain;
		//DummyBusinessObject dummy;

		public void SetValuesForTest(ZDateTime dateTime, ZString dataType)
		{
		}

		public ZString DataType
		{
			get
			{
				return null;
			}
		}

		public ZString FileNameOnly
		{
			get
			{
				return null;
			}
		}

		public ZDecimal FileSizeInMB { get; private set; }

		public IDisposable OpenForEdit()
		{
			throw new NotImplementedException();
		}

		public Stream GetImageDataReader() => new CargoWise.IO.Shim.SubStreamableStream();
		public void SetImageDataStream(Stream stream) { }

		public string CreateReference()
		{
			throw new NotImplementedException();
		}
	}
}
