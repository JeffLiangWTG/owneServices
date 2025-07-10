//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChequeBookLookups
//
//    This class should be used for overriding collections in AutoAccChequeBookLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChequeBookLookups : AutoAccChequeBookLookups
	{
		public AccChequeBookLookups(AutoAccChequeBook parent) : base(parent)
		{
		}

		#region Printers Collection

		public BusinessObjectCollection Printers
		{
			get
			{
				ZQuery filter = new ZQuery(StmPrintQueueSchema.SQ_AllowPrinting, ZBool.True);
				BusinessObjectCollection printQueueCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueueCollection>(), new object[] { Factory, filter });
				//printQueueCollection.Load(new ZQuery(StmPrintQueueSchema.SQ_AllowPrinting, ZBool.True));
				printQueueCollection.Sort(new SortInfo(StmPrintQueueSchema.SQ_DisplayName.Name, ListSortDirection.Ascending));
				return printQueueCollection;
			}
		}

		#endregion
	}
}
