using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class EntryNumberDataObjectReader : DataObjectReader<EntryNumber, CusEntryNumber>
	{
		public EntryNumberDataObjectReader(EntryNumber entryNumberDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusEntryNumCollection entryNumbers, BusinessObject entryNumberParent)
			: base(entryNumberDataObject, logger, factory)
		{
			Argument.NotNull(entryNumbers, "CusEntryNumCollection entryNumbers");
			this.entryNumberParent = Argument.NotNull(entryNumberParent, "BusinessObject entryNumberParent");
			this.entryNumberBusinessObjectProvider = (dataObj) => new EntryNumberBusinessObjectFinder(dataObject).Find(entryNumbers.Cast<CusEntryNumber>());
		}

		public EntryNumberDataObjectReader(EntryNumber entryNumberDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, BusinessObject entryNumberParent, Func<EntryNumber, CusEntryNumber> entryNumberBusinessObjectProvider)
			: base(entryNumberDataObject, logger, factory)
		{
			this.entryNumberParent = Argument.NotNull(entryNumberParent, "BusinessObject entryNumberParent");
			this.entryNumberBusinessObjectProvider = Argument.NotNull(entryNumberBusinessObjectProvider, "entryNumberBusinessObjectProvider");
		}

		CusEntryNumber entryNumberBO;
		readonly BusinessObject entryNumberParent;
		readonly Func<EntryNumber, CusEntryNumber> entryNumberBusinessObjectProvider;

		#region Implementation

		protected override CusEntryNumber GetExistingBusinessObject()
		{
			var entryNumber = entryNumberBusinessObjectProvider(dataObject);

			if (entryNumber != null)
			{
				isExistingEntrySystemGenerated = entryNumber.CE_EntryIsSystemGenerated;
			}

			return entryNumber;
		}

		bool isExistingEntrySystemGenerated;

		protected override CusEntryNumber GetNewBusinessObject()
		{
			var entryNumberBO = base.GetNewBusinessObject();
			entryNumberBO.CE_ParentID = entryNumberParent.PK;
			entryNumberBO.CE_ParentTable = entryNumberParent.TableName;
			return entryNumberBO;
		}

		protected override void PopulateBusinessObject(CusEntryNumber entryNumberBO)
		{
			var isOutdatedSZBEntryNumber = dataObject.Type.GetCodeAsUpperCase() == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber
				&& dataObject.IssueDate != null && dataObject.IssueDate < entryNumberBO.CE_IssueDate;

			if (HasEmptyEntryValue(dataObject))
			{
				this.entryNumberBO = entryNumberBO;
				entryNumberBO.Factory.Saving += Factory_Saving;
			}

			if (!isOutdatedSZBEntryNumber && !isExistingEntrySystemGenerated)
			{
				SetValue(entryNumberBO, CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				SetValue(entryNumberBO, CusEntryNumSchema.CE_EntryIsSystemGenerated, false);
				SetValue(entryNumberBO, CusEntryNumSchema.CE_EntryLineReference, dataObject.EntryLineReference);
				SetValue(entryNumberBO, CusEntryNumSchema.CE_EntryNum, dataObject.Number);
				SetValue(entryNumberBO, CusEntryNumSchema.CE_EntryStatus, dataObject.EntryStatus);
				SetValue(entryNumberBO, CusEntryNumSchema.CE_EntryType, dataObject.Type);
				SetValue(entryNumberBO, CusEntryNumSchema.CE_ExpiryDate, dataObject.ExpiryDate);
				SetValue(entryNumberBO, CusEntryNumSchema.CE_IssueDate, dataObject.IssueDate);
				SetValue(entryNumberBO, CusEntryNumSchema.CE_RN_NKCountryCode, dataObject.CountryOfIssue);
			}
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			factory.Saving -= Factory_Saving;
			var shipment = entryNumberBO.Parent as CommonShipment;
			if (shipment != null && entryNumberBO != null
				&& shipment.ShipmentCustomsEntryNumber.ShouldDeleteEmptyNumber(entryNumberBO))
			{
				entryNumberBO.Delete();
				var typeName = GetBusinessObjectHumanReadableName(entryNumberBO);
				var reason = Res.GetString("718acc19-59e3-42ee-bae3-0bc8c1298efc", "{0} was not populated because Entry Type is empty or Entry Number is empty with non-exempt Entry Type.", typeName);
				logger.Log(LogType.Warning, reason);
			}
		}

		bool HasEmptyEntryValue(EntryNumber dataObject) => !dataObject.Type.Code.HasValue || !dataObject.Number.HasValue || dataObject.Type.Code.Value == ZString.Empty
			|| (dataObject.Number.Value == ZString.Empty);

		#endregion
	}
}
