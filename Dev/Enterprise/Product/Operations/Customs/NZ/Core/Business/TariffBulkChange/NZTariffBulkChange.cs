using System.IO;
using System.Runtime.InteropServices;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business
{
	public class NZTariffBulkChange : TariffBulkChange, IObsoleteValidation
	{
		public NZTariffBulkChange(BusinessObjectFactory factory)
			: base(factory, BaseCusClassification.ClassificationType.Both)
		{
		}

		#region Overrides

		protected override bool IsPivotTariffNumSupported
		{
			get { return false; }
		}

		protected override ZString[] ValidPivotTypes
		{
			get { return new ZString[] { ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.HTB }; }
		}

		public override ZString ReferenceKey
		{
			get { return "HS2017 TARIFF " + lookupType; }
		}

		public override ZGuid CountryPK
		{
			get { return Core.Constants.CountryGuids.NewZealand; }
		}

		public override ZString CountryCode
		{
			get { return Core.Constants.CountryCodes.NewZealand; }
		}

		public void ChangeTCO(Stream str, [Optional] bool isSaveAllowed)
		{
			IsSaveAllowed = isSaveAllowed;
			if (str != null)
			{
				using (str)
				{
					StreamReader reader = new StreamReader(str);
					while (reader.Peek() >= 0)
					{
						string[] csvColumns = reader.ReadLine().Split(',');
						if (csvColumns.Length == 3)
						{
							ZString originalConcessionCode = csvColumns[0];
							ZString tariffNumPrefixFilter = csvColumns[1];
							ZString replacementConcessionCode = csvColumns[2];

							if (originalConcessionCode.Length == 7 && (replacementConcessionCode.IsEmpty || replacementConcessionCode.Length == 7))
							{
								ZQuery classificationFilter = new ZQuery();
								classificationFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, lookupType);
								if (!tariffNumPrefixFilter.IsEmpty)
								{
									classificationFilter.AddToFilter(CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.StartsWith, tariffNumPrefixFilter);
								}
								var addInfoQuery = AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.Equal, originalConcessionCode, CusClassificationSchema.CC_AddInfo, NZCusClassificationSchema.CC_ConcessionCode.Name.Substring(3));
								classificationFilter.AddToFilter(addInfoQuery);
								classificationFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
								var oldTCOClassifications = new BaseClassificationCollection<CusClassification>(Factory, classificationFilter);
								oldTCOClassifications.Load();
								foreach (CusClassification classification in oldTCOClassifications)
								{
									classification.CC_ConcessionCode = replacementConcessionCode;
								}
							}
						}
					}
				}
				str.Dispose();
			}
		}

		public ContinueWithSave TCOAdditionalContinueWithSave()
		{
			ContinueWithSave result = ContinueWithSave.No;
			string messageText = "";
			if (!IsProductionDataBase)
			{
				if (IsDateInvalid)
				{
					messageText = DateWarningMessage;
					result = ContinueWithSave.Yes;
				}
				else
				{
					result = ContinueWithSave.Yes;
				}
			}
			else if (IsDemoCompany)
			{
				Globals.Message.ShowError(DemoBranchMessage, "Access Denied");
			}
			else
			{
				if (!IsSaveAllowed)
				{
					Globals.Message.ShowError(SaveNotAllowedMessage, SaveNotAllowedResourceString);
				}
				else
				{
					result = ContinueWithSave.Yes;
				}
			}
			if (result == ContinueWithSave.Yes)
			{
				if (!string.IsNullOrEmpty(messageText))
				{
					messageText += " ";
				}
				messageText += "Your Data Base will now be updated with Concession changes, Do you wish to continue?";
				if (Globals.Message.Show(messageText, "Final Confirmation", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question) != ZDialogResult.Yes)
				{
					result = ContinueWithSave.No;
				}
			}
			return result;
		}

		#endregion

		protected override ITBCClassificationCollection<TariffBulkChange.TBCClassification> GetTBCClassificationCollection(BusinessObjectFactory factory)
		{
			return new TBCClassificationCollection(factory);
		}

		protected override ITBCClassificationCollection<TariffBulkChange.TBCClassification> GetTBCClassificationCollection(BusinessObjectFactory factory, ZQuery filter)
		{
			return new TBCClassificationCollection(factory, filter);
		}

		public class TBCClassificationCollection : TBCClassificationCollection<TBCClassification>
		{
			public TBCClassificationCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public TBCClassificationCollection(BusinessObjectFactory factory, ZQuery filter)
				: base(factory, filter)
			{
			}

			protected override bool AllowNewCore => false;
		}

		public new class TBCClassification : TariffBulkChange.TBCClassification
		{
			public TBCClassification(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			protected override TariffFormatter GetTariffFormatter()
			{
				return NZTariffFormatter;
			}

			protected TariffFormatter NZTariffFormatter
			{
				get { return fTariffFormatter ?? (fTariffFormatter = new NZTariffFormatter()); }
			}
			TariffFormatter fTariffFormatter;
		}
	}
}
