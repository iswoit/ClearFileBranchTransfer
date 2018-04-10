using System;
using System.Collections.Generic;
using System.Text;

namespace ClearFileBranchTransfer
{
    public class AccList
    {
        private string _market;
        private List<string> _list;


        public AccList(string market, List<string> list)
        {
            _market = market;
            _list = list;
        }


        public string Market
        {
            get { return _market; }
        }

        public List<string> List
        {
            get { return _list; }
        }
    }
}
