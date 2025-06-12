namespace WatchListCrawlerV3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Compare
    {
        private string old_filename;
        private string new_filename;
        private List<string> additions;
        private List<string> deletions;
        private Dictionary<string, string> changes;

        public Compare(string filename)
        {
            this.new_filename = filename;
            this.old_filename = filename + "_OLD";
            this.additions = new List<string>();
            this.deletions = new List<string>();
            this.changes = new Dictionary<string, string>();
        }



        public Dictionary<string, string> getChanges()
        {
            return changes;
        }
    }
}
