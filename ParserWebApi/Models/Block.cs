using System.Collections.Generic;

namespace ParserWebApi
{
    public class Block
    {
        public string Name { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();
    }
}