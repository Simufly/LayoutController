using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutController
{
    public class LayoutInfo
    {
        public string name { get; set; }
        public bool enable { get; set; }
        public int screenCount { get; set; }
        public float totalWidth { get; set; }
        public float totalHeight { get; set; }
        public float startX { get; set; }
        public float startY { get; set; }
        public List<ModuleLayoutInfo> moduleLayoutInfos { get; set; }
    }
}
