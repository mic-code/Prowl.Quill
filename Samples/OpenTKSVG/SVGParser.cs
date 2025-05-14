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
using System.Globalization;

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
                    DrawPath(canvas, dAttribute);
                }
            }
        }

        public static void DrawPath(Canvas canvas, string pathData)
        {
            canvas.BeginPath();

            //Console.WriteLine(pathData);

            var commandList = new List<string>();
            if (!string.IsNullOrEmpty(pathData))
            {
                var matches = Regex.Matches(pathData, @"([a-zA-Z])([^a-zA-Z]*)");
                foreach (Match match in matches)
                {
                    commandList.Add(match.Groups[1].Value + match.Groups[2].Value.Trim());
                }
            }

            var lastMoveTo = new List<double>();
            var currentPoint = new List<double>();

            // At this point, commandList contains strings like "M150 5", "L75 200", "L225 200", "Z"
            foreach (var commandSegment in commandList)
            {
                var command = commandSegment[0];
                var parametersString = commandSegment.Length > 1 ? commandSegment.Substring(1).Trim() : "";
                var coordinates = new List<double>();

                if (!string.IsNullOrEmpty(parametersString))
                {
                    var parts = parametersString.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (double.TryParse(part, NumberStyles.Any, CultureInfo.InvariantCulture, out double coord))
                        {
                            coordinates.Add(coord);
                            //Console.WriteLine(coord);
                        }
                        else
                        {
                            // Handle parsing error, e.g., log or throw
                            Console.WriteLine($"Warning: Could not parse coordinate '{part}' in command '{commandSegment}'");
                        }
                    }
                }

                switch (command)
                {
                    case 'm':
                    case 'M':
                        lastMoveTo.Clear();
                        lastMoveTo.AddRange(coordinates);
                        canvas.MoveTo(coordinates[0], coordinates[1]);
                        currentPoint.Clear();
                        currentPoint.AddRange(coordinates);
                        break;
                    case 'l':
                        canvas.LineTo(currentPoint[0] + coordinates[0], currentPoint[1] + coordinates[1]);
                        break;
                    case 'L':
                        canvas.LineTo(coordinates[0], coordinates[1]);
                        break;
                    case 'q':
                        canvas.QuadraticCurveTo(currentPoint[0] + coordinates[0], currentPoint[1] + coordinates[1], currentPoint[0] + coordinates[2], currentPoint[1] + coordinates[3]);
                        break;
                    case 'Q':
                        canvas.QuadraticCurveTo(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
                        break;
                    case 'z':
                    case 'Z':
                        canvas.ClosePath();
                        break;
                }
                //Console.WriteLine($"Command segment: {commandSegment}");
            }
            canvas.Stroke();
        }
    }
}
