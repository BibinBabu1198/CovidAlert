using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GACSWebApi.Common
{
    public class TemplateMaker
    {
        public const string Centername = "{centername}";
        public const string Address = "{address}";
        public const string Pincode = "{pincode}";
        public const string Date = "{date}";
        public const string AgeLimit = "{min_age_limit}";
        public const string vaccine = "{vaccine}";
        public const string AvailableCapacity = "{available_capacity}";
        public const string AvailableCapacityDose1 = "{available_capacity_dose1}";
        public const string AvailableCapacityDose2 = "{available_capacity_dose2}";
        public const string VaccineType = "{vaccineType}";
        public const string Fee = "{fee}";


        public static string GenrateTemplate(string Template, Dictionary<string, string> KeyValues)
        {
            foreach (KeyValuePair<string, string> entry in KeyValues)
            {
                Template = Template.Replace(entry.Key, entry.Value);
            }

            return Template;
        }

    }
}
