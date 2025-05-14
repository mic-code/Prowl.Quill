using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Svg.Dom;
using Prowl.Quill;

namespace OpenTKSVG
{
    class SVGParser
    {
        public static void ParseAndDraw(Canvas canvas, string svg)
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
                    Draw(canvas, dAttribute);
                }
            }
        }

        public static void Draw(Canvas canvas, string pathData)
        {
            canvas.BeginPath();

            Console.WriteLine(pathData);

            var commandList = new List<string>();
            if (!string.IsNullOrEmpty(pathData))
            {
                var matches = Regex.Matches(pathData, @"([a-zA-Z])([^a-zA-Z]*)");
                foreach (Match match in matches)
                {
                    commandList.Add(match.Groups[1].Value + match.Groups[2].Value.Trim());
                }
            }

            // At this point, commandList contains strings like "M150 5", "L75 200", "L225 200", "Z"
            foreach (var commandSegment in commandList)
            {
                var command = commandSegment[0];

                switch(command)
                {
                    case 'M':
                        //canvas.MoveTo()
                        break;
                    case 'L':
                        //canvas.MoveTo()
                        break;
                    case 'Z':
                        //canvas.MoveTo()
                        break;
                }
                Console.WriteLine($"Command segment: {commandSegment}");
            }
        }
    }
}
