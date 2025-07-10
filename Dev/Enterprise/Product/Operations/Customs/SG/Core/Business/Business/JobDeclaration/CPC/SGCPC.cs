using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG
{
	public class SGCPC : AutoSGCPC, ICusCPC
	{
		public SGCPC(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : AutoSGCPC.Schema
		{
			public const string SG_APCCodeDescription = "SG_APCCodeDescription";
		}

		public new JobDeclaration Parent
		{
			get
			{
				if (fParent == null || fParent.PK != B7_ParentID)
				{
					ZGuid reference = B7_ParentID.IsEmpty ? b7_ParentIDCachedOnRelationshipResetByCore : B7_ParentID;
					fParent = Factory.Load<JobDeclaration>(reference);
				}
				return fParent != null && !fParent.IsDeleted ? fParent : null;
			}
		}
		JobDeclaration fParent;

		public RefCusProcedure AdditionalProcedureCode
		{
			get
			{
				RefCusProcedure result = null;
				var procedureCode = SG_CPCCode.SubstringSafe(0, 3);
				var concession = SG_CPCCode.SubstringSafe(3, 4);
				var apcCode = new RefCusProcedure.Loader(Factory).LoadFromProcedureAndPreviousProcedureAndConcession(procedureCode, ZString.Empty, concession, ZString.Empty, Core.Constants.CountryCodes.Singapore, ZDateTime.Today);
				if (apcCode != null)
				{
					result = apcCode;
				}

				return result;
			}
		}

		[BusinessObjectTestExclude]
		public override ZString B7_ParentTableCode
		{
			get { return base.B7_ParentTableCode; }
			set
			{
				if (!value.IsEmpty && value != JobDeclarationSchema.Constants.Prefix)
				{
					throw new NotSupportedException("Setting SGCPC.B7_ParentTableCode is not supported.");
				}
				base.B7_ParentTableCode = value;
			}
		}

		[BusinessObjectTestExclude]
		public override ZGuid B7_ParentID
		{
			get { return base.B7_ParentID; }
			set
			{
				if (!IsCopying && value.IsEmpty)
				{
					b7_ParentIDCachedOnRelationshipResetByCore = base.B7_ParentID;
				}
				ZGuid oldValue = B7_ParentID;
				base.B7_ParentID = value;
				if (!IsCopying && oldValue != B7_ParentID)
				{
					if (!oldValue.IsEmpty && !B7_ParentID.IsEmpty)
					{
						throw new NotSupportedException("Setting SGCPC.B7_ParentID is not supported.");
					}
				}
			}
		}
		ZGuid b7_ParentIDCachedOnRelationshipResetByCore;

		public override ZString B7_AddInfoData
		{
			get { return base.B7_AddInfoData; }
			set
			{
				base.B7_AddInfoData = value;
				if (Parent != null)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(SGCPCAddInfoLookups.CPCCodeList))]
		public override ZString SG_CPCCode
		{
			get { return base.SG_CPCCode; }
			set
			{
				bool hasChanges = value != base.SG_CPCCode;
				base.SG_CPCCode = value;
				if (hasChanges)
				{
					SG_APCCodeDescription = DetermineCPCDesc(value.SubstringSafe(3, 4));
				}
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(AddInfoLookups) + "." + nameof(SGCPCAddInfoLookups.CPCDescList))]
		public ZString SG_APCCodeDescription
		{
			get
			{
				sg_APCCodeDescription = DetermineCPCDesc(SG_CPCCode.SubstringSafe(3, 4));
				return sg_APCCodeDescription;
			}
			set
			{
				bool hasChanges = value != sg_APCCodeDescription;
				sg_APCCodeDescription = value;
				SG_APCCodeDescriptionInfo.RefreshBinding();
				if (hasChanges || SG_CPCCode.IsEmpty)
				{
					DisplayCPCCode(value);
				}
			}
		}
		ZString sg_APCCodeDescription;

		public ZPropertyInfo SG_APCCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.SG_APCCodeDescription); }
		}

		public void DisplayCPCCode(string cpc)
		{
			SG_CPCCode = Parent.GetCPCCodeForDeclarationType(cpc);
		}

		ZString DetermineCPCDesc(string concessionCode)
		{
			return Parent?.GetCPCDescriptionForDeclarationType(concessionCode);
		}

		#region ICusCPC Members

		ZString ICusCPC.APCCodeName
		{
			get { return SG_CPCCode; }
		}

		IEnumerable<ICusProcessingCodes> ICusCPC.PCOccurrences
		{
			get
			{
				yield return CusProcessingCodes.New(SG_PC1, SG_PC2, SG_PC3);
				if (!SG_PC4.IsEmpty)
				{
					yield return CusProcessingCodes.New(SG_PC4, SG_PC5, SG_PC6);

					if (!SG_PC7.IsEmpty)
					{
						yield return CusProcessingCodes.New(SG_PC7, SG_PC8, SG_PC9);

						if (!SG_PC10.IsEmpty)
						{
							yield return CusProcessingCodes.New(SG_PC10, SG_PC11, SG_PC12);

							if (!SG_PC13.IsEmpty)
							{
								yield return CusProcessingCodes.New(SG_PC13, SG_PC14, SG_PC15);
							}
						}
					}
				}
			}
		}

		#endregion

		class CusProcessingCodes : ICusProcessingCodes
		{
			public static CusProcessingCodes New(ZString code1, ZString code2, ZString code3)
			{
				return !code1.IsEmpty || !code2.IsEmpty || !code3.IsEmpty ? new CusProcessingCodes() { processingCode1 = code1, processingCode2 = code2, processingCode3 = code3 } : null;
			}

			public ZString processingCode1 { get; set; }
			public ZString processingCode2 { get; set; }
			public ZString processingCode3 { get; set; }

			#region ICusProcessingCodes Members

			ZString ICusProcessingCodes.ProcessingCode1
			{
				get { return processingCode1; }
			}

			ZString ICusProcessingCodes.ProcessingCode2
			{
				get { return processingCode2; }
			}

			ZString ICusProcessingCodes.ProcessingCode3
			{
				get { return processingCode3; }
			}

			#endregion
		}
	}
}
