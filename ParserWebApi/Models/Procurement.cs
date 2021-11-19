using System.Collections.Generic;

namespace ParserWebApi
{
    public class Procurement
    {
        public string Number { get; set; }
        public List<Block> Blocks { get; set; } = new List<Block>();
        public List<Lot> Lots { get; set; } = new List<Lot>();
    }
}