using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Svg.Dom;

namespace OpenTKSVG
{
    class SVGParser
    {
        public static List<string> Parse(string svg)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(svg);
            var pathElements = document.QuerySelectorAll<ISvgElement>("path").ToList();

            var pathDataList = new List<string>();
            foreach (var pathElement in pathElements)
            {
                var dAttribute = pathElement.GetAttribute("d");
                if (!string.IsNullOrEmpty(dAttribute))
                {
                    pathDataList.Add(dAttribute);
                }
            }
            return pathDataList;
        }
    }
}
