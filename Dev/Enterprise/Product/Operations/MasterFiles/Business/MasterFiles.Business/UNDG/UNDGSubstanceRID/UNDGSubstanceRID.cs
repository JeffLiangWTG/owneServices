using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.RID_UNNO), DescriptionProperty(Schema.RID_PSN)]
	public class UNDGSubstanceRID : AutoUNDGSubstanceRID,
		IUNDGAttributeParent,
		IUNDGStandardSubstance,
		IDGSubstance
	{
		public UNDGSubstanceRID(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("9eba0755-8515-6eb5-4911-faeca3310055", "Dangerous Goods Substance - {0}", CalculateShortcutName());

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetDefaultDetailsLanguage();
		}

		#region Properties

		#region Labels

		public ZString RID_Label1
		{
			get
			{
				var handlingCodes = base.RID_Labels;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split('+');
					return split[0];
				}

				return ZString.Empty;
			}
		}

		public ZString RID_Label2
		{
			get
			{
				var handlingCodes = base.RID_Labels;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split('+');
					if (split.Length > 1)
					{
						return split[1];
					}
				}

				return ZString.Empty;
			}
		}

		public ZString RID_Label3
		{
			get
			{
				var handlingCodes = base.RID_Labels;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split('+');
					if (split.Length > 2)
					{
						return split[2];
					}
				}

				return ZString.Empty;
			}
		}

		public ZString RID_Label4
		{
			get
			{
				var handlingCodes = base.RID_Labels;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split('+');
					if (split.Length > 3)
					{
						return split[3];
					}
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region RID_IsSystemDefined

		public ZBool RID_IsSystemDefined => true;

		#endregion

		#region RID_Class

		[List("Lookups.RIDClassList")]
		public override ZString RID_Class
		{
			get { return base.RID_Class; }
			set { base.RID_Class = value; }
		}

		#endregion

		#region RID_PG

		[List("Lookups.PackingGroupList")]
		public override ZString RID_PG
		{
			get { return base.RID_PG; }
			set { base.RID_PG = value; }
		}

		#endregion

		#region DG_ExceptedQuantityCode

		[List("Lookups.ExceptedQuantityList")]
		public override ZString RID_ExceptedQuantityCode
		{
			get { return base.RID_ExceptedQuantityCode; }
			set { base.RID_ExceptedQuantityCode = value; }
		}

		#endregion

		#region Names

		[ChildEditable(true)]
		public UNDGAttributeZZCollection Names
		{
			get
			{
				if (names == null)
				{
					names = new UNDGAttributeZZCollection(Factory, this, ViewUNDGAttributeLookups.TypeConstants.ProperShippingName);
					RegisterEditableChildObject(names);
				}

				return names;
			}
		}

		UNDGAttributeZZCollection names;

		#endregion

		#region Classification Codes

		public ZString ClassificationCode1
		{
			get
			{
				var classificationCode = base.RID_ClassificationCode;
				if (!classificationCode.IsEmpty)
				{
					var split = classificationCode.Split(ClassificationCodeSplitters);
					return split.Any() ? split[0].Trim() : classificationCode;
				}

				return ZString.Empty;
			}
		}

		public ZString ClassificationCode2
		{
			get
			{
				var classificationCode = base.RID_ClassificationCode;
				if (!classificationCode.IsEmpty)
				{
					var split = classificationCode.Split(ClassificationCodeSplitters);
					if (split.Length > 1)
					{
						return split[1].Trim();
					}
				}

				return ZString.Empty;
			}
		}

		string[] ClassificationCodeSplitters => new[] { (NoResString)" or ", " OR " };

		#endregion

		#region Qualifying Descriptive Text

		[ChildEditable(true)]
		public UNDGAttributeZZCollection QualifyingDescriptiveTexts
		{
			get
			{
				if (qualifyingDescriptiveTexts == null)
				{
					qualifyingDescriptiveTexts = new UNDGAttributeZZCollection(Factory, this, ViewUNDGAttributeLookups.TypeConstants.QualifyingDescriptiveText);
					RegisterEditableChildObject(qualifyingDescriptiveTexts);
				}

				return qualifyingDescriptiveTexts;
			}
		}

		UNDGAttributeZZCollection qualifyingDescriptiveTexts;

		#endregion

		#region IReferenceSubstance

		[List("Lookups.Languages")]
		[MaxLength(7)]
		public ZString DetailsLanguage
		{
			get { return detailsLanguage; }
			set
			{
				if (detailsLanguage != value)
				{
					CheckMaximumLength(DetailsLanguageInfo, value); // you are likely missing this line!
					detailsLanguage = value;
					DetailsLanguageInfo.RefreshBinding();
				}
			}
		}

		ZString detailsLanguage;

		public ZPropertyInfo DetailsLanguageInfo => GetZPropertyInfo(nameof(DetailsLanguage));

		void SetDefaultDetailsLanguage()
		{
			DetailsLanguage = ZString.Empty;
		}

		public ZString TableCode => TablePrefix;

		#endregion

		#region IDGSubstance

		ZString IDGSubstance.UNNO => RID_UNNO;

		ZString IDGSubstance.Variant => RID_Variant;

		ZString IDGSubstance.Standard => TablePrefix;

		#endregion

		#region UNDGCountryReferences

		[List("Lookups.UNDGCountryReferences")]
		public UNDGCountryReferenceManyToManyCollection<UNDGSubstanceRID> UNDGCountryReferences
		{
			get
			{
				if (undgCountryReferences == null)
				{
					undgCountryReferences = UNDGSubstance.GetUNDGCountryReferenceManyToManyCollection(this);
				}

				return undgCountryReferences;
			}
		}

		UNDGCountryReferenceManyToManyCollection<UNDGSubstanceRID> undgCountryReferences;

		#endregion

		#endregion
	}
}
