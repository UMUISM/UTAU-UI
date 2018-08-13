using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI
{
    struct otodata
    {
        double offset;
        double consonant;
        double cutoff;
        double preutterance;
        double overlap;
    }

    class Oto
    {
        public string getGenFile(string gen)
        {
            return gen + ".wav";
        }
    }
}
