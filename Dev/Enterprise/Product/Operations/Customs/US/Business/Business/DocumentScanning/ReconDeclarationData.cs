// Commented as Joo may want to add this back later.

//using System;
//using Enterprise.Customs.US.Business;
//using Enterprise.ZArchitecture.Modules.DocumentScanning;

//[assembly: AssemblyDataProvider(
//  typeof(ReconDeclarationData),
//  Enterprise.Core.Constants.DocManagerCodes.ReconDeclaration,
//  "Recon Declaration",
//  Country = "US")]

//namespace Enterprise.Customs.US.Business
//{
//  class ReconDeclarationData : AssemblyData
//  {
//    public override Type BusinessObjectType { get { return typeof(ReconDeclaration); } }		
//    public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
//    public override bool IsAllowedForUnallocatedeDocs { get { return false; } }
//  }
//}