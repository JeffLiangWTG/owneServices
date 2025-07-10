using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business
{
	public class ConstituentElement : AutoConstituentElement, IConstituentElement, ICusAddInfoTypeSupporter
	{
		public ConstituentElement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(ConstituentElement constituentElement)
				: base(constituentElement)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				AddFetchHints();
			}

			void AddFetchHints()
			{
				var cusAddInfoQuery = new ZQuery(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(CusAddInfoSchema.Instance, cusAddInfoQuery);
			}
		}

		#endregion

		#region Overrides

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();
			ConstituentElement result = (ConstituentElement)base.CloneInternal(args);

			foreach (ScientificData element in ScientificDataCollection)
			{
				result.ScientificDataCollection.Add((ScientificData)element.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(ScientificData), false)));
			}

			return result;
		}

		public override void Delete()
		{
			ScientificDataCollection.DeleteAll();
			base.Delete();
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USConstituentElementAddInfoLookups.UnitsOfMeasureList))]
		[ReadOnlyMember(nameof(IsUnknownBreakdownTotal))]
		public override ZString US_PGAUnitOfMeasure
		{
			get { return base.US_PGAUnitOfMeasure; }
			set { base.US_PGAUnitOfMeasure = value; }
		}

		[ReadOnlyMember(nameof(IsUnknownBreakdownTotal))]
		public override ZDecimal US_PGAPercentOfConstituentElement
		{
			get { return base.US_PGAPercentOfConstituentElement; }
			set { base.US_PGAPercentOfConstituentElement = value; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USConstituentElementAddInfoLookups.SpeciesNameCodeList))]
		public override ZString US_SpeciesName
		{
			get { return base.US_SpeciesName; }
			set { base.US_SpeciesName = value; }
		}

		public override ZString US_GenusName
		{
			get { return US_SpecialUseDesignation ? Special : base.US_GenusName.ToString(); }
			set { base.US_GenusName = value; }
		}
		internal const string Special = "SPECIAL";

		public bool US_GenusName_ReadOnly
		{
			get { return US_SpecialUseDesignation; }
		}

		public override ZBool US_SpecialUseDesignation
		{
			get { return base.US_SpecialUseDesignation; }
			set
			{
				base.US_SpecialUseDesignation = value;
				this.RefreshBinding();
				if (!US_SpecialUseDesignation)
				{
					US_SpeciesName = ZString.Empty;
				}
			}
		}

		public ZString SpecialUseDesignationFieldType
		{
			get { return US_SpecialUseDesignation ? nameof(FieldType.TextDropEdit) : nameof(FieldType.Text); }
		}

		[ReadOnlyMember(nameof(IsUnknownBreakdownTotal))]
		[MaxLength(51)]
		public override ZString US_PGANameOfTheConstituentElement
		{
			get { return base.US_PGANameOfTheConstituentElement; }
			set { base.US_PGANameOfTheConstituentElement = value; }
		}

		[ReadOnlyMember(nameof(IsUnknownBreakdownTotal))]
		public override ZDecimal US_PGAQuantityOfConstituentElement
		{
			get { return base.US_PGAQuantityOfConstituentElement; }
			set { base.US_PGAQuantityOfConstituentElement = value; }
		}

		[ReadOnlyMember(nameof(IsUnknownBreakdownTotal))]
		public override ZString US_UnknownBreakdownCountryCode
		{
			get { return base.US_UnknownBreakdownCountryCode; }
			set { base.US_UnknownBreakdownCountryCode = value; }
		}

		internal PGA LaceyAct
		{
			get { return Parent as PGA; }
		}

		public ZBool IsUnknownBreakdownTotal
		{
			get { return LaceyAct != null && LaceyAct.US_UnknownBreakdownTotal; }
		}

		#endregion

		#region Related Objects

		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				JobComInvoiceLine result = null;
				var parentBO = Parent;
				if (parentBO != null)
				{
					var pga = parentBO as PGA;
					if (pga != null)
					{
						result = pga.InvoiceLine;
					}
					else
					{
						var fda = parentBO as ACEFDA;
						result = fda != null ? fda.InvoiceLine : null;
					}
				}
				return result;
			}
		}

		#endregion

		#region IConstituentElement Members

		ZString IConstituentElement.Name
		{
			get { return US_PGANameOfTheConstituentElement; }
		}

		ZDecimal IConstituentElement.Quantity
		{
			get { return US_PGAQuantityOfConstituentElement; }
		}

		ZString IConstituentElement.UnitOfMeasure
		{
			get { return US_PGAUnitOfMeasure; }
		}

		ZDecimal IConstituentElement.Percent
		{
			get { return US_PGAPercentOfConstituentElement; }
		}

		ZString IConstituentElement.CountryCode
		{
			get { return US_UnknownBreakdownCountryCode; }
		}

		PGAScientificDataCollection IConstituentElement.ScientificData
		{
			get { return ScientificDataCollection; }
		}

		ZString IConstituentElement.GenusName
		{
			get { return US_GenusName; }
		}

		ZString IConstituentElement.SpeciesName
		{
			get { return US_SpeciesName; }
		}

		#endregion

		[ChildEditable(true)]
		public PGAScientificDataCollection ScientificDataCollection
		{
			get
			{
				if (pg05_pg15Data == null)
				{
					pg05_pg15Data = new PGAScientificDataCollection(this);
					pg05_pg15Data.Load();
					RegisterEditableChildObject(pg05_pg15Data);
				}
				return pg05_pg15Data;
			}
		}
		PGAScientificDataCollection pg05_pg15Data;

		internal ConstituentElement CopyIndividualColumns()
		{
			var result = Factory.New<ConstituentElement>();
			result.US_PGANameOfTheConstituentElement = US_PGANameOfTheConstituentElement;
			result.US_PGAQuantityOfConstituentElement = US_PGAQuantityOfConstituentElement;
			result.US_PGAUnitOfMeasure = US_PGAUnitOfMeasure;
			result.US_PGAPercentOfConstituentElement = US_PGAPercentOfConstituentElement;
			result.B7_ParentID = B7_ParentID;
			result.B7_ParentTableCode = B7_ParentTableCode;
			result.B7_Type = B7_Type;
			return result;
		}

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USSCI, typeof(ScientificData));
			return result;
		}

		#endregion
	}
}
