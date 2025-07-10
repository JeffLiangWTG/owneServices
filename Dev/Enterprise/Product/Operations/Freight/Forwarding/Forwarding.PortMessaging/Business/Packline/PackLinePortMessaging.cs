using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business.Packline;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	[ProvideMetaDataProperty("PropertiesReadOnlyState", MetaDataTypes.ReadOnly)]
	public class PackLinePortMessaging : AutoJobPackLinePortMessaging, IPortMessaging
	{
		public PackLinePortMessaging(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoJobPackLinePortMessaging.Schema
		{
			public const string DGTechnicalName = "DGTechnicalName";
		}

		#endregion

		#region Loader

		public static PackLinePortMessaging Load(ForwardingPackLine packline)
		{
			return packline.Factory.LoadTop1<PackLinePortMessaging>(new ZQuery(JobPackLinePortMessagingSchema.JLM_JL_PackLine, packline.PK));
		}

		public static PackLinePortMessaging LoadOrCreate(ForwardingPackLine packline)
		{
			var result = Load(packline);
			if (result == null)
			{
				result = packline.Factory.New<PackLinePortMessaging>();
				using (result.SuspendSettingHasChanges())
				{
					result.JLM_JL_PackLine = packline.PK;
				}
			}

			return result;
		}

		#endregion

		#region Business Objects Overrides

		public override bool IsSavedByFactory
		{
			get
			{
				if (!IsInDatabase && !IsDeleted && !this.HasData())
				{
					return false;
				}

				return base.IsSavedByFactory;
			}
		}

		#endregion

		#region Related Business Objects

		[RelatedBusinessObject("PackLine")]
		public override ZGuid JLM_JL_PackLine
		{
			get { return base.JLM_JL_PackLine; }
			set { base.JLM_JL_PackLine = value; }
		}

		public ForwardingPackLine PackLine
		{
			get { return Factory.Load<ForwardingPackLine>(JLM_JL_PackLine); }
		}

		#endregion

		#region Properties

		[List("Lookups.EntryTypeList")]
		public override ZString JLM_EntryType
		{
			get { return base.JLM_EntryType; }
			set
			{
				if (base.JLM_EntryType != value)
				{
					base.JLM_EntryType = value;
					ResetReadOnlyPropertiesDependingOnEntryType();
				}
			}
		}

		[List("Lookups.ExemptionReasonList")]
		public override ZString JLM_ExemptionReason
		{
			get { return base.JLM_ExemptionReason; }
			set
			{
				base.JLM_ExemptionReason = value;
			}
		}

		[List("Lookups.Annex30ATypeList")]
		public override ZString JLM_Annex30AType
		{
			get { return base.JLM_Annex30AType; }
			set
			{
				base.JLM_Annex30AType = value;
				ResetAnnex30AFailureProcess();
			}
		}

		[MaxLength(18)]
		public override ZString JLM_ExportDeclarationReference
		{
			get { return base.JLM_ExportDeclarationReference; }
			set { base.JLM_ExportDeclarationReference = value; }
		}

		public ZString CommodityDescription
		{
			get
			{
				var packLine = PackLine;
				var shipment = packLine?.Shipment;

				var list = new[]
				{
					packLine?.JL_HarmonisedCode,
					packLine?.JL_DetailedDescription,
					packLine?.JL_Description,
					shipment?.DetailedGoodsDescriptionNoteText,
					shipment?.JS_GoodsDescription
				};

				return list.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c)) ?? ZString.Empty;
			}
		}

		public ZString DGTechnicalName
		{
			get
			{
				var dangerousGoodsWithProvision = PackLine?.UNDGs.Where(undg => SubstanceHasIMOStandardAndSpecialProvision274(undg.Substance));
				if (dangerousGoodsWithProvision != null && dangerousGoodsWithProvision.Any())
				{
					var emptyTechicalNameDGs = dangerousGoodsWithProvision.Where(undg => undg.DI_TechnicalName == ZString.Empty);
					return emptyTechicalNameDGs.Any() ? ZString.Empty : dangerousGoodsWithProvision.First().DI_TechnicalName;
				}
				return ZString.Empty;
			}
		}

		bool SubstanceHasIMOStandardAndSpecialProvision274(UNDGSubstance substance)
		{
			return (substance != null
				&& substance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO
				&& substance.SpecialProvisions.Any(data => data.DC_Index == "274"));
		}

		public ZPropertyInfo DGTechnicalNameInfo
		{
			get { return GetZPropertyInfo(Schema.DGTechnicalName); }
		}

		#endregion

		#region Validation

		protected override JobPackLinePortMessagingValidation GetNewValidation()
		{
			var result = base.GetNewValidation();

			if (ShipmentPortMessagingManager.IsValidShipmentForDakosyPortMessaging(PackLine?.Shipment))
			{
				result.Add(new PackLinePortMessagingForDakosyValidation(this));
			}

			return result;
		}

		#endregion

		#region ReadOnly State

		protected bool GetPropertiesReadOnlyState(PropertyDescriptor property)
		{
			return PortMessagingHelper.GetPropertiesReadOnlyState(this, property);
		}

		#endregion

		#region Implementation

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new PackLinePortMessagingFetchStrategy(this);
		}

		void ResetReadOnlyPropertiesDependingOnEntryType()
		{
			JLM_ExemptionReason = ZString.Empty;

			if (JLM_ATBNumberInfo.ReadOnly)
			{
				JLM_ATBNumber = ZString.Empty;
			}

			if (JLM_Annex30ATypeInfo.ReadOnly)
			{
				JLM_Annex30AType = ZString.Empty;
			}

			if (JLM_ExportDeclarationReferenceInfo.ReadOnly)
			{
				JLM_ExportDeclarationReference = ZString.Empty;
			}

			if (JLM_MovementReferenceNumberInfo.ReadOnly)
			{
				JLM_MovementReferenceNumber = ZString.Empty;
			}

			if (JLM_MovementReferenceNumberCompleteInfo.ReadOnly)
			{
				JLM_MovementReferenceNumberComplete = ZBool.False;
			}

			if (JLM_LocalReferenceNumberInfo.ReadOnly)
			{
				JLM_LocalReferenceNumber = ZString.Empty;
			}

			if (JLM_LocalReferenceNumberCompleteInfo.ReadOnly)
			{
				JLM_LocalReferenceNumberComplete = ZBool.False;
			}

			ResetAnnex30AFailureProcess();

			if (!IsValidationSuspended)
			{
				Validation.ValidateJLM_ATBNumber();
				Validation.ValidateJLM_ExemptionReason();
				Validation.ValidateJLM_Annex30AType();
				Validation.ValidateJLM_MovementReferenceNumber();
				Validation.ValidateJLM_MovementReferenceNumberComplete();
				Validation.ValidateJLM_ExportDeclarationReference();
				Validation.ValidateJLM_CustomsReleaseDate();
			}
		}

		void ResetAnnex30AFailureProcess()
		{
			if (JLM_Annex30AFailureProcessInfo.ReadOnly)
			{
				JLM_Annex30AFailureProcess = false;
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateJLM_Annex30AFailureProcess();
			}
		}

		#endregion

		#region IPortMessaging Members

		ZString IPortMessaging.EntryType
		{
			get { return JLM_EntryType; }
		}

		ZString IPortMessaging.MovementReferenceNumber
		{
			get { return JLM_MovementReferenceNumber; }
		}

		ZBool IPortMessaging.MovementReferenceNumberComplete
		{
			get { return JLM_MovementReferenceNumberComplete; }
		}

		ZString IPortMessaging.ATBNumber
		{
			get { return JLM_ATBNumber; }
		}

		ZString IPortMessaging.ExemptionReason
		{
			get { return JLM_ExemptionReason; }
		}

		ZString IPortMessaging.Annex30AType
		{
			get { return JLM_Annex30AType; }
		}

		ZBool IPortMessaging.Annex30AFailureProcess
		{
			get { return JLM_Annex30AFailureProcess; }
		}

		ZString IPortMessaging.ExportDeclarationReference
		{
			get { return JLM_ExportDeclarationReference; }
		}

		ZDateTime IPortMessaging.CustomsReleaseDate
		{
			get { return JLM_CustomsReleaseDate; }
		}

		ZString IPortMessaging.MarksAndNumbers
		{
			get
			{
				var packLine = PackLine;
				if (packLine != null)
				{
					return packLine.JL_MarksAndNumbers.IsEmpty
						? packLine.Shipment != null ? packLine.Shipment.JS_MarksAndNumbers : ZString.Empty
						: packLine.JL_MarksAndNumbers;
				}

				return ZString.Empty;
			}
		}

		#endregion
	}
}
