using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;


namespace CargoWise.DPS.ContentManagement
{
    [IgnoreFirst(2)]
    [IgnoreEmptyLines()]

    [DelimitedRecord(";")]  // | for XLS
    public class UK_INVBAN_Entity
    {

        [FieldQuoted('"', QuoteMode.OptionalForBoth, MultilineMode.AllowForRead)]
        public string name;
        public string name1;
        public string name2;
        public string name3;
        public string name4;
        public string name5;
        public string title;
        public string dob;
        public string townOfBirth;
        public string countryOfBirth;
        public string nationality;
        public string passport;
        public string ni_number;
        public string position;
        public string address1;
        public string address2;
        public string address3;
        public string address4;
        public string address5;
        public string address6;
        public string post_zip;
        public string country;
        public string other;
        public string groupType;
        public string aliasType;
        public string regime;
        public string listedOn;
        public string lastUpdated;
        public string groupID;


    }
}
