using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using AngleSharp;
using AngleSharp.Dom;

namespace HomeFinder.Controllers.Parsers
{
    public struct RentalOffer
    {
        public string Href { get; set; }
        public string Address { get; set; }
        public string[] Description { get; set; }
    }



    public interface IParser
    {
        public string Source { get; }
        public Task<List<RentalOffer>> ParseByUrl(string url);
    }
}
