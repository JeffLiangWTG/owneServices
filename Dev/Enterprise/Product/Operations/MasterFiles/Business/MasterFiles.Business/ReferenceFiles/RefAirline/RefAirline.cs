using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefAirlineSchema.Constants.RM_TwoCharacterCode), DescriptionProperty(RefAirlineSchema.Constants.RM_AirlineName1)]
	public class RefAirline : AutoRefAirline, IRefAirline, IDocManagerSupport
	{
		public RefAirline(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static ZQuery ActiveFilter => new ZQuery(RefAirlineSchema.RM_IsActive, SQLComparisonOperator.Equal, true);

		#region Load

		public static RefAirline LoadFromAirlinePrefix(BusinessObjectFactory factory, ZString airlinePrefix)
		{
			var filter = new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, airlinePrefix);
			return factory.LoadTop1<RefAirline>(filter);
		}

		public static RefAirline LoadFromAirline2LetterCode(BusinessObjectFactory factory, ZString airline2LetterCode)
		{
			var filter = new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, airline2LetterCode);
			filter.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, "");
			var refAirlines = factory.Load<RefAirline>(filter);
			return refAirlines?.Length == 1 ? refAirlines[0] : null;
		}

		#endregion

		public static bool IsValidAirline2LetterCode(BusinessObjectFactory factory, ZString airline2LetterCode)
		{
			var filter = new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, airline2LetterCode);
			var refAirlines = factory.Load<RefAirline>(filter);
			return refAirlines.Length > 0;
		}

		public OrgHeader GetCorrespondingCarrierOrganisation()
		{
			OrgHeader result = null;
			if (!RM_EagleAddedAirlinePrefixOrAccountingCode.IsEmpty)
			{
				OrgMiscServ[] airlinesOrgMiscServ = Factory.Load<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_RM_Airline, PK));

				if (airlinesOrgMiscServ.Length > 0)
				{
					foreach (OrgMiscServ miscServ in airlinesOrgMiscServ)
					{
						if (GlbBranch.CurrentBranch.Country != null &&
							miscServ.OM_RN_NKEXDefaultCntryOfOrigin == GlbBranch.CurrentBranch.Country.RN_Code)
						{
							result = miscServ.Header;
							break;
						}
					}

					if (result == null)
					{
						result = airlinesOrgMiscServ[0].Header;
					}
				}
			}

			return result;
		}

		public bool HasSignedEAWBAgreement
		{
			get
			{
				return GenPivot != null;
			}
			set
			{
				if (HasSignedEAWBAgreement == value)
				{
					return;
				}

				if (value)
				{
					var pivot = Factory.New<GenPivot>();
					pivot.XX_Relation1ID = PK;
					pivot.XX_Relation2ID = Env.CurrentCompany.PK;
					pivot.XX_RelationType = Constants.GenPivotTypes.AirlineSignedAWBAgreementWithCompany;
					this.RegisterEditableChildObject(pivot);
				}
				else
				{
					GenPivot.Delete();
				}
				HasChanges = true;
			}
		}

		[ChildEditable()]
		public RefAirlineEFreightRuleCollection EFreightStatusCollection
		{
			get
			{
				if (efreightStatusCollection == null)
				{
					efreightStatusCollection = new RefAirlineEFreightRuleCollection(this);
					efreightStatusCollection.Load();
					RegisterEditableChildObject(efreightStatusCollection);
				}

				return efreightStatusCollection;
			}
		}
		RefAirlineEFreightRuleCollection efreightStatusCollection;

		[ChildEditable()]
		public RefAirlineDefaultCommodityCodeCollection DefaultCommodityCodeCollection
		{
			get
			{
				if (defaultCommodityCodeCollection == null)
				{
					defaultCommodityCodeCollection = new RefAirlineDefaultCommodityCodeCollection(this);
					defaultCommodityCodeCollection.Load();
					RegisterEditableChildObject(defaultCommodityCodeCollection);
				}

				return defaultCommodityCodeCollection;
			}
		}
		RefAirlineDefaultCommodityCodeCollection defaultCommodityCodeCollection;

		[ChildEditable()]
		public RefAirlineSpecialHandlingCodeCollection RefAirlineSpecialHandlingCodeCollection
		{
			get
			{
				if (refAirlineSpecialHandlingCodeCollection == null)
				{
					refAirlineSpecialHandlingCodeCollection = new RefAirlineSpecialHandlingCodeCollection(this);
					refAirlineSpecialHandlingCodeCollection.Load();
					RegisterEditableChildObject(refAirlineSpecialHandlingCodeCollection);
				}

				return refAirlineSpecialHandlingCodeCollection;
			}
		}
		RefAirlineSpecialHandlingCodeCollection refAirlineSpecialHandlingCodeCollection;

		GenPivot GenPivot
		{
			get
			{
				if (genPivot != null && genPivot.IsDeleted)
				{
					genPivot = null;
				}

				if (genPivot == null)
				{
					var query = new ZQuery(GenPivotSchema.XX_Relation1ID, PK);
					query.AddToFilter(GenPivotSchema.XX_Relation2ID, Env.CurrentCompany.PK);
					query.AddToFilter(GenPivotSchema.XX_RelationType, Constants.GenPivotTypes.AirlineSignedAWBAgreementWithCompany);
					genPivot = Factory.LoadTop1<GenPivot>(query);
					if (genPivot != null)
					{
						this.RegisterEditableChildObject(genPivot);
					}
				}
				return genPivot;
			}
		}
		GenPivot genPivot;

		public override void Delete()
		{
			if (EFreightStatusCollection != null)
			{
				EFreightStatusCollection.RemoveAndDeleteAll();
			}

			if (DefaultCommodityCodeCollection != null)
			{
				DefaultCommodityCodeCollection.RemoveAndDeleteAll();
			}

			if (RefAirlineSpecialHandlingCodeCollection != null)
			{
				RefAirlineSpecialHandlingCodeCollection.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("2450FB48-50FD-4D44-9D83-CB4DEAFB74BB", "Airline - {0}", CalculateShortcutName());

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Airline);
				}
				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var noteTypes = new NoteTypeCollection();
				noteTypes.Add(PredefinedNoteTypes.Instance.TermsAndConditions);
				return noteTypes;
			}
		}

		#endregion
	}
}
