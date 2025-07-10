CREATE USER refdbrepowriter WITHOUT LOGIN WITH DEFAULT_SCHEMA=[dbo];
GO
CREATE USER refdbreporeader WITHOUT LOGIN WITH DEFAULT_SCHEMA=[dbo];
GO
GRANT EXECUTE ON [dbo].[RefAccTaxRateUserView_Ins] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefAccTaxRateUserView_Del] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefAccTaxRateUserView_Ups] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefCusCodeListAttributeUserView_Ins] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefCusCodeListAttributeUserView_Del] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefCusCodeListAttributeUserView_Ups] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefCusCodeListUserView_Ins] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefCusCodeListUserView_Ups] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefShippingLineUserView_Ins] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefShippingLineUserView_Del] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefShippingLineUserView_Ups] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefStlScriptUserView_Ins] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefStlScriptUserView_Del] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefStlScriptUserView_Ups] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefCusProcedureUserView_Ins] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefCusProcedureUserView_Ups] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefCusProcedureAttributeUserView_Ins] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefCusProcedureAttributeUserView_Del] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefCusProcedureAttributeUserView_Ups] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefVesselUserView_Ins] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefVesselUserView_Del] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefVesselUserView_Ups] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefUNLOCOUserView_Ins] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefUNLOCOUserView_Del] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefUNLOCOUserView_Ups] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefPortPolygonUserView_Ins] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefPortPolygonUserView_Del] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RefPortPolygonUserView_Ups] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[SetUserId] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[GetUserId] TO refdbrepowriter;
GO
GRANT EXECUTE ON [dbo].[RecordDataSetChangeHistory] TO refdbrepowriter;
GO
GRANT EXECUTE ON TYPE::SourcePKList to refdbrepowriter;
GO
