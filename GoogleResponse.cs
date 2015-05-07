using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StanVre
{
    public class GoogleResponse
    {
        public int result_index { get; set; }
        public List<Result> result { get; set; }
    }
    public class Result
    {
        public List<Alternative> alternative { get; set; }
    }
    public class Alternative
    {
        public double confidence { get; set; }
        public string transcript { get; set; }
    }
}
