//using System;
//using System.Collections.Generic;
//using System.Text;

//using Enterprise.ZArchitecture;
//using CargoWise.Types;
//using Enterprise.ZArchitecture.Business;

//namespace Enterprise.Customs.US.Business
//{
//  public class BillCollectionForEntry : Enterprise.Customs.Business.BillCollectionForEntry
//  {
//    public BillCollectionForEntry(CusEntryHeader EntryHeader)
//      : base(EntryHeader)
//    {
//    }

//    public new Bill this[int index]
//    {
//      get { return (Bill)base[index]; }
//    }

//    public new Bill AddNew()
//    {
//      return (Bill)base.AddNew();
//    }
//  }
//}

//#region Test
//#if DEBUG
//namespace Enterprise.Customs.US.Business.Testing
//{
//  using Enterprise.ZArchitecture.Business.Testing;

//  public class BillCollectionForEntryTest : Customs.Business.Testing.BillCollectionForEntryTest
//  {
//    protected override BusinessObjectCollection GetCollectionToTest()
//    {
//      declaration = SetUpDeclaration();
//      entryHeader = declaration.CustomsEntryHeaders[0];
//      return new BillCollectionForEntry(entryHeader);
//    }

//    #region Implementation

//    protected JobDeclaration SetUpDeclaration()
//    {
//      declaration = Factory.New<JobDeclaration>();
//      entryHeader = declaration.CustomsEntryHeaders.AddNew();
//      return declaration;
//    }

//    protected override void SetUp()
//    {
//      base.SetUp();
//      declaration = SetUpDeclaration();
//      entryHeader = declaration.CustomsEntryHeaders[0];
//    }
//    JobDeclaration declaration;
//    CusEntryHeader entryHeader;

//    #endregion
//  }
//}
//#endif
//#endregion