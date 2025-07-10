//using CargoWise.EntityFramework;
//using Enterprise.Core;
//using Enterprise.DocumentEngine;
//using Enterprise.DocumentEngineCore.DocumentSupport;
//using Enterprise.Environment;
//using Enterprise.MasterFiles.Business;
//using Enterprise.ZArchitecture.Schema;

//namespace Enterprise.TransportBookings.Business
//{
//    public class DtbBookingDocumentPrinter : NonPersistentBusinessObject, IObsoleteValidation
//    {
//        public DtbBookingDocumentPrinter(IDocumentSupportable parent, BusinessObjectFactory factory)
//            : base(factory)
//        {
//            Parent = parent;
//        }

//        readonly IDocumentSupportable Parent;

//        public void PrintDocument()
//        {
//            var printTask = new PrintTask();
//            PrepareDocumentPack(printTask);
//            RunTask(printTask);
//        }

//        #region PrepareDocumentPack

//        void PrepareDocumentPack(PrintTask printTask)
//        {
//            DocumentCommand documentCommand = GetDocumentCommand();
//            var documentPack = (documentCommand == null) ? new DocumentPack() : new DocumentPack(documentCommand, Parent, null, null);
//            if (documentPack.Count > 0)
//            {
//                printTask.Add(documentPack);
//            }
//        }

//        DocumentCommand GetDocumentCommand()
//        {
//            var documentQuery = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
//            documentQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, Constants.BusinessContext.DtbBooking);
//            documentQuery.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
//            documentQuery.AddToFilter(StmMenuItemSchema.SU_IsPublished, true);

//            return Factory.LoadTop1<DocumentCommand>(documentQuery);
//        }

//        #endregion

//        #region RunTask

//        void RunTask(PrintTask printTask)
//        {
//            if (HasDocuments(printTask))
//            {
//                printTask.RunWithDeliveryForm(AllowedDeliveryOptions.All, Env);
//            }
//        }

//        bool HasDocuments(PrintTask printTask)
//        {
//            foreach (DocumentPack documentPack in printTask.GetDocumentPacks())
//            {
//                if (documentPack.Count > 0)
//                {
//                    return true;
//                }
//            }
//            return false;
//        }

//        #endregion
//    }
//}
