using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MizSuite.ConsoleApp
{
    public class LineClass
    {
        public string Content { get; set; }
        public int Index { get; set; }

        public bool HasChanged { get; set; }
        public LineClass(string line, int index)
        {
            Content = line;
            Index = index;
        }

        public static List<LineClass> CreateList(string[] fileLines)
        {
            var result = new List<LineClass>();
            for(int i = 0; i < fileLines.Length; i++)
            {
                result.Add(new LineClass(fileLines[i], i));
            }
            return result;
        }
    }
}



