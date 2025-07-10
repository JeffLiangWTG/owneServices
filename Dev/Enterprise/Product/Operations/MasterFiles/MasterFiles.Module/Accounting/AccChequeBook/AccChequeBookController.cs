using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Module Controller for AccChequeBook.
	/// </summary>
	public class AccChequeBookController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccChequeBookController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccChequeBook;
			}
		}

		public override ControllerID ID
		{
			//TODO: Steps to Complete:
			// 1. Open C:\Dev\Enterprise\Architecture\Modules\ControllerRegistration.cs in ZModules.
			// 2. Add to ControllerIDs class:
			//		public static ControllerID AccChequeBook = new ControllerID("AccChequeBook");
			// 3. Add to ControllerList constructor:
			//		Add(new ControllerInfo(ControllerIDs.AccChequeBook, "Enterprise.MasterFiles.Module", "Enterprise.MasterFiles.Module.AccChequeBookController"));
			get { return ControllerIDs.AccChequeBook; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			//TODO: Return your BusinessObject type here.
			//		If you are using a collection as your top level BusinessEntity,
			//		override GetLoadedBusinessEntityInLocalFactory() and GetNewBusinessEntityInLocalFactory()
			get { return typeof(AccChequeBook); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			//TODO: Return your ZForm here.
			return new AccChequeBookForm((AccChequeBook)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get
			{
				return Env.Security.ChequeBooksModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get
			{
				return Env.Security.ChequeBooksModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get
			{
				return Env.Security.ChequeBooksModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get
			{
				return Env.Security.ChequeBooks;
			}
		}
	}
}
