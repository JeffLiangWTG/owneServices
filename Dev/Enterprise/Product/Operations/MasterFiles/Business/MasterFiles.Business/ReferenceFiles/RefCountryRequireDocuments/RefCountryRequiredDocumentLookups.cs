using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryRequiredDocumentLookups : AutoRefCountryRequiredDocumentLookups
	{
		public RefCountryRequiredDocumentLookups(AutoRefCountryRequiredDocument parent)
			: base(parent)
		{
		}

		#region Documents

		public RefDocTypeCollection RT_ReferenceType_List
		{
			get
			{
				if (fRT_ReferenceType_List == null)
				{
					DocTypeCategoryQuery filter = new DocTypeCategoryQuery(Factory, Constants.ReferenceTypes.SupplyChainLogistics);
					filter.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_IsActive, true);
					filter.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_DocType, SQLComparisonOperator.NotEqual, Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument);
					filter.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_DocType, SQLComparisonOperator.NotEqual, Core.Constants.RefDocTypes.InternallyCreatedPublicDocument);
					fRT_ReferenceType_List = new RefDocTypeCollection(Factory, filter);
				}
				return fRT_ReferenceType_List;
			}
		}
		RefDocTypeCollection fRT_ReferenceType_List;

		#endregion

		#region RefCountry_List

		public RefCountryCollection RefCountry_List
		{
			get
			{
				if (refCountry_List == null)
				{
					refCountry_List = new RefCountryCollection(Factory);
				}
				return refCountry_List;
			}
		}
		RefCountryCollection refCountry_List;

		#endregion

		#region DocUsage_List

		public CodeDescriptionPairList DocUsage_List
		{
			get
			{
				if (fDocUsage_List == null)
				{
					fDocUsage_List = new CodeDescriptionPairList();
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.All, Res.GetString("628b0210-2c4a-4b41-91c1-05a08a93c8bb", "All"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Export, Res.GetString("2ba026cd-94de-4df6-8a30-9b6046392fd6", "Export"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Import, Res.GetString("5372912d-e109-4523-8a3f-11e56ddf41c8", "Import"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Both, Res.GetString("5cef6038-cf6d-4cf8-b428-eae0f0787736", "Both Export and Import"));
					fDocUsage_List.AddPair(JobRequiredDocument.DocUsage.Domestic, Res.GetString("39278844-956e-40b7-9058-5f5c25f9b1cd", "Domestic"));
				}
				return fDocUsage_List;
			}
		}
		CodeDescriptionPairList fDocUsage_List;

		#endregion

		#region TransportMode_List

		public CodeDescriptionPairList TransportMode_List
		{
			get
			{
				if (fTransportMode_List == null)
				{
					fTransportMode_List = new CodeDescriptionPairList();
					fTransportMode_List.AddPair(Constants.TransportModes.All, Res.GetString("628b0210-2c4a-4b41-91c1-05a08a93c8bb", "All"));
					fTransportMode_List.AddPair(Constants.TransportModes.Sea, Res.GetString("63f001c8-ebae-4ac7-b5a2-8cf879f8fa7b", "Sea Freight"));
					fTransportMode_List.AddPair(Constants.TransportModes.Air, Res.GetString("810c2444-039f-4cab-a62a-f6cd4b4d5291", "Air Freight"));
					fTransportMode_List.AddPair(Constants.ContainerModes.FCL, Res.GetString("8f49b633-37ca-455c-9d8a-26d87a23a24a", "Full Container Load"));
					fTransportMode_List.AddPair(Constants.ContainerModes.LCL, Res.GetString("312e5fd8-cffe-40ad-9663-7c342fe9bce6", "Less Container Load"));
					fTransportMode_List.AddPair(Constants.TransportModes.Rail, Res.GetString("1b43de2c-6dcf-4cd4-9d08-e0c4c4d8ffaa", "Rail Freight"));
					fTransportMode_List.AddPair(Constants.TransportModes.Road, Res.GetString("0ea4542b-077b-417b-b366-12e67474c31c", "Road Freight"));
				}
				return fTransportMode_List;
			}
		}
		CodeDescriptionPairList fTransportMode_List;

		#endregion
	}
}
