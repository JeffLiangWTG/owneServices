using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class CusOutturnDataObjectReader<TCusOutturn> : ShipmentDataObjectReader<TCusOutturn>
		where TCusOutturn : CusOutturn
	{
		public CusOutturnDataObjectReader(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, CusOutturnHeader outturnheader) : base(shipment, logger, factory)
		{
			this.outturnheader = Argument.NotNull(outturnheader, nameof(outturnheader));
			outturnheaderRow = GetColumnIndexer(outturnheader);
		}

		#region override
		public override DataContextType DataContextType => DataContextType.Outturn;

		protected override IMatchingBusinessEntityFinder<TCusOutturn> GetCombinedReferenceMatcher() => null;

		protected override TCusOutturn GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		protected override TCusOutturn GetNewBusinessObject()
		{
			return (TCusOutturn)outturnheader.Outturns.AddNew();
		}

		protected override void PopulateBusinessObject(TCusOutturn targetBO)
		{
			using (SuspendSetters(targetBO))
			{
				var outturnRow = GetColumnIndexer(targetBO);
				var outturnHeaderPK = outturnheaderRow.GetValue(CusOutturnHeaderSchema.PK);
				SetValue(outturnRow, CusOutturnSchema.C5_C6, outturnHeaderPK);
				SetValue(outturnRow, CusOutturnSchema.C5_OuterPackUnits, dataObject.OuterPacksPackageType.GetNullableCodeAsUpperCase());
				SetValue(outturnRow, CusOutturnSchema.C5_PackagesUnits, dataObject.TotalNoOfPacksPackageType.GetNullableCodeAsUpperCase());
				SetValue(outturnRow, CusOutturnSchema.C5_OuterPacks, dataObject.OuterPacks);
				SetValue(outturnRow, CusOutturnSchema.C5_PackagesOutturned, dataObject.TotalNoOfPacks);

				FillDates(outturnRow);
				FillContainers(outturnRow);
				FillAddInfos(outturnRow);
				FillNotes(outturnRow);
				FillAdditionalBills(outturnRow);
				FillPackingLines(outturnRow);
				FillCountrySpecificDetails(outturnRow);
			}
		}
		#endregion

		#region Fill business object
		protected virtual void FillCountrySpecificDetails(IColumnIndexer outturnRow)
		{
		}

		void FillDates(IColumnIndexer outturnRow)
		{
			if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
			{
				FillDates(outturnRow, dataObject.DateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(CusOutturnSchema.C5_CargoUnpackDate, new[] { DateType.Unpack }),
					new DateTypeSchemaColumnMap(CusOutturnSchema.C5_CargoReceiptDate, new[] { DateType.Received }));
			}
		}

		void FillContainers(IColumnIndexer outturnRow)
		{
			if (dataObject.ContainerCollection != null && dataObject.ContainerCollection.Count > 0)
			{
				var container = dataObject.ContainerCollection.FirstOrDefault();
				SetValue(outturnRow, CusOutturnSchema.C5_CargoType, container.ContainerType.GetNullableCodeAsUpperCase());
				SetValue(outturnRow, CusOutturnSchema.C5_ContainerNumber, container.ContainerNumber);
				SetValue(outturnRow, CusOutturnSchema.C5_ContainerSeal, container.Seal);
				SetValue(outturnRow, CusOutturnSchema.C5_SealIntactIndicator, container.IsSealOk);
			}
		}

		void FillAddInfos(IColumnIndexer outturnRow)
		{
			var addInfos = dataObject.AddInfoCollection;
			if (addInfos != null && addInfos.Count > 0)
			{
				FillAddInfosCore(outturnRow, addInfos, CusOutturnSchema.C5_DamageIndicator, Constants.AddInfoKeys.Outturn.IsDamage);
				FillAddInfosCore(outturnRow, addInfos, CusOutturnSchema.C5_PillageIndicator, Constants.AddInfoKeys.Outturn.IsPillage);
			}
		}

		void FillAddInfosCore(IColumnIndexer outturnRow, IEnumerable<AddInfo> addInfos, SchemaBoolColumn column, ZString key)
		{
			var addInfo = addInfos.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
			if (addInfo != null)
			{
				SetValue(outturnRow, column, addInfo.Value.GetValueOrDefault() == YesNoList.Codes.Yes ? ZBool.True : ZBool.False);
			}
		}

		void FillNotes(IColumnIndexer outturnRow)
		{
			var notes = dataObject.NoteCollection;
			if (notes != null && notes.Count > 0)
			{
				FillNoteCore(outturnRow, notes, CusOutturnSchema.C5_GoodsDescription, Constants.Note.Descriptions.GoodsDescription);
				FillNoteCore(outturnRow, notes, CusOutturnSchema.C5_MarksAndNumbers, Constants.Note.Descriptions.MarksAndNumbersDescription);
			}
		}

		void FillNoteCore(IColumnIndexer outturnRow, IEnumerable<Note> notes, SchemaStringColumn column, ZString description)
		{
			var note = notes.FirstOrDefault(x => x.Description.GetValueOrDefault() == description);
			if (note != null)
			{
				SetValue(outturnRow, column, note.NoteText);
			}
		}

		void FillAdditionalBills(IColumnIndexer outturnRow)
		{
			var additionalBills = dataObject.AdditionalBillCollection;
			if (additionalBills != null && additionalBills.Count > 0)
			{
				var additionalBill = additionalBills.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House);
				if (additionalBill != null)
				{
					SetValue(outturnRow, CusOutturnSchema.C5_HouseBill, additionalBill.BillNumber);
					SetValue(outturnRow, CusOutturnSchema.C5_MasterBill, additionalBill.ParentBillNumber);
				}
			}
		}

		void FillPackingLines(IColumnIndexer outturnRow)
		{
			var packingLines = dataObject.PackingLineCollection;
			if (packingLines != null && packingLines.Count > 0)
			{
				var packingLine = packingLines.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House);
				if (packingLine != null)
				{
					SetValue(outturnRow, CusOutturnSchema.C5_VolumeOutturned, packingLine.OutturnedVolume);
				}
			}
		}
		#endregion

		#region SuspendSetters
		IDisposable SuspendSetters(TCusOutturn outturnHeader)
		{
			return outturnHeader.SetterSuspender.SuspendSetting(GetOutturnPropertiesToSuspendSetting().ToArray());
		}

		protected virtual IEnumerable<ZString> GetOutturnPropertiesToSuspendSetting() => Array.Empty<ZString>();
		#endregion

		protected readonly CusOutturnHeader outturnheader;
		protected readonly IColumnIndexer outturnheaderRow;
	}
}
