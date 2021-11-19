using System.Collections.Generic;

namespace ParserWebApi
{
    public class Lot
    {
        public List<Block> Blocks { get; set; } = new List<Block>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
    }
    
    public class Customer
    {
        public List<Field> Fields { get; set; } = new List<Field>();
        public List<GPZ> GPZs { get; set; } = new List<GPZ>();
    }

    public class GPZ
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Count { get; set; }
        public string Country { get; set; }
    }
}