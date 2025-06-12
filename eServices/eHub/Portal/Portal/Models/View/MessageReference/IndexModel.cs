using System.Collections.Generic;

namespace CargoWise.eHub.Portal.Models.View.MessageReference
{
    public class IndexModel
    {
        public List<string> CodeList { get; set; }

        public string Selected { get; set; }

        public string errorMessage { get; set; }
    }
}