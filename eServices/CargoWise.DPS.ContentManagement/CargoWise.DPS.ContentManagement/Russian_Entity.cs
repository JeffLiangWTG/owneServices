using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;


namespace CargoWise.DPS.ContentManagement
{

    [IgnoreEmptyLines()]

    [DelimitedRecord("|")]
    public class Russian_Entity
    {

        public string refNo;
        public string enName;
        public string byName;
        public string ruName;
        public string dob;
        public string pob;
        public string address;
        public string passport;
        public string position;

    }
}
