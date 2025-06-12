using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Blogical.Shared.Functoids
{
    /// <summary>
    /// Object that holds DataTable and timeout value
    /// </summary>
    public  class DataObject
    {
        private  DateTime _loadTime = DateTime.Now;

        /// <summary>
        /// Holds tomeout value
        /// </summary>
        public  DateTime LoadTime
        {
            get
            {
                return _loadTime;
            }
            set
            {
                _loadTime = value;
            }
        }

        /// <summary>
        /// Holds query result
        /// </summary>
        public  DataTable Data
        {
            get;
            set;
        }
    }
}
