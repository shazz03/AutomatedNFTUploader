using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedNFTUpload
{
    public class Attribute
    {
        public string trait_type { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }

    public class AttributeReplace
    {

        public string trait_type { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
    public class Metadata
    {
        public string dna { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public int edition { get; set; }
        public object date { get; set; }
        public List<Attribute> attributes { get; set; }
    }
}
