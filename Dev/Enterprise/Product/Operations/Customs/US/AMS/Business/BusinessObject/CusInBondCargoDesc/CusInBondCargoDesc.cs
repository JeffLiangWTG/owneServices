using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondCargoDesc : Customs.Business.CusInBondCargoDesc,
		ICargoDescription,
		ICanDelete,
		ISailingSynchronisationTarget<BillOfLadingPackLine>,
		ISailingSynchronisationTarget<AgencyShipmentContainer>
	{
		public CusInBondCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties

		public bool ShouldSynchroniseWithConsol
		{
			get
			{
				var container = Container;
				return container != null && container.ShouldSynchronise;
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.Tariffs))]
		public override ZString BY_FormattedHarmonisedTariff
		{
			get { return TariffFormatter.DisplayFormat(BY_HarmonisedTariff); }
			set
			{
				var oldValue = BY_FormattedHarmonisedTariff;
				var newValue = TariffFormatter.Format(value);
				if (newValue.Length == 2 || newValue.Length == 4)
				{
					BY_HarmonisedTariff = newValue.PadRight(6, '0');
				}
				else
				{
					BY_HarmonisedTariff = newValue;
				}

				BY_FormattedHarmonisedTariffInfo.RefreshBinding(oldValue);
			}
		}

		public CusInBondBill Bill
		{
			get
			{
				var container = Container;
				return container == null ? null : container.Bill;
			}
		}

		public ZWeight Weight
		{
			get { return new ZWeight(BY_GrossWeight, BY_GrossWeightUnit); }
		}

		public bool IsInventoryRecordValidationMode
		{
			get
			{
				var bill = Bill;
				return bill != null && bill.IsInventoryRecordValidationMode;
			}
		}

		#endregion

		#region Override Properties

		#region BY_ParentID

		[RelatedBusinessObject("Container")]
		public override ZGuid BY_ParentID
		{
			get { return base.BY_ParentID; }
			set { base.BY_ParentID = value; }
		}

		public CusInBondContainer Container
		{
			get { return Factory.Load<CusInBondContainer>(BY_ParentID); }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.ManifestUnitList))]
		public override ZString BY_ManifestUnitCode
		{
			get { return base.BY_ManifestUnitCode; }
			set { base.BY_ManifestUnitCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.WeightUnitList))]
		public override ZString BY_GrossWeightUnit
		{
			get { return base.BY_GrossWeightUnit; }
			set { base.BY_GrossWeightUnit = value; }
		}

		[MeasureUnit(Schema.BY_GrossWeightUnit, MeasureUnitType.Weight)]
		public override ZDecimal BY_GrossWeight
		{
			get { return base.BY_GrossWeight; }
			set { base.BY_GrossWeight = value; }
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.Tariffs))]
		[MaxLength(Schema.BY_HarmonisedTariffMaxLength)]
		public override ZString BY_HarmonisedTariff
		{
			get { return base.BY_HarmonisedTariff; }
			set
			{
				var oldFormattedValue = BY_FormattedHarmonisedTariff;
				base.BY_HarmonisedTariff = TariffFormatter.Format(value);
				BY_FormattedHarmonisedTariffInfo.RefreshBinding(oldFormattedValue);
			}
		}

		public override ZString BY_Description
		{
			get { return base.BY_Description; }
			set { base.BY_Description = Regex.Replace(value.ToUpper(), NoneValidCharactersMatchPattern, ""); }
		}
		const string NoneValidCharactersMatchPattern = @"[^!@\#\$%\^&\*\(\)-_=\+\[\{\]}\\\|;:'"",<\.>/\?`~¢\ ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789]";

		StmNote DetailedDescriptionNote
		{
			get
			{
				if (cachedNote == null)
				{
					cachedNote = new CachedProperty<StmNote>(Factory, delegate
					{
						return Notes.FindByDescription("Detailed Goods Description").FirstOrDefault();
					});
				}
				return cachedNote.Value;
			}
		}
		CachedProperty<StmNote> cachedNote;

		public ZString DetailedDescription
		{
			get => DetailedDescriptionNote?.ST_NoteDataAsText ?? ZString.Empty;
			set
			{
				var detailedDescriptionNote = DetailedDescriptionNote;
				var oldValue = detailedDescriptionNote?.ST_NoteDataAsText ?? ZString.Empty;
				if (value != oldValue)
				{
					if (value.IsEmpty)
					{
						detailedDescriptionNote?.Delete();
					}
					else
					{
						if (detailedDescriptionNote == null)
						{
							detailedDescriptionNote = Notes.AddNew();
							detailedDescriptionNote.ST_NoteType = nameof(StmNoteVisibility.INT);
							detailedDescriptionNote.ST_IsCustomDescription = true;
							detailedDescriptionNote.ST_Description = "Detailed Goods Description";
						}
						detailedDescriptionNote.ST_NoteDataAsText = value;
					}
				}
			}
		}

		public override ZPropertyInfo BY_HarmonisedTariffInfo
		{
			get { return GetZPropertyInfo(Schema.BY_HarmonisedTariff); }
		}

		public new CusInBondCargoDescLookups Lookups
		{
			get { return (CusInBondCargoDescLookups)base.Lookups; }
		}

		public new CusInBondCargoDescValidation Validation
		{
			get { return (CusInBondCargoDescValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		new TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter tariffFormatter;

		protected override Customs.Business.CusInBondCargoDescLookups GetNewLookups()
		{
			return new CusInBondCargoDescLookups(this);
		}

		protected override Customs.Business.CusInBondCargoDescValidation GetNewValidation()
		{
			return new CusInBondCargoDescValidation(this);
		}

		protected override Type FeeTypeCore => typeof(CusInBondFee);
		#endregion

		#region ICargoDescription Members

		ZString ICargoDescription.Description
		{
			get
			{
				var description = DetailedDescription;
				if (description.IsEmpty)
				{
					description = BY_Description;
				}
				return ACEOceanManifestIllegalCharacters.ReplaceIllegalCharacters(description, '?');
			}
		}

		ZInt ICargoDescription.Value
		{
			get { return BY_MonetaryValue.Round(0).ToZInt(); }
		}

		ZInt ICargoDescription.Weight
		{
			get
			{
				var result = BY_GrossWeight;
				if (!result.IsEmpty)
				{
					var unit = ((ICargoDescription)this).WeightUnit;
					if (unit == WeightUnitList.Codes.Kilograms)
					{
						result = Weight.InKilogramsSafe;
					}
				}
				return result.Round(0).ToZInt();
			}
		}

		ZString ICargoDescription.WeightUnit
		{
			get
			{
				var grossWeightUnit = BY_GrossWeightUnit;
				return Core.Constants.Weight.ContainsCode(grossWeightUnit) ? (grossWeightUnit == Core.Constants.Weight.Pounds ? WeightUnitList.Codes.Pounds : WeightUnitList.Codes.Kilograms) : grossWeightUnit.ToString();
			}
		}

		ZDecimal ICargoDescription.PieceCount
		{
			get { return new ZDecimal(BY_PieceCount); }
		}

		ZString ICargoDescription.HarmonizedNumber
		{
			get { return BY_HarmonisedTariff; }
		}

		ZString ICargoDescription.MarksAndNumbers
		{
			get { return ACEOceanManifestIllegalCharacters.ReplaceIllegalCharacters(BY_MarksAndNumbers, '?'); }
		}

		ZString ICargoDescription.C4Number
		{
			get { return BY_CusC4Number; }
		}

		ZString ICargoDescription.CountryCode
		{
			get { return BY_RN_NKCountryOfOrigin; }
		}

		ZString ICargoDescription.ManifestUnitCode
		{
			get { return BY_ManifestUnitCode; }
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !ShouldSynchroniseWithConsol; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("AMS|CusInBondCargoDesc|24366B81-1F5E-4D07-9235-920EBC09884E", "Commodity values are copied from the Consol. If you want to delete this record, please do it in the Consol, or you may tick 'Override Freight Defaults'."); }
		}

		#endregion

		#region ISailingSynchronisationTarget<BillOfLadingPackLine> Members

		bool ISailingSynchronisationTarget<BillOfLadingPackLine>.IsMatched(BillOfLadingPackLine sailingTarget)
		{
			return IsMatched(sailingTarget);
		}

		bool IsMatched(BillOfLadingPackLine sailingPackLine)
		{
			var sailingContainer = ((ISailingSynchronisationTarget<BillOfLadingContainer>)Container).Source;
			var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Bill).Source;
			return IsMatched(sailingPackLine, sailingContainer, sailingBill);
		}

		bool IsMatched(BillOfLadingPackLine sailingPackLine, BillOfLadingContainer sailingContainer, BillOfLading sailingBill)
		{
			return sailingContainer != null && sailingPackLine.JL_JC == sailingContainer.PK && sailingBill != null && sailingBill.IsContainerised && sailingPackLine.JL_JS == sailingBill.PK &&
				this.BY_HarmonisedTariff == sailingPackLine.JL_HarmonisedCode.Left(this.BY_HarmonisedTariffInfo.MaxLength);
		}

		void ISailingSynchronisationTarget<BillOfLadingPackLine>.Set(BillOfLadingPackLine sailingTarget)
		{
			this.BY_HarmonisedTariff = sailingTarget.JL_HarmonisedCode.Left(this.BY_HarmonisedTariffInfo.MaxLength);
		}

		void ISailingSynchronisationTarget<BillOfLadingPackLine>.Synchronise()
		{
			var sailingPackLine = ((ISailingSynchronisationTarget<BillOfLadingPackLine>)this).Source;
			if (sailingPackLine != null)
			{
				BY_GrossWeight = sailingPackLine.JL_ActualWeight;
				BY_GrossWeightUnit = sailingPackLine.JL_ActualWeightUQ;
				BY_ManifestUnitCode = sailingPackLine.JL_F3_NKPackType;
				BY_PieceCount = sailingPackLine.JL_PackageCount;
				BY_Description = sailingPackLine.JL_DetailedDescription.Left(CusInBondCargoDesc.Schema.BY_DescriptionMaxLength);
				BY_MarksAndNumbers = sailingPackLine.JL_MarksAndNumbers.Left(CusInBondCargoDesc.Schema.BY_MarksAndNumbersMaxLength);
			}
		}

		BillOfLadingPackLine ISailingSynchronisationTarget<BillOfLadingPackLine>.Source
		{
			get
			{
				if (billOfLadingPackLineSource != null && IsMatched(billOfLadingPackLineSource))
				{
					return billOfLadingPackLineSource;
				}
				billOfLadingPackLineSource = null;
				var sailingContainer = ((ISailingSynchronisationTarget<BillOfLadingContainer>)Container).Source;
				var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Bill).Source;
				if (sailingContainer != null && sailingBill != null)
				{
					billOfLadingPackLineSource = sailingContainer.PackLines.Cast<BillOfLadingPackLine>().FirstOrDefault(x => IsMatched(x, sailingContainer, sailingBill));
				}
				return billOfLadingPackLineSource;
			}
		}
		BillOfLadingPackLine billOfLadingPackLineSource;

		public bool HasSailingLinkage
		{
			get
			{
				var container = Container;
				return container != null && Container.HasSailingLinkage;
			}
		}

		#endregion

		#region ISailingSynchronisationTarget<AgencyShipmentContainer>

		AgencyShipmentContainer ISailingSynchronisationTarget<AgencyShipmentContainer>.Source
		{
			get
			{
				if (agencyShipmentContainerSource != null && IsMatched(agencyShipmentContainerSource))
				{
					return agencyShipmentContainerSource;
				}
				agencyShipmentContainerSource = null;
				var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Bill).Source;
				if (sailingBill != null && !sailingBill.IsContainerised)
				{
					if (sailingBill.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff)
					{
						agencyShipmentContainerSource = sailingBill.Vehicles.Cast<AgencyShipmentContainer>().FirstOrDefault(x => IsMatched(x, sailingBill));
					}
					else
					{
						agencyShipmentContainerSource = sailingBill.TopLevelPacks.Cast<AgencyShipmentContainer>().FirstOrDefault(x => IsMatched(x, sailingBill));
					}
				}
				return agencyShipmentContainerSource;
			}
		}
		AgencyShipmentContainer agencyShipmentContainerSource;

		bool ISailingSynchronisationTarget<AgencyShipmentContainer>.IsMatched(AgencyShipmentContainer sailingTarget)
		{
			return IsMatched(sailingTarget);
		}

		bool IsMatched(AgencyShipmentContainer sailingContainer)
		{
			var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Bill).Source;
			return IsMatched(sailingContainer, sailingBill);
		}

		bool IsMatched(AgencyShipmentContainer sailingContainer, BillOfLading sailingBill)
		{
			return sailingBill != null && !sailingBill.IsContainerised && (sailingBill.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff ?
				sailingBill.Vehicles.Contains(sailingContainer) : sailingBill.TopLevelPacks.Contains(sailingContainer)) &&
				this.BY_HarmonisedTariff == sailingContainer.JC_HarmonisedCode.Left(this.BY_HarmonisedTariffInfo.MaxLength);
		}

		void ISailingSynchronisationTarget<AgencyShipmentContainer>.Set(AgencyShipmentContainer sailingTarget)
		{
			using (this.GetValidationSuspender())
			{
				this.BY_HarmonisedTariff = sailingTarget.JC_HarmonisedCode.Left(this.BY_HarmonisedTariffInfo.MaxLength);
			}
		}

		void ISailingSynchronisationTarget<AgencyShipmentContainer>.Synchronise()
		{
			var sailingContainer = ((ISailingSynchronisationTarget<AgencyShipmentContainer>)this).Source;
			if (sailingContainer != null)
			{
				using (GetValidationSuspender())
				{
					BY_GrossWeight = sailingContainer.JC_GrossWeight;
					BY_GrossWeightUnit = sailingContainer.JC_GrossWeightUQ;
					BY_PieceCount = sailingContainer.JC_ContainerCount;
					BY_ManifestUnitCode = sailingContainer.JC_F3_NKPackType;
					BY_Description = sailingContainer.JC_Description.Left(CusInBondCargoDesc.Schema.BY_DescriptionMaxLength);
					BY_MarksAndNumbers = sailingContainer.JC_MarksAndNumbers.Left(CusInBondCargoDesc.Schema.BY_MarksAndNumbersMaxLength);
					var header = Bill?.Header;
					if (header != null)
					{
						BY_MonetaryValue = header.ConvertToUSD(sailingContainer.JC_GoodsValue, sailingContainer.JC_RX_NKGoodsCurrency);
					}
				}
			}
		}
		public decimal DeminimusValue => FeeCalculationHelper.GetDeminimus(Factory);

		#endregion
	}
}
